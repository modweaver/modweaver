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
        
        internal static List<ModManifest> mods = new() { loader };

        public ModManifest getModById(string id) {
            return mods.Find(manifest => manifest.metadata.id == id);
        }

        public static void discoverMods() {
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
                
                Logger.Info("Mod {} version {} has been found", 
                    manifest.metadata.title, manifest.metadata.version);
            }
        }
        
        public static void loadMods() {
            
        }

        // this is called by modweaver.preload on unity scene load.
        public static void handoff() {
            ConsoleCreator.Create();
            Console.WriteLine("hlep");
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
        }
        
        // do mod load before this!
        public static void addModsToMenu() {
            Logger.Debug("Adding mods to game menu");
            foreach (var mod in mods) {
                ModsMenu.instance.CreateButton(mod.metadata.title, () => {
                    var page = Announcer.ModsPopup(mod.metadata.title);
                    var mod2 = mod;
                    mod2.modsMenuPopup = page;
                    mods.Remove(mod);
                    mods.Add(mod2);
                    page.CreateParagraph($"ID: {mod.metadata.id}");
                    page.CreateParagraph($"Version: {mod.metadata.version}");
                    page.CreateParagraph($"Authors: {string.Join(", ", mod.metadata.authors)}");
                    page.CreateParagraph($"Designed for SpiderHeck version: {mod.metadata.gameVersion}");
                });
            }
        }
    }
}