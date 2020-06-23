using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace PfmCL {
    class Pfm {
        public delegate void PackAction(string packFileName, List<string> containedFiles);

        static void Main(string[] args) {
            if (args.Length < 1) {
                Usage();
                return;
            }

            List<string> arguments = new List<string>(args.Length - 1);
            for (int i = 2; i < args.Length; i++) {
                arguments.Add(args[i]);
            }
            new Pfm(args[0]) {
                PackFileName = args[1],
                ContainedFiles = arguments
            }.Run();
        }

        string PackFileName {
            get;
            set;
        }
        List<string> containedFileList;
        List<string> ContainedFiles {
            get {
                List<string> result = containedFileList;
                if (result == null || result.Count == 0) {
                    try {
                        result = new List<string>();
                        PackFile pack = new PackFileCodec().Open(PackFileName);
                        pack.Files.ForEach(p => result.Add(p.FullPath));
                    } catch {}
                }
                return result;
            }
            set {
                containedFileList = value;
            }
        }
        PackAction action;
        
        private bool Verbose {
            get; set;
        }

        Pfm(string command) {
            if (command.Contains("v")) {
                Verbose = true;
                command = command.Replace("v", "");
            }
            switch (command) {
                case "c": 
                    action = CreatePack;
                    break;
                case "x":
                    action = ExtractPack;
                    break;
                case "t":
                    action = ListPack;
                    break;
                case "u":
                    action = UpdatePackReplace;
                    break;
                case "a":
                    action = UpdatePackAddOnly;
                    break;
                case "m":
                    action = ExportToModToolXml;
                    break;
            }
        }

        public void Run() {
            if (action != null) {
                action(PackFileName, ContainedFiles);
            } else {
                Usage();
            }
        }

        static void Usage() {
            Console.WriteLine("usage: pfm <command> <packFile> [<file1>,...]");
            Console.WriteLine("available commands:");
            Console.WriteLine("'c' to create");
            Console.WriteLine("'x' to extract (no file arguments: extract all)");
            Console.WriteLine("'t' to list contents (ignores file arguments)");
            Console.WriteLine("'u' to update (replaces files with same path)");
            Console.WriteLine("'a' to add (does not replace files with same path)");
//            Console.WriteLine("'m' to export to official mod tool format XML");
        }
        
        private void HandlingFile(string file) {
            if (Verbose) {
                Console.WriteLine(file);
            }
        }

        /*
         * Create a new pack containing the given files.
         */
        void CreatePack(string packFileName, List<string> containedFiles) {
            try {
                PFHeader header = new PFHeader("PFH4") {
                    Version = 0,
                    Type = PackType.Mod
                };
                PackFile packFile = new PackFile(packFileName, header);
                foreach (string file in containedFiles) {
                    try {
                        HandlingFile(file);
                        PackedFile toAdd = new PackedFile(file);
                        packFile.Add(toAdd, true);
                    } catch (Exception e) {
                        Console.Error.WriteLine("Failed to add {0}: {1}", file, e.Message);
                    }
                }
                new PackFileCodec().WriteToFile(packFileName, packFile);
            } catch (Exception e) {
                Console.Error.WriteLine("Failed to write {0}: {1}", packFileName, e.Message);
            }
        }

        /*
         * Lists the contents of the given pack file.
         * List parameter is ignored.
         */
        void ListPack(string packFileName, List<string> containedFiles) {
            try {
                PackFile pack = new PackFileCodec().Open(packFileName);
                foreach (PackedFile file in pack) {
                    Console.WriteLine(file.FullPath);
                }
            } catch (Exception e) {
                Console.Error.WriteLine("Failed to list contents of {0}: {1}", packFileName, e.Message);
            }
        }

        /*
         * Unpacks the given files from the given pack file, or all if contained files list is empty.
         */
        void ExtractPack(string packFileName, List<string> containedFiles) {
            PackFile pack = new PackFileCodec().Open(packFileName);
            foreach(string entryPath in containedFiles) {
                try {
                    PackEntry entry = pack[entryPath];
                    if (entry is VirtualDirectory) {
                        VirtualDirectory directory = entry as VirtualDirectory;
                        directory.AllFiles.ForEach(f => ExtractPackedFile(f));
                    } else {
                        ExtractPackedFile(entry as PackedFile);
                    }
                } catch (Exception e) {
                    Console.Error.WriteLine("Failed to extract {0}: {1}", entryPath, e.Message);
                }
            }
        }
        /*
         * Extracts the given file, retaining its path relative to the current directory.
         */
        private void ExtractPackedFile(PackedFile file) {
            try {
                string systemPath = file.FullPath.Replace('/', Path.DirectorySeparatorChar);
                string directoryName = Path.GetDirectoryName(systemPath);
                if (directoryName.Length != 0 && !Directory.Exists(directoryName)) {
                    Directory.CreateDirectory(directoryName);
                }
                HandlingFile(file.FullPath);
                using (var fileStream = new MemoryStream(file.Data)) {
                    fileStream.CopyTo(File.Create(systemPath));
                }
            } catch (Exception e) {
                Console.Error.WriteLine("Failed to extract {0}: {1}", file.FullPath, e.Message);
            }
        }

        /*
         * Updates the given pack file, adding the files from the given list.
         * Will replace files already present in the pack when the replace parameter is true (default).
         */
        void UpdatePack(string packFileName, List<string> toAdd, bool replace) {
            try {
                PackFile toUpdate = new PackFileCodec().Open(packFileName);
                foreach (string file in toAdd) {
                    try {
                        HandlingFile(file);
                        toUpdate.Add(new PackedFile(file), replace);
                    } catch (Exception e) {
                        Console.Error.WriteLine("Failed to add {0}: {1}", file, e.Message);
                    }
                }
                string tempFile = Path.GetTempFileName();
                new PackFileCodec().WriteToFile(tempFile, toUpdate);
                File.Delete(packFileName);
                File.Move(tempFile, packFileName);
            } catch (Exception e) {
                Console.Error.WriteLine("Failed to update {0}: {1}", packFileName, e.Message);
            }
        }

        void UpdatePackReplace(string packFileName, List<string> toAdd) {
            UpdatePack(packFileName, toAdd, true);
        }
        
        /*
         * Updates the given pack file, adding the files from the list;
         * will not replace files already present in the pack.
         */
        void UpdatePackAddOnly(string packFileName, List<string> toAdd) {
            UpdatePack(packFileName, toAdd, false);
        }
        
        void ExportToModToolXml(string packFileName, List<string> unused) {
            
        }
    }
}
