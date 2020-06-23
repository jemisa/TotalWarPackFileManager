using System;
using System.IO;
using System.Collections.Generic;

using Coordinates2D = System.Tuple<float, float>;
using Coordinates3D = System.Tuple<float, float, float>;

namespace EsfLibrary {
    public class EsfArrayNode<T> : EsfValueNode<T[]>, ICodecNode {
        public EsfArrayNode(EsfCodec codec, EsfType code) : base(delegate(string s) { throw new InvalidOperationException(); }) {
            Codec = codec;
            Separator = " ";
            TypeCode = code;
            ConvertItem = DefaultFromString;
            Value = new T[0];
        }
        // string to T
        public Converter<T> ConvertItem { get; set; }
        static T DefaultFromString(string toConvert) {
            return (T) Convert.ChangeType(toConvert, typeof(T));
        }
        
        public override EsfNode CreateCopy() {
            return new EsfArrayNode<T>(Codec, TypeCode) {
                TypeCode = this.TypeCode,
                Value = this.Value
            };
        }
        public override void ToXml(TextWriter writer, string indent) {
            writer.WriteLine("{2}<{0} Length=\"{1}\"/>", TypeCode, Value.Length, indent);
        }

        #region ICodecNode Implementation
        protected virtual EsfType ContainedTypeCode {
            get {
                return (EsfType) (TypeCode - 0x40);
            }
        }
        public void Decode(BinaryReader reader, EsfType type) {
            EsfType containedTypeCode = ContainedTypeCode;
#if DEBUG
            // Console.WriteLine("decoding array type code {0} containing {1}", type, containedTypeCode);
#endif

            int size = Codec.ReadSize(reader);
#if DEBUG
            // Console.WriteLine("Reading array[{0}] with {1} elements", type, size);
#endif
            List<T> read = new List<T>();
            using (var itemReader = new BinaryReader(new MemoryStream(reader.ReadBytes(size)))) {
                while (itemReader.BaseStream.Position < size) {
                    read.Add(ReadFromCodec(itemReader, containedTypeCode));
                }
            }
            Value = read.ToArray();
        }
        public void Encode(BinaryWriter writer) {
            EsfType containedTypeCode = ContainedTypeCode;
            EsfType myRealType = (EsfType) (containedTypeCode + 0x40);
#if DEBUG
            Console.WriteLine("encoding array with {2} bytes, code {0} containing {1}", myRealType, containedTypeCode, Value.Length);
#endif
            
            writer.Write ((byte) myRealType);
            byte[] encodedArray = new byte[0];
            using (var stream = new MemoryStream()) {
                using (var memWriter = new BinaryWriter(stream)) {
                    CodecNode<T> valueNode = Codec.CreateValueNode(containedTypeCode, false) as CodecNode<T>;
                    valueNode.TypeCode = containedTypeCode;
                    foreach(T item in Value) {
                        valueNode.Value = item;
                        valueNode.WriteValue(memWriter);
                    }
                    encodedArray = stream.ToArray();
#if DEBUG
                    Console.WriteLine("size is {0}", encodedArray.Length);
#endif
                    Codec.WriteOffset(writer, encodedArray.Length);
                    writer.Write(encodedArray);
                }
            }
        }
        private T ReadFromCodec(BinaryReader reader, EsfType containedTypeCode) {
            EsfValueNode<T> node = (EsfValueNode<T>) Codec.ReadValueNode(reader, containedTypeCode);
            return node.Value;
        }
        #endregion

        public override void FromString(string value) {
#if DEBUG
            Console.WriteLine("decoding {0}", value);
#endif
            string[] elements = value.Split(Separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<T> values = new List<T>(elements.Length);
            foreach(string e in elements) {
                values.Add(ConvertItem(e));
            }
            if (Parent == null) {
                Console.WriteLine("No parent set! I'm sad.");
            }
            Value = values.ToArray() ?? new T[0];
        }

        #region Framework overrides
        public override bool Equals(object o) {
            EsfArrayNode<T> otherNode = o as EsfArrayNode<T>;
            bool result = otherNode != null;
            result &= ArraysEqual(Value, otherNode.Value);
            return result;
        }
        public override int GetHashCode() {
            return Value.GetHashCode();
        }
        public string Separator {
            get; set;
        }
        public override string ToString() {
            string result = "";
            try {
                if (Value != null) {
                    result = string.Join(Separator, Value);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                result = Value.ToString();
                result = string.Format("{0}{1}]", result.Substring(0, result.Length-1), Value.Length);
            }
            // result = string.Format("{0}{1}]", result.Substring(0, result.Length-1), Value.Length);
            return result;
        }
        #endregion

        #region Utility Functions
        static bool ArraysEqual<O> (O[] array1, O[] array2) {
            bool result = array1.Length == array2.Length;
            if (result) {
                for (int i = 0; i < array1.Length; i++) {
                    if (!EqualityComparer<O>.Default.Equals (array1[i], array2[i])) {
                        result = false;
                        break;
                    }
                }
            }
            if (!result) {
            }
            return result;
        }
        static O[] ParseArray<O> (string value, Converter<O> convert) {
            string[] itemStrings = value.Split(' ');
            List<O> result = new List<O>(itemStrings.Length);
            foreach(string s in itemStrings) {
                result.Add(convert(s));
            }
            return result.ToArray();
        }
        #endregion
    }
    
    public class RawDataNode : EsfValueNode<byte[]>, ICodecNode {
        public RawDataNode(EsfCodec codec) : base(delegate(string s) { throw new InvalidOperationException(); }) {
            Codec = codec;
            TypeCode = EsfType.UINT8_ARRAY;
        }
        public override EsfNode CreateCopy() {
            return new RawDataNode(Codec) {
                TypeCode = this.TypeCode,
                Value = this.Value
            };
        }
        public override void ToXml(TextWriter writer, string indent) {
            writer.WriteLine("{2}<{0} Length=\"{1}\"/>", TypeCode, Value.Length, indent);
        }
        #region ICodecNode Implementation
        public void Decode(BinaryReader reader, EsfType type) {
            int size = Codec.ReadSize(reader);
            Value = reader.ReadBytes(size);
        }
        public void Encode(BinaryWriter writer) {
            writer.Write ((byte) TypeCode);
            Codec.WriteOffset(writer, Value.Length);
            writer.Write(Value);
        }
        #endregion

        #region Framework overrides
        public override bool Equals(object o) {
            RawDataNode otherNode = o as RawDataNode;
            bool result = otherNode != null;
            result = result && EqualityComparer<byte[]>.Default.Equals(Value, otherNode.Value);
            return result;
        }
        public override int GetHashCode() {
            return Value.GetHashCode();
        }
        public override string ToString() {
            string result = Value.ToString();
            result = string.Format("{0}{1}]", result.Substring(0, result.Length-1), Value.Length);
            return result;
        }
        #endregion
    }
}

