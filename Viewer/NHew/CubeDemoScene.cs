﻿using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Viewer.GraphicModels;
using Viewer.NHew;


//assimpnet

namespace WpfTest.Scenes
{
    /// <summary>
    /// Displays a spinning cube and an fps counter. Background color defaults to <see cref="Color.CornflowerBlue"/> and changes to <see cref="Color.Black"/> while left mouse button down is registered.
    /// Note that this is just an example implementation of <see cref="WpfGame"/>.
    /// Based on: http://msdn.microsoft.com/en-us/library/bb203926(v=xnagamestudio.40).aspx
    /// </summary>
    public class CubeDemoScene : WpfGame
    {
        private BasicEffect _basicEffect;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        private Matrix _projectionMatrix;

        private bool _disposed;

        ArcBallCamera _camera;
        protected override void Initialize()
        {

            _disposed = false;
            new WpfGraphicsDeviceService(this);
            //Components.Add(new FpsComponent(this));
            //Components.Add(new TimingComponent(this));
            //Components.Add(new TextComponent(this, "Leftclick anywhere in the game to change background color", new Vector2(1, 0), HorizontalAlignment.Right));

            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);
            _basicEffect = ShaderConfiguration.CreateBasicEffect(GraphicsDevice);

   
            // Camera         
            _camera = new ArcBallCamera(1, new Vector3(0));
            _camera.NearPlane = 0.001f;
            _camera.Zoom = 10;

            RefreshProjection();
            CreateScene();

            base.Initialize();
        }

        public void CreateScene()
        {

            _cubeModel = new CubeModel();
            _cubeModel.Create(GraphicsDevice);

            _cube0 = new MeshInstance()
            {
                Model = _cubeModel,
                World = Matrix.Identity * Matrix.CreateScale(0.05f)
            };

            // Load scene


            var path = @"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\";
            var models = new string[] { /*"brt_paladin_head_01", "brt_paladin_head_04", "brt_paladin_torso_03", "brt_paladin_legs_01" ,*/ "brt_paladin_torso_02" };

            var skeletonByteChunk = ByteChunk.FromFile(path + @"Skeleton\humanoid01.anim");
            var skel = Skeleton.Create(skeletonByteChunk, out string tt);
            skelModel = SkeletonModel.Create(skel);

            var anim = ByteChunk.FromFile(path + "hu1_rifle_celebrate_02.anim");
            var anim_ = Animation.Create(anim);
            animModle = AnimationModel.Create(anim_, skelModel);


            var modelChunk = ByteChunk.FromFile(path + models[0] + ".rigid_model_v2");
            RigidModel rigidModel = RigidModel.Create(modelChunk, out var error);
            _rmv2CompoundModel = new Rmv2CompoundModel();
            _rmv2CompoundModel.Create(GraphicsDevice, rigidModel, 0);

            _rmv2Compound = new MeshInstance()
            {
                Model = _rmv2CompoundModel,
                World = Matrix.Identity
            };





        }

        CubeModel _cubeModel;
        MeshInstance _cube0;

        Rmv2CompoundModel _rmv2CompoundModel;
        MeshInstance _rmv2Compound;


        public class SkeletonModel
        {
            public class BoneInfo
            {
                public Matrix Position { get; set; }
                public Matrix WorldPosition { get; set; }
                public int Index { get; set; }
                public int ParentIndex { get; set; }
            }

            public List<VertexBuffer> Buffers = new List<VertexBuffer>();
            public List<BoneInfo> Bones = new List<BoneInfo>();

            public static SkeletonModel Create(Skeleton skeleton)
            {
                SkeletonModel model = new SkeletonModel();
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
                        WorldPosition = pos
                    };
                    model.Bones.Add(info);
                }


                for (int i = 0; i < model.Bones.Count(); i++)
                {
                    if (model.Bones[i].ParentIndex == -1)
                        continue;
                    model.Bones[i].WorldPosition = model.Bones[i].WorldPosition * model.Bones[model.Bones[i].ParentIndex].WorldPosition;
                }

                return model;
            }
        }

        public class AnimationModel
        {
            public class AnimationKeyFrame
            { 
                public int BoneIndex { get; set; }
                public int ParentBoneIndex { get; set; }
                public Matrix Transform { get; set; }
            }

            public List<AnimationFrame> Animation = new List<AnimationFrame>();

            public class AnimationFrame
            {
                public List<AnimationKeyFrame> KeyFrames = new List<AnimationKeyFrame>();
            }

            public static AnimationModel Create(Animation animation, SkeletonModel skeletonModel)
            {
                AnimationModel model = new AnimationModel();
                for (int frameIndex = 0; frameIndex < animation.Frames.Count(); frameIndex++)
                {
                    var originalData = animation.Frames[frameIndex];
                    var currentAnimationFrame = new AnimationFrame();
                    
                    // Copy base pose
                    for (int i = 0; i < skeletonModel.Bones.Count(); i++)
                    {
                        currentAnimationFrame.KeyFrames.Add(new AnimationKeyFrame()
                        {
                            Transform = skeletonModel.Bones[i].Position,
                            BoneIndex = skeletonModel.Bones[i].Index,
                            ParentBoneIndex = skeletonModel.Bones[i].ParentIndex
                        });
                    }

                    // Apply animation
                    for (int i = 0; i < originalData.Transforms.Count(); i++)
                    {
                        var index = animation.posIDArr[0][i];
                        var pos = originalData.Transforms[i];
                        var temp = currentAnimationFrame.KeyFrames[index].Transform;
                        temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                        currentAnimationFrame.KeyFrames[index].Transform = temp;
                    }

                    for (int i = 0; i < originalData.Quaternion.Count(); i++)
                    {
                        var index = animation.rotIDArr[0][i];

                        var q = new Microsoft.Xna.Framework.Quaternion(
                             originalData.Quaternion[i][0],
                             originalData.Quaternion[i][1],
                             originalData.Quaternion[i][2],
                             originalData.Quaternion[i][3]);
                        q.Normalize();

                        var translation = currentAnimationFrame.KeyFrames[index].Transform.Translation;
                        currentAnimationFrame.KeyFrames[index].Transform = Matrix.CreateFromQuaternion(q) * Matrix.CreateTranslation(translation.X, translation.Y, translation.Z); ;
                    }

                    // Convert to worldspace
                    for (int i = 0; i < currentAnimationFrame.KeyFrames.Count(); i++)
                    {
                        var parentindex = currentAnimationFrame.KeyFrames[i].ParentBoneIndex;
                        if (parentindex == -1)
                            continue;
                        currentAnimationFrame.KeyFrames[i].Transform = currentAnimationFrame.KeyFrames[i].Transform * currentAnimationFrame.KeyFrames[parentindex].Transform;
                    }
                    model.Animation.Add(currentAnimationFrame);
                }

                return model;
            }
        }

        SkeletonModel skelModel;
        AnimationModel animModle;


        

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Components.Clear();
            _disposed = true;

            _basicEffect.Dispose();
            _basicEffect = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Update projection matrix values, both in the calculated matrix <see cref="_projectionMatrix"/> as well as
        /// the <see cref="_basicEffect"/> projection.
        /// </summary>
        private void RefreshProjection()
        {
            _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), // 45 degree angle
                (float)GraphicsDevice.Viewport.Width /
                (float)GraphicsDevice.Viewport.Height,
                .001f, 100.0f);
            _basicEffect.Projection = _projectionMatrix;
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            _camera.Update(mouseState, keyboardState);
            base.Update(gameTime);
        }

        int animIndex = 0;
        protected override void Draw(GameTime time)
        {
            //The projection depends on viewport dimensions (aspect ratio).
            // Because WPF controls can be resized at any time (user resizes window)
            // we need to refresh the values each draw call, otherwise cube will look distorted to user
            RefreshProjection();

            GraphicsDevice.Clear( Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
     
            _basicEffect.View = _camera.ViewMatrix;

            _cube0.Render(GraphicsDevice, _basicEffect);
            _rmv2Compound.Render(GraphicsDevice, _basicEffect);
            _basicEffect.World = Matrix.Identity;
           
            
            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                animIndex++;
                if (animIndex >= animModle.Animation.Count)
                    animIndex = 0;
                pass.Apply();
                for (int i = 0; i < animModle.Animation[animIndex].KeyFrames.Count; i++)
                {
                    if (animModle.Animation[animIndex].KeyFrames[i].ParentBoneIndex == -1)
                        continue;

                    var parentIndex = animModle.Animation[animIndex].KeyFrames[i].ParentBoneIndex;
                    var posA = animModle.Animation[animIndex].KeyFrames[i].Transform;
                    var posB = animModle.Animation[animIndex].KeyFrames[parentIndex].Transform;

                    var vertices = new[]
                    {
                        new VertexPositionNormalTexture(posA.Translation, new Vector3(0,0,0), new Vector2(0,0)),
                        new VertexPositionNormalTexture(posB.Translation, new Vector3(0,0,0), new Vector2(0,0))
                    };
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }
            }
                        

            base.Draw(time);
        }
    }
}
