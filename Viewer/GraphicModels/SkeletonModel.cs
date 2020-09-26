using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;
using static Viewer.Animation.AnimationClip;

namespace Viewer.GraphicModels
{


    public class SkeletonModel : IRenderableContent
    {
        public class BoneInfo
        {
            public Matrix Position { get; set; }
            public Matrix WorldPosition { get; set; }
            public Matrix Inv { get; set; }
            public int Index { get; set; }
            public int ParentIndex { get; set; }
            public string Name { get; set; }
        }

        public List<BoneInfo> Bones = new List<BoneInfo>();
        AnimationPlayer _animationPlayer;

        LineBox _lineBox = new LineBox();

        public void Create(AnimationPlayer animationPlayer, SkeletonFile skeleton)
        {
            _lineBox.Create();
            _animationPlayer = animationPlayer;
           
            for (int i = 0; i < skeleton.Bones.Count(); i++)
            {
                var x = new Microsoft.Xna.Framework.Quaternion(
                    skeleton.Bones[i].Rotation_X,
                    skeleton.Bones[i].Rotation_Y,
                    skeleton.Bones[i].Rotation_Z,
                    skeleton.Bones[i].Rotation_W);
                x.Normalize();

                var pos = Matrix.CreateFromQuaternion(x) * Matrix.CreateTranslation(skeleton.Bones[i].Position_X, skeleton.Bones[i].Position_Y, skeleton.Bones[i].Position_Z);
                var info = new BoneInfo()
                {
                    Index = skeleton.Bones[i].Id,
                    ParentIndex = skeleton.Bones[i].ParentId,
                    Position = pos,
                    WorldPosition = pos,
                    Inv = Matrix.Invert(pos),
                    Name = skeleton.Bones[i].Name
                };
                Bones.Add(info);
            }

            for (int i = 0; i < Bones.Count(); i++)
            {
                var parentIndex = Bones[i].ParentIndex;
                if (parentIndex == -1)
                    continue;
                Bones[i].WorldPosition = Bones[i].WorldPosition * Bones[parentIndex].WorldPosition;
            }
        }

        public void Render(Matrix world, GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            AnimationFrame frame = _animationPlayer.GetCurrentFrame();
             
            for (int i = 0; i < Bones.Count(); i++)
            {
                var parentIndex = Bones[i].ParentIndex;
                if (parentIndex == -1)
                    continue;

                var bonePos = Bones[i].WorldPosition;
                var parentBonePos = Bones[parentIndex].WorldPosition;

                if (frame != null)
                {
                    var currentBoneAnimationoffset = frame.BoneTransforms[i].Transform;
                    var parentBoneAnimationoffset = frame.BoneTransforms[parentIndex].Transform;

                    bonePos = Matrix.Multiply(bonePos, currentBoneAnimationoffset);
                    parentBonePos = Matrix.Multiply(parentBonePos, parentBoneAnimationoffset);
                }

                var vertices = new[]
                {
                    new VertexPositionNormalTexture(bonePos.Translation, new Vector3(0,0,0), new Vector2(0,0)),
                    new VertexPositionNormalTexture(parentBonePos.Translation, new Vector3(0,0,0), new Vector2(0,0))
                };

                MeshInstance instance = new MeshInstance()
                {
                    Model = _lineBox,
                };
                instance.Render(Matrix.CreateScale(0.05f) * bonePos, device, effect, effectPass);


                effect.Parameters["World"].SetValue(world);
                effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
                effectPass.Apply();
                device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                
            }
        }

        public void Dispose()
        {
        
        }
    }
}
