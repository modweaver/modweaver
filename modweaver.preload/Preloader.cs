using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using HarmonyLib.Tools;
using modweaver.core;
using modweaver.preload;
using RestSharp;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace modweaver.preload {
    internal class CommitsResponse {
        internal string sha;
    }
    public class Preloader {
        private static bool hasLoadedCore;
        private static string preloaderPath;
        private static string modLibPath = "modloader/modLibs";

        public static bool needsUpdate(string versionFile) {
            var current = File.ReadAllText(versionFile);
            var currentRef = current.Split(':')[0];
            var currentHash = current.Split(':')[1];
            // version file is in the format of "branch:hash"
            // if the hash is different from the latest
            // api.github.com/repos/modweaver/modweaver/commits/{currentRef}
            // then return true
            
            // set security policies
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
            // make a request to the github api to get the latest commit hash#
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "modweaver");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            System.Environment.SetEnvironmentVariable("MONO_TLS_PROVIDER", "legacy");
            var req = UnityWebRequest.Get("http://api.github.com/repos/modweaver/modweaver/commits/" + currentRef);
            var req2 = req.SendWebRequest();
            var awa = false;
            req2.completed += (op) => {
                var resp = req.downloadHandler.text;
                //var resp = http.GetAsync($"http://api.github.com/repos/modweaver/modweaver/commits/{currentRef}").Result;
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<CommitsResponse>(resp);
                /*var options = new RestClientOptions("https://api.github.com");
                var client = new RestClient(options);
                var response = client.GetJson<CommitsResponse>($"repos/modweaver/modweaver/commits/{currentRef}");*/
                if (response.sha != currentHash) {
                    Console.WriteLine("[modweaver.preload] Needs update");
                    Console.WriteLine("[modweaver.preload] Current: " + currentHash);
                    Console.WriteLine("[modweaver.preload] Latest: " + response.sha);
                    awa = true;
                }
                Console.WriteLine("[modweaver.preload] No update needed");
            };
            while (!req2.isDone) { }
            return awa;
        }
        
        public static void initPreloader(string plpath, bool fromUpdater = false) {
            ModweaverEnvironment.getVars();
            //ConsoleCreator.Create();
            preloaderPath = plpath;
            var gameDirectory = Path.GetDirectoryName(ModweaverEnvironment.doorstopGameExecutable) ?? ".";
            
            var updaterTemp = Path.Combine(gameDirectory, "modweaver/temp-updater");
            
            // time for updater shennanigans
            if (!fromUpdater) {
                try {
                    ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
                    var needUp = needsUpdate(Path.Combine(gameDirectory, "modweaver/.modweaver_version_do_not_touch"));
                    if (needUp) {
                        var wc = new WebClient();
                        if (Directory.Exists(updaterTemp)) {
                            Directory.Delete(updaterTemp, true);
                            Directory.CreateDirectory(updaterTemp);
                        }
                        var zipPath = Path.Combine(updaterTemp, "modweaver.zip");
                        wc.DownloadFile("http://nightly.link/modweaver/modweaver/workflows/build/main/Modweaver%20ZIP.zip", zipPath);
                        // extract the zip
                        var zipExtractionSite = Path.Combine(updaterTemp, "modweaver");
                        System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, Path.Combine(gameDirectory, "modweaver"));
                        // copy the files over, retaining the file tree and all directories
                        foreach (var file in Directory.GetFiles(zipExtractionSite, "*", SearchOption.AllDirectories)) {
                            var relativePath = Util.getRelativePath(zipExtractionSite, file);
                            /*if (relativePath.EndsWith("modweaver.preload.dll")) {
                                relativePath = relativePath.Replace("modweaver.preload.dll", "modweaver.preload_new.dll");
                            }*/
                            //!! FIXME ENABLE THE ABOVE IF STUFF BREAKS
                            var destPath = Path.Combine(gameDirectory, "modweaver", relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath) ?? throw new InvalidOperationException());
                            File.Copy(file, destPath, true);
                        }



                    }
                }
                catch (Exception e) {
                    File.WriteAllText("modweaver/preloader_updater_error.txt", e.ToString());
                }
            }

            var harmonyFile = Path.Combine(gameDirectory, "modweaver/libs/0Harmony.dll");
            var ass = Assembly.LoadFile(harmonyFile);
            //File.WriteAllText("modweaver/latest.log", $"[modweaver] Assembly@ : {ass.FullName}");
            HarmonyFileLog.Enabled = true;
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
            Console.WriteLine("[modweaver.preload] Handing off to Core...");
            CoreMain.handoff();
            hasLoadedCore = true;
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