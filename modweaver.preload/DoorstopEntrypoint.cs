// needed for doorstop entrypoint
// ReSharper disable once CheckNamespace

using System;
using System.Reflection;
using System.IO;
using HarmonyLib;
using HarmonyLib.Tools;
using modweaver.preload;
using UnityEngine;
using Patches = modweaver.preload.Patches;

namespace Doorstop {
    public class Entrypoint {
        private static readonly string[] criticalAssemblies = {
            "Mono.Cecil.dll",
            "Mono.Cecil.Mdb.dll",
            "Mono.Cecil.Pdb.dll",
            "Mono.Cecil.Rocks.dll"
        };

        private static void loadCriticalAssemblies() {
            foreach (var criticalAssembly in criticalAssemblies)
                try {
                    Assembly.LoadFile(Path.Combine(
                        Path.Combine(ModweaverEnvironment.doorstopGameExecutable, Path.Combine("modweaver", "libs")),
                        criticalAssembly));
                }
                catch (Exception) {
                    // Suppress error for now
                    // TODO: Should we crash here if load fails? Can't use logging at this point
                }
        }

        private static string preloaderPath;

        // need for doorstop entrypoint
        public static void Start() {
            try {
                InitPreloader();
            }
            catch (Exception e) {
                File.WriteAllText("modweaver/initex.txt", e.ToString());
            }
        }
        
        public static void InitPreloader() {
            ModweaverEnvironment.getVars();
            ConsoleCreator.Create();
            
            var gameDirectory = Path.GetDirectoryName(ModweaverEnvironment.doorstopGameExecutable) ?? ".";
            var harmonyFile = Path.Combine(gameDirectory, "modweaver/libs/0Harmony.dll");
            var ass = Assembly.LoadFile(harmonyFile);
            File.WriteAllText("modweaver/latest.log", $"[modweaver] Assembly@ : {ass.FullName}");
            HarmonyFileLog.Enabled = true;
            //File.WriteAllText("modweaver/latest.log", "Hello from Unity!");
            Patches.Patch();
            //Debug.Log("[modweaver] HELLO WORLD");
            //File.WriteAllText("/home/ecorous/modweaver_preloader.txt", $"wawa!! {DateTime.Now:yyyy-M-d_HH:mm:ss_fff}");
            var silentExceptionLog = $"modweaver_preload_{DateTime.Now:yyyy-MM-dd_HH:mm:ss_fff}.log";
            Console.WriteLine("[modweaver.preloader] Loading1!");
            Console.WriteLine("[modweaver.preloader] Loading2!");
            
            silentExceptionLog = Path.Combine(gameDirectory, silentExceptionLog);

            

            AppDomain.CurrentDomain.AssemblyResolve += resolveCurrentDirectory;

            //typeof(Entrypoint).Assembly.GetType("");
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