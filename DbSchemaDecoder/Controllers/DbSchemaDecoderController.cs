using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DbSchemaDecoder.Controllers
{
    public class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public NextItemController NextItemController { get; set; }
        public DbTableDefinitionController DbTableDefinitionController { get; set; }
        //public BruteForceController BruteForceController { get; set; }
        public CaSchemaController CaSchemaController { get; set; }
        public TableEntriesController TableEntriesController { get; set; }
        public HeaderInformationController SelectedFileHeaderInformationController { get; set; }

        WindowState _windowState;

        public DbSchemaDecoderController(WindowState eventHub, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {
            _windowState = eventHub;

            TableEntriesController = new TableEntriesController(_windowState, dbTableItemSourceUpdater);
            NextItemController = new NextItemController(_windowState);
            //BruteForceController = new BruteForceController(_windowState);
            CaSchemaController = new CaSchemaController(_windowState);
            DbTableDefinitionController = new DbTableDefinitionController(_windowState);
            SelectedFileHeaderInformationController = new HeaderInformationController(_windowState);


            SchemaManager.Instance.CurrentGame = Common.GameTypeEnum.Warhammer2;
            string fileName = "converted_schema_wh2";

            var content = File.ReadAllText(@"C:\Users\ole_k\Desktop\Rusttest\hello_cargo\" + fileName + ".json");
            var root = JsonConvert.DeserializeObject<RootObject>(content);


           
            SchemaFile schemaFile = new SchemaFile();
            schemaFile.GameEnum = SchemaManager.Instance.CurrentGame;
            schemaFile.Version = 1;
            schemaFile.TableDefinitions = new Dictionary<string, List<DbTableDefinition>>();
            foreach (var ronTableDefinitions in root.VersionedFiles)
            {
                if (ronTableDefinitions.Key == "DepManager")
                    continue;

                if (ronTableDefinitions.Key == "Loc")
                    ronTableDefinitions.Label = SchemaManager.LocTableName;

                schemaFile.TableDefinitions.Add(ronTableDefinitions.Label, new List<DbTableDefinition>());
                foreach (var ronTable in ronTableDefinitions.Items)
                {
                    var newTableDefinition = new DbTableDefinition()
                    {
                        TableName = ronTableDefinitions.Label,
                        Version = ronTable.Version,
                        ColumnDefinitions = new List<DbColumnDefinition>()
                    };

                    foreach (var ronField in ronTable.Fields)
                    {
                        var newField = new DbColumnDefinition();
                        newField.Name = ronField.name;
                        newField.Type = GetTypeEnum(ronField.field_type);
                        newField.IsKey = ronField.is_key;
                        newField.MaxLength = ronField.max_length;
                        newField.IsFileName = ronField.is_filename;
                        newField.Description = ronField.description;
                        newField.FilenameRelativePath = ronField.filename_relative_path;
                        
                        if (ronField.is_reference != null)
                        {
                            var values = (ronField.is_reference as JArray).Values<string>().ToArray();
                            newField.TableReference = values[0] + "." + values[1];
                        }

                        newTableDefinition.ColumnDefinitions.Add(newField);
                    }

                    schemaFile.TableDefinitions[ronTableDefinitions.Label].Add(newTableDefinition);
                }
            }

            SchemaManager.Instance.UpdateCurrentTableDefinitions(schemaFile);
            //SchemaManager.Instance.UpdateCurrentTableDefinition("tableName", new List<DbTableDefinition>())
        }

        public DbTypesEnum GetTypeEnum(string value)
        {
            throw new NotImplementedException("Todo");
            return DbTypesEnum.uint32;
        }



        class RootObject
        {
            public int Version { get; set; }

            [JsonProperty("versioned_files")]
            public List<VersionedFile> VersionedFiles { get; set; }
        }

        [JsonConverter(typeof(VersionedFileConverter))]
        class VersionedFile
        {
            public string Key { get; set; }  // not sure if "DB" is a name or a type, so I just called it "Key"
            public string Label { get; set; }  // not sure what the "Table0" string represents, so I just called it "Label"
            public List<Item> Items { get; set; }
        }

        class Item
        {
            public int Version { get; set; }
            public List<Field> Fields { get; set; }
            public List<object> Localised { get; set; }
        }

        public class Field
        {
            public string name { get; set; }
            public string field_type { get; set; }
            public bool is_key { get; set; }
            public object default_value { get; set; }
            public int max_length { get; set; }
            public bool is_filename { get; set; }
            public string filename_relative_path { get; set; }
            public object is_reference { get; set; }
            public object lookup { get; set; }
            public string description { get; set; }
            public int ca_order { get; set; }

        }

        public class FieldTableReference
        { 
            public string TableName { get; set; }
            public string ColumnName { get; set; }
        }
    

        public class VersionedFileConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(VersionedFile);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                JProperty prop = obj.Properties().First();

                VersionedFile file = new VersionedFile
                {
                    Key = prop.Name,
                    Items = new List<Item>()
                };

                JArray array = (JArray)prop.Value;
                if (array.Count > 0)
                {
                    if (array[0].Type == JTokenType.String)
                    {
                        file.Label = (string)array[0];
                        file.Items = array[1].ToObject<List<Item>>(serializer);
                    }
                    else
                    {
                        file.Items = array.ToObject<List<Item>>(serializer);
                    }
                }

                return file;
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
