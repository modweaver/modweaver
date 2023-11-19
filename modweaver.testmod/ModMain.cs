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
        }

        public override void OnGUI(ModsMenuPopup ui) {
            ui.CreateTitle("I'm a cool mod :3");
        }
    }
}