using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Coordinates2D = System.Tuple<float, float>;
using Coordinates3D = System.Tuple<float, float, float>;

namespace EsfLibrary {
    public delegate T ValueReader<T>(BinaryReader reader);
    public delegate void ValueWriter<T>(BinaryWriter writer, T value);
    public delegate void NodeWriter(BinaryWriter writer, EsfNode node);

    public class EsfCodecUtil {

        public static EsfCodec GetCodec(Stream stream) {
            EsfCodec codec = null;
            byte[] magicBuffer = new byte[4]; 
            stream.Read(magicBuffer, 0, 4);
            using (BinaryReader reader = new BinaryReader(new MemoryStream(magicBuffer))) {
                uint magic = reader.ReadUInt32();
                codec = CodecFromCode(magic);
            }
            return codec;
        }
        static EsfCodec CodecFromCode(uint code) {
            EsfCodec codec = null;
            switch (code) {
                case 0xABCE:
                    codec = new AbceCodec();
                    break;
                case 0xABCF:
                    codec = new AbcfFileCodec();
                    break;
                case 0xABCA:
                    codec = new AbcaFileCodec();
                    break;
            }
            return codec;
        }
        public static void WriteEsfFile(string filename, EsfFile file) {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename))) {
                file.Codec.EncodeRootNode(writer, file.RootNode);
            }
        }
        public static EsfFile LoadEsfFile(string filename) {
            byte[] fileData = File.ReadAllBytes(filename);
            EsfCodec codec;
            using (var stream = new MemoryStream(fileData)) {
                codec = GetCodec(stream);
            }
            return new EsfFile(codec.Parse(fileData), codec);
        }
        
        public static void LogReadNode(EsfNode node, long position) {
            Console.WriteLine("{1:x}: read {0}", node, position);
        }
    }

    public class EsfFile {
        public EsfNode RootNode {
            get;
            private set;
        }
        public EsfCodec Codec {
            get;
            set;
        }
        public EsfFile(EsfNode rootNode, EsfCodec codec) {
            Codec = codec;
            RootNode = rootNode;
        }
        public EsfFile(Stream stream, EsfCodec codec) {
            using (var reader = new BinaryReader(stream)) {
                Codec = codec;
                RootNode = Codec.Parse(reader);
            }
        }
        public override bool Equals(object obj) {
            bool result = false;
            EsfFile file = obj as EsfFile;
            if (file != null) {
                result = Codec.ID == file.Codec.ID;
                result &= (RootNode as ParentNode).Equals(file.RootNode);
            }
            return result;
        }
        
        public override int GetHashCode() {
            return 2*Codec.ID.GetHashCode() + 3*RootNode.GetHashCode();
        }
    }

    #region Headers
    public class EsfHeader {
        public uint ID { get; set; }
    }
    #endregion

    public abstract class EsfCodec {
        protected byte[] buffer = null;
        
        public EsfCodec(uint id) {
            ID = id;
        }
        
        public readonly uint ID;

        public abstract void WriteHeader(BinaryWriter writer);
        
        public delegate void NodeStarting(byte typeCode, long readerPosition);
        public delegate void NodeRead(EsfNode node, long readerPosition);
        public delegate void WriteLog(string log);
        
        public event NodeStarting NodeReadStarting;
        public event NodeRead NodeReadFinished;
        //public event WriteLog Log;

        protected SortedList<int, string> nodeNames = new SortedList<int, string>();

        public SortedList<int, String> NodeNames
        {
            get
            {
                return nodeNames;
            }
            set
            {
                nodeNames = value;
            }
        }

        #region Reader Methods
        public static bool ReadBool(BinaryReader reader) { return reader.ReadBoolean(); }
        public static sbyte ReadSbyte(BinaryReader reader) { return reader.ReadSByte(); }
        public static short ReadShort(BinaryReader reader) { return reader.ReadInt16(); }
        public static int ReadInt(BinaryReader reader) { return reader.ReadInt32(); }
        public static long ReadLong(BinaryReader reader) { return reader.ReadInt64(); }
        public static byte ReadByte(BinaryReader reader) { return reader.ReadByte(); }
        public static ushort ReadUshort(BinaryReader reader) { return reader.ReadUInt16(); }
        public static uint ReadUInt(BinaryReader reader) { return reader.ReadUInt32(); }
        public static ulong ReadUlong(BinaryReader reader) { return reader.ReadUInt64(); }
        public static float ReadFloat(BinaryReader reader) { return reader.ReadSingle(); }
        public static double ReadDouble(BinaryReader reader) { return reader.ReadDouble(); }
        public static Coordinates2D ReadCoordinates2D(BinaryReader r) { 
            return new Coordinates2D(r.ReadSingle(), r.ReadSingle());
        }
        public static Coordinates3D ReadCoordinates3D(BinaryReader r) {
            return new Coordinates3D(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        }
        // virtual to be able to override
        protected virtual string ReadUtf16String(BinaryReader reader) { return ReadUtf16(reader); }
        protected virtual string ReadAsciiString(BinaryReader reader) { return ReadAscii (reader); }
        #endregion

        #region String Readers/Writers
        public static string ReadUtf16(BinaryReader reader) {
            ushort strLength = reader.ReadUInt16();
            return Encoding.Unicode.GetString(reader.ReadBytes(strLength * 2));
        }
        public static void WriteUtf16(BinaryWriter writer, string toWrite) {
            writer.Write((ushort)toWrite.Length);
            writer.Write(Encoding.Unicode.GetBytes(toWrite));
        }
        public static string ReadAscii(BinaryReader reader) {
            ushort strLength = reader.ReadUInt16();
            return Encoding.ASCII.GetString(reader.ReadBytes(strLength));
        }
        public static void WriteAscii(BinaryWriter writer, string toWrite) {
            writer.Write((ushort)toWrite.Length);
            writer.Write(Encoding.ASCII.GetBytes(toWrite));
        }
        // a string reader looking up a string by its key
        public static string ReadStringReference(BinaryReader reader, Dictionary<string, int> list) {
            int referenceId = reader.ReadInt32();
            string result = null;
            foreach(string key in list.Keys) {
                int candidate = list[key];
                if (candidate == referenceId) {
                    result = key;
                    break;
                }
            }
            return result;
        }
        #endregion

        public EsfHeader Header {
            get;
            set;
        }

        // decodes the full stream, returning the root node
        public virtual EsfNode Parse(byte[] data) {
            return Parse(new MemoryStream(data));
        }
        public virtual EsfNode Parse(Stream stream) {
            MemoryStream bufferStream = stream as MemoryStream;
            if (bufferStream == null) {
                bufferStream = new MemoryStream();
                stream.CopyTo(bufferStream);
            }
            buffer = bufferStream.ToArray();
            using (BinaryReader reader = new BinaryReader(bufferStream)) {
                return Parse(reader);
            }
        }
        public EsfNode Parse(BinaryReader reader) {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Header = ReadHeader(reader);
            uint nodeNameOffset = reader.ReadUInt32();
            long restorePosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(nodeNameOffset, SeekOrigin.Begin);
            ReadNodeNames(reader);
            reader.BaseStream.Seek(restorePosition, SeekOrigin.Begin);
            EsfNode result = Decode(reader);
            result.Codec = this;
            return result;
        }

        public abstract EsfHeader ReadHeader(BinaryReader reader);

        #region General node decoding
        // reads a node from the reader at its current position
        public EsfNode Decode(BinaryReader reader) {
            // read type code
            byte typeCode = reader.ReadByte();
            if (NodeReadStarting != null) {
                NodeReadStarting(typeCode, reader.BaseStream.Position-1);
            }
            EsfNode result = Decode(reader, typeCode);
            if (NodeReadFinished != null) {
                NodeReadFinished(result, reader.BaseStream.Position);
            }
            // Console.WriteLine(string.Format("decoded {0} at {1:x}", result.TypeCode, reader.BaseStream.Position));
            return result;
        }
        public virtual EsfNode Decode(BinaryReader reader, byte code) {
            // create node appropriate to type code
            EsfNode result;
            try {
                long position = reader.BaseStream.Position;
                EsfType typeCode = (EsfType) code;
                // writeDebug = reader.BaseStream.Position > 0xd80000;
                if (code < (byte) EsfType.BOOL_ARRAY) {
                    result = ReadValueNode(reader, typeCode);
                    // if (Log != null) { Log(result.ToXml()); };
                } else if (typeCode < EsfType.RECORD) {
                    result = ReadArrayNode(reader, typeCode);
                } else if (typeCode == EsfType.RECORD) {
                    result = ReadRecordNode(reader, code);
                } else if (typeCode == EsfType.RECORD_BLOCK) {
                    result = ReadRecordArrayNode(reader, code);
                } else {
                    throw new InvalidDataException(string.Format("Type code {0:x} at {1:x} invalid", typeCode, reader.BaseStream.Position - 1));
                }
                // if (Log != null) { Log(string.Format("Read node {0} / {1}", result, result.TypeCode));
                if (NodeReadFinished != null) {
                    NodeReadFinished(result, position);
                }
            } catch (Exception e) {
                Console.WriteLine(string.Format("Exception at {0:x}: {1}", reader.BaseStream.Position, e));
                throw e;
            }
            return result;
        }
        #endregion
        
        public void EncodeRootNode(BinaryWriter writer, EsfNode rootNode) {
            WriteHeader(writer);
            long currentPosition = writer.BaseStream.Position;
            writer.Write(0);
            WriteRecordNode(writer, rootNode);
            // remember the offset of the node name list
            long nodeNamePosition = writer.BaseStream.Position;
            WriteNodeNames(writer);
            writer.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
            writer.Write((uint)nodeNamePosition);
        }

        // encodes the given node
        public void Encode(BinaryWriter writer, EsfNode node) {
            try {
                //NamedNode named = node as NamedNode;
                if (node.TypeCode == EsfType.RECORD_BLOCK_ENTRY) {
                    EncodeSized(writer, (node as ParentNode).AllNodes);
                } else if (node.TypeCode < EsfType.BOOL_ARRAY) {
                    WriteValueNode(writer, node);
                } else if (node.TypeCode < EsfType.RECORD) {
                    WriteArrayNode(writer, node);
                } else if (node.TypeCode == EsfType.RECORD) {
                    WriteRecordNode(writer, node);
                } else if (node.TypeCode == EsfType.RECORD_BLOCK) {
                    WriteRecordArrayNode(writer, node);
                } else {
                    throw new NotImplementedException(string.Format("Cannot write type code {0:x} at {1:x}", node.TypeCode));
                }
            } catch {
                Console.WriteLine(string.Format("failed to write node {0}", node));
                throw;
            }
        }

        #region Value Nodes
        // create
        public virtual EsfNode CreateValueNode(EsfType typeCode, bool optimize = true)
        {
            EsfNode result;
            switch (typeCode) {
            case EsfType.BOOL:
                result = new BoolNode();
                break;
            case EsfType.INT8:
                result = new SByteNode();
                break;
            case EsfType.INT16:
                result = new ShortNode();
                break;
            case EsfType.INT32:
                result = new IntNode();
                break;
            case EsfType.INT64:
                result = new LongNode();
                break;
            case EsfType.UINT8:
                result = new ByteNode();
                break;
            case EsfType.UINT16:
                result = new UShortNode();
                break;
            case EsfType.UINT32:
                result = new UIntNode();
                break;
            case EsfType.UINT64:
                result = new ULongNode();
                break;
            case EsfType.SINGLE:
                result = new FloatNode();
                break;
            case EsfType.DOUBLE:
                result = new DoubleNode();
                break;
            case EsfType.COORD2D:
                result = new Coordinate2DNode();
                break;
            case EsfType.COORD3D:
                result = new Coordinates3DNode();
                break;
            case EsfType.UTF16:
                result = new StringNode(ReadUtf16, WriteUtf16);
                break;
            case EsfType.ASCII:
                result = new StringNode(ReadAscii, WriteAscii);
                break;
            case EsfType.ANGLE:
                result = new UShortNode();
                break;
            //TODO confirm intended data types of types indicated by unknown enum values
            case EsfType.UNKNOWN_23:
                result = new SByteNode();
                break;
            case EsfType.UNKNOWN_24:
                result = new ShortNode();
                break;
            case EsfType.UNKNOWN_26:
                result = new Type26Node();
                break;
            default:
                throw new InvalidDataException(string.Format("Invalid type code {0:x}", typeCode));
            }
            result.TypeCode = typeCode;
            return result;
        }
        // read
        public EsfNode ReadValueNode(BinaryReader reader, EsfType typeCode) {
            try {
                EsfNode result = CreateValueNode(typeCode);
                (result as ICodecNode).Decode(reader, typeCode);
                return result;
            } catch (Exception e) {
                throw new InvalidDataException(string.Format("{0} at {1:x}", e.Message, reader.BaseStream.Position));
            }
        }

        public virtual void WriteValueNode(BinaryWriter writer, EsfNode node) {
            if (node is ICodecNode) {
                (node as ICodecNode).Encode(writer);
            } else {
                throw new InvalidDataException(string.Format("Invalid type code {0:x} at {1:x}", node.TypeCode, writer.BaseStream.Position));
            }
        }
        #endregion

        #region Array Nodes
        protected virtual EsfNode CreateArrayNode(EsfType typeCode) {
            EsfNode result = null;
            switch (typeCode) {
            case EsfType.BOOL_ARRAY:
                result = new EsfArrayNode<bool>(this, typeCode);
                    // CreateArrayNode<bool>(reader), ItemReader = ReadBool);
                break;
            case EsfType.INT8_ARRAY:
                result = new EsfArrayNode<sbyte>(this, typeCode);
                break;
            case EsfType.INT16_ARRAY:
                result = new EsfArrayNode<short>(this, typeCode);
                break;
            case EsfType.INT32_ARRAY:
                result = new EsfArrayNode<int>(this, typeCode);
                break;
            case EsfType.INT64_ARRAY:
                result = new EsfArrayNode<long>(this, typeCode);
                break;
            case EsfType.UINT8_ARRAY:
                result = new EsfArrayNode<byte>(this, typeCode);
                break;
            case EsfType.UINT16_ARRAY:
                result = new EsfArrayNode<ushort>(this, typeCode);
                break;
            case EsfType.UINT32_ARRAY:
                result = new EsfArrayNode<uint>(this, typeCode);
                break;
            case EsfType.UINT64_ARRAY:
                result = new EsfArrayNode<ulong>(this, typeCode);
                break;
            case EsfType.SINGLE_ARRAY:
                result = new EsfArrayNode<float>(this, typeCode);
                break;
            case EsfType.DOUBLE_ARRAY:
                result = new EsfArrayNode<double>(this, typeCode);
                break;
            case EsfType.COORD2D_ARRAY:
                result = new EsfArrayNode<Coordinates2D>(this, typeCode) {
                    Separator = "-"
                };
                break;
            case EsfType.COORD3D_ARRAY:
                result = new EsfArrayNode<Coordinates2D>(this, typeCode) {
                    Separator = "-"
                };
                break;
            case EsfType.UTF16_ARRAY:
                result = new EsfArrayNode<string>(this, EsfType.UTF16_ARRAY);
                break;
            case EsfType.ASCII_ARRAY:
                result = new EsfArrayNode<string>(this, EsfType.ASCII_ARRAY);
                break;
            case EsfType.ANGLE_ARRAY:
                result = new EsfArrayNode<ushort>(this, typeCode);
                break;
            default:
                throw new InvalidDataException(string.Format("Unknown array type code {0}", typeCode));
            }
            result.TypeCode = typeCode;
            return result;
        }
        protected virtual EsfNode ReadArrayNode(BinaryReader reader, EsfType typeCode) {
            EsfNode result;
            try {
                result = CreateArrayNode(typeCode);
            } catch (Exception) {
                throw new InvalidDataException(string.Format("{0} at {1:x}", reader.BaseStream.Position));
            }
            (result as ICodecNode).Decode(reader, typeCode);
            return result;
        }
        protected virtual byte[] ReadArray(BinaryReader reader) {
            int size = (int) ReadSize(reader);
            return reader.ReadBytes(size);
        }
        
//        public T[] ReadArrayItems<T>(BinaryReader reader, ValueReader<T> ReadValue) {
//            long targetOffset = ReadSize(reader);
//            List<T> items = new List<T>();
//            while (reader.BaseStream.Position < targetOffset) {
//                items.Add(ReadValue(reader));
//            }
//            return items.ToArray();
//        }

        protected void WriteArrayNode(BinaryWriter writer, EsfNode arrayNode) {
            // writer.Write((byte) arrayNode.TypeCode);
#if DEBUG
            Console.WriteLine("writing array node type {0} at {1}", arrayNode.TypeCode, writer.BaseStream.Position);
#endif
            (arrayNode as ICodecNode).Encode(writer);
        }
//        protected byte[] EncodeArrayNode<T>(T[] array, ValueWriter<T> WriteItem) {
//            byte[] result;
//            MemoryStream bufferStream = new MemoryStream();
//            using (BinaryWriter writer = new BinaryWriter(bufferStream)) {
//                for (int i = 0; i < array.Length; i++) {
//                    WriteItem(writer, array[i]);
//                }
//                result = bufferStream.ToArray();
//            }
//            return result;
//        }
        #endregion

        #region Record Nodes
        // read an identified node from the reader at its current position
        public virtual RecordNode ReadRecordNode(BinaryReader reader, byte typeCode, bool forceDecode = false) {
            RecordNode node;
            if (!forceDecode && buffer != null) {
                node = new MemoryMappedRecordNode(this, buffer, (int) reader.BaseStream.Position);
            } else {
                node = new RecordNode(this, typeCode);
            }
            node.Decode(reader, EsfType.RECORD);
            return node;
        }
        protected virtual void WriteRecordNode(BinaryWriter writer, EsfNode node) {
            RecordNode recordNode = node as RecordNode;
            recordNode.Encode(writer);
        }
        #endregion

        #region Record Block Nodes
        protected virtual EsfNode ReadRecordArrayNode(BinaryReader reader, byte typeCode) {
            RecordArrayNode result = new RecordArrayNode(this);
            result.Decode(reader, EsfType.RECORD_BLOCK);
            return result;
        }

//        protected virtual void WriteRecordArrayNode(BinaryWriter writer, EsfNode node) {
//            NamedNode recordBlockNode = node as NamedNode;
//            ushort nameIndex = (ushort)nodeNames.IndexOfValue(recordBlockNode.Name);
//            WriteRecordInfo(writer, (byte) EsfType.RECORD_BLOCK, nameIndex, recordBlockNode.Version);
//            long beforePosition = writer.BaseStream.Position;
//            writer.Seek(4, SeekOrigin.Current);
//            // write number of entries
//            WriteSize(writer, recordBlockNode.AllNodes.Count);
//            foreach (EsfNode child in recordBlockNode.AllNodes) {
//                EncodeSized(writer, (child as ParentNode).AllNodes);
//            }
////            EncodeSized(writer, recordBlockNode.AllNodes);
//            long afterPosition = writer.BaseStream.Position;
//            writer.BaseStream.Seek(beforePosition, SeekOrigin.Begin);
//            WriteSize(writer, afterPosition);
//            writer.BaseStream.Seek(afterPosition, SeekOrigin.Begin);
//        }
        protected void WriteRecordArrayNode(BinaryWriter writer, EsfNode node) {
            RecordArrayNode recordBlockNode = node as RecordArrayNode;
            if (recordBlockNode != null) {
                recordBlockNode.Encode(writer);
            } else {
                throw new InvalidOperationException();
            }
        }
        #endregion
        
        #region Version-dependent Overridables
        // Reading data
        protected virtual void ReadNodeNames(BinaryReader reader) {
            int count = reader.ReadInt16();
            nodeNames = new SortedList<int, string>(count);
            for (ushort i = 0; i < count; i++) {
                nodeNames.Add(i, ReadAscii(reader));
            }
        }
        protected virtual void WriteNodeNames(BinaryWriter writer) {
            writer.Write((short)nodeNames.Count);
            for (int i = 0; i < nodeNames.Count; i++) {
                WriteAscii(writer, nodeNames[i]);
            }
        }
        
        public virtual int ReadCount(BinaryReader reader) {
            return reader.ReadInt32();
        }
        public virtual int ReadSize(BinaryReader reader) {
            int targetOffset = ReadCount(reader);
            return (int) (targetOffset - reader.BaseStream.Position);
        }
        public virtual void WriteSize(BinaryWriter writer, long size) {
            writer.Write((uint)size);
        }
        public virtual void WriteOffset(BinaryWriter writer, long offset) {
            // write the target offset: position + 4 byte size + byte count
            WriteSize(writer, offset + writer.BaseStream.Position + 4);
        }

        public virtual void ReadRecordInfo(BinaryReader reader, byte typeCode, out string name, out byte version) {
            ushort nameIndex = reader.ReadUInt16();
            name = GetNodeName (nameIndex);
            version = reader.ReadByte();
        }
        public virtual void WriteRecordInfo(BinaryWriter writer, byte typeCode, string name, byte version) {
            writer.Write((byte)typeCode);
            ushort nameIndex = GetNodeNameIndex(name);
            writer.Write(nameIndex);
            writer.Write(version);
        }

        public virtual List<EsfNode> ReadToOffset(BinaryReader reader, long targetOffset) {
            List<EsfNode> result = new List<EsfNode>();

            //FIXME this function can currently read past the target offset.  Below is partial code / psuedocode that may be more accurate but is not being implemented prior to tests providing both a method of confirming this is accurate in case an incorrect assumption was made and a method of finding failed decodes when new data types appear that would otherwise be obvious because they cause exceptions here that would be caught here with the pseudocode.  An initial test has also shown the partial code to generate erroneous entries after the initial failed entry under rare circumstances (ex: A Type26 followed by a boolean when, if the Type26 was parsed correctly, the data interpreted as a boolean would be part of the Type26 and the boolean entry passed even though it wasn't valid either (a value of 20 evaluates to boolean true even though it wouldn't be a saved boolean)).
            //
            //try
            //{
            //    while(reader.BaseStream.Position < targetOffset)
            //        result.Add(Decode(reader));
            //}
            //catch(InvalidDataException e)
            //{
            //    Console.Error.WriteLine("Unparsable data found while reading to offset {0}, generated exception follows:", targetOffset);
            //    Console.Error.WriteLine(e.ToString());
            //    //result.Add(**Undecoded byte display**);
            //    reader.BaseStream.Position = targetOffset;
            //}

            //if(reader.BaseStream.Position != targetOffset)
            //{
            //    Console.Error.WriteLine("Read ran to {0}, which is past past the expected offset {1}", reader.BaseStream.Position, targetOffset);
            //    reader.BaseStream.Position = targetOffset;
            //}

            while(reader.BaseStream.Position < targetOffset)
                result.Add(Decode(reader));
            return result;
        }
        public virtual void EncodeSized(BinaryWriter writer, List<EsfNode> nodes, bool writeCount = false) {
            long sizePosition = writer.BaseStream.Position;
            writer.Seek(4, SeekOrigin.Current);
            if (writeCount) {
                WriteSize(writer, nodes.Count);
            }
            Encode(writer, nodes);
            long positionAfter = writer.BaseStream.Position;
            // go back to before the encoded contents and write the size
            writer.BaseStream.Seek(sizePosition, SeekOrigin.Begin);
            WriteSize(writer, positionAfter);
            writer.BaseStream.Seek(positionAfter, SeekOrigin.Begin);
        }
        public void Encode(BinaryWriter writer, List<EsfNode> nodes) {
            foreach (EsfNode node in nodes) {
                Encode(writer, node);
            }
        }
        #endregion

        #region Helpers
        // helper methods
        public string GetNodeName(ushort nameIndex) {
            // return string.Format("Node with name index {0}", nameIndex);
            try {
                return nodeNames[nameIndex];
            } catch {
                Console.WriteLine(string.Format("Exception: invalid node index {0}", nameIndex));
                throw;
            }
        }
        public ushort GetNodeNameIndex(string nodename) {
            return (ushort) nodeNames.IndexOfValue(nodename);
        }
        public NodeRead CreateEventDelegate() {
            NodeRead result = null;
            if (NodeReadFinished != null) {
                result = delegate(EsfNode node, long position) { NodeReadFinished(node, position); };
            }
            return result;
        }
        #endregion
    }
}

