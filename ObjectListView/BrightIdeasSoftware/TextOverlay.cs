namespace BrightIdeasSoftware
{
    using BrightIdeasSoftware.Design;
    using System;
    using System.ComponentModel;
    using System.Drawing;

    [TypeConverter(typeof(OverlayConverter))]
    public class TextOverlay : TextAdornment, ITransparentOverlay, IOverlay
    {
        private int insetX = 20;
        private int insetY = 20;

        public TextOverlay()
        {
            base.Alignment = ContentAlignment.BottomRight;
        }

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            Rectangle rectangle = r;
            rectangle.Inflate(-this.InsetX, -this.InsetY);
            base.DrawText(g, rectangle, base.Text, 0xff);
        }

        [Description("The horizontal inset by which the position of the overlay will be adjusted"), DefaultValue(20), NotifyParentProperty(true), Category("Appearance - ObjectListView")]
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

        [DefaultValue(20), Category("Appearance - ObjectListView"), Description("Gets or sets the vertical inset by which the position of the overlay will be adjusted"), NotifyParentProperty(true)]
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

        [Obsolete("Use CornerRounding instead", false), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool RoundCorneredBorder
        {
            get
            {
                return (base.CornerRounding > 0f);
            }
            set
            {
                if (value)
                {
                    base.CornerRounding = 16f;
                }
                else
                {
                    base.CornerRounding = 0f;
                }
            }
        }
    }
}

