using System;
using System.IO;
using System.Collections.Generic;
using Common;

namespace MMS {
    class Mod {
        public Mod(string name) {
            this.name = name;
            
            Directory.CreateDirectory(ModDirectory);
        }

        string name;
        public string Name {
            get {
                return name;
            }
            set {
                if (name != null) {
                    string previousDirectory = ModDirectory;
                    string targetDirectory = Path.Combine(MmsBaseDirectory, value);
                    if (!Directory.Exists(targetDirectory)) {
                        Directory.Move(previousDirectory, targetDirectory);
                        name = value;
                    } else {
                        throw new InvalidOperationException(
                            string.Format("Cannot rename mod: new backup directory {0} exists", 
                                      targetDirectory));
                    }
                }
            }
        }

        #region Backup Path and Accessor
        public string ModDirectory {
            get { return Path.Combine(MmsBaseDirectory, Name); }
        }

        // MMS directory
        public static string MmsBaseDirectory {
            get {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appDataPath = Path.Combine(appDataPath, "MMS");
                return appDataPath;
                // the old path (in the ak installation folder)
                // return Path.Combine(ModTools.Instance.InstallDirectory, "MMS");
            }
        }
        public IFileDataAccessor Accessor {
            get { return new FileSystemDataAccessor(ModDirectory); }
        }
        #endregion

        #region Synchronizers
        ModDataSynchronizer DataSynchronizer {
            get {
                return new ModDataSynchronizer(this);
            }
        }
        #endregion

        bool isActive;
        public bool IsActive {
            get {
                return isActive;
            }
            set {
                if (IsActive == value || string.IsNullOrEmpty(name)) {
                    return;
                }
                if (value) {
#if DEBUG
                    Console.WriteLine("*** restoring backup of {0}", Name);
#endif
                    // retrieve data from mod directory
                    Restore();

                    ModTools.SetBobRulePackName(Name);

                } else if (!string.IsNullOrEmpty(Name)) {
#if DEBUG
                    Console.WriteLine("*** backing up {0}", Name);
#endif
                    Backup(true);
                }
                isActive = value;
            }
        }

        public void Backup(bool restoreOriginal = false) {
            // backup files from raw data path to the mod directory
            ModDataSynchronizer synchronizer = DataSynchronizer;
            synchronizer.BackupToMod();
            if (restoreOriginal) {
                ModTools.RestoreOriginalData(synchronizer.ChangedFiles);
            }
        }
        public void Restore() {
            DataSynchronizer.SynchronizeFromMod();
        }

        #region Mod Install/Uninstall
        public void Install() {
            if (!File.Exists(PackFilePath)) {
                throw new FileNotFoundException("Pack file not present");
            }
            File.Copy(PackFilePath, InstalledPackPath);
            //bool contained = false;
            //List<string> writeLines = new List<string>();
            //if (File.Exists(Game.STW.ScriptFile)) {
            //    foreach (string line in File.ReadAllLines(Game.STW.ScriptFile)) {
            //        string addLine = line;
            //        if (line.Contains(PackFileName)) {
            //            addLine = ScriptFileEntry;
            //            contained = true;
            //        }
            //        writeLines.Add(addLine);
            //    }
            //}
            //if (!contained) {
            //    writeLines.Add(ScriptFileEntry);
            //}
            //File.WriteAllLines(Game.STW.ScriptFile, writeLines);
        }
        public void Uninstall() {
            if (File.Exists(InstalledPackPath)) {
                File.Delete(InstalledPackPath);
            }
            //if (File.Exists(Game.STW.ScriptFile)) {
            //    List<string> writeLines = new List<string>();
            //    foreach (string line in File.ReadAllLines(Game.STW.ScriptFile)) {
            //        string addLine = line;
            //        if (line.Contains(PackFileName) && !line.StartsWith("#")) {
            //            addLine = string.Format("#{0}", ScriptFileEntry);
            //        }
            //        writeLines.Add(addLine);
            //    }
            //    File.WriteAllLines(Game.STW.ScriptFile, writeLines);
            //}
        }
        #endregion

        public string InstalledPackPath {
            get {
                return Path.Combine(Game.STW.DataDirectory, PackFileName);
            }
        }
        public string ScriptFileEntry {
            get {
                return string.Format("mod \"{0}\";", PackFileName);
            }
        }
        public string PackFileName {
            get {
                return string.Format("{0}.pack", Name);
            }
        }
        public string PackFilePath {
            get {
                return Path.Combine(ModTools.Instance.RetailPath, "data", PackFileName);
            }
        }

        public ICollection<string> EditedAfterPackCreation {
            get {
                List<string> newerInWorkingSet = new List<string>();
                DateTime packFileTime = File.GetLastWriteTime(PackFilePath);
                foreach (string file in new DirectoryEnumerable(ModTools.Instance.WorkingDataPath)) {
                    if (File.GetLastWriteTime(file) > packFileTime) {
                        newerInWorkingSet.Add(file);
                    }
                }
                return newerInWorkingSet;
            }
        }

        #region Overrides
        public override string ToString() {
            return string.Format("{0}{1}", Name, (IsActive ? " *" : "")); ;
        }
        public override bool Equals(object obj) {
            bool result = false;
            if (obj is Mod) {
                result = (obj as Mod).name.Equals(name);
            }
            return result;
        }
        public override int GetHashCode() {
            return name.GetHashCode();
        }
        #endregion
    }
}
