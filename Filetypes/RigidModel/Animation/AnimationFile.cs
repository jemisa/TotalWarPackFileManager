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

        public BoneInfo[] Bones;

        public List<int> DynamicTranslationMappingID = new List<int>();
        public List<int> DynamicRotationMappingID = new List<int>();
        public List<Frame> DynamicFrames = new List<Frame>();

        public List<int> StaticTranslationMappingID = new List<int>();
        public List<int> StaticRotationMappingID = new List<int>();
        public Frame StaticFrame { get; set; } = null;
        public float FrameRate { get; set; }
        public float AnimationTotalPlayTimeInSec { get; set; }
        public string SkeletonName { get; set; }
        public uint AnimationType { get; set; }

        public uint Unknown0_alwaysOne { get; set; }
        public uint Unknown1_alwaysZero { get; set; }
  
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
            var output = new AnimationFile();
            chunk.Reset();
            output.AnimationType = chunk.ReadUInt32();
            output.Unknown0_alwaysOne = chunk.ReadUInt32();        // Always 1?
            output.FrameRate = chunk.ReadSingle();
            var nameLength = chunk.ReadShort();
            output.SkeletonName = chunk.ReadFixedLength(nameLength);
            output.Unknown1_alwaysZero = chunk.ReadUInt32();        // Always 0? padding?

            if (output.AnimationType == 7)
                output.AnimationTotalPlayTimeInSec = chunk.ReadSingle(); // Play time

            var boneCount = chunk.ReadUInt32();

            output.Bones = new BoneInfo[boneCount];
            for (int i = 0; i < boneCount; i++)
            {
                var boneNameSize = chunk.ReadShort();
                output.Bones[i] = new BoneInfo()
                {
                    Name = chunk.ReadFixedLength(boneNameSize),
                    ParentId = chunk.ReadInt32(),
                    Id = i
                };
            }

            // Remapping tables, not sure how they really should be used, but this works.
            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();          // This just counts up when ever the value is not -1, one set for each flag
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();
                
                if (boneFlag == 0)
                    output.DynamicTranslationMappingID.Add(i);
                if (boneFlag == 39)
                    output.StaticTranslationMappingID.Add(i);
            }   

            for (int i = 0; i < boneCount; i++)
            {
                var boneId = chunk.ReadByte();
                var boneFlag = chunk.ReadByte();
                var ukn = chunk.ReadShort();
            
                if (boneFlag == 0)
                    output.DynamicRotationMappingID.Add(i);
                if (boneFlag == 39)
                output.StaticRotationMappingID.Add(i);
            }


            // A single static frame - Can be inverse, a pose or empty. Not sure? Hand animations are stored here
            if (output.AnimationType == 7)
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

                output.StaticFrame = frame;
            }

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

                    output.DynamicFrames.Add(frame);
                }
            }
            // ----------------------

            return output;
        }

    }
}
