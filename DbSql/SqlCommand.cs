using Common;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;

namespace DbSql {
    public abstract class SqlCommand {
        protected bool AllTables { get; set; }
        // the pack file to store upon commit
        public PackFile SaveTo { get; set; }
        
        /*
         * Source Packed Files.
         */
        private IEnumerable<PackedFile> allPackedFiles;
        public virtual IEnumerable<PackedFile> PackedFiles { 
            protected get {
                return Filter (allPackedFiles);
            }
            set {
                allPackedFiles = value;
            }
        }
        /*
         * Utility method to retrieve all packed files from the given enumerable
         * fitting the set table name.
         */
        protected IEnumerable<PackedFile> Filter(IEnumerable<PackedFile> files) {
            List<PackedFile> filtered = new List<PackedFile>();
            if (files == null) {
                return filtered;
            }
            foreach(PackedFile file in files) {
                if (!file.FullPath.StartsWith("db")) {
                    continue;
                }
                string tableType = DBFile.Typename(file.FullPath);
                if (AllTables || tables.Contains(tableType)) {
                    filtered.Add(file);
                }
            }
            return filtered;
        }
        /*
         * Retrieve the matching packed files decoded as db files.
         */
        protected IEnumerable<DBFile> DbFiles {
            get {
                List<DBFile> result = new List<DBFile>();
                foreach(PackedFile packed in PackedFiles) {
                    try {
                        result.Add(PackedFileDbCodec.Decode(packed));
                    } catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }
                return result;
            }
        }
        
        protected List<string> tables = new List<string>();
        public abstract void Execute();
  
        /*
         * Fill tables list from given string (usually a regex group match).
         */
        protected void ParseTables(string parse) {
            foreach (string table in parse.Split(',')) {
                if (!string.IsNullOrEmpty(table.Trim())) {
                    tables.Add (table.Trim());
                }
            }
        }
    }
    
    /*
     * Subclass only working on certain fields within a table.
     */
    public abstract class FieldCommand : SqlCommand {
        private List<string> fields = new List<string>();
        public List<string> Fields { 
            get { 
                return fields;
            }
            set { 
                fields.Clear();
                fields.AddRange(value);
            }
        }
        protected bool AllFields {
            get {
                return fields.Count == 1 && fields[0].Equals("*");
            }
        }
        protected void ParseFields(string parse) {
            foreach (string field in parse.Split(',')) {
                fields.Add(field.Trim());
            }
        }
    }
}

