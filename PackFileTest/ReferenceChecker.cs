using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Filetypes;

namespace PackFileTest {
    class ReferenceChecker {
        public ReferenceChecker() {
            FailedResults = new Dictionary<PackFile, CheckResult>();
        }
        public ReferenceChecker(string reference, List<string> referencing) : this() {
            referenceTo = reference;
            referencesFrom.AddRange(referencing);
        }

        public static List<ReferenceChecker> CreateCheckers() {
            List<ReferenceChecker> checkers = new List<ReferenceChecker>();
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            DBTypeMap.Instance.InitializeTypeMap(Directory.GetCurrentDirectory());
            foreach (TypeInfo table in DBTypeMap.Instance.AllInfos.SelectMany(x=>x.Value)) 
            {
                foreach (FieldInfo info in table.Fields) {
                    if (!string.IsNullOrEmpty(info.ForeignReference)) {
                        List<string> addTo;
                        if (!result.TryGetValue(info.ForeignReference, out addTo)) {
                            addTo = new List<string>();
                            result[info.ForeignReference] = addTo;
                        }
                        addTo.Add(string.Format("{0}.{1}", table, info.Name));
                    }
                }
            }
            foreach (string referenced in result.Keys) {
                checkers.Add(new ReferenceChecker(referenced, result[referenced]));
            }
            return checkers;
        }

        string referenceTo;
        public string ReferencedTable {
            get {
                return referenceTo.Split('.')[0];
            }
        }
        public string ReferencedFieldName {
            get {
                return referenceTo.Split('.')[1];
            }
        }

        private List<string> referencesFrom = new List<string>();
        public List<string> ReferencesFrom {
            get { return referencesFrom; }
        }
        private List<PackFile> packFiles = new List<PackFile>();
        public List<PackFile> PackFiles {
            get { return packFiles; }
        }
        public Dictionary<PackFile, CheckResult> FailedResults {
            get;
            private set;
        }

        public void CheckReferences() {
            packFiles.ForEach(pack => {
                CheckPack(pack);
            });
        }
        void CheckPack(PackFile pack) {
            PackedFile referenced = null;
            Dictionary<PackedFile, List<string>> referencing = new Dictionary<PackedFile, List<string>>();
            foreach (PackedFile packed in pack) {
                if (packed.FullPath.StartsWith("db")) {
                    if (DBFile.Typename(packed.FullPath).Equals(ReferencedTable)) {
                        referenced = packed;
                    }
                    foreach (string referenceFrom in referencesFrom) {
                        if (referenceFrom.Split('.')[0].Equals(DBFile.Typename(packed.FullPath))) {
                            List<string> referencingList;
                            if (!referencing.TryGetValue(packed, out referencingList)) {
                                referencingList = new List<string>();
                                referencing.Add(packed, referencingList);
                            }
                            referencingList.Add(referenceFrom);
                        }
                    }
                }
            }
            if (referenced != null) {
                foreach (PackedFile referencingFile in referencing.Keys) {
                    foreach(string fieldReference in referencing[referencingFile]){
                        CheckResult result = new CheckResult {
                            ReferencingTable = referencingFile,
                            ReferencingFieldName = fieldReference.Split('.')[1],
                            ReferencedTable = referenced,
                            ReferencedFieldName = this.ReferencedFieldName
                        };
                        if (result.UnfulfilledReferences.Count > 0) {
                            FailedResults.Add(pack, result);
                            break;
                        }
                    }
                    if (FailedResults.ContainsKey(pack)) {
                        break;
                    }
                }
            }
        }
    }

    class CheckResult {
        public PackedFile ReferencingTable {
            get;
            set;
        }
        public string ReferencingFieldName {
            get;
            set;
        }
        public string ReferencingString {
            get {
                return string.Format("{0}.{1}", ReferencingTable.FullPath, ReferencingFieldName);
            }
        }
        public PackedFile ReferencedTable {
            get;
            set;
        }
        public string ReferencedFieldName {
            get;
            set;
        }
        public string ReferencedString {
            get {
                return string.Format("{0}.{1}", ReferencedTable.FullPath, ReferencedFieldName);
            }
        }

        public SortedSet<string> ValidValues {
            get {
                SortedSet<string> values = new SortedSet<string>();
                try {
                    // DBFile dbFile = PackedFileDbCodec.Decode(ReferencedTable);
                    DBReferenceMap.FillFromPacked(values, ReferencedTable, ReferencedFieldName);
                } catch {
                    // Console.WriteLine("could not determine value from {0}", ReferencedTable.FullPath);
                }
                return values;
            }
        }

        public SortedSet<string> UnfulfilledReferences {
            get {
                SortedSet<string> result = new SortedSet<string>();
                try {
                    DBReferenceMap.FillFromPacked(result, ReferencingTable, ReferencingFieldName);
                    SortedSet<string> valid = ValidValues;
                    result.RemoveWhere(s => valid.Contains(s));
                } catch (Exception ex) {
                    result.Add(ex.Message);
                }
                return result;
            }
        }
    }
}
