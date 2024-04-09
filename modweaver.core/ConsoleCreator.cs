using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace modweaver.core {
    internal static class ConsoleCreator {
        private static bool initialised;
        private static IntPtr stdoutHandle;
        private static Stream stdoutStream;
        private static StreamWriter writer;
        
        public static void Create() {
            if (!ConfigHandler.getConfig().showConsole) return;
            if(initialised) return;
            initialised = true;

            if (!Win32.AllocConsole()) {
                File.WriteAllText("modweaver/consolefail.txt", "AllocConsole native call failed");
            }
            
            stdoutHandle = Win32.CreateFile("CONOUT$", 
                0x80000000 | 0x40000000, 
                2, // FILE_SHARE_WRITE
                IntPtr.Zero, 
                3, // OPEN_EXISTING
                0, IntPtr.Zero);
            
            if(stdoutHandle == IntPtr.Zero) {
                File.WriteAllText("modweaver/consolefail.txt", "CreateFile native call failed");
            }
            var sfh = new SafeFileHandle(stdoutHandle, true);
            stdoutStream = new FileStream(sfh, FileAccess.Write);
            writer = new StreamWriter(stdoutStream);
            writer.AutoFlush = true;
            writer.WriteLine("Console allocated");
            Console.SetOut(writer);
            Console.WriteLine("Configured stdout");
        }

        public static void Destroy() {
            if(!initialised) return;
            initialised = false;
            
            stdoutStream.Close();
            writer.Close();
            Win32.FreeConsole();
        }
    }
    
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