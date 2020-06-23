namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    public class BorderDecoration : AbstractDecoration
    {
        private Pen borderPen;
        private Size boundsPadding;
        private float cornerRounding;
        private Brush fillBrush;

        public BorderDecoration() : this(new Pen(Color.FromArgb(0x40, Color.Blue), 1f))
        {
        }

        public BorderDecoration(Pen borderPen)
        {
            this.boundsPadding = new Size(-1, 2);
            this.cornerRounding = 16f;
            this.fillBrush = new SolidBrush(Color.FromArgb(0x40, Color.Blue));
            this.BorderPen = borderPen;
        }

        public BorderDecoration(Pen borderPen, Brush fill)
        {
            this.boundsPadding = new Size(-1, 2);
            this.cornerRounding = 16f;
            this.fillBrush = new SolidBrush(Color.FromArgb(0x40, Color.Blue));
            this.BorderPen = borderPen;
            this.FillBrush = fill;
        }

        protected virtual Rectangle CalculateBounds()
        {
            return Rectangle.Empty;
        }

        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            Rectangle bounds = this.CalculateBounds();
            if (!bounds.IsEmpty)
            {
                this.DrawFilledBorder(g, bounds);
            }
        }

        protected void DrawFilledBorder(Graphics g, Rectangle bounds)
        {
            bounds.Inflate(this.BoundsPadding);
            GraphicsPath roundedRect = this.GetRoundedRect(bounds, this.CornerRounding);
            if (this.FillBrush != null)
            {
                g.FillPath(this.FillBrush, roundedRect);
            }
            if (this.BorderPen != null)
            {
                g.DrawPath(this.BorderPen, roundedRect);
            }
        }

        protected GraphicsPath GetRoundedRect(RectangleF rect, float diameter)
        {
            GraphicsPath path = new GraphicsPath();
            if (diameter <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }
            RectangleF ef = new RectangleF(rect.X, rect.Y, diameter, diameter);
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

        public Pen BorderPen
        {
            get
            {
                return this.borderPen;
            }
            set
            {
                this.borderPen = value;
            }
        }

        public Size BoundsPadding
        {
            get
            {
                return this.boundsPadding;
            }
            set
            {
                this.boundsPadding = value;
            }
        }

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

        public Brush FillBrush
        {
            get
            {
                return this.fillBrush;
            }
            set
            {
                this.fillBrush = value;
            }
        }
    }
}

