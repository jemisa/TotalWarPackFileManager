using System;
using System.IO;
using System.Collections.Generic;
using Filetypes;

namespace SchemaIntegration {
    public class SchemaCanonizer {
        public void Canonize() {
        }
    }
            /*
            List<TypeInfo> infos = CreateAllInfos();
            TypeInfo last = null;
            List<TypeInfo> cleaned = new List<TypeInfo>();
            foreach(TypeInfo info in infos) {
                bool addInfo = true;
                if (last != null && last.Name.Equals(info.Name) && last.SameTypes(info)) {
                    addInfo = false;
                    // check if all fields have the same name
                    // if not, we do need to add the duplicate and correct manually
                    for (int i = 0; i < last.Fields.Count && !addInfo; i++) {
                        if (!last.Fields[i].Name.Equals(info.Fields[i].Name)) {
                            addInfo = true;
                            Console.WriteLine("duplicate info for {0} version {1} due to different field names {2} and {3}", 
                                              info.Name, info.Version, info.Fields[i].Name, last.Fields[i].Name);
                        }
                    }
                }
                if (addInfo) {
                    cleaned.Add(info);
                }
                last = info;
            }
            DBTypeMap.Instance.AllInfos.Clear();
            DBTypeMap.Instance.AllInfos.AddRange(infos);
            DBTypeMap.Instance.SaveToFile(Directory.GetCurrentDirectory(), "canon");
        }
        
        /*
         * Takes all the loaded type infos from DBTypeMap and creates single typeinfos
         * for every applicable version.
         *
        List<TypeInfo> CreateAllInfos() {
            List<TypeInfo> result = new List<TypeInfo>();
            foreach(TypeInfo info in DBTypeMap.Instance.AllInfos) {
                // collect a type info for each version
                List<int> versions = GetAllVersions(info);
                foreach(int version in versions) {
                    // List<FieldInfo> fields = info.ForVersion(version);
                    TypeInfo type = new TypeInfo {
                        Name = info.Name,
                        Version = version
                    };
                    type.Fields.AddRange(fields);
                    result.Add(type);
                }
                // remove all start and end versions from fields
                info.Fields.ForEach(f => { 
                    f.StartVersion = 0;
                    f.LastVersion = int.MaxValue;
                });
            }
            result.Sort();
            return result;
        }
        
        /*
         * Retrieve all versions for which the given type info has definitions.
         *
        private List<int> GetAllVersions(TypeInfo info) {
            List<int> result = new List<int>();
            result.Add(info.Version);
            foreach(FieldInfo field in info.Fields) {
                if (field.LastVersion != int.MaxValue) {
                    if (!result.Contains(field.LastVersion+1)) {
                        result.Add(field.LastVersion+1);
                    }
                }
                if (field.StartVersion != 0) {
                    if (!result.Contains(field.StartVersion)) {
                        result.Add (field.StartVersion);
                    }
                }
            }
            return result;
        }
    */
}

