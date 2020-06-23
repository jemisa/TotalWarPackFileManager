using System;
using System.IO;

namespace EsfLibrary {
    public class OptimizedBoolNode : CodecNode<bool> {
        public OptimizedBoolNode() : base(bool.Parse) { }
        public override EsfType TypeCode {
            get {
                return Value ? EsfType.BOOL_TRUE : EsfType.BOOL_FALSE;
            }
            set {
                // ignore
            }
        }
        protected override bool ReadValue(BinaryReader reader, EsfType readAs) {
            bool result;
            switch (readAs) {
                case EsfType.BOOL:
                    result = reader.ReadBoolean();
                    break;
                case EsfType.BOOL_TRUE:
                    result = true;
                    break;
                case EsfType.BOOL_FALSE:
                    result = false;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return result;

        }
        public override void WriteValue(BinaryWriter writer) {
            // the type code already contains the value
        }
        public override EsfNode CreateCopy() {
            return new OptimizedBoolNode {
                Value = this.Value
            };
        }
    }

    public class OptimizedIntNode : CodecNode<int> {
        public OptimizedIntNode() : base(int.Parse) { }
        private EsfType setType;
        public bool SingleByteMin { get; set; }
        public override EsfType TypeCode {
            get {
                if (setType != EsfType.INVALID) {
                    return setType;
                }
                if (Value == int.MinValue) {
                    return EsfType.INT32;
                }
                EsfType result = EsfType.INT32_ZERO;
                // remove sign bit if applicable
                int value = Math.Abs(Value);
                if ((value & 0x7f800000) != 0) {
                    result = EsfType.INT32;
                } else if ((value & 0x7f8000) != 0) {
                    result = EsfType.INT32_24BIT;
                } else if ((value & 0x7f80) != 0 || SingleByteMin) {
                    result = EsfType.INT32_SHORT;
                } else if (value > 0) {
                    result = EsfType.INT32_BYTE;
                }
                return result;
            }
            set {
                setType = value;
            }
        }
        protected override int ReadValue(BinaryReader reader, EsfType readAs) {
            int result;
            switch (readAs) {
                case EsfType.INT32_ZERO:
                    result = 0;
                    break;
                case EsfType.INT32_BYTE:
                    result = reader.ReadSByte();
                    break;
                case EsfType.INT32_SHORT:
                    result = reader.ReadInt16();
                    break;
                case EsfType.INT32_24BIT:
                    result = ReadInt24(reader);
                    break;
                case EsfType.INT32:
                    result = reader.ReadInt32();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return result;
        }
        protected void WriteInt24(BinaryWriter writer) {
            uint write = ((uint)Math.Abs(Value));
            if (Value < 0) {
                uint highBitSet = 0x800000u;
                write = write + highBitSet;
            }
            byte toWrite;
            uint mask = 0xff << 16; // mask highest byte first
            for (int i = 16; i >= 0; i -= 8) {
                // mask byte
                uint masked = mask & write;
                // shift to lowest byte and cut off last byte
                toWrite = (byte)(masked >> i);
                writer.Write(toWrite);
                // mask next byte
                mask = mask >> 8;
            }
        }
        int ReadInt24(BinaryReader reader) {
            int value = reader.ReadByte();
            bool sign = (value & 0x80) != 0;
            value = value & 0x7f;
            for (int i = 0; i < 2; i++) {
                value = (value << 8) + reader.ReadByte();
            }
            if (sign) {
                value = -value;
            }
            return value;
        }
        public override void WriteValue(BinaryWriter writer) {
#if DEBUG
            if (Modified) {
                Console.WriteLine("Writing optimized {0}", Value);
            }
#endif
            switch (TypeCode) {
                case EsfType.INT32_ZERO:
                    break;
                case EsfType.INT32_BYTE:
                    writer.Write((sbyte)Value);
                    break;
                case EsfType.INT32_SHORT:
                    writer.Write((short)Value);
                    break;
                case EsfType.INT32_24BIT:
                    WriteInt24(writer);
                    break;
                case EsfType.INT32:
                    writer.Write(Value);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        public override EsfNode CreateCopy() {
            return new OptimizedIntNode {
                Value = this.Value
            };
        }
    }

    public class OptimizedUIntNode : CodecNode<uint> {
        public OptimizedUIntNode() : base(uint.Parse) { }
        private EsfType setType;
        public bool SingleByteMin { get; set; }
        public override EsfType TypeCode {
            get {
                if (setType != EsfType.INVALID) {
                    return setType;
                }
                EsfType result;
                if (Value > 0xffffff) {
                    result = EsfType.UINT32;
                } else if (Value > 0xffff) {
                    result = EsfType.UINT32_24BIT;
                } else if (Value > 0xff) {
                    result = EsfType.UINT32_SHORT;
                } else if (Value > 1 || SingleByteMin) {
                    result = EsfType.UINT32_BYTE;
                } else {
                    result = (Value == 1) ? EsfType.UINT32_ONE : EsfType.UINT32_ZERO;
                }
                return result;
            }
            set {
                setType = value;
            }
        }
        protected override uint ReadValue(BinaryReader reader, EsfType readAs) {
            uint result;
            switch (readAs) {
                case EsfType.UINT32_ZERO:
                    result = 0;
                    break;
                case EsfType.UINT32_ONE:
                    result = 1;
                    break;
                case EsfType.UINT32_BYTE:
                    result = reader.ReadByte();
                    break;
                case EsfType.UINT32_SHORT:
                    result = reader.ReadUInt16();
                    break;
                case EsfType.UINT32_24BIT:
                    result = ReadUInt24(reader);
                    break;
                case EsfType.UINT32:
                    result = reader.ReadUInt32();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return result;
        }
        protected void WriteUInt24(BinaryWriter writer) {
            byte toWrite;
            uint mask = 0xff << 16; // mask highest byte first
            for (int i = 16; i >= 0; i -= 8) {
                // mask byte
                uint masked = mask & Value;
                // shift to lowest byte and cut off last byte
                toWrite = (byte)(masked >> i);
                writer.Write(toWrite);
                // mask next byte
                mask = mask >> 8;
            }
        }
        uint ReadUInt24(BinaryReader reader) {
            uint value = 0;
            for (int i = 0; i < 3; i++) {
                value = (value << 8) + reader.ReadByte();
            }
            return value;
        }
        public override void WriteValue(BinaryWriter writer) {
            switch (TypeCode) {
                case EsfType.UINT32_ZERO:
                case EsfType.UINT32_ONE:
                    break;
                case EsfType.UINT32_BYTE:
                    writer.Write((byte)Value);
                    break;
                case EsfType.UINT32_SHORT:
                    writer.Write((ushort)Value);
                    break;
                case EsfType.UINT32_24BIT:
                    WriteUInt24(writer);
                    break;
                case EsfType.UINT32:
                    writer.Write(Value);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        public override EsfNode CreateCopy() {
            return new OptimizedUIntNode {
                Value = this.Value
            };
        }
    }

    public class OptimizedFloatNode : CodecNode<float> {
        public OptimizedFloatNode()
            : base(float.Parse) {
        }
        public override EsfType TypeCode {
            get {
                return Value == 0 ? EsfType.SINGLE_ZERO : EsfType.SINGLE;
            }
            set {
                // doing nothing
            }
        }
        protected override float ReadValue(BinaryReader reader, EsfType readAs) {
            return (readAs == EsfType.SINGLE_ZERO) ? 0 : reader.ReadSingle();
        }
        public override void WriteValue(BinaryWriter writer) {
            if (TypeCode != EsfType.SINGLE_ZERO) {
                writer.Write(Value);
            }
        }
        public override EsfNode CreateCopy() {
            return new OptimizedFloatNode {
                Value = this.Value
            };
        }
    }
    
    public class OptimizedArrayNode<T> : EsfArrayNode<T> {
        public delegate EsfValueNode<N> NodeFactory<N>(N val);
        
        NodeFactory<T> CreateNode;
        EsfType optimizedType;
        public override EsfType TypeCode {
            get {
                if (optimizedType == EsfType.INVALID) {
                    // iterate all values and use highest code for all
                    // (highest code has longest encoding)
                    byte maxContained = 0;
                    foreach(T item in Value) {
                        EsfValueNode<T> node = CreateNode(item);
                        maxContained = Math.Max(maxContained, (byte) node.TypeCode);
                    }
                    optimizedType = (EsfType) (maxContained + 0x40);
                }
                return optimizedType;
            }
            set {
                base.TypeCode = value;
            }
        }
        /*
         * Override to invalidate type code after setting a new value.
         */
        public override T[] Value {
            get {
                return base.Value;
            }
            set {
                base.Value = value;
                if (value.Length != 0) {
                    optimizedType = EsfType.INVALID;
                } else {
                    optimizedType = base.TypeCode;
                }
            }
        }

        public OptimizedArrayNode(EsfCodec codec, EsfType typeCode, NodeFactory<T> factory) : base(codec, typeCode) {
            CreateNode = factory;
            optimizedType = typeCode;
        }
    }
}