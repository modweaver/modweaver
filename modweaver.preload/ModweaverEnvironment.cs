using System;
using System.IO;

namespace modweaver.preload {
    public static class ModweaverEnvironment {
        public static string doorstopTargetAssembly { get; private set; }

        public static string doorstopManagedAssemblies { get; private set; }

        public static string doorstopGameExecutable { get; private set; }

        public static string[] doorstopDllSearchDirs { get; private set; }

        internal static void getVars() {
            doorstopTargetAssembly = Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH");
            doorstopManagedAssemblies = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
            doorstopGameExecutable = Environment.GetEnvironmentVariable("DOORSTOP_PROCESS_PATH");
            var searchDirs = Environment.GetEnvironmentVariable("DOORSTOP_DLL_SEARCH_DIRS");
            string[] array;
            if (searchDirs == null)
                array = null;
            else
                array = searchDirs.Split(Path.PathSeparator);
            if (array == null)
                array = Array.Empty<string>();
            doorstopDllSearchDirs = array;

            // Paths.spiderheckDir = Path.GetDirectoryName(doorstopGameExecutable);
            // Paths.modweaverDir = Path.Combine(Paths.spiderheckDir, "modweaver");
            // Paths.libsDir = Path.Combine(Paths.modweaverDir, "libs");
        }
    }
}