using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;
using WpfTest.Scenes;
using static Viewer.Animation.AnimationClip;

namespace Viewer.GraphicModels
{


    public class SkeletonEntity
    {
        public void Update()
        { 
            // Updete skeleton
            // Update boxes
        }

        public void Render()
        { 
            // Draw skeleton
            // Draw boxes
        }
    
    
    }


    public class SkeletonModel : RenderItem
    {
        public class BoneInfo
        {
            public Matrix Position { get; set; }
            public Matrix WorldPosition { get; set; }
            public Matrix AnimatedPosition { get; set; }
            public int Index { get; set; }
            public int ParentIndex { get; set; }
            public string Name { get; set; }
        }

        public List<BoneInfo> Bones = new List<BoneInfo>();
        AnimationPlayer _animationPlayer;

        LineBox _lineBox = new LineBox();

        public SkeletonModel(Effect shader) : base(null, shader)
        {
        }

        public int GetBoneIndex(string name)
        {
            for (int i = 0; i < Bones.Count(); i++)
            {
                if (Bones[i].Name == name)
                    return i;
            }

            return -1;
        }

        public Matrix GetAnimatedBone(int index)
        {
            return Bones[index].AnimatedPosition;
        }

        public void Create(AnimationPlayer animationPlayer, AnimationFile skeleton)
        {
            _lineBox.Create();
            _animationPlayer = animationPlayer;
           
            for (int i = 0; i < skeleton.Bones.Count(); i++)
            {
                var x = new Quaternion(
                    skeleton.DynamicFrames[0].Quaternion[i][0],
                    skeleton.DynamicFrames[0].Quaternion[i][1],
                    skeleton.DynamicFrames[0].Quaternion[i][2],
                    skeleton.DynamicFrames[0].Quaternion[i][3]);
                x.Normalize();

                var scale = Matrix.CreateScale(1); 
                if (i == 0)
                    scale = Matrix.CreateScale(-1, 1, 1);
                var pos = scale * Matrix.CreateFromQuaternion(x) * Matrix.CreateTranslation(skeleton.DynamicFrames[0].Transforms[i].X, skeleton.DynamicFrames[0].Transforms[i].Y, skeleton.DynamicFrames[0].Transforms[i].Z);
                var info = new BoneInfo()
                {
                    Index = skeleton.Bones[i].Id,
                    ParentIndex = skeleton.Bones[i].ParentId,
                    Position = pos,
                    WorldPosition = pos,
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
                Bones[i].AnimatedPosition = Bones[i].WorldPosition;
            }
        }

        public override void Update(GameTime time)
        {
            AnimationFrame frame = _animationPlayer.GetCurrentFrame();

            for (int i = 0; i < Bones.Count(); i++)
            {
                var parentIndex = Bones[i].ParentIndex;
                if (parentIndex == -1)
                    continue;

                if (frame != null)
                {
                    var bonePos = Bones[i].WorldPosition;
                    var parentBonePos = Bones[parentIndex].WorldPosition;

                    var currentBoneAnimationoffset = frame.BoneTransforms[i].Transform;
                    var parentBoneAnimationoffset = frame.BoneTransforms[parentIndex].Transform;

                    Bones[i].AnimatedPosition = Matrix.Multiply(bonePos, currentBoneAnimationoffset);
                    Bones[parentIndex].AnimatedPosition = Matrix.Multiply(parentBonePos, parentBoneAnimationoffset);
                }
            }
        }

        public override void Draw(GraphicsDevice device, Matrix world, CommonShaderParameters commonShaderParameters)
        {
            if (!Visible)
                return;

            for (int i = 0; i < Bones.Count(); i++)
            {
                var parentIndex = Bones[i].ParentIndex;
                if (parentIndex == -1)
                    continue;

                var vertices = new[]
                {
                    new VertexPosition(Bones[i].AnimatedPosition.Translation),
                    new VertexPosition(Bones[parentIndex].AnimatedPosition.Translation)
                };

                foreach (var pass in _shader.CurrentTechnique.Passes)
                {
                    ApplyCommonShaderParameters(commonShaderParameters, Matrix.Identity);
                    _shader.Parameters["Color"].SetValue(new Vector3(0,0,0));
                    pass.Apply();
                    device.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }

                DrawCube(device, commonShaderParameters, Matrix.CreateScale(0.05f) * Bones[i].AnimatedPosition);
            }
        }

        void DrawCube(GraphicsDevice device, CommonShaderParameters commonShaderParameters, Matrix world)
        {
            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                ApplyCommonShaderParameters(commonShaderParameters, world);
                _shader.Parameters["Color"].SetValue(new Vector3(.25f, 1, .25f));
                pass.Apply();
                _lineBox.Render(device);
            }
        }
    }
}
