using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Filetypes;
using Filetypes.Codecs;

namespace PackFileTest {
    public class ModelsTest<T> : PackedFileTest {
        public string ValidTypes {
            get; set;
        }
        public override int TestCount {
            get {
                return successes.Count + countErrors.Count + generalErrors.Count + incompleteReads.Count;
            }
        }
  
        #region Test Fails
        public override List<string> FailedTests {
            get {
                List<string> result = base.FailedTests;
                if (generalErrors.Count > 0) {
                    result.Add("General errors:");
                    result.AddRange(countErrors);
                }
                if (countErrors.Count > 0) {
                    result.Add("Wrong content count:");
                    result.AddRange(countErrors);
                }
                if (incompleteReads.Count > 0) {
                    result.Add("Not all Data read:");
                    result.AddRange(incompleteReads);
                }
                return result;
            }
        }
        
        List<string> countErrors = new List<string>();
        List<string> incompleteReads = new List<string>();
        List<string> successes = new List<string>();
        List<string> emptyFiles = new List<string>();
        #endregion
        
        public override bool CanTest(PackedFile file) {
            return DBFile.Typename(file.FullPath).Equals (ValidTypes);
        }
        
        public override void TestFile(PackedFile packed) {
            allTestedFiles.Add(packed.FullPath);
            if (packed.Data.Length == 0) {
                emptyFiles.Add(packed.FullPath);
                return;
            }
            using (var stream = new MemoryStream(packed.Data)) {
                BuildingModelFile bmFile = new BuildingModelFile(PackedFileDbCodec.Decode(packed));
                if (bmFile.Header.EntryCount != bmFile.Models.Count) {
                    countErrors.Add(string.Format("{0}: invalid count. Should be {1}, is {2}",
                                                  packed.Name, bmFile.Header.EntryCount, bmFile.Models.Count));
                } else {
                    successes.Add(string.Format("{0}", packed.FullPath));
                }
            }
        }
        
        public override void PrintResults() {
            if (TestCount != 0) {
                Console.WriteLine("Models test ({0}):", ValidTypes);
                Console.WriteLine("Supported Files: {0}", successes.Count);
                Console.WriteLine("Empty Files: {0}", successes.Count);
            }
            if (countErrors.Count != 0) {
                PrintList("Entry counts errors", countErrors);
            }
            if (generalErrors.Count != 0) {
                PrintList("General errors", generalErrors);
            }
        }
    }
}

