using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Common;
using Filetypes;
using Filetypes.Codecs;

namespace PackFileTest {
    class PackTest {
#pragma warning disable 414
        // just to keep track of what's available in testAll:
        // -mt: run building models test
        // -mb: run naval models test
        private static string[] OPTIONS = { 
            // -tm: initialize DBTypeMap from given game
            "-tm",
            // -db: test db files; -t: also run tsv export/reimport test
            "-db", "-t",
            // -Xm: run building/naval model tests
            "-bm", "-nm",
            // -uv: run unit variant tests
            "-uv", 
            // -g: set games to run against (default: none)
            "-g",
            // -gf: run group formation tests
            "-gf", 
            // -v : verbose output
            "-v",
            // -tg: test game
            "-tg",
            // -x : don't wait for user input after finishing
            "-x",
            // -cr: check references
            "-cr",
            // -dg: dump guids from pack db files
            "-dg"
        };
#pragma warning restore 414

        bool testTsvExport = false;
        bool outputTables = false;
        bool verbose = false;
        bool waitForKey = true;

        private List<PackedFileTest.TestFactory> testFactories = new List<PackedFileTest.TestFactory>();
        private List<Game> games = new List<Game>();

        private static string OPTIONS_FILENAME = "testoptions.txt";

        public static void Main(string[] args) {
            new PackTest().TestAll(args);
        }

        void TestAll(string[] args) {
            IEnumerable<string> arguments = args;
            if (args.Length == 0) {
                if (File.Exists(OPTIONS_FILENAME)) {
                    arguments = File.ReadAllLines(OPTIONS_FILENAME);
                } else {
                    Console.Error.Write("Missing options; use file {0}", OPTIONS_FILENAME);
                    return;
                }
            }

            // make sure R2 knows its directory (no autodetect)
            if (File.Exists("gamedir_r2tw.txt")) {
                Game.R2TW.GameDirectory = File.ReadAllText("gamedir_r2tw.txt").Trim();
            }

            foreach (string dir in arguments) {
                if (dir.StartsWith("#") || dir.Trim().Equals("")) {
                    continue;
                }
                //CaFieldInfo inf;
                if (dir.StartsWith("-g")) {
                    games.Clear();
                    string gamesArg = dir.Substring(2);
                    if ("ALL".Equals(gamesArg)) {
                        games.AddRange(Game.Games);
                    } else {
                        string[] gameIds = gamesArg.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string gameId in gameIds) {
                            games.Add(Game.ById(gameId));
                        }
                    }
                } else if (dir.Equals("-v")) {
                    Console.WriteLine("Running in verbose mode");
                    verbose = true;
                } else if (dir.Equals("-cr")) {
                    CheckReferences();
                } else if (dir.Equals("-x")) {
                    waitForKey = false;
                } else if (dir.StartsWith("-tm")) {
                    string typeMapFile = dir.Substring(3);
                    DBTypeMap.Instance.initializeFromFile(typeMapFile);
                } else if (dir.Equals("-t")) {
                    Console.WriteLine("TSV export/import enabled");
                    testTsvExport = true;
                } else if (dir.Equals("-db")) {
                    Console.WriteLine("Database Test enabled");
                    testFactories.Add(CreateDbTest);
                } else if (dir.Equals("-uv")) {
                    Console.WriteLine("Unit Variant Test enabled");
                    testFactories.Add(CreateUnitVariantTest);
                } else if (dir.StartsWith("-gf")) {
                    Console.WriteLine("Group formations test enabled");
                    GroupformationTest test = new GroupformationTest();
                    test.testFile(dir.Split(" ".ToCharArray())[1].Trim());
                } else if (dir.Equals("-ot")) {
                    outputTables = true;
                    Console.WriteLine("will output tables of db files");
                } else if (dir.StartsWith("-tg")) {
                    foreach (Game game in games) {
                        Console.WriteLine("Testing game {0}", game.Id);
                        PackedFileTest.TestAllPacks(testFactories, Path.Combine(game.DataDirectory), verbose);
                    }
                } else if (dir.StartsWith("-dg")) {
                    string file = dir.Substring(3);
                    PackFile pack = null;
                    List<string> tables = new List<string>();
                    foreach (string line in File.ReadAllLines(file)) {
                        if (pack == null) {
                            pack = new PackFileCodec().Open(line);
                        } else {
                            tables.Add(line);
                        }
                    }
                    DumpAllGuids(pack, tables);
                } else {
                    PackedFileTest.TestAllPacks(testFactories, dir, verbose);
                }
            }
            if (waitForKey) {
                Console.WriteLine("Test run finished, press any key");
                Console.ReadKey();
            }
        }

        void DumpAllGuids(PackFile pack, List<string> tables) {
            foreach (PackedFile file in pack) {
                if (file.FullPath.StartsWith("db")) {
                    string table = DBFile.Typename(file.FullPath);
                    if (tables.Contains(table)) {
                        DBFileHeader header = PackedFileDbCodec.readHeader(file);
                        Console.WriteLine("{0} - {1}", table, header.GUID);
                    }
                }
            }
        }

        void CheckReferences() {
            foreach (Game game in games) {
                if (!game.IsInstalled) {
                    continue;
                }
                List<ReferenceChecker> checkers = ReferenceChecker.CreateCheckers();
                foreach (string packPath in Directory.GetFiles(game.DataDirectory, "*pack")) {
                    Console.WriteLine("adding {0}", packPath);
                    PackFile packFile = new PackFileCodec().Open(packPath);
                    foreach (ReferenceChecker checker in checkers) {
                        checker.PackFiles.Add(packFile);
                    }
                }
                Console.WriteLine();
                Console.Out.Flush();
                foreach (ReferenceChecker checker in checkers) {
                    checker.CheckReferences();
                    Dictionary<PackFile, CheckResult> result = checker.FailedResults;
                    foreach (PackFile pack in result.Keys) {
                        CheckResult r = result[pack];
                        Console.WriteLine("pack {0} failed reference from {1} to {2}",
                            pack.Filepath, r.ReferencingString, r.ReferencedString, string.Join(",", r.UnfulfilledReferences));
                    }
                }
            }
        }

        #region Test Factory Methods
        //        PackedFileTest CreateBuildingModelTest() {
        //            return new ModelsTest<BuildingModel> {
        //                Codec = BuildingModelCodec.Instance,
        //                ValidTypes = "models_building_tables",
        //                Verbose = verbose
        //            };
        //        }
        //        PackedFileTest CreateNavalModelTest() {
        //            return new ModelsTest<ShipModel> {
        //                Codec = ShipModelCodec.Instance,
        //                ValidTypes = "models_naval_tables",
        //                Verbose = verbose
        //            };
        //        }
        PackedFileTest CreateUnitVariantTest() {
            return new UnitVariantTest();
        }
        PackedFileTest CreateDbTest() {
            return new DBFileTest {
                TestTsv = testTsvExport,
                OutputTable = outputTables,
                Verbose = verbose
            };
        }
        #endregion

    }
}
