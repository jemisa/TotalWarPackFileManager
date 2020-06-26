using Common;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    /*
     * An SQL command deleting data from a table.
     * Can contain a where clause to selectively delete; will delete all data
     * if no where clause was given.
     */
    public class DeleteCommand : SqlCommand {
        // format of this command
        public static Regex DELETE_RE = new Regex("delete from (.*)(where .*)?", RegexOptions.RightToLeft);
        
        // the where clause
        private WhereClause whereClause;
  
        /*
         * Create delete command from given string.
         */
        public DeleteCommand (string toParse) {
            Match match = DELETE_RE.Match(toParse);
            ParseTables(match.Groups[1].Value);
            if (match.Groups.Count > 2 && !string.IsNullOrEmpty(match.Groups[2].Value)) {
                whereClause = new WhereClause(match.Groups[2].Value);
            }
        }
        
        /*
         * Delete all entries matching the where clause if any was given,
         * or all entries if none was given.
         */
        public override void Execute() {
            if (SaveTo == null) {
                return;
            }
            foreach(PackedFile packed in PackedFiles) {
                PackedFile result = new PackedFile(packed.FullPath, false);
                DBFile dbFile = PackedFileDbCodec.Decode(packed);
                List<DBRow> kept = new List<DBRow>();
                foreach(DBRow field in dbFile.Entries) {
                    if (whereClause != null && !whereClause.Accept(field)) {
                        kept.Add(field);
                    }
                }
                DBFile newDbFile = new DBFile(dbFile.Header, dbFile.CurrentType);
                newDbFile.Entries.AddRange(kept);
                result.Data = PackedFileDbCodec.GetCodec(packed).Encode(newDbFile);
                SaveTo.Add(result, true);
            }
        }
    }
}

