namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class BeforeSortingEventArgs : CancellableEventArgs
    {
        public OLVColumn ColumnToGroupBy;
        public OLVColumn ColumnToSort;
        public System.Windows.Forms.SortOrder GroupByOrder;
        public bool Handled;
        public OLVColumn SecondaryColumnToSort;
        public System.Windows.Forms.SortOrder SecondarySortOrder;
        public System.Windows.Forms.SortOrder SortOrder;

        public BeforeSortingEventArgs(OLVColumn column, System.Windows.Forms.SortOrder order, OLVColumn column2, System.Windows.Forms.SortOrder order2)
        {
            this.ColumnToGroupBy = column;
            this.GroupByOrder = order;
            this.ColumnToSort = column;
            this.SortOrder = order;
            this.SecondaryColumnToSort = column2;
            this.SecondarySortOrder = order2;
        }

        public BeforeSortingEventArgs(OLVColumn groupColumn, System.Windows.Forms.SortOrder groupOrder, OLVColumn column, System.Windows.Forms.SortOrder order, OLVColumn column2, System.Windows.Forms.SortOrder order2)
        {
            this.ColumnToGroupBy = groupColumn;
            this.GroupByOrder = groupOrder;
            this.ColumnToSort = column;
            this.SortOrder = order;
            this.SecondaryColumnToSort = column2;
            this.SecondarySortOrder = order2;
        }
    }
}

