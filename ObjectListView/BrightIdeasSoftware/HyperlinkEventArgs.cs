namespace BrightIdeasSoftware
{
    using System;

    public class HyperlinkEventArgs : EventArgs
    {
        private OLVColumn column;
        private int columnIndex = -1;
        public bool Handled;
        private OLVListItem item;
        private ObjectListView listView;
        private object model;
        private int rowIndex = -1;
        private OLVListSubItem subItem;
        private string url;

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
                return this.model;
            }
            internal set
            {
                this.model = value;
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

        public string Url
        {
            get
            {
                return this.url;
            }
            internal set
            {
                this.url = value;
            }
        }
    }
}

