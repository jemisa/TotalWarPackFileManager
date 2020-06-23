namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class CheckStateRenderer : BaseRenderer
    {
        private Rectangle CalculateCheckBoxBounds(Graphics g, Rectangle cellBounds)
        {
            Size smallImageSize = base.ListView.SmallImageSize;
            return this.AlignRectangle(cellBounds, new Rectangle(0, 0, smallImageSize.Width, cellBounds.Height));
        }

        protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex)
        {
            return cellBounds;
        }

        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            if (this.CalculateCheckBoxBounds(g, base.Bounds).Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.CheckBox;
            }
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            r = this.CalculateCheckBoxBounds(g, r);
            this.DrawImage(g, r, base.Column.GetCheckStateImage(base.RowObject));
        }
    }
}

