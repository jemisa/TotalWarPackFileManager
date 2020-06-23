namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class ListViewGroupComparer : IComparer<ListViewGroup>
    {
        private SortOrder sortOrder;

        public ListViewGroupComparer(SortOrder order)
        {
            this.sortOrder = order;
        }

        public int Compare(ListViewGroup x, ListViewGroup y)
        {
            int num;
            IComparable tag = x.Tag as IComparable;
            if (((tag != null) && (y.Tag != null)) && (y.Tag != DBNull.Value))
            {
                num = tag.CompareTo(y.Tag);
            }
            else
            {
                num = string.Compare(x.Header, y.Header, StringComparison.CurrentCultureIgnoreCase);
            }
            if (this.sortOrder == SortOrder.Descending)
            {
                num = -num;
            }
            return num;
        }
    }
}

