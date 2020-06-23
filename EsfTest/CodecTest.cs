using EsfLibrary;
using System;
using System.IO;
using System.Collections.Generic;

namespace EsfTest {
    public class CodecTest {
        TestableAbcaCodec codec = new TestableAbcaCodec();
        
        public CodecTest() {
            codec.AddNodeName(0, "root");
            codec.AddNodeName(1, "test");
        }

        public void run() {
            TestEquals();
            TestIntCodec();
            TestUIntCodec();
            TestUIntArrayCodec();
            TestOptimizedIntNode();
            TestOptimizedUIntArrayCodec();
            TestRecordNode();
            TestRecordArrayNode();
            Console.WriteLine("All tests successful");
            Console.ReadKey();
        }
        
        public void TestRecordArrayNode() {
            List<EsfNode> records = new List<EsfNode>();
            for (int i = 0; i < 5; i++) {
                RecordEntryNode entry = new RecordEntryNode(codec) {
                    Name = "test - " + i,
                    Value = createSomeNodes()
                };
                records.Add(entry);
            }
            RecordArrayNode array = new RecordArrayNode(codec, (byte) EsfType.RECORD_BLOCK) {
                Name = "test",
                Value = records
            };
            VerifyEncodeDecode(array, false);
        }
        
        public void TestRecordNode() {
            RecordNode node = new RecordNode(codec, (byte)EsfType.RECORD) {
                Name = "test",
                Value = createSomeNodes()
            };
            VerifyEncodeDecode(node);
        }

        private void VerifyEncodeDecode(EsfNode node, bool withoutCodec = true) {
            if (withoutCodec) {
                // encode only the single node
                ICodecNode codecNode = node as ICodecNode;
                if (codecNode != null) {
                    byte[] bytes;
                    using (var stream = new MemoryStream()) {
                        using (var writer = new BinaryWriter(stream)) {
                            codecNode.Encode(writer);
                        }
                        bytes = stream.ToArray();
                    }
                    EsfNode decoded = node.CreateCopy();
                    ((ICodecNode) decoded).Decode(new BinaryReader(new MemoryStream(bytes, 1, bytes.Length-1)), node.TypeCode);
                    assertEqual(node, decoded);
                }
            }
            
            // now the test with the full file codec
            List<EsfNode> singleNode = new List<EsfNode>();
            singleNode.Add(node);
            VerifyEncodeDecode(singleNode);
        }
        private byte[] Encode(ICodecNode node) {
            byte[] result;
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream)) {
                    node.Encode(writer);
                }
                result = stream.ToArray();
            }
            return result;
        }
        private void VerifyEncodeDecode(List<EsfNode> nodes) {
            RecordNode rootNode = CreateRootNode();
            rootNode.Value = nodes;
            // EsfFile encodedFile = new EsfFile(rootNode, codec);
            MemoryStream stream = new MemoryStream();
            byte[] encoded;
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                codec.EncodeRootNode(writer, rootNode);
                encoded = stream.ToArray();
            }
            EsfNode decoded;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(encoded))) {
                decoded = codec.Parse (reader);
            }
            assertEqual(rootNode, decoded);
            // EsfNode decodedResult = (decoded as ParentNode).AllNodes[0];
            for(int i = 0; i < rootNode.AllNodes.Count; i++) {
                assertEqual(rootNode.AllNodes[i], (decoded as ParentNode).AllNodes[i]);
            }
//            nodes.ForEach(node => {assertEqual(node, decodedResult); });
        }
        
        private RecordNode CreateRootNode() {
            return new RecordNode(codec, (byte) EsfType.RECORD) { Name = "root" };
        }
        private List<EsfNode> createSomeNodes() {
            List<EsfNode> list = new List<EsfNode>();
            list.Add(new UIntNode { Value = 1, Codec = codec, TypeCode = EsfType.UINT32 });
            list.Add(new UIntNode { Value = 2, Codec = codec, TypeCode = EsfType.UINT32 });
            list.Add(new UIntNode { Value = 3, Codec = codec, TypeCode = EsfType.UINT32 });
            list.Add(new UIntNode { Value = 4, Codec = codec, TypeCode = EsfType.UINT32 });
            list.Add(new UIntNode { Value = 5, Codec = codec, TypeCode = EsfType.UINT32 });
            return list;
        }
        
        public void TestUIntArrayCodec() {
            uint[] array = { 0, 1, 2, 3, 4, 5 };
            //List<EsfNode> nodes = new List<EsfNode>();
            EsfArrayNode<uint> node = new EsfArrayNode<uint>(codec, EsfType.UINT32_ARRAY) { 
                Value = array
            };
            VerifyEncodeDecode(node);
        }
        
        public void TestOptimizedUIntArrayCodec() {
            uint[] array = { 0, 1, 2, 3, 4, 5 };
            //List<EsfNode> nodes = new List<EsfNode>();
            EsfArrayNode<uint> node = new OptimizedArrayNode<uint>(codec, EsfType.UINT32_ARRAY, delegate(uint val) {
                return new OptimizedUIntNode {
                    Value = val,
                    SingleByteMin = true
                };
            }) {
                Value = array
            };
            VerifyEncodeDecode(node);
        }
        
        public void TestEquals() {
            EsfNode valueNode = new IntNode { Value = 1 };
            EsfNode valueNode2 = new IntNode { Value = 1 };
            assertEqual(valueNode, valueNode2);
            
            List<EsfNode> nodeList1 = new List<EsfNode>();
            nodeList1.Add(valueNode);
            List<EsfNode> nodeList2 = new List<EsfNode>();
            nodeList2.Add(valueNode);
            RecordNode node = new RecordNode(null) { Name = "name", Value = nodeList1 };
            EsfNode node2 = new RecordNode(null) { Name = "nodename", Value = nodeList2 };
            assertNotEqual(node, node2);
            node = new RecordNode(null) { Name = "nodename", Value = nodeList1 };
            assertEqual(node, node2);
            
            AbcaFileCodec codec = new AbcaFileCodec();
            EsfFile file = new EsfFile(node, codec);
            AbceCodec codec2 = new AbceCodec();
            EsfFile file2 = new EsfFile(node2, codec2);
            assertNotEqual(file, file2);
            file2.Codec = codec;
            assertEqual(file, file2);
        }
        
        private void TestIntNode(int val, EsfType expectedTypeCode = EsfType.INVALID) {
            EsfValueNode<int> node = new OptimizedIntNode { Value = val };
            TestNode(node, expectedTypeCode);
        }
        private void TestUIntNode(uint val, EsfType typeCode = EsfType.UINT32) {
            EsfValueNode<uint> node = new OptimizedUIntNode { Value = val, TypeCode = typeCode };
            TestNode(node);
        }

        private EsfValueNode<T> TestNode<T>(EsfValueNode<T> node, EsfType expectedTypeCode = EsfType.INVALID) {
            byte[] data = encodeNode(node);
            EsfNode decoded = decodeNode(data);
            EsfValueNode<T> node2 = decoded as EsfValueNode<T>;
            assertEqual(node, node2);
            if (expectedTypeCode != EsfType.INVALID) {
                assertEqual(node2.TypeCode, expectedTypeCode);
            }
            encodeNode(node2);
            return node2;
        }
        byte[] encodeNode(EsfNode node) {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                codec.Encode(writer, node);
                return stream.ToArray();
            }
        }
        EsfNode decodeNode(byte[] data) {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data))) {
                return codec.Decode(reader);
            }
        }
        
        #region Asserts
        public void assert(bool toValidate, bool expected) {
            if (toValidate != expected) {
                Console.WriteLine("Validation failed");
                throw new InvalidOperationException("Validation failed");
            }
        }
        public void assertFalse(bool toValidate) {
            assert(toValidate, false);
        }
        public void assertTrue(bool toValidate) {
            assert (toValidate, true);
        }
        public void assertEqual(object o1, object o2) {
            if (!o1.Equals(o2)) {
                Console.WriteLine("Validation failed");
                throw new InvalidOperationException("Validation failed");
            }
        }
        public void assertNotEqual(object o1, object o2) {
            if (o1.Equals(o2)) {
                Console.WriteLine("Validation failed");
                throw new InvalidOperationException("Validation failed");
            }
        }
        #endregion
        
        #region Old Tests
        public void TestIntCodec() {
            TestIntNode(0);
            TestIntNode(1);
            TestIntNode(0x100);
            TestIntNode(181);
            TestIntNode(0x10000);
            TestIntNode(1573280);
            TestIntNode(0x1000000);
            TestIntNode(int.MaxValue);
            TestIntNode(-1, EsfType.INT32_BYTE);
            TestIntNode(-0xff);
            TestIntNode(-0xffff);
            TestIntNode(-0xffffff);
            TestIntNode(-11831522);
            TestIntNode(int.MinValue);
            
            IntNode testNode = new IntNode {
                Value = 17
            };
            VerifyEncodeDecode(testNode);
            
            IntNode node = new IntNode();
            node.FromString("17");
            assertEqual(node, testNode);
        }
        
        public void TestOptimizedIntNode() {
            OptimizedIntNode optimized = new OptimizedIntNode {
                Codec = codec, Value = 5
            };
            OptimizedIntNode other = new OptimizedIntNode {
                Codec = codec, Value = 5
            };
            assertEqual(optimized, other);
            
            RecordNode rootNode = CreateRootNode();
            rootNode.Value = new List<EsfNode>(new EsfNode[] { other });
            //rootNode.AllNodes.Add(other);
            other.FromString("1");
            assertTrue(other.Modified);
            assertTrue(rootNode.Modified);
            assertEqual(other.Value, 1);

            //assertEqual(optimized, other);
        }
        
        public void TestUIntCodec() {
            TestUIntNode(0);
            TestUIntNode(1);
            TestUIntNode(480, EsfType.UINT32_SHORT);
            TestUIntNode(0x100);
            TestUIntNode(0x10000);
            TestUIntNode(0x1000000);
            TestUIntNode(uint.MaxValue);
            
            VerifyEncodeDecode(new UIntNode {
                Value = 17
            });
        }
        #endregion
    }
    
    
    public class TestableAbcaCodec : AbcaFileCodec {
        public void AddNodeName(int index, string name) {
            nodeNames.Add(index, name);
        }
    }
}

