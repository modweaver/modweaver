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

        // need for doorstop entrypoint
        public static void Start() {
            try {
                InitPreloader();
            }
            catch (Exception e) {
                File.WriteAllText("modweaver/preloader_init_error.txt", e.ToString());
            }
        }

        public static void InitPreloader() {
            ModweaverEnvironment.getVars();
            //ConsoleCreator.Create();

            var gameDirectory = Path.GetDirectoryName(ModweaverEnvironment.doorstopGameExecutable) ?? ".";
            var harmonyFile = Path.Combine(gameDirectory, "modweaver/libs/0Harmony.dll");
            var ass = Assembly.LoadFile(harmonyFile);
            //File.WriteAllText("modweaver/latest.log", $"[modweaver] Assembly@ : {ass.FullName}");
            HarmonyFileLog.Enabled = true;
            AppDomain.CurrentDomain.AssemblyResolve += resolveCurrentDirectory;
            // https://harmony.pardeike.net/articles/patching-edgecases.html#patching-too-early-missingmethodexception-in-unity
            SceneManager.sceneLoaded += sceneLoadHandler;
        }

        // handoff to core
        private static void sceneLoadHandler(Scene s, LoadSceneMode l) {
            if (hasLoadedCore) return;
            Console.WriteLine("[modweaver.preload] Handing off to Core...");
            CoreMain.handoff();
            hasLoadedCore = true;
        }

        internal static Assembly resolveCurrentDirectory(object _, ResolveEventArgs args) {
            // Can't use Utils here because it's not yet resolved
            var name = new AssemblyName(args.Name);

            try {
                return Assembly.LoadFile(Path.Combine(preloaderPath, $"{name.Name}.dll"));
            }
            catch (Exception) {
                return null;
            }
        }
    }
}