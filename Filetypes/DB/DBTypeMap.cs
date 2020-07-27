using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;

namespace Filetypes
{
    /*
     * Class mapping db type names to type infos able to decode them.
     * Contains methods querying type infos for several situations.
     * A type info can be determined by type name and version number (ETW) or
     * by a GUID, type name and version number (NTW and later).
     * Not all DB files contain all information relevant to determining the exact info
     * they are encoded as, therefore often several type infos may be applicable for a given file
     * without reading the whole table.
     */
    public class DBTypeMap
    {
        public static readonly string MASTER_SCHEMA_FILE_NAME = "master_schema.json";
        public static readonly string SCHEMA_USER_FILE_NAME = "schema_user.json";

        Dictionary<string, List<TypeInfo>> _typeInfos = new Dictionary<string, List<TypeInfo>>();
        //List<TypeInfo> typeInfos = new List<TypeInfo>();
        public Dictionary<string, List<TypeInfo>> AllInfos
        {
            get
            {
                return _typeInfos;
            }
        }

        /*
         * Singleton access.
         */
        public static DBTypeMap Instance { get; private set; } = new DBTypeMap();
        private DBTypeMap()
        {
            // prevent instantiation
        }

        /*
         * Query if any schema has been loaded.
         */
        public bool Initialized
        {
            get
            {
                return _typeInfos.Count != 0;
            }
        }

        /*
         * A list of schema files that may be present to contain schema data.
         * The user file is intended to store the data for a user after he has edited a field name or suchlike
         * so he is able to send it in for us to integrate; but since now schema files are split up per game,
         * that is not really viable anymore because it would only work for the game he currently saved as.
         */
        public static readonly string[] SCHEMA_FILENAMES = {
            SCHEMA_USER_FILE_NAME, MASTER_SCHEMA_FILE_NAME
        };

        /*
         * Retrieve all infos currently loaded for the given table,
         * either in the table/version format or from the GUID list.
         */
        public List<TypeInfo> GetAllInfos(string tableName)
        {
            if (_typeInfos.ContainsKey(tableName))
                return _typeInfos[tableName];
            return new List<TypeInfo>();
        }
        /*
         * Get all infos matching the given table and version.
         * There may be more than one because sometimes, there are several GUIDs with
         * the same type/version but different structures.
         */
        public List<TypeInfo> GetVersionedInfos(string key, int version)
        {
            var result = GetAllInfos(key);
            result.Sort(new BestVersionComparer { TargetVersion = version });
#if DEBUG
            Console.WriteLine("Returning {0} infos for {1}/{2}", result.Count, key, version);
#endif
            return result;
        }

        #region Initialization / IO
        /*
         * Read schema from given directory, in the order of the SCHEMA_FILENAMES.
         */
        public void InitializeTypeMap(string basePath)
        {
            if (Initialized)
                return;

            foreach (string file in SCHEMA_FILENAMES)
            {
                string xmlFile = Path.Combine(basePath, file);
                if (File.Exists(xmlFile))
                {
                    initializeFromFile(xmlFile);
                    break;
                }
            }
        }

        public void initializeFromFile(string filename)
        {
            var content = File.ReadAllText(filename);
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<TypeInfo>>>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
            _typeInfos = data;
        }




        /*
         * Stores the whole schema to a file at the given directory with the given suffix.
         */
        public void SaveToFile(string path, string suffix)
        {
            string filename = Path.Combine(path, GetUserFilename(suffix));
            string backupName = filename + ".bak";
            if (File.Exists(filename))
                File.Copy(filename, backupName);

            SaveToFile(filename);

            if (File.Exists(backupName))
                File.Delete(backupName);

        }

        public void SaveToFile(string filename)
        {
#if DEBUG
            Console.WriteLine("saving schema file {0}", filename);
#endif
            var jsonStr = JsonConvert.SerializeObject(_typeInfos, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filename, jsonStr);
        }
        #endregion


        #region Utilities
        public string GetUserFilename(string suffix)
        {
            return string.Format(string.Format("schema_{0}.xml", suffix));
        }
        #endregion

        #region Supported Type/Version Queries
        /*
         * Retrieve all supported Type Names.
         */
        public List<string> DBFileTypes
        {
            get
            {
                var keys = _typeInfos.Select(x => x.Key).ToList();
                keys.Sort();
                return keys;
            }
        }

        /*
         * Retrieve the highest version for the given type.
         */
        public int MaxVersion(string type)
        {
            if (_typeInfos.ContainsKey(type))
            {
                var entries = _typeInfos[type];
                return entries.Max(x => x.Version);
            }
            return 0;
        }
        /*
         * Query if the given type is supported at all.
         */
        public bool IsSupported(string type)
        {
            return _typeInfos.ContainsKey(type);
        }
        #endregion

        /*
         * Note:
         * The names of the TypeInfos iterated here cannot be changed using the
         * enumeration; the FieldInfo lists and contained FieldInfos can.
         */
        public Dictionary<string, List<TypeInfo>>.Enumerator GetEnumerator()
        {
            return _typeInfos.GetEnumerator();
        }
    }

    /*
     * Class defining a db type by GUID. They do still carry their type name
     * and a version number along; the name/version tuple may not be unique though.
     */
    public class GuidTypeInfo : IComparable<GuidTypeInfo>
    {
        public GuidTypeInfo(string guid) : this(guid, "", 0) { }
        public GuidTypeInfo(string guid, string type, int version)
        {
            Guid = guid;
            TypeName = type;
            Version = version;
        }
        public string Guid { get; set; }
        public string TypeName { get; set; }
        public int Version { get; set; }
        /*
         * Comparable (mostly to sort the master schema for easier version control).
         */
        public int CompareTo(GuidTypeInfo other)
        {
            int result = TypeName.CompareTo(other.TypeName);
            if (result == 0)
            {
                result = Version - other.Version;
            }
            if (result == 0)
            {
                result = Guid.CompareTo(other.Guid);
            }
            return result;
        }
        #region Framework overrides
        public override bool Equals(object obj)
        {
            bool result = obj is GuidTypeInfo;
            if (result)
            {
                if (string.IsNullOrEmpty(Guid))
                {
                    result = (obj as GuidTypeInfo).TypeName.Equals(TypeName);
                    result &= (obj as GuidTypeInfo).Version.Equals(Version);
                }
                else
                {
                    result = (obj as GuidTypeInfo).Guid.Equals(Guid);
                }
            }
            return result;
        }
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("{1}/{2} # {0}", Guid, TypeName, Version);
        }
        #endregion
    }

    /*
     * Comparer for two guid info instances.
     */
    class GuidInfoComparer : Comparer<GuidTypeInfo>
    {
        public override int Compare(GuidTypeInfo x, GuidTypeInfo y)
        {
            int result = x.TypeName.CompareTo(y.TypeName);
            if (result == 0)
            {
                result = y.Version - x.Version;
            }
            return result;
        }
    }

    /*
     * Compares two versioned infos to best match a version being looked for.
     */
    class BestVersionComparer : IComparer<TypeInfo>
    {
        public int TargetVersion { get; set; }
        public int Compare(TypeInfo info1, TypeInfo info2)
        {
            int difference1 = info1.Version - TargetVersion;
            int difference2 = info2.Version - TargetVersion;
            return difference2 - difference1;
        }
    }
}
