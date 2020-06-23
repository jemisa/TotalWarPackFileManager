using Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    public class DeleteTableCommand : SqlCommand {
        public static Regex DELETE_TABLE_RE = new Regex("delete table (.*)");
        public DeleteTableCommand (string toParse) {
            Match match = DELETE_TABLE_RE.Match(toParse);
            ParseTables (match.Groups[1].Value);
        }
        
        public override void Execute() {
            foreach(PackedFile packed in PackedFiles) {
                packed.Deleted = true;
            }
        }
    }
}

