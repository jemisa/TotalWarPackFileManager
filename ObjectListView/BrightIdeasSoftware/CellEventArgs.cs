namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class CellEventArgs : EventArgs
    {
        private OLVColumn column;
        private int columnIndex = -1;
        public bool Handled;
        private OlvListViewHitTestInfo hitTest;
        private OLVListItem item;
        private ObjectListView listView;
        private Point location;
        private object model;
        private Keys modifierKeys;
        private int rowIndex = -1;
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

        public OlvListViewHitTestInfo HitTest
        {
            get
            {
                return this.hitTest;
            }
            internal set
            {
                this.hitTest = value;
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

        public Point Location
        {
            get
            {
                return this.location;
            }
            internal set
            {
                this.location = value;
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

        public Keys ModifierKeys
        {
            get
            {
                return this.modifierKeys;
            }
            internal set
            {
                this.modifierKeys = value;
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
    }
}

