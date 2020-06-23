using System;
using System.IO;
using System.Collections.Generic;
using Common;

namespace Filetypes {
    #region Buildings Model
    public class BuildingModelFile {
        DBFileHeader header;
        // TypeInfo info;
        
        List<List<FieldInstance>> fields = new List<List<FieldInstance>>();
        
        public BuildingModelFile(DBFile file) {
            fields = new List<List<FieldInstance>>(file.Entries);
            header = file.Header;
            // info = file.CurrentType;
        }
        
        public DBFileHeader Header {
            get {
                return header;
            }
        }
        
        public List<BuildingModel> Models {
            get {
                List<BuildingModel> models = new List<BuildingModel>();
                fields.ForEach(fl => models.Add(new BuildingModel(fl)));
                return models;
            }
            set {
                fields.Clear();
                value.ForEach(m => fields.Add(m.Fields));
            }
        }
    }
    
    public class BuildingModel {
        List<FieldInstance> fields;
        public BuildingModel() {
            fields = new List<FieldInstance>();
            fields.Add(Types.StringType().CreateInstance());
            fields.Add(Types.StringType().CreateInstance());
            fields.Add(Types.IntType().CreateInstance());
            for (int i = 0; i < 9; i++) {
                fields.Add(Types.SingleType().CreateInstance());
            }
        }

        public BuildingModel(List<FieldInstance> f) {
            fields = f;
        }

        public List<FieldInstance> Fields {
            get {
                return new List<FieldInstance>(fields);
            }
        }
        
        public string Name { 
            get { return fields[0].Value; }
            set { fields[0].Value = value; }
        }
        public string TexturePath { 
            get { return fields[1].Value; }
            set { fields[1].Value = value; }
        }
        public int Unknown { 
            get { return int.Parse(fields[2].Value); }
            set { fields[2].Value = value.ToString(); }
        }
        public List<BuildingModelEntry> Entries {
            get {
                List<BuildingModelEntry> entries = new List<BuildingModelEntry>();
                ListField list = fields[3] as ListField;
                if (list != null) {
                    list.Contained.ForEach(e => entries.Add (new BuildingModelEntry(e)));
                }
                return entries;
            }
        }
    }

    public class BuildingModelEntry {
        List<FieldInstance> fields;
        public BuildingModelEntry(List<FieldInstance> f) {
            fields = f;
        }
        public string Name { 
            get { return fields[0].Value; }
            set { fields[0].Value = value; }
        }
        public int Unknown { 
            get { return int.Parse(fields[1].Value); }
            set { fields[1].Value = value.ToString(); }
        }
        public List<Coordinates> Coordinates {
            get {
                List<Coordinates> coords = new List<Coordinates>();
                for(int i = 0; i < 3; i++) {
                    // starting from 2 because of fields "name" and "unknown"
                    Coordinates coord = new Coordinates(fields, 3*i + 2);
                    coords.Add(coord);
                }
                return coords;
            }
        }
        
        public override string ToString() {
            return string.Format("[BuildingModelEntry: Name={0}, Unknown={1}]", Name, Unknown);
        }
    }
    #endregion

//    #region Building Model Codec
//    /*
//     * Building models codec.
//     */
//    public class BuildingModelCodec : ModelCodec<BuildingModel>  {
//        private static BuildingModelCodec instance = new BuildingModelCodec();
//        public static BuildingModelCodec Instance {
//            get {
//                return instance;
//            }
//        }
//        protected override ModelFile<BuildingModel> CreateFile() {
//            return new BuildingModelFile();
//        }
//
//        public override BuildingModel ReadModel(BinaryReader reader) {
//            BuildingModel result = new BuildingModel {
//                Name = IOFunctions.readCAString(reader),
//                TexturePath = IOFunctions.readCAString(reader),
//                Unknown = reader.ReadInt32()
//            };
//
//            IOFunctions.FillList(result.Entries, ReadBuildingEntry, reader);
//            return result;
//        }
//
//        BuildingModelEntry ReadBuildingEntry(BinaryReader reader) {
//            BuildingModelEntry entry = new BuildingModelEntry {
//                Name = IOFunctions.readCAString(reader),
//                Unknown = reader.ReadInt32()
//            };
//            ReadCoordinates(reader, entry);
//            return entry;
//        }
//        protected override void WriteModel(BinaryWriter writer, BuildingModel model) {
//            IOFunctions.writeCAString(writer, model.Name);
//            IOFunctions.writeCAString(writer, model.TexturePath);
//            writer.Write(model.Unknown);
//        }
//    }
//    #endregion
 
}

