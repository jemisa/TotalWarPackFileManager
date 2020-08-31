using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel.Animation
{
    public class Animation
    {
        public class Frame
        {
            public class Transform
            {
                public Transform(float x, float y, float z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }

                public float X;
                public float Y;
                public float Z;
            }

            public List<Transform> Transforms { get; set; } = new List<Transform>();
            public List<short[]> Quaternion { get; set; } = new List<short[]>();
        }

        public List<int>[] TranslationMappingID = new List<int>[] { new List<int>(), new List<int>() };
        public List<int>[] RotationMappingID = new List<int>[] { new List<int>(), new List<int>() };
        public List<Frame> Frames = new List<Frame>();

        public static Animation Create(ByteChunk chunk)
        {
            var ouput = new Animation();
            chunk.Reset();
            var type = chunk.ReadUInt32();
            var unk0 = chunk.ReadUInt32();
            var unk1 = chunk.ReadShort();
            var unk2 = chunk.ReadShort();
            var nameSize= chunk.ReadShort();
            var skeletonName = chunk.ReadFixedLength(nameSize);
            var ukn3 = chunk.ReadUInt32();

            if(type == 7)
                chunk.ReadUInt32();

            var boneCount = chunk.ReadUInt32();

            var boneName = new List<string>();
            var boneParent = new List<uint>();
            for (int i = 0; i < boneCount; i++)
            {
                var boneNameSize = chunk.ReadShort();
                boneName.Add(chunk.ReadFixedLength(boneNameSize));
                boneParent.Add(chunk.ReadUInt32());
            }

            ouput.TranslationMappingID = new List<int>[] { new List<int>(), new List<int>() };
            ouput.RotationMappingID = new List<int>[] { new List<int>(), new List<int>() };

            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0x00)//: # for animated
                    ouput.TranslationMappingID[0].Add(i);
                if (boneFlag == 0x27)//: # for static
                    ouput.TranslationMappingID[1].Add(i);
            }

            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0x00)//: # for animated
                    ouput.RotationMappingID[0].Add(i);
                if (boneFlag == 0x27)//: # for static
                    ouput.RotationMappingID[1].Add(i);
            }

            // We dont care about this?
            if (type == 7)
            {
                var staticPosCount = chunk.ReadUInt32();
                var staticRotCount = chunk.ReadUInt32();
                for (int i = 0; i < staticPosCount; i++)
                    chunk.ReadBytes(4 * 3);

                for (int i = 0; i < staticRotCount; i++)
                    chunk.ReadBytes(2 * 4);
            }

            var animPosCount = chunk.ReadUInt32();
            var animRotCount = chunk.ReadUInt32();
            var frameCount = chunk.ReadUInt32();

            for (int i = 0; i < frameCount; i++)
            {
                var frame = new Frame();
                for (int j = 0; j < animPosCount; j++)
                {
                    var pos = new Frame.Transform(chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle());
                    frame.Transforms.Add(pos);
                }

                for (int j = 0; j < animRotCount; j++)
                {
                    var quat = new short[4] { chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort() };
                    frame.Quaternion.Add(quat);
                }

                ouput.Frames.Add(frame);
            }

            return ouput;
        }

    }
}
