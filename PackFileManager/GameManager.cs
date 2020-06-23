using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Common;
using Filetypes;
using PackFileManager.Properties;
using System.Windows.Forms;
using CommonDialogs;

using TableVersions = System.Collections.Generic.SortedList<string, int>;

namespace PackFileManager {
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
                DBTypeMap.Instance.InitializeTypeMap(InstallationPath);
            }

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

        static string InstallationPath {
            get {
                return Path.GetDirectoryName(Application.ExecutablePath);
            }
        }
        #region Game Directories File
        // path to save game directories in
        static string GameDirFilepath {
            get {
                return Path.Combine(InstallationPath, "gamedirs.txt");
            }
        }

        // the entry for the given game
        static string GamedirFileEntry(Game g) {
            return string.Format("{0}{1}{2}", g.Id, Path.PathSeparator,
                                 g.GameDirectory == null ? Game.NOT_INSTALLED : g.GameDirectory);
        }

        // load the given game's directory from the gamedirs file
        public static void LoadGameLocationFromFile(Game g) {
            string result = null;
            // load from file
            if (File.Exists(GameDirFilepath)) {
                // marker that file entry was present
                result = "";
                foreach (string line in File.ReadAllLines(GameDirFilepath)) {
                    string[] split = line.Split(new char[] { Path.PathSeparator });
                    if (split[0].Equals(g.Id)) {
                        result = split[1];
                        break;
                    }
                }
            }
            if (result != null) {
                g.GameDirectory = result;
            }
        }

        // write game directories to gamedirs file
        static void SaveGameDirs() {
            // save to file
            List<string> entries = new List<string>();
            if (File.Exists(GameDirFilepath)) {
                entries.AddRange(File.ReadAllLines(GameDirFilepath));
            }
            List<string> newEntries = new List<string>();
            foreach (Game g in Game.Games) {
                string write = GamedirFileEntry(g);
                foreach (string entry in entries) {
                    if (entry.StartsWith(g.Id)) {
                        write = GamedirFileEntry(g);
                    }
                }
                newEntries.Add(string.Format("{0}{1}", write, Environment.NewLine));
            }
            File.WriteAllLines(GameDirFilepath, newEntries);
        }
        #endregion

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
                string schemaFile = Path.Combine(InstallationPath, game.MaxVersionFilename);
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
            string filePath = Path.Combine(InstallationPath, game.MaxVersionFilename);
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

