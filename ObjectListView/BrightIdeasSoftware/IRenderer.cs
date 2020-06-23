namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public interface IRenderer
    {
        Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex);
        void HitTest(OlvListViewHitTestInfo hti, int x, int y);
        bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject);
        bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject);
    }
}

