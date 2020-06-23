using System;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

namespace MMS {
    /*
     * An abstraction of the access I need to synchronized between directories or zip files.
     */
    interface IFileDataAccessor {
        bool FileExists(string file);
        bool DirectoryExists(string dir);
        bool IsEmpty(string dir);

        DateTime GetLastWriteTime(string file);
        int GetFileAttributes(string file);

        void DeleteFile(string file);
        void DeleteDirectory(string directory);
        void WriteFile(string file, Stream fromStream, DateTime writeTime, int attributes);

        Stream GetFileContents(string file);

        // return contained files and directories
        IEnumerable<string> GetFiles(string dir);
        IEnumerable<string> GetDirectories(string dir);
    }

    /*
     * A file accessor to the file system; all access queries
     * need to be relative to the initially given base directory.
     */
    class FileSystemDataAccessor : IFileDataAccessor {
        string baseDir;
        public FileSystemDataAccessor(string dir) {
            baseDir = string.Format("{0}{1}", dir, Path.DirectorySeparatorChar);
        }
        // existence
        public bool FileExists(string file) {
            return File.Exists(FullPath(file));
        }
        public bool DirectoryExists(string file) {
            return Directory.Exists(FullPath(file));
        }

        // metadata queries
        public DateTime GetLastWriteTime(string file) {
            return File.GetLastWriteTime(FullPath(file));
        }
        public int GetFileAttributes(string file) {
            return (int)File.GetAttributes(FullPath(file));
        }

        // file manipulation
        public void DeleteFile(string file) {
            File.Delete(FullPath(file));
            string dir = Path.GetDirectoryName(FullPath(file));
            while (IsEmpty(dir)) {
                Directory.Delete(dir);
                dir = Path.GetDirectoryName(dir);
            }
        }
        public bool IsEmpty(string dir) {
            return Directory.GetFiles(FullPath(dir)).Length == 0 && Directory.GetDirectories(FullPath(dir)).Length == 0;
        }
        public void DeleteDirectory(string dir) {
            Directory.Delete(FullPath(dir));
        }
        public void WriteFile(string file, Stream fromStream, DateTime writeTime, int attributes) {
            string fullPath = FullPath(file);
            if (!Directory.Exists(Path.GetDirectoryName(fullPath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }
            using (var outStream = File.Create(fullPath)) {
                fromStream.CopyTo(outStream);
            }
            File.SetLastWriteTime(FullPath(file), writeTime);
            File.SetAttributes(FullPath(file), (FileAttributes)attributes);
        }

        // contents retrieval
        public Stream GetFileContents(string file) {
            return File.OpenRead(FullPath(file));
        }

        // directory contents queries
        public IEnumerable<string> GetFiles(string dir) {
            return MakeRelative(Directory.GetFiles(FullPath(dir)));
        }
        public IEnumerable<string> GetDirectories(string dir) {
            if (Directory.Exists(FullPath(dir))) {
                return MakeRelative(Directory.GetDirectories(FullPath(dir)));
            } else {
                return new string[0];
            }
        }

        #region Helpers
        IEnumerable<string> MakeRelative(string[] absolutePaths) {
            List<string> result = new List<string>(absolutePaths.Length);
            foreach (string absolute in absolutePaths) {
                result.Add(GetRelativePath(absolute));
            }
            return result;
        }
        string GetRelativePath(string absolutePath) {
            Uri baseUri = new Uri(baseDir);
            Uri absoluteUri = new Uri(absolutePath);
            Uri relativeUri = baseUri.MakeRelativeUri(absoluteUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }
        string FullPath(string file) {
            return Path.Combine(baseDir, file);
        }
        #endregion

        public override string ToString() {
            return baseDir;
        }
    }

    /*
     * Provide access to zip file contents.
     */
    class ZipFileAccessor : IFileDataAccessor {
        ZipFile zipFile;

        public ZipFileAccessor(string zipFilePath) {
            zipFile = new ZipFile(zipFilePath);
        }

        // existence queries
        public bool FileExists(string file) {
            ZipEntry entry = zipFile.GetEntry(file);
            return entry != null && entry.IsFile;
        }
        public bool DirectoryExists(string dir) {
            ZipEntry entry = zipFile.GetEntry(dir);
            return entry != null && entry.IsDirectory;
        }
        public bool IsEmpty(string dir) {
            throw new NotSupportedException();
        }

        // metadata queries
        public DateTime GetLastWriteTime(string file) {
            DateTime writeTime = zipFile.GetEntry(file).DateTime;
            return writeTime;
        }
        public int GetFileAttributes(string file) {
            return zipFile.GetEntry(file).ExternalFileAttributes;
        }

        // file manipulation
        public void DeleteFile(string file) {
            throw new NotSupportedException();
        }
        public void DeleteDirectory(string dir) {
            throw new NotSupportedException();
        }
        public void WriteFile(string file, Stream stream, DateTime writeTime, int attributes) {
            throw new NotSupportedException();
        }

        // content access
        public Stream GetFileContents(string file) {
            return zipFile.GetInputStream(zipFile.GetEntry(file));
        }

        #region Directory Contents Queries

        // files
        public IEnumerable<string> GetFiles(string dir) {
            return CollectContainedEntries(dir, delegate(ZipEntry entry) { return entry.IsFile; });
        }

        // directories
        public IEnumerable<string> GetDirectories(string dir) {
            return CollectContainedEntries(dir, delegate(ZipEntry entry) { return entry.IsDirectory;  });
        }

        // helper function
        IEnumerable<string> CollectContainedEntries(string dir, Predicate<ZipEntry> collect) {
            ICollection<string> result = new List<string>();
            int startAt = zipFile.FindEntry(dir, false);
            dir = UnifyDirectory(dir);
            for (int i = startAt + 1; i < zipFile.Count; i++) {
                ZipEntry entry = zipFile[i];
                string unifiedName = UnifyDirectory(entry.Name);
                if (collect(entry) && dir.Equals(Path.GetDirectoryName(unifiedName))) {
                    result.Add(entry.Name);
                } else if (!entry.Name.StartsWith(dir)) {
                    // we're done here... gone past the directory
                    // break;
                }
            }
            return result;
        }

        #endregion

        /*
         * Directory entries retrieved from the zip library have '/'
         * as path delimiters and one trailing; make a proper directory path of it.
         */
        static string UnifyDirectory(string dir) {
            if (dir.EndsWith("/")) {
                dir = dir.Remove(dir.Length - 1, 1);
            }
            dir = dir.Replace('/', Path.DirectorySeparatorChar);
            return dir;
        }

        public override string ToString() {
            return Path.GetFileName(zipFile.Name);
        }
    }
}
