using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;
using Filetypes.ByteParsing;
using Filetypes.DB;

namespace Filetypes.Codecs 
{
    public class PackedFileDbCodec : ICodec<DBFile> 
    {
        string _typeName;

        #region Internal
        // header markers
        static uint GUID_MARKER = BitConverter.ToUInt32(new byte[] { 0xFD, 0xFE, 0xFC, 0xFF }, 0);
        static uint VERSION_MARKER = BitConverter.ToUInt32(new byte[] { 0xFC, 0xFD, 0xFE, 0xFF }, 0);
        #endregion

        public static DBFile Decode(PackedFile file)
        {
            PackedFileDbCodec codec = FromFilename(file.FullPath);
            return codec.Decode(file.Data);
        }
        public static PackedFileDbCodec FromFilename(string filename)
        {
            return new PackedFileDbCodec(DBFile.Typename(filename));
        }

        public PackedFileDbCodec(string type) 
        {
            _typeName = type;
        }

        public byte[] ReadAllBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        #region Read
        /*
		 * Reads a db file from stream, using the version information
		 * contained in the header read from it.
		 */
        public DBFile Decode(Stream stream) 
        {
            byte[] buffer = ReadAllBytes(stream);
            ByteChunk byteChunk = new ByteChunk(buffer);

            DBFileHeader header = readHeader(byteChunk);

            var tableDef = SchemaManager.Instance.GetTableDefinitionsForTable(_typeName, header.Version);
            try
            {
                DBFile result = ReadFile(byteChunk, header, tableDef);
                return result;
            }
            catch 
            { }
            return null;
     
        }

        DBFile ReadFile(ByteChunk byteChunk, DBFileHeader header, DbTableDefinition info) 
        {
            DBFile file = new DBFile(header, info);
            for (int i = 0; i < header.EntryCount; i++)
            {
                try
                {
                    var row = ReadFields(byteChunk, info);
                    file.Entries.Add(row);
                }
                catch (Exception x)
                {
                    string message = string.Format("{2} at entry {0}, db version {1}", i, file.Header.Version, x.Message);
                    throw new DBFileNotSupportedException(message, x);
                }
            }

            if (file.Entries.Count != header.EntryCount) 
                throw new DBFileNotSupportedException(string.Format("Expected {0} entries, got {1}", header.EntryCount, file.Entries.Count));
             else if (byteChunk.BytesLeft != 0) 
                throw new DBFileNotSupportedException(string.Format("Expected {0} bytes, {1} bytes left in stream", header.Length, byteChunk.BytesLeft));
            
           
            return file;
        }


        public DBFile Decode(byte[] data) 
        {
            using (MemoryStream stream = new MemoryStream(data, 0, data.Length))
            {
                var file = Decode(stream);
                return file;
            }
            
        }
        #endregion

        public static bool CanDecode(PackedFile packedFile, out string display) 
        {
            display = "";
            bool result = true;
            string key = DBFile.Typename(packedFile.FullPath);
            if (SchemaManager.Instance.IsSupported(key)) {
                /*try {
                    DBFileHeader header = readHeader(packedFile);
                    int maxVersion = DBTypeMap.Instance.MaxVersion(key);
                    if (maxVersion != 0 && header.Version > maxVersion) {
                        display = string.Format("{0}: needs {1}, has {2}", key, header.Version, DBTypeMap.Instance.MaxVersion(key));
                        result = false;
                    } else {
                        display = string.Format("Version: {0}", header.Version);
                    }
                } catch (Exception x) {
                    display = string.Format("{0}: {1}", key, x.Message);
                }*/
            } else {
                display = string.Format("{0}: no definition available", key);
                result = false;
            }
            return result;
        }

        #region Read Header
        public static DBFileHeader readHeader(PackedFile file) 
        {
            var chunk = new ByteChunk(file.Data);
            return readHeader(chunk);
        }

        public static DBFileHeader readHeader(ByteChunk byteChunk) 
        {
            int version = 0;
            string guid = "";
            bool hasMarker = false;

            var guidByte = byteChunk.PeakUint32();
            if (guidByte == GUID_MARKER)
            {
                byteChunk.ReadUInt32();
                guid = byteChunk.ReadStringAscii();
            }

            var marker = byteChunk.PeakUint32();
            if (marker == VERSION_MARKER)
            {
                byteChunk.ReadInt32();
                hasMarker = true;
                version = byteChunk.ReadInt32();
            }

            var unknownByte = byteChunk.ReadByte();
            var entryCount = byteChunk.ReadUInt32();

            DBFileHeader header = new DBFileHeader()
            {
                GUID = guid,
                EntryCount = entryCount,
                HasVersionMarker = hasMarker,
                Version = version,
                UnknownByte = unknownByte
            };
            return header;
        }
        #endregion

        // creates a list of field values from the given type.
        // stream needs to be positioned at the beginning of the entry.
        DBRow ReadFields(ByteChunk byteChunk, DbTableDefinition tableDefinition) 
        {
            DBRow row = new DBRow(tableDefinition);
            for (int i = 0; i < row.Count; i++)
            {
                try
                {
                    row[i].Decode(byteChunk);
                }
                catch (Exception x)
                {
                    throw new InvalidDataException(string.Format
                        ("Failed to read field {0}/{1}, type {3} ({2})", i, row.Count, x.Message, tableDefinition.TableName));
                }
            }

            return row;
        }

        #region Write
        /*
         * Encodes db file to the given stream.
         */
        public void Encode(Stream stream, DBFile file) 
        {
            BinaryWriter writer = new BinaryWriter(stream);
            file.Header.EntryCount = (uint)file.Entries.Count;
            WriteHeader(writer, file.Header);
            foreach (var entry in file.Entries)
                WriteRow(writer, entry);
            writer.Flush();
        }
        /*
         * Encode db file to memory and return it as a byte array.
         */
        public byte[] Encode(DBFile file) 
        {
            using (MemoryStream stream = new MemoryStream()) 
            {
                Encode(stream, file);
                return stream.ToArray();
            }
        }

        /*
         * Writes the given header to the given writer.
         */
        void WriteHeader(BinaryWriter writer, DBFileHeader header)
        {
            if (header.GUID != "") 
            {
                writer.Write(GUID_MARKER);
                IOFunctions.WriteCAString(writer, header.GUID, Encoding.Unicode);
            }
            if (header.Version != 0) 
            {
                writer.Write(VERSION_MARKER);
                writer.Write(header.Version);
            }

            writer.Write((byte)1);
            writer.Write(header.EntryCount);
        }

        /*
         * Write the given entry to the given writer.
         */
        void WriteRow(BinaryWriter writer, List<DbField> fields) {
#if DEBUG
            for (int i = 0; i < fields.Count; i++) {
                try {
                    var field = fields[i];
                    field.Encode(writer);
                } catch (Exception x) {
                    Console.WriteLine(x);
                    throw x;
                }
            }
#else
            fields.ForEach(delegate(FieldInstance field) { field.Encode(writer); });
#endif
        }
        #endregion
    }
}