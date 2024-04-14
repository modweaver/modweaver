using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using UnityEngine.Networking;

namespace modweaver.core {
    public class UpdateHelper {
        public static string refName = "main";
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public static void checkForNewVersion(string versionFile, Action doUpdate) {
            var current = File.ReadAllText(versionFile).TrimEnd('\n');
            var currentRef = current.Split(':')[0];
            refName = currentRef;
            var currentHash = current.Split(':')[1];
            // version file is in the format of "branch:hash"
            // if the hash is different from the latest
            // api.github.com/repos/modweaver/modweaver/commits/{currentRef}
            // then return true
            
            var req = UnityWebRequest.Get("https://api.github.com/repos/modweaver/modweaver/commits/" + currentRef);
            var operation = req.SendWebRequest();
            
            operation.completed += op => {
                var resp = req.downloadHandler.text;
                
                // newtonsoft json doesn't like partial deserialization
                var json = JObject.Parse(resp);
                var latestCommit = json.GetValue("sha")?.ToString();
                
                if (latestCommit != currentHash) {
                    logger.Info("Needs update!"); 
                    logger.Debug("Current: {Commit}",currentHash);
                    logger.Debug("Latest: {Commit}", latestCommit);
                    doUpdate();
                }
                else {
                    logger.Info("No update needed");
                }
            };
        }
        
        public static void downloadUpdateZip(string downloadToPath, Action onDownloadFinished) {
            logger.Debug("Starting download...");
            string url = $"https://nightly.link/modweaver/modweaver/workflows/build/{refName}/Modweaver%20ZIP.zip";
            var req = UnityWebRequest.Get(url);
            var operation = req.SendWebRequest();
            operation.completed += op => {
                logger.Debug("Finished downloading zip - {Size} bytes", req.downloadHandler.data.Length);
                File.WriteAllBytes(downloadToPath, req.downloadHandler.data);
                logger.Debug("Written update to file {Path}", downloadToPath);
                onDownloadFinished();
            };
        }
    }
}