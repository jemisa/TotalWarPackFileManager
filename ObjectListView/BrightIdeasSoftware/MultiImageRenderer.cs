namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;

    public class MultiImageRenderer : BaseRenderer
    {
        private object imageSelector;
        private int maximumValue;
        private int maxNumberImages;
        private int minimumValue;

        public MultiImageRenderer()
        {
            this.maxNumberImages = 10;
            this.minimumValue = 0;
            this.maximumValue = 100;
        }

        public MultiImageRenderer(object imageSelector, int maxImages, int minValue, int maxValue) : this()
        {
            this.ImageSelector = imageSelector;
            this.MaxNumberImages = maxImages;
            this.MinimumValue = minValue;
            this.MaximumValue = maxValue;
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            Image image = this.GetImage(this.ImageSelector);
            if (image != null)
            {
                IConvertible aspect = base.Aspect as IConvertible;
                if (aspect != null)
                {
                    int maxNumberImages;
                    double num = aspect.ToDouble(NumberFormatInfo.InvariantInfo);
                    if (num <= this.MinimumValue)
                    {
                        maxNumberImages = 0;
                    }
                    else if (num < this.MaximumValue)
                    {
                        maxNumberImages = 1 + ((int) ((this.MaxNumberImages * (num - this.MinimumValue)) / ((double) this.MaximumValue)));
                    }
                    else
                    {
                        maxNumberImages = this.MaxNumberImages;
                    }
                    int width = image.Width;
                    int height = image.Height;
                    if (r.Height < image.Height)
                    {
                        width = (int) ((image.Width * r.Height) / ((float) image.Height));
                        height = r.Height;
                    }
                    Rectangle inner = r;
                    inner.Width = (this.MaxNumberImages * (width + base.Spacing)) - base.Spacing;
                    inner.Height = height;
                    inner = this.AlignRectangle(r, inner);
                    for (int i = 0; i < maxNumberImages; i++)
                    {
                        g.DrawImage(image, inner.X, inner.Y, width, height);
                        inner.X += width + base.Spacing;
                    }
                }
            }
        }

        [Category("Behavior"), DefaultValue(-1), Description("The index of the image that should be drawn")]
        public int ImageIndex
        {
            get
            {
                if (this.imageSelector is int)
                {
                    return (int) this.imageSelector;
                }
                return -1;
            }
            set
            {
                this.imageSelector = value;
            }
        }

        [Description("The index of the image that should be drawn"), Category("Behavior"), DefaultValue((string) null)]
        public string ImageName
        {
            get
            {
                return (this.imageSelector as string);
            }
            set
            {
                this.imageSelector = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object ImageSelector
        {
            get
            {
                return this.imageSelector;
            }
            set
            {
                this.imageSelector = value;
            }
        }

        [Description("Values greater than or equal to this will have MaxNumberImages images drawn"), Category("Behavior"), DefaultValue(100)]
        public int MaximumValue
        {
            get
            {
                return this.maximumValue;
            }
            set
            {
                this.maximumValue = value;
            }
        }

        [DefaultValue(10), Category("Behavior"), Description("The maximum number of images that this renderer should draw")]
        public int MaxNumberImages
        {
            get
            {
                return this.maxNumberImages;
            }
            set
            {
                this.maxNumberImages = value;
            }
        }

        [Description("Values less than or equal to this will have 0 images drawn"), Category("Behavior"), DefaultValue(0)]
        public int MinimumValue
        {
            get
            {
                return this.minimumValue;
            }
            set
            {
                this.minimumValue = value;
            }
        }
    }
}

