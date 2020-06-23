namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class ImageDecoration : ImageAdornment, IDecoration, IOverlay
    {
        private OLVListItem listItem;
        private OLVListSubItem subItem;

        public ImageDecoration()
        {
            base.Alignment = ContentAlignment.MiddleRight;
        }

        public ImageDecoration(Image image) : this()
        {
            base.Image = image;
        }

        public ImageDecoration(Image image, ContentAlignment alignment) : this()
        {
            base.Image = image;
            base.Alignment = alignment;
        }

        public ImageDecoration(Image image, int transparency) : this()
        {
            base.Image = image;
            base.Transparency = transparency;
        }

        public ImageDecoration(Image image, int transparency, ContentAlignment alignment) : this()
        {
            base.Image = image;
            base.Transparency = transparency;
            base.Alignment = alignment;
        }

        public virtual void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            base.DrawImage(g, base.CalculateItemBounds(this.ListItem, this.SubItem));
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

