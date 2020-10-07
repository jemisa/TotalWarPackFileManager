using Filetypes.ByteParsing;
using System.Collections.Generic;

namespace Filetypes.RigidModel
{
    public class AnimationFile
    {
        public class BoneInfo
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        public class Frame
        {
            public List<float[]> Transforms { get; set; } = new List<float[]>();
            public List<short[]> Quaternion { get; set; } = new List<short[]>();
        }

        public class AnimationHeader
        {
            public uint AnimationType { get; set; }
            public uint Unknown0_alwaysOne { get; set; }
            public float FrameRate { get; set; }
            public string SkeletonName { get; set; }
            public uint Unknown1_alwaysZero { get; set; }
        }

        public BoneInfo[] Bones;

        // Version 7 spesific 
        public float AnimationTotalPlayTimeInSec { get; set; }

        public List<int> StaticTranslationMappingID = new List<int>();
        public List<int> StaticRotationMappingID = new List<int>();
        public Frame StaticFrame { get; set; } = null;


        // Keyframes/default pose
        public List<int> DynamicTranslationMappingID = new List<int>();
        public List<int> DynamicRotationMappingID = new List<int>();
        public List<Frame> DynamicFrames = new List<Frame>();


        public AnimationHeader Header { get; set; } = new AnimationHeader();
  
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
            output.Header.AnimationType = chunk.ReadUInt32();
            output.Header.Unknown0_alwaysOne = chunk.ReadUInt32();        // Always 1?
            output.Header.FrameRate = chunk.ReadSingle();
            var nameLength = chunk.ReadShort();
            output.Header.SkeletonName = chunk.ReadFixedLength(nameLength);
            output.Header.Unknown1_alwaysZero = chunk.ReadUInt32();        // Always 0? padding?


            if (output.Header.AnimationType == 7)
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
            if (output.Header.AnimationType == 7)
            {
                var staticPosCount = chunk.ReadUInt32();
                var staticRotCount = chunk.ReadUInt32();
                if(staticPosCount != 0 && staticRotCount != 0)
                    output.StaticFrame = ReadFrame(chunk, staticPosCount, staticRotCount);
            }

            // Animation Data
            var animPosCount = chunk.ReadUInt32();
            var animRotCount = chunk.ReadUInt32();
            var frameCount = chunk.ReadUInt32();    // Always 3 when there is no data? Why?

            if (animPosCount != 0 || animRotCount != 0)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    var frame = ReadFrame(chunk, animPosCount, animRotCount);
                    output.DynamicFrames.Add(frame);
                }
            }
            // ----------------------

            return output;
        }

        static Frame ReadFrame(ByteChunk chunk, uint positions, uint rotations)
        {
            var frame = new Frame();
            for (int j = 0; j < positions; j++)
            {
                var pos = new float[3] { chunk.ReadSingle(), chunk.ReadSingle(), chunk.ReadSingle() };
                frame.Transforms.Add(pos);
            }

            for (int j = 0; j < rotations; j++)
            {
                var quat = new short[4] { chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort(), chunk.ReadShort() };
                frame.Quaternion.Add(quat);
            }
            return frame;
        }

    }
}
