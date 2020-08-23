using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common;

namespace Filetypes
{
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
        public abstract bool TryDecode(BinaryReader reader);

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

        public override bool TryDecode(BinaryReader reader)
        {
            Decode(reader);
            return true;
        }
    }
}

