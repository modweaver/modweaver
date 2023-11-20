using System.Linq;
using System.Reflection;
using modweaver.core;
using NLog;

namespace modweaver {
    public abstract class Mod {
        public static TMod get<TMod>() where TMod : Mod {
            var asm = typeof(TMod).Assembly;
            var mod = CoreMain.mods.First(m => m.Assembly == asm);
            return (TMod)mod;
        }

        protected Mod() {
            
        }

        // Gets called when the mod is first created
        // Not all mods may be ready at this time (libraries may not be fully ready etc)
        public abstract void Init();

        // Gets called after all mods have been loaded and Init()'d
        public abstract void Ready();
        
        // Gets called whenever the mod's menu page is opened
        public abstract void OnGUI(ModsMenuPopup ui);
        
        public Assembly Assembly { get; internal set; }
        public ModManifest Manifest { get; internal set; }
        public ModManifest.Metadata Metadata => Manifest.metadata;
        public Logger Logger { get; internal set; }
        public ModConfig Config { get; set; }
    }
}