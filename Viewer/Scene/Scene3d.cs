using Filetypes.ByteParsing;
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
    public class Scene3d : WpfGame
    {
        private BasicEffect _basicEffect;
        private WpfKeyboard _keyboard;
        private WpfMouse _mouse;
        private Matrix _projectionMatrix;

        private bool _disposed;

        ArcBallCamera _camera;
        public List<MeshInstance> DrawBuffer = new List<MeshInstance>();

        public delegate void LoadSceneCallback(GraphicsDevice device);
        public LoadSceneCallback LoadScene { get; set; }

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

            LoadScene?.Invoke(GraphicsDevice);


            return;

            var path = @"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\";
            var models = new string[] { "brt_paladin_head_01","brt_paladin_head_04", "brt_paladin_torso_03", "brt_paladin_legs_01", "brt_paladin_torso_02" };

            /*
            VariantMeshDefinition.Create(path + "VariantMeshDef.txt");

            var skeletonByteChunk = ByteChunk.FromFile(path + @"Skeleton\humanoid01.anim");
            var skel = Skeleton.Create(skeletonByteChunk, out string tt);
            skelModel = SkeletonModel.Create(skel);

            var anim = ByteChunk.FromFile(path + "hu1_rifle_celebrate_02.anim");
            var anim_ = Animation.Create(anim);
            animationInfo = AnimationInformation.Create(anim_, skelModel);


            var modelChunk = ByteChunk.FromFile(path + models[3] + ".rigid_model_v2");
            _rTemp = RigidModel.Create(modelChunk, out var error);*/


            



        }

  


        CubeModel _cubeModel;
        MeshInstance _cube0;





        public class AnimationInformation
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
                public List<AnimationKeyFrame> BoneTransforms = new List<AnimationKeyFrame>();
            }

            public static AnimationInformation Create(Animation animation, SkeletonModel skeletonModel)
            {
                AnimationInformation model = new AnimationInformation();
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
                       var q = new Microsoft.Xna.Framework.Quaternion(animQ[0], animQ[1], animQ[2], animQ[3]);
                       q.Normalize();
                   
                       var mappingIdx = animation.RotationMappingID[0][i];
                       var translation = currentFrame.BoneTransforms[mappingIdx].Transform.Translation;
                        currentFrame.BoneTransforms[mappingIdx].Transform = Matrix.CreateFromQuaternion(q) * Matrix.CreateTranslation(translation);
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

                    model.Animation.Add(currentFrame);
                }

                return model;
            }
        }

        SkeletonModel skelModel;
        AnimationInformation animationInfo;


        

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
        
           //animIndex++;
           //if (animIndex >= animationInfo.Animation.Count)
           //    animIndex = 0;

            //animIndex = 0;

            /*_rmv2CompoundModel = new Rmv2CompoundModel();
            _rmv2CompoundModel.Create(GraphicsDevice, _rTemp, animationInfo, 0, animIndex);
            
            _rmv2Compound = new MeshInstance()
            {
                Model = _rmv2CompoundModel,
                World = Matrix.Identity
            };*/

            //The projection depends on viewport dimensions (aspect ratio).
            // Because WPF controls can be resized at any time (user resizes window)
            // we need to refresh the values each draw call, otherwise cube will look distorted to user
            RefreshProjection();

            GraphicsDevice.Clear( Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
     
            _basicEffect.View = _camera.ViewMatrix;

            _cube0.Render(GraphicsDevice, _basicEffect);
            foreach(var item in DrawBuffer)
                item.Render(GraphicsDevice, _basicEffect);

            //_rmv2Compound.Render(GraphicsDevice, _basicEffect);
            //_basicEffect.World = Matrix.Identity;
            //

            // foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            // {
            //     pass.Apply();
            //
            //    for (int i = 0; i < skelModel.Bones.Count; i++)
            //    {
            //        var parentIndex = skelModel.Bones[i].ParentIndex;
            //        if (parentIndex == -1)
            //            continue;
            //    
            //        var posA = Vector3.Transform(skelModel.Bones[i].WorldPosition.Translation,
            //            animationInfo.Animation[animIndex].BoneTransforms[i].Transform);
            //
            //        var posB = Vector3.Transform(skelModel.Bones[parentIndex].WorldPosition.Translation,
            //            animationInfo.Animation[animIndex].BoneTransforms[parentIndex].Transform);
            //    
            //        var vertices = new[]
            //        {
            //            new VertexPositionNormalTexture(posA, new Vector3(0,0,0), new Vector2(0,0)),
            //            new VertexPositionNormalTexture(posB, new Vector3(0,0,0), new Vector2(0,0))
            //        };
            //        GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
            //    }
            //
            // }


            base.Draw(time);
        }
    }
}
