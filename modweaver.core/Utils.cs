using System;
using System.IO;

namespace modweaver.core {
    public class Utils {
        
    }

    public static class Paths {
        public static string spiderheckDir { get; internal set; } = ".";
        public static string  modweaverDir { get; internal set; } = "modweaver";
        public static string  libsDir { get; internal set; } = Path.Combine("modweaver", "libs");
        
    }
}