using System;
using System.IO;
using System.Reflection;
using HarmonyLib.Tools;
using modweaver.core;
using modweaver.preload;
using UnityEngine.SceneManagement;

// needed for doorstop entrypoint
// ReSharper disable once CheckNamespace

namespace Doorstop {
    public class Entrypoint {
        private static bool hasLoadedCore;
        private static string preloaderPath;
        private static string modLibPath = "modloader/modLibs";

        // need for doorstop entrypoint
        public static void Start() {
            try {
                Preloader.initPreloader(preloaderPath);
            }
            catch (Exception e) {
                File.WriteAllText("modweaver/preloader_init_error.txt", e.ToString());
            }
        }

        
    }
}