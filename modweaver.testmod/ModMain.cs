using System;
using System.IO;

namespace modweaver.testmod {
    [ModMainClass]
    public class ModMain : Mod {
        public override void Init() {
            Logger.Info("Test mod init method is called.");
        }

        public override void Ready() {
            Logger.Info("Test mod is ready!");
            
            Config.set("test1", 42);
            Config.set("test2", true, "otherfile");

            var test1 = Config.get("test1", -1);
            var test2 = Config.get("test2", false, "otherfile");
            
            Logger.Debug("Config test values: {V1}, {V2}", test1, test2);
        }

        public override void OnGUI(ModsMenuPopup ui) {
            ui.CreateTitle("I'm a cool mod :3");
        }
    }
}