using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using Filetypes.Codecs;

namespace Filetypes {
    /*
     * A class caching all data for a given db column; used to handle fields referencing data in
     * columns of other tables, possibly other files.
     * The map will always load data from the game packs (because references to the vanilla entries
     * need always be possible), but can be set to also read from an additional pack file
     * (namely, the mod pack the user is working on).
     */
    public class DBReferenceMap {
        public static readonly DBReferenceMap Instance = new DBReferenceMap();
        Dictionary<string, SortedSet<string>> valueCache = new Dictionary<string, SortedSet<string>>();
        Dictionary<string, SortedSet<string>> gamePackCache = new Dictionary<string, SortedSet<string>>();
        Dictionary<string, List<PackedFile>> typeToPackedCache = new Dictionary<string, List<PackedFile>>();
        PackFile lastPack = null;

        private DBReferenceMap() {
        }
  
        /*
         * The current pack to load data from besides the vanilla data.
         */
        public PackFile CurrentPack {
            get { return lastPack; }
            set {
                if ((value != null && lastPack != null) &&
                    (value.Filepath != lastPack.Filepath)) {
                    // clear cache when using another pack file
                    valueCache.Clear();
                    typeToPackedCache.Clear();
                }
                lastPack = value;
            }
        }
        /*
         * The game to load the basic reference data for.
         */
        List<string> gamePacks = new List<string>();
        public List<string> GamePacks {
            get { return gamePacks; }
            set {
                gamePacks = value != null ? value : new List<string>();
                gamePackCache.Clear();
            }
        }

        public SortedSet<string> this[string key] {
            get {
                return valueCache[key];
            }
        }
        /*
         * Retrieve data for the given field in the given table, using the given packed files.
         */
        SortedSet<string> CollectValues(string tableName, string fieldName, IEnumerable<PackedFile> packedFiles) {
            SortedSet<string> result = null;
#if DEBUG
            Console.WriteLine("Looking for {0}:{1} in {2}", tableName, fieldName, packedFiles);
#endif
            // enable load from multiple files
            List<string> loadedFrom = new List<string>();
            foreach (PackedFile packed in packedFiles) {
                string currentTable = DBFile.Typename(packed.FullPath);
                if (!packed.FullPath.StartsWith("db")) {
                    continue;
                }
                if (currentTable.Equals(tableName)) {
                    // load from several files, but not with the same name
                    string fileName = Path.GetFileName(packed.FullPath);
                    if (loadedFrom.Contains(fileName))
                    {
                        continue;
                    }
                    if (result == null) {
                        result = new SortedSet<string>();
                    }
#if DEBUG
                    Console.WriteLine("Found {0}:{1} in {2}", tableName, fieldName, packedFiles);
#endif
                    try {
                        FillFromPacked(result, packed, fieldName);
                    } catch {
                        return null;
                    }
                    loadedFrom.Add(fileName);
                // } else if (found) {
                    // once we're past the files with the correct type, stop searching
                    // break;
                } else {
                    // we didn't find the right table type, but cache the PackedFile we created along the way
                    List<PackedFile> cacheFiles;
                    if (!typeToPackedCache.TryGetValue(currentTable, out cacheFiles)) {
                        cacheFiles = new List<PackedFile>();
                        typeToPackedCache.Add(currentTable, cacheFiles);
                    }
                    cacheFiles.Add(packed);
                } 
            }
            return result;
        }
        /*
         * Fills the given string collection with data from the field in the given packed file.
         */
        public static void FillFromPacked(SortedSet<string> result, PackedFile packed, string fieldName) {
            DBFile dbFile = PackedFileDbCodec.Decode(packed);
            foreach (DBRow entry in dbFile.Entries) {
                string toAdd = entry[fieldName].Value;
                if (toAdd != null) {
                    result.Add(toAdd);
                }
            }
        }
        /*
         * Retrieve the values for the given reference table/field, with the parameter key
         * encoded as "table_name.field_name".
         */
        public SortedSet<string> ResolveReference(string key) {
            if (key.Length == 0) {
                return null;
            }
#if DEBUG
            Console.WriteLine("resolving reference {0}", key);
#endif
            string[] split = key.Split('.');
            string tableName = split[0];
            string fieldName = split[1];

            List<string> result = new List<string>();
            SortedSet<string> fromPack = new SortedSet<string>();
            if (!valueCache.TryGetValue(key, out fromPack)) {
                fromPack = CollectValues(tableName, fieldName, CurrentPack);
                valueCache.Add(key, fromPack);
            }
            if (fromPack != null) {
                result.AddRange(fromPack);
            }

            SortedSet<string> fromGame;
            if (!gamePackCache.TryGetValue(key, out fromGame)) {
                IEnumerable<PackedFile> packedFiles;
                if (typeToPackedCache.ContainsKey(tableName)) {
                    packedFiles = typeToPackedCache[tableName];
                } else {
                    packedFiles = new MultiPackEnumerable(gamePacks);
                }
                fromGame = CollectValues(tableName, fieldName, packedFiles);
                if (fromGame != null) {
                    gamePackCache.Add(key, fromGame);
                }
            }
            if (fromGame != null) {
                result.AddRange(fromGame);
            }
            
            SortedSet<string> resultSet = new SortedSet<string>(result);
            return resultSet;
        }
    }
}
