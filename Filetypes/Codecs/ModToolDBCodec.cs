using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Filetypes.Codecs {
    /*
     * Decodes a DBFile to the format used by CA's Assembly Kit.
     */
    public class ModToolDBCodec : ICodec<DBFile> {
        const string DATE_FORMAT = "ddd d. MMM HH:mm:ss yyyy";
        static readonly CultureInfo GB_CULTURE = CultureInfo.CreateSpecificCulture("en-GB");
        
        FieldMappingManager nameManager;

        public ModToolDBCodec(FieldMappingManager corMan) {
            nameManager = corMan;
        }

        public DBFile Decode(Stream stream) {
            throw new NotSupportedException();
        }

        // write given file to given stream in ca xml format
        public void Encode(Stream dbFile, DBFile file) {
            string typeName = file.CurrentType.Name.Replace("_tables", "");
            using (var writer = new StreamWriter(dbFile)) {
                // write header
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.Write("<dataroot export_time=\"{0}\" "+
                             "revision=\"2\" "+
                             "export_branch=\"T:/branches/shogun2/patch/common/TWeak/Framework\" "+
                             "export_user=\"modder\">", 
                             DateTime.Now.ToString(DATE_FORMAT, GB_CULTURE));
                string guid = file.Header.GUID;
                if (string.IsNullOrEmpty(guid) 
                    && FieldMappingManager.Instance.TableGuidMap.ContainsKey(typeName)) {
                    guid = FieldMappingManager.Instance.TableGuidMap[typeName];
                }
                if (!string.IsNullOrEmpty(guid)) {
                    writer.WriteLine(string.Format("<edit_uuid>{0}</edit_uuid>", file.Header.GUID));
                }
                foreach(List<FieldInstance> fields in file.Entries) {
                    // write all fields from the 
                    writer.WriteLine(" <{0}>", typeName);
                    foreach (FieldInstance field in fields) {
                        if (FieldMappingManager.Instance.IsConstantPackField(typeName, field.Name)) {
                            continue;
                        }
                        string fieldName;
                        try {
                            fieldName = nameManager.GetXmlFieldName(typeName, field.Name);
                        } catch {
                            throw new DBFileNotSupportedException(
                                string.Format("No xml field name for {0}:{1}", typeName, field.Name));
                        }
                        string toWrite = Encode(field);
                        writer.WriteLine("  <{0}>{1}</{0}>", fieldName, toWrite);
                    }
                    writer.WriteLine(" </{0}>", typeName);
                }
                writer.WriteLine("</dataroot>");
            }
        }
        
        string Encode(FieldInstance field) {
            string result = field.Value;
            if (field.Info.TypeCode == TypeCode.Boolean) {
                result = bool.Parse(field.Value) ? "1" : "0";
            } else if (field.Info.TypeCode == TypeCode.Single) {
                float val = float.Parse(result);
                result = val.ToString(GB_CULTURE);
            }
            return result;
        }
    }
}

