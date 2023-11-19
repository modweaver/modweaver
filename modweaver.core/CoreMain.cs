using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using modweaver.preload;
using Newtonsoft.Json;
using NLog;

namespace modweaver.core {
    public class CoreMain {
        internal static ModManifest loader = new() {
            metadata = new ModManifest.Metadata {
                id = "org.modweaver.loader",
                version = "0.1.0",
                title = "ModWeaver",
                authors = new List<string> { "ModWeaver Team" },
                gameVersion = "1.4"
            },
            dependencies = new List<ModManifest.Dependency>(),
            incompatibilities = new List<string>()
        };

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        internal static List<Mod> mods = new();
        private static Dictionary<string, ModManifest> discoveredMods = new();

        public Mod getModById(string id) {
            return mods.Find(mod => mod.Metadata.id == id);
        }

        public static void discoverMods() {
            discoveredMods.Clear();
            var modsPath = Path.Combine(Paths.modweaverDir, "mods");
            foreach (var dllPath in Directory.EnumerateFiles(modsPath, "*.dll", SearchOption.AllDirectories)) {
                var dllName = Path.GetFileName(dllPath);
                var asm = Assembly.ReflectionOnlyLoadFrom(dllPath);
                
                var resources = asm.GetManifestResourceNames();
                if (!resources.Contains(ModManifest.embeddedResourceName)) {
                    Logger.Debug("Skipping DLL {DllName} as it had no manifest resource", dllName);
                    continue;
                }

                var manifestText = "";
                using (var manifestStream = asm.GetManifestResourceStream(ModManifest.embeddedResourceName))
                using (var reader = new StreamReader(manifestStream)) {
                    manifestText = reader.ReadToEnd();
                }

                var manifest = JsonConvert.DeserializeObject<ModManifest>(manifestText);
                if (manifest == null) {
                    Logger.Warn("Mod {} has an invalid manifest file! Skipping load", dllName);
                    continue;
                }

                if (manifest.metadata == null) {
                    Logger.Warn("Mod {} has no metadata in its manifest! Skipping load", dllName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(manifest.metadata.title) ||
                    string.IsNullOrWhiteSpace(manifest.metadata.id) ||
                    string.IsNullOrWhiteSpace(manifest.metadata.version) ||
                    string.IsNullOrWhiteSpace(manifest.metadata.gameVersion) ||
                    !manifest.metadata.authors.Any()) {
                    Logger.Warn("Mod {} is missing one or more of its metadata fields! Skipping load", dllName);
                }
                
                Logger.Info("Mod {} version {} has been found", 
                    manifest.metadata.title, manifest.metadata.version);
                
                discoveredMods.Add(dllPath, manifest);
            }
            
            //TODO: verify dependencies and incompatibilities
        }
        
        public static void loadMods() {
            foreach (var discovered in discoveredMods) {
                var assembly = Assembly.LoadFrom(discovered.Key);
                var manifest = discovered.Value;

                var types = assembly.GetTypes()
                    .Where(t => t.IsClass)
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.BaseType != null)
                    .Where(t => t.BaseType == typeof(Mod))
                    .Where(t => t.GetCustomAttributes().Any(attr => attr.GetType() == typeof(ModMainClassAttribute)))
                    .ToArray();

                if (types.Length < 1) {
                    Logger.Warn("Mod {} contains no valid main classes!", manifest.metadata.title);
                    continue;
                }
                var mainClassType = types[0];

                if (types.Length > 1) {
                    Logger.Warn("Mod {} contains multiple main classes! Loading only the first class ({}).",
                        manifest.metadata.title, mainClassType.FullName);
                    continue;
                }
                
                Logger.Debug("Creating instance of main class {} for mod {}",
                    mainClassType.Name, manifest.metadata.title);
                
                var instance = (Mod) Activator.CreateInstance(mainClassType);
                instance.Manifest = manifest;
                instance.Assembly = assembly;
                instance.Logger = LogManager.GetLogger(manifest.metadata.id);
                
                instance.Init();
                mods.Add(instance);
            }

            mods.All(mod => {
                mod.Ready();
                return true;
            });
        }

        // this is called by modweaver.preload on unity scene load.
        public static void handoff() {
            ConsoleCreator.Create();
            LogManager.Setup().LoadConfiguration(b => {
                b.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToConsole();
                b.ForLogger().FilterMinLevel(LogLevel.Trace)
                    .WriteToFile(Path.Combine(Paths.modweaverDir, "latest.log"));
            });
            Logger.Info("Recieved handoff");
            Logger.Debug("Patching...");
            Patches.Patch();
            
            Logger.Debug("Discovering mods...");
            discoverMods();
            Logger.Debug("Loading mods...");
            loadMods();
            Logger.Info("All ({ModCount}) mods have been loaded!", mods.Count);
        }
        
        // do mod load before this!
        public static void addModsToMenu() {
            Logger.Debug("Adding mods to game menu");
            foreach (var mod in mods) {
                ModsMenu.instance.CreateButton(mod.Metadata.title, () => {
                    var ui = Announcer.ModsPopup(mod.Metadata.title);
                    ui.CreateParagraph($"ID: {mod.Metadata.id}");
                    ui.CreateParagraph($"Version: {mod.Metadata.version}");
                    ui.CreateParagraph($"Authors: {string.Join(", ", mod.Metadata.authors)}");
                    ui.CreateParagraph($"Designed for SpiderHeck version: {mod.Metadata.gameVersion}");
                    ui.CreateDivider();
                    mod.OnGUI(ui);
                });
            }
        }
    }
}