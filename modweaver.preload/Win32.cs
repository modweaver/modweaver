using System;
using System.Runtime.InteropServices;

namespace modweaver.preload {
    internal static class Win32 {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern bool FreeConsole();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(string fileName,
            uint desiredAccess,
            int shareMode,
            IntPtr securityAttributes,
            int creationDisposition,
            int flagsAndAttributes,
            IntPtr templateFile);
    }
}