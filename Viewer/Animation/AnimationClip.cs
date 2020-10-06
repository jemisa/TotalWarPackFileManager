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

            public AnimationKeyFrame Clone()
            {
                var newItem = new AnimationKeyFrame()
                {
                    BoneIndex = BoneIndex,
                    ParentBoneIndex = ParentBoneIndex,
                    Transform = Transform,
                };
                return newItem;
            }
        }

        AnimationFile _animation;
        SkeletonModel _skeletonModel;

        public List<AnimationFrame> KeyFrameCollection = new List<AnimationFrame>();

        public class AnimationFrame
        {
            public List<AnimationKeyFrame> BoneTransforms = new List<AnimationKeyFrame>();

            public AnimationFrame Clone()
            {
                var newItem = new AnimationFrame();
                foreach (var transform in BoneTransforms)
                    newItem.BoneTransforms.Add(transform.Clone());
                return newItem;
            }
        }

        void ApplyFrame(bool animateInPlace, AnimationFile.Frame frame, List<int> translationMappings, List<int> rotationMapping, AnimationFrame currentFrame)
        {
            if (frame == null)
                return;
           
            for (int i = 0; i < frame.Transforms.Count(); i++)
            {
                var dynamicIndex = translationMappings[i];
                if (dynamicIndex != -1)
                {
                    var pos = frame.Transforms[i];

                    var temp = currentFrame.BoneTransforms[dynamicIndex].Transform;
                    temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                    currentFrame.BoneTransforms[dynamicIndex].Transform = temp;
                }
            }

            for (int i = 0; i < frame.Quaternion.Count(); i++)
            {
                var dynamicIndex = rotationMapping[i];
                if (dynamicIndex != -1)
                {
                    var animQ = frame.Quaternion[i];
                    var quaternion = new Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                    quaternion.Normalize();
                    var translation = currentFrame.BoneTransforms[dynamicIndex].Transform.Translation;
                    currentFrame.BoneTransforms[dynamicIndex].Transform = Matrix.CreateFromQuaternion(quaternion) * Matrix.CreateTranslation(translation);
                }
            }
        }

        public void ReCreate(bool animateInPlace)
        {
            KeyFrameCollection.Clear();

            var defaultFrame = new AnimationFrame();
            for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
            {
                defaultFrame.BoneTransforms.Add(new AnimationKeyFrame()
                {
                    Transform = (_skeletonModel.Bones[i].Position),
                    BoneIndex = _skeletonModel.Bones[i].Index,
                    ParentBoneIndex = _skeletonModel.Bones[i].ParentIndex
                });
            }

            ApplyFrame(animateInPlace, _animation.StaticFrame, _animation.StaticTranslationMappingID, _animation.StaticRotationMappingID, defaultFrame);

            if (_animation.DynamicFrames.Count() == 0)
            {
                KeyFrameCollection.Add(defaultFrame); 
            }
            else
            {
                for (int frameIndex = 0; frameIndex < _animation.DynamicFrames.Count(); frameIndex++)
                {
                    var animationKeyFrameData = _animation.DynamicFrames[frameIndex];

                    var currentFrame = defaultFrame.Clone();
                    ApplyFrame(animateInPlace, animationKeyFrameData, _animation.DynamicTranslationMappingID, _animation.DynamicRotationMappingID, currentFrame);
                    KeyFrameCollection.Add(currentFrame);
                }
            }

            for (int frameIndex = 0; frameIndex < KeyFrameCollection.Count(); frameIndex++)
            {
                var currentFrame = KeyFrameCollection[frameIndex];

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
