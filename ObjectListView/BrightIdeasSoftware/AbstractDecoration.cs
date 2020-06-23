namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class AbstractDecoration : IDecoration, IOverlay
    {
        private OLVListItem listItem;
        private OLVListSubItem subItem;

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
        }

        public Rectangle CellBounds
        {
            get
            {
                if ((this.ListItem == null) || (this.SubItem == null))
                {
                    return Rectangle.Empty;
                }
                return this.ListItem.GetSubItemBounds(this.ListItem.SubItems.IndexOf(this.SubItem));
            }
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

        public Rectangle RowBounds
        {
            get
            {
                if (this.ListItem == null)
                {
                    return Rectangle.Empty;
                }
                return this.ListItem.Bounds;
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

