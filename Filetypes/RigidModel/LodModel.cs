using Common;
using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Filetypes.RigidModel
{


    public enum AlphaMode : UInt32
    {
        Opaque = 0,
        Alpha_Test = 1,
        Alpha_Blend = 2
    };

    public enum VertexFormat : UInt32
    {
        Unknown = 99,
        Default = 0,
        Weighted = 3,
        Cinematic = 4
    };

    public class LodModel
    {
        public GroupTypeEnum GroupType { get; set; }

        public List<Bone> Bones { get; set; } = new List<Bone>();
        public uint BoneCount { get; set; }
        public uint VertexCount { get; set; }
        public uint FaceCount { get; set; }
        public uint VertexFormatValue { get; set; }
        public VertexFormat VertexFormat { get; set; } = VertexFormat.Unknown;
        public string MateriaType { get; set; }
        public string ModelName { get; set; }
        public string TextureDirectory { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public uint MaterialCount { get; set; }
        public List<Material> Materials { get; set; } = new List<Material>();
        public float[] Pivot { get; set; } = new float[4];

        public byte[] Unknown0;
        public byte[] Unknown1;
        public AlphaMode AlphaMode { get; set; }

        public Vertex[] VertexArray;
        public ushort[] IndicesBuffer;

        public static LodModel Create(ByteChunk chunk)
        {
            int indexAtStart = chunk.Index;
            chunk.Index = indexAtStart;

            var offset = chunk.Index;
            var lodModel = new LodModel();

            lodModel.GroupType = (GroupTypeEnum)chunk.ReadUInt32();
            lodModel.Unknown0 = chunk.ReadBytes(4);

            var VertOffset = chunk.ReadUInt32() + offset;
            lodModel.VertexCount = chunk.ReadUInt32();
            var FaceOffset = chunk.ReadUInt32() + offset;
            lodModel.FaceCount = chunk.ReadUInt32();

            // BoundingBox
            lodModel.BoundingBox = BoundingBox.Create(chunk);

            lodModel.MateriaType = Util.SanatizeFixedString(chunk.ReadFixedLength(30));
            var unk = chunk.ReadUShort();
            lodModel.VertexFormatValue = chunk.ReadUShort();
            lodModel.ModelName = Util.SanatizeFixedString(chunk.ReadFixedLength(32));
            lodModel.TextureDirectory = Util.SanatizeFixedString(chunk.ReadFixedLength(256));

            var unknownChunk0 = chunk.ReadBytes(258); // Contains some transformations
            
            lodModel.Pivot[0] = chunk.ReadSingle();
            lodModel.Pivot[1] = chunk.ReadSingle();
            lodModel.Pivot[2] = chunk.ReadSingle();
            lodModel.Pivot[3] = chunk.ReadSingle();

            var unknownChunk1 = chunk.ReadBytes(148); // Contains some transformations?

            lodModel.BoneCount = chunk.ReadUInt32();
            lodModel.MaterialCount = chunk.ReadUInt32();

            var unknownChunk3 = chunk.ReadBytes(140);

            for (int i = 0; i < lodModel.BoneCount; i++)
                lodModel.Bones.Add(Bone.Create(chunk));

            for (int i = 0; i < lodModel.MaterialCount; i++)
                lodModel.Materials.Add(Material.Create(chunk));

            lodModel.Unknown1 = chunk.ReadBytes(4);
            lodModel.AlphaMode = (AlphaMode)chunk.ReadUInt32();

            lodModel.VertexArray = CreateVertexArray(lodModel, chunk, lodModel.VertexCount, lodModel.VertexFormatValue);
            lodModel.IndicesBuffer = CreateIndexArray(chunk, (int)lodModel.FaceCount);

            return lodModel;
        }

        static ushort[] CreateIndexArray(ByteChunk chunk, int indexCount)
        {
            ushort[] output = new ushort[indexCount];
            for (int i = 0; i < indexCount; i++)
                output[i] = chunk.ReadUShort();
            return output;
        }

        static Vertex[] CreateDefaultVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];

            for (int i = 0; i < count; i++)
            {
                var bytes = chunk.ReadBytes(32);
                var subChucnk = new ByteChunk(bytes);
                var vertex = new Vertex();

                vertex.Position.X = subChucnk.ReadFloat16();   //0
                vertex.Position.Y = subChucnk.ReadFloat16();    //2
                vertex.Position.Z = subChucnk.ReadFloat16();    //4

                var u0 = subChucnk.ReadFloat16();       // 6
                vertex.Uv0 = subChucnk.ReadFloat16();       // 8        uv0
                vertex.Uv1 = subChucnk.ReadFloat16();       // 10
                var u3 = subChucnk.ReadFloat16();       // 12
                var u4 = subChucnk.ReadFloat16();       // 14

                var b0 = subChucnk.ReadByte();          //15
                var t0 = (b0 / 255.0f * 2.0f) - 1.0f;

                var b1 = subChucnk.ReadByte();          //16
                var t1 = (b1 / 255.0f * 2.0f) - 1.0f;

                var b2 = subChucnk.ReadByte();          //17
                var t2 = (b2 / 255.0f * 2.0f) - 1.0f;

                vertex.Normal.X = t0;
                vertex.Normal.Y = t1;
                vertex.Normal.Z = t2;


                output[i] = vertex;
            }

            return output;
        }

        static Vertex[] CreateWeighthedVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];
            for (int i = 0; i < count; i++)
            {
                var bytes = chunk.ReadBytes(28);
                var subChucnk = new ByteChunk(bytes);
                var vertex = new Vertex();

                vertex.Position.X = -subChucnk.ReadFloat16();   //0
                vertex.Position.Y = subChucnk.ReadFloat16();    //2
                vertex.Position.Z = subChucnk.ReadFloat16();    //4

                var u = subChucnk.ReadByte();           // 6
                var u0 = subChucnk.ReadByte();          // 7      
                var boneIndex = subChucnk.ReadByte();   // 8
                var u1 = subChucnk.ReadByte();          // 9

                var boneWeight0 = subChucnk.ReadByte(); // 10
                vertex.BoneInfos.Add(new Vertex.BoneInfo() { BoneIndex = boneIndex, BoneWeight = boneWeight0 / 255.0f });

                var u2 = subChucnk.ReadByte();          // 11
                vertex.Normal.X = -((subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f);    //12
                vertex.Normal.Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //13
                vertex.Normal.Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //14
                var u3 = subChucnk.ReadByte();          // 15
                vertex.Uv0 = subChucnk.ReadFloat16();      // 16
                vertex.Uv1 = subChucnk.ReadFloat16();      // 18
                var u4 = subChucnk.ReadBytes(8);       // 20

                output[i] = vertex;
            }
            return output;
  
        }

        static Vertex[] CreateCinematicVertex(ByteChunk chunk, uint count)
        {
            Vertex[] output = new Vertex[count];

                for (int i = 0; i < count; i++)
                {
                    var bytes = chunk.ReadBytes(32);
                    var subChucnk = new ByteChunk(bytes);
                    var vertex = new Vertex();

                    vertex.Position.X = -subChucnk.ReadFloat16();   //0
                    vertex.Position.Y = subChucnk.ReadFloat16();    //2
                    vertex.Position.Z = subChucnk.ReadFloat16();    //4

                    var ukn = subChucnk.ReadFloat16();  //6
                    var bone0 = subChucnk.ReadByte();   //8
                    var bone1 = subChucnk.ReadByte();   //9
                    var bone2 = subChucnk.ReadByte();   //10
                    var bone3 = subChucnk.ReadByte();   //11


                    var weight0 = subChucnk.ReadByte(); //12
                    var weight1 = subChucnk.ReadByte(); //13
                    var weight2 = subChucnk.ReadByte(); //14
                    var weight3 = subChucnk.ReadByte(); //15

                    vertex.Normal.X = -((subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f);    //16
                    vertex.Normal.Y = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //17
                    vertex.Normal.Z = (subChucnk.ReadByte() / 255.0f * 2.0f) - 1.0f;    //18

                    var x = 255.0f;
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone0,
                        BoneWeight = (float)weight0 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone1,
                        BoneWeight = (float)weight1 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone2,
                        BoneWeight = (float)weight2 / x
                    });
                    vertex.BoneInfos.Add(new Vertex.BoneInfo()
                    {
                        BoneIndex = bone3,
                        BoneWeight = (float)weight3 / x
                    });

                    var ukn1 = subChucnk.ReadByte();           // 19
                    vertex.Uv0 = subChucnk.ReadFloat16();      // 20
                    vertex.Uv1 = subChucnk.ReadFloat16();      // 22
                    output[i] = vertex;
                }
            return output;
        }

        static Vertex[] CreateVertexArray(LodModel model, ByteChunk chunk, uint count,uint vertexType)
        {
            switch (vertexType)
            {
                case 0:
                    model.VertexFormat = VertexFormat.Default;
                    return CreateDefaultVertex(chunk, count);
                case 3:
                    model.VertexFormat = VertexFormat.Weighted;
                    return CreateWeighthedVertex(chunk, count);
                case 4:
                    model.VertexFormat = VertexFormat.Cinematic;
                    return CreateCinematicVertex(chunk, count);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    

    public class Vertex
    {
        public class BoneInfo
        { 
            public byte BoneIndex { get; set; }
            public float BoneWeight { get; set; }

            public override string ToString()
            {
                return $"I:{ BoneIndex} - W:{BoneWeight}";
            }
        }

        public class Vector3
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public Vector3(float x = 0, float y = 0, float z = 0)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public Vector3 Position { get; set; } = new Vector3();
        public Vector3 Normal { get; set; } = new Vector3();


        public float Uv0 { get; set; }
        public float Uv1 { get; set; }

        public List<BoneInfo> BoneInfos { get; set; } = new List<BoneInfo>();
    }
}
