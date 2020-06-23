namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    public class HotItemStyle : Component, IItemStyle
    {
        private Color backColor;
        private IDecoration decoration;
        private System.Drawing.Font font;
        private System.Drawing.FontStyle fontStyle;
        private Color foreColor;
        private IOverlay overlay;

        [DefaultValue(typeof(Color), "")]
        public Color BackColor
        {
            get
            {
                return this.backColor;
            }
            set
            {
                this.backColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IDecoration Decoration
        {
            get
            {
                return this.decoration;
            }
            set
            {
                this.decoration = value;
            }
        }

        public System.Drawing.Font Font
        {
            get
            {
                return this.font;
            }
            set
            {
                this.font = value;
            }
        }

        [DefaultValue(0)]
        public System.Drawing.FontStyle FontStyle
        {
            get
            {
                return this.fontStyle;
            }
            set
            {
                this.fontStyle = value;
            }
        }

        [DefaultValue(typeof(Color), "")]
        public Color ForeColor
        {
            get
            {
                return this.foreColor;
            }
            set
            {
                this.foreColor = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IOverlay Overlay
        {
            get
            {
                return this.overlay;
            }
            set
            {
                this.overlay = value;
            }
        }
    }
}

