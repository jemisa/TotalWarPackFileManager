using System;
using System.IO;

namespace Common
{
    public class DirectoryHelper
    {
        public static string UserDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); } }
        public static string FpmDirectory { get { return UserDirectory + "\\FPM"; } }
        public static string SchemaDirectory { get { return FpmDirectory + "\\Schemas"; } }
        public static string LogDirectory { get { return FpmDirectory + "\\Logs"; } }

        public static void EnsureCreated()
        {
            EnsureCreated(FpmDirectory);
            EnsureCreated(SchemaDirectory);
            EnsureCreated(LogDirectory);
        }

        static void EnsureCreated(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
