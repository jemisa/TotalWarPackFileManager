using Filetypes.ByteParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackFileManagerUnitTests.FileTypes
{
    [TestClass]
    public class Temp
    {


        public enum GroupTypeEnum
        {
            Actor = 	82,
            Banner = 	26,
            Bow = 64,
            BuildingAcropolis = 86,
            BuildingGeneric = 68,
            Cloak =  60,
            Creature = 65,
            PlantA = 74,
            PlantB = 75,
            Projectile = 70,
            Shield = 72,
            StatueA = 54,
            StatueB = 57,
            Tunic = 79,
            Wave =22,
        }


        public class LodData
        {

            public uint GroupsCount { get; set; }
            public uint Unknown0 { get; set; }
            public uint Unknown1 { get; set; }
            public uint StartOffset { get; set; }
            public float Scale { get; set; }
            public uint Unknown2 { get; set; }
            public uint Unknown3 { get; set; }

            public static LodData Create(ByteChunk chunk)
            {
                var data = new LodData()
                {
                    GroupsCount = chunk.ReadUInt32(),
                    Unknown0 = chunk.ReadUInt32(),
                    Unknown1 = chunk.ReadUInt32(),
                    StartOffset = chunk.ReadUInt32(),
                    Scale = chunk.ReadSingle(),
                    Unknown2 = chunk.ReadUInt32(),
                    Unknown3 = chunk.ReadUInt32()
                };
                return data;
            }
        }


        public class Bone
        { 
            public string Name { get; set; }
            public static Bone Create(ByteChunk chunk)
            {
                return new Bone()
                {
                    Name = chunk.ReadFixedLength(84),
                };
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class Material
        {
            public string Name { get; set; }
            public int Type { get; set; }
            public static Material Create(ByteChunk chunk)
            {
                return new Material()
                {
                    Type = chunk.ReadInt32(),
                    Name = chunk.ReadFixedLength(256),
                };
            }

            public override string ToString()
            {
                return Type + " "  +  Name;
            }
        }

        public class BoundingBox
        {
            public float MinimumX {get;set;}
            public float MinimumY {get;set;}
            public float MinimumZ {get;set;}
            public float MaximumX {get;set;}
            public float MaximumY {get;set;}
            public float MaximumZ { get; set; }
            public static BoundingBox Create(ByteChunk chunk)
            {
                return new BoundingBox()
                {
                    MinimumX = chunk.ReadSingle(),
                    MinimumY = chunk.ReadSingle(),
                    MinimumZ = chunk.ReadSingle(),
                    MaximumX = chunk.ReadSingle(),
                    MaximumY = chunk.ReadSingle(),
                    MaximumZ = chunk.ReadSingle(),
                };
            }

            public override string ToString()
            {
                return $"x(min:{MinimumX} max:{MaximumX}) y(min:{MinimumY} max:{MaximumY}) z(min:{MinimumZ} max:{MaximumZ})";
            }
        }

        public class LodModel
        {
            public GroupTypeEnum GroupType { get; set; }

            List<Bone> Bones { get; set; }
            public uint BoneCount { get; set; }

            public uint vertexCount { get; set; }
            public uint faceCount { get; set; }
            public uint vertexType { get; set; }
            public string materiaType { get; set; }
            public string modelName { get; set; }
            public string textureDirectory { get; set; }

            public BoundingBox BoundingBox { get; set; }

            public uint materialCount { get; set; }
            public List<Material> Materials { get; set; }

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
                lodModel.textureDirectory= chunk.ReadFixedLength(256);

                var uknown1 = chunk.ReadBytes(422); // Contains some transformations

                lodModel.BoneCount = chunk.ReadUInt32();
                lodModel.materialCount = chunk.ReadUInt32();

                var uknown2 = chunk.ReadBytes(140);

                lodModel.Bones = new List<Bone>();
                for (int i = 0; i < lodModel.BoneCount; i++)
                    lodModel.Bones.Add(Bone.Create(chunk));


                lodModel.Materials = new List<Material>();
                for (int i = 0; i < lodModel.materialCount; i++)
                    lodModel.Materials.Add(Material.Create(chunk));

                var uknown3 = chunk.ReadBytes(8);

                var VbufferSize = ((FaceOffset - VertOffset) / lodModel.vertexCount);
                byte[] vertexBuffer = chunk.ReadBytes((int)(lodModel.vertexCount * VbufferSize));
                byte[] FaceBuff = chunk.ReadBytes((int)lodModel.faceCount * 2);

                return lodModel;
            }
        }


        [TestMethod]
        public void ConvertTabele_validDefinition()
        {
            var bytes = File.ReadAllBytes(@"C:\temp\datafiles\vmp_black_coach_01.rigid_model_v2");
            ByteChunk chunk = new ByteChunk(bytes);

            // Headers
            var magic = chunk.ReadFixedLength(4);
            var meshCount = chunk.ReadUInt32();
            var lodCount = chunk.ReadUInt32();
            var baseSkeleton = chunk.ReadFixedLength(128);

            chunk.Index = 140;

            var lodList = new List<LodData>();
            for (int i = 0; i < lodCount; i++)
                lodList.Add(LodData.Create(chunk));

            var lodModels = new List<LodModel>();
            for (int i = 0; i < lodCount; i++)
                lodModels.Add(LodModel.Create(chunk));

            return;
            /*
             
             
                 flag = 0
    Matnames = []
    bonenames = []
    texList = [] 
    matList = []
    
    f = NoeBitStream(data, NOE_LITTLEENDIAN)
   

    Magic = noeStrFromBytes(f.readBytes(4), "ASCII")
    print(Magic)
    if Magic != "RMV2":
        print("worng file format")
    
    MeshCount = f.readInt()
    LodCount = f.readInt()

    f.seek(140,0) # seek 0x8c from start of file
    
    f.seek(LodCount*28,1)
    
    
    # get vertex, weight, uv data
    
    s = 0
    if DEBUG == 1:
        LodCount = MeshCount

             while s < (LodCount):
        offset = f.tell()
        test = f.readUInt() # check i am in the right place
        f.seek(4,1)
        VertOffset = f.readUInt() + offset
        VertexCount = f.readUInt()
        FaceOffset = f.readUInt() + offset
        FaceCount = f.readUInt()         
        VbufferSize = int((FaceOffset - VertOffset)/VertexCount)
        f.seek(24,1)
        MatType = ReadStringKnown(f, 30)
        VertType = f.readUInt()
        ModelName = ReadStringKnown(f, 32)
        MaterialName = ReadStringKnown(f, 514)
        LenMat = len(MaterialName)
        f.seek(156,1)
        FFFF = f.readInt64()
        NumOfBones = f.readUInt() 
        NumberOfMats = f.readUInt() 

        f.seek(140,1)

        for i in range(NumOfBones):
            bonename = ReadStringKnown(f, 84)  # read string of known size, have bug with noeStrFromBytes(f.readBytes(514), "ASCII")
            bonenames.append(bonename)
        
        Matnames.append([])
        for i in range(NumberOfMats):
            matname = ReadStringKnown(f, 260)
            matname = matname[LenMat:]
            Matnames[s].append(matname)       
        
        f.seek(4,1)       

        unk1 = f.readUInt()  # have 1 , 0, also FF FF FF FF       
        
        
        material, tex = Get_Texture(Matnames,NumberOfMats,s)       
        if material != 0:
            texList += tex
            matList.append(material)

        VertBuff = f.readBytes(VertexCount * VbufferSize)           
        rapi.rpgBindPositionBufferOfs(VertBuff, noesis.RPGEODATA_HALFFLOAT, VbufferSize, 0) # vertex


        if VertType == 196608: #buffer 28
            rapi.rpgBindUV1BufferOfs(VertBuff, noesis.RPGEODATA_HALFFLOAT, VbufferSize, 16)
            rapi.rpgBindBoneIndexBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 8, 1) # bones
            rapi.rpgBindBoneWeightBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 10, 1) # weights
        if VertType == 0: #buffer 32
            rapi.rpgBindUV1BufferOfs(VertBuff, noesis.RPGEODATA_HALFFLOAT, VbufferSize, 8)
            rapi.rpgBindBoneIndexBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 8, 4) # bones
            rapi.rpgBindBoneWeightBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 12, 4) # weights
        if VertType == 262144: #buffer 32
            rapi.rpgBindUV1BufferOfs(VertBuff, noesis.RPGEODATA_HALFFLOAT, VbufferSize, 20)
            rapi.rpgBindBoneIndexBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 8, 4) # bones
            rapi.rpgBindBoneWeightBufferOfs(VertBuff, noesis.RPGEODATA_UBYTE, VbufferSize, 12, 4) # weights

        FaceBuff = f.readBytes(FaceCount * 2)
        rapi.rpgSetName(ModelName + str(s))
        #rapi.rpgCommitTriangles(None, noesis.RPGEODATA_USHORT, VertexCount, noesis.RPGEO_POINTS, 1)
        if material != 0:
            rapi.rpgSetMaterial(material.name)
        rapi.rpgCommitTriangles(FaceBuff, noesis.RPGEODATA_USHORT, FaceCount, noesis.RPGEO_TRIANGLE, 1)       
        
        rapi.rpgClearBufferBinds()
                
        s += 1
             
             */



        }
    }
}
