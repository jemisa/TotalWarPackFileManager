using System;
using System.Collections.Generic;
using System.IO;
using Common;

namespace Filetypes {
	public class LocCodec : Codec<LocFile> {
		public static readonly LocCodec Instance = new LocCodec();
		
		// static helper to set pack file data
		public static byte[] Encode(LocFile file) {
			using (MemoryStream stream = new MemoryStream()) {
				LocCodec.Instance.Encode (stream, file);
				return stream.ToArray ();
			}
		}
        
        public LocFile Decode(byte[] data) {
            using(MemoryStream stream = new MemoryStream(data)) {
                return Decode (stream);
            }
        }

		// read from pack
        public LocFile Decode(Stream stream) {
			LocFile file = new LocFile ();
			//file.Name = Path.GetFileName (packedFile.FullPath);
			using (BinaryReader reader = new BinaryReader (stream)) {
				if (reader.ReadInt16 () != -257) {
					throw new FileLoadException ("Illegal loc file: doesn't have a byte order mark");
				}
				byte[] buffer = reader.ReadBytes (3);
				if (((buffer [0] != 0x4c) || (buffer [1] != 0x4f)) || (buffer [2] != 0x43)) {
					throw new FileLoadException ("Illegal loc file: doesn't have LOC string");
				}
				reader.ReadByte ();
				if (reader.ReadInt32 () != 1) {
					throw new FileLoadException ("Illegal loc file: File version isn't '1'");
				}
				int numEntries = reader.ReadInt32 ();
				// file.entries = new List<LocEntry> ();
				for (int i = 0; i < numEntries; i++) {
					string tag = IOFunctions.ReadCAString (reader);
					string localised = IOFunctions.ReadCAString (reader);
					bool tooltip = reader.ReadBoolean ();
					file.Entries.Add (new LocEntry (tag, localised, tooltip));
				}
				reader.Close ();
			}
			return file;
		}
		
		// write to stream
		public void Encode(Stream stream, LocFile file) {
			using (var writer = new BinaryWriter(stream)) {
				writer.Write ((short)(-257));
				writer.Write ("LOC".ToCharArray ());
				writer.Write ((byte)0);
				writer.Write (1);
				writer.Write (file.NumEntries);
				for (int i = 0; i < file.NumEntries; i++) {
					IOFunctions.WriteCAString (writer, file.Entries [i].Tag);
					IOFunctions.WriteCAString (writer, file.Entries [i].Localised);
					writer.Write (file.Entries [i].Tooltip);
				}
				writer.Flush ();
			}
		}
	}
}

