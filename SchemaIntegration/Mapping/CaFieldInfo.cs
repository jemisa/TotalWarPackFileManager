using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Filetypes;

namespace SchemaIntegration.Mapping {
    class CaFieldInfo {
        public CaFieldInfo(string name, string type) {
            if (name == null || type == null) {
                throw new InvalidDataException();
            }
            Name = name;
            fieldType = type;
        }

        public string Name { get; private set; }

        string fieldType;
        public string FieldType {
            get { return fieldType; }
        }

        public bool Ignored {
            get {
                return fieldType.Equals("memo");
            }
        }

        public FieldReference Reference { get; set; }

        public static List<CaFieldInfo> ReadInfo(string xmlDirectory, string tableName, out string guid) {
            guid = "unknown";
            List<CaFieldInfo> result = new List<CaFieldInfo>();
            try {
                string filename = Path.Combine(xmlDirectory, string.Format("TWaD_{0}.xml", tableName));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                foreach (XmlNode node in xmlDoc.ChildNodes) {
                    if (node.Name.Equals("root")) {
                        foreach (XmlNode fieldNode in node.ChildNodes) {
                            if (fieldNode.Name.Equals("edit_uuid")) {
                                guid = fieldNode.InnerText;
                            }
                            if (!fieldNode.Name.Equals("field")) {
                                continue;
                            }
                            string name = null, type = null;
                            string refTable = null, refField = null;
                            foreach (XmlNode itemNode in fieldNode.ChildNodes) {
                                if (itemNode.Name.Equals("name")) {
                                    name = itemNode.InnerText;
                                } else if (itemNode.Name.Equals("field_type")) {
                                    type = itemNode.InnerText;
                                } else if (itemNode.Name.Equals("column_source_table")) {
                                    refTable = itemNode.InnerText;
                                } else if (itemNode.Name.Equals("column_source_column")) {
                                    refField = itemNode.InnerText;
                                }
                            }
                            CaFieldInfo info = new CaFieldInfo(name, type);
                            if (refTable != null && refField != null) {
                                info.Reference = new FieldReference(refTable, refField);
                            }
                            result.Add(info);
                        }
                    }
                }
            } catch { }
            return result;
        }

        public static CaFieldInfo FindInList(List<CaFieldInfo> list, string name) {
            CaFieldInfo result = null;
            foreach (CaFieldInfo info in list) {
                if (info.Name.Equals(name)) {
                    result = info;
                    break;
                }
            }
            return result;
        }
    }
}
