namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class LightBoxDecoration : BorderDecoration
    {
        public LightBoxDecoration()
        {
            base.BoundsPadding = new Size(-1, 4);
            base.CornerRounding = 8f;
            base.FillBrush = new SolidBrush(Color.FromArgb(0x30, Color.Black));
        }

        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if (r.Contains(olv.PointToClient(Cursor.Position)))
            {
                using (Region region = new Region(r))
                {
                    Rectangle rowBounds = base.RowBounds;
                    rowBounds.Inflate(base.BoundsPadding);
                    region.Exclude(base.GetRoundedRect(rowBounds, base.CornerRounding));
                    Region clip = g.Clip;
                    g.Clip = region;
                    g.FillRectangle(base.FillBrush, r);
                    g.Clip = clip;
                }
            }
        }
    }
}

