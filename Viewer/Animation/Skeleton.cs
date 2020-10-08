﻿using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Viewer.Animation
{
    public class Skeleton
    {
        public Matrix[] Transform { get; private set; }
        public Matrix[] WorldTransform { get; private set; }
        public int[] ParentBoneId { get; private set; }
        public string[] BoneNames { get; private set; }
        public int BoneCount { get; set; }

        public Skeleton(AnimationFile skeletonFile)
        {
            BoneCount = skeletonFile.Bones.Count();
            Transform = new Matrix[BoneCount];
            WorldTransform = new Matrix[BoneCount];
            ParentBoneId = new int[BoneCount];
            BoneNames = new string[BoneCount];

            for (int i = 0; i < BoneCount; i++)
            {
                ParentBoneId[i] = skeletonFile.Bones[i].ParentId;
                BoneNames[i] = skeletonFile.Bones[i].Name;
            }

            int skeletonWeirdIndex = 0;
            for (int i = 0; i < BoneCount; i++)
            {
                var quat = new Quaternion(
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i][0],
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i][1],
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i][2],
                    skeletonFile.DynamicFrames[skeletonWeirdIndex].Quaternion[i][3]);
                quat.Normalize();
                var rotationMatrix = Matrix.CreateFromQuaternion(quat);
                var translationMatrix =  Matrix.CreateTranslation(
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i][0],
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i][1],
                            skeletonFile.DynamicFrames[skeletonWeirdIndex].Transforms[i][2]);

                var scale = Matrix.CreateScale(1);
                //if(i == 0)
                //    scale = Matrix.CreateScale(-1, 1, 1);
                
                var transform = scale * rotationMatrix * translationMatrix;

                Transform[i] = transform;
                WorldTransform[i] = transform;
            }


            for (int i = 0; i < BoneCount; i++)
            {
                var parentIndex = skeletonFile.Bones[i].ParentId;
                if (parentIndex == -1)
                    continue;
                WorldTransform[i] = WorldTransform[i] * WorldTransform[parentIndex];
            }
        }

        public int GetBoneIndex(string name)
        {
            for (int i = 0; i < BoneNames.Count(); i++)
            {
                if (BoneNames[i] == name)
                    return i;
            }

            return -1;
        }
    }
}
