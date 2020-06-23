using Common;
using System;
using System.Text.RegularExpressions;

namespace DbSql {
    public class AddTableCommand : SqlCommand {
        public static Regex ADD_RE = new Regex("add table (.*)");
        public AddTableCommand (string toParse) {
            Match match = ADD_RE.Match(toParse);
            ParseTables (match.Groups[1].Value);
        }

        public override void Execute() {
            if (SaveTo != null) {
                foreach(PackedFile packed in PackedFiles) {
                    SaveTo.Add (packed, true);
                }
            }
        }
    }
}

