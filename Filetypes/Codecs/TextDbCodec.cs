using Common;
using Filetypes.ByteParsing;
using Filetypes.DB;
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
            while(!reader.EndOfStream) 
                read.Add(reader.ReadLine());
            
            var tableSchema = SchemaManager.Instance.GetTableDefinitionsForTable(typeInfoName, version);
            foreach(var columnDefinition in tableSchema.ColumnDefinitions) 
            {
                List<DBRow> entries = new List<DBRow> ();
                foreach(var line in read) 
                {
                    var strArray = line.Split(TABS, StringSplitOptions.None);

                    List<DbField> item = new List<DbField>();
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        DbField field = new DbField(columnDefinition.Type);
                        string fieldValue = CsvUtil.Unformat(strArray[i]);
                        field.Value = fieldValue;
                        item.Add(field);
                    }
                    entries.Add(new DBRow(tableSchema, item));
                }
      
                DBFileHeader header = new DBFileHeader()
                {
                    GUID = "",
                    EntryCount = (uint)entries.Count,
                    HasVersionMarker = version != 0,
                    Version = version,
                    UnknownByte = 0
                };

                file = new DBFile (header, tableSchema);
                file.Entries.AddRange (entries);
                
            }
            return file;
        }

        // write the given file to stream
        public void Encode(Stream stream, DBFile file) 
        {
            StreamWriter writer = new StreamWriter (stream);
            // write header
            writer.WriteLine (file.CurrentType.TableName);
            writer.WriteLine (Convert.ToString (file.Header.Version));

            List<string> toWrite = new List<string>();
            foreach (var field in file.CurrentType.ColumnDefinitions)
                toWrite.Add(field.Name);

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

