using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using modweaver.preload;
using Newtonsoft.Json;
using NLog;
using Tomlyn;
using Unity.Collections.LowLevel.Unsafe;

namespace modweaver.core {
    public class CoreMain {
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        internal static List<Mod> mods = new();
        
        private static Dictionary<string, ModManifest> discoveredMods = new();

        public Mod getModById(string id) {
            return mods.Find(mod => mod.Metadata.id == id);
        }

        private static void discoverMods() {
            discoveredMods.Clear();
            var modsPath = Path.Combine(Paths.modweaverDir, ConfigHandler.getConfig().modsDirectory);
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
                
                Logger.Debug("Trying to do silly dll loading stuff");
                var dllLoader = "load_me_pls";
                foreach (var resource in resources) {
                    if (resource.StartsWith(dllLoader) && resource.EndsWith(".dll")) {
                        Logger.Debug("Found loadable DLL {Resource}", resource);
                        var loaderStream = asm.GetManifestResourceStream(resource);
                            // get bytes from stream
                        var bytes = ReadFully(loaderStream);
                        var name = "";
                        var ass = Assembly.Load(bytes);
                        name = ass.GetName().Name + ".dll";
                        var modLibPath = Path.Combine(Paths.modweaverDir, "modLibs");
                        var loaderPath = Path.Combine(modLibPath, name);
                        // write to loaderPath
                        File.WriteAllBytes(loaderPath, bytes);
                    } else {
                        Logger.Debug("Skipping resource for dll loading {Resource}", resource);
                    }
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
            
            //TODO: verify dependencies
            foreach (var kv in discoveredMods) {
                var path = kv.Key;
                var manifest = kv.Value;
                var incompats = manifest.incompatibilities;
                var toRemove = new List<string>();
                foreach (var incompat in incompats) {
                    if (discoveredMods.Values.Any(m => m.metadata.id == incompat)) {
                        Logger.Warn("Mod {} is incompatible with mod {}! Not loading either mod.",
                            manifest.metadata.id, incompat);
                        toRemove.Add(path);
                        var wawa = discoveredMods.ToList().Find(predicate => predicate.Value.metadata.id == incompat);
                        toRemove.Add(wawa.Key);
                        break;
                    }
                }
                
                foreach (var mod in toRemove) {
                    if (discoveredMods.ContainsKey(mod)) discoveredMods.Remove(mod);
                }
            }
        }
        
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
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
                    if (ConfigHandler.getConfig().playRoulette) {
                        Logger.Warn("Mod {} contains multiple main classes! Time to play rulette!",
                            manifest.metadata.title);
                        var rand = new Random();
                        mainClassType = types[rand.Next(0, types.Length)];
                    }
                    else {
                        Logger.Warn("Mod {} contains multiple main classes! Not loading this mod.",
                            manifest.metadata.title);
                        continue;
                    }
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
            try {
                Logger.Info("Recieved handoff");
                Logger.Debug("Loading config...");
                ConfigHandler.setupConfig();
                Logger.Debug("Patching...");
                Patches.Patch();

                Logger.Debug("Discovering mods...");
                discoverMods();
                Logger.Debug("Loading mods...");
                loadMods();
                Logger.Info("All ({ModCount}) mods have been loaded!", mods.Count);
            }
            catch (Exception e) {
                Logger.Error(e, "An error occured while loading ModWeaver. Please check the logs for more information.");
            }
        }
        
        // do mod load before this!
        public static void addModsToMenu() {
            Logger.Debug("Adding mods to game menu");
            
            var sh = BuiltInMods.spiderheck.metadata;
            var mw = BuiltInMods.modweaver.metadata;
            
            ModsMenu.instance.CreateButton(sh.title, () => {
                var ui = Announcer.ModsPopup(sh.title);
                ui.CreateDivider();
                ui.CreateParagraph($"Authors: {string.Join(", ", sh.authors)}");
                ui.CreateParagraph($"Version: {sh.version}");
                ui.CreateParagraph($"ID: {sh.id}");
            });
            ModsMenu.instance.CreateButton(mw.title, () => {
                var ui = Announcer.ModsPopup(mw.title);
                ui.CreateDivider();
                ui.CreateParagraph($"Authors: {string.Join(", ", mw.authors)}");
                ui.CreateParagraph($"Version: {mw.version}");
                ui.CreateParagraph($"Designed for SpiderHeck version: {mw.gameVersion}");
                ui.CreateParagraph($"ID: {mw.id}");
            });
            
            foreach (var mod in mods) {
                ModsMenu.instance.CreateButton(mod.Metadata.title, () => {
                    var ui = Announcer.ModsPopup(mod.Metadata.title);
                    ui.CreateDivider();
                    if (!string.IsNullOrWhiteSpace(mod.Metadata.description)) {
                        ui.CreateParagraph($"{mod.Metadata.description}");
                        ui.CreateParagraph(" ");
                    }
                    ui.CreateParagraph($"Authors: {string.Join(", ", mod.Metadata.authors)}");
                    ui.CreateParagraph($"Version: {mod.Metadata.version}");
                    ui.CreateParagraph($"Designed for SpiderHeck version: {mod.Metadata.gameVersion}");
                    ui.CreateParagraph($"ID: {mod.Metadata.id}");
                    mod.OnGUI(ui);
                });
            }
        }
    }
}