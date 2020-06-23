namespace BrightIdeasSoftware
{
    using System;

    public class FormatRowEventArgs : EventArgs
    {
        private int displayIndex = -1;
        private OLVListItem item;
        private ObjectListView listView;
        private int rowIndex = -1;
        public bool UseCellFormatEvents;

        public int DisplayIndex
        {
            get
            {
                return this.displayIndex;
            }
            internal set
            {
                this.displayIndex = value;
            }
        }

        public OLVListItem Item
        {
            get
            {
                return this.item;
            }
            internal set
            {
                this.item = value;
            }
        }

        public ObjectListView ListView
        {
            get
            {
                return this.listView;
            }
            internal set
            {
                this.listView = value;
            }
        }

        public object Model
        {
            get
            {
                return this.Item.RowObject;
            }
        }

        public int RowIndex
        {
            get
            {
                return this.rowIndex;
            }
            internal set
            {
                this.rowIndex = value;
            }
        }
    }
}

