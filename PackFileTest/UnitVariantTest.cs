using System;
using System.IO;
using System.Collections.Generic;
using Common;
using Filetypes;

namespace PackFileTest {
	public class UnitVariantTest : PackedFileTest {
        UnitVariantCodec codec = new UnitVariantCodec();
        
        SortedSet<string> supported = new SortedSet<string>();
		SortedSet<string> wrongSize = new SortedSet<string>();
		SortedSet<string> wrongData = new SortedSet<string>();
		
		public override bool CanTest(PackedFile file) {
			return file.FullPath.EndsWith (".unit_variant");
		}
		
		public override void TestFile(PackedFile file) {
            allTestedFiles.Add(file.FullPath);
            byte[] original = file.Data;
            UnitVariantFile uvFile = null;
            using (MemoryStream stream = new MemoryStream(original, 0, original.Length)) {
                uvFile = codec.Decode(stream);
            }
			byte[] bytes = UnitVariantCodec.Encode (uvFile);
			if (file.Size != bytes.Length) {
				wrongSize.Add (file.FullPath);
			} else {
				// verify data
				byte[] origData = file.Data;
				for (int i = 0; i < origData.Length; i++) {
					if (origData [i] != bytes [i]) {
						wrongData.Add (file.FullPath);
						return;
					}
				}
				supported.Add (file.FullPath);
			}
		}
		
		public override void PrintResults() {
			if (allTestedFiles.Count != 0) {
				Console.WriteLine ("Unit Variant Test:");
				Console.WriteLine ("Successful: {0}/{1}", supported.Count, allTestedFiles.Count);
				PrintList ("Wrong Size", wrongSize);
				PrintList ("Wrong Data", wrongData);
			}
		}
	}
}

