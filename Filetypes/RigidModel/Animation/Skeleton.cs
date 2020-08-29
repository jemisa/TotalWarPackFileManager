using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel.Animation
{
    public class Skeleton
    {
		public string Name { get; set; }
		public List<BoneInfo> Bones = new List<BoneInfo>();

		public static Skeleton Create(ByteChunk chunk, out string errorMessage)
        {
			var skeleton = new Skeleton();

			errorMessage = "";
			chunk.Reset();

			var unk0 = chunk.ReadBytes(12);
			var nameLength = chunk.ReadUShort();
			skeleton.Name = chunk.ReadFixedLength(nameLength);
			var flag = chunk.ReadUInt32();
			if(flag == 0)
				chunk.ReadUInt32();

			var boneCount = chunk.ReadInt32();
			for (int i = 0; i < boneCount; i++)
				skeleton.Bones.Add(new BoneInfo());

			List<string> boneNames = new List<string>();
			for (int i = 0; i < boneCount; i++)
			{
				
				var boneNameSize = chunk.ReadShort();
				var boneName = chunk.ReadFixedLength(boneNameSize);
				var parentId = chunk.ReadInt32();
				skeleton.Bones[i].Name = boneName;
				skeleton.Bones[i].Id = i;
				skeleton.Bones[i].ParentId = parentId;
			}

			//X Bytes - UInt16[BonesCount] // They are the bone IDs.
			//X Bytes - UInt16[BonesCount] // They are the remapped bone IDs.
			// Store in two different arrays

			for (int i = 0; i < boneCount; i++)
			{ 
				var x = chunk.ReadInt32();
				var y = chunk.ReadInt32();
			}

			if (flag == 0)
				chunk.ReadBytes(8);


			var posCount = chunk.ReadInt32();
			var rotCount = chunk.ReadInt32();
			var frameCount = chunk.ReadInt32();

			for (int i = 0; i < posCount; i++)
			{
				skeleton.Bones[i].Position_X = chunk.ReadSingle();
				skeleton.Bones[i].Position_Y = chunk.ReadSingle();
				skeleton.Bones[i].Position_Z = chunk.ReadSingle();
			}

			for (int i = 0; i < rotCount; i++)
			{
				skeleton.Bones[i].Rotation_X = chunk.ReadShort();
				skeleton.Bones[i].Rotation_Y = chunk.ReadShort();
				skeleton.Bones[i].Rotation_Z = chunk.ReadShort();
				skeleton.Bones[i].Rotation_W = chunk.ReadShort();
			}

			var a = chunk.ReadInt32();
			var b = chunk.ReadInt32();

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
			return $"{Position_X}, {Position_Y}, {Position_Z}";
        }
    }
}
