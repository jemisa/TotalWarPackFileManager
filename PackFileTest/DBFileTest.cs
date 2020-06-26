using System;
using System.IO;
using System.Collections.Generic;
using Common;
using Filetypes;
using Filetypes.Codecs;

namespace PackFileTest {
    public class DBFileTest : PackedFileTest {
        // to conveniently set breakpoints for certain tables.
#if DEBUG
        private string debug_at = "";
#endif

        public bool TestTsv {
            get; set;
        }

        public bool OutputTable {
            get; set;
        }

        public override bool CanTest(PackedFile packed) {
            bool result = packed.FullPath.StartsWith("db");
            result &= !DBFile.Typename(packed.FullPath).Equals ("models_building_tables");
            result &= !DBFile.Typename(packed.FullPath).Equals ("models_naval_tables");
            return result;
        }

        public override int TestCount {
            get {
                return supported.Count + noDefinition.Count + noDefForVersion.Count +
                    invalidDefForVersion.Count + emptyTables.Count + tsvFails.Count;
            }
        }

        public override List<string> FailedTests {
            get {
                List<string> result = base.FailedTests;
                result.AddRange(FormatAllFails("No Definition:", noDefinition));
                result.AddRange(FormatAllFails("No Definition for version:", noDefForVersion));
                result.AddRange(FormatAllFails("Invalid Definition for version:", invalidDefForVersion));
                result.AddRange(FormatAllFails("TSV fails:", tsvFails));
                return result;
            }
        }

        #region Result lists
        public SortedSet<Tuple<string, int>> supported = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);
        public SortedSet<Tuple<string, int>> emptyTables = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);

        public SortedSet<Tuple<string, int>> noDefinition = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);
        public SortedSet<Tuple<string, int>> noDefForVersion = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);
        public SortedSet<Tuple<string, int>> invalidDefForVersion = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);
        public SortedSet<Tuple<string, int>> tsvFails = new SortedSet<Tuple<string, int>>(VERSION_COMPARE);
        #endregion
        
        // test the given packed file as a database file
        // tests PackedFileCodec and the db definitions we have
        public override void TestFile(PackedFile file) {
            if (!DBTypeMap.Instance.Initialized) {
                DBTypeMap.Instance.InitializeTypeMap(Directory.GetCurrentDirectory());
            }
            allTestedFiles.Add(file.FullPath);
            if (file.Size == 0) {
                emptyTables.Add(new Tuple<string, int>(DBFile.Typename(file.FullPath), -1));
                return;
            }

            // PackedFileDbCodec packedCodec = PackedFileDbCodec.FromFilename(file.FullPath);
            string type = DBFile.Typename(file.FullPath);
            DBFileHeader header = PackedFileDbCodec.readHeader(file);
            Tuple<string, int> tuple = new Tuple<string, int>(string.Format("{0} # {1}", type, header.GUID), header.Version);
            if (OutputTable) {
                Console.WriteLine("TABLE:{0}#{1}#{2}", type, header.Version, header.GUID);
            }
            Console.Out.Flush();
            if (header.EntryCount == 0) {
                // special case: we will never find out the structure of a file
                // if it contains no data
                emptyTables.Add(tuple);
            } else if (DBTypeMap.Instance.IsSupported(type)) {
                SortedSet<Tuple<string, int>> addTo = null;
                try {
#if DEBUG
                    if (!string.IsNullOrEmpty(debug_at) && file.FullPath.Contains(debug_at)) {
                        Console.WriteLine("stop right here");
                    }
#endif
                    // a wrong db definition might not cause errors,
                    // but read less entries than there are
                    DBFile dbFile = PackedFileDbCodec.Decode(file);
                    if (dbFile.Entries.Count == dbFile.Header.EntryCount) {
                        addTo = supported;
                        
                        //if (!string.IsNullOrEmpty(header.GUID)) {
                        //    List<FieldInfo> fields = new List<FieldInfo>(DBTypeMap.Instance.GetVersionedInfo(header.GUID, type, header.Version).fields);
                        //    DBTypeMap.Instance.SetByGuid(header.GUID, type, header.Version, fields);
                        //}
                        
                        // only test tsv import/export if asked,
                        // it takes some time more than just the read checks
                        if (TestTsv) {
                            TestTsvExport(dbFile);
                        }
                    } else {
                        // didn't get what we expect
                        addTo = invalidDefForVersion;
#if DEBUG
                        if (!string.IsNullOrEmpty(debug_at) && file.FullPath.EndsWith(debug_at)) {
                            Console.WriteLine("adding watched to invalid");
                        }
#endif
                    }
                } catch {
#if DEBUG
                    if (!string.IsNullOrEmpty(debug_at) && file.FullPath.EndsWith(debug_at)) {
                        Console.WriteLine("adding watched to invalid");
                    }
#endif
                    addTo = invalidDefForVersion;
                }
                addTo.Add(tuple);
            } else {
                noDefinition.Add(tuple);
            }
        }

        // test the tsv codec
        public void TestTsvExport(DBFile originalFile) {
            Tuple<string, int> tuple = new Tuple<string, int>(originalFile.CurrentType.Name, originalFile.Header.Version);
            DBFile reimport;
            try {
                // export to tsv
                TextDbCodec codec = new TextDbCodec();
                string exportPath = Path.Combine(Path.GetTempPath(), "exportTest.tsv");
#if DEBUG
                if (originalFile.CurrentType.Name.Equals(debug_at)) {
                    Console.WriteLine("stop right here");
                }
#endif
                using (Stream filestream = File.Open(exportPath, FileMode.Create)) {
                    codec.Encode(filestream, originalFile);
                }
                // re-import
                using (Stream filestream = File.OpenRead(exportPath)) {
                    reimport = codec.Decode(filestream);
                    // check all read values against original ones
                    for (int row = 0; row < originalFile.Entries.Count; row++) {
                        for (int column = 0; column < originalFile.CurrentType.Fields.Count; column++) {
                            FieldInstance originalValue = originalFile[row, column];
                            FieldInstance reimportValue = reimport[row, column];
                            if (!originalValue.Equals(reimportValue)) {
                                tsvFails.Add(tuple);
                            }
                        }
                    }
                }
            } catch (Exception x) {
                Console.WriteLine(x);
                tsvFails.Add(tuple);
            }
        }

         // write files that failed to filesystem individually for later inspection
		void ExtractFiles(string dir, PackFile pack, ICollection<Tuple<string, int>> toExtract) {
			if (toExtract.Count != 0) {
				string path = Path.Combine (dir, "failed");
				Directory.CreateDirectory (path);
				foreach (Tuple<string, int> failed in toExtract) {
					string failType = failed.Item1;
					string failPath = string.Format ("db\\{0}_tables\\{0}", failType);
					PackedFile found = null;
					foreach (PackedFile packed in pack.Files) {
						if (packed.FullPath.Equals (failPath)) {
							found = packed;
							break;
						}
					}
					if (found != null) {
						string filePath = Path.Combine (path, string.Format ("{0}_{1}", failType, failed.Item2));
						File.WriteAllBytes (Path.Combine (dir, filePath), found.Data);
					} else {
						Console.WriteLine ("cant extract {0}", failPath);
					}
				}
			}
		}

		public override void PrintResults() {
            if (!(supported.Count == 0 && emptyTables.Count == 0)) {
                Console.WriteLine("Database Test:");
                Console.WriteLine("Supported Files: {0}", supported.Count);
                Console.WriteLine("Empty Files: {0}", emptyTables.Count);
            }
			PrintList ("General errors", generalErrors);
			PrintList ("No description", noDefinition);
			PrintList ("no definition for version", noDefForVersion);
			PrintList ("invalid description", invalidDefForVersion);
			PrintList ("Tsv exports", tsvFails);
			Console.WriteLine ();
		}
		
        #region Print Utilities
		static void PrintList(string label, ICollection<Tuple<string, int>> list) {
			if (list.Count != 0) {
				Console.WriteLine ("{0}: {1}", label, list.Count);
				foreach (Tuple<string, int> tableVersion in list) {
					Console.WriteLine ("Type {0}, Version {1}", tableVersion.Item1, tableVersion.Item2);
				}
			}
		}

		static void PrintList(string label, ICollection<Tuple<string, int, int>> list) {
			if (list.Count != 0) {
				Console.WriteLine ("{0}: {1}", label, list.Count);
				foreach (Tuple<string, int, int> tableVersion in list) {
					Console.WriteLine ("Type {0}, Version {1} downgraded {2}", tableVersion.Item1, tableVersion.Item2, tableVersion.Item3);
				}
			}
		}
        #endregion
  
        #region Error Message Formatting utilities
        List<string> FormatAllFails(string title, ICollection<Tuple<string, int>> fails) {
            List<string> result = new List<string>();
            if (fails.Count > 0) {
                result.Add(title);
                foreach (Tuple<string, int> fail in fails) {
                    result.Add(FormatFailedTest(fail));
                }
            }
            return result;
        }
        string FormatFailedTest(Tuple<string, int> tableAndVersion) {
            return string.Format("{0} - version {1}", tableAndVersion.Item1, tableAndVersion.Item2);
        }
        #endregion


		// i like sorted things
        class TableVersionComparer : IComparer<Tuple<string, int>> {
            public int Compare(Tuple<string, int> a, Tuple<string, int> b) {
                int result = a.Item1.CompareTo(b.Item1);
                if (result == 0) {
                    result = a.Item2.CompareTo(a.Item2);
                }
                return result;
            }
        }
        static IComparer<Tuple<string, int>> VERSION_COMPARE = new TableVersionComparer();
    }
}