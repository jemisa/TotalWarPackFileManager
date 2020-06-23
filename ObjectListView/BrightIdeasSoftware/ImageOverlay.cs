namespace BrightIdeasSoftware
{
    using BrightIdeasSoftware.Design;
    using System;
    using System.ComponentModel;
    using System.Drawing;

    [TypeConverter(typeof(OverlayConverter))]
    public class ImageOverlay : ImageAdornment, ITransparentOverlay, IOverlay
    {
        private int insetX = 20;
        private int insetY = 20;

        public ImageOverlay()
        {
            base.Alignment = ContentAlignment.BottomRight;
        }

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            Rectangle rectangle = r;
            rectangle.Inflate(-this.InsetX, -this.InsetY);
            base.DrawImage(g, rectangle, base.Image, 0xff);
        }

        [Description("The horizontal inset by which the position of the overlay will be adjusted"), Category("Appearance - ObjectListView"), DefaultValue(20), NotifyParentProperty(true)]
        public int InsetX
        {
            get
            {
                return this.insetX;
            }
            set
            {
                this.insetX = Math.Max(0, value);
            }
        }

        [DefaultValue(20), Category("Appearance - ObjectListView"), NotifyParentProperty(true), Description("Gets or sets the vertical inset by which the position of the overlay will be adjusted")]
        public int InsetY
        {
            get
            {
                return this.insetY;
            }
            set
            {
                this.insetY = Math.Max(0, value);
            }
        }
    }
}

