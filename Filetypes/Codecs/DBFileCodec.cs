using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Common;

namespace Filetypes.Codecs {
    /*
     * A class parsing dbfiles from and to data streams in packed file format.
     */
    public class PackedFileDbCodec : ICodec<DBFile> 
    {
        
        string _typeName;
        public delegate void EntryLoaded(FieldInfo info, string value);
        public delegate void HeaderLoaded(DBFileHeader header);
        public delegate void LoadingPackedFile(PackedFile packed);


        #region Internal
        // header markers
        static UInt32 GUID_MARKER = BitConverter.ToUInt32(new byte[] { 0xFD, 0xFE, 0xFC, 0xFF }, 0);
        static UInt32 VERSION_MARKER = BitConverter.ToUInt32(new byte[] { 0xFC, 0xFD, 0xFE, 0xFF }, 0);
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

        #region Read
        /*
		 * Reads a db file from stream, using the version information
		 * contained in the header read from it.
		 */
        public DBFile Decode(Stream stream) 
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.BaseStream.Position = 0;
                DBFileHeader header = readHeader(reader);
                List<TypeInfo> infos = DBTypeMap.Instance.GetVersionedInfos(_typeName, header.Version);
                if (infos.Count == 0)
                    infos.AddRange(DBTypeMap.Instance.GetAllInfos(_typeName));

                foreach (TypeInfo realInfo in infos)
                {
                    try
                    {
                        DBFile result = ReadFile(reader, header, realInfo);
                        return result;
                    }
                    catch 
                    { }
                }
                return null;
            }
        }

        DBFile ReadFile(BinaryReader reader, DBFileHeader header, TypeInfo info) 
        {
            reader.BaseStream.Position = header.Length;
            DBFile file = new DBFile(header, info);
            int i = 0;
            while (reader.BaseStream.Position < reader.BaseStream.Length) 
            {
                try 
                {
                    file.Entries.Add(ReadFields(reader, info));
                    i++;
                } 
                catch (Exception x) 
                {
                    string message = string.Format("{2} at entry {0}, db version {1}", i, file.Header.Version, x.Message);
                    throw new DBFileNotSupportedException(message, x);
                }
            }

            if (file.Entries.Count != header.EntryCount) 
                throw new DBFileNotSupportedException(string.Format("Expected {0} entries, got {1}", header.EntryCount, file.Entries.Count));
             else if (reader.BaseStream.Position != reader.BaseStream.Length) 
                throw new DBFileNotSupportedException(string.Format("Expected {0} bytes, read {1}", header.Length, reader.BaseStream.Position));
            
           
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
            bool result = true;
            string key = DBFile.Typename(packedFile.FullPath);
            if (DBTypeMap.Instance.IsSupported(key)) {
                try {
                    DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
                    int maxVersion = DBTypeMap.Instance.MaxVersion(key);
                    if (maxVersion != 0 && header.Version > maxVersion) {
                        display = string.Format("{0}: needs {1}, has {2}", key, header.Version, DBTypeMap.Instance.MaxVersion(key));
                        result = false;
                    } else {
                        display = string.Format("Version: {0}", header.Version);
                    }
                } catch (Exception x) {
                    display = string.Format("{0}: {1}", key, x.Message);
                }
            } else {
                display = string.Format("{0}: no definition available", key);
                result = false;
            }
            return result;
        }

        #region Read Header
        public static DBFileHeader readHeader(PackedFile file) 
        {
            using (MemoryStream stream = new MemoryStream(file.Data, (int)0, (int)file.Size)) 
            {
                return readHeader(new BinaryReader(stream));
            }
        }

        public static DBFileHeader readHeader(BinaryReader reader) 
        {
            byte index = reader.ReadByte();
            int version = 0;
            string guid = "";
            bool hasMarker = false;
            uint entryCount = 0;

            try {
                if (index != 1) 
                {
                    // I don't think those can actually occur more than once per file
                    while (index == 0xFC || index == 0xFD) {
                        var bytes = new List<byte>(4);
                        bytes.Add(index);
                        bytes.AddRange(reader.ReadBytes(3));
                        UInt32 marker = BitConverter.ToUInt32(bytes.ToArray(), 0);
                        if (marker == GUID_MARKER) {
                            guid = IOFunctions.ReadCAString(reader, Encoding.Unicode);
                            index = reader.ReadByte();
                        } else if (marker == VERSION_MARKER) {
                            hasMarker = true;
                            version = reader.ReadInt32();
                            index = reader.ReadByte();
                            // break;
                        } 
                        else 
                        {
                            throw new DBFileNotSupportedException(string.Format("could not interpret {0}", marker));
                        }
                    }
                }
                entryCount = reader.ReadUInt32();
            } 
            catch 
            {
            }
            DBFileHeader header = new DBFileHeader(guid, version, entryCount, hasMarker);
            return header;
        }
        #endregion

        // creates a list of field values from the given type.
        // stream needs to be positioned at the beginning of the entry.
        DBRow ReadFields(BinaryReader reader, TypeInfo type, bool skipHeader = true) 
        {
            if (!skipHeader) 
                readHeader(reader);
            
            List<FieldInstance> entry = new List<FieldInstance>();
            for (int i = 0; i < type.Fields.Count; ++i) 
            {
                FieldInfo field = type.Fields[i];

                FieldInstance instance = null;
                try 
                {
                    instance = field.CreateInstance();
                    instance.Decode(reader);
                    entry.Add(instance);
                } 
                catch (Exception x) 
                {
                    throw new InvalidDataException(string.Format
                        ("Failed to read field {0}/{1}, type {3} ({2})", i, type.Fields.Count, x.Message, instance.Info.TypeName));
                }
            }
            DBRow result = new DBRow(type, entry);
            return result;
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
            file.Entries.ForEach(delegate(DBRow e) { WriteEntry(writer, e); });
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
        void WriteEntry(BinaryWriter writer, List<FieldInstance> fields) {
#if DEBUG
            for (int i = 0; i < fields.Count; i++) {
                try {
                    FieldInstance field = fields[i];
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