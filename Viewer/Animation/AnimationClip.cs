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
        AnimationFile _skeletonModel;

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

            Matrix[] skeletonTransform = new Matrix[_skeletonModel.Bones.Count()];
            Matrix[] skeletonWorldTransform = new Matrix[_skeletonModel.Bones.Count()];

            int skeletonWeirdIndex = 0;
            for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
            {
                var x = new Quaternion(
                    _skeletonModel.DynamicFrames[skeletonWeirdIndex].Quaternion[i][0],
                    _skeletonModel.DynamicFrames[skeletonWeirdIndex].Quaternion[i][1],
                    _skeletonModel.DynamicFrames[skeletonWeirdIndex].Quaternion[i][2],
                    _skeletonModel.DynamicFrames[skeletonWeirdIndex].Quaternion[i][3]);
                x.Normalize();

                var scale = Matrix.CreateScale(1);
                if (i == 0)
                    scale = Matrix.CreateScale(-1, 1, 1);
                var pos = scale * Matrix.CreateFromQuaternion(x) * 
                    Matrix.CreateTranslation(
                        _skeletonModel.DynamicFrames[skeletonWeirdIndex].Transforms[i].X, 
                        _skeletonModel.DynamicFrames[skeletonWeirdIndex].Transforms[i].Y, 
                        _skeletonModel.DynamicFrames[skeletonWeirdIndex].Transforms[i].Z);

                skeletonTransform[i] = pos;
                skeletonWorldTransform[i] = pos;
            }

            for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
            {
                var parentIndex = _skeletonModel.Bones[i].ParentId;
                if (parentIndex == -1)
                    continue;
                skeletonWorldTransform[i] = skeletonWorldTransform[i] * skeletonWorldTransform[parentIndex];
            }


            var defaultFrame = new AnimationFrame();
            for (int i = 0; i < _skeletonModel.Bones.Count(); i++)
            {
                defaultFrame.BoneTransforms.Add(new AnimationKeyFrame()
                {
                    Transform = (skeletonTransform[i]),
                    BoneIndex = _skeletonModel.Bones[i].Id,
                    ParentBoneIndex = _skeletonModel.Bones[i].ParentId
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
                    var inv = Matrix.Invert(skeletonWorldTransform[i]);
                    currentFrame.BoneTransforms[i].Transform = Matrix.Multiply(inv, currentFrame.BoneTransforms[i].Transform);
                }
            }
        }



        public static AnimationClip Create(AnimationFile animation, AnimationFile skeletonModel)
        {
            AnimationClip model = new AnimationClip();
            model._animation = animation;
            model._skeletonModel = skeletonModel;
            model.ReCreate(false);
            return model;


        }
    }
}
