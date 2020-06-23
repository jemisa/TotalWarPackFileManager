using System;
using System.IO;
using System.Collections.Generic;

namespace EsfLibrary {
    public abstract class DelegatingNode : RecordNode {
        protected DelegatingNode(EsfCodec codec, byte originalCode = 0) : base(codec, originalCode) {
        }
        
        public override List<EsfNode> Value {
            get {
                List<EsfNode> values = Decoded.Value;
                return values;
            }
            set {
                Decoded.Value = value;
            }
        }
        protected RecordNode decoded;
        protected RecordNode Decoded {
            get {
                if (decoded == null) {
                    decoded = DecodeDelegate();
                    decoded.Parent = this;
                    decoded.Modified = false;
                    decoded.ModifiedEvent += ModifiedDelegate;
                }
                return decoded;
            }
        }
        public override bool Modified {
            get {
                return base.Modified;
            }
            set {
                // don't set modified if we haven't decoded our delegate yet
                // so we don't decode it now without the value having changed
                if (decoded != null) {
                    base.Modified = value;
                }
            }
        }
        protected abstract RecordNode DecodeDelegate();

        private void ModifiedDelegate(EsfNode node) {
            Modified = node.Modified;
        }
    }
 
    /**
     * A record node that keeps track where in memory it was loaded from
     * and only evaluates that data when its child nodes are requested.
     */
    public class MemoryMappedRecordNode : DelegatingNode {
        byte[] buffer;
        private int mapStart;
        private int byteCount;

        public MemoryMappedRecordNode (EsfCodec codec, byte[] bytes, int start) : base(codec, bytes[start-1]) {
            Codec = codec;
            buffer = bytes;
            mapStart = start-1;
            InvalidateSiblings = true;
            // byteCount = count;
        }
        
        // if this is set, the siblings following this node will also be invalidated
        // when a modification of this node occurs
        public bool InvalidateSiblings {
            get; set;
        }
  
        #region Invalidation
        bool invalid;
        public bool Invalid { 
            get { return invalid; }
            set {
                if (invalid != value) {
                    invalid = value;
                    if (value) {
                        NodeIterator iterator = new ParentIterator {
                            Visit = Invalidate
                        };
                        iterator.Iterate(Parent);
                        
                        if (InvalidateSiblings) {
                            // all nodes after this one have their
                            // addresses shifted
                            iterator = new SiblingIterator {
                                Visit = InvalidateAll
                            };
                            iterator.Iterate (this);

                            // make sure when encoding, we don't get lazy and
                            // copy blocks of invalid data for contained nodes
                            iterator = new ChildIterator {
                                Visit = InvalidateAll
                            };
                            iterator.Iterate(Decoded);
                        }
                    }
                }
            }
        }
        public override bool Modified {
            get {
                return base.Modified;
            }
            set {
                base.Modified = value;
                Invalid |= Modified;
            }
        }

        /*
         * Invalidate until the first memory-mapped node is found.
         */
        private bool Invalidate(EsfNode node) {
            MemoryMappedRecordNode mapped = node as MemoryMappedRecordNode;
            bool continuteIteration = mapped == null;
            if (mapped != null) {
                mapped.Invalid = true;
            }
            // don't continue when a node was invalidated
            return !continuteIteration;
        }
        /*
         * Invalidate all memory-mapped nodes we come across.
         */
        private bool InvalidateAll(EsfNode node) {
            Invalidate(node);
            return true;
        }
        #endregion

        protected override RecordNode DecodeDelegate() {
            RecordNode result;
            using (var reader = new BinaryReader(new MemoryStream(buffer))) {
                reader.BaseStream.Seek(mapStart+1, SeekOrigin.Begin);
                result = Codec.ReadRecordNode(reader, OriginalTypeCode, true);
            }
            return result;
        }

        public override void Decode(BinaryReader reader, EsfType type) {
            // we need to keep track of the original data to give it to the actual parser later
            string name;
            byte remember;
            Codec.ReadRecordInfo(reader, OriginalTypeCode, out name, out remember);
            Name = name;
            Version = remember;
            int size = Codec.ReadSize(reader);
            int infoSize = (int)(reader.BaseStream.Position - mapStart);
            byteCount = size + infoSize;
            reader.BaseStream.Seek(size, SeekOrigin.Current);
        }

        public override void Encode(BinaryWriter writer) {
            if (Invalid) {
                Decoded.Encode(writer);
            } else {
                writer.Write(buffer, mapStart, byteCount);
            }
        }

        public override EsfNode CreateCopy() {
            if (!Invalid) {
                return new MemoryMappedRecordNode(Codec, buffer, mapStart + 1) {
                    Name = this.Name
                };
            } else {
                return Decoded.CreateCopy();
            }
        }
    }
}

