using Common;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbSql {
    using RowValues = List<string>;

    public interface IValueSource {
        List<RowValues> Values { get; }
    }
    
    /*
     * SQL command inserting a new row to a table.
     */
    class InsertCommand : SqlCommand {
        // form of the insert command: groups are 1-table to insert into; 2-ValueSource
        public static Regex INSERT_RE = new Regex("insert into (.*) (.*)", RegexOptions.RightToLeft);

        IValueSource Source { get; set; }
        
        public override IEnumerable<PackedFile> PackedFiles {
            protected get {
                return base.PackedFiles;
            }
            set {
                base.PackedFiles = value;
                // if we have a select statement as our value provider,
                // update its source pack too
                SelectCommand selectCommand = Source as SelectCommand;
                if (selectCommand != null) {
                    selectCommand.PackedFiles = value;
                }
            }
        }
        
        /*
         * Parse the given string to create an insert command.
         */
        public InsertCommand (string toParse) {
            Match match = INSERT_RE.Match(toParse);
            foreach (string table in match.Groups[1].Value.Split(',')) {
                tables.Add(table.Trim());
            }
            Source = ParseValueSource (match.Groups[2].Value);
        }
        
        /*
         * Insert the previously given values into the db table.
         * A warning will be printed and no data added if the given data doesn't
         * fit the db file's structure.
         */
        public override void Execute() {
            // insert always into packed files at the save to file
            foreach(PackedFile packed in PackedFiles) {
                // we'll read from packed, but that is in the source pack;
                // get or create the db file in the target pack
                DBFile targetFile = GetTargetFile (packed);
                foreach(RowValues insertValues in Source.Values) {
                    if (targetFile.CurrentType.Fields.Count == insertValues.Count) {
                        DBRow newRow = targetFile.GetNewEntry();
                        for (int i = 0; i < newRow.Count; i++) {
                            newRow[i].Value = insertValues[i];
                        }
                        targetFile.Entries.Add (newRow);
                    } else {
                        Console.WriteLine("Cannot insert: was given {0} values, expecting {1} in {2}", 
                                          insertValues.Count, targetFile.CurrentType.Fields.Count, packed.FullPath);
                        Console.WriteLine("Values: {0}", string.Join(",", insertValues));
                    }
                }
                // encode and store in target pack
                PackedFile newPacked = new PackedFile(packed.FullPath, false);
                newPacked.Data = PackedFileDbCodec.GetCodec(newPacked).Encode(targetFile);
                SaveTo.Add(newPacked, true);
            }
        }
  
        /*
         * Open or create db file in the target pack file.
         */
        DBFile GetTargetFile(PackedFile packed) {
            DBFile targetFile = null;
            PackedFile existingPack = SaveTo[packed.FullPath] as PackedFile;
            if (existingPack != null) {
                targetFile = PackedFileDbCodec.Decode(existingPack);
            }
            if (targetFile == null) {
                DBFile file = PackedFileDbCodec.Decode(packed);
                targetFile = new DBFile(file.Header, file.CurrentType);
            }
            return targetFile;
        }
        
        /*
         * Create a value source from the given string (single row fixed values
         * or select command providing the data).
         */
        IValueSource ParseValueSource(string toParse) {
            IValueSource source = null;
            if (FixedValues.VALUES_RE.IsMatch(toParse)) {
                source = new FixedValues(toParse);
            } else if (SelectCommand.SELECT_RE.IsMatch(toParse)) {
                SelectCommand selectCommand = new SelectCommand(toParse) {
                    PackedFiles = this.PackedFiles,
                    Silent = true
                };
                source = selectCommand;
            }
            return source;
        }
    }
    
    /*
     * A value source providing exactly one row with fixed values;
     * this is parsed from an SQL "values(v1,v2,v3)" expression.
     */
    class FixedValues : IValueSource {
        public static Regex VALUES_RE = new Regex("values *\\(.*\\)");
        
        public List<RowValues> Values { get; private set; }
        
        public FixedValues(string toParse) {
            Values = new List<RowValues>();
            Match match = VALUES_RE.Match(toParse);
            RowValues insertValues = new List<string>();
            foreach(string val in match.Groups[1].Value.Split(',')) {
                insertValues.Add(val.Trim());
            }
            Values.Add(insertValues);
        }
    }
}

