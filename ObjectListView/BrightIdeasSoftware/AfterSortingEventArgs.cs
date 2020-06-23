namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class AfterSortingEventArgs : EventArgs
    {
        private OLVColumn columnToGroupBy;
        private OLVColumn columnToSort;
        private System.Windows.Forms.SortOrder groupByOrder;
        private OLVColumn secondaryColumnToSort;
        private System.Windows.Forms.SortOrder secondarySortOrder;
        private System.Windows.Forms.SortOrder sortOrder;

        public AfterSortingEventArgs(BeforeSortingEventArgs args)
        {
            this.columnToGroupBy = args.ColumnToGroupBy;
            this.groupByOrder = args.GroupByOrder;
            this.columnToSort = args.ColumnToSort;
            this.sortOrder = args.SortOrder;
            this.secondaryColumnToSort = args.SecondaryColumnToSort;
            this.secondarySortOrder = args.SecondarySortOrder;
        }

        public AfterSortingEventArgs(OLVColumn groupColumn, System.Windows.Forms.SortOrder groupOrder, OLVColumn column, System.Windows.Forms.SortOrder order, OLVColumn column2, System.Windows.Forms.SortOrder order2)
        {
            this.columnToGroupBy = groupColumn;
            this.groupByOrder = groupOrder;
            this.columnToSort = column;
            this.sortOrder = order;
            this.secondaryColumnToSort = column2;
            this.secondarySortOrder = order2;
        }

        public OLVColumn ColumnToGroupBy
        {
            get
            {
                return this.columnToGroupBy;
            }
        }

        public OLVColumn ColumnToSort
        {
            get
            {
                return this.columnToSort;
            }
        }

        public System.Windows.Forms.SortOrder GroupByOrder
        {
            get
            {
                return this.groupByOrder;
            }
        }

        public OLVColumn SecondaryColumnToSort
        {
            get
            {
                return this.secondaryColumnToSort;
            }
        }

        public System.Windows.Forms.SortOrder SecondarySortOrder
        {
            get
            {
                return this.secondarySortOrder;
            }
        }

        public System.Windows.Forms.SortOrder SortOrder
        {
            get
            {
                return this.sortOrder;
            }
        }
    }
}

