using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using Common;
using Filetypes;
using SchemaIntegration.Mapping;

//using ValueList = System.Collections.Generic.List<string>;
//using Table = System.Tuple<string, System.Collections.Generic.List<string>>;
using NameMapping = System.Tuple<string, string>;

namespace SchemaIntegration {
    /*
     * Finds the field correspondencies between the db file schema and the ones used by CA.
     * Does this by comparing all values within a column, so it depends on
     * the pack file containing the exact same patch level as the xml files.
     * Also, might fail on columns with very few values or where there are several columns
     * in a table which have the exact same values.
     */
    public class FieldCorrespondencyFinder {
        string xmlDirectory;

        Dictionary<string, MappedDataTable> mappedTables = new Dictionary<string, MappedDataTable>();
        
        public FieldCorrespondencyFinder (string packFile, string xmlDir) {
            xmlDirectory = xmlDir;
            DBTypeMap.Instance.InitializeTypeMap(Directory.GetCurrentDirectory());
            // initialize patchFileValues from pack file
            PackFile pack = new PackFileCodec().Open(packFile);
            foreach(PackedFile contained in pack.Files) {
                if (contained.FullPath.StartsWith("db")) {
                    // no need to resolve if it's done already...
                    string tableName = DBFile.Typename(contained.FullPath).Replace("_tables", "");
                    try {
                        PackedFileDbCodec codec = PackedFileDbCodec.GetCodec(contained);
                        codec.AutoadjustGuid = false;
                        DBFile dbFile = codec.Decode(contained.Data);

                        MappedDataTable table = new MappedDataTable(tableName);
                        ValuesFromPack(table, dbFile);
                        ValuesFromXml(table);

                        mappedTables[tableName] = table;
#if DEBUG
                    } catch (Exception e) {
                        Console.Error.WriteLine(e.Message);
                    }
#else
                    } catch { }
#endif
                }
            }
        }

        public bool RetainExistingMappings { get; set; }
        
        public void FindAllCorrespondencies() {
            if (!RetainExistingMappings) {
                FieldMappingManager.Instance.Clear();
            }

            // add manually adjusted mappings
            Dictionary<string, List<NameMapping>> manualMappings = FieldMappingManager.LoadFromFile("manual_correspondencies.xml");

            foreach (MappedDataTable table in mappedTables.Values) {
                FindCorrespondencies(table);

                if (table.UnmappedPackFieldNames.Count == 1 && table.UnmappedXmlFieldNames.Count == 1) {
                    table.AddMapping(table.UnmappedPackFieldNames[0], table.UnmappedXmlFieldNames[0]);
                }

                if (manualMappings.ContainsKey(table.TableName)) {
                    foreach (NameMapping mapping in manualMappings[table.TableName]) {
                        table.AddMapping(mapping.Item1, mapping.Item2);
                    }
                    if (table.UnmappedPackFieldNames.Count == 1 && table.UnmappedXmlFieldNames.Count == 1) {
                        table.AddMapping(table.UnmappedPackFieldNames[0], table.UnmappedXmlFieldNames[0]);
                    }
                }

                List<string> unmappedPackFields = new List<string>(table.UnmappedPackFieldNames);
                List<string> unmappedXmlFields = new List<string>(table.UnmappedXmlFieldNames);
                foreach (string unmappedPack in unmappedPackFields) {
                    foreach (string unmappedXml in unmappedXmlFields) {
                        if (unmappedXml.Equals(unmappedPack)) {
                            table.AddMapping(unmappedPack, unmappedXml);
                            break;
                        }
                    }
                }
            }

            FieldMappingManager.Instance.Clear();

            foreach (MappedDataTable table in mappedTables.Values) {
                FieldMappingManager.Instance.MappedTables[table.TableName] = table;
            }
        }
        
        /*
         * Find the corresponding fields for all columns in the given table.
         */
        void FindCorrespondencies(MappedDataTable table) {
            
            foreach(string field in table.PackData.Fields) {
                NameMapping mapping = FindCorrespondency(table, field);
                if (mapping != null) {
                    table.AddMapping(mapping.Item1, mapping.Item2);
                }
            }
        }
        
        /*
         * In the given list, find the table with all values equal to the given one from the pack.
         */
        NameMapping FindCorrespondency(MappedDataTable dataTable, string fieldName) {
            string existingMapping = FieldMappingManager.Instance.GetXmlFieldName(dataTable.TableName, fieldName);
            if (existingMapping != null) {
                return new NameMapping(fieldName, existingMapping);
            }

            NameMapping result = null;
            List<string> values = dataTable.PackData.Values(fieldName);

            List<string> packTableNames = dataTable.PackData.FieldsContainingValues(values);
            List<string> xmlTableNames = dataTable.XmlData.FieldsContainingValues(values);

            // check if we have an existing mapping
            if (FieldMappingManager.Instance.MappedTables.ContainsKey(dataTable.TableName)) {
                MappedTable removeMap = FieldMappingManager.Instance.MappedTables[dataTable.TableName];
                foreach(string packField in removeMap.Mappings.Keys) {
                    packTableNames.Remove(packField);
                    xmlTableNames.Remove(removeMap.Mappings[packField]);
                }
            }

            if (packTableNames.Count == 1 && xmlTableNames.Count == 1) {
                // only one left for each
                result = new NameMapping(packTableNames[0], xmlTableNames[0]);
            } else {
                // find matching names
                foreach(string xmlField in xmlTableNames) {
                    if (xmlField.Equals(UnifyName(fieldName))) {
                        result = new NameMapping(fieldName, xmlField);
                        break;
                    }
                }
            }

#if DEBUG
            if (result == null) {
                Console.WriteLine("Did not find corresponding for field {0}, values {1}", fieldName, string.Join(",", values));
            }
#endif

            return result;
        }

        string UnifyName(string packColumnName) {
            return packColumnName.Replace(' ', '_').ToLower();
        }
        
        /*
         * Retrieve the columnname/valuelist collection from the db file of the given type.
         */
        void ValuesFromPack(MappedDataTable table, DBFile dbFile) {
            foreach(List<FieldInstance> row in dbFile.Entries) {
                foreach(FieldInstance field in row) {
                    table.PackData.AddFieldValue(field.Name, field.Value);
                }
            }
        }
  
        /*
         * Retrieve the columnname/valuelist collection from the table with the given name.
         */
        void ValuesFromXml(MappedDataTable fill) {
            XmlDocument tableDocument = new XmlDocument();
            string xmlFile = Path.Combine(xmlDirectory, string.Format("{0}.xml", fill.TableName));
            string guid;
            List<CaFieldInfo> infos = CaFieldInfo.ReadInfo(xmlDirectory, fill.TableName, out guid);
            infos.ForEach(i => {
                fill.XmlDataTypes[i.Name] = i.FieldType;
            });

            if (File.Exists(xmlFile)) {
                tableDocument.Load(xmlFile);
                foreach(XmlNode node in tableDocument.ChildNodes[1]) {
                    if (node.Name.Equals(fill.TableName)) {
                        Dictionary<string, string> keyValues = new Dictionary<string, string>();
                        infos.ForEach(i => { 
                            keyValues[i.Name] = "";
                        });
                        foreach(XmlNode valueNode in node.ChildNodes) {
                            //fill.XmlData.AddFieldValue(valueNode.Name, valueNode.InnerXml);
                            keyValues[valueNode.Name] = valueNode.InnerText;
                        }
                        foreach (string key in keyValues.Keys) {
                            fill.XmlData.AddFieldValue(key, keyValues[key]);
                        }
                    } else if ("edit_uuid".Equals(node.Name)) {
                        fill.Guid = node.InnerText;
                    } else {
                        // skip header
                        continue;
                    }
                }
            }
        }

        /*
         * Query the user for the unresolved columns of the given table.
         * Return false if user requested quit.
         */
        public bool ManuallyResolveTableMapping(string tableName) {

            MappedDataTable table = mappedTables[tableName];
            Console.WriteLine("\nTable {0}", table.TableName);
            List<NameMapping> mappings = new List<NameMapping>();
            List<string> candidates = new List<string>(table.UnmappedXmlFieldNames);
            foreach (string query in table.UnmappedPackFieldNames) {
                if (candidates.Count == 0) {
                    continue;
                }
                Console.WriteLine("Enter corresponding field for \"{0}\":", query);
                for (int i = 0; i < candidates.Count; i++) {
                    Console.WriteLine("{0}: {1}", i, candidates[i]);
                }
                int response = -1;
                while (response != int.MinValue && (response < 0 || response > candidates.Count - 1)) {
                    string val = Console.ReadLine();
                    if ("q".Equals(val)) {
                        return false;
                    } else if ("n".Equals(val)) {
                        response = int.MinValue;
                        break;
                    }
                    int.TryParse(val, out response);
                }
                if (response == int.MinValue) {
                    continue;
                }
                string mapped = candidates[response];
                candidates.Remove(mapped);
                mappings.Add(new NameMapping(query, mapped));
            }
            return true;
        }
    }
}
