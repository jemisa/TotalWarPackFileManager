using Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Filetypes.Codecs {
    /*
     * TSV import/export.
     */
    public class TextDbCodec : ICodec<DBFile> {
        static char[] QUOTES = { '"' };
        static char[] TABS = { '\t' };
        static char[] GUID_SEPARATOR = { ':' };
        
        public static readonly ICodec<DBFile> Instance = new TextDbCodec();
        
        public static byte[] Encode(DBFile file) {
            using (MemoryStream stream = new MemoryStream()) {
                TextDbCodec.Instance.Encode(stream, file);
                return stream.ToArray();
            }
        }

        public DBFile Decode(Stream stream) {
            return Decode (new StreamReader (stream));
        }

        // read from given stream
        public DBFile Decode(StreamReader reader) {
            // another tool might have saved tabs and quotes around this 
            // (at least open office does)
            string typeInfoName = reader.ReadLine ().Replace ("\t", "").Trim (QUOTES);
            string[] split = typeInfoName.Split(GUID_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 2) {
                typeInfoName = split[0];
            }
            string versionStr = reader.ReadLine ().Replace ("\t", "").Trim (QUOTES);
            int version;
            switch (versionStr) {
            case "1.0":
                version = 0;
                break;
            case "1.2":
                version = 1;
                break;
            default:
                version = int.Parse (versionStr);
                break;
            }

            DBFile file = null;
            // skip table header
            reader.ReadLine();
            List<String> read = new List<String>();
            while(!reader.EndOfStream) {
                read.Add(reader.ReadLine());
            }
            
            List<TypeInfo> infos = DBTypeMap.Instance.GetVersionedInfos(typeInfoName, version);
            foreach(TypeInfo info in infos) {
                bool parseSuccessful = true;
                
                List<DBRow> entries = new List<DBRow> ();
                foreach(String line in read) {
                    try {
                        String[] strArray = line.Split (TABS, StringSplitOptions.None);
                        if (strArray.Length != info.Fields.Count) {
                            parseSuccessful = false;
                            break;
                        }
                        List<FieldInstance> item = new List<FieldInstance> ();
                        for (int i = 0; i < strArray.Length; i++) {
                            FieldInstance field = info.Fields [i].CreateInstance();
                            string fieldValue = CsvUtil.Unformat (strArray [i]);
                            field.Value = fieldValue;
                            item.Add (field);
                        }
                        entries.Add (new DBRow(info, item));
#if DEBUG
                    } catch (Exception x) {
                        Console.WriteLine (x);
#else
                    } catch {
#endif
                        parseSuccessful = false;
                        break;
                    }
                }
                if (parseSuccessful) {
                    String guid = "";
                    DBFileHeader header = new DBFileHeader (guid, version, (uint)entries.Count, version != 0);
                    file = new DBFile (header, info);
                    file.Entries.AddRange (entries);
                    break;
                }
            }
            return file;
        }

        // write the given file to stream
        public void Encode(Stream stream, DBFile file) {
            StreamWriter writer = new StreamWriter (stream);
            // write header
            writer.WriteLine (file.CurrentType.Name);
            writer.WriteLine (Convert.ToString (file.Header.Version));
            List<string> toWrite = new List<string>();
            file.CurrentType.Fields.ForEach(f => toWrite.Add(f.Name));
            writer.WriteLine(string.Join("\t", toWrite));
            // write entries
            file.Entries.ForEach(e => {
                toWrite.Clear();
                e.ForEach(field => toWrite.Add(field.Value));
                writer.WriteLine(string.Join("\t", toWrite));
            });
            writer.Flush ();
        }
    }
}

