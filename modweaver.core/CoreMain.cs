using System;
using System.Collections.Generic;
using modweaver.preload;

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

        internal static List<ModManifest> mods = new() { loader };

        public ModManifest getModById(string id) {
            return mods.Find(manifest => manifest.metadata.id == id);
        }

        public void discoverMods() {
            
        }
        
        public void loadMods() {
            
        }

        // this is called by modweaver.preload on unity scene load.
        public static void handoff() {
            Console.WriteLine("[modweaver.core] Received handoff");
            Console.WriteLine("[modweaver.core] Patching...");
            Patches.Patch();
        }
        
        // do mod load before this!
        public static void addModsToMenu() {
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