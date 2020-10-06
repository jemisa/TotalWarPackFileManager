using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

        AnimationFile _animation;
        SkeletonModel _skeletonModel;

        public List<AnimationFrame> KeyFrameCollection = new List<AnimationFrame>();

        public class AnimationFrame
        {
            public List<AnimationKeyFrame> BoneTransforms = new List<AnimationKeyFrame>();
        }


        void ApplyFrame(bool animateInPlace, AnimationFile.Frame animationKeyFrameData, AnimationFrame currentFrame, AnimationFile.Frame staticFrame)
        {
            int boneCount = 72;
            for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
            {
                // Compute translation
                //  Static
                //  Dynamic

                // Compute rotation
                //  Static
                //  Dynamic

                // Compute final keyframe

            }

            // Apply animation translation
            if(animationKeyFrameData != null)
            for (int i = 0; i < animationKeyFrameData.Transforms.Count(); i++)
            {
                var dynamicIndex = _animation.DynamicTranslationMappingID[i];
                if (dynamicIndex != -1)
                {
                    var pos = animationKeyFrameData.Transforms[i];

                    var temp = currentFrame.BoneTransforms[dynamicIndex].Transform;
                    temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                    currentFrame.BoneTransforms[dynamicIndex].Transform = temp;
                }


            }

            if(staticFrame != null)
            for (int i = 0; i < staticFrame.Transforms.Count(); i++)
            {
                var staticIndex = _animation.StaticTranslationMappingID[i];
                if (staticIndex != -1)
                {
                    var pos = staticFrame.Transforms[i];

                    var temp = currentFrame.BoneTransforms[staticIndex].Transform;
                    temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                    currentFrame.BoneTransforms[staticIndex].Transform = temp;
                }
            }

            // Apply animation rotation
            if (animationKeyFrameData != null)
                for (int i = 0; i < animationKeyFrameData.Quaternion.Count(); i++)
            {
                var dynamicIndex = _animation.DynamicRotationMappingID[i];
                if (dynamicIndex != -1)
                {
                    var animQ = animationKeyFrameData.Quaternion[i];
                    var quaternion = new Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                    quaternion.Normalize();
                    var translation = currentFrame.BoneTransforms[dynamicIndex].Transform.Translation;
                    currentFrame.BoneTransforms[dynamicIndex].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                }



            }

            if (staticFrame != null)
                for (int i = 0; i < staticFrame.Quaternion.Count(); i++)
            {
                var staticIndex = _animation.StaticRotationMappingID[i];
                if (staticIndex != -1)
                {
                    var animQ = staticFrame.Quaternion[i];
                    var quaternion = new Microsoft.Xna.Framework.Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                    quaternion.Normalize();
                    var translation = currentFrame.BoneTransforms[staticIndex].Transform.Translation;
                    currentFrame.BoneTransforms[staticIndex].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                }
            }
        }

        public void ReCreate(bool animateInPlace)
        {
            KeyFrameCollection.Clear();
            for (int frameIndex = 0; frameIndex < _animation.DynamicFrames.Count(); frameIndex++)
            {
                var animationKeyFrameData = _animation.DynamicFrames[frameIndex];
                var currentFrame = new AnimationFrame();

                // Copy base pose
                for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
                {
                    currentFrame.BoneTransforms.Add(new AnimationKeyFrame()
                    {
                        Transform = (_skeletonModel.Bones[i].Position),
                        BoneIndex = _skeletonModel.Bones[i].Index,
                        ParentBoneIndex = _skeletonModel.Bones[i].ParentIndex
                    });
                }

                ApplyFrame(animateInPlace, animationKeyFrameData, currentFrame, _animation.StaticFrame);


                // Move into world space
                for (int i = 0; i < currentFrame.BoneTransforms.Count(); i++)
                {
                    var parentindex = currentFrame.BoneTransforms[i].ParentBoneIndex;
                    if (parentindex == -1)
                        continue;

                    currentFrame.BoneTransforms[i].Transform = currentFrame.BoneTransforms[i].Transform * currentFrame.BoneTransforms[parentindex].Transform;
                }

                // Mult with inverse bind matrix, in worldspace
                for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
                {
                    var inv = Matrix.Invert(_skeletonModel.Bones[i].WorldPosition);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }

                KeyFrameCollection.Add(currentFrame);
            }

        }

        public void ReCreate2(bool animateInPlace)
        {
            KeyFrameCollection.Clear();
  
                var animationKeyFrameData = _animation.StaticFrame;
                var currentFrame = new AnimationFrame();

                // Copy base pose
                for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
                {
                    currentFrame.BoneTransforms.Add(new AnimationKeyFrame()
                    {
                        Transform = (_skeletonModel.Bones[i].Position),
                        BoneIndex = _skeletonModel.Bones[i].Index,
                        ParentBoneIndex = _skeletonModel.Bones[i].ParentIndex
                    });
                }

                // Apply animation translation
                if (animationKeyFrameData != null)
                {
               
                    for (int i = 0; i < animationKeyFrameData.Transforms.Count(); i++)
                    {
                        var mappingIdx = _animation.StaticTranslationMappingID[i];
                        var pos = animationKeyFrameData.Transforms[i];
                        if (animateInPlace && mappingIdx == 0)
                            pos = new AnimationFile.Frame.Transform(0, 0, 0);
                        var temp = currentFrame.BoneTransforms[mappingIdx].Transform;
                        temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                        currentFrame.BoneTransforms[mappingIdx].Transform = temp;
                    }

                    // Apply animation rotation
                    for (int i = 0; i < animationKeyFrameData.Quaternion.Count(); i++)
                    {
                        var animQ = animationKeyFrameData.Quaternion[i];
                        var quaternion = new Microsoft.Xna.Framework.Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                        quaternion.Normalize();

                        var mappingIdx = _animation.StaticRotationMappingID[i];
                        var translation = currentFrame.BoneTransforms[mappingIdx].Transform.Translation;
                        currentFrame.BoneTransforms[mappingIdx].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                    }
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
                for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
                {
                    var inv = Matrix.Invert(_skeletonModel.Bones[i].WorldPosition);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }

                KeyFrameCollection.Add(currentFrame);
          

        }

        public static AnimationClip Create(AnimationFile animation, SkeletonModel skeletonModel)
        {
            AnimationClip model = new AnimationClip();
            model._animation = animation;
            model._skeletonModel = skeletonModel;
            model.ReCreate(false);
            return model;


        }
    }
}
