using Common;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PackFileManager.PackedTreeView
{
    public class TreeViewIconCreator
    {
        ILogger _logger = Logging.Create<TreeViewIconCreator>();
        int _imagePixelSize = 16;

        public Image Folder { get; set; }
        public Image DefaultFile { get; set; }
        public Image TextFile { get; set; }
        public Image DatabaseFile { get; set; }

        public void Load()
        {
            try
            {
                Folder = LoadAndResize(@"Resources\TreeViewIcons\icons8-folder-48.png");
                DefaultFile = LoadAndResize(@"Resources\TreeViewIcons\icons8-file-48.png");
                TextFile = LoadAndResize(@"Resources\TreeViewIcons\icons8-txt-48.png");
                DatabaseFile = LoadAndResize(@"Resources\TreeViewIcons\icons8-database-48.png");
            }
            catch (Exception e)
            {
                _logger.Fatal(e.Message);
            }
        }

        Image LoadAndResize(string path)
        {
            var img = Bitmap.FromFile(path);
            var resized = ResizeImage(img, _imagePixelSize, _imagePixelSize);
            return resized;
        }

        Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
