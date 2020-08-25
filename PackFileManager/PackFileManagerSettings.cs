using Common;
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

        public GameTypeEnum CurrentGame { get; set; } = GameTypeEnum.Unknown;
        public string MyModDirectory { get; set; }
        public List<string> RecentUsedFiles { get; set; } = new List<string>();
        public List<GamePathPair> GameDirectories { get; set; } = new List<GamePathPair>();
        public List<CustomFileExtentionHighlightsMapping> CustomFileExtentionHighlightsMappings { get; set; } = new List<CustomFileExtentionHighlightsMapping>();
    }

    class PackFileManagerSettingService
    {
        public static string SettingsFile
        {
            get
            {
                return Path.Combine(DirectoryHelper.FpmDirectory, "PackFileManagerSettings.txt");
            }
        }

        public static PackFileManagerSettings CurrentSettings { get; set; }



        public static void AddLastUsedFile(string filePath)
        {
            int maxRecentFiles = 5;

            // Remove the file if it is add already
            var index = CurrentSettings.RecentUsedFiles.IndexOf(filePath);
            if (index != -1)
                CurrentSettings.RecentUsedFiles.RemoveAt(index);

            // Add the file
            CurrentSettings.RecentUsedFiles.Insert(0, filePath);

            // Ensure we only have maxRecentFiles in the list
            var currentFileCount = CurrentSettings.RecentUsedFiles.Count;
            if (currentFileCount > maxRecentFiles)
            {
                CurrentSettings.RecentUsedFiles.RemoveRange(maxRecentFiles, currentFileCount - maxRecentFiles);
            }
            Save();
        }

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
