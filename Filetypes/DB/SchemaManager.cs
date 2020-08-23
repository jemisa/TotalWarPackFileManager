using Common;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Filetypes.ByteParsing;

namespace Filetypes.DB
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

        public GameTypeEnum CurrentGame { get; set; }

        public static SchemaManager Instance { get; private set; } = new SchemaManager();

        public bool IsCreated { get; private set; } = false;
        public SchemaManager()
        {
            BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public void Create()
        {
            if (IsCreated)
                return;
            
            // Depricated fool
            string path = BasePath + "\\Files\\" + "DepricatedMasterSchema.json";
            var content = LoadSchemaFile(path);
            if (content != null)
                _gameTableDefinitions.Add(GameTypeEnum.Unknown, content);

            // Games
            foreach (var game in Game.Games)
            {
                Load(game.GameType);
            }

            IsCreated = true;
        }

        public Dictionary<string, List<DbTableDefinition>> GetTableDefinitions(GameTypeEnum gameType)
        {
            if (!_gameTableDefinitions.ContainsKey(gameType))
                return null;
            return _gameTableDefinitions[gameType].TableDefinitions;
        }

        public bool IsSupported(string tableName)
        {
            return true;
        }

        public List<DbTableDefinition> GetTableDefinitionsForTable(GameTypeEnum gameType, string tableName)
        {
            if (!_gameTableDefinitions.ContainsKey(gameType))
                return new List<DbTableDefinition>();

            if (_gameTableDefinitions[gameType].TableDefinitions.ContainsKey(tableName))
                return _gameTableDefinitions[gameType].TableDefinitions[tableName];
            return new List<DbTableDefinition>();
        }

        public DbTableDefinition GetTableDefinitionsForTable(string tableName, int version)
        {
            throw new NotImplementedException();
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
            if (content != null)
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
}
