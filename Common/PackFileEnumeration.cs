using System;
using System.IO;
using System.Collections.Generic;

namespace Common {
    #region Single Pack File Iteration
    /*
     * Enumerates the packed files of a single pack.
     */
    public class PackedFileEnumerable : IEnumerable<PackedFile> {
        private string filepath;
        public PackedFileEnumerable(string path) {
            filepath = path;
        }
        public IEnumerator<PackedFile> GetEnumerator() {
            return new PackFileEnumerator(filepath);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new PackFileEnumerator(filepath);
        }
        public override string ToString() {
            return filepath;
        }
    }
    /*
     * Enumerates packed files contained in a pack without reading in the 
     * full file list from the pack so iteration can be interrupted once a file is found.
     */
    public class PackFileEnumerator : IEnumerator<PackedFile>, IDisposable {
        BinaryReader reader;
        PFHeader header;
        long offset;
        uint currentFileIndex;
        string filepath;
        PackedFile currentFile = null;
        long startPosition;
        public PFHeader Header {
            get {
                return header;
            }
        }
        /*
         * Enumerator for pack at given file path.
         */
        public PackFileEnumerator(string path) {
            filepath = path;
            reader = new BinaryReader(File.OpenRead(path));
            header = PackFileCodec.ReadHeader(reader);
            startPosition = reader.BaseStream.Position;
            Reset();
        }
        /*
         * Re-start enumeration.
         */
        public void Reset() {
            currentFileIndex = 0;
            reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);
            offset = header.DataStart;
            currentFile = null;
        }
        /*
         * Find next file entry and return true if there is one, otherwise false.
         */
        public bool MoveNext() {
            currentFileIndex++;
            if (currentFileIndex > header.FileCount) {
                return false;
            } 
            uint size = reader.ReadUInt32();
            //FIXME this is wrong, different PFH versions have different length additionalInfo
            //TODO this is mostly duplicated with PackFileCodec.  Reducing code duplication would be wise.
            if (Header.HasAdditionalInfo) {
                header.AdditionalInfo = reader.ReadInt64();
            }
            if(Header.PackIdentifier == "PFH5")
                reader.ReadByte();
            try {
                string packedFileName = IOFunctions.ReadZeroTerminatedAscii(reader);
                // this is easier because we can use the Path methods
                // under both Windows and Unix
                packedFileName = packedFileName.Replace('\\', Path.DirectorySeparatorChar);

                currentFile = new PackedFile(filepath, packedFileName, offset, size);
                offset += size;

                return true;
            } catch (Exception ex) {
                Console.WriteLine("Failed enumeration of {2}/{3} file in {0}: {1}", 
                    Path.GetFileName(filepath), ex, currentFileIndex, header.FileCount);
                Console.WriteLine("Current position in file: {0}; last succesful file: {1}", 
                    reader.BaseStream.Position, Current.FullPath);
            }
            return false;
        }
        /*
         * Dispose of the reader which is kept open throughout enumeration to release its resources.
         */
        public void Dispose() {
            reader.Dispose();
        }
        /*
         * Retrieve current pack file if any.
         */
        public PackedFile Current {
            get {
                return currentFile;
            }
        }
        object System.Collections.IEnumerator.Current {
            get {
                return Current;
            }
        }
    }
    #endregion

    #region Multiple Pack File Enumeration
    /*
     * Enumerates the packed files across several packs.
     */
    public class MultiPackEnumerable : IEnumerable<PackedFile> {
        IEnumerable<string> paths;
        public MultiPackEnumerable(IEnumerable<string> files) {
            paths = files;
        }
        public IEnumerator<PackedFile> GetEnumerator() {
            return new MultiPackEnumerator(paths);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return new MultiPackEnumerator(paths);
        }
        public override string ToString() {
            return string.Join(new string(Path.PathSeparator, 1), paths);
        }
    }
    /*
     * Enumerate several pack's contained files by enumerating every one of them.
     */
    public class MultiPackEnumerator : DelegatingEnumerator<PackedFile> {
        // the paths of the pack files to enumerate
        IEnumerator<string> paths;
        /*
         * Create enumerator for given pack files.
         */
        public MultiPackEnumerator(IEnumerable<string> files) {
            paths = files.GetEnumerator();
        }
        /*
         * Restart enumeration.
         */
        public override void Reset() {
            base.Reset();
            paths.Reset();
        }
        /*
         * Create next pack file enumerable if there are paths left.
         */
        protected override IEnumerator<PackedFile> NextEnumerator() {
            IEnumerator<PackedFile> result = null;
            if (paths.MoveNext()) {
                result = new PackFileEnumerator(paths.Current);
            }
            return result;
        }
        /*
         * Dispose of all contained enumerators.
         */
        public override void Dispose() {
            base.Dispose();
            paths.Dispose();
        }
    }
    #endregion
}

