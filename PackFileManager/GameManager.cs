using System.Collections.Generic;
using System.IO;
using Common;
using Filetypes;
using PackFileManager.Properties;
using System.Windows.Forms;
using CommonDialogs;

using TableVersions = System.Collections.Generic.SortedList<string, int>;
using System.Linq;

namespace PackFileManager
{
    /*
     * Manager for the available games.
     */
    public class GameManager {

        public delegate void GameChange();
        public event GameChange GameChanged;

#if DEBUG
        bool createdGameSchemata = false;
#endif
        
        private static GameManager instance;
        public static GameManager Instance {
            get {
                if (instance == null) {
                    instance = new GameManager();
                }
                return instance;
            }
        }
        private GameManager() {
            if (!DBTypeMap.Instance.Initialized) {
                DBTypeMap.Instance.InitializeTypeMap(PackFileManagerSettingService.InstallationPath);
            }

            PackFileManagerSettingService.Load();

            // correct game install directories 
            // (should be needed for first start only)
            Game.Games.ForEach(g => LoadGameLocationFromFile(g));
            CheckGameDirectories();
            
            string gameName = Settings.Default.CurrentGame;
            if (!string.IsNullOrEmpty(gameName)) {
                CurrentGame = Game.ById(gameName);
            }
            foreach(Game game in Game.Games) {
                if (CurrentGame != null) {
                    break;
                }
                if (game.IsInstalled) {
                    CurrentGame = game;
                }
            }
            // no game installed?
            if (CurrentGame == null) {
                CurrentGame = DefaultGame;
            }

#if DEBUG
            if (createdGameSchemata) {
                MessageBox.Show("Had to create game schema file");
            }
#endif
        }

        // load the given game's directory from the gamedirs file
        public static void LoadGameLocationFromFile(Game g)
        {
            var savedGameDirectories = PackFileManagerSettingService.CurrentSettings.GameDirectories;
            var gameDir = savedGameDirectories.FirstOrDefault(x => x.Game == g.Id);
            g.GameDirectory = gameDir?.Path;
        }

        // write game directories to gamedirs file
        static void SaveGameDirs() 
        {
            foreach (var game in Game.Games)
            {
                var dir = game.GameDirectory == null ? Game.NOT_INSTALLED : game.GameDirectory;
                var currentEntry = PackFileManagerSettingService.CurrentSettings.GameDirectories.FirstOrDefault(x => x.Game == game.Id);
                if (currentEntry != null)
                    currentEntry.Path = dir;
                else
                    PackFileManagerSettingService.CurrentSettings.GameDirectories.Add(new PackFileManagerSettings.GamePathPair() { Game = game.Id,Path = dir});
            }

            PackFileManagerSettingService.Save();
        }
        

        static Game DefaultGame = Game.TWH2;
        Game current;
        public Game CurrentGame {
            get {
                return current;
            }
            set {
                if (current != value) {
                    current = value != null ? value : DefaultGame;
                    if (current != null) {
                        Settings.Default.CurrentGame = current.Id;

                        // load the appropriate type map
                        LoadGameMaxDbVersions();

                        // invalidate cache of reference map cache
                        List<string> loaded = new PackLoadSequence() {
                            IgnorePack = PackLoadSequence.IsDbCaPack
                        }.GetPacksLoadedFrom(current.GameDirectory);
                        DBReferenceMap.Instance.GamePacks = loaded;
                    }
                    if (GameChanged != null) {
                        GameChanged();
                    }
                }
            }
        }
        
        public static void CheckGameDirectories() {
            bool writeGameDirFile = false;
            foreach(Game g in Game.Games) {
                if (g.GameDirectory == null) {
                    // if there was an empty entry in file, don't ask again
                    DirectoryDialog dlg = new DirectoryDialog() {
                        Description = string.Format("Please point to Location of {0}\nCancel if not installed.", g.Id)
                    };
                    dlg.Refresh();
                    if (dlg.ShowDialog() == DialogResult.OK) {
                        g.GameDirectory = dlg.SelectedPath;
                    } else {
                        // add empty entry to file for next time
                        g.GameDirectory = Game.NOT_INSTALLED;
                    }
                    writeGameDirFile = true;
                } else if (g.GameDirectory.Equals(Game.NOT_INSTALLED)) {
                    // mark as invalid
                    g.GameDirectory = null;
                }
            }
            // don't write the file if user wasn't queried
            if (writeGameDirFile) {
                SaveGameDirs();
            }
        }
        
        #region Game-specific schema (typemap) handling
        private void LoadGameMaxDbVersions() {
            try {
                Game game = CurrentGame;
                if (gameDbVersions.ContainsKey(game)) {
                    return;
                }
                string schemaFile = Path.Combine(PackFileManagerSettingService.InstallationPath, game.MaxVersionFilename);
                if (File.Exists(schemaFile)) {
                    SortedList<string, int> versions = SchemaOptimizer.ReadTypeVersions(schemaFile);
                    if (versions != null) {
                        gameDbVersions.Add(game, versions);
                    }
                } else {
                    // rebuild from master schema
                    CreateSchemaFile(game);
                }
            } catch { }
        }
        public void CreateSchemaFile(Game game) {
            string filePath = Path.Combine(PackFileManagerSettingService.InstallationPath, game.MaxVersionFilename);
            if (game.IsInstalled && !File.Exists(filePath)) {
                SchemaOptimizer optimizer = new SchemaOptimizer() {
                    PackDirectory = Path.Combine(game.GameDirectory, "data"),
                    SchemaFilename = filePath
                };
                optimizer.FilterExistingPacks();

#if DEBUG
                createdGameSchemata = true;
#endif
            }
        }
        
        private Dictionary<Game, TableVersions> gameDbVersions = new Dictionary<Game, TableVersions>();
        public int GetMaxDbVersion(string tableName) {
            int result = -1;
            SortedList<string, int> tablesToVersion;
            if (gameDbVersions.TryGetValue(CurrentGame, out tablesToVersion)) {
                if (!tablesToVersion.TryGetValue(tableName, out result)) {
                    result = -1;
                }
            }
            return result;
        }
        #endregion
    }
}

