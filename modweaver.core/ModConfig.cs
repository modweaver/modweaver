using System.IO;
using modweaver.core;
using Newtonsoft.Json.Linq;

namespace modweaver {
    public class ModConfig {
        private readonly string modId;
        private readonly string configDir;
        
        internal ModConfig(string id) {
            modId = id;
            configDir = Path.Combine(Paths.modweaverDir, "config", modId);
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
        }

        public T get<T>(string key, T def = default, string fileName = "main") {
            var filePath = Path.Combine(configDir, fileName + ".json");
            
            if (!File.Exists(filePath)) {
                var obj = new JObject {
                    [key] = JToken.FromObject(def)
                };
                File.WriteAllText(filePath, obj.ToString());
                return def;
            }
            else {
                var obj = JObject.Parse(File.ReadAllText(filePath));
                var val = obj[key];
                if (val == null) {
                    return def;
                }
                return val.Value<T>() ?? def;
            }
        }

        public void set<T>(string key, T value, string fileName = "main") {
            var filePath = Path.Combine(configDir, fileName + ".json");
            JObject obj;
            obj = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();
            obj[key] = JToken.FromObject(value);
            File.WriteAllText(filePath, obj.ToString());
        }
    }
}