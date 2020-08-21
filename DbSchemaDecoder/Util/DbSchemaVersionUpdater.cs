using Filetypes;
using Filetypes.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{
    class DbSchemaVersionUpdater
    {
        class SchemaFile
        {
            public int Version { get; set; } = 1;
            public Dictionary<string, List<DbTableDefinition>> TableDefinitions { get; set; } = new Dictionary<string, List<DbTableDefinition>>();
        }

        public void Convert()
        {
            SchemaFile file = new SchemaFile();
            var allTableDefinitions = DBTypeMap.Instance.AllInfos;

            foreach (var oldTable in allTableDefinitions)
            {
                file.TableDefinitions.Add(oldTable.Key, new List<DbTableDefinition>());
                var currentNewDefinitionList = file.TableDefinitions[oldTable.Key];

                var tableName = oldTable.Key;
                var oldFieldCollections = oldTable.Value;
                foreach (var fieldCollection in oldFieldCollections)
                {
                    var newEntry = new DbTableDefinition()
                    {
                        TableName = fieldCollection.Name,
                        Version = fieldCollection.Version,
                        ColumnDefinitions = ConvertFieldInfos(fieldCollection.Fields)
                    };

                    currentNewDefinitionList.Add(newEntry);
                }
            }

            var strContent = JsonConvert.SerializeObject(file, Formatting.Indented);
            File.WriteAllText(@"C:\temp\DepricatedMasterSchema.json", strContent);
        }


        //public Dictionary<string, List<DbTableDefinition>> CreateGameSpesificDefinition(string game, filel)

        List<DbColumnDefinition> ConvertFieldInfos(List<FieldInfo> fieldInfos)
        {
            var output = new List<DbColumnDefinition>();
            foreach (var field in fieldInfos)
            {
                var newDbColumnDefinition = new DbColumnDefinition
                {
                    MetaData = new DbFieldMetaData()
                    {
                        Name = field.Name,
                        IsOptional = field.Optional,
                        FieldReference = EnsureEmptyIfNull(field.ReferencedField),
                        TableReference = EnsureEmptyIfNull(field.ReferencedTable),
                    },
                    Type = field.TypeEnum
                };
                output.Add(newDbColumnDefinition);
            }

            return output;
        }

        public string EnsureEmptyIfNull(string value)
        {
            if (value == null)
                return "";
            return value;
        }
    }
}
