namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;

    public class ImageAdornment : GraphicAdornment
    {
        private System.Drawing.Image image;
        private bool shrinkToWidth;

        public void DrawImage(Graphics g, Rectangle r)
        {
            if (this.ShrinkToWidth)
            {
                this.DrawScaledImage(g, r, this.Image, base.Transparency);
            }
            else
            {
                this.DrawImage(g, r, this.Image, base.Transparency);
            }
        }

        public void DrawImage(Graphics g, Rectangle r, System.Drawing.Image image, int transparency)
        {
            if (image != null)
            {
                this.DrawImage(g, r, image, image.Size, transparency);
            }
        }

        public void DrawImage(Graphics g, Rectangle r, System.Drawing.Image image, Size sz, int transparency)
        {
            if (image != null)
            {
                Rectangle rectangle = base.CreateAlignedRectangle(r, sz);
                try
                {
                    base.ApplyRotation(g, rectangle);
                    this.DrawTransparentBitmap(g, rectangle, image, transparency);
                }
                finally
                {
                    base.UnapplyRotation(g);
                }
            }
        }

        public void DrawScaledImage(Graphics g, Rectangle r, System.Drawing.Image image, int transparency)
        {
            if (image != null)
            {
                Size sz = image.Size;
                if (image.Width > r.Width)
                {
                    float num = ((float) r.Width) / ((float) image.Width);
                    sz.Height = (int) (image.Height * num);
                    sz.Width = r.Width - 1;
                }
                this.DrawImage(g, r, image, sz, transparency);
            }
        }

        protected void DrawTransparentBitmap(Graphics g, Rectangle r, System.Drawing.Image image, int transparency)
        {
            ImageAttributes imageAttr = null;
            if (transparency != 0xff)
            {
                imageAttr = new ImageAttributes();
                float num = ((float) transparency) / 255f;
                float[][] numArray2 = new float[5][];
                float[] numArray3 = new float[5];
                numArray3[0] = 1f;
                numArray2[0] = numArray3;
                numArray3 = new float[5];
                numArray3[1] = 1f;
                numArray2[1] = numArray3;
                numArray3 = new float[5];
                numArray3[2] = 1f;
                numArray2[2] = numArray3;
                numArray3 = new float[5];
                numArray3[3] = num;
                numArray2[3] = numArray3;
                numArray3 = new float[5];
                numArray3[4] = 1f;
                numArray2[4] = numArray3;
                float[][] newColorMatrix = numArray2;
                imageAttr.SetColorMatrix(new ColorMatrix(newColorMatrix));
            }
            g.DrawImage(image, r, 0, 0, image.Size.Width, image.Size.Height, GraphicsUnit.Pixel, imageAttr);
        }

        [NotifyParentProperty(true), Category("Appearance - ObjectListView"), Description("The image that will be drawn"), DefaultValue((string) null)]
        public System.Drawing.Image Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
            }
        }

        [DefaultValue(false), Description("Will the image be shrunk to fit within its width?"), Category("Appearance - ObjectListView")]
        public bool ShrinkToWidth
        {
            get
            {
                return this.shrinkToWidth;
            }
            set
            {
                this.shrinkToWidth = value;
            }
        }
    }
}

