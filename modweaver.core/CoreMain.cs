using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using modweaver.preload;
using Newtonsoft.Json;
using NLog;
using Tomlyn;

namespace modweaver.core {
    public class CoreMain {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        internal static List<Mod> mods = new();
        
        private static Dictionary<string, ModManifest> discoveredMods = new();

        public Mod getModById(string id) {
            return mods.Find(mod => mod.Metadata.id == id);
        }

        private static void discoverMods() {
            discoveredMods.Clear();
            var modsPath = Path.Combine(Paths.modweaverDir, "mods");
            if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);
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

                var tmo = new TomlModelOptions();
                tmo.ConvertPropertyName = s => s;
                tmo.ConvertFieldName = s => s;
                ModManifest manifest;
                try {
                    manifest = Toml.ToModel<ModManifest>(manifestText);
                }
                catch (Exception e) {
                    Logger.Warn("Mod {0} has an invalid manifest file! Error: {1}", dllName, e.Message);
                    continue;
                }

                //var manifest = JsonConvert.DeserializeObject<ModManifest>(manifestText);

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

        private static void loadMods() {
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
                    Logger.Warn("Mod {} contains multiple main classes! Not loading this mod.",
                        manifest.metadata.title);
                    continue;
                }
                
                Logger.Debug("Creating instance of main class {} for mod {}",
                    mainClassType.Name, manifest.metadata.title);
                
                var instance = (Mod) Activator.CreateInstance(mainClassType);
                instance.Manifest = manifest;
                instance.Assembly = assembly;
                instance.Logger = LogManager.GetLogger(manifest.metadata.id);
                instance.Config = new ModConfig(manifest.metadata.id);
                
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
                b.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToColoredConsole();
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
            
            var sh = BuiltInMods.spiderheck.metadata;
            var mw = BuiltInMods.modweaver.metadata;
            
            ModsMenu.instance.CreateButton(sh.title, () => {
                var ui = Announcer.ModsPopup(sh.title);
                ui.CreateParagraph($"ID: {sh.id}");
                ui.CreateParagraph($"Version: {sh.version}");
                ui.CreateParagraph($"Authors: {string.Join(", ", sh.authors)}");
                //ui.CreateDivider();
            });
            ModsMenu.instance.CreateButton(mw.title, () => {
                var ui = Announcer.ModsPopup(mw.title);
                ui.CreateParagraph($"ID: {mw.id}");
                ui.CreateParagraph($"Version: {mw.version}");
                ui.CreateParagraph($"Authors: {string.Join(", ", mw.authors)}");
                ui.CreateParagraph($"Designed for SpiderHeck version: {mw.gameVersion}");
                //ui.CreateDivider();
            });
            
            foreach (var mod in mods) {
                ModsMenu.instance.CreateButton(mod.Metadata.title, () => {
                    var ui = Announcer.ModsPopup(mod.Metadata.title);
                    ui.CreateParagraph($"ID: {mod.Metadata.id}");
                    ui.CreateParagraph($"Version: {mod.Metadata.version}");
                    ui.CreateParagraph($"Authors: {string.Join(", ", mod.Metadata.authors)}");
                    ui.CreateParagraph($"Designed for SpiderHeck version: {mod.Metadata.gameVersion}");
                    mod.OnGUI(ui);
                });
            }

            
        }
    }
}