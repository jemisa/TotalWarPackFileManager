namespace BrightIdeasSoftware
{
    using System;

    public class FormatCellEventArgs : FormatRowEventArgs
    {
        private OLVColumn column;
        private int columnIndex = -1;
        private OLVListSubItem subItem;

        public OLVColumn Column
        {
            get
            {
                return this.column;
            }
            internal set
            {
                this.column = value;
            }
        }

        public int ColumnIndex
        {
            get
            {
                return this.columnIndex;
            }
            internal set
            {
                this.columnIndex = value;
            }
        }

        public OLVListSubItem SubItem
        {
            get
            {
                return this.subItem;
            }
            internal set
            {
                this.subItem = value;
            }
        }
    }
}

