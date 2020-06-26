using Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Filetypes.Codecs
{
    /*
     * Codec reading an XML file provided by CA to create a DBFile.
     * Type info for a table will be read from a TWaD file corresponding to that table.
     * Note that tables are referred to without the _tables suffix by CA,
     * but a proper typename in PFM API does contain it.
     */
    public class CaXmlDbFileCodec : ICodec<DBFile>
    {
        private string xmlPath;

        // type info cache
        static Dictionary<string, TypeInfo> allInfos = new Dictionary<string, TypeInfo>();

        /*
         * Create the xml codec to read table and table meta data from the given path.
         */
        public CaXmlDbFileCodec(string path)
        {
            xmlPath = path;
        }
        /*
         * Query type info for the given table.
         */
        public TypeInfo TypeInfoByTableName(string tablename)
        {
            TypeInfo info = null;
            if (!allInfos.ContainsKey(tablename))
            {
                allInfos[tablename] = LoadTypeInfos(tablename);
            }
            allInfos.TryGetValue(tablename, out info);
            return info;
        }
        /*
         * Decode the given stream to return its data as a DBFile.
         */
        public DBFile Decode(Stream stream)
        {
            DBFile result = null;
            using (TextReader reader = new StreamReader(stream))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);

                foreach (XmlNode dataroot in doc.ChildNodes)
                {
                    string guid = "";
                    foreach (XmlNode entry in dataroot.ChildNodes)
                    {
                        // ignore uuid
                        if ("edit_uuid".Equals(entry.Name))
                        {
                            guid = entry.InnerText;
                            continue;
                        }

                        // use cached type info or read from TWaD if none has been cached yet
                        string recordName = entry.Name;
                        TypeInfo typeinfo;
                        if (!allInfos.TryGetValue(recordName, out typeinfo))
                        {
                            typeinfo = LoadTypeInfos(recordName);
                            allInfos[recordName] = typeinfo;
                        }

                        // create a new header upon the first data item
                        if (result == null)
                        {
                            DBFileHeader header = new DBFileHeader(guid, 0, 0, false);
                            result = new DBFile(header, typeinfo);
                        }

                        // get a field-to-value map and remember the fields requiring translation
                        Dictionary<string, string> fieldValues = new Dictionary<string, string>();
                        List<string> requireTranslation = new List<string>();
                        foreach (XmlNode row in entry.ChildNodes)
                        {
                            fieldValues[row.Name] = row.InnerText;
                            XmlAttribute at = row.Attributes["requires_translation"];
                            if (at != null && "true".Equals(at.Value))
                            {
                                requireTranslation.Add(row.Name);
                            }
                        }

                        // create entry from type info and fill with values
                        List<FieldInstance> fields = result.GetNewEntry();
                        foreach (FieldInstance field in fields)
                        {
                            string val;
                            try
                            {
                                if (fieldValues.TryGetValue(field.Name, out val))
                                {
                                    if (field.Info.TypeName.Equals("boolean"))
                                    {
                                        field.Value = "1".Equals(val) ? "true" : "false";
                                    }
                                    else
                                    {
                                        field.Value = val;
                                    }
                                    field.RequiresTranslation = requireTranslation.Contains(field.Name);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Wait a minute!");
                                throw e;
                            }
                        }
                        result.Entries.Add(new DBRow(typeinfo, fields));
                    }
                }
            }
            return result;
        }

        /*
         * Retrieve type info for the given field from the given list.
         */
        private FieldInfo InfoByName(List<FieldInfo> infos, string fieldName)
        {
            FieldInfo result = null;
            foreach (FieldInfo info in infos)
            {
                if (info.Name.Equals(fieldName))
                {
                    result = info;
                    break;
                }
            }
            return result;
        }
        /*
         * Load type info from TWaD for the given tables name.
         */
        private TypeInfo LoadTypeInfos(string name)
        {
            string twadFilename = string.Format("TWaD_{0}.xml", name.Replace("_tables", ""));
            string twadPath = Path.Combine(xmlPath, twadFilename);
            if (!File.Exists(twadPath))
            {
                return null;
            }
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            // string guid = "";
            using (var reader = File.OpenText(twadPath))
            {
                XmlDocument defDoc = new XmlDocument();
                defDoc.Load(reader);
                foreach (XmlNode root in defDoc.ChildNodes)
                {
                    foreach (XmlNode fieldNode in root.ChildNodes)
                    {
                        if ("edit_uuid".Equals(fieldNode.Name))
                        {
                            // guid = fieldNode.InnerText;
                        }
                        else
                        {
                            fieldInfos.Add(CreateInfoFromNode(fieldNode));
                        }
                    }
                }
            }
            TypeInfo typeInfo = new TypeInfo(fieldInfos)
            {
                Name = name
            };
            // typeInfo.ApplicableGuids.Add(guid);
            return typeInfo;
        }
        /*
         * Utility method to create a FieldInfo from the attributes and data
         * of the given node.
         */
        private FieldInfo CreateInfoFromNode(XmlNode node)
        {
            bool optional = "0".Equals(node["required"].InnerText);
            XmlNode typeNode = node["field_type"];
            string typeText = typeNode.InnerXml;
            if ("text".Equals(typeText))
            {
                typeText = string.Format("{0}string_ascii", (optional ? "opt" : ""));
            }
            FieldInfo info = Types.FromTypeName(typeText);
            info.Optional = optional;
            info.Name = node["name"].InnerText;
            info.PrimaryKey = "1".Equals(node["primary_key"].InnerText);
            XmlNode refTableNode = node["column_source_table"];
            if (refTableNode != null)
            {
                string refTable = refTableNode.InnerText;
                string refColumn = node["column_source_column"].InnerText;
                // Console.WriteLine("reference found: {0}:{1}", string.Format("{0}_tables", refTable), refColumn);
                info.FieldReference = new FieldReference(string.Format("{0}_tables", refTable), refColumn);
            }
            return info;
        }

        public void Encode(Stream stream, DBFile dbFile)
        {
        }
    }
}
