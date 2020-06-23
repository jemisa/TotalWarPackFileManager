using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace MMS {
    class ExternalProcesses {
        static char[] PATH_SEPARATOR = { Path.PathSeparator };
        static string GetExecutable(string executableItem) {
            if (!File.Exists(LAUNCH_CONFIG_FILE)) {
                return null;
            }
            string executable = null;
            foreach (string line in File.ReadAllLines(LAUNCH_CONFIG_FILE)) {
                string[] split = line.Split(PATH_SEPARATOR);
                if (split[0].Equals(executableItem)) {
                    executable = split[1];
                    break;
                }
            }
            return executable;
        }

        static readonly string LAUNCH_CONFIG_FILE = "executables.txt";
        public static Process Launch(string executableItem, ProcessStartInfo info) {
            string executable = GetExecutable(executableItem);
            executable = executable.Replace("%modtools%", 
                Path.Combine(ModTools.Instance.InstallDirectory, "binaries"));
            if (executable == null) {
                return null;
            }
#if DEBUG
            Console.WriteLine("Launching {0}", executable);
#endif
            info.FileName = executable;
            info.UseShellExecute = false;
            Process process = Process.Start(info);
            process.WaitForExit();
            return process;
        }
    }
}
