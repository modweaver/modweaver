using System;
using System.IO;
using System.Reflection;
using HarmonyLib.Tools;
using UnityEngine.SceneManagement;

namespace modweaver.preload {
    
    public class Preloader {
        private static bool hasLoadedCore;
        private static string preloaderPath;
        private static string modLibPath = "modloader/modLibs";
        
        public static void initPreloader(string plpath, bool fromUpdater = false) {
            ModweaverEnvironment.getVars();
            //ConsoleCreator.Create();
            preloaderPath = plpath;
            var gameDirectory = Path.GetDirectoryName(ModweaverEnvironment.doorstopGameExecutable) ?? ".";
            
            var updaterTemp = Path.Combine(gameDirectory, "modweaver", "temp-updater");
            
            // time for updater shennanigans
            if (!fromUpdater) {
                try {
                    var zipPath = Path.Combine(updaterTemp, "modweaver.zip");
                    if (File.Exists(zipPath)) {
                        // extract the zip
                        var zipExtractionSite = Path.Combine(updaterTemp, "modweaver");
                        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, zipExtractionSite);
                        // copy the files over, retaining the file tree and all directories
                        foreach (var file in Directory.GetFiles(zipExtractionSite, "*", SearchOption.AllDirectories)) {
                            var relativePath = Util.getRelativePath(zipExtractionSite, file);
                            if (relativePath.EndsWith("winhttp.dll")) {
                                // we can't overwrite a loaded DLL
                                continue;
                            }
                            /*if (relativePath.EndsWith("modweaver.preload.dll")) {
                                relativePath = relativePath.Replace("modweaver.preload.dll", "modweaver.preload_new.dll");
                            }*/
                            //!! FIXME ENABLE THE ABOVE IF STUFF BREAKS
                            var destPath = Path.Combine(gameDirectory, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath) ?? throw new InvalidOperationException());
                            File.Copy(file, destPath, true);
                        }

                        Directory.Delete(updaterTemp, true);
                    }
                }
                catch (Exception e) {
                    File.WriteAllText("modweaver/preloader_updater_error.txt", e.ToString());
                }
            }

            var harmonyFile = Path.Combine(gameDirectory, "modweaver/libs/0Harmony.dll");
            //var ass = Assembly.LoadFile(harmonyFile);
            //File.WriteAllText("modweaver/latest.log", $"[modweaver] Assembly@ : {ass.FullName}");
            //HarmonyFileLog.Enabled = true;
            modLibPath = Path.Combine(gameDirectory, "modweaver/modLibs");
            if (!Directory.Exists(modLibPath)) {
                Directory.CreateDirectory(modLibPath);
            }
            AppDomain.CurrentDomain.AssemblyResolve += customResolver;
            // https://harmony.pardeike.net/articles/patching-edgecases.html#patching-too-early-missingmethodexception-in-unity
            SceneManager.sceneLoaded += sceneLoadHandler;
        }

        // handoff to core
        private static void sceneLoadHandler(Scene s, LoadSceneMode l) {
            if (hasLoadedCore) return;
            hasLoadedCore = true;
            Console.WriteLine("[modweaver.preload] Handing off to Core...");

            var corePath = Path.Combine(Path.GetDirectoryName(ModweaverEnvironment.doorstopGameExecutable) ?? ".", "modweaver", "libs", "modweaver.core.dll");
            var coreAsm = Assembly.LoadFile(corePath);
            var type = coreAsm.GetType("modweaver.core.CoreMain") ?? throw new Exception("Couldn't load CoreMain type");
            var method = type.GetMethod("handoff", BindingFlags.Static | BindingFlags.Public) ?? throw new Exception("Couldn't load handoff method");
            method.Invoke(null, Array.Empty<object>());
        }

        internal static Assembly customResolver(object _, ResolveEventArgs args) {
            // Can't use Utils here because it's not yet resolved
            var name = new AssemblyName(args.Name);

            try {
                return Assembly.LoadFile(Path.Combine(preloaderPath, $"{name.Name}.dll"));
            }
            catch (Exception) {
                try {
                    return Assembly.LoadFile(Path.Combine(modLibPath, $"{name.Name}.dll"));
                }
                catch (Exception) {
                    return null;
                }
            }
        }
    }
}