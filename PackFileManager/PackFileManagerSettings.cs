using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace PackFileManager
{
    public class PackFileManagerSettings
    {
        public class GamePathPair
        {
            public string Game { get; set; }
            public string Path { get; set; }
        }

         public class CustomFileExtentionHighlightsMapping
         {
             public string Extention { get; set; }
             public string HighlightMapping { get; set; }
         }

        public string MyModDirectory { get; set; }
        public List<string> RecentUsedFiles { get; set; } = new List<string>();
        public List<GamePathPair> GameDirectories { get; set; } = new List<GamePathPair>();
        public List<CustomFileExtentionHighlightsMapping> CustomFileExtentionHighlightsMappings { get; set; } = new List<CustomFileExtentionHighlightsMapping>();
    }

    class PackFileManagerSettingService
    {
        public static string InstallationPath
        {
            get
            {
                return Path.GetDirectoryName(Application.ExecutablePath);
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
