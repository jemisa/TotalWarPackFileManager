using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Filetypes {
	public class UnitVariantCodec : Codec<UnitVariantFile> {
		public static readonly UnitVariantCodec Instance = new UnitVariantCodec();
		
		public static byte[] Encode(UnitVariantFile file) {
			using (MemoryStream stream = new MemoryStream()) {
				Instance.Encode (stream, file);
				return stream.ToArray ();
			}
		}
		
		// read from file
		public UnitVariantFile Decode(Stream stream) {
			UnitVariantFile file = new UnitVariantFile ();
			using (BinaryReader reader = new BinaryReader (stream)) {
				byte[] buffer = reader.ReadBytes (4);
				if ((((buffer [0] != 0x56) || (buffer [1] != 0x52)) || (buffer [2] != 0x4e)) || (buffer [3] != 0x54)) {
					throw new FileLoadException ("Illegal unit_variant file: Does not start with 'VRNT'");
				}
				file.Version = reader.ReadUInt32 ();
				int entries = (int)reader.ReadUInt32 ();
				file.Unknown1 = reader.ReadUInt32 ();
				byte[] buffer3 = reader.ReadBytes (4);
				file.B1 = buffer3 [0];
				file.B2 = buffer3 [1];
				file.B3 = buffer3 [2];
				file.B4 = buffer3 [3];
				file.Unknown2 = BitConverter.ToUInt32 (buffer3, 0);
				if (file.Version == 2) {
					file.Unknown3 = reader.ReadInt32 ();
				}
				file.UnitVariantObjects = new List<UnitVariantObject> (entries);
				for (int i = 0; i < entries; i++) {
					UnitVariantObject item = ReadObject (reader);
					file.UnitVariantObjects.Add (item);
				}
				for (int j = 0; j < file.UnitVariantObjects.Count; j++) {
					for (int k = 0; k < file.UnitVariantObjects[j].StoredEntryCount; k++) {
						MeshTextureObject mto = ReadMTO (reader);
						file.UnitVariantObjects [j].MeshTextureList.Add (mto);
					}
				}
			}
			return file;
		}

		private static UnitVariantObject ReadObject(BinaryReader reader) {
            string modelName = IOFunctions.ReadZeroTerminatedUnicode (reader);
            // ignore index
            reader.ReadUInt32();
            uint num2 = reader.ReadUInt32();
            uint entries = reader.ReadUInt32();
            // ignore mesh start
            reader.ReadUInt32();
			UnitVariantObject item = new UnitVariantObject {
                ModelPart = modelName,
                Num2 = num2,
				StoredEntryCount = entries
			};
			return item;
		}
		
		private static MeshTextureObject ReadMTO(BinaryReader reader) {
			MeshTextureObject obj3 = new MeshTextureObject {
                Mesh = IOFunctions.ReadZeroTerminatedUnicode (reader),
                Texture = IOFunctions.ReadZeroTerminatedUnicode (reader),
                Bool1 = reader.ReadBoolean (),
                Bool2 = reader.ReadBoolean ()
            };
			return obj3;
		}
		
		// write to stream
		public void Encode(Stream stream, UnitVariantFile file) {
			using (BinaryWriter writer = new BinaryWriter(stream)) {
				writer.Write ("VRNT".ToCharArray (0, 4));
				writer.Write (file.Version);
				writer.Write ((uint)file.UnitVariantObjects.Count);
				writer.Write (file.Unknown1);
				writer.Write (file.Unknown2);
				if (file.Version == 2) {
					writer.Write (file.Unknown3);
				}
                // write all unit variant entries
				int mtoStartIndex = 0;
                for (int i = 0; i < file.UnitVariantObjects.Count; i++) {
                    UnitVariantObject uvo = file.UnitVariantObjects[i];
                    IOFunctions.WriteZeroTerminatedUnicode(writer, uvo.ModelPart);
                    writer.Write((uint) i);    // index
                    writer.Write(uvo.Num2);    // always 0 afaict
                    writer.Write(uvo.EntryCount);
                    writer.Write(mtoStartIndex);
                    mtoStartIndex += (int) uvo.EntryCount;
                }
                // write all meshes
                foreach(UnitVariantObject uvo in file.UnitVariantObjects) {
                    foreach(MeshTextureObject mto in uvo.MeshTextureList) {
                        IOFunctions.WriteZeroTerminatedUnicode (writer, mto.Mesh);
                        IOFunctions.WriteZeroTerminatedUnicode (writer, mto.Texture);
                        writer.Write (mto.Bool1);
                        writer.Write (mto.Bool2);
                    }
                }
				writer.Flush ();
			}
		}
	}
}

