using Common;
using Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SchemaIntegration.Mapping;
using System.Text.RegularExpressions;

namespace SchemaIntegration {
    class MainClass {
        private static string[] OPTIONS = { 
            // -as: add entries from other schema file by GUID if they don't exists already
            "-as",
            // -cs: read from CA schema directory and correct references
            "-cs",
            // -i : integrate other schema file, overwrite existing entries
            "-i",
            // -mx: find corresponding fields in mod tools xml files
            "-mx",
            // -c: canonicalize type infos (remove start and end versions)
            "-c"
        };
        public static void Main(string[] args) {
            new MainClass().Run(args);
        }
        public void Run(string[] args) {
            foreach (string dir in args) {
                if (dir.StartsWith("-as")) {
                    string path = dir.Substring(3);
                    IntegrateAll(path, integrator.AddSchemaFile);
                } else if (dir.StartsWith("-i")) {
                    string path = dir.Substring(2);
                    IntegrateAll(path, integrator.IntegrateFile);
                } else if (dir.StartsWith("-cs")) {
                    ReplaceSchemaNames(dir.Substring(3));
                } else if (dir.StartsWith("-mx")) {
                    string[] split = dir.Substring(3).Split(Path.PathSeparator);
                    FindCorrespondingFields(split[0], split[1]);
                } else if (dir.Equals("-c")) {
                    SchemaCanonizer sc = new SchemaCanonizer();
                    sc.Canonize();
                } else if (dir.Equals("-v")) {
                    verbose = true;
                }
            }
            SaveSchema();
        }
        private bool verbose;
        private List<Game> games = new List<Game>();
        private SchemaIntegrator integrator = new SchemaIntegrator {
            OverwriteExisting = true,
            IntegrateExisting = true,
            CheckAllFields = true
        };

        delegate void Integrate(string path);

        void IntegrateAll(string paths, Integrate integrate) {
            if (games.Count == 0) {
                Console.WriteLine("Warning: no games set for integration!");
            }
            foreach (Game g in games) {
                integrator.VerifyAgainst = g;
                integrator.Verbose = verbose;
                string[] files = paths.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string file in files) {
                    integrate(file);
                }
            }
        }

        void ReplaceSchemaNames(string xmlDirectory) {
            Dictionary<Tuple<string, string>, string> renamedFields = new Dictionary<Tuple<string, string>, string>();
            foreach (string table in DBTypeMap.Instance.DBFileTypes) {
                string lookupString = table.Replace("_tables", "");
                Console.WriteLine("table {0}", table);
                List<TypeInfo> infos = DBTypeMap.Instance.GetAllInfos(table);
                string guid;
                foreach (TypeInfo typeInfo in infos) {
                    List<CaFieldInfo> caInfos = CaFieldInfo.ReadInfo(xmlDirectory, lookupString, out guid);

                    foreach (FieldInfo info in typeInfo.Fields) {
                        string newName = FieldMappingManager.Instance.GetXmlFieldName(lookupString, info.Name);
                        if (newName != null) {
                            // remember rename to be able to correct references to this later
                            Tuple<string, string> tableFieldTuple = new Tuple<string, string>(table, info.Name);
                            if (!renamedFields.ContainsKey(tableFieldTuple)) {
                                renamedFields.Add(tableFieldTuple, newName);
                            }
                            info.Name = newName;
                            Console.WriteLine("{0}->{1}", info.Name, newName);

                            CaFieldInfo caInfo = CaFieldInfo.FindInList(caInfos, newName);
                            if (caInfo != null) {
                                FieldReference reference = caInfo.Reference;
                                if (reference != null) {
                                    reference.Table = string.Format("{0}_tables", reference.Table);
                                }
                                info.FieldReference = reference;
                            }
                        }
                    }
                }
            }
        }
        void FindCorrespondingFields(string packFile, string modToolsDirectory) {
            string xmlDirectory = Path.Combine(modToolsDirectory, "db");

            FieldMappingManager manager = FieldMappingManager.Instance;
            FieldCorrespondencyFinder finder = new FieldCorrespondencyFinder(packFile, xmlDirectory);
            // finder.RetainExistingMappings = true;
            finder.FindAllCorrespondencies();
            Console.WriteLine("saving");
            manager.Save();
        }

        #region Save Schema
        static readonly Regex NumberedFieldNameRe = new Regex("([^0-9]*)([0-9]+)");
        void SaveSchema() 
        {
            var items = DBTypeMap.Instance.AllInfos.SelectMany(x => x.Value).ToList();
            items.ForEach(info => 
            {
                MakeFieldNamesUnique(info.Fields);
            });
            DBTypeMap.Instance.SaveToFile(Directory.GetCurrentDirectory(), "user");
        }
        void MakeFieldNamesUnique(List<FieldInfo> fields) {
            List<string> used = new List<string>();
            for (int i = 0; i < fields.Count; i++) {
                FieldInfo info = fields[i];
                if (used.Contains(info.Name)) {
                    string newName = MakeNameUnique(info.Name, used, i + 1);
                    info.Name = newName;
                }
                used.Add(info.Name);
            }
        }
        string MakeNameUnique(string name, ICollection<string> alreadyUsed, int index) {
            string result = name;
            int number = index;
            while (alreadyUsed.Contains(result)) {
                if (NumberedFieldNameRe.IsMatch(result)) {
                    Match match = NumberedFieldNameRe.Match(result);
                    number = int.Parse(match.Groups[2].Value) + 1;
                    result = string.Format("{0}{1}", match.Groups[1].Value, number);
                } else {
                    result = string.Format("{0}{1}", name, number);
                }
            }
            return result;
        }
        #endregion
    }
}
