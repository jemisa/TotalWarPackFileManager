using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Content.Pipeline.Builder;
using MonoGame.Framework.WpfInterop;
using MonoGame.Framework.WpfInterop.Input;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Navigation;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.NHew;


//assimpnet

namespace WpfTest.Scenes
{

    class FastBitmap : IDisposable
    {
        Bitmap _bitmap;
        byte[] _rgbValues;
        IntPtr _intPtr;
        System.Drawing.Imaging.BitmapData _bmpData;
        int _pixelLength;

        public FastBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;


            var rect = new System.Drawing.Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
            _bmpData =
                 _bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                 _bitmap.PixelFormat);
            _intPtr = _bmpData.Scan0;
            _pixelLength = Image.GetPixelFormatSize(_bitmap.PixelFormat) / 8;
            int bytes = Math.Abs(_bmpData.Stride) * _bitmap.Height;
            _rgbValues = new byte[bytes];
             System.Runtime.InteropServices.Marshal.Copy(_intPtr, _rgbValues, 0, bytes);
        }

        public System.Drawing.Color GetPixel(int x, int y)
        {
            var index = (y * _bitmap.Width * _pixelLength) + x * _pixelLength;
            byte r = _rgbValues[index+0];
            byte g = _rgbValues[index+1];
            byte b = _rgbValues[index+2];
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public void SetPixel(int x, int y, System.Drawing.Color colour)
        {
            var index = (y * _bitmap.Width * _pixelLength) + x * _pixelLength;
            _rgbValues[index + 0] = colour.R;
            _rgbValues[index + 1] = colour.G;
            _rgbValues[index + 2] = colour.B;
        }

        public Bitmap ToBitmap()
        {
            int bytes = Math.Abs(_bmpData.Stride) * _bitmap.Height;
            System.Runtime.InteropServices.Marshal.Copy(_rgbValues, 0, _intPtr, bytes);

            _bitmap.UnlockBits(_bmpData);
            return _bitmap;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    class CubemapGeneratorHelper
    {
        GraphicsDevice _device;
        string[] _expectedImageNames = new string[] { "posx", "negx", "posy", "negy", "posz", "negz" };
        string _textureDir;
        int _size;

        string InputDirectory { get { return _textureDir + @"\raw\"; } }
        string OutputDirectory { get { return _textureDir + @"\Processed\"; } }

        public void Create(string textureDirectory, GraphicsDevice device)
        {
            _device = device;
            _textureDir = textureDirectory;

            var files = Directory.GetFiles(InputDirectory);
            if (files.Count() != 6)
                throw new Exception("Directory does not contain 6 images");

            int[] imageSize = new int[6];
            for (int i = 0; i < _expectedImageNames.Count(); i++)
            {
                var imageName = InputDirectory + @"\" + _expectedImageNames[i] + ".jpg";
                using (var steam = File.OpenRead(imageName))
                {
                    using (Texture2D temp = Texture2D.FromStream(_device, steam))
                    {
                        imageSize[i] = temp.Width;
                        if (temp.Width != temp.Height)
                            throw new Exception("Image must be power of 2");
                    }
                    
                }
            }

            _size = imageSize.First();
            var imagesWihWrongSize = imageSize.Where(x => x != _size).Count();
            if (imagesWihWrongSize != 0)
                throw new Exception("One of the images has a different size then the first one");
        }

        public TextureCube SimpleCubeMap()
        {
            var _imageList = new Texture2D[6];
            for (int i = 0; i < _expectedImageNames.Count(); i++)
            {
                //var imageName = OutputDirectory + @"blur\"  + _expectedImageNames[i] + "_1483.jpg";
                var imageName = @"C:\temp\Result\" + _expectedImageNames[i] + ".png";
                using (var steam = File.OpenRead(imageName))
                {
                    _imageList[i] = Texture2D.FromStream(_device, steam);
                }
            }

            var textureCube = new TextureCube(_device, _imageList.First().Width, false, SurfaceFormat.Color);
            for (int i = 0; i < 6; i++)
            {
                var colour = new Microsoft.Xna.Framework.Color[_imageList[i].Width * _imageList[i].Width];
                _imageList[i].GetData(colour);
                textureCube.SetData((CubeMapFace)i, colour);
            }
            return textureCube;
        }

        public TextureCube CreateCubemapTexture(string outputSubFolder, int blurSize = 0)
        {
            var mipMaps = GetMipmapResolutions(_size, 8);

            var outDir = OutputDirectory;
            if (!string.IsNullOrWhiteSpace(outputSubFolder))
            {
                outDir = OutputDirectory + outputSubFolder + @"\";
                if (Directory.Exists(outDir) == false)
                    Directory.CreateDirectory(outDir);

            }

            if(Directory.GetFiles(outDir).Count() == 0)
                Compute(blurSize, mipMaps, outDir);


            var textureCube = new TextureCube(_device, _size, true, SurfaceFormat.Color);
            for (int mip = 0; mip < mipMaps.Count(); mip++)
            {
                for (int i = 0; i < _expectedImageNames.Count(); i++)
                {
                    var imageName = outDir + _expectedImageNames[i] + "_" + mip + ".jpg";
                    using (var steam = File.OpenRead(imageName))
                    {
                        using (var image = Texture2D.FromStream(_device, steam))
                        {
                            var dataSize = image.Width * image.Width;
                            var data = new Microsoft.Xna.Framework.Color[image.Width * image.Width];
                            image.GetData(data);
                            textureCube.SetData((CubeMapFace)i, mip, null, data, 0, dataSize);
                        }
                    }
                    
                }
            }


            return textureCube;
        }

        private void Compute(int blurSize, int[] mipMaps, string outDir)
        {
            var blur = new GaussianLayer(blurSize);
            ImageFactory[] lastProcessing = new ImageFactory[6];
            for (int i = 0; i < _expectedImageNames.Count(); i++)
            {
                lastProcessing[i] = new ImageFactory();
                lastProcessing[i].Load(InputDirectory + _expectedImageNames[i] + ".jpg");
                lastProcessing[i].Resize(new System.Drawing.Size(512,512));
                if (blurSize != 0)
                    lastProcessing[i].GaussianBlur(blur);
                lastProcessing[i].Save(outDir + _expectedImageNames[i] + "_" + 0 + ".jpg");
            }

            for (int mip = 1; mip < mipMaps.Count(); mip++)
            {
                for (int i = 0; i < _expectedImageNames.Count(); i++)
                {
                    lastProcessing[i].Resize(new System.Drawing.Size(mipMaps[mip], mipMaps[mip]));
                    if (blurSize != 0)
                        lastProcessing[i].GaussianBlur(blur);
                    lastProcessing[i].Save(outDir + _expectedImageNames[i] + "_" + mip + ".jpg");
                }
            }
        }

        public void SuperIm()
        {
            Bitmap[] images = new Bitmap[6];
            for (int i = 0; i < _expectedImageNames.Count(); i++)
            {
                var imageName = InputDirectory + _expectedImageNames[i] + ".jpg";
                images[i] = new Bitmap(imageName);
                images[i] = new Bitmap(images[i], new System.Drawing.Size(512,512));
            }

            Bitmap[] superImages = new Bitmap[6];
            for (int i = 0; i < 6; i++)
            {
                superImages[i] = CreateSuperImage(images, i);
                var path = $@"C:\temp\superImage_{i}.png";
                superImages[i].Save(path);
            }

            Bitmap[] filteredImages = new Bitmap[6];
            for (int i = 0; i < 6; i++)
            {
                StackBlur.StackBlur.Process(superImages[i], 128);
                //filteredImages[i] = BoxFilterSuperImage(superImages[i], 250);
                var path = $@"C:\temp\FilterImage_{i}.png";
                superImages[i].Save(path);
            }


            // Extract image
            for (int i = 0; i < 6; i++)
            {
                var finalBitmap = new Bitmap(512, 512);
                using (var graphics = Graphics.FromImage(finalBitmap))
                {
                    var destination = new RectangleF(0,0, finalBitmap.Width, finalBitmap.Height);

                    var imageSize = superImages[i].Width / 3;
                    var sourceRect = new RectangleF(imageSize, imageSize, imageSize, imageSize);

                    graphics.DrawImage(superImages[i], destination, sourceRect, GraphicsUnit.Pixel);
                }
                var path = $@"C:\temp\final_{i}.png";
                finalBitmap.Save(path);

                var path2 = $@"C:\temp\result\{_expectedImageNames[i]}.png";
                finalBitmap.Save(path2);
            }
        }

        Bitmap BoxFilterSuperImage(Bitmap image, int filterSize)
        {
            var imageSize = image.Width / 3;
            FastBitmap bitmapCopy = new FastBitmap(image.Clone() as Bitmap);




            for (int y = 0; y < imageSize; y++)
            {
                int realY = y + imageSize;
                for (int x = 0; x < imageSize; x++)
                {
                    int realX = x + imageSize;
                    var color = BlurY(bitmapCopy, realX, realY, filterSize);
                    bitmapCopy.SetPixel(realX, realY, color);
                }
            }

            for (int y = 0; y < imageSize; y++)
            {
                int realY = y + imageSize;
                for (int x = 0; x < imageSize; x++)
                {
                    int realX = x + imageSize;
                    var color = BlurX(bitmapCopy, realX, realY, filterSize);
                    bitmapCopy.SetPixel(realX, realY, color);
                }
            }


            return bitmapCopy.ToBitmap();
        }

        System.Drawing.Color BlurX(FastBitmap image, int x, int y, int filterSize)
        {
            int halfFilter = (filterSize - 1) / 2;
            int pixelR = 0;
            int pixelG = 0;
            int pixelB = 0;

            for (int i = 0; i < filterSize; i++)
            {
                var currentPixel = image.GetPixel(x + i - halfFilter, y);
                pixelR += currentPixel.R;
                pixelG += currentPixel.G;
                pixelB += currentPixel.B;
            }

            var resultR = pixelR / filterSize;
            var resultG = pixelG / filterSize;
            var resultB = pixelB / filterSize;
            return System.Drawing.Color.FromArgb(resultR, resultG, resultB);
        }


        System.Drawing.Color BlurY(FastBitmap image, int x, int y, int filterSize)
        {
            int halfFilter = (filterSize - 1) / 2;
            int pixelR = 0;
            int pixelG = 0;
            int pixelB = 0;

            for (int i = 0; i < filterSize; i++)
            {
                var currentPixel = image.GetPixel(x , y+ i - halfFilter);
                pixelR += currentPixel.R;
                pixelG += currentPixel.G;
                pixelB += currentPixel.B;
            }

            var resultR = pixelR / filterSize;
            var resultG = pixelG / filterSize;
            var resultB = pixelB / filterSize;
            return System.Drawing.Color.FromArgb(resultR, resultG, resultB);
        }

        Bitmap CreateSuperImage(Bitmap[] sources, int currentIndex)
        {
            var size = sources[currentIndex].Width;
            var superSize = size * 3;
            Bitmap image = new Bitmap(superSize, superSize);

            if (currentIndex == 0)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[0], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    var belovCopy = sources[3].Clone() as Bitmap;
                    belovCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(belovCopy, new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Above
                    var aboveCopy = sources[2].Clone() as Bitmap;
                    aboveCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(aboveCopy, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Right
                    var rightCopy = sources[4].Clone() as Bitmap;
                    //rightCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(rightCopy, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Left
                    var leftCopy = sources[5].Clone() as Bitmap;
                    //leftCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(leftCopy, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);
                }
                return image;
            }


            if (currentIndex == 1)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[1], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    var belovCopy = sources[3].Clone() as Bitmap;
                    belovCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(belovCopy, new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Above
                    var aboveCopy = sources[2].Clone() as Bitmap;
                    aboveCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(aboveCopy, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Right
                    var rightCopy = sources[5].Clone() as Bitmap;
                    //rightCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(rightCopy, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Left
                    var leftCopy = sources[4].Clone() as Bitmap;
                    //leftCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(leftCopy, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);
                }
                return image;
            }


            if (currentIndex == 2)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[2], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    graphics.DrawImage(sources[4], new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);     // Below

                    // Above
                    var copy5 = sources[5].Clone() as Bitmap;
                    copy5.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    graphics.DrawImage(copy5, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);      // Above

                    // Right
                    var copy1 = sources[1].Clone() as Bitmap;
                    copy1.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(copy1, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);      // Above

                    // Left
                    var copy0 = sources[0].Clone() as Bitmap;
                    copy0.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(copy0, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);      // Above
                }
                return image;
            }


            if (currentIndex == 3)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[3], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    var belovCopy = sources[5].Clone() as Bitmap;
                    belovCopy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    graphics.DrawImage(belovCopy, new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Above
                    var aboveCopy = sources[4].Clone() as Bitmap;
                    //aboveCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(aboveCopy, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Right
                    var rightCopy = sources[1].Clone() as Bitmap;
                    rightCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(rightCopy, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Left
                    var leftCopy = sources[0].Clone() as Bitmap;
                    leftCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(leftCopy, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);
                }
                return image;
            }

            if (currentIndex == 4)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[4], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    var belovCopy = sources[3].Clone() as Bitmap;
                    // belovCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(belovCopy, new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Above
                    var aboveCopy = sources[2].Clone() as Bitmap;
                    //aboveCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(aboveCopy, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Right
                    var rightCopy = sources[1].Clone() as Bitmap;
                    //rightCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(rightCopy, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Left
                    var leftCopy = sources[0].Clone() as Bitmap;
                    //leftCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(leftCopy, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);
                }
                return image;
            }

            if (currentIndex == 5)
            {
                using (var graphics = Graphics.FromImage(image))
                {
                    graphics.DrawImage(sources[5], new RectangleF(size, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Belov
                    var belovCopy = sources[3].Clone() as Bitmap;
                    belovCopy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    graphics.DrawImage(belovCopy, new RectangleF(size, size * 2, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Above
                    var aboveCopy = sources[2].Clone() as Bitmap;
                    aboveCopy.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    graphics.DrawImage(aboveCopy, new RectangleF(size, 0, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Right
                    var rightCopy = sources[0].Clone() as Bitmap;
                    //rightCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    graphics.DrawImage(rightCopy, new RectangleF(0, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);

                    // Left
                    var leftCopy = sources[1].Clone() as Bitmap;
                    //leftCopy.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    graphics.DrawImage(leftCopy, new RectangleF(size * 2, size, size, size), new RectangleF(0, 0, size, size), GraphicsUnit.Pixel);
                }
                return image;
            }

            return null;
        }

        int[] GetMipmapResolutions(int initial, int minResolution)
        {
            List<int> output = new List<int>();
            output.Add(initial);
            while (output.Last() != minResolution)
            {
                var value = output.Last() / 2;
                output.Add(value);
            }

            return output.ToArray();
        }


    
    
    }

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
            CubemapGeneratorHelper cubemapGeneratorHelper = new CubemapGeneratorHelper();
            cubemapGeneratorHelper.Create(@"C:\Users\ole_k\source\repos\TotalWarPackFileManager\Viewer\Content\Textures\CubeMaps\GamleStan", GraphicsDevice);
            //textureCube = cubemapGeneratorHelper.CreateCubemapTexture("Blur", 28);
            //cubemapGeneratorHelper.CreateCubemapTexture("Unprocessed", 0);
            cubemapGeneratorHelper.SuperIm();
            textureCube = cubemapGeneratorHelper.SimpleCubeMap();
          

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

        private void DrawModelWithEffect(Effect effect, Vector3 cameraPos, Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(world * mesh.ParentBone.Transform);
                    effect.Parameters["View"].SetValue(view);
                    effect.Parameters["Projection"].SetValue(projection);
                    effect.Parameters["SkyboxTexture"].SetValue(textureCube);
                    effect.Parameters["CameraPosition"].SetValue(cameraPos);
                    effect.Parameters["WorldInverseTranspose"].SetValue(
                                            Matrix.Transpose(Matrix.Invert(world * mesh.ParentBone.Transform)));
                }
                mesh.Draw();
            }
        }

        protected override void Draw(GameTime time)
        {
            RefreshProjection();

            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);
     
     
            _basicEffect.View = _camera.ViewMatrix;



            /**/
            float distance = 20;
            var angle = 0.002f;
            var cameraPosition = distance * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
            var view = Matrix.CreateLookAt(cameraPosition, new Vector3(0, 0, 0), Vector3.UnitY);
            RasterizerState originalRasterizerState = GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            //skybox.Draw(_camera.ViewMatrix, _projectionMatrix, _camera.Position);

            GraphicsDevice.RasterizerState = originalRasterizerState;

            DrawModelWithEffect(_reflectShader, _camera.Position, _cube, Matrix.Identity, _camera.ViewMatrix, _projectionMatrix);

            /*GraphicsDevice.RasterizerState.CullMode = CullMode.CullClockwiseFace;
            skybox.Draw(_camera.ViewMatrix, _projectionMatrix, _camera.Position);
            GraphicsDevice.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;*/

            return;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            _shader.CurrentTechnique = _shader.Techniques["Diffuse"];
      


            foreach (var pass in _shader.CurrentTechnique.Passes)
            {

                pass.Apply();
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
                    _shader.Parameters["World"].SetValue(worldMatrix);
                    _shader.Parameters["View"].SetValue(_camera.ViewMatrix);
                    _shader.Parameters["Projection"].SetValue(_projectionMatrix);
                    _shader.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));

                    /*
                     float4x4 World;
float4x4 View;
float4x4 Projection;
                     */

                    item.Render(GraphicsDevice, _shader);
                }
            }

            //_cube0.Render(GraphicsDevice, _shader);
        

            base.Draw(time);
        }
    }
}
