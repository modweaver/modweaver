using System;
using System.Text;
using HarmonyLib;
using HarmonyLib.Tools;
using modweaver.core;
using NLog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace modweaver.preload {
    public static class Patches {
        static Harmony harmony = new Harmony("org.modweaver.loader");
        public static void Patch() {
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(VersionNumberTextMesh), "Start")]
    internal class VersionTextPatch {
        internal static bool hasDoneTextPatch = false;
        internal static VersionNumberTextMesh instance;

        [HarmonyPostfix]
        public static void Postfix(ref VersionNumberTextMesh __instance) {
            if (!hasDoneTextPatch) {
                /*var textMesh = textMeshInfo.GetValue(__instance);

                //Logger.Log(LogLevel.Warning, "TextMesh via reflection: " + textMesh.ToString());

                var setTextInfo = textMesh.GetType().GetMethods().Where(m => m.Name == "SetText").Where(m => m.GetParameters().Length == 1).First();

                var currentText = (string)textMesh.GetType().GetProperty("text").GetValue(textWMesh, null);
                StringBuilder sb = new StringBuilder(currentText);
                sb.Append("\n\nMods:");asa

                foreach (var plugin in Util.Plugins)
                {
                    var name = plugin.Value.Metadata.Name;
                    var version = plugin.Value.Metadata.Version;

                    sb.Append("\n- ");
                    sb.Append(name);
                    sb.Append(" v");
                    sb.Append(version);
                }

                setTextInfo.Invoke(textMesh, new object[] { sb });

                textMeshInfo.SetValue(__instance, textMesh);

                hasDoneTextPatch = true;*/
                CoreMain.Logger.Debug("Patching VersionNumberTextMesh");
                //__instance.textMesh.text = "SPIDERHECK; ModWeaver ALPHA 0.1.0";
                var newText = __instance.textMesh.text;
                newText += "\n\nModweaver loaded\nMade with love by the modweaver team <3333";
                __instance.textMesh.SetText(newText);
                hasDoneTextPatch = true;
            }
        }
    }

    [HarmonyPatch(typeof(CustomTiersScreen), "Start")]
    public static class AddModMenu {
        [HarmonyPostfix]
        public static void Postfix() {
            HudController.instance.EnableModsButton(); // we do this here to ensure everything has loaded
            CoreMain.addModsToMenu();
        }
    }
    
    [HarmonyPatch(typeof(SteamLeaderboards), "UpdateScore")]
    internal class DisableLeaderboard
    {
        public static bool Prefix(int score)
        {
            CoreMain.Logger.Debug("Skipping leaderboard save of {}", score);
            return false;
        }
    }
    
    [HarmonyPatch("HudController", "ShowQuickGamePrompt")]
    internal class StopQuickGame
    {
        private static bool done = false;
        
        public static bool Prefix(ref object __instance)
        {
            CoreMain.Logger.Debug("Attempted to show QuickPlay prompt. BLOCKED!");
            if(!done) RuntimeApis.Announce("QuickPlay has been disabled in modded play", 255, 255, 255, __instance.GetType());
            done = !done;
            return false;
        }
    }
    
    public class RuntimeApis
    {
        public static void Announce(string text, int colorR, int colorG, int colorB, Type gameTypeRef) {
            var color = new Color(colorR, colorG, colorB);
            Announcer.instance.Announce(text, color, true);
        }
    }
}