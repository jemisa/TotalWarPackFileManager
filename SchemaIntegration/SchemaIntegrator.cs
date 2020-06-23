using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common;
using Filetypes;

namespace SchemaIntegration {
    /**
     * Reads a schema file and integrates it into the currently loaded definitions
     * by renaming all applicable fields to the names contained in the new file.
     * 
     * Applicable fields are those that are contained in tables of the same version.
     */
    public class SchemaIntegrator {
        public bool Verbose {
            get; set;
        }
        public Game VerifyAgainst {
            get;
            set;
        }
        public bool IntegrateExisting {
            get; set;
        }
        public bool OverwriteExisting {
            get; set;
        }
        
        private Dictionary<string, List<FieldInfo>> references = null;
        private Dictionary<string, PackedFile> packedFiles = new Dictionary<string, PackedFile>();
        
        public SchemaIntegrator() {
            if (!DBTypeMap.Instance.Initialized) {
                DBTypeMap.Instance.initializeFromFile(Path.Combine(Directory.GetCurrentDirectory(), DBTypeMap.MASTER_SCHEMA_FILE_NAME));
            }
        }
        
        private List<string> failedIntegrations = new List<String>();
        public List<string> FailedIntegrations {
            get {
                return failedIntegrations;
            }
        }
        
        private void BuildReferenceCache() {
            if (references != null) {
                return;
            }
            references = new Dictionary<string, List<FieldInfo>>();
            Console.WriteLine("building reference cache");
            foreach(TypeInfo typeInfo in DBTypeMap.Instance) {
                foreach(FieldInfo field in typeInfo.Fields) {
                    if (!string.IsNullOrEmpty(field.ForeignReference)) {
                        List<FieldInfo> addTo;
                        if (!references.TryGetValue(field.ForeignReference, out addTo)) {
#if DEBUG
                            // Console.WriteLine("Reference found: {0}", field.ForeignReference);
#endif
                            addTo = new List<FieldInfo>();
                        }
                        addTo.Add(field);
                    }
                }
            }

            Console.WriteLine("ok, done");
        }
        
        /*
         * Add db files from given pack to the packedFiles dictionary.
         */
        private void LoadDbFiles() {
            if (VerifyAgainst != null) {
                Console.WriteLine("building pack file cache");
                foreach (string file in new PackLoadSequence().GetPacksLoadedFrom(VerifyAgainst.GameDirectory)) {
                    PackFile pack = new PackFileCodec().Open(file);
                    foreach (PackedFile packed in pack) {
                        Console.WriteLine("loading {0}", packed.FullPath);
                        if (packed.FullPath.StartsWith("db")) {
                            string typename = DBFile.Typename(packed.FullPath);
                            if (!packedFiles.ContainsKey(typename)) {
                                packedFiles[typename] = packed;
                            }
                        }
                    }
                }
            }
        }
        
        public void IntegrateFile(string filename) {
            LoadDbFiles();
            BuildReferenceCache();
            Console.WriteLine("Integrating schema file {0}", filename);
            XmlImporter importer = null;
            using (var stream = File.OpenRead(filename)) {
                importer = new XmlImporter(stream);
                importer.Import();
            }

            if (IntegrateExisting) {
                foreach (TypeInfo type in importer.Imported) {
                    TypeInfo replaced = null;
                    foreach(TypeInfo existing in DBTypeMap.Instance.GetVersionedInfos(type.Name, type.Version)) {
                        if (existing.SameTypes(type)) {
                            replaced = existing;
                            break;
                        }
                    }
                    if (replaced != null) {
                        DBTypeMap.Instance.AllInfos.Remove(replaced);
                    }
                    DBTypeMap.Instance.AllInfos.Add(type);
                }
            }
        }

        public void AddSchemaFile(string file) {
            using (var stream = File.OpenRead(file)) {
                XmlImporter importer = new XmlImporter(stream);
                importer.Import();
                foreach (TypeInfo info in importer.Imported) {
                    Console.WriteLine("adding {0}", info.Name);
                    DBTypeMap.Instance.AllInfos.Add (info);
                }
            }
        }

        static void ReplaceUnknowns(List<FieldInfo> replaceIn, List<FieldInfo> replaceFrom) {
            if (replaceIn.Count != replaceFrom.Count) {
                return;
            }
            for (int i = 0; i < replaceIn.Count; i++) {
                FieldInfo fromInfo = replaceFrom[i];
                if (!UNKNOWN_RE.IsMatch(fromInfo.Name)) {
                    continue;
                }
                FieldInfo toInfo = replaceIn[i];
                
                if (!fromInfo.TypeName.Equals(toInfo.TypeName)) {
                    return;
                }
                toInfo.Name = fromInfo.Name.ToLower();
            }
        }

        static readonly Regex UNKNOWN_RE = new Regex("[Uu]nknown[0-9]*");
        void IntegrateInto (string type, List<FieldInfo> integrateInto, List<FieldInfo> integrateFrom) {
            for(int i = 0; i < integrateFrom.Count; i++) {
                FieldInfo fromInfo = integrateFrom[i];
                if (integrateInto.Count <= i) {
                    Console.WriteLine(" stopping integration: can't find {0}. field in {1}", 
                                      i, FormatTable(type, integrateInto));
                }
                FieldInfo to = integrateInto[i];
                if (to.TypeCode != fromInfo.TypeCode) {
                    throw new InvalidDataException(string.Format("Field {0}: invalid Type (can't integrate {1}, original {2})",
                                                                 i, fromInfo.TypeName, to.TypeName));
                }
                // don't rename to "unknown"
                if (UNKNOWN_RE.IsMatch(fromInfo.Name)) {
                    if (Verbose) {
                        Console.WriteLine("Not renaming {0} to {1}", fromInfo.Name, to.Name);
                    }
                    continue;
                }
                if (!to.Name.Equals(fromInfo.Name)) {
                    if (Verbose) {
                        Console.WriteLine("Renaming {0} to {1}", fromInfo.Name, to.Name);
                    }
                    CorrectReferences(type, to, fromInfo.Name);
                    to.Name = fromInfo.Name;
                }
            }
        }
        
        private List<string> DoSearch(DBFile dataOne, DBFile dataTwo) {
            List<string> messages = new List<string>();
            if (dataTwo.Entries.Count == 0) {
                return messages;
            }
            for(int i = 0; i < dataTwo.CurrentType.Fields.Count; i++) {
                FieldInfo info = dataTwo.CurrentType.Fields[i];
                if (dataOne.Entries.Count == 0 || dataOne.Entries.Count != dataTwo.Entries.Count) {
                    continue;
                }
                if (dataTwo.Entries[0][i].RequiresTranslation) {
                    continue;
                }
                
                // description fields are localized, so they are not packed directly
                if (info.Name.Contains("description") || info.Name.Contains("localis") || 
                    info.Name.Contains("onscreen") || info.Name.Equals("author")) {
                    continue;
                }
                List<string> fieldValues = GetFieldValues(dataTwo.Entries, i);
                if (fieldValues == null) {
                    continue;
                }
                List<string> correspondingFields = GetMatchingFieldNames(dataOne, fieldValues);
                if (correspondingFields.Count == 0 && !AllValuesEqual(fieldValues)) {
                    messages.Add(string.Format("No matching fields found for {0}:{1}", dataTwo.CurrentType.Name, info.Name));
                    messages.Add(string.Format("values: {0}", string.Join(",", fieldValues)));
                }
            }
            return messages;
        }
        private bool AllValuesEqual(List<string> fieldValues) {
            string firstValue = null;
            foreach(string value in fieldValues) {
                if (firstValue == null) {
                    firstValue = value;
                    continue;
                }
                if (!firstValue.Equals(value)) {
                    return false;
                }
            }
            return true;
        }
        
        // when integrating CA tables, check all values for all fields?
        // that takes really long
        public bool CheckAllFields { get; set; }
        
        List<string> GetFieldValues(List<DBRow> fieldList, int fieldIndex) {
            List<string> fieldData = new List<string>();
            foreach(List<FieldInstance> fields in fieldList) {
                fieldData.Add(fields[fieldIndex].Value);
            }
            return fieldData;
        }
        List<string> GetMatchingFieldNames(DBFile file, List<string> toMatch) {
            List<string> result = new List<string>();
            for (int i = 0; i < file.CurrentType.Fields.Count; i++) {
                List<string> values = GetFieldValues(file.Entries, i);
                if (Enumerable.SequenceEqual<string>(values, toMatch)) {
                    result.Add(file.CurrentType.Fields[i].Name);
                } else if (file.CurrentType.Name.Equals("boolean") && values.Count == toMatch.Count) {
                    // check for booleans stored as ints in the xml
                    bool match = true;
                    for(int valIndex = 0; valIndex < toMatch.Count; valIndex++) {
                        bool matchValue = Boolean.Parse(toMatch[i]);
                        bool checkValue = "1".Equals(values[i]);
                        if (matchValue != checkValue) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        result.Add(file.CurrentType.Fields[i].Name);
                    }
                }
            }
            return result;
        }

        void AddCaReferences(TypeInfo caInfo, List<FieldInfo> existingInfo) {
            List<FieldInfo> ourPrimaryKeys = new List<FieldInfo>();
            foreach (FieldInfo caField in caInfo.Fields) {
                if (caField.FieldReference != null || caField.PrimaryKey) {

                    // find our field so we can set reference or pkey
                    foreach (FieldInfo ourField in existingInfo) {
                        if (ourField.Name.Equals(caField.Name)) {
                            if (caField.PrimaryKey) {
                                ourPrimaryKeys.Add(ourField);
                            }
                            if (caField.FieldReference != null && packedFiles.ContainsKey(caField.ReferencedTable)) {
                                // found the corresponding field
                                ourField.FieldReference = caField.FieldReference;
                                break;
                            } else if (ourField.FieldReference != null) {
                                ourField.FieldReference = null;
                            }
                            break;
                        }
                    }
                }
            }
            // adjust pkeys
            foreach(FieldInfo info in existingInfo) {
                bool newPkey = ourPrimaryKeys.Contains(info);
#if DEBUG
                if (newPkey != info.PrimaryKey) {
                    Console.WriteLine("{0}setting {1} as primary key", (newPkey ? "" : "un"), info.Name);
                    info.PrimaryKey = newPkey;
                }
#endif
            }
        }
        
        void CorrectReferences(string type, FieldInfo toInfo, string newName) {
            string referenceString = FieldReference.FormatReference(type, toInfo.Name);
            
            if (references.ContainsKey(referenceString)) {
                foreach(FieldInfo info in references[referenceString]) {
                    info.ReferencedField = newName;
                    Console.WriteLine("Correcting reference {0}: to {1}", 
                                      referenceString, info.ForeignReference);
                }
            }
        }
        
        static string FormatTable(string tableName, List<FieldInfo> info) {
            return string.Format("{0}: {1}", tableName, string.Join(",", info));
        }

        public static bool CanDecode(PackedFile dbFile) {
            bool valid = false;
            try {
                DBFileHeader header = PackedFileDbCodec.readHeader(dbFile);
                DBFile decoded = PackedFileDbCodec.Decode(dbFile);
                valid = (decoded.Entries.Count == header.EntryCount);
                return valid;
            } catch (Exception) {
            }
            return valid;
        }
        
        static ICollection<List<FieldInfo>> InfosForTypename(string type, int version) {
            ICollection<List<FieldInfo>> result = new List<List<FieldInfo>>();
            foreach (TypeInfo info in DBTypeMap.Instance.GetVersionedInfos(type, version)) {
                result.Add(info.Fields);
            }
            return result;
        }
  
        /*
         * This doesn't really belong here...
         * changes all strings in an existing table definition to string_asci.
         */
        public void ConvertAllStringsToAscii(string packFile) {
            PackFile file = new PackFileCodec().Open(packFile);
            foreach (PackedFile packed in file) {
                if (packed.FullPath.StartsWith("db")) {
                    string typename = DBFile.Typename(packed.FullPath);
                    DBFileHeader header = PackedFileDbCodec.readHeader(packed);
                    if (!string.IsNullOrEmpty(header.GUID)) {
                        // List<FieldInfo> infos = DBTypeMap.Instance.GetInfoByGuid(header.GUID);
                        if (!CanDecode(packed)) {
                            // we don't have an entry for this yet; try out the ones we have
                            List<TypeInfo> allInfos = DBTypeMap.Instance.GetAllInfos(typename);
                            if (allInfos.Count > 0) {
                                // TryDecode(packed, header, allInfos);
                            } else {
                                Console.WriteLine("no info at all for {0}", typename);
                            }
                        } else {
                            Console.WriteLine("already have info for {0}", header.GUID);
                        }
                    }
                }
            }
        }
        
        /*
        void TryDecode(PackedFile dbFile, DBFileHeader header, List<TypeInfo> infos) {
            foreach (TypeInfo info in infos) {
                // register converted to type map
                List<FieldInfo> converted = ConvertToAscii(info.Fields);
                DBTypeMap.Instance.SetByGuid(header.GUID, DBFile.Typename(dbFile.FullPath), header.Version, converted);
                bool valid = SchemaIntegrator.CanDecode(dbFile);
                if (!valid) {
                    DBTypeMap.Instance.SetByGuid(header.GUID, DBFile.Typename(dbFile.FullPath), header.Version, null);
                } else {
                    // found it! :)
                    Console.WriteLine("adding converted info for guid {0}", header.GUID);
                    break;
                }
            }
        }
        */

        List<FieldInfo> ConvertToAscii(List<FieldInfo> old) {
            List<FieldInfo> newInfos = new List<FieldInfo>(old.Count);
            foreach (FieldInfo info in old) {
                string typeName = info.TypeName.EndsWith("string") ? string.Format("{0}_ascii", info.TypeName) : info.TypeName;
                FieldInfo newInfo = Types.FromTypeName(typeName);
                newInfo.Name = info.Name;
                newInfo.FieldReference = info.FieldReference;
                if (!string.IsNullOrEmpty(newInfo.ForeignReference)) {
                    newInfo.ForeignReference = info.ForeignReference;
                    //newInfo.ReferencedField = info.ReferencedField;
                }
                newInfo.PrimaryKey = info.PrimaryKey;
                newInfos.Add(newInfo);   
            }
            return newInfos;
        }
    }
}

