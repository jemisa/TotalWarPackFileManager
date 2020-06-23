namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class DescribedTaskRenderer : BaseRenderer
    {
        private Size cellPadding = new Size(2, 2);
        private string descriptionAspectName;
        private Color descriptionColor = Color.DimGray;
        private Font descriptionFont;
        private Munger descriptionGetter;
        private int imageTextSpace = 4;
        private Color titleColor;
        private Font titleFont;

        public virtual void DrawDescribedTask(Graphics g, Rectangle r, string title, string description, Image image)
        {
            SolidBrush brush;
            Rectangle rectangle = r;
            rectangle.Inflate(-this.CellPadding.Width, -this.CellPadding.Height);
            Rectangle rect = rectangle;
            if (image != null)
            {
                g.DrawImage(image, rectangle.Location);
                int num = image.Width + this.ImageTextSpace;
                rect.X += num;
                rect.Width -= num;
            }
            if (base.IsItemSelected && !base.ListView.UseTranslucentSelection)
            {
                using (brush = new SolidBrush(this.GetTextBackgroundColor()))
                {
                    g.FillRectangle(brush, rect);
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                using (StringFormat format = new StringFormat(StringFormatFlags.NoWrap))
                {
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Near;
                    Font titleFontOrDefault = this.TitleFontOrDefault;
                    using (brush = new SolidBrush(this.TitleColorOrDefault))
                    {
                        g.DrawString(title, titleFontOrDefault, brush, rect, format);
                    }
                    SizeF ef = g.MeasureString(title, titleFontOrDefault, rect.Width, format);
                    rect.Y += (int) ef.Height;
                    rect.Height -= (int) ef.Height;
                }
            }
            if (!string.IsNullOrEmpty(description))
            {
                using (StringFormat format2 = new StringFormat())
                {
                    format2.Trimming = StringTrimming.EllipsisCharacter;
                    using (brush = new SolidBrush(this.DescriptionColorOrDefault))
                    {
                        g.DrawString(description, this.DescriptionFontOrDefault, brush, rect, format2);
                    }
                }
            }
        }

        protected virtual string GetDescription()
        {
            if (string.IsNullOrEmpty(this.DescriptionAspectName))
            {
                return string.Empty;
            }
            if (this.descriptionGetter == null)
            {
                this.descriptionGetter = new Munger(this.DescriptionAspectName);
            }
            return (this.descriptionGetter.GetValue(base.RowObject) as string);
        }

        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            if (base.Bounds.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.Text;
            }
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            this.DrawDescribedTask(g, r, base.Aspect as string, this.GetDescription(), this.GetImage());
        }

        [Description("The number of pixels that renderer will leave empty around the edge of the cell"), Category("Appearance - ObjectListView"), DefaultValue(typeof(Size), "2,2")]
        public Size CellPadding
        {
            get
            {
                return this.cellPadding;
            }
            set
            {
                this.cellPadding = value;
            }
        }

        [Description("The name of the aspect of the model object that contains the task description"), Category("Appearance - ObjectListView"), DefaultValue((string) null)]
        public string DescriptionAspectName
        {
            get
            {
                return this.descriptionAspectName;
            }
            set
            {
                this.descriptionAspectName = value;
            }
        }

        [DefaultValue(typeof(Color), "DimGray"), Category("Appearance - ObjectListView"), Description("The color of the description")]
        public Color DescriptionColor
        {
            get
            {
                return this.descriptionColor;
            }
            set
            {
                this.descriptionColor = value;
            }
        }

        [Browsable(false)]
        public Color DescriptionColorOrDefault
        {
            get
            {
                if (this.DescriptionColor.IsEmpty || (base.IsItemSelected && !base.ListView.UseTranslucentSelection))
                {
                    return this.GetForegroundColor();
                }
                return this.DescriptionColor;
            }
        }

        [Category("Appearance - ObjectListView"), Description("The font that will be used to draw the description of the task"), DefaultValue((string) null)]
        public Font DescriptionFont
        {
            get
            {
                return this.descriptionFont;
            }
            set
            {
                this.descriptionFont = value;
            }
        }

        [Browsable(false)]
        public Font DescriptionFontOrDefault
        {
            get
            {
                return (this.DescriptionFont ?? base.ListView.Font);
            }
        }

        [Category("Appearance - ObjectListView"), Description("The number of pixels that that will be left between the image and the text"), DefaultValue(4)]
        public int ImageTextSpace
        {
            get
            {
                return this.imageTextSpace;
            }
            set
            {
                this.imageTextSpace = value;
            }
        }

        [Category("Appearance - ObjectListView"), Description("The color of the title"), DefaultValue(typeof(Color), "")]
        public Color TitleColor
        {
            get
            {
                return this.titleColor;
            }
            set
            {
                this.titleColor = value;
            }
        }

        [Browsable(false)]
        public Color TitleColorOrDefault
        {
            get
            {
                if (base.IsItemSelected || this.TitleColor.IsEmpty)
                {
                    return this.GetForegroundColor();
                }
                return this.TitleColor;
            }
        }

        [Category("Appearance - ObjectListView"), Description("The font that will be used to draw the title of the task"), DefaultValue((string) null)]
        public Font TitleFont
        {
            get
            {
                return this.titleFont;
            }
            set
            {
                this.titleFont = value;
            }
        }

        [Browsable(false)]
        public Font TitleFontOrDefault
        {
            get
            {
                return (this.TitleFont ?? base.ListView.Font);
            }
        }
    }
}

