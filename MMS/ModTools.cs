using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using Common;

namespace MMS {
    /*
     * Represents the Mod Tools installation and is responsible for original data backup and restore
     * (from modding.zip in game installation directory).
     */
    class ModTools {
        private static ModTools instance;
        public static ModTools Instance {
            get {
                if (instance == null) {
                    instance = new ModTools();
                }
                return instance;
            }
        }

        private ModTools() { }

        string installDir;
        public string InstallDirectory {
            get {
                return installDir;
            }
            set {
                installDir = value;
                installationAccessor = new FileSystemDataAccessor(installDir);
                // RestoreOriginalData();
            }
        }

        IFileDataAccessor installationAccessor;
        public IFileDataAccessor InstallationAccessor {
            get {
                return installationAccessor;
            }
        }

        // the modding.zip file in the original shogun installation
        public static string OriginalZipPath {
            get {
                return Path.Combine(Game.STW.GameDirectory, "modding", "modding.zip");
            }
        }
        IFileDataAccessor originalDataAccessor;
        public IFileDataAccessor OriginalDataAccessor {
            get {
                if (originalDataAccessor == null) {
                    originalDataAccessor = new ZipFileAccessor(OriginalZipPath);
                }
                return originalDataAccessor;
            }
        }

        public string RawDataPath {
            get {
                return Path.Combine(InstallDirectory, "raw_data");
            }
        }
        public string WorkingDataPath {
            get {
                return Path.Combine(InstallDirectory, "working_data");
            }
        }
        public string BobRuleFilePath {
            get {
                return Path.Combine(WorkingDataPath, "rules.bob");
            }
        }

        public string RetailPath {
            get {
                return Path.Combine(InstallDirectory, "retail");
            }
        }
        public string BinariesPath {
            get {
                return Path.Combine(InstallDirectory, "binaries");
            }
        }

        /*
         * Edit rules.bob to create a pack with the mod's name.
         */
        static readonly Regex PACK_ENTRY_RE = new Regex(@".*/(.*)\.pack");
        public static void SetBobRulePackName(string newName) {
            if (File.Exists(ModTools.Instance.BobRuleFilePath)) {
                string[] lines = File.ReadAllLines(ModTools.Instance.BobRuleFilePath);
                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i];
                    if (PACK_ENTRY_RE.IsMatch(line)) {
                        Match match = PACK_ENTRY_RE.Match(line);
                        string oldPackName = match.Groups[1].Value;
                        string newLine = line.Replace(oldPackName, newName);
                        lines[i] = newLine;
                    }
                }
                File.WriteAllLines(ModTools.Instance.BobRuleFilePath, lines);
            }
        }

        #region Restore Original Data
        static DirectorySynchronizer RestoreSynchronizer {
            get {
                // restore edited files from original raw data
                DirectorySynchronizer synchronizer = new DirectorySynchronizer {
                    // from original raw data directory
                    SourceAccessor = Instance.OriginalDataAccessor,
                    // to working set raw data
                    TargetAccessor = Instance.InstallationAccessor,
                    // if file was edited from the original
                    CopyFile = NewerThanOriginalFile,
                    DeleteAdditionalFiles = true
                };
                return synchronizer;
            }
        }
        /*
         * Restore raw_data from backup and clean working_data directory.
         */
        public static void RestoreOriginalData() {
            // restore edited files from original raw data
            DirectorySynchronizer synchronizer = RestoreSynchronizer;
            foreach (string directory in Instance.OriginalDataAccessor.GetDirectories("")) {
                synchronizer.Synchronize(directory);
            }

            ModTools.SetBobRulePackName("mod");

            if (Directory.Exists(Instance.RetailPath)) {
                Directory.Delete(Instance.RetailPath, true);
            }
        }
        public static void RestoreOriginalData(List<string> restoreCandidates) {
            // restore edited files from original raw data
            DirectorySynchronizer synchronizer = RestoreSynchronizer;
            foreach (string file in restoreCandidates) {
                synchronizer.SynchronizeFile(file);
            }

            ModTools.SetBobRulePackName("mod");

            if (Directory.Exists(Instance.RetailPath)) {
                Directory.Delete(Instance.RetailPath, true);
            }
        }

        // query if the given file has been edited from the original
        public static bool NewerThanOriginalFile(string file) {
            // not present in installation directory... so yeah, get it
            bool result = !Instance.InstallationAccessor.FileExists(file);
            // was edited in the installation directory... restore it
            result |= Instance.InstallationAccessor.GetLastWriteTime(file) > Instance.originalDataAccessor.GetLastWriteTime(file);
            return result;
        }
        #endregion

        /*
        void CreateZipFile() {
#if DEBUG
            DateTime start = DateTime.Now;
#endif
            ICollection<Thread> zipThreads = new List<Thread>();
            ICollection<Zipper> zippers = new List<Zipper>();
            foreach (string subDirectory in Directory.GetDirectories(RawDataPath)) {
                Zipper zipper = new Zipper(String.Format("{0}.zip", subDirectory), subDirectory);
                Thread zipThread = new Thread(zipper.Zip);
                zipThreads.Add(zipThread);
                zipThread.Start();
            }
            // wait for all threads to finish zipping
            foreach (Thread thread in zipThreads) {
                thread.Join();
            }

#if DEBUG
            DateTime startMerge = DateTime.Now;
            Console.WriteLine("starting merge of files");
#endif
            // merge all files from all created zips
            ZipFile mergedZip = ZipFile.Create(OriginalRawDataZipPath);
            mergedZip.BufferSize = zippers.Count * 100 * 1024 * 1024;
            mergedZip.BeginUpdate();
            foreach (Zipper zipper in zippers) {
                ZipFile toMerge = zipper.CreatedZip;
                toMerge.BufferSize = 100 * 1024 * 1024;
                foreach (ZipEntry entry in toMerge) {
                    mergedZip.Add(entry);
                }
            }
            mergedZip.CommitUpdate();
            mergedZip.Close();
#if DEBUG
            TimeSpan duration = DateTime.Now - start;
            Console.WriteLine("merge finished; took {0}", duration);
            Console.WriteLine("full compression took {0}", duration);
#endif
        }

        void AddFilesToZip(ZipFile addTo, string directory) {
            addTo.AddDirectory(directory);
            foreach (string file in Directory.GetFiles(directory)) {
                addTo.Add(file);
            }
            foreach (string dir in Directory.GetDirectories(directory)) {
                AddFilesToZip(addTo, dir);
            }
        }
    }

    /*
    class Zipper {
        string zipFileName;
        string baseDirectory;

        ZipFile zipFile;
        public ZipFile CreatedZip {
            get {
                return zipFile;
            }
        }

        public Zipper(string zipFile, string baseDir) {
            zipFileName = zipFile;
            baseDirectory = baseDir;
        }

        public void Zip() {
#if DEBUG
            DateTime start = DateTime.Now;
            Console.WriteLine("Compressing {0} into {1}", baseDirectory, zipFileName);
#endif
            zipFile = ZipFile.Create(zipFileName);
            zipFile.BufferSize = 100 * 1024 * 1024;
            zipFile.BeginUpdate();
            AddFilesToZip(baseDirectory);
            zipFile.CommitUpdate();
#if DEBUG
            Console.WriteLine("Compression for {0} finished: took {1}", baseDirectory, DateTime.Now - start);
#endif
        }

        void AddFilesToZip(string directory) {
            zipFile.AddDirectory(directory);
            foreach (string file in Directory.GetFiles(directory)) {
                zipFile.Add(file);
            }
            foreach (string dir in Directory.GetDirectories(directory)) {
                AddFilesToZip(dir);
            }
        }
     * */
    }
}
