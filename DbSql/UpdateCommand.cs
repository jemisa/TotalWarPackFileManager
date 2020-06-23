using Common;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    /*
     * Update data for rows in a table.
     */
    public class UpdateCommand : FieldCommand {
        // form of the update command.
        // the part after the "set" contains comma-separated key=value pairs, with the key being the field name
        public static Regex UPDATE_RE = new Regex("update (.*) set (.*)( where .*)", RegexOptions.RightToLeft);

        private WhereClause whereClause;
        
        List<string> assignedValues = new List<string>();
  
        /*
         * Parse the given string to create an update command.
         */
        public UpdateCommand(string toParse) {
            Match m = UPDATE_RE.Match(toParse);
            ParseTables(m.Groups[1].Value);
            foreach(string fieldAssignment in m.Groups[2].Value.Split(',')) {
                string[] assignment = fieldAssignment.Split('=');
                Fields.Add(assignment[0].Trim());
                assignedValues.Add(assignment[1].Trim());
            }
            if (m.Groups.Count > 3) {
                whereClause = new WhereClause(m.Groups[3].Value);
            }
        }
        /*
         * Select all rows matching the where clause (or all in none was given)
         * and set the given values to all corresponding fields.
         * Note: If the assignment list contains a non-existing field,
         * that assignment is ignored without warning.
         */
        public override void Execute() {
            foreach(PackedFile packed in PackedFiles) {
                DBFile dbFile = PackedFileDbCodec.Decode(packed);
                foreach(List<FieldInstance> fieldInstance in dbFile.Entries) {
                    if (whereClause != null && !whereClause.Accept(fieldInstance)) {
                        continue;
                    }
                    AdjustValues(fieldInstance);
                }
                packed.Data = PackedFileDbCodec.GetCodec(packed).Encode(dbFile);
            }
        }
        /*
         * Set the given values to the appropriate fields for the given list.
         */
        private void AdjustValues(List<FieldInstance> fields) {
            foreach(FieldInstance field in fields) {
                if (Fields.Contains(field.Info.Name)) {
                    int index = Fields.IndexOf(field.Info.Name);
                    field.Value = assignedValues[index];
                }
            }
        }
    }
}