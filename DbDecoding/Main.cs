using Common;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.IO;

namespace DbDecoding {
    class MainClass {
        [STAThreadAttribute]
        public static void Main(string[] args) {
            PackFile exported = null;
            bool showManager = (args.Length > 0 && args[0].Equals("-g"));
            if (showManager) {
                DBTableDisplay display = new DBTableDisplay();
                display.ShowDialog();
            } else {
                bool export = (args.Length > 0 && args[0].Equals("-x"));
                Console.WriteLine("exporting undecoded to file");
                exported = new PackFile("undecoded.pack", new PFHeader("PFH4"));
                DBTypeMap.Instance.initializeFromFile("master_schema.xml");
                foreach (Game game in Game.Games) {
                    LoadGameLocationFromFile(game);
                    if (game.IsInstalled) {
                        foreach (string packFileName in Directory.EnumerateFiles(game.DataDirectory, "*pack")) {
                            Console.WriteLine("checking {0}", packFileName);
                            PackFile packFile = new PackFileCodec().Open(packFileName);
                            foreach (VirtualDirectory dir in packFile.Root.Subdirectories) {
                                if (dir.Name.Equals("db")) {
                                    foreach(PackedFile dbFile in dir.AllFiles) {
                                        if (dbFile.Name.Contains("models_naval")) {
                                            continue;
                                        }
                                        // DBFileHeader header = PackedFileDbCodec.readHeader(dbFile);
                                        DBFile decoded = PackedFileDbCodec.Decode(dbFile);
                                        DBFileHeader header = PackedFileDbCodec.readHeader(dbFile);
                                        if (decoded == null && header.EntryCount != 0) {
                                            Console.WriteLine("failed to read {0} in {1}", dbFile.FullPath, packFile);
                                            if (export) {
                                                String exportFileName = String.Format("db/{0}_{1}_{2}", game.Id, dbFile.Name, Path.GetFileName(packFileName)).ToLower();
                                                PackedFile exportedDbFile = new PackedFile(exportFileName, false) {
                                                    Data = dbFile.Data
                                                };
                                                exported.Add(exportedDbFile);
                                            } else {
                                                string key = DBFile.Typename(dbFile.FullPath);
                                                bool unicode = false;
                                                if (game == Game.ETW ||
                                                    game == Game.NTW ||
                                                    game == Game.STW)
                                                {
                                                    unicode = true;
                                                }
                                                DecodeTool.DecodeTool decoder = new DecodeTool.DecodeTool(unicode)
                                                {
                                                    TypeName = key,
                                                    Bytes = dbFile.Data
                                                };
                                                decoder.ShowDialog();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (export) {
                            new PackFileCodec().Save(exported);
                        }
                    } else {
                        Console.Error.WriteLine("Game {0} not installed in {1}", game, game.GameDirectory);
                    }
                }
            }
        }
        // load the given game's directory from the gamedirs file
        public static void LoadGameLocationFromFile(Game g) {
            string result = null;
            string GameDirFilepath = "gamedirs.txt";
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
    }
}
