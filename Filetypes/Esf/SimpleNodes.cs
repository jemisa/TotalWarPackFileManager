using System;
using Coordinates2D = System.Tuple<float, float>;
using Coordinates3D = System.Tuple<float, float, float>;
using System.IO;

namespace Filetypes
{
    #region Primitive Value Nodes
    public class IntNode : DelegatingDecoderNode<int> {
        public IntNode()
            : base(int.Parse,
                EsfCodec.ReadInt,
                delegate(BinaryWriter writer, int v) { writer.Write(v); }) {
            TypeCode = EsfType.INT32;
        }

        public override EsfNode CreateCopy() {
            return new IntNode {
                Value = this.Value
            };
        }
    }
    public class UIntNode : DelegatingDecoderNode<uint>
    {
        public UIntNode() : base(uint.Parse,
                                 EsfCodec.ReadUInt,
                                 delegate(BinaryWriter writer, uint u) { writer.Write(u); })
        {
            TypeCode = EsfType.UINT32;
        }

        public override EsfNode CreateCopy()
        {
            return new UIntNode
            {
                Value = this.Value,
                Modified = false
            };
        }
    }
    public class BoolNode : DelegatingDecoderNode<bool> {
        public BoolNode()
            : base(bool.Parse,
                EsfCodec.ReadBool,
                delegate(BinaryWriter writer, bool b) { writer.Write(b); }) {
                    TypeCode = EsfType.BOOL;
        }
        public override EsfNode CreateCopy() {
            return new BoolNode {
                Value = this.Value
            };
        }
    }
    public class FloatNode : DelegatingDecoderNode<float> {
        public FloatNode()
            : base(float.Parse,
                EsfCodec.ReadFloat,
                delegate(BinaryWriter writer, float f) { writer.Write(f); }) {
                    TypeCode = EsfType.SINGLE;
        }
        public override EsfNode CreateCopy() {
            return new FloatNode {
                Value = this.Value
            };
        }
    }

    public class ByteNode : DelegatingDecoderNode<byte> {
        public ByteNode()
            : base(byte.Parse,
                EsfCodec.ReadByte,
                delegate(BinaryWriter writer, byte b) { writer.Write(b); }) {
            TypeCode = EsfType.UINT8;
        }
        public override EsfNode CreateCopy() {
            return new ByteNode {
                Value = this.Value
            };
        }
    }
    public class SByteNode : DelegatingDecoderNode<sbyte> {
        public SByteNode() : base(sbyte.Parse,
                EsfCodec.ReadSbyte,
                delegate(BinaryWriter writer, sbyte b) { writer.Write(b); }) {
                    TypeCode = EsfType.INT8;
        }
        public override EsfNode CreateCopy() {
            return new SByteNode {
                Value = this.Value
            };
        }
    }
    public class ShortNode : DelegatingDecoderNode<short> {
        public ShortNode()
            : base(short.Parse,
             EsfCodec.ReadShort,
                delegate(BinaryWriter writer, short b) { writer.Write(b); }) {
                    TypeCode  = EsfType.INT16;
        }
        public override EsfNode CreateCopy() {
            return new ShortNode {
                Value = this.Value
            };
        }
    }
    public class UShortNode : DelegatingDecoderNode<ushort> {
        public UShortNode()
            : base(ushort.Parse,
                EsfCodec.ReadUshort,
                delegate(BinaryWriter writer, ushort b) { writer.Write(b); }) {
                    TypeCode = EsfType.UINT16;
        }
        public override EsfNode CreateCopy() {
            return new UShortNode {
                Value = this.Value
            };
        }
    }
    public class LongNode : DelegatingDecoderNode<long> {
        public LongNode()
            : base(long.Parse,
                EsfCodec.ReadLong,
                delegate(BinaryWriter writer, long b) { writer.Write(b); }) {
                    TypeCode = EsfType.INT64;
        }
        public override EsfNode CreateCopy() {
            return new LongNode {
                Value = this.Value
            };
        }
    }
    public class ULongNode : DelegatingDecoderNode<ulong> {
        public ULongNode()
            : base(ulong.Parse,
                EsfCodec.ReadUlong,
                delegate(BinaryWriter writer, ulong b) { writer.Write(b); }) {
                    TypeCode = EsfType.UINT64;
        }
        public override EsfNode CreateCopy() {
            return new ULongNode {
                Value = this.Value
            };
        }
    }
    public class DoubleNode : DelegatingDecoderNode<double> {
        public DoubleNode()
            : base(double.Parse,
               EsfCodec.ReadDouble,
                delegate(BinaryWriter writer, double b) { writer.Write(b); }) {
                    TypeCode = EsfType.DOUBLE;
        }
        public override EsfNode CreateCopy() {
            return new DoubleNode {
                Value = this.Value
            };
        }
    }
    #endregion

    public class StringNode : DelegatingDecoderNode<string> {
        public StringNode(ValueReader<string> reader, ValueWriter<string> writer)
            : base(delegate(string v) { return v; },
                reader,
                writer) {
        }
        public override EsfNode CreateCopy() {
            return new StringNode(Read, Write) {
                TypeCode = this.TypeCode,
                Value = this.Value
            };
        }
    }

    public class Coordinate2DNode : CodecNode<Coordinates2D> {
        static Coordinates2D Parse(string value) {
            string removedBrackets = value.Substring(1, value.Length - 2);
            string[] coords = removedBrackets.Split(',');
            Console.WriteLine("Trying to parse [{0}] - [{1}]", coords[0].Trim(), coords[1].Trim());
            Coordinates2D result = new Coordinates2D(
                float.Parse(coords[0].Trim()),
                float.Parse(coords[1].Trim())
            );
            return result;
        }
        public Coordinate2DNode() : base(Parse) { 
            TypeCode = EsfType.COORD2D;
        }
        protected override Coordinates2D ReadValue(BinaryReader reader, EsfType readAs) {
            Coordinates2D result = new Coordinates2D(reader.ReadSingle(), reader.ReadSingle());
            return result;
        }
        public override void WriteValue(BinaryWriter writer) {
            writer.Write(Value.Item1);
            writer.Write(Value.Item2);
        }
        public override EsfNode CreateCopy() {
            return new Coordinate2DNode {
                Value = this.Value
            };
        }
    }
    public class Coordinates3DNode : CodecNode<Coordinates3D> {
        static Coordinates3D Parse(string value) {
            string removedBrackets = value.Substring(1, value.Length - 2);
            string[] coords = removedBrackets.Split(',');
            Coordinates3D result = new Coordinates3D(
                float.Parse(coords[0].Trim()),
                float.Parse(coords[1].Trim()),
                float.Parse(coords[2].Trim())
            );
            return result;
        }
        public Coordinates3DNode() : base(Parse) { }
        protected override Coordinates3D ReadValue(BinaryReader reader, EsfType readAs) {
            Coordinates3D result = new Coordinates3D(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            return result;
        }
        public override void WriteValue(BinaryWriter writer) {
            writer.Write(Value.Item1);
            writer.Write(Value.Item2);
            writer.Write(Value.Item3);
        }
        public override EsfNode CreateCopy() {
            return new Coordinates3DNode {
                Value = this.Value
            };
        }
    }
}