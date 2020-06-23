namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class TextDecoration : TextAdornment, IDecoration, IOverlay
    {
        private OLVListItem listItem;
        private OLVListSubItem subItem;

        public TextDecoration()
        {
            base.Alignment = ContentAlignment.MiddleRight;
        }

        public TextDecoration(string text) : this()
        {
            base.Text = text;
        }

        public TextDecoration(string text, ContentAlignment alignment) : this()
        {
            base.Text = text;
            base.Alignment = alignment;
        }

        public TextDecoration(string text, int transparency) : this()
        {
            base.Text = text;
            base.Transparency = transparency;
        }

        public TextDecoration(string text, int transparency, ContentAlignment alignment) : this()
        {
            base.Text = text;
            base.Transparency = transparency;
            base.Alignment = alignment;
        }

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            base.DrawText(g, base.CalculateItemBounds(this.ListItem, this.SubItem));
        }

        public OLVListItem ListItem
        {
            get
            {
                return this.listItem;
            }
            set
            {
                this.listItem = value;
            }
        }

        public OLVListSubItem SubItem
        {
            get
            {
                return this.subItem;
            }
            set
            {
                this.subItem = value;
            }
        }
    }
}

