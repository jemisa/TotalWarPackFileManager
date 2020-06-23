using System;
using System.IO;
using System.Collections.Generic;
using Common;

namespace Filetypes {
    public class GroupformationCodec : Codec<GroupformationFile> {

        public GroupformationFile Decode(Stream stream) {
            List<Groupformation> formations;
            using (BinaryReader reader = new BinaryReader(stream)) {
                uint formationCount = reader.ReadUInt32();
                formations = new List<Groupformation>((int) formationCount);
                for (int j = 0; j < formationCount; j++) {
                    Groupformation formation = new Groupformation();
                    formation.Name = IOFunctions.ReadCAString(reader);
                    formation.Priority = reader.ReadSingle();
                    formation.Purpose = reader.ReadUInt32();
                    // Console.WriteLine("reading formation {0}, purpose {1}", formation.Name, formation.Purpose);
                    formation.Minima = ReadList<Minimum>(reader, ReadMinimum);
                    formation.Factions = ReadList<string>(reader, IOFunctions.ReadCAString);
                    formation.Lines = ReadList<Line>(reader, ReadLine);
                    formations.Add(formation);
                }
            }
            GroupformationFile formationFile = new GroupformationFile { Formations = formations };
            return formationFile;
        }

        public void Encode(Stream encodeTo, GroupformationFile file) {
            Console.WriteLine("encoding formation file");
            using (BinaryWriter writer = new BinaryWriter(encodeTo)) {
                writer.Write ((uint) file.Formations.Count);
                foreach(Groupformation formation in file.Formations) {
                    IOFunctions.WriteCAString(writer, formation.Name);
                    writer.Write (formation.Priority);
                    writer.Write((uint) formation.Purpose);
                    WriteList(writer, formation.Minima, WriteMinimum);
                    WriteList(writer, formation.Factions, IOFunctions.WriteCAString);
                    WriteList(writer, formation.Lines, WriteLine);
                    ///////
                }
            }
        }
        
        #region List Read/Write Helpers
        delegate T ItemReader<T>(BinaryReader reader);
        delegate void ItemWriter<T>(BinaryWriter writer, T toWrite);
        List<T> ReadList<T>(BinaryReader reader, ItemReader<T> readItem) {
            List<T> list = new List<T>();
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++) {
                list.Add(readItem(reader));
            }
            return list;
        }
        void WriteList<T>(BinaryWriter writer, List<T> items, ItemWriter<T> writeItem) {
            writer.Write(items.Count);
            foreach(T item in items) {
                writeItem(writer, item);
            }
        }
        #endregion
  
        #region Read/Write Minimum
        Minimum ReadMinimum(BinaryReader reader) {
            int unitClassIndex = reader.ReadInt32();
            Minimum result = new Minimum {
                Percent = reader.ReadUInt32() 
            };
            result.UnitClass.ClassIndex = unitClassIndex;
            return result;
        }
        void WriteMinimum(BinaryWriter writer, Minimum minimum) {
            writer.Write(minimum.UnitClass.ClassIndex);
            writer.Write(minimum.Percent);
        }
        #endregion

        #region Read/Write Priority-Class Pairs
        PriorityClassPair ReadPriorityClassPair(BinaryReader reader) {
            PriorityClassPair pair = new PriorityClassPair { Priority = reader.ReadSingle() };
            pair.UnitClass.ClassIndex = reader.ReadInt32();
            return pair;
        }
        void WritePriorityClassPair(BinaryWriter writer, PriorityClassPair pair) {
            writer.Write(pair.Priority);
            writer.Write(pair.UnitClass.ClassIndex);
        }
        #endregion

        #region Read/Write Lines
        List<Line> ReadLines(BinaryReader reader) {
            int lineCount = reader.ReadInt32();
            List<Line> result = new List<Line>(lineCount);
            for (int i = 0; i < lineCount; i++) {
                result.Add(ReadLine(reader));
            }
            return result;
        }
        public void WriteLines(BinaryWriter writer, List<Line> lines) {
            for(int i = 0; i < lines.Count; i++) {
                writer.Write(i == 0 ? lines.Count : i);
                WriteLine(writer, lines[i]);
            }
        }

        Line ReadLine(BinaryReader reader) {
            Line line;
            int id = reader.ReadInt32();
            LineType lineType = (LineType) reader.ReadUInt32();
            //Console.WriteLine("reading line type {0} at {1}", lineType, reader.BaseStream.Position);
            if (lineType == LineType.spanning) {
                line = new SpanningLine {
                    Blocks = ReadList<int>(reader, delegate(BinaryReader r) { return r.ReadInt32(); })
                };
            } else if (lineType == LineType.absolute || lineType == LineType.relative) {
                BasicLine basicLine = (lineType == LineType.absolute) ? new BasicLine() : new RelativeLine();
                basicLine.Priority = reader.ReadSingle();
                if (lineType == LineType.relative) {
                    (basicLine as RelativeLine).RelativeTo = reader.ReadUInt32();
                }
                basicLine.Shape = reader.ReadInt32();
                basicLine.Spacing = reader.ReadSingle();
                basicLine.Crescent_Y_Offset = reader.ReadSingle();
                basicLine.X = reader.ReadSingle();
                basicLine.Y = reader.ReadSingle();
                basicLine.MinThreshold = reader.ReadInt32();
                basicLine.MaxThreshold = reader.ReadInt32();
                basicLine.PriorityClassPairs = ReadList<PriorityClassPair>(reader, ReadPriorityClassPair);
                line = basicLine;
            } else {
                throw new InvalidDataException("unknown line type " + lineType);
            }
            line.Id = id;
            return line;
        }

        void WriteLine(BinaryWriter writer, Line line) {
            writer.Write(line.Id);
            writer.Write((uint) line.Type);
            if (line.Type == LineType.spanning) {
                WriteList<int>(writer, (line as SpanningLine).Blocks, delegate(BinaryWriter w, int u) { w.Write(u); });
            } else {
                BasicLine basicLine = line as BasicLine;
                writer.Write(basicLine.Priority);
                if (basicLine is RelativeLine) {
                    writer.Write((basicLine as RelativeLine).RelativeTo);
                }
                writer.Write(basicLine.Shape);
                writer.Write(basicLine.Spacing);
                writer.Write(basicLine.Crescent_Y_Offset);
                writer.Write(basicLine.X);
                writer.Write(basicLine.Y);
                writer.Write(basicLine.MinThreshold);
                writer.Write(basicLine.MaxThreshold);
                WriteList(writer, basicLine.PriorityClassPairs, WritePriorityClassPair);
            }
        }
        #endregion
    }

    public class GroupformationFile {
        public List<Groupformation> Formations {
            get; set;
        }
        public override string ToString() {
            return string.Format("[GroupformationFile: {0} formations]", Formations.Count);
        }
    }

    public class Groupformation {
        public Groupformation() {
            Lines = new List<Line>();
            Lines.Add(new BasicLine());
            Minima = new List<Minimum>();
            Factions = new List<string>();
        }
        
        public String Name { get; set; }
        float priority;
        public float Priority { 
            get { 
                return priority; 
                // Console.WriteLine("retrieving priority of {0}", Name);
            }
            set { 
                priority = value;
                // Console.WriteLine("changing priority of {1} to {0}", value, Name);
            }
        }
        public uint Purpose { get; set; }
        public List<Minimum> Minima { get; set; }
        public List<string> Factions { get; set; }
        public int Unknown1 { get; set; }
        public int Unknown2 { get; set; }
        public List<Line> Lines { get; set; }
    }

    public enum Purpose {
      attack = 1,            // 0000.0001
      defend = 2,            // 0000.0010
      // attack || defend
      attack_defend = 3,     // 0000.0011
      river_attack = 4,      // 0000.0100
      naval = 96,            // 0110.0000
      // 128 || attack
      attack_ = 129,         // 1000.0001
      // 128 || defend
      defend_ = 130,         // 1000.0010
      // 128 || attack || defend
      attack_defend_ = 131,  // 1000.0011
      ambush_ = 132,         // 1000.0100
      naval_ = 352 // 0000.0001 0110.0000
    }
    
    public class UnitClass {
        string[] nameReference;
        public int ClassIndex { get; set; }
        public UnitClass(string[] referenceArray) {
            nameReference = referenceArray;
        }
        public string ClassName {
            get { 
                string name = 
                    ClassIndex < nameReference.Length 
                    ? nameReference[ClassIndex] 
                    : "unknown";
                return string.Format("{0} ({1})", ClassIndex, name);
            }
            set { 
                for(int i = 0; i < nameReference.Length; i++) {
                    if (nameReference[i].Equals(value)) {
                        ClassIndex = i;
                        return;
                    }
                }
                throw new InvalidDataException("Not a valid unit class: " + value);
            }
        }
    }

    public class PriorityClassPair {
        public float Priority { get; set; }
        private UnitClass unitClass = new UnitClass(UnitClasses.CLASSES);
        public UnitClass UnitClass { 
            get { return unitClass; }
        }
        public string Display { get { return string.Format("{0} - {1}", Priority, UnitClass.ClassName); }}
    }

    public class Minimum {
        public uint Percent { get; set; }
        private UnitClass unitClass = new UnitClass(UnitClasses.CLASSES2);
        public UnitClass UnitClass {
            get { return unitClass; }
        }
    }
    public enum LineType {
        absolute = 0, relative = 1, spanning = 3
    }

    #region Lines
    public abstract class Line {
        public LineType Type { get; private set; }
        public Line(LineType type) { Type = type; }
        public int Id { get; set; }
        public virtual string Display {
            get { return string.Format("{0} - {1}", Id, Type); }
        }
    }
    public class BasicLine : Line {
        static readonly string[] SHAPES = new string[]{
            "line", "column", "crescent front", "crescent back"
        };
        public BasicLine() : this(LineType.absolute) {}
        protected BasicLine(LineType type) : base(type) {
            PriorityClassPairs = new List<PriorityClassPair>();
        }
        public override string Display {
            get {
                return string.Format("{0} ({1})", base.Display, ShapeName);
            }
        }
        float priority;
        public float Priority { 
            get { 
                return priority; 
//                Console.WriteLine("retrieving priority of {0}", Id);
            }
            set { 
                // Console.WriteLine("changing priority of {1} from {2} to {0}", value, Id, priority);
                priority = value;
            }
        }
        public int Shape { get; set; }
        public string ShapeName { 
            get { 
                return Shape < SHAPES.Length ? SHAPES[Shape] : "unknown";
                // return string.Format("{0} ({1})", Shape, name);
            }
        }
        public float Spacing { get; set; }
        public float Crescent_Y_Offset { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int MinThreshold { get; set; }
        public int MaxThreshold { get; set; }
        public List<PriorityClassPair> PriorityClassPairs { get; set; }
    }
    public class RelativeLine : BasicLine {
        public RelativeLine() : base(LineType.relative) {}
        public uint RelativeTo { get; set; }
    }
    public class SpanningLine : Line {
        public SpanningLine() : base (LineType.spanning) {
            Blocks = new List<int>();
        }
        public List<int> Blocks { get; set; }
    }
    #endregion

    public class UnitClasses {
        public static string[] CLASSES = {
            "artillery_fixed",
            "artillery_foot",
            "artillery_horse",
            "cavalry_camels",
            "cavalry_heavy",
            "cavalry_irregular",
            "cavalry_lancers",
            "cavalry_light",
            "cavalry_missile",
            "cavalry_standard",
            "dragoons",
            "elephants",
            "general",
            "infantry_berserker",
            "infantry_elite",
            "infantry_grenadiers",
            "infantry_irregulars",
            "infantry_light",
            "infantry_line",
            "infantry_melee",
            "infantry_militia",
            "infantry_mob",
            "infantry_skirmishers",
            "naval_admiral",
            "naval_bomb_ketch",
            "naval_brig",
            "naval_dhow",
            "naval_fifth_rate",
            "naval_first_rate",
            "naval_fourth_rate",
            "naval_heavy_galley",
            "naval_indiaman",
            "naval_light_galley",
            "naval_lugger",
            "naval_medium_galley",
            "naval_over_first_rate",
            "naval_razee",
            "naval_rocket_ship",
            "naval_second_rate",
            "naval_sixth_rate",
            "naval_sloop",
            "naval_steam_ship",
            "naval_third_rate",
            "naval_xebec",
            "naval_transport",
            "infantry_spearman",
            "infantry_heavy",
            "infantry_special",
            "infantry_bow",
            "infantry_matchlock",
            "infantry_sword",
            "siege",
            "cavalry_sword",
            "cavalry_lancer",
            "naval_heavy_ship",
            "naval_medium_ship",
            "naval_light_ship",
            "naval_cannon_ship",
            "naval_galleon",
            "naval_trade_ship "
        };
        
        public static string[] CLASSES2 = {
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "unknown",
            "naval_heavy_ship",
            "naval_medium_ship",
            "naval_light_ship"
        };
    }
}

