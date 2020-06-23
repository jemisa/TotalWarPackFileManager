namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    public delegate bool RenderDelegate(EventArgs e, Graphics g, Rectangle r, object rowObject);
}

