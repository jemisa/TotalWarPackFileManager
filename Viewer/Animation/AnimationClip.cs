using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.GraphicModels;

namespace Viewer.Animation
{
    public class AnimationClip
    {
        public class AnimationKeyFrame
        {
            public int BoneIndex { get; set; }
            public int ParentBoneIndex { get; set; }
            public Matrix Transform { get; set; }
        }

        public List<AnimationFrame> KeyFrameCollection = new List<AnimationFrame>();

        public class AnimationFrame
        {
            public List<AnimationKeyFrame> BoneTransforms = new List<AnimationKeyFrame>();
        }

        public static AnimationClip Create(AnimationFile animation, SkeletonModel skeletonModel)
        {
            AnimationClip model = new AnimationClip();
            for (int frameIndex = 0; frameIndex < animation.Frames.Count(); frameIndex++)
            {
                var animationKeyFrameData = animation.Frames[frameIndex];
                var currentFrame = new AnimationFrame();

                // Copy base pose
                for (int i = 0; i < skeletonModel.Bones.Count(); i++)
                {
                    currentFrame.BoneTransforms.Add(new AnimationKeyFrame()
                    {
                        Transform = (skeletonModel.Bones[i].Position),
                        BoneIndex = skeletonModel.Bones[i].Index,
                        ParentBoneIndex = skeletonModel.Bones[i].ParentIndex
                    });
                }

                // Apply animation translation
                for (int i = 0; i < animationKeyFrameData.Transforms.Count(); i++)
                {
                    var index = animation.TranslationMappingID[0][i];
                    var pos = animationKeyFrameData.Transforms[i];
                    var temp = currentFrame.BoneTransforms[index].Transform;
                    temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                    currentFrame.BoneTransforms[index].Transform = temp;
                }

                // Apply animation rotation
                for (int i = 0; i < animationKeyFrameData.Quaternion.Count(); i++)
                {
                    var animQ = animationKeyFrameData.Quaternion[i];
                    var quaternion = new Microsoft.Xna.Framework.Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                    quaternion.Normalize();

                    var mappingIdx = animation.RotationMappingID[0][i];
                    var translation = currentFrame.BoneTransforms[mappingIdx].Transform.Translation;
                    currentFrame.BoneTransforms[mappingIdx].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                }

                // Move into world space
                for (int i = 0; i < currentFrame.BoneTransforms.Count(); i++)
                {
                    var parentindex = currentFrame.BoneTransforms[i].ParentBoneIndex;
                    if (parentindex == -1)
                        continue;

                    currentFrame.BoneTransforms[i].Transform = currentFrame.BoneTransforms[i].Transform * currentFrame.BoneTransforms[parentindex].Transform;
                }

                // Mult with inverse bind matrix, in worldspace
                for (int i = 0; i < skeletonModel.Bones.Count(); i++)
                {
                    var inv = Matrix.Invert(skeletonModel.Bones[i].WorldPosition);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }

                model.KeyFrameCollection.Add(currentFrame);
            }

            return model;
        }
    }
}
