using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PackFileManager
{
    class PackFileManagerSettings
    {
        public class GamePathPair
        {
            public string Game { get; set; }
            public string Path { get; set; }
        }

        public List<string> RecentUsedFiles { get; set; } = new List<string>();
        public List<GamePathPair> GameDirectories { get; set; } = new List<GamePathPair>();
    }

    class PackFileManagerSettingManager
    {
        public static string InstallationPath
        {
            get
            {
                return Path.GetDirectoryName(Application.ExecutablePath);
            }
        }
        // path to save game directories in
        public static string GameDirFilepath
        {
            get
            {
                return Path.Combine(InstallationPath, "gamedirs.txt");
            }
        }

        public static string SettingsFile
        {
            get
            {
                return Path.Combine(InstallationPath, "PackFileManagerSettings.txt");
            }
        }

        public static PackFileManagerSettings CurrentSettings { get; set; }

        public static void Save()
        {
            var jsonStr = JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented);
            File.WriteAllText(SettingsFile, jsonStr);
        }

        public static PackFileManagerSettings Load()
        {
            if (File.Exists(SettingsFile))
            {
                var content = File.ReadAllText(SettingsFile);
                CurrentSettings = JsonConvert.DeserializeObject<PackFileManagerSettings>(content);
            }
            else
            {
                CurrentSettings = new PackFileManagerSettings();
            }

            return CurrentSettings;
        }
    }
}
