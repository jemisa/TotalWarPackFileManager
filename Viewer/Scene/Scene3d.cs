using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.NHew;
using Viewer.Scene;


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
        public List<AnimationPlayer> AnimationPlayers = new List<AnimationPlayer>();

        public delegate void LoadSceneCallback(GraphicsDevice device);
        public LoadSceneCallback LoadScene { get; set; }

        CubeModel _cubeModel;
        MeshInstance _cube0;

        protected override void Initialize()
        {
            //ContentManager
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






        Effect _shader;
        Texture2D _texture;
        Skybox skybox;
        Model _cube;
        TextureCube _skyBoxTexture;
        Effect _reflectShader;
        TextureCube textureCube;
        public void CreateScene()
        {
            //CubemapGeneratorHelper cubemapGeneratorHelper = new CubemapGeneratorHelper();
            //cubemapGeneratorHelper.Create(@"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\Textures\CubeMaps\GamleStan", GraphicsDevice);
            ////textureCube = cubemapGeneratorHelper.CreateCubemapTexture("Blur", 28);
            ////cubemapGeneratorHelper.CreateCubemapTexture("Unprocessed", 0);
            //cubemapGeneratorHelper.SuperIm();
            //textureCube = cubemapGeneratorHelper.SimpleCubeMap();
          

            //var Content = new ContentManager(Services) { RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\MonoContentPipeline\bin\Windows\AnyCPU\Debug\Content" };
            var Content = new ContentManager(Services) 
            { 
                RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\bin\Windows"
            };
            _shader = Content.Load<Effect>("Shaders\\TestShader");
            _reflectShader = Content.Load<Effect>("Shaders\\Reflection");
            _cube = Content.Load<Model>("DebugModels/UntexturedSphere");
            _skyBoxTexture = Content.Load<TextureCube>("Textures//Sunset");
            // _texture = Content.Load<Texture2D>("ColorMap");
            //  var d  = new PipelineManager("", "", "");
            //  d.FindDefaultProcessor()
            _cubeModel = new CubeModel();
            _cubeModel.Create(GraphicsDevice);
            skybox = new Skybox("Textures/Sunset", Content);
            _cube0 = new MeshInstance()
            {
                Model = _cubeModel,
                World = Matrix.Identity * Matrix.CreateScale(0.05f)
            };
            //DrawBuffer.Add(_cube0);

            LoadScene?.Invoke(GraphicsDevice);
        }

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
                .01f, 20);
            _basicEffect.Projection = _projectionMatrix;
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = _mouse.GetState();
            var keyboardState = _keyboard.GetState();

            _camera.Update(mouseState, keyboardState);

            foreach (var player in AnimationPlayers)
                player.Update(gameTime);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime time)
        {
            RefreshProjection();

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _basicEffect.View = _camera.ViewMatrix;



            /**/
            float distance = 20;


            var angle = 0.002f;
            var cameraPosition = distance * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
            var view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);
           //RasterizerState originalRasterizerState = GraphicsDevice.RasterizerState;
           //RasterizerState rasterizerState = new RasterizerState();
           //rasterizerState.CullMode = CullMode.None;
           //GraphicsDevice.RasterizerState = rasterizerState;

            //skybox.Draw(_camera.ViewMatrix, _projectionMatrix, _camera.Position);

       //     GraphicsDevice.RasterizerState = originalRasterizerState;

            //DrawModelWithEffect(_reflectShader, _camera.Position, _cube, Matrix.Identity, _camera.ViewMatrix, _projectionMatrix);

            /*GraphicsDevice.RasterizerState.CullMode = CullMode.CullClockwiseFace;
            skybox.Draw(_camera.ViewMatrix, _projectionMatrix, _camera.Position);
            GraphicsDevice.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;*/

        //
        //    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        //    GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        //
            _shader.CurrentTechnique = _shader.Techniques["Diffuse"];

            LineBox tempBox = new LineBox();
            tempBox.Create();

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                

               
                foreach (var item in DrawBuffer)
                {
                    /*Matrix worldInverse = Matrix.Invert(Matrix.Identity);
                    Vector4 vLightDirection = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    Vector4 vecEye = new Vector4(x, y, zHeight, 0);
                    Vector4 vColorDiffuse = new Vector4(0.8f, 0.0f, 0.0f, 1.0f);
                    Vector4 vColorSpecular = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                    Vector4 vColorAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);*/


                    Vector4 vecEye = new Vector4(_camera.ViewMatrix.Translation, 0);
                    Vector4 vColorDiffuse = new Vector4(0.8f, 0.0f, 0.0f, 1.0f);
                    Vector4 vColorSpecular = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                    Vector4 vColorAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);

                    // .. and pass it into our shader.
                    // To access a parameter defined in our shader file ( Shader.fx ), use effectObject.Parameters["variableName"]
                    var worldMatrix = Matrix.Identity;
                    Matrix worldInverse = Matrix.Invert(worldMatrix);
                    Vector4 vLightDirection = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
                    
                    _shader.Parameters["View"].SetValue(_camera.ViewMatrix);
                    _shader.Parameters["Projection"].SetValue(_projectionMatrix);

            


                    item.Render(item.World, GraphicsDevice, _shader, pass);
                    
                }
            }

            //_cube0.Render(GraphicsDevice, _shader);
        

            base.Draw(time);
        }


        

    }
}
