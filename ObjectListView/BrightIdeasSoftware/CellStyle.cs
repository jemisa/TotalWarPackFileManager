namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CellStyle : IItemStyle
    {
        private Color backColor;
        private System.Drawing.Font font;
        private System.Drawing.FontStyle fontStyle;
        private Color foreColor;

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
    }
}

