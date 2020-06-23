using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common;
using Filetypes;


namespace DbSql {
    using QueryResult = List<FieldInstance>;

    class MainClass {

        public static void Main(string[] args) {
            Script script = new Script();
            foreach(string arg in args) {
                if (arg.StartsWith("-p")) {
                    script.SourcePack = new PackFileCodec().Open(arg.Substring(2));
                } else if (arg.StartsWith("-tm")) {
                    script.TypeMapFile = arg.Substring(3);
                } else if (arg.StartsWith("-s")) {
                    script.ExecuteLine(arg.Substring(2));
                } else if (arg.StartsWith("-f")) {
                    string file = arg.Substring(2);
                    foreach(string line in File.ReadLines(file)) {
                        script.ExecuteLine(line);
                    }
                } else if (arg.StartsWith("-c")) {
                    script.Commit();
                }
            }
        }
    }
    
    public class Script {
        public PackFile SourcePack { 
            get; 
            set; 
        }
        private PackFile targetPack;
        public PackFile TargetPack {
            get {
                return targetPack;
            }
            set {
                targetPack = value;
                commands.ForEach(c => { c.SaveTo = targetPack; });
            }
        }
        private List<SqlCommand> commands = new List<SqlCommand>();
        public string TypeMapFile {
            set {
                DBTypeMap.Instance.initializeFromFile(value);
            }
        }
        public void ExecuteLine(string line) {
            if (line.StartsWith("open")) {
                SourcePack = new PackFileCodec().Open(line.Substring(5));
                commands.ForEach(c => { c.PackedFiles = SourcePack; });
            } else if (line.StartsWith("schema")) {
                TypeMapFile = line.Substring(7);
            } else if (line.StartsWith("commit")) {
                Commit();
            } else if (line.StartsWith("save")) {
                string filename = line.Substring(5);
                if (File.Exists(filename)) {
                    TargetPack = new PackFileCodec().Open(filename);
                } else {
                    TargetPack = new PackFile(filename, new PFHeader("PFH4") {
                        Type = PackType.Mod
                    });
                }
            } else if (line.StartsWith("script")) {
                string filename = line.Substring(7);
                if (File.Exists(filename)) {
                    Script included = new Script {
                        SourcePack = this.SourcePack,
                        TargetPack = this.TargetPack
                    };
                    included.commands.AddRange(commands);
                    foreach (string fileLine in File.ReadAllLines(filename)) {
                        included.ExecuteLine(fileLine);
                    }
                    SourcePack = included.SourcePack;
                    TargetPack = included.TargetPack;
                    commands = included.commands;
                }
            } else if (!line.StartsWith("#") && !string.IsNullOrEmpty(line.Trim())) {
                try {
                    SqlCommand command = ParseCommand(line);
                    command.Execute();
                    commands.Add(command);
                } catch (Exception e) {
                    Console.WriteLine("Failed to execute '{0}': {1}", line, e);
                }
            }
        }
        public void Commit() {
            if (TargetPack != null) {
                new PackFileCodec().Save(TargetPack);
            }
        }
        public SqlCommand ParseCommand(string sql) {
            SqlCommand command = null;
            if (InsertCommand.INSERT_RE.IsMatch(sql)) {
                command = new InsertCommand(sql);
            } else if (SelectCommand.SELECT_RE.IsMatch(sql)) {
                command = new SelectCommand(sql);
            } else if (UpdateCommand.UPDATE_RE.IsMatch(sql)) {
                command = new UpdateCommand(sql);
            } else if (DeleteCommand.DELETE_RE.IsMatch(sql)) {
                command = new DeleteCommand(sql);
            } else if (HelpCommand.HELP_RE.IsMatch(sql)) {
                command = new HelpCommand(sql);
            } else if (AddTableCommand.ADD_RE.IsMatch(sql)) {
                command = new AddTableCommand(sql);
            }
            if (command != null) {
                command.PackedFiles = SourcePack;
                command.SaveTo = TargetPack;
            }
            return command;
        }
    }
}
