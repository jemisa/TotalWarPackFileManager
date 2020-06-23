using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMS {
    /*
     * Synchronizes data to and from a mod backup directory to the working directory.
     */
    class ModDataSynchronizer {
        public ModDataSynchronizer(Mod mod) {
            ModAccessor = new FileSystemDataAccessor(mod.ModDirectory);
            WorkingSetAccessor = ModTools.Instance.InstallationAccessor;
            OriginalAccessor = ModTools.Instance.OriginalDataAccessor;
        }

        // the mod subdirectory in which to keep the backup
        public IFileDataAccessor ModAccessor;

        // the working set directory to backup from
        public IFileDataAccessor WorkingSetAccessor;

        // the original files from the unpacked modding.zip
        // used to check if a working set file was added at all
        // may be zero, then file will always be copied if a backup of it isn't already present
        public IFileDataAccessor OriginalAccessor;

        List<string> restoreFromOriginal = new List<string>();
        // only valid after a backup has been performed
        public List<string> ChangedFiles {
            get {
                return restoreFromOriginal;
            }
        }

        public void SynchronizeFromMod() {
            DirectorySynchronizer synchronizer = new DirectorySynchronizer {
                // from mod
                SourceAccessor = ModAccessor,
                // to working set
                TargetAccessor = WorkingSetAccessor,
                // all files
                CopyFile = DirectorySynchronizer.AlwaysCopy,
                // don't delete any files that are already there...
                // we assume the target directory is in its original state
                // and we need all files that weren't included in the backup
                DeleteAdditionalFiles = false
            };
            foreach (string dir in ModAccessor.GetDirectories("")) {
                if (BackupDir(dir)) {
                    synchronizer.Synchronize(dir);
                }
            }
        }

        static bool BackupDir(string dir) {
            bool result = !"MMS".Equals(dir);
            result &= !"binaries".Equals(dir);
            return result;
        }

        public void BackupToMod() {
            restoreFromOriginal.Clear();
            DirectorySynchronizer synchronizer = new DirectorySynchronizer {
                // from working set
                SourceAccessor = WorkingSetAccessor,
                // to mod backup directory
                TargetAccessor = ModAccessor,
                // only if file was edited and there isn't a recent backup already
                CopyFile = BackupIfEdited
            };
            foreach (string dir in WorkingSetAccessor.GetDirectories("")) {
                if (BackupDir(dir)) {
                    synchronizer.Synchronize(dir);
                }
            }
            // remember all files that were backed up
            restoreFromOriginal.AddRange(synchronizer.SynchronizedFiles);
        }

        // query if the given file has been edited from the original
        static bool NewerThanOriginalFile(string file) {
            // not present in installation directory... so yeah, get it
            bool result = !ModTools.Instance.InstallationAccessor.FileExists(file);
            // was edited in the installation directory... restore it
            result |= ModTools.Instance.InstallationAccessor.GetLastWriteTime(file) >
                ModTools.Instance.OriginalDataAccessor.GetLastWriteTime(file);
            return result;
        }

        /*
         * Check if the given file was edited from the original (if OriginalAccessor != null),
         * and if there isn't a recent backup of that file present already.
         */
        bool BackupIfEdited(string file) {
            bool result = true;
            if (OriginalAccessor != null) {
                result = !OriginalAccessor.FileExists(file);
                // file created during bob session?
                if (!result) {
                    // so it was originally there... but was it edited during this session?
                    DateTime lastEdit = WorkingSetAccessor.GetLastWriteTime(file);
                    DateTime originalTime = OriginalAccessor.GetLastWriteTime(file);
                    result = lastEdit > originalTime;
                }
            }
            if (result && ModAccessor.FileExists(file)) {
                // so it was edited... but was that edit before this session, or do we have it already?
                if (WorkingSetAccessor.GetLastWriteTime(file) > ModAccessor.GetLastWriteTime(file)) {
                    result = true;
                } else {
                    result = false;
                    restoreFromOriginal.Add(file);
                }
            }
            return result;
        }
    }
}
