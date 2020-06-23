namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class ModelObjectComparer : IComparer, IComparer<object>
    {
        private OLVColumn column;
        private ModelObjectComparer secondComparer;
        private SortOrder sortOrder;

        public ModelObjectComparer(OLVColumn col, SortOrder order)
        {
            this.column = col;
            this.sortOrder = order;
        }

        public ModelObjectComparer(OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2) : this(col, order)
        {
            if (((col != col2) && (col2 != null)) && (order2 != SortOrder.None))
            {
                this.secondComparer = new ModelObjectComparer(col2, order2);
            }
        }

        public int Compare(object x, object y)
        {
            int num = 0;
            object obj2 = this.column.GetValue(x);
            object obj3 = this.column.GetValue(y);
            if (this.sortOrder == SortOrder.None)
            {
                return 0;
            }
            bool flag = (obj2 == null) || (obj2 == DBNull.Value);
            bool flag2 = (obj3 == null) || (obj3 == DBNull.Value);
            if (flag || flag2)
            {
                if (flag && flag2)
                {
                    num = 0;
                }
                else
                {
                    num = flag ? -1 : 1;
                }
            }
            else
            {
                num = this.CompareValues(obj2, obj3);
            }
            if (this.sortOrder == SortOrder.Descending)
            {
                num = -num;
            }
            if ((num == 0) && (this.secondComparer != null))
            {
                num = this.secondComparer.Compare(x, y);
            }
            return num;
        }

        public int CompareValues(object x, object y)
        {
            string strA = x as string;
            if (strA != null)
            {
                return string.Compare(strA, (string) y, StringComparison.CurrentCultureIgnoreCase);
            }
            IComparable comparable = x as IComparable;
            if (comparable != null)
            {
                return comparable.CompareTo(y);
            }
            return 0;
        }
    }
}

