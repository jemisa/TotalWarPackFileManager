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
            public class Pos3d
            {
                public Pos3d(float x, float y, float z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }

                public float X;
                public float Y;
                public float Z;
            }

            public List<Pos3d> Positions { get; set; } = new List<Pos3d>();
            public List<short[]> Quat { get; set; } = new List<short[]>();
        }

        public List<int>[] posIDArr = new List<int>[] { new List<int>(), new List<int>() };
        public List<int>[] rotIDArr = new List<int>[] { new List<int>(), new List<int>() };
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

            ouput.posIDArr = new List<int>[] { new List<int>(), new List<int>() };
            ouput.rotIDArr = new List<int>[] { new List<int>(), new List<int>() };

            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0x00)//: # for animated
                    ouput.posIDArr[0].Add(i);
                if (boneFlag == 0x27)//: # for static
                    ouput.posIDArr[1].Add(i);
            }

            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0x00)//: # for animated
                    ouput.rotIDArr[0].Add(i);
                if (boneFlag == 0x27)//: # for static
                    ouput.rotIDArr[1].Add(i);
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
                    var pos = new Frame.Pos3d(chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle());
                    frame.Positions.Add(pos);
                }

                for (int j = 0; j < animRotCount; j++)
                {
                    var quat = new short[4] { chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort() };
                    frame.Quat.Add(quat);
                }

                ouput.Frames.Add(frame);
            }

            return ouput;
        }

    }
}
