using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common;

namespace Filetypes {
    /*
     * A single item of data in a db table.
     * These can be encoded/decoded to and from a string.
     */
    [DebuggerDisplay("{Value}:{Info}; ")]
    public abstract class FieldInstance {
        public FieldInstance(FieldInfo fieldInfo, string value = "") {
            Info = fieldInfo;
            Value = value;
        }

        public FieldInfo Info { get; private set; }
        public string Name {
            get { 
                return Info.Name; 
            }
            set {
                Info.Name = value;
            }
        }
  
        /*
         * The encoded value of this instance.
         * Subclasses can override the Value member to provide own decoding/encoding
         * to their actual data type.
         */
        string val;
        public virtual string Value { 
            get { 
                return val; 
            }
            set { val = value; }
        }
        /*
         * Query the decoded data's length in bytes for this field value.
         */
        public virtual int Length {
            get; protected set;
        }
        public virtual int ReadLength {
            get {
                return Length;
            }
        }
        
        /*
         * Only provided in CA xml files, not needed for binary decoding.
         */
        public bool RequiresTranslation { get; set; }
  
        /*
         * Create a copy of this field value.
         */
        public virtual FieldInstance CreateCopy() {
            FieldInstance copy = Info.CreateInstance();
            copy.Value = Value;
            return copy;
        }
        
        public abstract void Encode(BinaryWriter writer);
        public abstract void Decode(BinaryReader reader);
        
        #region Framework Overrides
        public override string ToString() {
            return Value;
        }
        public override bool Equals(object o) {
            bool result = o is FieldInstance;
            if (result) {
                result = Value.Equals ((o as FieldInstance).Value);
            }
            return result;
        }
        public override int GetHashCode() {
            return 2 * Info.GetHashCode () + 3 * Value.GetHashCode ();
        }
        #endregion
    }

    /*
     * String Field.
     */
    public class StringField : FieldInstance {
        protected Encoding stringEncoding = Encoding.Unicode;

        public StringField() : this(Types.StringType()) {}
        public StringField(FieldInfo info) : base(info, "") {}
        public override int Length {
            get {
                return stringEncoding.GetBytes(Value).Length;
            }
        }
        public override int ReadLength {
            get {
                return Length + 2;
            }
        }
        public override void Decode(BinaryReader reader) {
            Value = IOFunctions.ReadCAString (reader, stringEncoding);
        }
        public override void Encode(BinaryWriter writer) {
            IOFunctions.WriteCAString (writer, Value.Trim(), stringEncoding);
        }
    }
    
    /*
     * It's actually StringFieldUTF8, but I'm not going to rename it now.
     */
    public class StringFieldAscii : StringField {
        public StringFieldAscii() : base(Types.StringTypeAscii()) { 
            stringEncoding = Encoding.UTF8;
        }
    }

    /*
     * 4 byte Int Field.
     */
    public class IntField : FieldInstance {
        public IntField() : base(Types.IntType(), "0") { Length = 4; }
        public override void Decode(BinaryReader reader) {
            Value = reader.ReadInt32 ().ToString ();
        }     
        public override void Encode(BinaryWriter writer) {
            writer.Write (int.Parse (Value));
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                base.Value = string.IsNullOrEmpty(value) ? "0" : int.Parse(value).ToString();
            }
        }
    }
    
    /*
     * 2-byte Short Field.
     */
    public class ShortField : FieldInstance {
        public ShortField() : base(Types.ShortType(), "0") { Length = 2;  }
        public override void Decode(BinaryReader reader) {
            Value = reader.ReadUInt16 ().ToString ();
        }
        public override void Encode(BinaryWriter writer) {
            writer.Write (short.Parse (Value));
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                base.Value = short.Parse(value).ToString();
            }
        }
    }
    
    /*
     * Single Field.
     */
    public class SingleField : FieldInstance {
        public SingleField() : base(Types.SingleType(), "0") { Length = 4; }
        public override void Decode(BinaryReader reader) {
            Value = reader.ReadSingle ().ToString ();
        }
        public override void Encode(BinaryWriter writer) {
            writer.Write (float.Parse (Value));
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                base.Value = float.Parse(value).ToString();
            }
        }
    }
    
    public class DoubleField : FieldInstance {
        public DoubleField() : base(Types.DoubleType(), "0") { Length = 8; }
        public override void Decode(BinaryReader reader) {
            Value = reader.ReadDouble ().ToString ();
        }
        public override void Encode(BinaryWriter writer) {
            writer.Write (double.Parse (Value));
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                base.Value = double.Parse(value).ToString();
            }
        }
    }
    
    /*
     * Bool Field.
     */
    public class BoolField : FieldInstance {
        public BoolField() : base(Types.BoolType(), false.ToString()) { Length = 1; }
        public override void Decode(BinaryReader reader) {
            byte b = reader.ReadByte ();
            if (b == 0 || b == 1) {
                Value = Convert.ToBoolean (b).ToString ();
            } else {
                throw new InvalidDataException("- invalid - ({0:x2})");
            }
        }
        public override void Encode(BinaryWriter writer) {
            writer.Write (bool.Parse(Value));
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                base.Value = bool.Parse(value).ToString();
            }
        }
    }
    
    /*
     * Opt String Field.
     */
    public class OptStringField : FieldInstance {
        private bool readLengthZero = false;
        protected Encoding stringEncoding = Encoding.Unicode;
        public OptStringField() : base(Types.OptStringType()) {}
        public OptStringField(FieldInfo info) : base(info) {}
        public override void Decode(BinaryReader reader) {
            string result = "";
            byte b = reader.ReadByte ();
            if (b == 1) {
                result = IOFunctions.ReadCAString (reader, stringEncoding);
                readLengthZero = result.Length == 0;
            } else if (b != 0) {
                throw new InvalidDataException (string.Format("- invalid - ({0:x2})", b));
            }
            Value = result;
        }

        public override int Length {
            get {
                return stringEncoding.GetBytes(Value).Length;
            }
        }
        public override int ReadLength {
            get {
                if (readLengthZero) {
                    return 3;
                } else {
                    // 1 byte for true/false, two for string length if not empty
                    return base.ReadLength + (Value.Length == 0 ? 1 : 3);
                }
            }
        }
        public override void Encode(BinaryWriter writer) {
            writer.Write (Value.Length > 0);
            if (Value.Length > 0) {
                IOFunctions.WriteCAString (writer, Value.Trim(), stringEncoding);
            }
        }
    }
    public class OptStringFieldAscii : OptStringField {
        public OptStringFieldAscii() : base(Types.OptStringTypeAscii()) { 
            stringEncoding = Encoding.UTF8;
        }
    }

    /*
     * VarByte Field.
     */
    public class VarByteField : FieldInstance {
        public VarByteField() : this(1) {}
        public VarByteField(int len) : base(Types.ByteType()) { Length = len; }
        public override void Decode(BinaryReader reader) {
            if (Length == 0) {
                Value = "";
                return;
            }
            byte[] bytes = reader.ReadBytes (Length);
            StringBuilder result = new StringBuilder (3 * bytes.Length);
            result.Append (string.Format ("{0:x2}", bytes [0]));
            for (int i = 1; i < bytes.Length; i++) {
                result.Append (string.Format (" {0:x2}", bytes [i]));
            }
            base.Value = result.ToString ();
        }
        public override void Encode(BinaryWriter writer) {
            string[] split = Value.Split (' ');
            foreach (string s in split) {
                writer.Write (byte.Parse(s, System.Globalization.NumberStyles.HexNumber));
            }
        }
        public override string Value {
            get {
                return base.Value;
            }
            set {
                if (string.IsNullOrEmpty(value)) {
                    base.Value = "";
                } else {
#if DEBUG
                    Console.WriteLine("parsing '{0}' as byte", value);
#endif
                    StringBuilder result = new StringBuilder(value.Length);
                    string[] split = value.Split(' ');
                    result.Append(string.Format("{0}", byte.Parse(split[0]).ToString()));
                    for(int i = 1; i < split.Length; i++) {
                        result.Append(string.Format(" {0}", byte.Parse(split[1]).ToString("x2")));
                    }
                    base.Value = result.ToString();
                }
            }
        }
    }
 
    
    /*
     * List Field.
     */
    public class ListField : FieldInstance {
        public ListField(ListType type) : base(type) {}
        
        public override string Value {
            get {
                return string.Format("{0} entries, length {1}", contained.Count, Length);
            }
        }
        
        public override int Length {
            get {
                // the item count
                int result = 4;
                // the items' indices, if applicable
                if ((Info as ListType).EncodeItemIndices) {
                    result += contained.Count * 4;
                }
                // added length of all contained items
                contained.ForEach(i => i.ForEach(f => result += f.Length));
                return result;
            }
        }
        
        private List<List<FieldInstance>> contained = new List<List<FieldInstance>>();
        public List<List<FieldInstance>> Contained {
            get {
                return contained;
            }
        }
        
        public ListType ContainerType {
            get {
                return Info as ListType;
            }
        }

        public override FieldInstance CreateCopy() {
            ListField field = new ListField(Info as ListType);
            contained.ForEach(l => {
                List<FieldInstance> clone = new List<FieldInstance>(l.Count);
                l.ForEach(i => clone.Add(i.CreateCopy()));
                field.Contained.Add(clone);
            });
            return field;
        }
        
        public override void Encode(BinaryWriter writer) {
            writer.Write(contained.Count);
            for (int i = 0; i < contained.Count; i++) {
                if (ContainerType.EncodeItemIndices) {
                    writer.Write(i);
                }
                foreach(FieldInstance field in contained[i]) {
                    field.Encode(writer);
                }
            }
        }
        
        public override void Decode(BinaryReader reader) {
            contained.Clear();
            int itemCount = reader.ReadInt32();
            contained.Capacity = itemCount;
            for(int i = 0; i < itemCount; i++) {
                if (ContainerType.EncodeItemIndices) {
                    reader.ReadInt32();
                }
                List<FieldInstance> entry = new List<FieldInstance>(ContainerType.Infos.Count);
                foreach(FieldInfo info in ContainerType.Infos) {
                    FieldInstance field = info.CreateInstance();
                    field.Decode(reader);
                    entry.Add(field);
                }
                contained.Add(entry);
            }
        }
    }
}

