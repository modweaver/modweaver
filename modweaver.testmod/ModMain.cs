using System;
using System.IO;
using HarmonyLib;
using modweaver.core;

namespace modweaver.testmod {
    [ModMainClass]
    public class ModMain : Mod {
        public override void Init() {
            Logger.Info("Test mod init method is called.");
            Harmony harmony = new Harmony(Metadata.id); 
            Logger.Info("Running patches!");
            harmony.PatchAll();
            Logger.Info("Patched!");
        }

        public override void Ready() {
            Logger.Info("Test mod is ready!");
            
            Config.set("test1", 42);
            Config.set("test2", true, "otherfile");

            var test1 = Config.get("test1", -1);
            var test2 = Config.get("test2", false, "otherfile");
            
            Logger.Debug("Config test values: {V1}, {V2}", test1, test2);
        }

        public override void OnGUI(ModsMenuPopup ui) {
            ui.CreateTitle("I'm a cool mod :3");
        }
    }

    [HarmonyPatch(typeof(VersionNumberTextMesh), "Start")]
    public static class VNTMPatch {
        
        [HarmonyPostfix]
        public static void Postfix(ref VersionNumberTextMesh __instance) {
            __instance.textMesh.text += "\nhello world :3";
        }
    }
}