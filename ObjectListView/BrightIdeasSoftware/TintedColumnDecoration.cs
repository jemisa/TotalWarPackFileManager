namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class TintedColumnDecoration : AbstractDecoration
    {
        private OLVColumn columnToTint;
        private Color tint;
        private SolidBrush tintBrush;

        public TintedColumnDecoration()
        {
            this.Tint = Color.FromArgb(15, Color.Blue);
        }

        public TintedColumnDecoration(OLVColumn column) : this()
        {
            this.ColumnToTint = column;
        }

        public override void Draw(ObjectListView olv, Graphics g, Rectangle r)
        {
            if ((olv.View == View.Details) && (olv.GetItemCount() != 0))
            {
                OLVColumn column = this.ColumnToTint ?? olv.SelectedColumn;
                if (column != null)
                {
                    Point scrolledColumnSides = BrightIdeasSoftware.NativeMethods.GetScrolledColumnSides(olv, column.Index);
                    if (scrolledColumnSides.X != -1)
                    {
                        Rectangle rect = new Rectangle(scrolledColumnSides.X, r.Top, scrolledColumnSides.Y - scrolledColumnSides.X, r.Bottom);
                        OLVListItem lastItemInDisplayOrder = olv.GetLastItemInDisplayOrder();
                        if (lastItemInDisplayOrder != null)
                        {
                            Rectangle bounds = lastItemInDisplayOrder.Bounds;
                            if (!(bounds.IsEmpty || (bounds.Bottom >= rect.Bottom)))
                            {
                                rect.Height = bounds.Bottom - rect.Top;
                            }
                        }
                        g.FillRectangle(this.tintBrush, rect);
                    }
                }
            }
        }

        public OLVColumn ColumnToTint
        {
            get
            {
                return this.columnToTint;
            }
            set
            {
                this.columnToTint = value;
            }
        }

        public Color Tint
        {
            get
            {
                return this.tint;
            }
            set
            {
                if (this.tint != value)
                {
                    if (this.tintBrush != null)
                    {
                        this.tintBrush.Dispose();
                        this.tintBrush = null;
                    }
                    this.tint = value;
                    this.tintBrush = new SolidBrush(this.tint);
                }
            }
        }
    }
}

