using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
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
        private KeyboardState _keyboardState;
        private WpfMouse _mouse;
        private MouseState _mouseState;
        private Matrix _projectionMatrix;
        private List<VertexBuffer> _vertexBuffer;
        private VertexDeclaration _vertexDeclaration;
        private Matrix _viewMatrix;
        private Matrix _worldMatrix;
        private bool _disposed;

        ArcBallCamera _camera2;
        protected override void Initialize()
        {

   

            _disposed = false;
            new WpfGraphicsDeviceService(this);
            //Components.Add(new FpsComponent(this));
            //Components.Add(new TimingComponent(this));
            //Components.Add(new TextComponent(this, "Leftclick anywhere in the game to change background color", new Vector2(1, 0), HorizontalAlignment.Right));

            float tilt = MathHelper.ToRadians(0);  // 0 degree angle
                                                   // Use the world matrix to tilt the cube along x and y axes.
            _worldMatrix = Matrix.CreateRotationX(tilt) * Matrix.CreateRotationY(tilt);
            _viewMatrix = Matrix.CreateLookAt(new Vector3(5, 5, 5), Vector3.Zero, Vector3.Up);

            _basicEffect = new BasicEffect(GraphicsDevice);

            _basicEffect.World = _worldMatrix;
            _basicEffect.View = _viewMatrix;
            RefreshProjection();

            // primitive color
            _basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            _basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            _basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            _basicEffect.SpecularPower = 5.0f;
            _basicEffect.Alpha = 1.0f;
           

            _basicEffect.LightingEnabled = true;
            if (_basicEffect.LightingEnabled)
            {
                _basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
                if (_basicEffect.DirectionalLight0.Enabled)
                {
                    // x direction
                    _basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f); // range is 0 to 1
                    _basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
                    // points from the light to the origin of the scene
                    _basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
                }

                _basicEffect.DirectionalLight1.Enabled = true;
                if (_basicEffect.DirectionalLight1.Enabled)
                {
                    // y direction
                    _basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                    _basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
                    _basicEffect.DirectionalLight1.SpecularColor = Vector3.One;
                }

                _basicEffect.DirectionalLight2.Enabled = true;
                if (_basicEffect.DirectionalLight2.Enabled)
                {
                    // z direction
                    _basicEffect.DirectionalLight2.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
                    _basicEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
                    _basicEffect.DirectionalLight2.SpecularColor = Vector3.One;
                }
            }

            _vertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );

   
            _vertexBuffer = CreateModel();
            _keyboard = new WpfKeyboard(this);
            _mouse = new WpfMouse(this);



            _camera2 = new ArcBallCamera(1, new Vector3(0));
            _camera2.NearPlane = 0.001f;
            _camera2.Zoom = 10;
            base.Initialize();
        }




        class SkeletonModel
        {
            public class BoneInfo
            {
                public Matrix Position { get; set; }
                public Matrix WorldPosition { get; set; }
                public int Index { get; set; }
                public int ParentIndex { get; set; }
            }

            public List<VertexBuffer> Buffers = new List<VertexBuffer>();
            VertexDeclaration _vertDec;

            public List<BoneInfo> Bones = new List<BoneInfo>();
            public static SkeletonModel Create(Skeleton skeleton, GraphicsDevice graphicsDevice)
            {
                SkeletonModel model = new SkeletonModel();
                for (int i = 0; i < skeleton.Bones.Count(); i++)
                {
                    var x = new Quaternion(
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

        class AnimationModel
        {
            /*public class BoneInfo
            {
                public Matrix Position { get; set; }
                public Matrix WorldPosition { get; set; }
                public int Index { get; set; }
                public int ParentIndex { get; set; }
            }*/

            public List<VertexBuffer> Buffers = new List<VertexBuffer>();
            VertexDeclaration _vertDec;

            public List<SkeletonModel.BoneInfo> Bones = new List<SkeletonModel.BoneInfo>();
            public List<Matrix> Animation = new List<Matrix>();
            public static AnimationModel Create(Animation animation, Skeleton skeleton, GraphicsDevice graphicsDevice)
            {
                AnimationModel model = new AnimationModel();
                for (int i = 0; i < skeleton.Bones.Count(); i++)
                {
                    var x = new Quaternion(
                        skeleton.Bones[i].Rotation_X,
                        skeleton.Bones[i].Rotation_Y,
                        skeleton.Bones[i].Rotation_Z,
                        skeleton.Bones[i].Rotation_W);
                    x.Normalize();

                    var pos = Matrix.CreateFromQuaternion(x) * Matrix.CreateTranslation(skeleton.Bones[i].Position_X, skeleton.Bones[i].Position_Y, skeleton.Bones[i].Position_Z);
                    var info = new SkeletonModel.BoneInfo()
                    {
                        Index = skeleton.Bones[i].Id,
                        ParentIndex = skeleton.Bones[i].ParentId,
                        Position = pos,
                        WorldPosition = pos
                    };
                    model.Bones.Add(info);
                }


                /*
                 for (int i=0; i < numInfluenceBones-1; i++) // N.B., the last boneweight is not need! 
{ 
	fWeight = boneWeight[ i ]; 
	vertexPos += inputVertexPos * final_transform[ i ] * fWeight; 
	fLastWeight -= fWeight; 
} 

                 */

                //for (int i = 0; i < model.Bones.Count(); i++)
                //{
                //    if (model.Bones[i].ParentIndex == -1)
                //        continue;
                //    model.Bones[i].WorldPosition = model.Bones[i].WorldPosition * model.Bones[model.Bones[i].ParentIndex].WorldPosition;
                //}

                // Apply animation
                var frame = animation.Frames[25];
                for (int i = 0; i < model.Bones.Count(); i++)
                    model.Animation.Add(model.Bones[i].WorldPosition);

                for (int i = 0; i < frame.Positions.Count(); i++)
                {
                    var index = animation.posIDArr[0][i];
                    var pos = frame.Positions[i];
                    var temp = model.Animation[index];
                    temp.Translation = new Vector3(pos.X, pos.Y, pos.Z);
                    model.Animation[index] = temp;
                }

                for (int i = 0; i < frame.Quat.Count(); i++)
                {
                    var index = animation.rotIDArr[0][i];

                    var q = new Quaternion(
                         frame.Quat[i][0],
                         frame.Quat[i][1],
                         frame.Quat[i][2],
                         frame.Quat[i][3]);
                    q.Normalize();

                   var translation = model.Bones[index].WorldPosition.Translation;
                    model.Animation[index] = Matrix.CreateFromQuaternion(q) *   Matrix.CreateTranslation(translation.X, translation.Y, translation.Z); ;
                    //model.Animation[index].Translation = translation;
                    //var temp = model.Animation[index];
                    //temp.Translation = translation;
                    //model.Animation[index] = temp;
                }

              for (int i = 0; i < model.Animation.Count(); i++)
              {
                  var parentindex = model.Bones[i].ParentIndex;
                  if (model.Bones[i].ParentIndex == -1)
                      continue;
                  model.Animation[i] = model.Animation[i] * model.Animation[parentindex];
              }

                //for (int i = 0; i < model.Bones.Count(); i++)
                //{
                //    if (model.Bones[i].ParentIndex == -1)
                //        continue;
                //    model.Bones[i].WorldPosition = model.Bones[i].WorldPosition * model.Animation[i];
                //}

                return model;
            }
        }

        SkeletonModel skelModel;
        AnimationModel animModle;
        List<VertexBuffer> CreateModel()
        {
            List<VertexBuffer> outputList = new List<VertexBuffer>();

            var model = @"C:\temp\datafiles\vmp_black_coach_01.rigid_model_v2";
            var path = @"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\";
            var models = new string[] { /*"brt_paladin_head_01", "brt_paladin_head_04", "brt_paladin_torso_03", "brt_paladin_legs_01" ,*/ "brt_paladin_torso_02" };


             var skeletonByteChunk = ByteChunk.FromFile(path + @"Skeleton\humanoid01.anim");
           
           var skel = Skeleton.Create(skeletonByteChunk, out string tt);
            skelModel = SkeletonModel.Create(skel, GraphicsDevice);

            var anim = ByteChunk.FromFile(path + "hu1_rifle_celebrate_02.anim");
            var anim_ = Animation.Create(anim);
            animModle = AnimationModel.Create(anim_, skel, GraphicsDevice);
            //var chunk = ByteChunk.FromFile(@"C:\Users\ole_k\Downloads\sphere_coord_1_2_3_r_4_scaled_2_5_2_rotated_90_0_0.rigid_model_v2");
            for (int i = 0; i < models.Length; i++)
            {
                var chunk = ByteChunk.FromFile(path + models[i] + ".rigid_model_v2");
                //var chunk = ByteChunk.FromFile(model);
                RigidModel rigidModel = RigidModel.Create(chunk, out var error);

                var lodModel = rigidModel.LodInformations[0].LodModels[0];
                var cubeVertices = new VertexPositionNormalTexture[lodModel.IndicesBuffer.Length];

                for (int j = 0; j < lodModel.IndicesBuffer.Length; j++)
                {
                    var index = lodModel.IndicesBuffer[j];
                    var vert = lodModel.VertexArray[index];

                    Vector3 vertexPos = new Vector3(vert.X, vert.Y, vert.Z);
                    Vector3 normal = new Vector3(vert.Normal_X, vert.Normal_Y, vert.Normal_Z);
                    Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
                    cubeVertices[j] = new VertexPositionNormalTexture(vertexPos, normal, textureTopLeft);
                }

                VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, _vertexDeclaration, cubeVertices.Length, BufferUsage.None);
                vertexBuffer.SetData(cubeVertices);
                outputList.Add(vertexBuffer);
            }
            
            
            
            return outputList;
        }

        VertexBuffer CreateBuffer()
        {
            Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, 1.0f);
            Vector3 bottomLeftFront = new Vector3(-1.0f, -1.0f, 1.0f);
            Vector3 topRightFront = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 bottomRightFront = new Vector3(1.0f, -1.0f, 1.0f);
            Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, -1.0f);
            Vector3 topRightBack = new Vector3(1.0f, 1.0f, -1.0f);
            Vector3 bottomLeftBack = new Vector3(-1.0f, -1.0f, -1.0f);
            Vector3 bottomRightBack = new Vector3(1.0f, -1.0f, -1.0f);

            Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureTopRight = new Vector2(1.0f, 0.0f);
            Vector2 textureBottomLeft = new Vector2(0.0f, 1.0f);
            Vector2 textureBottomRight = new Vector2(1.0f, 1.0f);

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            var cubeVertices = new VertexPositionNormalTexture[36];



            // Front face.
            cubeVertices[0] = new VertexPositionNormalTexture(topLeftFront, frontNormal, textureTopLeft);
            cubeVertices[1] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
            cubeVertices[2] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);
            cubeVertices[3] = new VertexPositionNormalTexture(bottomLeftFront, frontNormal, textureBottomLeft);
            cubeVertices[4] = new VertexPositionNormalTexture(bottomRightFront, frontNormal, textureBottomRight);
            cubeVertices[5] = new VertexPositionNormalTexture(topRightFront, frontNormal, textureTopRight);

            // Back face.
            cubeVertices[6] = new VertexPositionNormalTexture(topLeftBack, backNormal, textureTopRight);
            cubeVertices[7] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
            cubeVertices[8] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
            cubeVertices[9] = new VertexPositionNormalTexture(bottomLeftBack, backNormal, textureBottomRight);
            cubeVertices[10] = new VertexPositionNormalTexture(topRightBack, backNormal, textureTopLeft);
            cubeVertices[11] = new VertexPositionNormalTexture(bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            cubeVertices[12] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
            cubeVertices[13] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);
            cubeVertices[14] = new VertexPositionNormalTexture(topLeftBack, topNormal, textureTopLeft);
            cubeVertices[15] = new VertexPositionNormalTexture(topLeftFront, topNormal, textureBottomLeft);
            cubeVertices[16] = new VertexPositionNormalTexture(topRightFront, topNormal, textureBottomRight);
            cubeVertices[17] = new VertexPositionNormalTexture(topRightBack, topNormal, textureTopRight);

            // Bottom face.
            cubeVertices[18] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
            cubeVertices[19] = new VertexPositionNormalTexture(bottomLeftBack, bottomNormal, textureBottomLeft);
            cubeVertices[20] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
            cubeVertices[21] = new VertexPositionNormalTexture(bottomLeftFront, bottomNormal, textureTopLeft);
            cubeVertices[22] = new VertexPositionNormalTexture(bottomRightBack, bottomNormal, textureBottomRight);
            cubeVertices[23] = new VertexPositionNormalTexture(bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            cubeVertices[24] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);
            cubeVertices[25] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
            cubeVertices[26] = new VertexPositionNormalTexture(bottomLeftFront, leftNormal, textureBottomRight);
            cubeVertices[27] = new VertexPositionNormalTexture(topLeftBack, leftNormal, textureTopLeft);
            cubeVertices[28] = new VertexPositionNormalTexture(bottomLeftBack, leftNormal, textureBottomLeft);
            cubeVertices[29] = new VertexPositionNormalTexture(topLeftFront, leftNormal, textureTopRight);

            // Right face.
            cubeVertices[30] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
            cubeVertices[31] = new VertexPositionNormalTexture(bottomRightFront, rightNormal, textureBottomLeft);
            cubeVertices[32] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);
            cubeVertices[33] = new VertexPositionNormalTexture(topRightBack, rightNormal, textureTopRight);
            cubeVertices[34] = new VertexPositionNormalTexture(topRightFront, rightNormal, textureTopLeft);
            cubeVertices[35] = new VertexPositionNormalTexture(bottomRightBack, rightNormal, textureBottomRight);

            VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, _vertexDeclaration, cubeVertices.Length, BufferUsage.None);
            vertexBuffer.SetData(cubeVertices);
            return vertexBuffer;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Components.Clear();
            _disposed = true;

            //_vertexBuffer.Dispose();
            _vertexBuffer = null;

            _vertexDeclaration.Dispose();
            _vertexDeclaration = null;

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

            _mouseState = _mouse.GetState();
            _keyboardState = _keyboard.GetState();

            if (_mouseState.LeftButton == ButtonState.Pressed)
            {
                if (_mousePressedLastFrame == false)
                {
                    Console.WriteLine("Pressed");
                    mouseX = _mouseState.X;
                    mouseY = _mouseState.Y;
                }
                else
                {
                    var speed = 0.01f;
                    var diffX = mouseX - _mouseState.X;
                    var diffY = mouseY - _mouseState.Y;
                    mouseX = _mouseState.X;
                    mouseY = _mouseState.Y;
                    _camera2.Yaw += diffX * speed;
                    _camera2.Pitch += diffY * speed;
                    Console.WriteLine($"{diffX }, {diffY} - {_camera2.Yaw}, {_camera2.Pitch }");
                }
                _mousePressedLastFrame = true;
            }

            if (_mouseState.LeftButton == ButtonState.Released && _mousePressedLastFrame)
            {
                _mousePressedLastFrame = false;
                Console.WriteLine("Release");
            }

            var moseSpeed = -0.5f;
            if (_keyboardState.IsKeyDown(Keys.W))
            {
                _camera2.Zoom +=moseSpeed * 0.1f;
            }

 
            if (_keyboardState.IsKeyDown(Keys.S))
            {
                _camera2.Zoom -= moseSpeed * 0.1f;
            }
            if (_keyboardState.IsKeyDown(Keys.Q))
            {
                _camera2.MoveCameraUp(moseSpeed * 0.1f);
            }

            if (_keyboardState.IsKeyDown(Keys.E))
            {
                _camera2.MoveCameraUp(-moseSpeed*0.1f);
            }

            if (_keyboardState.IsKeyDown(Keys.Space))
                toggle = true;
            else
                toggle = false;

            // camera.Update(gameTime);
            base.Update(gameTime);
        }

        bool _mousePressedLastFrame = false;

        float mouseX;
        float mouseY;
        private float _rotation;
        bool toggle = false;
        protected override void Draw(GameTime time)
        {
            //The projection depends on viewport dimensions (aspect ratio).
            // Because WPF controls can be resized at any time (user resizes window)
            // we need to refresh the values each draw call, otherwise cube will look distorted to user
            RefreshProjection();

            GraphicsDevice.Clear( Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
     


            // Rotate cube around up-axis.
            // only update cube when the game is active
            //if (IsActive)
            //    _rotation += (float)time.ElapsedGameTime.TotalMilliseconds / 1000 * MathHelper.TwoPi;
            //_basicEffect.World = Matrix.CreateRotationY(_rotation) * _worldMatrix;
            //_basicEffect.View = _viewMatrix;

            //_basicEffect.World = Matrix.CreateTranslation(-camera.cameraPosition);
            //_basicEffect.Projection = _camera2.ProjectionMatrix;
            _basicEffect.View = _camera2.ViewMatrix;

           foreach (var mesh in _vertexBuffer)
           {
               GraphicsDevice.SetVertexBuffer(mesh);
           
               foreach (var pass in _basicEffect.CurrentTechnique.Passes)
               {
                   pass.Apply();
                   GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, mesh.VertexCount);
               }
           }

            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {

                //GraphicsDevice.SetVertexBuffer(skelModel.Buffers[0]);
                pass.Apply();

                if(toggle)
                for (int i = 0; i < animModle.Bones.Count; i++)
                {
                    if (animModle.Bones[i].ParentIndex == -1)
                        continue;
                
                    var posA = animModle.Bones[i].WorldPosition;
                    var posB = animModle.Bones[animModle.Bones[i].ParentIndex].WorldPosition;
                
                    var vertices = new[]
                    {
                        new VertexPositionNormalTexture(posA.Translation, new Vector3(0,0,0), new Vector2(0,0)),
                        new VertexPositionNormalTexture(posB.Translation, new Vector3(0,0,0), new Vector2(0,0))
                    };
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                
                }
                if(!toggle)
                for (int i = 0; i < animModle.Bones.Count; i++)
                {
                    if (animModle.Bones[i].ParentIndex == -1)
                        continue;

                    var index = i;
                    var parentIndex = animModle.Bones[i].ParentIndex;
                    var posA = animModle.Animation[i];
                    var posB = animModle.Animation[parentIndex];

                    var vertices = new[]
                    {
                        new VertexPositionNormalTexture(posA.Translation, new Vector3(0,0,0), new Vector2(0,0)),
                        new VertexPositionNormalTexture(posB.Translation, new Vector3(0,0,0), new Vector2(0,0))
                    };
                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);

                }

                // GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, skelModel.Buffers[0], 0, 1);
            }


                base.Draw(time);
        }
    }
}
