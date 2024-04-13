using System.IO;
using JetBrains.Annotations;
using modweaver.core;
using NLog;
using Tomlyn;
using Tomlyn.Helpers;

namespace modweaver.core {
    internal static class ConfigHandler {
        [CanBeNull] private static ModweaverConfig config;
        
        private static TomlModelOptions options {
            get {
                var t = new TomlModelOptions(); t.ConvertPropertyName = s => s; t.ConvertFieldName = s => s; return t;
            }
            set { }
        }

        internal static ModweaverConfig getConfig() {
            if (config == null) setupConfig();
            return config;
        }
        
        internal static void setupConfig() {
            var configFile = Path.Combine(Paths.modweaverDir, "config", "modweaver.toml");
            if (!File.Exists(configFile)) {
                if (!Directory.Exists(Path.Combine(Paths.modweaverDir, "config"))) {
                    Directory.CreateDirectory(Path.Combine(Paths.modweaverDir, "config"));
                }
                config = new ModweaverConfig();
                File.WriteAllText(configFile, Toml.FromModel(config, options));
            } else {
                var contents = File.ReadAllText(configFile);
                try {
                    config = Toml.ToModel<ModweaverConfig>(contents);
                }
                catch (TomlException) {
                    CoreMain.Logger.Warn("Found issues with modweaver.toml, overwriting with default");
                    config = new ModweaverConfig();
                    File.WriteAllText(configFile, Toml.FromModel(config, options));
                }
            }
            
        }
    }
    
    internal class ModweaverConfig {
        public string modsDirectory { get; set; }
        public bool playRoulette { get; set; }
        public bool showConsole { get; set; }
        
        public ModweaverConfig() {
            modsDirectory = "mods";
            playRoulette = false;
            showConsole = true;
        }
        
        
    }
}