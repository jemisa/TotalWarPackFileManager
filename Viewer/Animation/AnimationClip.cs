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

        public void ReCreate(bool animateInPlace)
        {
            KeyFrameCollection.Clear();
            for (int frameIndex = 0; frameIndex < _animation.Frames.Count(); frameIndex++)
            {
                var animationKeyFrameData = _animation.Frames[frameIndex];
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
                for (int i = 0; i < animationKeyFrameData.Transforms.Count(); i++)
                {
                    var index = _animation.TranslationMappingID[0][i];
                    var pos = animationKeyFrameData.Transforms[i];
                    if (animateInPlace && index == 0)
                        pos = new AnimationFile.Frame.Transform(0, 0, 0);
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

                    var mappingIdx = _animation.RotationMappingID[0][i];
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
                for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
                {
                    var inv = Matrix.Invert(_skeletonModel.Bones[i].WorldPosition);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }

                KeyFrameCollection.Add(currentFrame);
            }

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
