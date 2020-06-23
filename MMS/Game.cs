using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Common {
    public class Game {
        private static string WOW_NODE = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}";
        private static string WIN_NODE = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {0}";
        
        public static readonly Game STW = new Game("STW", "34330", "Shogun2");
        public static readonly Game NTW = new Game("NTW", "34030", "Napoleon");
        public static readonly Game ETW = new Game("ETW", "10500", "Empire", "user.empire_script.txt");
        private static readonly Game[] GAMES = new Game[] {
            STW, NTW, ETW
        };

        public static List<Game> Games {
            get {
                List<Game> result = new List<Game>();
                foreach (Game g in GAMES) {
                    result.Add(g);
                }
                return result;
            }
        }
        public static Game ById(string id) {
            Game result = null;
            foreach(Game game in GAMES) {
                if (game.Id.Equals(id)) {
                    result = game;
                    break;
                }
            }
            return result;
        }
        
        public string Id {
            get; private set;
        }
        private string steamId;
        public string UserDir {
            get; private set;
        }
        string gameDirectory;
        public string GameDirectory {
            get {
                string dir = GetInstallLocation(WOW_NODE);
                if (string.IsNullOrEmpty(dir)) {
                    dir = GetInstallLocation(WIN_NODE);
                }
                if (!string.IsNullOrEmpty(gameDirectory)) {
                    dir = gameDirectory;
                }
                return dir;
            }
            set {
                gameDirectory = value;
            }
        }
        public string DataDirectory {
            get {
                return Path.Combine(GameDirectory, "data");
            }
        }
        public string ScriptFilename {
            get;
            private set;
        }
        public string ScriptDirectory {
            get {
                return Path.Combine(UserDir, "scripts");
            }
        }
        public string ScriptFile {
            get {
                string result = Path.Combine(ScriptDirectory, ScriptFilename);
                return result;
            }
        }
        public bool IsInstalled {
            get {
                return Directory.Exists(GameDirectory);
            }
        }
        public string SchemaFilename {
            get {
                return string.Format("schema_{0}.xml", Id);
            }
        }
        
        public Game(string i, string steam, string gameDir, string scriptFile = "user.script.txt") {
            Id = i;
            steamId = steam;
            UserDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                   "The Creative Assembly", gameDir);
            ScriptFilename = scriptFile;
        }

        private string GetInstallLocation(string node) {
            string str = null;
            try {
                string regKey = string.Format(WOW_NODE, steamId);
                str = (string) Registry.GetValue(regKey, "InstallLocation", "");
                // check if directory actually exists
                if (!string.IsNullOrEmpty(str) && !Directory.Exists(str)) {
                    str = null;
                }
            } catch {}
            return str;
        }
    }
}

