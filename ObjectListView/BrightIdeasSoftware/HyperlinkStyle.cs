namespace BrightIdeasSoftware
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class HyperlinkStyle : Component
    {
        private CellStyle normalStyle;
        private Cursor overCursor;
        private CellStyle overStyle;
        private CellStyle visitedStyle;

        public HyperlinkStyle()
        {
            this.Normal = new CellStyle();
            this.Normal.ForeColor = Color.Blue;
            this.Over = new CellStyle();
            this.Over.FontStyle = FontStyle.Underline;
            this.Visited = new CellStyle();
            this.Visited.ForeColor = Color.Purple;
            this.OverCursor = Cursors.Hand;
        }

        [Category("Appearance"), Description("How should hyperlinks be drawn")]
        public CellStyle Normal
        {
            get
            {
                return this.normalStyle;
            }
            set
            {
                this.normalStyle = value;
            }
        }

        [Description("How should hyperlinks be drawn when the mouse is over them?"), Category("Appearance")]
        public CellStyle Over
        {
            get
            {
                return this.overStyle;
            }
            set
            {
                this.overStyle = value;
            }
        }

        [Description("What cursor should be shown when the mouse is over a link?"), Category("Appearance")]
        public Cursor OverCursor
        {
            get
            {
                return this.overCursor;
            }
            set
            {
                this.overCursor = value;
            }
        }

        [Category("Appearance"), Description("How should hyperlinks be drawn after they have been clicked")]
        public CellStyle Visited
        {
            get
            {
                return this.visitedStyle;
            }
            set
            {
                this.visitedStyle = value;
            }
        }
    }
}

