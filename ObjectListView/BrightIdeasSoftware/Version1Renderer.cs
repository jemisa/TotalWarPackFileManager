namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    [ToolboxItem(false)]
    internal class Version1Renderer : AbstractRenderer
    {
        public BrightIdeasSoftware.RenderDelegate RenderDelegate;

        public Version1Renderer(BrightIdeasSoftware.RenderDelegate renderDelegate)
        {
            this.RenderDelegate = renderDelegate;
        }

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            if (this.RenderDelegate == null)
            {
                return base.RenderSubItem(e, g, cellBounds, rowObject);
            }
            return this.RenderDelegate(e, g, cellBounds, rowObject);
        }
    }
}

