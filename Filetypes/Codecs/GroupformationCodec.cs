using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.Codecs
{
    public class GroupformationCodec : ICodec<GroupformationFile>
    {

        public GroupformationFile Decode(Stream stream)
        {
            List<Groupformation> formations;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                uint formationCount = reader.ReadUInt32();
                formations = new List<Groupformation>((int)formationCount);
                for (int j = 0; j < formationCount; j++)
                {
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

        public void Encode(Stream encodeTo, GroupformationFile file)
        {
            Console.WriteLine("encoding formation file");
            using (BinaryWriter writer = new BinaryWriter(encodeTo))
            {
                writer.Write((uint)file.Formations.Count);
                foreach (Groupformation formation in file.Formations)
                {
                    IOFunctions.WriteCAString(writer, formation.Name);
                    writer.Write(formation.Priority);
                    writer.Write((uint)formation.Purpose);
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
        List<T> ReadList<T>(BinaryReader reader, ItemReader<T> readItem)
        {
            List<T> list = new List<T>();
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                list.Add(readItem(reader));
            }
            return list;
        }
        void WriteList<T>(BinaryWriter writer, List<T> items, ItemWriter<T> writeItem)
        {
            writer.Write(items.Count);
            foreach (T item in items)
            {
                writeItem(writer, item);
            }
        }
        #endregion

        #region Read/Write Minimum
        Minimum ReadMinimum(BinaryReader reader)
        {
            int unitClassIndex = reader.ReadInt32();
            Minimum result = new Minimum
            {
                Percent = reader.ReadUInt32()
            };
            result.UnitClass.ClassIndex = unitClassIndex;
            return result;
        }
        void WriteMinimum(BinaryWriter writer, Minimum minimum)
        {
            writer.Write(minimum.UnitClass.ClassIndex);
            writer.Write(minimum.Percent);
        }
        #endregion

        #region Read/Write Priority-Class Pairs
        PriorityClassPair ReadPriorityClassPair(BinaryReader reader)
        {
            PriorityClassPair pair = new PriorityClassPair { Priority = reader.ReadSingle() };
            pair.UnitClass.ClassIndex = reader.ReadInt32();
            return pair;
        }
        void WritePriorityClassPair(BinaryWriter writer, PriorityClassPair pair)
        {
            writer.Write(pair.Priority);
            writer.Write(pair.UnitClass.ClassIndex);
        }
        #endregion

        #region Read/Write Lines
        List<Line> ReadLines(BinaryReader reader)
        {
            int lineCount = reader.ReadInt32();
            List<Line> result = new List<Line>(lineCount);
            for (int i = 0; i < lineCount; i++)
            {
                result.Add(ReadLine(reader));
            }
            return result;
        }
        public void WriteLines(BinaryWriter writer, List<Line> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                writer.Write(i == 0 ? lines.Count : i);
                WriteLine(writer, lines[i]);
            }
        }

        Line ReadLine(BinaryReader reader)
        {
            Line line;
            int id = reader.ReadInt32();
            LineType lineType = (LineType)reader.ReadUInt32();
            //Console.WriteLine("reading line type {0} at {1}", lineType, reader.BaseStream.Position);
            if (lineType == LineType.spanning)
            {
                line = new SpanningLine
                {
                    Blocks = ReadList<int>(reader, delegate (BinaryReader r) { return r.ReadInt32(); })
                };
            }
            else if (lineType == LineType.absolute || lineType == LineType.relative)
            {
                BasicLine basicLine = (lineType == LineType.absolute) ? new BasicLine() : new RelativeLine();
                basicLine.Priority = reader.ReadSingle();
                if (lineType == LineType.relative)
                {
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
            }
            else
            {
                throw new InvalidDataException("unknown line type " + lineType);
            }
            line.Id = id;
            return line;
        }

        void WriteLine(BinaryWriter writer, Line line)
        {
            writer.Write(line.Id);
            writer.Write((uint)line.Type);
            if (line.Type == LineType.spanning)
            {
                WriteList<int>(writer, (line as SpanningLine).Blocks, delegate (BinaryWriter w, int u) { w.Write(u); });
            }
            else
            {
                BasicLine basicLine = line as BasicLine;
                writer.Write(basicLine.Priority);
                if (basicLine is RelativeLine)
                {
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

}
