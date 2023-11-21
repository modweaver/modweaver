using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace modweaver.core {
    [Obsolete(
        "EXPERIMENTAL: This API is subject to change. The Experimental attribute is not available on .NET Framework 4.7.2, so we use Obsolete.")]
    public class ModConfig {
        private readonly string modId;
        private readonly string configDir;

        public ModConfig(string id) {
            modId = id;
            configDir = Path.Combine(Paths.modweaverDir, "config", modId);
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
        }

        public string ModId => modId;
        public string ConfigDir => configDir;

        public T get<T>(string key, T def = default, string fileName = "main") {
            var filePath = Path.Combine(configDir, $"{fileName}.toml");

            if (File.Exists(filePath)) {
                T val;
                try {
                    var table = Toml.ToModel(File.ReadAllText(filePath));
                    var valTry = table[key];
                    val = (T)valTry;
                }
                catch (Exception e) {
                    val = def;
                }

                return val;
            }


            var newTable = new TomlTable {
                [key] = def
            };
            File.WriteAllText(filePath, Toml.FromModel(newTable));
            return def;
        }

        public void set<T>(string key, T value, string fileName = "main") {
            var filePath = Path.Combine(configDir, $"{fileName}.toml");
            var table = File.Exists(filePath) ? Toml.ToModel(File.ReadAllText(filePath)) : new TomlTable();
            table[key] = value;
            File.WriteAllText(filePath, Toml.FromModel(table));
        }
    }
}