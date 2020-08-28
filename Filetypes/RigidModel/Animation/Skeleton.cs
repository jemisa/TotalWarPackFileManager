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
        public static Skeleton Create(ByteChunk chunk, out string errorMessage)
        {
            errorMessage = "";
			chunk.Reset();

			var unk0 = chunk.ReadBytes(12);
			var nameLength = chunk.ReadUShort();
			var name = chunk.ReadFixedLength(nameLength);
			var flag = chunk.ReadUInt32();
			if(flag == 0)
				chunk.ReadUInt32();
			var boneCount = chunk.ReadInt32();

			List<string> boneNames = new List<string>();
			for (int i = 0; i < boneCount; i++)
			{
				var boneNameSize = chunk.ReadShort();
				var boneName = chunk.ReadFixedLength(boneNameSize);
				var parentId = chunk.ReadInt32();
				boneNames.Add(boneName);
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

			var vertList = new List<AnimationEntry>();
			for (int i = 0; i < posCount; i++)
			{
				var x = chunk.ReadSingle();
				var y = chunk.ReadSingle();
				var z = chunk.ReadSingle();

				var vert = new AnimationEntry()
				{
					X = x,
					Y = y,
					Z = z
				};
				vertList.Add(vert);
			}

			for (int i = 0; i < rotCount; i++)
			{
				var t0 = chunk.ReadFloat16();
				var t1 = chunk.ReadFloat16();
				var t2 = chunk.ReadFloat16();
				var t3 = chunk.ReadFloat16();
			}

			var a = chunk.ReadInt32();
			var b = chunk.ReadInt32();

			return new Skeleton();








		}
    }

	class AnimationEntry
	{
		public float X;
		public float Y;
		public float Z;

        public override string ToString()
        {
			return $"{X}, {Y}, {Z}";
        }
    }
}
