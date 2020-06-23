using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace MMS {
    /*
     * Synchronizes the contents of two directories.
     */
    class DirectorySynchronizer {
        List<string> synchronizedFiles = new List<string>();
        public List<string> SynchronizedFiles {
            get {
                return synchronizedFiles;
            }
        }
        List<string> deletedFiles = new List<string>();
        public List<string> DeletedFiles {
            get {
                return deletedFiles;
            }
        }

        public static bool AlwaysCopy(string file) {
            return true;
        }

        // needs to be set to decide what files to copy
        public Predicate<string> CopyFile { get; set; }

        // from and to directory
        public IFileDataAccessor SourceAccessor { get; set; }
        public IFileDataAccessor TargetAccessor { get; set; }

        // true (default): will delete files that are in the target directory, but not in source
        public bool DeleteAdditionalFiles { get; set; }

        public DirectorySynchronizer() {
            DeleteAdditionalFiles = true;
        }

        /*
         * Perform synchronization between the two directories.
         */
        public void Synchronize(string directory = "") {
            if (SourceAccessor == null || TargetAccessor == null) {
                throw new InvalidOperationException("Cannot decide source or target directory");
            }
            if (CopyFile == null) {
                throw new InvalidOperationException("Cannot decide what files to copy");
            }
#if DEBUG
            Console.WriteLine("synchronizing {2} from {0} to {1}", SourceAccessor, TargetAccessor, directory);
#endif
            // remove files that are in the target, but not in the source folder
            if (DeleteAdditionalFiles) {
                DeleteFilesFromTarget(directory);
            }
            // copy files from the source to the target folder
            if (SourceAccessor.DirectoryExists(directory)) {
                CopyFilesFromDirectories(directory);
            }
        }

        // delete all files that are in the target, but not source directory
        void DeleteFilesFromTarget(string baseDir) {
            // no directory? no files to delete
            if (!TargetAccessor.DirectoryExists(baseDir)) {
                return;
            }
            foreach (string filename in TargetAccessor.GetFiles(baseDir)) {
                SynchronizeFile(filename);
            }
            foreach (string subDirectory in TargetAccessor.GetDirectories(baseDir)) {
                DeleteFilesFromTarget(subDirectory);
            }
        }

        // copy all files in given directory and iterate all subdirectories
        void CopyFilesFromDirectories(string dir) {
            foreach (string filename in SourceAccessor.GetFiles(dir)) {
                SynchronizeFile(filename);
            }
            foreach (string directory in SourceAccessor.GetDirectories(dir)) {
                CopyFilesFromDirectories(directory);
            }
        }

        // copy given file from source to target directory if appropriate
        public void SynchronizeFile(string file) {
            if (SourceAccessor.FileExists(file)) {
                if (CopyFile(file)) {
#if DEBUG
                    Console.WriteLine("copying from {1} to {2}: {0}", file, SourceAccessor, TargetAccessor);
#endif
                    using (var sourceStream = SourceAccessor.GetFileContents(file)) {
                        DateTime writeTime = SourceAccessor.GetLastWriteTime(file);
                        int attributes = SourceAccessor.GetFileAttributes(file);
                        TargetAccessor.WriteFile(file, sourceStream, writeTime, attributes);
                    }
                    synchronizedFiles.Add(file);
                }
            } else if (DeleteAdditionalFiles && TargetAccessor.FileExists(file)) {
#if DEBUG
                Console.WriteLine("deleting {0} from {1}", file, TargetAccessor);
#endif
                TargetAccessor.DeleteFile(file);

                synchronizedFiles.Add(file);
            }
        }
    }
    
    public class DirectoryEnumerable : IEnumerable<string> {
        string directory;
        public DirectoryEnumerable(string dir) {
            directory = dir;
        }
        public IEnumerator<string> GetEnumerator() {
            return new DirectoryEnumerator(directory);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new DirectoryEnumerator(directory);
        }
    }
    public class DirectoryEnumerator : DelegatingEnumerator<string> {
        string directory;

        // the files in this enumerator's directory
        IEnumerator<string> files;

        // the directories in this enumerator's directory
        IEnumerator<string> directories;

        public DirectoryEnumerator(string dir) {
            directory = dir;
            Reset ();
        }
        
        protected override IEnumerator<string> NextEnumerator() {
            if (directories.MoveNext()) {
                // recursively iterate contained directories
                return new DirectoryEnumerator(directories.Current.ToString());
            }
            return files;
        }
        
        public override void Reset() {
            base.Reset ();
            files = new StringEnumerator(Directory.GetFiles(directory).GetEnumerator());
            directories = new StringEnumerator(Directory.GetDirectories(directory).GetEnumerator());
        }
    }
    public class StringEnumerator : IEnumerator<string> {
        private IEnumerator delegateTo;
        public StringEnumerator(IEnumerator enumerator) {
            delegateTo = enumerator;
        }
        public bool MoveNext() {
            return delegateTo.MoveNext();
        }
        public string Current {
            get {
                return delegateTo.Current.ToString();
            }
        }
        object System.Collections.IEnumerator.Current {
            get {
                return delegateTo.Current;
            }
        }
        public void Reset() {
            delegateTo.Reset();
        }
        public void Dispose() {
            delegateTo = null;
        }
    }
}
