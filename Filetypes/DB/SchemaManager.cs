﻿using Common;
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
using System.Runtime.CompilerServices;
using Serilog;

namespace Filetypes.DB
{
    public class DbTableDefinition
    {
        public string TableName { get; set; }
        public int Version { get; set; }
        public List<DbColumnDefinition> ColumnDefinitions { get; set; } = new List<DbColumnDefinition>();
    }

    public class SchemaFile
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public GameTypeEnum GameEnum { get; set; }
        public int Version { get; set; } = 1;
        public Dictionary<string, List<DbTableDefinition>> TableDefinitions { get; set; } = new Dictionary<string, List<DbTableDefinition>>();
    }

    public class SchemaManager
    {
        ILogger _logger = Logging.Create<SchemaManager>();

        public static string LocTableName { get { return "LocTable"; } }

        Dictionary<GameTypeEnum, SchemaFile> _gameTableDefinitions = new Dictionary<GameTypeEnum, SchemaFile>();

        public GameTypeEnum CurrentGame { get; set; }

        public static SchemaManager Instance { get; private set; } = new SchemaManager();

        public bool IsCreated { get; private set; } = false;
        public SchemaManager()
        {
        }

        public void Create()
        {
            if (IsCreated)
                return;

            _logger.Information("Created");


            foreach (var game in Game.Games)
                Load(game.GameType);

            IsCreated = true;
        }

        public void UpdateCurrentTableDefinitions(SchemaFile schemaFile)
        {
            _gameTableDefinitions[CurrentGame] = schemaFile;
            Save();
        }

        public void UpdateCurrentTableDefinition(DbTableDefinition newTableDefinition)
        {
            if (_gameTableDefinitions[CurrentGame].TableDefinitions.ContainsKey(newTableDefinition.TableName))
            {
                var added = false;
                var defs = _gameTableDefinitions[CurrentGame].TableDefinitions[newTableDefinition.TableName];
                for (int i = 0; i < defs.Count; i++)
                {
                    if (defs[i].Version == newTableDefinition.Version)
                    {
                        defs[i].ColumnDefinitions = newTableDefinition.ColumnDefinitions;
                        added = true;
                        break;
                    }
                }

                if (added == false)
                    _gameTableDefinitions[CurrentGame].TableDefinitions[newTableDefinition.TableName].Add(newTableDefinition);
            }
            else
            {
                _gameTableDefinitions[CurrentGame].TableDefinitions.Add(newTableDefinition.TableName, new List<DbTableDefinition>());
                _gameTableDefinitions[CurrentGame].TableDefinitions[newTableDefinition.TableName].Add(newTableDefinition);
            }
            Save();
        }

        public Dictionary<string, List<DbTableDefinition>> GetTableDefinitions(GameTypeEnum gameType)
        {
            if (!_gameTableDefinitions.ContainsKey(gameType))
                return null;
            return _gameTableDefinitions[gameType].TableDefinitions;
        }

        public bool IsSupported(string tableName)
        {
            var definition = GetTableDefinitionsForTable(tableName);
            if (definition.Count != 0)
                return true;
            return false;
        }

        public List<DbTableDefinition> GetTableDefinitionsForTable( string tableName)
        {
            if (!_gameTableDefinitions.ContainsKey(CurrentGame))
                return new List<DbTableDefinition>();

            if (_gameTableDefinitions[CurrentGame].TableDefinitions.ContainsKey(tableName))
                return _gameTableDefinitions[CurrentGame].TableDefinitions[tableName];
            return new List<DbTableDefinition>();
        }

        public DbTableDefinition GetTableDefinitionsForTable(string tableName, int version)
        {
            var def = GetTableDefinitionsForTable(tableName).FirstOrDefault(x => x.Version == version);
            if (def != null)
                return def;
            return new  DbTableDefinition();
        }

        public bool Save()
        {
            _logger.Information("Trying to save file");
            try
            {
                if (!_gameTableDefinitions.ContainsKey(CurrentGame))
                    return false;
                string path = DirectoryHelper.SchemaDirectory + "\\" + Game.GetByEnum(CurrentGame).Id + "_schema.json";
                var content = JsonConvert.SerializeObject(_gameTableDefinitions[CurrentGame], Formatting.Indented);
                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception e)
            {
                _logger.Fatal(e.Message);
                throw e;
            }
        }

        void Load(GameTypeEnum game)
        {
            if (_gameTableDefinitions.ContainsKey(game))
                return;
            
            string path = DirectoryHelper.SchemaDirectory + "\\" + Game.GetByEnum(game).Id + "_schema.json";
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
