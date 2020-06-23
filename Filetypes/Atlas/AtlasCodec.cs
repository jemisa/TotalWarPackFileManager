using System;
using System.IO;
using Common;

namespace Filetypes {
    public class AtlasCodec : Codec<AtlasFile> {
        public static readonly AtlasCodec Instance = new AtlasCodec();

        public AtlasFile Decode(Stream file) {
            using (BinaryReader reader = new BinaryReader(file)) {
                if (reader.ReadInt32() != 1) {
                    throw new FileLoadException("Illegal atlas file: Does not start with '1'");
                }
                reader.ReadBytes(4);
                int numEntries = reader.ReadInt32();
                AtlasFile result = new AtlasFile();
                for (int i = 0; i < numEntries; i++) {
                    AtlasObject item = new AtlasObject {
                        Container1 = IOFunctions.ReadZeroTerminatedUnicode(reader),
                        Container2 = IOFunctions.ReadZeroTerminatedUnicode(reader),
                        X1 = reader.ReadSingle(),
                        Y1 = reader.ReadSingle(),
                        X2 = reader.ReadSingle(),
                        Y2 = reader.ReadSingle(),
                        X3 = reader.ReadSingle(),
                        Y3 = reader.ReadSingle()
                    };
                    result.add(item);
                }
                return result;
            }
        }
        public void Encode(Stream stream, AtlasFile toEncode) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                writer.Write((uint)1);
                writer.Write((uint)0);
                int numEntries = toEncode.Entries.Count;
                writer.Write(numEntries);
                for (int i = 0; i < numEntries; i++) {
                    IOFunctions.WriteZeroTerminatedUnicode(writer, toEncode.Entries[i].Container1);
                    IOFunctions.WriteZeroTerminatedUnicode(writer, toEncode.Entries[i].Container2);
                    writer.Write(toEncode.Entries[i].X1);
                    writer.Write(toEncode.Entries[i].Y1);
                    writer.Write(toEncode.Entries[i].X2);
                    writer.Write(toEncode.Entries[i].Y2);
                    writer.Write(toEncode.Entries[i].X3);
                    writer.Write(toEncode.Entries[i].Y3);
                }
            }
        }
    }
}