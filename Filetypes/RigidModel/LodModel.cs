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

            var uknown1 = chunk.ReadBytes(422); // Contains some transformations

            lodModel.BoneCount = chunk.ReadUInt32();
            lodModel.materialCount = chunk.ReadUInt32();

            var uknown2 = chunk.ReadBytes(140);

            for (int i = 0; i < lodModel.BoneCount; i++)
                lodModel.Bones.Add(Bone.Create(chunk));

            for (int i = 0; i < lodModel.materialCount; i++)
                lodModel.Materials.Add(Material.Create(chunk));

            var uknown3 = chunk.ReadBytes(8);

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
                        X = subChucnk.ReadFloat16(),
                        Y = subChucnk.ReadFloat16(),
                        Z = subChucnk.ReadFloat16(),
                    };
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
    public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
