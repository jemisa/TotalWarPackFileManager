using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
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
        public List<RenderItem> DrawBuffer = new List<RenderItem>();
        public List<AnimationPlayer> AnimationPlayers = new List<AnimationPlayer>();

        public delegate void LoadSceneCallback(GraphicsDevice device);
        public LoadSceneCallback LoadScene { get; set; }

        CubeModel _cubeModel;
      


        ResourceLibary _resourceLibary;

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

        public void SetResourceLibary(ResourceLibary resourceLibary)
        {
            _resourceLibary = resourceLibary;
            _resourceLibary.XnaContentManager = new ContentManager(Services)
            {
                RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\bin\Windows"
            };
            _resourceLibary.LoadEffect("Shaders\\TestShader", ShaderTypes.Mesh);
            _resourceLibary.LoadEffect("Shaders\\LineShader", ShaderTypes.Line);

        }




        //Effect _shader;
        //Effect _lineShader;
        //Texture2D _texture;
        //Skybox skybox;
        //Model _cube;
        //TextureCube _skyBoxTexture;
        //Effect _reflectShader;
        //TextureCube textureCube;
        public void CreateScene()
        {
            //CubemapGeneratorHelper cubemapGeneratorHelper = new CubemapGeneratorHelper();
            //cubemapGeneratorHelper.Create(@"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\Textures\CubeMaps\GamleStan", GraphicsDevice);
            ////textureCube = cubemapGeneratorHelper.CreateCubemapTexture("Blur", 28);
            ////cubemapGeneratorHelper.CreateCubemapTexture("Unprocessed", 0);
            //cubemapGeneratorHelper.SuperIm();
            //textureCube = cubemapGeneratorHelper.SimpleCubeMap();
          

            //var Content = new ContentManager(Services) { RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\MonoContentPipeline\bin\Windows\AnyCPU\Debug\Content" };
            //var Content = new ContentManager(Services) 
            //{ 
            //    RootDirectory = @"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\bin\Windows"
            //};
            //_shader = Content.Load<Effect>("Shaders\\TestShader");
            //_lineShader = Content.Load<Effect>("Shaders\\LineShader");
            //_reflectShader = Content.Load<Effect>("Shaders\\Reflection");
            //_cube = Content.Load<Model>("DebugModels/UntexturedSphere");
            //_skyBoxTexture = Content.Load<TextureCube>("Textures//Sunset");
            //// _texture = Content.Load<Texture2D>("ColorMap");
            ////  var d  = new PipelineManager("", "", "");
            ////  d.FindDefaultProcessor()
            //_cubeModel = new CubeModel();
            //_cubeModel.Create(GraphicsDevice);
            //skybox = new Skybox("Textures/Sunset", Content);
            //_cube0 = new MeshInstance()
            //{
            //    Model = _cubeModel,
            //    World = Matrix.Identity * Matrix.CreateScale(0.05f)
            //};
            ////DrawBuffer.Add(_cube0);

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
            //_shader.CurrentTechnique = _shader.Techniques["Diffuse"];


            CommonShaderParameters commonShaderParameters = new CommonShaderParameters()
            {
                Projection = _projectionMatrix,
                View = _camera.ViewMatrix
            };

            foreach (var item in DrawBuffer)
                item.Update(time);

            foreach (var item in DrawBuffer)
                item.Draw(GraphicsDevice, commonShaderParameters);

            base.Draw(time);
        }
    }

    public class CommonShaderParameters
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
    }


    public class RenderItem 
    {
        protected IRenderableContent _model;
        protected Effect _shader;
        public bool Visible { get; set; } = true;
        public Matrix World { get; set; } = Matrix.Identity;

        public RenderItem(IRenderableContent model, Effect shader)
        {
            _model = model;
            _shader = shader;
        }

        public virtual void Draw(GraphicsDevice device, CommonShaderParameters commonShaderParameters)
        {
            if (!Visible)
                return;

            foreach (var pass in _shader.CurrentTechnique.Passes)
            {
                ApplyCommonShaderParameters(commonShaderParameters, World);
                ApplyCustomShaderParams();
                pass.Apply();
                _model.Render(device);
            }
        }

        protected void ApplyCommonShaderParameters(CommonShaderParameters commonShaderParameters, Matrix world)
        {
            _shader.Parameters["View"].SetValue(commonShaderParameters.View);
            _shader.Parameters["Projection"].SetValue(commonShaderParameters.Projection);
            _shader.Parameters["World"].SetValue(world);
            _shader.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
        }

        public virtual void ApplyCustomShaderParams()
        { 
        
        }

        public virtual void Update(GameTime time)
        { }
    }

    public class LineRenderItem : RenderItem
    {
        public Vector3 Colour { get; set; } = new Vector3(0, 0, 0);

        public LineRenderItem(LineModel model, Effect shader) :base(model, shader){ }

        public override void ApplyCustomShaderParams()
        {
            _shader.Parameters["Color"].SetValue(Colour);
        }
    }


    public class MeshRenderItem : RenderItem
    {
        public AttachmentResolver AttachmentResolver { get; set; }
        public MeshRenderItem(MeshModel model, Effect shader) : base(model, shader) 
        {
        
        }

        public override void ApplyCustomShaderParams()
        {
            if (AttachmentResolver != null)
                _shader.Parameters["World"].SetValue(AttachmentResolver.GetWorldMatrix());
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
        }
    }

    public class AttachmentResolver
    {
        int _boneIndex = -1;
        SkeletonModel _skeleton;
        public AttachmentResolver(string name, SkeletonModel skeleton)
        {
            _skeleton = skeleton;
            _boneIndex = _skeleton.GetBoneIndex(name);
        }

        public Matrix GetWorldMatrix()
        {
            return /*Matrix.CreateTranslation(0.5f, 0, 0) * */_skeleton.GetAnimatedBone(_boneIndex);
        }
    }
}
