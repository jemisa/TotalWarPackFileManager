using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel.Animation
{
    public class SkeletonFile
    {
		public string Name { get; set; }
		BoneInfo[] _Bones;

		public BoneInfo[] Bones { get { return Frames[0]; } }

		public List<BoneInfo[]> Frames = new List<BoneInfo[]>();

		public static SkeletonFile Create(ByteChunk chunk, out string errorMessage)
        {
			var skeleton = new SkeletonFile();

			errorMessage = "";
			chunk.Reset();

			var unk0 = chunk.ReadBytes(12);
			var nameLength = chunk.ReadUShort();
			skeleton.Name = chunk.ReadFixedLength(nameLength);
			var flag = chunk.ReadUInt32();
			if(flag == 0)
				chunk.ReadUInt32();

			var boneCount = chunk.ReadInt32();
			skeleton._Bones = new BoneInfo[boneCount];

			for (int i = 0; i < boneCount; i++)
			{
				var boneNameSize = chunk.ReadShort();
				var boneName = chunk.ReadFixedLength(boneNameSize);
				var parentId = chunk.ReadInt32();
				skeleton._Bones[i] = new BoneInfo();
				skeleton._Bones[i].Name = boneName;
				skeleton._Bones[i].Id = i;
				skeleton._Bones[i].ParentId = parentId;
			}

			//X Bytes - UInt16[BonesCount] // They are the bone IDs.
			//X Bytes - UInt16[BonesCount] // They are the remapped bone IDs.
			// Store in two different arrays

			for (int i = 0; i < boneCount; i++)
			{ 


				chunk.ReadInt32(); System.Security.Cryptography.AsymmetricSignatureDeformatter asd // Mapping related?
				chunk.ReadInt32();
			}

			if (flag == 0)
				chunk.ReadBytes(8);


			var posCount = chunk.ReadInt32();
			var rotCount = chunk.ReadInt32();
			var frameCount = chunk.ReadInt32();

			for (int f = 0; f < frameCount; f++)
			{
				skeleton.Frames.Add(new BoneInfo[boneCount]);
				Array.Copy(skeleton._Bones, skeleton.Frames[f], skeleton._Bones.Length);

				for (int i = 0; i < posCount; i++)
				{
					skeleton.Frames[f][i].Position_X = chunk.ReadSingle();
					skeleton.Frames[f][i].Position_Y = chunk.ReadSingle();
					skeleton.Frames[f][i].Position_Z = chunk.ReadSingle();
				}

				for (int i = 0; i < rotCount; i++)
				{
					skeleton.Frames[f][i].Rotation_X = chunk.ReadShort();
					skeleton.Frames[f][i].Rotation_Y = chunk.ReadShort();
					skeleton.Frames[f][i].Rotation_Z = chunk.ReadShort();
					skeleton.Frames[f][i].Rotation_W = chunk.ReadShort();
				}
			}

			return skeleton;
		}
    }

	public class BoneInfo
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int ParentId { get; set; }
		public float Position_X { get;set;}
		public float Position_Y { get;set;}
		public float Position_Z { get;set; } 

		public short Rotation_X { get; set; }
		public short Rotation_Y { get; set; }
		public short Rotation_Z { get; set; }
		public short Rotation_W { get; set; }

		public override string ToString()
        {
			return $"{Name} {Id}->{ParentId} : {Position_X}, {Position_Y}, {Position_Z}";
        }
    }
}
