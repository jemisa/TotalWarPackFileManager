using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using NameMapping = System.Tuple<string, string>;

namespace Filetypes {
    /*
     * Provides mapping between column names of schema.xml (from the community mod tools)
     * to those used by CA.
     */
    public class FieldMappingManager {
        #region Singleton
        private static FieldMappingManager instance;
        public static FieldMappingManager Instance {
            get {
                if (instance == null) {
                    instance = new FieldMappingManager();
                }
                return instance;
            }
        }
        #endregion

        // load name correspondencies from file
        public const string DEFAULT_FILE_NAME = "correspondencies.xml";
        public const string DEFAULT_PARTIAL_FILE_NAME = "partial_correspondencies.xml";
        private FieldMappingManager() {
            LoadFromFile(DEFAULT_FILE_NAME, mappedTables);
            LoadFromFile(DEFAULT_PARTIAL_FILE_NAME, mappedTables);
        }
        
        #region Field Mappings
        Dictionary<string, MappedTable> mappedTables = new Dictionary<string, MappedTable>();
        public Dictionary<string, MappedTable> MappedTables {
            get { return mappedTables; }
        }

        // retrieve ca xml tag for given db schema table/field combination
        public string GetXmlFieldName(string tableName, string field) {
            string result = null;
            if (mappedTables.ContainsKey(tableName)) {
                Dictionary<string, string> lookup = mappedTables[tableName].Mappings;
                result = lookup.ContainsKey(field) ? lookup[field] : null;
            }
            return result;
        }
//        public List<NameMapping> GetMappedFieldsForTable(string table) {
//            List<NameMapping> fields = new List<NameMapping>();
//            if (mappedTables.ContainsKey(table)) {
//                Dictionary<string, string> mappings = mappedTables[table].Mappings;
//                foreach(string key in mappings.Keys) {
//                    fields.Add(new NameMapping(key, mappings[key]));
//                }
//            }
//            return fields;
//        }
        public bool IsConstantPackField(string tableName, string field) {
            bool result = false;
            if (mappedTables.ContainsKey(tableName)) {
                MappedTable table = mappedTables[tableName];
                result = table.ConstantPackValues.ContainsKey(field);
            }
            return result;
        }
        #endregion
        
        #region Constant Values
        List<NameMapping> ConstantXmlValues(string tableName) {
            List<NameMapping> result = new List<NameMapping>();
            if (mappedTables.ContainsKey(tableName)) {
                MappedTable table = mappedTables[tableName];
                foreach(string key in table.ConstantXmlValues.Keys) {
                    result.Add(new NameMapping(key, table.ConstantXmlValues[key]));
                }
            }
            return result;
        }
        #endregion

        #region Guids
        Dictionary<string, string> tableGuidMap = new Dictionary<string, string>();
        public Dictionary<string, string> TableGuidMap {
            get { return tableGuidMap; }
        }
        
        public string GetGuidForTable(string tableName) {
            string result = null;
            tableGuidMap.TryGetValue(tableName, out result);
            return result;
        }
        #endregion
        
        public void Clear() {
            mappedTables.Clear();
            tableGuidMap.Clear();
        }

        #region Load/Save
        static readonly string ROOT_TAG = "correspondencies";
        //static readonly string TABLE_TAG = "table";
        static readonly string FIELD_TAG = "field";
        static readonly string GUID_ATTRIBUTE = "guid";
        static readonly string NAME_ATTRIBUTE = "name";
        static readonly string PACK_ATTRIBUTE = "pack";
        static readonly string XML_ATTRIBUTE = "xml";
        static readonly string CONSTANT_ATTRIBUTE = "constant";
        static readonly string UNMAPPED_PACK_FIELDS = "unmappedPackedFields";
        static readonly string UNMAPPED_XML_FIELDS = "unmappedXmlFields";  

        public static void LoadFromFile(string filename, Dictionary<string, MappedTable> tables) {
            try {
                XmlDocument xmlFile = new XmlDocument();
                xmlFile.Load(filename);
                char[] separator = new char[] { ',' };
                foreach(XmlNode tableNode in xmlFile.ChildNodes[0].ChildNodes) {
                    string tableName = tableNode.Attributes[NAME_ATTRIBUTE].Value;
                    MappedInfoTable table = new MappedInfoTable(tableName);
                    table.Guid = tableNode.Attributes[GUID_ATTRIBUTE].Value;
                    tables.Add(tableName, table);
                    foreach(XmlNode fieldNode in tableNode.ChildNodes) {
                        if (fieldNode.Name.Equals(FIELD_TAG)) {
                            string packName = FromAttribute(fieldNode, PACK_ATTRIBUTE);
                            string xmlName = FromAttribute(fieldNode, XML_ATTRIBUTE);
                            string constantValue = FromAttribute(fieldNode, CONSTANT_ATTRIBUTE);
                            if (packName != null && xmlName != null) {
                                // mapping
                                table.AddMapping(packName, xmlName);
                            } else if (constantValue != null) {
                                // constant value... for pack or xml field?
                                if (packName != null) {
                                    table.AddConstantPackValue(packName, constantValue);
                                } else if (xmlName != null) {
                                    table.AddConstantXmlValue(xmlName, constantValue);
                                } else throw new InvalidDataException();
                            } else {
                                throw new InvalidDataException();
                            }
                        } else if (fieldNode.Name.Equals(UNMAPPED_PACK_FIELDS)) {
                            string[] list = fieldNode.InnerText.Split(separator);
                            foreach (string field in list) {
                                table.PackDataFields.Add(field);
                            }
                        } else if (fieldNode.Name.Equals(UNMAPPED_XML_FIELDS)) {
                            string[] list = fieldNode.InnerText.Split(separator);
                            foreach (string field in list) {
                                table.XmlDataFields.Add(field);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("failed to load {0}: {1}", filename, ex.Message);
            }
        }
        static string FromAttribute(XmlNode node, string attribute) {
            return (node.Attributes[attribute] != null) ? node.Attributes[attribute].Value : null;
        }
        public static Dictionary<string, List<NameMapping>> LoadFromFile(string filename) {
            Dictionary<string, List<NameMapping>> tableToMapping = new Dictionary<string,List<NameMapping>>();
            try {
                XmlDocument xmlFile = new XmlDocument();
                xmlFile.Load(filename);
                foreach (XmlNode tableNode in xmlFile.ChildNodes[0].ChildNodes) {
                    List<NameMapping> mappings = new List<NameMapping>();
                    string tableName = tableNode.Attributes[NAME_ATTRIBUTE].Value;
                    foreach (XmlNode fieldNode in tableNode.ChildNodes) {
                        if (fieldNode.Name.Equals(FIELD_TAG)) {
                            string packName = fieldNode.Attributes[PACK_ATTRIBUTE].Value;
                            string xmlName = fieldNode.Attributes[XML_ATTRIBUTE].Value;
                            mappings.Add(new NameMapping(packName, xmlName));
                        }
                    }
                    tableToMapping.Add(tableName, mappings);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine("failed to load {0}: {1}", filename, ex.Message);
            }
            return tableToMapping;
        }
        
        // store to file
        public void Save() {
            StreamWriter fullyMappedWriter = File.CreateText(DEFAULT_FILE_NAME);
            StreamWriter partiallyMappedWriter = File.CreateText(DEFAULT_PARTIAL_FILE_NAME);
            fullyMappedWriter.WriteLine("<{0}>", ROOT_TAG);
            partiallyMappedWriter.WriteLine("<{0}>", ROOT_TAG);
   
            foreach (MappedTable table in mappedTables.Values) {
                StreamWriter writeTo;
                if (table.IsFullyMapped) {
                    writeTo = fullyMappedWriter;
                } else {
                    writeTo = partiallyMappedWriter;
                }
                SaveToFile(writeTo, table);
            }

            fullyMappedWriter.WriteLine("</{0}>", ROOT_TAG);
            partiallyMappedWriter.WriteLine("</{0}>", ROOT_TAG);
            fullyMappedWriter.Dispose();
            partiallyMappedWriter.Dispose();
        }
        
        // write the given mapped table to given file
        public void SaveToFile(StreamWriter file, MappedTable table) {
            string guid = table.Guid;
            file.WriteLine(" <table {0}=\"{1}\" {2}=\"{3}\">", 
                           NAME_ATTRIBUTE, table.TableName, GUID_ATTRIBUTE, guid);

            WriteMappedFields(file, PACK_ATTRIBUTE, XML_ATTRIBUTE, table.Mappings);
            WriteMappedFields(file, PACK_ATTRIBUTE, CONSTANT_ATTRIBUTE, table.ConstantPackValues);
            WriteMappedFields(file, XML_ATTRIBUTE, CONSTANT_ATTRIBUTE, table.ConstantXmlValues);

            WriteList(file, UNMAPPED_PACK_FIELDS, table.UnmappedPackFieldNames);
            WriteList(file, UNMAPPED_XML_FIELDS, table.UnmappedXmlFieldNames);
            file.WriteLine(" </table>");
        }
        
        static void WriteMappedFields(StreamWriter writer, string fromTag, string toTag, Dictionary<string, string> map) {
            foreach(string key in map.Keys) {
                writer.WriteLine(string.Format("  <field {0}=\"{2}\" {1}=\"{3}\" />", fromTag, toTag, key, map[key]));
            }
        }

        static void WriteList(StreamWriter writer, string tag, List<string> list) {
            if (list.Count != 0) {
                writer.WriteLine(string.Format("  <{1}>{0}</{1}>", string.Join(",", list), tag));
            }
        }
        #endregion
    }
}

