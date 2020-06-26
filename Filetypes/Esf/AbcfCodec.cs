using System;
using System.Collections.Generic;
using System.IO;

namespace Filetypes {
    public class AbcfFileCodec : AbceCodec {
        protected int headerLength = 16;

        #region String Lookup lists
        protected Dictionary<string, int> Utf16StringList = new Dictionary<string, int>();
        protected Dictionary<string, int> AsciiStringList = new Dictionary<string, int>();
        #endregion

        #region String Reference Functions
        static Dictionary<string, int> ReadStringList(BinaryReader reader, ValueReader<string> readString) {
            // amount of strings in the list
            int count = reader.ReadInt32();
            Dictionary<string, int> result = new Dictionary<string, int>(count);
            for (int i = 0; i < count; i++) {
                // first string, then reference ID
                string read = readString(reader);
                result.Add(read, reader.ReadInt32());
            }
            return result;
        }
        static void WriteStringList(BinaryWriter writer, Dictionary<string, int> stringList, ValueWriter<string> writeString) {
            writer.Write(stringList.Count);
            foreach(string s in stringList.Keys) {
                writeString(writer, s);
                writer.Write(stringList[s]);
            }
        }
        void WriteStringReference(BinaryWriter writer, string toWrite, Dictionary<string, int> referenceList) {
            int index;
            if (referenceList.ContainsKey(toWrite)) {
                index = referenceList[toWrite];
            } else {
                index = referenceList.Count;
                while (referenceList.ContainsValue(index)) {
                    index++;
                }
                referenceList.Add(toWrite, index);
            }
            writer.Write(index);
        }
        #endregion
  
        public AbcfFileCodec(uint id = 0xABCF) : base(id) { }

        // re-rout the string reading to looking up in the appropriate table
        protected override string ReadUtf16String(BinaryReader reader) {
            return ReadStringReference (reader, Utf16StringList);
        }
        protected override string ReadAsciiString(BinaryReader reader) {
            return ReadStringReference (reader, AsciiStringList);
        }
        protected void WriteAsciiReference(BinaryWriter w, string s) {
            WriteStringReference(w, s, AsciiStringList);
        }
        protected void WriteUtf16Reference(BinaryWriter w, string s) {
            WriteStringReference(w, s, Utf16StringList);
        }

        public override EsfNode CreateValueNode(EsfType typeCode, bool optimize = false) {
            EsfNode result;
            switch (typeCode) {
                case EsfType.UTF16:
                    result = new StringNode(ReadUtf16String, WriteUtf16Reference);
                    break;
                case EsfType.ASCII:
                    result = new StringNode(ReadAsciiString, WriteAsciiReference);
                    break;

                // HACK: RoninX
                case EsfType.ASCII_W21:
                    result = new StringNode(ReadAsciiString, WriteAsciiReference);
                    break;
                case EsfType.ASCII_W25:
                    result = new StringNode(ReadAsciiString, WriteAsciiReference);
                    break;

                default:
                    return base.CreateValueNode(typeCode);
            }
            result.TypeCode = typeCode;
            return result;
        }

        // override to read the two string lists after the node names
        protected override void ReadNodeNames(BinaryReader reader) {
            base.ReadNodeNames(reader);
            // create lookup lists (positioned immediately after the node names)
            Utf16StringList = ReadStringList(reader, ReadUtf16);
            AsciiStringList = ReadStringList(reader, ReadAscii);
        }
        protected override void WriteNodeNames(BinaryWriter writer) {
            base.WriteNodeNames(writer);
            WriteStringList(writer, Utf16StringList, WriteUtf16);
            WriteStringList(writer, AsciiStringList, WriteAscii);
        }
    }
}
