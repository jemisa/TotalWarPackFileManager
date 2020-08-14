using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DbSchemaDecoder.Controllers
{
    public class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objecType)
        {
            return (objecType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objecType, object existingValue,
            JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<T>>();
            }
            return new List<T> { token.ToObject<T>() };
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

    class CaSchemaEntry : ICloneable
    {
        public int index { get; set; }
        public string field_uuid { get; set; }
        public string primary_key { get; set; }
        public string name { get; set; }
        public string field_type { get; set; }
        public string required { get; set; }
        public string max_length { get; set; }
        public string column_source_column { get; set; }
        public string column_source_table { get; set; }
        public string encyclopaedia_export { get; set; }
        public string requires_startpos_reprocess { get; set; }
        public string requires_campaign_map_reprocess { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    class CaSchema
    {
        public List<CaSchemaEntry> Entries { get; set; } = new List<CaSchemaEntry>();
        public string Error { get; set; }
        public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
    }


    class CaSchemaFileParser
    {
        //
        public class XmlHeaderInformation
        {
            public string @version { get; set; }
            public string @encoding { get; set; }
            public string @standalone { get; set; }

        }

        public class DOCTYPE
        {
            public string @name { get; set; }

        }

        public class TableDefinition
        {
            public string edit_uuid { get; set; }
            public string include_all_records_in_retail { get; set; }
            [JsonConverter(typeof(SingleOrArrayConverter<Field>))]
            public List<Field> field { get; set; }

        }

        public class CaSchemaTable
        {
            public XmlHeaderInformation xml { get; set; }
            //public DOCTYPE DOCTYPE { get; set; }
            public TableDefinition root { get; set; }

    }
    //


        public class Field
        {
            
            public string field_uuid { get; set; }
            public string primary_key { get; set; }
            public string name { get; set; }
            public string field_type { get; set; }
            public string required { get; set; }
            public string max_length { get; set; }

            [JsonConverter(typeof(SingleOrArrayConverter<string>))]
            public List<string> column_source_column { get; set; } = new List<string>();
            public string column_source_table { get; set; }
            public string encyclopaedia_export { get; set; }
            public string requires_startpos_reprocess { get; set; }
            public string requires_campaign_map_reprocess { get; set; }

        }



        readonly string _caDbDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Total War WARHAMMER II\assembly_kit\raw_data\db";
        
        public CaSchemaFileParser()
        { 
        
        }

        // A list of fields removed from the game by ca, they should not be added 
        readonly string[] _fieldsRemovedFromTheGameByCa = new string[] 
        { 
            "game_expansion_key",
            "localised_text",
            "localised_name",
            "localised_tooltip",
            "description",
            "objectives_team_1",
            "objectives_team_2",
            "short_description_text",
            "historical_description_text",
            "strengths_weaknesses_text",
            "onscreen",
            "onscreen_text",
            "onscreen_name",
            "onscreen_description",
            "on_screen_name",
            "on_screen_description",
            "on_screen_target"
        };


        public CaSchema Load(string tableType)
        {
            CaSchema output = new CaSchema();

            var filename = tableType.Replace("_tables", "");
            string path = _caDbDirectory + "\\TWaD_" + filename + ".xml";
            if (!File.Exists(path))
            {
                output.Error = $"Unable to find file '{path}'";
                return output;
            }

            CaSchemaTable table;
            try
            {
                var xmlContent = File.ReadAllText(path);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                string jsonStr = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                table = JsonConvert.DeserializeObject<CaSchemaTable>(jsonStr);
            }
            catch (Exception e)
            {
                output.Error = $"Error parsing: {e.Message}";
                return output;
            }

            var filteredTables = table.root.field.Where(x => !_fieldsRemovedFromTheGameByCa.Contains(x.name));
            // Create the table
            var index = 1;
            foreach (var item in filteredTables)
            {
                CaSchemaEntry entry = new CaSchemaEntry();

                entry.index = index++;
                entry.field_uuid = item.field_uuid;
                entry.primary_key = item.primary_key;
                entry.name = item.name;
                entry.field_type = item.field_type;
                entry.required = item.required;
                entry.max_length = item.max_length;
                entry.column_source_column = item.column_source_column.FirstOrDefault();
                entry.column_source_table = item.column_source_table;
                entry.encyclopaedia_export = item.encyclopaedia_export;
                entry.requires_startpos_reprocess = item.requires_startpos_reprocess;
                entry.requires_campaign_map_reprocess = item.requires_campaign_map_reprocess;

                output.Entries.Add(entry);
            }

            return output;
        }
    }
}
