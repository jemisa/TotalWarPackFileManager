namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public class RowBorderDecoration : BorderDecoration
    {
        private int leftColumn = -1;
        private int rightColumn = -1;

        protected override Rectangle CalculateBounds()
        {
            Rectangle rowBounds = base.RowBounds;
            if (base.ListItem != null)
            {
                if (this.LeftColumn >= 0)
                {
                    Rectangle subItemBounds = base.ListItem.GetSubItemBounds(this.LeftColumn);
                    if (!subItemBounds.IsEmpty)
                    {
                        rowBounds.Width = rowBounds.Right - subItemBounds.Left;
                        rowBounds.X = subItemBounds.Left;
                    }
                }
                if (this.RightColumn >= 0)
                {
                    Rectangle rectangle3 = base.ListItem.GetSubItemBounds(this.RightColumn);
                    if (!rectangle3.IsEmpty)
                    {
                        rowBounds.Width = rectangle3.Right - rowBounds.Left;
                    }
                }
            }
            return rowBounds;
        }

        public int LeftColumn
        {
            get
            {
                return this.leftColumn;
            }
            set
            {
                this.leftColumn = value;
            }
        }

        public int RightColumn
        {
            get
            {
                return this.rightColumn;
            }
            set
            {
                this.rightColumn = value;
            }
        }
    }
}

