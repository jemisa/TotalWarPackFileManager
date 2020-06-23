namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    public class TextAdornment : GraphicAdornment
    {
        private Color backColor = Color.Empty;
        private Color borderColor = Color.Empty;
        private float borderWidth;
        private float cornerRounding = 16f;
        private System.Drawing.Font font;
        private int maximumTextWidth;
        private System.Drawing.StringFormat stringFormat;
        private string text;
        private Color textColor = Color.DarkBlue;
        private int WorkingTransparency;
        private bool wrap = true;

        protected Rectangle CalculateTextBounds(Graphics g, Rectangle r, string text)
        {
            int width = (this.MaximumTextWidth <= 0) ? r.Width : this.MaximumTextWidth;
            SizeF ef = g.MeasureString(text, this.FontOrDefault, width, this.StringFormat);
            Size sz = new Size(1 + ((int) ef.Width), 1 + ((int) ef.Height));
            return base.CreateAlignedRectangle(r, sz);
        }

        protected void DrawBorderedText(Graphics g, Rectangle textRect, string text, int transparency)
        {
            Rectangle rect = textRect;
            rect.Inflate(((int) this.BorderWidth) / 2, ((int) this.BorderWidth) / 2);
            rect.Y--;
            try
            {
                base.ApplyRotation(g, textRect);
                using (GraphicsPath path = this.GetRoundedRect(rect, this.CornerRounding))
                {
                    this.WorkingTransparency = transparency;
                    if (this.HasBackground)
                    {
                        g.FillPath(this.BackgroundBrush, path);
                    }
                    g.DrawString(text, this.FontOrDefault, this.TextBrush, textRect, this.StringFormat);
                    if (this.HasBorder)
                    {
                        g.DrawPath(this.BorderPen, path);
                    }
                }
            }
            finally
            {
                base.UnapplyRotation(g);
            }
        }

        public void DrawText(Graphics g, Rectangle r)
        {
            this.DrawText(g, r, this.Text, base.Transparency);
        }

        public void DrawText(Graphics g, Rectangle r, string text, int transparency)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Rectangle textRect = this.CalculateTextBounds(g, r, text);
                this.DrawBorderedText(g, textRect, text, transparency);
            }
        }

        protected GraphicsPath GetRoundedRect(Rectangle rect, float diameter)
        {
            GraphicsPath path = new GraphicsPath();
            if (diameter > 0f)
            {
                RectangleF ef = new RectangleF((float) rect.X, (float) rect.Y, diameter, diameter);
                path.AddArc(ef, 180f, 90f);
                ef.X = rect.Right - diameter;
                path.AddArc(ef, 270f, 90f);
                ef.Y = rect.Bottom - diameter;
                path.AddArc(ef, 0f, 90f);
                ef.X = rect.Left;
                path.AddArc(ef, 90f, 90f);
                path.CloseFigure();
                return path;
            }
            path.AddRectangle(rect);
            return path;
        }

        [DefaultValue(typeof(Color), ""), Category("Appearance - ObjectListView"), Description("The background color of the text")]
        public Color BackColor
        {
            get
            {
                return this.backColor;
            }
            set
            {
                this.backColor = value;
            }
        }

        [Browsable(false)]
        public Brush BackgroundBrush
        {
            get
            {
                return new SolidBrush(Color.FromArgb(this.WorkingTransparency, this.BackColor));
            }
        }

        [Category("Appearance - ObjectListView"), Description("The color of the border around the text"), DefaultValue(typeof(Color), "")]
        public Color BorderColor
        {
            get
            {
                return this.borderColor;
            }
            set
            {
                this.borderColor = value;
            }
        }

        [Browsable(false)]
        public Pen BorderPen
        {
            get
            {
                return new Pen(Color.FromArgb(this.WorkingTransparency, this.BorderColor), this.BorderWidth);
            }
        }

        [Category("Appearance - ObjectListView"), Description("The width of the border around the text"), DefaultValue((float) 0f)]
        public float BorderWidth
        {
            get
            {
                return this.borderWidth;
            }
            set
            {
                this.borderWidth = value;
            }
        }

        [Category("Appearance - ObjectListView"), Description("How rounded should the corners of the border be? 0 means no rounding."), DefaultValue((float) 16f), NotifyParentProperty(true)]
        public float CornerRounding
        {
            get
            {
                return this.cornerRounding;
            }
            set
            {
                this.cornerRounding = value;
            }
        }

        [Description("The font that will be used to draw the text"), NotifyParentProperty(true), DefaultValue((string) null), Category("Appearance - ObjectListView")]
        public System.Drawing.Font Font
        {
            get
            {
                return this.font;
            }
            set
            {
                this.font = value;
            }
        }

        [Browsable(false)]
        public System.Drawing.Font FontOrDefault
        {
            get
            {
                return (this.Font ?? new System.Drawing.Font("Tahoma", 16f));
            }
        }

        [Browsable(false)]
        public bool HasBackground
        {
            get
            {
                return (this.BackColor != Color.Empty);
            }
        }

        [Browsable(false)]
        public bool HasBorder
        {
            get
            {
                return ((this.BorderColor != Color.Empty) && (this.BorderWidth > 0f));
            }
        }

        [Category("Appearance - ObjectListView"), Description("The maximum width the text (0 means no maximum). Text longer than this will wrap"), DefaultValue(0)]
        public int MaximumTextWidth
        {
            get
            {
                return this.maximumTextWidth;
            }
            set
            {
                this.maximumTextWidth = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.StringFormat StringFormat
        {
            get
            {
                if (this.stringFormat == null)
                {
                    this.stringFormat = new System.Drawing.StringFormat();
                    this.stringFormat.Alignment = StringAlignment.Center;
                    this.stringFormat.LineAlignment = StringAlignment.Center;
                    this.stringFormat.Trimming = StringTrimming.EllipsisCharacter;
                    if (!this.Wrap)
                    {
                        this.stringFormat.FormatFlags = StringFormatFlags.NoWrap;
                    }
                }
                return this.stringFormat;
            }
            set
            {
                this.stringFormat = value;
            }
        }

        [Description("The text that will be drawn over the top of the ListView"), Category("Appearance - ObjectListView"), DefaultValue((string) null), NotifyParentProperty(true), Localizable(true)]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        [Browsable(false)]
        public Brush TextBrush
        {
            get
            {
                return new SolidBrush(Color.FromArgb(this.WorkingTransparency, this.TextColor));
            }
        }

        [Category("Appearance - ObjectListView"), DefaultValue(typeof(Color), "DarkBlue"), NotifyParentProperty(true), Description("The color of the text")]
        public Color TextColor
        {
            get
            {
                return this.textColor;
            }
            set
            {
                this.textColor = value;
            }
        }

        [DefaultValue(true), Description("Will the text wrap?"), Category("Appearance - ObjectListView")]
        public bool Wrap
        {
            get
            {
                return this.wrap;
            }
            set
            {
                this.wrap = value;
            }
        }
    }
}

