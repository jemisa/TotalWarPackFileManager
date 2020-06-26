using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Coordinates2D = System.Tuple<float, float>;
using Coordinates3D = System.Tuple<float, float, float>;
using System.IO;

namespace Filetypes {
    public interface ICodecNode {
        void Decode(BinaryReader reader, EsfType readAs);
        void Encode(BinaryWriter writer);
    }
    public interface INamedNode {
        string GetName();
    }

    public abstract class EsfNode {
        public delegate void Modification (EsfNode node);
        public event Modification ModifiedEvent;

        public EsfCodec Codec { get; set; }
        public virtual EsfType TypeCode { get; set; }

        #region Properties
        private EsfNode parent;
        public EsfNode Parent { 
            get { return parent; }
            set {
                parent = value;
            }
        }
        public Type SystemType { get; set; }
        
        // property Deleted; also sets Modified
        private bool deleted = false;
        public bool Deleted { 
            get {
                return deleted;
            }
            set {
                deleted = value;
                Modified = true;
            }
        }
        
        // property Modified; also sets parent to new value
        protected bool modified = false;
        public virtual bool Modified {
            get {
                return modified;
            }
            set {
                if (modified != value) {
                    modified = value;
                    RaiseModifiedEvent();
                    if (modified && Parent != null) {
                        Parent.Modified = value;
                    }
                    // Console.WriteLine("{1}setting modified: {0}", this, (modified ? "" : "un"));
                }
            }
        }
        protected void RaiseModifiedEvent() {
            if (ModifiedEvent != null) {
#if DEBUG
                Console.WriteLine("modified: {0}", this);
#endif
                ModifiedEvent(this);
            }
        }
        #endregion
        public virtual void FromString(string value) {
            throw new InvalidOperationException();
        }

        public abstract void ToXml(TextWriter writer, string indent);
        public abstract EsfNode CreateCopy();
    }

    [DebuggerDisplay("ValueNode: {TypeCode}")]
    public class EsfValueNode<T> : EsfNode {
        public EsfValueNode(T value) {
            val = value;
        }
        public delegate S Converter<S>(string value);
        protected Converter<T> ConvertString;
        
        public EsfValueNode() : this (null) {}

        public EsfValueNode(Converter<T> converter) : base() {
            SystemType = typeof(T);
            ConvertString = converter;
        }

        T val;
        public virtual T Value {
            get {
                return val;
            }
            set {
                if (!EqualityComparer<T>.Default.Equals (val, value)) {
                    val = value;
                    Modified = true;
                }
            }
        }

        public override void FromString(string value) {
            Value = ConvertString(value);
        }
        public override string ToString() {
            return val.ToString();
        }
        
        public override bool Equals(object o) {
            bool result = false;
            try {
                T otherValue = (o as EsfValueNode<T>).Value;
                result = (otherValue != null) && EqualityComparer<T>.Default.Equals(val, otherValue);
            } catch {}
            if (!result) {
            }
            return result;
        }
        
        public override EsfNode CreateCopy() {
            return new EsfValueNode<T> {
                TypeCode = this.TypeCode,
                Value = this.Value
            };
        }
        
        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override void ToXml(TextWriter writer, string indent) {
            writer.WriteLine(string.Format("{2}<{0} Value=\"{1}\"/>", TypeCode, Value, indent));
        }
    }

    public abstract class CodecNode<T> : EsfValueNode<T>, ICodecNode {
        public CodecNode(Converter<T> conv) : base(conv) { }
        public void Decode(BinaryReader reader, EsfType readAs) {
            Value = ReadValue(reader, readAs);
        }

        /**
         * <summary>Reads a binary representation of the node's data from the <paramref name="reader"/> and returns a copy of the underlying read data.</summary>
         * <remarks>This does not set the node's data on its own.  If the intent is to set the node's data, that must be manually done after calling this function.</remarks>
         * 
         * <param name="reader">A <see cref="BinaryReader"/> pointed at a copy of this node type's binary-encoded data.</param>
         * <param name="readAs">The <see cref="EsfType"/> of this node.</param>
         * <returns>A copy of this node type's underlying data.</returns>
         */
        protected abstract T ReadValue(BinaryReader reader, EsfType readAs);
        public void Encode(BinaryWriter writer) {
            if (TypeCode == EsfType.INVALID) {
                throw new InvalidDataException("Cannot encode without valid type code");
            }
            writer.Write((byte)TypeCode);
            WriteValue(writer);
        }

        /**
         * <summary>Writes the node's binary-encoded data to a given <see cref="BinaryWriter"/>.</summary>
         * 
         * <param name="writer">The writer that will be writing the node's binary-encoded data.</param>
         */
        public abstract void WriteValue(BinaryWriter writer);
    }

}

