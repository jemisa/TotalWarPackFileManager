using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Filetypes.RigidModel
{
    public class LodModel
    {
        public GroupTypeEnum GroupType { get; set; }

        List<Bone> Bones { get; set; } = new List<Bone>();
        public uint BoneCount { get; set; }
        public uint vertexCount { get; set; }
        public uint faceCount { get; set; }
        public uint vertexType { get; set; }
        public string materiaType { get; set; }
        public string modelName { get; set; }
        public string textureDirectory { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public uint materialCount { get; set; }
        public List<Material> Materials { get; set; } = new List<Material>();

        public Vertex[] VertexArray;
        public ushort[] IndicesBuffer;

        public static LodModel Create(ByteChunk chunk)
        {
            int indexAtStart = chunk.Index;
            chunk.Index = indexAtStart;

            var offset = chunk.Index;
            var lodModel = new LodModel();

            var groupType = chunk.ReadUInt32();
            lodModel.GroupType = (GroupTypeEnum)groupType;

            var uknown0 = chunk.ReadUInt32();

            var VertOffset = chunk.ReadUInt32() + offset;
            lodModel.vertexCount = chunk.ReadUInt32();
            var FaceOffset = chunk.ReadUInt32() + offset;
            lodModel.faceCount = chunk.ReadUInt32();

            // BoundingBox
            lodModel.BoundingBox = BoundingBox.Create(chunk);

            lodModel.materiaType = chunk.ReadFixedLength(30);
            lodModel.vertexType = chunk.ReadUInt32();
            lodModel.modelName = chunk.ReadFixedLength(32);
            lodModel.textureDirectory = chunk.ReadFixedLength(256);

            var uknown1 = chunk.ReadBytes(258); // Contains some transformations
            var pivX = chunk.ReadSingle();
            var pivY = chunk.ReadSingle();
            var pivZ = chunk.ReadSingle();


            var unknown2 = chunk.ReadBytes(152); // Contains some transformations?


            lodModel.BoneCount = chunk.ReadUInt32();
            lodModel.materialCount = chunk.ReadUInt32();

            var uknown3 = chunk.ReadBytes(140);

            for (int i = 0; i < lodModel.BoneCount; i++)
                lodModel.Bones.Add(Bone.Create(chunk));

            for (int i = 0; i < lodModel.materialCount; i++)
                lodModel.Materials.Add(Material.Create(chunk));

            var uknown4 = chunk.ReadBytes(8);
            // last 4 = ?
            /*
             typedef enum DDS_ALPHA_MODE
            {
              DDS_ALPHA_MODE_UNKNOWN       = 0,
              DDS_ALPHA_MODE_STRAIGHT      = 1,
              DDS_ALPHA_MODE_PREMULTIPLIED = 2,
              DDS_ALPHA_MODE_OPAQUE        = 3,
              DDS_ALPHA_MODE_CUSTOM        = 4,
            } DDS_ALPHA_MODE;
             */

            lodModel.VertexArray = CreateVertexArray(chunk, lodModel.vertexCount, lodModel.vertexType);
            lodModel.IndicesBuffer = CreateIndexArray(chunk, (int)lodModel.faceCount);

            return lodModel;
        }

        static ushort[] CreateIndexArray(ByteChunk chunk, int indexCount)
        {
            ushort[] output = new ushort[indexCount];
            for (int i = 0; i < indexCount; i++)
                output[i] = chunk.ReadUShort();
            return output;
        }

        static Vertex[] CreateVertexArray(ByteChunk chunk, uint count,uint vertexType)
        {
            Vertex[] output = new Vertex[count];
            if (vertexType == 196608)
            {
                for (int i = 0; i < count; i++)
                {
                    var bytes = chunk.ReadBytes(28);
                    var subChucnk = new ByteChunk(bytes);
                    var vertex = new Vertex()
                    {
                        X = subChucnk.ReadFloat16(),        // 0
                        Y = subChucnk.ReadFloat16(),        // 2
                        Z = subChucnk.ReadFloat16(),        // 4
                    };

                    var u = subChucnk.ReadByte();           // 6
                    var u0 = subChucnk.ReadByte();          // 7      
                    var boneIndex = subChucnk.ReadByte();   // 8
                    var u1 = subChucnk.ReadByte();          // 9
                    var boneWeight0 = subChucnk.ReadByte(); // 10

                    var u2 = subChucnk.ReadByte();          // 11
                    vertex.Normal_X = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //12
                    vertex.Normal_Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //13
                    vertex.Normal_Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //14
                    var u3 = subChucnk.ReadByte();          // 11
                    var uv0 = subChucnk.ReadFloat16();      // 16
                    var u4 = subChucnk.ReadBytes(10);       // 18

                    output[i] = vertex;
                }
            }
            else if (vertexType == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var bytes = chunk.ReadBytes(32);
                    var subChucnk = new ByteChunk(bytes);
                    var vertex = new Vertex()
                    {
                        X = subChucnk.ReadFloat16(),
                        Y = subChucnk.ReadFloat16(),
                        Z = subChucnk.ReadFloat16(),
                    };


                    var u0 = subChucnk.ReadFloat16();       // 4
                    var u1 = subChucnk.ReadFloat16();       // 5
                    var u2 = subChucnk.ReadFloat16();       // 6
                    var u3 = subChucnk.ReadFloat16();       // 7
                    var u4 = subChucnk.ReadFloat16();       // 8

                    var b0 = subChucnk.ReadByte();
                    var t0 = (b0 / 255.0f * 2.0f) - 1.0f;

                    var b1 = subChucnk.ReadByte();
                    var t1 = (b1 / 255.0f * 2.0f) - 1.0f;

                    var b2 = subChucnk.ReadByte();
                    var t2 = (b2 / 255.0f * 2.0f) - 1.0f;

                    vertex.Normal_X = t0;
                    vertex.Normal_Y = t1;
                    vertex.Normal_Z = t2;


                    output[i] = vertex;
                }

            }
            else if (vertexType == 262144)
            {
                for (int i = 0; i < count; i++)
                {
                    var bytes = chunk.ReadBytes(32);
                    var subChucnk = new ByteChunk(bytes);
                    var vertex = new Vertex()
                    {
                        X = subChucnk.ReadFloat16(),
                        Y = subChucnk.ReadFloat16(),
                        Z = subChucnk.ReadFloat16(),
                    };

                    var ukn = subChucnk.ReadFloat16();
                    var bone0 = subChucnk.ReadByte();
                    var bone1 = subChucnk.ReadByte();
                    var bone2 = subChucnk.ReadByte();
                    var bone3 = subChucnk.ReadByte();


                    var weight0 = subChucnk.ReadByte();
                    var weight1 = subChucnk.ReadByte();
                    var weight2 = subChucnk.ReadByte();
                    var weight3 = subChucnk.ReadByte();
        
                    vertex.Normal_X = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;
                    vertex.Normal_Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;
                    vertex.Normal_Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;

                    output[i] = vertex;
                }
            }
            else
                throw new NotImplementedException();


            return output;
        }
    }

    public class Vertex
    {
        public class BoneInfo
        { 
            public byte BoneIndex { get; set; }
            public byte BoneWeight { get; set; }
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float Normal_X { get; set; }
        public float Normal_Y { get; set; }
        public float Normal_Z { get; set; }

        public List<BoneInfo> BoneInfos { get; set; } = new List<BoneInfo>();
    }
}
