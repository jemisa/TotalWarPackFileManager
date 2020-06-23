namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public interface IOverlay
    {
        void Draw(ObjectListView olv, Graphics g, Rectangle r);
    }
}

