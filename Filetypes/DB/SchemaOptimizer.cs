using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Common;

namespace Filetypes {
    
    /*
     * Will go through all packs in a given directory, 
     * and determine the maximum version for all db files of any type.
     */
    public class SchemaOptimizer {

        // the directory to iterate pack files of
        public string PackDirectory { get; set; }

        // the filename to save the result in
        public string SchemaFilename { get; set; }

        public SchemaOptimizer () {
            PackDirectory = "";
            SchemaFilename = "schema_optimized.xml";
        }
        
        SortedList<string, int> maxVersion = new SortedList<string, int>();

        public void FilterExistingPacks() {
            if (Directory.Exists(PackDirectory)) {
                DateTime start = DateTime.Now;
                Console.WriteLine("Retrieving from {0}, storing to {1}", PackDirectory, SchemaFilename);
                
                maxVersion.Clear();

                // all pack files in game directory
                List<string> files = new List<string>(Directory.EnumerateFiles(PackDirectory, "*.pack"));
                // all files called "*patch*" for backed up earlier packs
                // files.AddRange(Directory.EnumerateFiles(PackDirectory, "*.patch*"));

                foreach (string path in files) {
                    try {
                        PackFile pack = new PackFileCodec().Open(path);
                        // only use CA packs
                        if (pack.Type != PackType.Release && pack.Type != PackType.Patch) {
                            Console.WriteLine("not handling {0}", path);
                            continue;
                        }
                        GetUsedTypes(pack);
                    } catch (Exception e) {
                        Console.WriteLine("Not able to include {0} into schema: {1}", path, e);
                    }
                }
                
                List<TypeVersionTuple> asTuples = new List<TypeVersionTuple>();
                foreach(string s in maxVersion.Keys) {
                    asTuples.Add(new TypeVersionTuple { Type = s, MaxVersion = maxVersion[s] });
                }
                using (var stream = File.Create(SchemaFilename)) {
                    XmlSerializer serializer = new XmlSerializer(asTuples.GetType());
                    serializer.Serialize(stream, asTuples);
                }

                DateTime end = DateTime.Now;
                Console.WriteLine("optimization took {0}", end.Subtract(start));
            }
        }

        private void GetUsedTypes(PackFile pack) {
            foreach (PackedFile packed in pack.Files) {
                if (packed.FullPath.StartsWith("db")) {
                    AddFromPacked(packed);
                }
            }
        }

        private void AddFromPacked(PackedFile packed) {
            if (packed.Size != 0) {
                string type = DBFile.Typename(packed.FullPath);
                DBFileHeader header = PackedFileDbCodec.readHeader(packed);
                int currentMaxVersion;
                if (!maxVersion.TryGetValue(type, out currentMaxVersion)) {
                    currentMaxVersion = header.Version;
                } else {
                    currentMaxVersion = Math.Max(header.Version, currentMaxVersion);
                }
                maxVersion[type] = currentMaxVersion;
            }
        }
        
        public static SortedList<string, int> ReadTypeVersions(string filename) {
            List<TypeVersionTuple> tuples = new List<TypeVersionTuple>();
            XmlSerializer ser = new XmlSerializer(tuples.GetType());
            using (var stream = File.OpenRead(filename)) {
                tuples = ser.Deserialize(stream) as List<TypeVersionTuple>;
            }
            SortedList<string, int> versions = new SortedList<string, int>();
            tuples.ForEach(e => versions.Add(e.Type, e.MaxVersion));
            return versions;
        }
    }    
}

