using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace Filetypes {
    public class TableConstraint {
        public string Name { get; set; }

        public string Table { get; set; }

        public List<string> Fields { get; set; }
    }

    class TableReferenceEntry {
        public string Name { get; set; }

        public string fromTable { get; set; }
        public string fromRow { get; set; }
        public int fromTableIndex { get; set; }

        public string toTable { get; set; }
        public string toRow { get; set; }
        public int toTableIndex { get; set; }
    }

    public class XsdParser {
        public static Regex TABLES_SUFFIX = new Regex ("_tables");
        int lastVersion = 0;
        SortedDictionary<string, List<TypeInfo>> allInfos = new SortedDictionary<string, List<TypeInfo>> ();
        XmlSchema schema;
        List<TableConstraint> constraints = new List<TableConstraint> ();

        // temporaries during parsing
        string currentDbFileName;
        TypeInfo currentInfo;
        List<TypeInfo> infos;

        public XsdParser (string file) {
            FileStream fs;
            XmlSchemaSet set;
            try {
                fs = new FileStream (file, FileMode.Open);
                schema = XmlSchema.Read (fs, new ValidationEventHandler (ShowCompileError));
                set = new XmlSchemaSet ();
                set.Add (schema);

                set.Compile ();

                Console.WriteLine ("reading finished");
                //loadXsd();
            } catch (XmlSchemaException e) {
                Console.WriteLine ("LineNumber = {0}", e.LineNumber);
                Console.WriteLine ("LinePosition = {0}", e.LinePosition);
                Console.WriteLine ("Message = {0}", e.Message);
                Console.WriteLine ("Source = {0}", e.Source);
            }
        }

        public SortedDictionary<string, List<TypeInfo>> loadXsd () {
            handleObject (schema);
            return allInfos;
        }

        private void startNewDbFile (XmlSchemaComplexType type) {
            // add previously read db file
            if (currentDbFileName != null) {
                addCurrentInfo ();
                allInfos.Add (currentDbFileName.Replace ("_tables", ""), infos);
            }
            lastVersion = 0;
            currentDbFileName = type.Name;
            currentInfo = new TypeInfo {
                Name = currentDbFileName
            };
        }

        private void addDbAttribute(XmlSchemaAttribute attribute) {
			if (attribute.UnhandledAttributes != null) {
				foreach (XmlAttribute unhandled in attribute.UnhandledAttributes) {
					if (unhandled.Name == "msprop:Optional" && unhandled.Value == "true") {
//						optional = true;
					}
					if (unhandled.Name == "msProp:VersionStart") {
						int nextVersion = int.Parse (unhandled.Value);
						addCurrentInfo ();
						lastVersion = nextVersion;
					}
				}
			}
			FieldInfo fieldType = Types.FromTypeName (attribute.AttributeSchemaType.TypeCode.ToString ());
			fieldType.Name = attribute.Name;
			currentInfo.Fields.Add (fieldType);
		}

        private void handleObject (XmlSchemaObject o) {
            string str = "unknown";
            XmlSchemaObjectCollection children = new XmlSchemaObjectCollection ();
            if (o is XmlSchema) {
                str = "root";
                children = ((XmlSchema)o).Items;
            } else if (o is XmlSchemaComplexType) {
                XmlSchemaComplexType type = (XmlSchemaComplexType)o;
                startNewDbFile (type);
                str = type.Name;
                children = type.Attributes;
                infos = new List<TypeInfo> ();
            } else if (o is XmlSchemaAttribute) {
                XmlSchemaAttribute attribute = (XmlSchemaAttribute)o;
                addDbAttribute (attribute);
                str = string.Format ("{0} ({1})", attribute.Name, attribute.AttributeSchemaType.TypeCode);
            } else if (o is XmlSchemaElement) {
                // children = ((XmlSchemaElement)o).
                XmlSchemaElement element = (XmlSchemaElement)o;
                foreach (XmlSchemaObject constraint in element.Constraints) {
                    if (constraint is XmlSchemaUnique) {
                        XmlSchemaUnique unique = (XmlSchemaUnique)constraint;
                        List<string> fields = new List<string> (unique.Fields.Count);
                        foreach (XmlSchemaObject field in unique.Fields) {
                            if (field is XmlSchemaXPath) {
                                fields.Add (((XmlSchemaXPath)field).XPath);
                            }
                        }
                        constraints.Add (new TableConstraint
                        {
                            Name = unique.Name,
                            Table = unique.Selector.XPath,
                            Fields = fields
                        });
                    } else if (constraint is XmlSchemaKeyref) {
                        XmlSchemaKeyref reference = (XmlSchemaKeyref)constraint;
                        string fromTable = reference.Selector.XPath.Substring (3).Replace ("_tables", "");
                        string fromRow = ((XmlSchemaXPath)reference.Fields [0]).XPath.Substring (1);
                        try {
                            TableReferenceEntry tableRef = resolveReference (reference.Name, fromTable, fromRow, reference.Refer.Name);
                            if (tableRef != null) {
                                Console.WriteLine ("{0}#{1} - {2}#{3}", tableRef.fromTable, tableRef.fromTableIndex, tableRef.toTable, tableRef.toTableIndex);
                            } else {
                                Console.WriteLine ("could not resolve reference");
                            }
                        } catch (Exception x) {
                            Console.WriteLine (x);
                        }
                    }
                }
            } else {
                Console.WriteLine ("unknown type: {0}", o);
            }

            if (o is XmlSchemaAnnotated && ((XmlSchemaAnnotated)o).UnhandledAttributes != null) {
                string attlist = "";
                new List<XmlAttribute> (((XmlSchemaAnnotated)o).UnhandledAttributes).ForEach (uh => attlist += " " + uh);
                str = string.Format ("{0} (unhandled: {1})", str, attlist);
            }

            foreach (XmlSchemaObject child in children) {
                handleObject (child);
            }
        }
        
        private void addCurrentInfo () {
            for (int i = infos.Count; i < lastVersion + 1; i++) {
                infos.Add (null);
            }
            infos [lastVersion] = currentInfo;
        }
        
        private static void ShowCompileError (object sender, ValidationEventArgs e) {
            Console.WriteLine ("Validation Error: {0}", e.Message);
        }

        TableReferenceEntry resolveReference(string referenceName, string fromTable, string fromRow, string uniqueName) {
            int index = findTableRowIndex (fromTable, fromRow);
            if (index == -1) {
                return null;
            }
            string toTable = null;
            int toIndex = -1;
            string row = null;
            foreach (TableConstraint constraint in constraints) {
                if (constraint.Name == uniqueName) {
                    if (constraint.Fields.Count != 1) {
                        return null;
                    }
                    toTable = constraint.Table.Substring (3).Replace ("_tables", "");
                    row = constraint.Fields [0].Substring (1);
                    toIndex = findTableRowIndex (toTable, row);
                }
            }
            if (toIndex == -1) {
                return null;
            }
            return new TableReferenceEntry { Name = referenceName,
                fromTable = fromTable, fromRow = fromRow, fromTableIndex = index,
                toTable = toTable, toRow = row, toTableIndex = toIndex };
        }

        int findTableRowIndex (string tableName, string rowName) {
            List<TypeInfo> fromInfos = null;
            if (!allInfos.TryGetValue (tableName, out fromInfos)) {
                return -1;
            }
            TypeInfo firstInfo = null;
            foreach (TypeInfo ti in fromInfos) {
                if (ti != null) {
                    firstInfo = ti;
                    break;
                }
            }
            if (firstInfo == null) {
                return -1;
            }
            int index = 0;
            for (; index < firstInfo.Fields.Count; index++) {
                if (firstInfo.Fields [index].Name == rowName) {
                    break;
                }
            }
            return (index == firstInfo.Fields.Count) ? -1 : index;
        }
    }
}