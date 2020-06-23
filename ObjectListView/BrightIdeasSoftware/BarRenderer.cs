namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Windows.Forms;

    public class BarRenderer : BaseRenderer
    {
        private System.Drawing.Brush backgroundBrush;
        private Color backgroundColor;
        private System.Drawing.Brush brush;
        private Color endColor;
        private Color fillColor;
        private Color frameColor;
        private float frameWidth;
        private int maximumHeight;
        private double maximumValue;
        private int maximumWidth;
        private double minimumValue;
        private int padding;
        private System.Drawing.Pen pen;
        private Color startColor;
        private bool useStandardBar;

        public BarRenderer()
        {
            this.useStandardBar = true;
            this.padding = 2;
            this.backgroundColor = Color.AliceBlue;
            this.frameColor = Color.Black;
            this.frameWidth = 1f;
            this.fillColor = Color.BlueViolet;
            this.startColor = Color.CornflowerBlue;
            this.endColor = Color.DarkBlue;
            this.maximumWidth = 100;
            this.maximumHeight = 0x10;
            this.minimumValue = 0.0;
            this.maximumValue = 100.0;
        }

        public BarRenderer(System.Drawing.Pen pen, System.Drawing.Brush brush) : this()
        {
            this.Pen = pen;
            this.Brush = brush;
            this.UseStandardBar = false;
        }

        public BarRenderer(int minimum, int maximum) : this()
        {
            this.MinimumValue = minimum;
            this.MaximumValue = maximum;
        }

        public BarRenderer(System.Drawing.Pen pen, Color start, Color end) : this()
        {
            this.Pen = pen;
            this.SetGradient(start, end);
        }

        public BarRenderer(int minimum, int maximum, System.Drawing.Pen pen, System.Drawing.Brush brush) : this(minimum, maximum)
        {
            this.Pen = pen;
            this.Brush = brush;
            this.UseStandardBar = false;
        }

        public BarRenderer(int minimum, int maximum, System.Drawing.Pen pen, Color start, Color end) : this(minimum, maximum)
        {
            this.Pen = pen;
            this.SetGradient(start, end);
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            Rectangle inner = Rectangle.Inflate(r, -this.Padding, -this.Padding);
            inner.Width = Math.Min(inner.Width, this.MaximumWidth);
            inner.Height = Math.Min(inner.Height, this.MaximumHeight);
            inner = this.AlignRectangle(r, inner);
            IConvertible aspect = base.Aspect as IConvertible;
            if (aspect != null)
            {
                double num = aspect.ToDouble(NumberFormatInfo.InvariantInfo);
                Rectangle bounds = Rectangle.Inflate(inner, -1, -1);
                if (num <= this.MinimumValue)
                {
                    bounds.Width = 0;
                }
                else if (num < this.MaximumValue)
                {
                    bounds.Width = (int) ((bounds.Width * (num - this.MinimumValue)) / this.MaximumValue);
                }
                if (!((!this.UseStandardBar || !ProgressBarRenderer.IsSupported) || base.IsPrinting))
                {
                    ProgressBarRenderer.DrawHorizontalBar(g, inner);
                    ProgressBarRenderer.DrawHorizontalChunks(g, bounds);
                }
                else
                {
                    g.FillRectangle(this.BackgroundBrush, inner);
                    if (bounds.Width > 0)
                    {
                        bounds.Width++;
                        bounds.Height++;
                        if (this.GradientStartColor == Color.Empty)
                        {
                            g.FillRectangle(this.Brush, bounds);
                        }
                        else
                        {
                            using (LinearGradientBrush brush = new LinearGradientBrush(inner, this.GradientStartColor, this.GradientEndColor, LinearGradientMode.Horizontal))
                            {
                                g.FillRectangle(brush, bounds);
                            }
                        }
                    }
                    g.DrawRectangle(this.Pen, inner);
                }
            }
        }

        public void SetGradient(Color start, Color end)
        {
            this.GradientStartColor = start;
            this.GradientEndColor = end;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Brush BackgroundBrush
        {
            get
            {
                if (!((this.backgroundBrush != null) || this.BackgroundColor.IsEmpty))
                {
                    return new SolidBrush(this.BackgroundColor);
                }
                return this.backgroundBrush;
            }
            set
            {
                this.backgroundBrush = value;
            }
        }

        [DefaultValue(typeof(Color), "AliceBlue"), Description("The color of the interior of the bar"), Category("Appearance - ObjectListView")]
        public Color BackgroundColor
        {
            get
            {
                return this.backgroundColor;
            }
            set
            {
                this.backgroundColor = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Brush Brush
        {
            get
            {
                if (!((this.brush != null) || this.FillColor.IsEmpty))
                {
                    return new SolidBrush(this.FillColor);
                }
                return this.brush;
            }
            set
            {
                this.brush = value;
            }
        }

        [Category("Appearance - ObjectListView"), DefaultValue(typeof(Color), "BlueViolet"), Description("What color should the 'filled in' part of the progress bar be")]
        public Color FillColor
        {
            get
            {
                return this.fillColor;
            }
            set
            {
                this.fillColor = value;
            }
        }

        [Description("What color should the frame of the progress bar be"), Category("Appearance - ObjectListView"), DefaultValue(typeof(Color), "Black")]
        public Color FrameColor
        {
            get
            {
                return this.frameColor;
            }
            set
            {
                this.frameColor = value;
            }
        }

        [Category("Appearance - ObjectListView"), Description("How many pixels wide should the frame of the progress bar be"), DefaultValue((float) 1f)]
        public float FrameWidth
        {
            get
            {
                return this.frameWidth;
            }
            set
            {
                this.frameWidth = value;
            }
        }

        [DefaultValue(typeof(Color), "DarkBlue"), Category("Appearance - ObjectListView"), Description("Use a gradient to fill the progress bar ending with this color")]
        public Color GradientEndColor
        {
            get
            {
                return this.endColor;
            }
            set
            {
                this.endColor = value;
            }
        }

        [DefaultValue(typeof(Color), "CornflowerBlue"), Category("Appearance - ObjectListView"), Description("Use a gradient to fill the progress bar starting with this color")]
        public Color GradientStartColor
        {
            get
            {
                return this.startColor;
            }
            set
            {
                this.startColor = value;
            }
        }

        [Category("Behavior"), DefaultValue(0x10), Description("The progress bar will never be taller than this")]
        public int MaximumHeight
        {
            get
            {
                return this.maximumHeight;
            }
            set
            {
                this.maximumHeight = value;
            }
        }

        [Category("Behavior"), DefaultValue((double) 100.0), Description("The maximum value for the range. Values greater than this will give a full bar")]
        public double MaximumValue
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

        [DefaultValue(100), Category("Behavior"), Description("The progress bar will never be wider than this")]
        public int MaximumWidth
        {
            get
            {
                return this.maximumWidth;
            }
            set
            {
                this.maximumWidth = value;
            }
        }

        [DefaultValue((double) 0.0), Category("Behavior"), Description("The minimum data value expected. Values less than this will given an empty bar")]
        public double MinimumValue
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

        [Description("How many pixels in from our cell border will this bar be drawn"), Category("Appearance - ObjectListView"), DefaultValue(2)]
        public int Padding
        {
            get
            {
                return this.padding;
            }
            set
            {
                this.padding = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Pen Pen
        {
            get
            {
                if (!((this.pen != null) || this.FrameColor.IsEmpty))
                {
                    return new System.Drawing.Pen(this.FrameColor, this.FrameWidth);
                }
                return this.pen;
            }
            set
            {
                this.pen = value;
            }
        }

        [DefaultValue(true), Description("Should this bar be drawn in the system style?"), Category("Appearance - ObjectListView")]
        public bool UseStandardBar
        {
            get
            {
                return this.useStandardBar;
            }
            set
            {
                this.useStandardBar = value;
            }
        }
    }
}

