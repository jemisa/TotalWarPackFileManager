using Common;
using Filetypes;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DbSql {
    /*
     * Command providing information about the db files.
     * Without any arguments, a list of db files will be provided;
     * with arguments, the fields and their types will be printed.
     */
    public class HelpCommand : SqlCommand {
        // the form of this command
        public static Regex HELP_RE = new Regex("help( .*)?");
        
        /*
         * Create help command, parsing the given string.
         */
        public HelpCommand (string toParse) {
            Match m = HELP_RE.Match(toParse);
            if (m.Groups.Count > 1) {
                ParseTables(m.Groups[1].Value);
            }
            AllTables = (tables.Count == 0);
        }
        /*
         * If no argument was given to the help clause, print a list of all db tables.
         * If arguments were given, print out a list of fields and their types of the tables 
         * corresponding to each argument; primary key fields will be marked with a "*".
         */
        public override void Execute() {
            if (AllTables) {
                // dump all table names
                foreach (PackedFile packed in PackedFiles) {
                    if (packed.FullPath.StartsWith("db")) {
                        Console.WriteLine(DBFile.Typename(packed.FullPath));
                    }
                }
            } else {
                foreach (DBFile dbFile in DbFiles) {
                    Console.WriteLine("{0}:", dbFile.CurrentType.Name);
                    foreach(FieldInfo info in dbFile.CurrentType.Fields) {
                        string reference = info.FieldReference != null 
                            ? string.Format(" -> {0}:{1}", info.ReferencedTable, info.ReferencedField) : "";
                        Console.WriteLine("{0} : {1}{2}{3}", info.Name, info.TypeName, info.PrimaryKey ? "*" : "", reference);
                    }
                }
            }
        }
    }
}

