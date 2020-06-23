using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace EsfLibrary {
    // 0x80 - 0x81
    [DebuggerDisplay("ParentNode: {Name}")]
    public abstract class ParentNode : EsfValueNode<List<EsfNode>>, INamedNode {
        public event Modification RenameEvent;

        public ParentNode() : base(new List<EsfNode>()) {
        }
        public ParentNode(byte code) : this() {
            originalCode = code;
        }
        
        string name;
        public string Name {
            get {
                return name;
            }
            set {
                bool different = !value.Equals(name);
                name = value;
                if (different && RenameEvent != null) {
                    RenameEvent(this);
                }
            }
        }
        public byte Version {
            get;
            protected set;
        }
        public int Size {
            get;
            set;
        }
        public string GetName() {
            return Name;
        }
        public override bool Modified {
            get {
                return modified;
            }
            set {
                base.Modified = value;
                if (!Modified) {
                    Value.ForEach(node => node.Modified = false);
                }
            }
        }

        private byte originalCode = 0;
        public byte OriginalTypeCode {
            get {
                return (originalCode == 0) ? (byte) TypeCode : originalCode;
            }
            set {
                originalCode = value;
            }
        }

        public override List<EsfNode> Value {
            get {
                return base.Value;
            }
            set {
                if (!Value.Equals (value)) {
                    // remove references from children
                    Value.ForEach(node => node.Parent = null);
                    base.Value = value;
                    Value.ForEach(node => node.Parent = this);
                    if (!Modified) {
                        Modified = true;
                    } else {
                        RaiseModifiedEvent();
                    }
#if DEBUG
                } else {
                    Console.WriteLine("Same value set, not regarding as modify");
#endif
                }
            }
        }
        public List<EsfNode> AllNodes {
            get {
                return Value;
            }
        }
        public List<ParentNode> Children {
            get {
                List<ParentNode> result = new List<ParentNode>();
                Value.ForEach(node => { if ((node is ParentNode)) result.Add(node as ParentNode); });
                return result;
            }
        }
        public List<EsfNode> Values {
            get {
                List<EsfNode> result = new List<EsfNode>();
                Value.ForEach(node => { if (!(node is ParentNode)) result.Add(node); });
                return result;
            }
        }
  
        public ParentNode this[string key] {
            get {
                ParentNode result = null;
                Children.ForEach(child => { if (child.Name == key) { result = child; return; }});
                if (result == null) {
                    throw new IndexOutOfRangeException(string.Format("Unknown child {0}", key));
                }
                return result;
            }
        }

        public override bool Equals(object obj) {
            bool result = false;
            ParentNode node = obj as ParentNode;
            if (node != null) {
                result = node.Name.Equals(Name);
                result &= node.AllNodes.Count == Value.Count;
                if (result) {
                    for(int i = 0; i < node.AllNodes.Count; i++) {
                        result &= node.AllNodes[i].Equals(Value[i]);
                        if (!result) {
                            break;
                        }
                    }
                }
            }
            if (!result) {
                return false;
            }
            return result;
        }
        public override int GetHashCode() {
            return 2*Name.GetHashCode() + 3*AllNodes.GetHashCode();
        }
        
        public override string ToString() {
            return string.Format("{0} {1}", TypeCode, Name);
        }

        public override void ToXml(TextWriter writer, string indent) {
            writer.WriteLine(string.Format("{2}<{0} name=\"{1}\">", TypeCode, Name, indent));
            string childIndent = indent + " ";
            foreach(ParentNode child in Children) {
                child.ToXml(writer, childIndent);
            }
            foreach(EsfNode val in Values) {
                val.ToXml(writer, childIndent);
//                result += " " + val.ToXml();
            }
            writer.WriteLine(string.Format("{1}</{0}>", TypeCode, indent));
            // return result;
        }
        public virtual string ToXml(bool end) {
            return end ? string.Format("</{0}>", TypeCode) : string.Format("<{0} name=\"{1}\">", TypeCode, Name);
        }

        protected void CopyMembers(ParentNode node) {
            node.Name = Name;
            node.OriginalTypeCode = OriginalTypeCode;
            node.Size = Size;
            node.Version = Version;
            List<EsfNode> nodeCopy = new List<EsfNode>();
            Value.ForEach(n => nodeCopy.Add(n.CreateCopy()));
            node.Value = nodeCopy;
        }
    }

    [DebuggerDisplay("Record: {Name}")]
    public class RecordNode : ParentNode, ICodecNode {
        public RecordNode(EsfCodec codec, byte originalCode = 0) : base(originalCode) {
            Codec = codec;
        }
        public virtual void Encode(BinaryWriter writer) {
            Codec.WriteRecordInfo(writer, (byte)TypeCode, Name, Version);
            Codec.EncodeSized(writer, AllNodes);
        }
        public override EsfType TypeCode {
            get {
                return EsfType.RECORD;
            }
            set {
                // ignore 
            }
        }
        public virtual void Decode(BinaryReader reader, EsfType unused) {
            string outName;
            byte outVersion;
            Codec.ReadRecordInfo(reader, OriginalTypeCode, out outName, out outVersion);
            Name = outName;
            Version = outVersion;
            Size = Codec.ReadSize(reader);
            Value = Codec.ReadToOffset(reader, reader.BaseStream.Position + Size);
        }
        public override string ToString() {
            return Name;
        }

        public override EsfNode CreateCopy() {
            RecordNode node = new RecordNode(Codec, OriginalTypeCode);
            CopyMembers(node);
            return node;
        }
    }

    [DebuggerDisplay("RecordArray: {Name}")]
    public class RecordArrayNode : ParentNode, ICodecNode {
        public override List<EsfNode> Value {
            get {
                return base.Value;
            }
            set {
                base.Value = value;
                for (int i = 0; i < value.Count; i++) {
                    (Value[i] as RecordEntryNode).Name = string.Format("{0} - {1}", Name, i);
                }
            }
        }
        
        public RecordArrayNode(EsfCodec codec, byte originalCode = 0) : base(originalCode) {
            Codec = codec;
        }
        public override EsfType TypeCode {
            get { return EsfType.RECORD_BLOCK; }
            set { }
        }
        public void Decode(BinaryReader reader, EsfType unused) {
            string name;
            byte version;
            Codec.ReadRecordInfo(reader, OriginalTypeCode, out name, out version);
            Name = name;
            Version = version;
            Size = (int) Codec.ReadSize(reader);
            int itemCount = Codec.ReadCount(reader);
            List<EsfNode> containedNodes = new List<EsfNode>(itemCount);
            for (int i = 0; i < itemCount; i++) {
                RecordEntryNode contained = new RecordEntryNode(Codec) {
                    Name = string.Format("{0} - {1}", Name, i),
                    TypeCode = EsfType.RECORD_BLOCK_ENTRY
                };
                contained.Decode(reader, EsfType.RECORD_BLOCK_ENTRY);
                containedNodes.Add(contained);
            }
            Value = containedNodes;
        }
        public void Encode(BinaryWriter writer) {
            Codec.WriteRecordInfo(writer, (byte)TypeCode, Name, Version);
            Codec.EncodeSized(writer, AllNodes, true);
        }

        public override EsfNode CreateCopy() {
            RecordArrayNode node = new RecordArrayNode(Codec, OriginalTypeCode);
            CopyMembers(node);
            return node;
        }
    }

    [DebuggerDisplay("Record Entry: {Name}")]
    public class RecordEntryNode : ParentNode, ICodecNode, INamedNode {
        public RecordEntryNode(EsfCodec codec) {
            TypeCode = EsfType.RECORD_BLOCK_ENTRY;
            Codec = codec;
        }
        public void Encode(BinaryWriter writer) {
            Codec.EncodeSized(writer, AllNodes);
        }
        public void Decode(BinaryReader reader, EsfType unused) {
            Size = (int) Codec.ReadSize(reader);
            Value = Codec.ReadToOffset(reader, reader.BaseStream.Position + Size);
        }
        public override EsfNode CreateCopy() {
            RecordEntryNode node = new RecordEntryNode(Codec);
            CopyMembers(node);
            return node;
        }
    }
}
