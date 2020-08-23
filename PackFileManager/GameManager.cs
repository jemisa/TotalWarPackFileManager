using System.Collections.Generic;
using System.IO;
using Common;
using Filetypes;
using PackFileManager.Properties;
using System.Windows.Forms;
using CommonDialogs;

using TableVersions = System.Collections.Generic.SortedList<string, int>;
using System.Linq;
using Filetypes.DB;

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
        private GameManager()
        {
            SchemaManager.Instance.Create();

            PackFileManagerSettingService.Load();

            // correct game install directories 
            // (should be needed for first start only)
            Game.Games.ForEach(g => LoadGameLocationFromFile(g));
            CheckGameDirectories();

            
            var gameEnum = PackFileManagerSettingService.CurrentSettings.CurrentGame;
            if (gameEnum != GameTypeEnum.Unknown)
            {
                CurrentGame = Game.GetByEnum(gameEnum);
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
                    if (current != null) 
                    {
                        PackFileManagerSettingService.CurrentSettings.CurrentGame = current.GameType;
                        PackFileManagerSettingService.Save();

                        // invalidate cache of reference map cache
                        var seq = new PackLoadSequence(){ IgnorePack = PackLoadSequence.IsDbCaPack };
                        List<string> loaded = seq.GetPacksLoadedFrom(current.GameDirectory);
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
    }
}

