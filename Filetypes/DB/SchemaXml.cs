using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Filetypes {
    using FieldInfoList = List<FieldInfo>;
    using TypeInfoList = List<TypeInfo>;
    
    public class XmlImporter {
        // table to contained fields
        List<TypeInfo> typeInfos = new List<TypeInfo>();
        public List<TypeInfo> Imported {
            get {
                return typeInfos;
            }
        }
        
        TextReader reader;
        public XmlImporter (Stream stream) {
            reader = new StreamReader (stream);
        }

        static string UnifyName(string name) {
            return name.ToLower ().Replace (" ", "_");
        }

        public void Import(bool unify = false) {
			XmlDocument doc = new XmlDocument ();
			doc.Load (reader);
			foreach (XmlNode node in doc.ChildNodes) {
				foreach (XmlNode tableNode in node.ChildNodes) {
                    string id;
                    int version = 0;
                    FieldInfoList fields = new FieldInfoList ();
                    // bool verifyEquality = false;
                    
                    XmlAttribute attribute = tableNode.Attributes["name"];
                    if (attribute != null) {
                        // pre-GUID table
                        id = attribute.Value;
                        if (unify) {
                            id = UnifyName (id);
                        }
                    } else {
                        id = tableNode.Attributes["table_name"].Value.Trim();
                        string table_version = tableNode.Attributes["table_version"].Value.Trim();
                        version = int.Parse(table_version);
                    }

                    FillFieldList(fields, tableNode.ChildNodes, unify);
                    TypeInfo info = new TypeInfo(fields) {
                        Name = id,
                        Version = version
                    };
#if DEBUG
                    // Console.WriteLine("Adding table {0} version {1}", info.Name, info.Version);
#endif
                    typeInfos.Add(info);
				}
			}
		}
        
        void FillFieldList(List<FieldInfo> fields, XmlNodeList nodes, bool unify = false) {
            // add all fields
            foreach(XmlNode fieldNode in nodes) {
                FieldInfo field = FromNode (fieldNode, unify);
                if (unify) {
                    field.Name = UnifyName (field.Name);
                }
                fields.Add (field);
            }
        }
        
        /* 
         * Collect the given node's attributes and create a field from them. 
         */
        FieldInfo FromNode(XmlNode fieldNode, bool unify) {
            FieldInfo description = null;
            try {
    			XmlAttributeCollection attributes = fieldNode.Attributes;
    			string name = attributes ["name"].Value;
    			string type = attributes ["type"].Value;
                
    			description = Types.FromTypeName (type);
    			description.Name = name;
    			if (attributes ["fkey"] != null) {
    				string reference = attributes ["fkey"].Value;
    				if (unify) {
    					reference = UnifyName (reference);
    				}
    				description.ForeignReference = reference;
    			}
    			if (attributes ["pk"] != null) {
    				description.PrimaryKey = true;
    			}
                
                ListType list = description as ListType;
                if (list != null) {
                    FillFieldList(list.Infos, fieldNode.ChildNodes, unify);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                throw e;
            }
			return description;
		}
    }

    public class XmlExporter {
        TextWriter writer;
        
        public bool LogWriting {
            get; set;
        }
        
        public XmlExporter (Stream stream) {
			writer = new StreamWriter (stream);
        }
        
        public void Export() {
            List<TypeInfo> sorted = new TypeInfoList(DBTypeMap.Instance.AllInfos);
            sorted.Sort();
            Export(sorted);
        }

        public void Export(List<TypeInfo> infos) {
#if DEBUG
            Console.WriteLine("storing {0} infos", infos.Count);
#endif
			writer.WriteLine ("<schema>");
            // WriteTables(infos, new VersionedTableInfoFormatter());
            List<TypeInfo> cleaned = new List<TypeInfo>(infos);
            cleaned.Sort();
#if DEBUG
            Console.WriteLine("cleaned: {0}", cleaned.Count);
#endif
            WriteTables(infos, new GuidTableInfoFormatter());
			writer.WriteLine ("</schema>");
			writer.Close ();
		}
        
        private void WriteTables(List<TypeInfo> tableDescriptions, TableInfoFormatter<TypeInfo> formatter) {
            foreach (TypeInfo typeInfo in tableDescriptions) {
                WriteTable(typeInfo, formatter);
            }
        }
        
        void WriteTable(TypeInfo id, TableInfoFormatter<TypeInfo> format) {
#if DEBUG
            // Console.WriteLine ("writing table {0}", id.Name, id.Version);
#endif
			writer.WriteLine (format.FormatHeader(id));
            WriteFieldInfos (id.Fields);
			writer.WriteLine ("  </table>");
            writer.Flush();
		}
        
        void WriteFieldInfos(FieldInfoList descriptions, int indent = 4) {
            foreach (FieldInfo description in descriptions) {
                StringBuilder builder = new StringBuilder(new string(' ', indent));
                builder.Append("<field ");
                if (!description.ForeignReference.Equals ("")) {
                    builder.Append (string.Format ("fkey='{0}' ", description.ForeignReference));
                }
                builder.Append (string.Format ("name='{0}' ", description.Name));
                builder.Append (string.Format ("type='{0}' ", description.TypeName));
                if (description.PrimaryKey) {
                    builder.Append ("pk='true' ");
                }
                if (description.TypeCode == TypeCode.Object) {
                    builder.Append(">");
                    writer.WriteLine(builder.ToString());
                    
                    // write contained elements
                    ListType type = description as ListType;
                    WriteFieldInfos(type.Infos, indent + 2);
                    
                    // end list tag
                    builder.Clear();
                    builder.Append(new string(' ', indent));
                    builder.Append("</field>");
                } else {
                    builder.Append ("/>");
                }
                writer.WriteLine (builder.ToString ());
            }
        }

        /*
         * Collect all GUIDs with the same type name and definition structure to store them in a single entry.
         */
        private List<TypeInfo> CompileSameDefinitions(List<TypeInfo> sourceList) {
            Dictionary<string, List<TypeInfo>> typeMap = new Dictionary<string, List<TypeInfo>>();
            
            foreach(TypeInfo typeInfo in sourceList) {
                if (!typeMap.ContainsKey(typeInfo.Name)) {
                    List<TypeInfo> addTo = new List<TypeInfo>();
                    addTo.Add(typeInfo);
                    typeMap.Add(typeInfo.Name, addTo);
                } else {
                    bool added = false;
                    foreach(TypeInfo existing in typeMap[typeInfo.Name]) {
                        if (Enumerable.SequenceEqual<FieldInfo>(typeInfo.Fields, existing.Fields)) {
                            added = true;
                            break;
                        }
                    }
                    if (!added) {
                        typeMap[typeInfo.Name].Add(typeInfo);
                    }
                }
            }
            List<TypeInfo> result = new List<TypeInfo>();
            foreach(List<TypeInfo> infos in typeMap.Values) {
                result.AddRange(infos);
            }
            return result;
        }
        
        /*
         * Create string from single definition entry.
         */
        public static string TableToString(String name, int version, FieldInfoList description) {
            string result = "";
            using (var stream = new MemoryStream()) {
                XmlExporter exporter = new XmlExporter(stream);
                TypeInfo info = new TypeInfo(description) {
                    Name = name,
                    Version = version
                };
                exporter.WriteTable(info, new GuidTableInfoFormatter());
                stream.Position = 0;
                result = new StreamReader(stream).ReadToEnd();
            }
            return result;
        }
    }

    #region Formatting
    abstract class TableInfoFormatter<T> {
        public abstract string FormatHeader(T toWrite);
        public string FormatField(FieldInfo description) {
            StringBuilder builder = new StringBuilder ("    <field ");
            builder.Append(FormatFieldContent(description));
            builder.Append ("/>");
            return builder.ToString();
        }
        public virtual string FormatFieldContent(FieldInfo description) {
            StringBuilder builder = new StringBuilder();
            if (!description.ForeignReference.Equals ("")) {
                builder.Append (string.Format ("fkey='{0}' ", description.ForeignReference));
            }
            builder.Append (string.Format ("name='{0}' ", description.Name));
            builder.Append (string.Format ("type='{0}' ", description.TypeName));
            if (description.PrimaryKey) {
                builder.Append ("pk='true' ");
            }
            return builder.ToString();
        }
    }
    /*
     * Formats header with tablename/version and list of applicable GUIDs.
     */
    class GuidTableInfoFormatter : TableInfoFormatter<TypeInfo> {
        static string HEADER_FORMAT = "  <table table_name='{0}'" + Environment.NewLine +
            "         table_version='{1}' >";

        public override string FormatHeader(TypeInfo info) {
            return string.Format(HEADER_FORMAT, info.Name, info.Version);
        }
    }
    #endregion
}

