using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Common;
using Filetypes.DB;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace Filetypes
{

    public class DbTableDefinition
    {
        public string TableName { get; set; }
        public int Version { get; set; }
        public List<DbColumnDefinition> ColumnDefinitions { get; set; } = new List<DbColumnDefinition>();
    }

    class SchemaFile
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GameTypeEnum GameEnum { get; set; }
        public int Version { get; set; } = 1;
        public Dictionary<string, List<DbTableDefinition>> TableDefinitions { get; set; } = new Dictionary<string, List<DbTableDefinition>>();
    }

    public class SchemaManager
    {
        public string BasePath { get; set; }

        Dictionary<GameTypeEnum, SchemaFile> _gameTableDefinitions = new Dictionary<GameTypeEnum, SchemaFile>();

        public SchemaManager()
        {
            BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void Create()
        {
            // Depricated fool
            string path = BasePath + "\\Files\\" + "DepricatedMasterSchema.json";
            var content = LoadSchemaFile(path);
            if(content != null)
                _gameTableDefinitions.Add(GameTypeEnum.Unknown, content);

            // Games
            foreach (var game in Game.Games)
            {
                Load( game.GameType);
            }
        }

        public Dictionary<string, List<DbTableDefinition>> GetTableDefinitions(GameTypeEnum gameType)
        {
            if (!_gameTableDefinitions.ContainsKey(gameType))
                return null;
            return _gameTableDefinitions[gameType].TableDefinitions;
        }

        public List<DbTableDefinition> GetTableDefinitionsForTable(GameTypeEnum gameType, string tableName)
        {
            if (!_gameTableDefinitions.ContainsKey(gameType))
                return new List<DbTableDefinition>();

            if (_gameTableDefinitions[gameType].TableDefinitions.ContainsKey(tableName))
                return _gameTableDefinitions[gameType].TableDefinitions[tableName];
            return new List<DbTableDefinition>();
        }

        public Dictionary<string, List<DbTableDefinition>> GetDepricated()
        {
            return GetTableDefinitions(GameTypeEnum.Unknown);
        }

        public bool Save(GameTypeEnum game)
        {
            if (!_gameTableDefinitions.ContainsKey(game))
                return false;
            string path = BasePath + "\\Files\\" + Game.GetByEnum(game).Id + "_schema.json";
            var content = JsonConvert.SerializeObject(_gameTableDefinitions[game]);
            File.WriteAllText(path, content);
            return true;
        }

        void Load(GameTypeEnum game)
        {
            if (_gameTableDefinitions.ContainsKey(game))
                return;
            string path = BasePath + "\\Files\\" + Game.GetByEnum(game).Id + "_schema.json";
            var content = LoadSchemaFile(path);
            if(content != null)
                _gameTableDefinitions.Add(game, content);
     
        }

        SchemaFile LoadSchemaFile(string path)
        {
            if (!File.Exists(path))
                return null;

            var content = File.ReadAllText(path);
            var schema = JsonConvert.DeserializeObject<SchemaFile>(content);
            return schema;
        }
    }

    public class DBTypeMap
    {

        Dictionary<string, List<TypeInfo>> _typeInfos = new Dictionary<string, List<TypeInfo>>();
        public Dictionary<string, List<TypeInfo>> AllInfos { get { return _typeInfos; } }

        public static DBTypeMap Instance { get; private set; } = new DBTypeMap();
        private DBTypeMap()
        {
            // prevent instantiation
        }

        public bool Initialized { get { return _typeInfos.Count != 0; } }

        public static readonly string[] SCHEMA_FILENAMES = { "schema_user.json", "master_schema.json" };

        public List<TypeInfo> GetAllInfos(string tableName)
        {
            if (_typeInfos.ContainsKey(tableName))
                return _typeInfos[tableName];
            return new List<TypeInfo>();
        }

        public List<TypeInfo> GetVersionedInfos(string key, int version)
        {
            var result = GetAllInfos(key);
            result.Sort(new BestVersionComparer { TargetVersion = version });
            return result;
        }

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
            var jsonStr = JsonConvert.SerializeObject(_typeInfos, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filename, jsonStr);
        }

        public string GetUserFilename(string suffix)
        {
            return string.Format(string.Format("schema_{0}.xml", suffix));
        }

        public List<string> DBFileTypes
        {
            get
            {
                var keys = _typeInfos.Select(x => x.Key).ToList();
                keys.Sort();
                return keys;
            }
        }

        public int MaxVersion(string type)
        {
            if (_typeInfos.ContainsKey(type))
            {
                var entries = _typeInfos[type];
                return entries.Max(x => x.Version);
            }
            return 0;
        }

        public bool IsSupported(string type)
        {
            return _typeInfos.ContainsKey(type);
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
