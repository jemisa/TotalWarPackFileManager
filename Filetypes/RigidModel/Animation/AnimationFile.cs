using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel.Animation
{
    public class AnimationFile
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

        public List<int> DynamicTranslationMappingID = new List<int>();
        public List<int> DynamicRotationMappingID = new List<int>();
        public List<Frame> DynamicFrames = new List<Frame>();

        public List<int> StaticTranslationMappingID = new List<int>();
        public List<int> StaticRotationMappingID = new List<int>();
        public Frame StaticFrame { get; set; } = null;

        public string SkeletonName { get; set; }
        public uint AnimationType { get; set; }

        public uint Unknown0 { get; set; }
        public short Unknown1 { get; set; }
        public short Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }

        public static string GetAnimationSkeletonName(ByteChunk chunk)
        {
            var animationType = chunk.ReadUInt32();
            var unknown0 = chunk.ReadUInt32();
            var unknown1 = chunk.ReadShort();
            var unknown2 = chunk.ReadShort();
            var nameSize = chunk.ReadShort();
            var skeletonName = chunk.ReadFixedLength(nameSize);
            return skeletonName;
        }

        public static AnimationFile Create(ByteChunk chunk)
        {
            var ouput = new AnimationFile();
            chunk.Reset();
            ouput.AnimationType = chunk.ReadUInt32();
            ouput.Unknown0 = chunk.ReadUInt32();        // Always 1?
            var framerate = chunk.ReadSingle();
            var nameSize= chunk.ReadShort();
            ouput.SkeletonName = chunk.ReadFixedLength(nameSize);
            ouput.Unknown3 = chunk.ReadUInt32();        // Always 0? padding?

            if (ouput.AnimationType == 7)
            {
                var animationTotalPlayTimeInSec = chunk.ReadSingle(); // Play time
            }
        

            var boneCount = chunk.ReadUInt32();

            var boneName = new List<string>();
            var boneParent = new List<int>();
            for (int i = 0; i < boneCount; i++)
            {
                var boneNameSize = chunk.ReadShort();
                boneName.Add(chunk.ReadFixedLength(boneNameSize));
                boneParent.Add(chunk.ReadInt32());
            }


            // Remapping tables, not sure how they really should be used, but this works.
            for (int i = 0; i < boneCount; i++)
            {

                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0)
                    ouput.DynamicTranslationMappingID.Add(i);
                if (boneFlag == 39)
                    ouput.StaticTranslationMappingID.Add(i);
            }

            for (int i = 0; i < boneCount; i++)
            {

                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();

                if (boneFlag == 0)
                    ouput.DynamicRotationMappingID.Add(i);
                if (boneFlag == 39)
                    ouput.StaticRotationMappingID.Add(i);
            }

            // ----------------------


            // A single static frame - Can be inverse, a pose or empty. Not sure? Hand animations are stored here
            if (ouput.AnimationType == 7)
            {
                var staticPosCount = chunk.ReadUInt32();
                var staticRotCount = chunk.ReadUInt32();

                var frame = new Frame();
                for (int j = 0; j < staticPosCount; j++)
                {
                    var pos = new Frame.Transform(chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle());
                    frame.Transforms.Add(pos);
                }

                for (int j = 0; j < staticRotCount; j++)
                {
                    var quat = new short[4] { chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort() };
                    frame.Quaternion.Add(quat);
                }

                ouput.StaticFrame = frame;
            }
            // ----------------------


            // Animation Data
            var animPosCount = chunk.ReadUInt32();
            var animRotCount = chunk.ReadUInt32();
            var frameCount = chunk.ReadUInt32();    // Always 3 when there is no data? Why?

            if (animPosCount != 0 || animRotCount != 0)
            {
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

                    ouput.DynamicFrames.Add(frame);
                }
            }
            // ----------------------

            return ouput;
        }

    }
}
