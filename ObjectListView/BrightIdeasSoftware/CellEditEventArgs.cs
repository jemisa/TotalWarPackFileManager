namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class CellEditEventArgs : EventArgs
    {
        public bool Cancel;
        private Rectangle cellBounds;
        private OLVColumn column;
        public System.Windows.Forms.Control Control;
        private OLVListItem listViewItem;
        private object rowObject;
        private int subItemIndex;
        private object value;

        public CellEditEventArgs(OLVColumn column, System.Windows.Forms.Control control, Rectangle r, OLVListItem item, int subItemIndex)
        {
            this.Control = control;
            this.column = column;
            this.cellBounds = r;
            this.listViewItem = item;
            this.rowObject = item.RowObject;
            this.subItemIndex = subItemIndex;
            this.value = column.GetValue(item.RowObject);
        }

        public Rectangle CellBounds
        {
            get
            {
                return this.cellBounds;
            }
        }

        public OLVColumn Column
        {
            get
            {
                return this.column;
            }
        }

        public OLVListItem ListViewItem
        {
            get
            {
                return this.listViewItem;
            }
        }

        public object RowObject
        {
            get
            {
                return this.rowObject;
            }
        }

        public int SubItemIndex
        {
            get
            {
                return this.subItemIndex;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

