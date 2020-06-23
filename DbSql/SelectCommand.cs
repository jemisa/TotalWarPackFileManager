using Common;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    using RowValues = List<string>;
    
    /*
     * Retrieve data from a table; can include a where clause to limit the rows to output.
     */
    public class SelectCommand : FieldCommand, IValueSource {
        // form of the select statement
        public static Regex SELECT_RE = new Regex("select (.*) from (.*)( *where .*)?", RegexOptions.RightToLeft);
        
        public bool Silent { get; set; }
        
        private WhereClause whereClause;
        private List<RowValues> values = null;
        public List<RowValues> Values { 
            get {
                if (values == null) {
                    Execute();
                }
                return values;
            }
        }
        
        public override IEnumerable<PackedFile> PackedFiles {
            protected get {
                return base.PackedFiles;
            }
            set {
                base.PackedFiles = value;
                values = null;
            }
        }
  
        /*
         * Parse given string to create select command.
         */
        public SelectCommand(string toParse) {
            Match match = SELECT_RE.Match(toParse);
            ParseFields (match.Groups[1].Value);
            ParseTables (match.Groups[2].Value);
            if (match.Groups.Count > 3 && !string.IsNullOrEmpty(match.Groups[3].Value)) {
                whereClause = new WhereClause(match.Groups[3].Value);
            }
        }
  
        /*
         * Output the selected fields from the table from rows matching the given where clause,
         * or from all rows if none was given.
         */
        public override void Execute() {
            // List<DBRow> result = new List<DBRow>();
            values = new List<RowValues>();
            foreach(DBFile db in DbFiles) {
                foreach(DBRow row in db.Entries) {
                    if (whereClause != null && !whereClause.Accept(row)) {
                        continue;
                    }
                    RowValues fieldValues = new RowValues();
                    if (AllFields) {
                        row.ForEach(v => { fieldValues.Add(v.Value); });
                    } else {
                        Fields.ForEach(f => fieldValues.Add(row[f].Value));
                    }
                    values.Add(fieldValues);
                }
            }
#if DEBUG
            // Console.WriteLine("{0} lines selected", values.Count);
#endif
            if (!Silent) {
                if (tables.Count > 0) {
                    Console.WriteLine("{0}:", tables[0]);
                }
                Values.ForEach(r => {
                    Console.WriteLine(string.Join(",", r));
                });
            }
        }
    }
}

