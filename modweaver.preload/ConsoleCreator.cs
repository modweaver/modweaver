using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace modweaver.preload {
    internal static class ConsoleCreator {
        private static bool initialised;
        private static IntPtr stdoutHandle;
        private static Stream stdoutStream;
        private static StreamWriter writer;
        
        public static void Create() {
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
            writer.WriteLine("Hello, console!");
            Console.SetOut(writer);
        }

        public static void Destroy() {
            if(!initialised) return;
            initialised = false;
            
            stdoutStream.Close();
            writer.Close();
            Win32.FreeConsole();
        }
    }
}