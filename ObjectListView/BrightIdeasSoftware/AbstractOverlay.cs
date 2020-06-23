namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class AbstractOverlay : ITransparentOverlay, IOverlay
    {
        private int transparency = 0x80;

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
        }

        [Description("How transparent should this overlay be"), NotifyParentProperty(true), DefaultValue(0x80), Category("Appearance - ObjectListView")]
        public int Transparency
        {
            get
            {
                return this.transparency;
            }
            set
            {
                this.transparency = Math.Min(0xff, Math.Max(0, value));
            }
        }
    }
}

