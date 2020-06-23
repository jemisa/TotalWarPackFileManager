namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ToolTipShowingEventArgs : CellEventArgs
    {
        public int? AutoPopDelay;
        public Color? BackColor;
        public System.Drawing.Font Font;
        public Color? ForeColor;
        public bool? IsBalloon;
        public System.Windows.Forms.RightToLeft RightToLeft;
        public BrightIdeasSoftware.ToolTipControl.StandardIcons? StandardIcon;
        public string Text;
        public string Title;
        private BrightIdeasSoftware.ToolTipControl toolTipControl;

        public BrightIdeasSoftware.ToolTipControl ToolTipControl
        {
            get
            {
                return this.toolTipControl;
            }
            internal set
            {
                this.toolTipControl = value;
            }
        }
    }
}

