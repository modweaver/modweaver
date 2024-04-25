using System;
using System.IO;

namespace modweaver.core {
    public class Utils {
        public static string version = UpdateHelper.getVersionString(Path.Combine(Paths.modweaverDir, ".modweaver_version_do_not_touch"));
    }

    public static class Paths {
        public static string spiderheckDir { get; internal set; } = ".";
        public static string  modweaverDir { get; internal set; } = "modweaver";
        public static string  libsDir { get; internal set; } = Path.Combine("modweaver", "libs");
        
    }
}