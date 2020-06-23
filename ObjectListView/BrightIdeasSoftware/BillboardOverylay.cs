namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class BillboardOverylay : TextOverlay
    {
        private Point location;

        public BillboardOverylay()
        {
            base.Transparency = 0xff;
            base.BackColor = Color.PeachPuff;
            base.TextColor = Color.Black;
            base.BorderColor = Color.Empty;
            base.Font = new Font("Tahoma", 10f);
        }

        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (!string.IsNullOrEmpty(base.Text))
            {
                Rectangle textRect = base.CalculateTextBounds(g, r, base.Text);
                textRect.Location = this.Location;
                if (textRect.Right > r.Width)
                {
                    textRect.X = Math.Max(r.Left, r.Width - textRect.Width);
                }
                if (textRect.Bottom > r.Height)
                {
                    textRect.Y = Math.Max(r.Top, r.Height - textRect.Height);
                }
                base.DrawBorderedText(g, textRect, base.Text, 0xff);
            }
        }

        public Point Location
        {
            get
            {
                return this.location;
            }
            set
            {
                this.location = value;
            }
        }
    }
}

