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

    public class Transformation
    {
        public FileVector3 Pivot { get; set; } = new FileVector3();
        public FileMatrix3x4[] Matrices { get; set; } = new FileMatrix3x4[] { new FileMatrix3x4(), new FileMatrix3x4(), new FileMatrix3x4() };
    }

    public class FileMatrix3x4
    {
        public FileVector4[] Matrix { get; set; } = new FileVector4[3] { new FileVector4(), new FileVector4(), new FileVector4() };

    }

    public class LodModel
    {
        public GroupTypeEnum MaterialId { get; set; }

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

        public Transformation Transformation { get; set; }

        public uint MaterialCount { get; set; }
        public List<Material> Materials { get; set; } = new List<Material>();

        public byte[] Unknown0;
        public byte[] Unknown1;
        public byte[] Unknown2;
        public byte[] Unknown3;
        public byte[] Unknown4;
        public byte[] AlphaKeyValue;
        public byte[] ShaderValues;

        public AlphaMode AlphaMode { get; set; }

        public Vertex[] VertexArray;
        public ushort[] IndicesBuffer;

        public static LodModel Create(ByteChunk chunk)
        {
            int indexAtStart = chunk.Index;
            chunk.Index = indexAtStart;

            var offset = chunk.Index;
            var lodModel = new LodModel();

            lodModel.MaterialId = (GroupTypeEnum)chunk.ReadUInt32();
            lodModel.Unknown0 = chunk.ReadBytes(4);

            var VertOffset = chunk.ReadUInt32() + offset;
            lodModel.VertexCount = chunk.ReadUInt32();
            var FaceOffset = chunk.ReadUInt32() + offset;
            lodModel.FaceCount = chunk.ReadUInt32();

            // BoundingBox
            lodModel.BoundingBox = BoundingBox.Create(chunk);

            lodModel.MateriaType = Util.SanatizeFixedString(chunk.ReadFixedLength(30));
            lodModel.Unknown1 = chunk.ReadBytes(2);
            lodModel.VertexFormatValue = chunk.ReadUShort();
            lodModel.ModelName = Util.SanatizeFixedString(chunk.ReadFixedLength(32));
            //lodModel.ShaderValues = chunk.ReadBytes(20);


            lodModel.TextureDirectory = Util.SanatizeFixedString(chunk.ReadFixedLength(256));
            lodModel.Unknown2 = chunk.ReadBytes(258); // Unknown data. Almost always 0, appart from 2 last bytes
            lodModel.Transformation = LoadTransformations(chunk);
            lodModel.Unknown3 = chunk.ReadBytes(8); 
            lodModel.BoneCount = chunk.ReadUInt32();
            lodModel.MaterialCount = chunk.ReadUInt32();

            lodModel.Unknown4 = chunk.ReadBytes(140);

            for (int i = 0; i < lodModel.BoneCount; i++)
                lodModel.Bones.Add(Bone.Create(chunk));

            for (int i = 0; i < lodModel.MaterialCount; i++)
                lodModel.Materials.Add(Material.Create(chunk));

            lodModel.AlphaKeyValue = chunk.ReadBytes(4);
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

        static Transformation LoadTransformations(ByteChunk chunk)
        {
            var output = new Transformation();
            output.Pivot.X= chunk.ReadSingle();
            output.Pivot.Y = chunk.ReadSingle();
            output.Pivot.Z = chunk.ReadSingle();

            for (int i = 0; i < 3; i++)
            {
                var matrix = output.Matrices[i];
                for (int row = 0; row < 3; row++)
                {
                    matrix.Matrix[row].X = chunk.ReadSingle();
                    matrix.Matrix[row].Y = chunk.ReadSingle();
                    matrix.Matrix[row].Z = chunk.ReadSingle();
                    matrix.Matrix[row].W = chunk.ReadSingle();
                }
            }
            
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
                    //chunk.Index += (int)count * 32;
                    model.VertexFormat = VertexFormat.Default;
                    return CreateDefaultVertex(chunk, count);
                    //return null;
                case 3:
                    //chunk.Index += (int)count * 28;
                    model.VertexFormat = VertexFormat.Weighted;
                    return CreateWeighthedVertex(chunk, count);
                    //return null;
                case 4:
                    //chunk.Index += (int)count * 32;
                    model.VertexFormat = VertexFormat.Cinematic;
                    //return null;
                    return CreateCinematicVertex(chunk, count);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class FileVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public FileVector3(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }


    public class FileVector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public FileVector4(float x = 0, float y = 0, float z = 0, float w = 0)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
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



        public FileVector3 Position { get; set; } = new FileVector3();
        public FileVector3 Normal { get; set; } = new FileVector3();


        public float Uv0 { get; set; }
        public float Uv1 { get; set; }

        public List<BoneInfo> BoneInfos { get; set; } = new List<BoneInfo>();
    }
}
