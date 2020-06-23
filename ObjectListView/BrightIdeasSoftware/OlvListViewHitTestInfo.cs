namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class OlvListViewHitTestInfo
    {
        public BrightIdeasSoftware.HitTestLocation HitTestLocation;
        private OLVListItem item;
        private ListViewHitTestLocations location;
        private OLVListSubItem subItem;
        public object UserData;

        public OlvListViewHitTestInfo(ListViewHitTestInfo hti)
        {
            this.item = (OLVListItem) hti.Item;
            this.subItem = (OLVListSubItem) hti.SubItem;
            this.location = hti.Location;
            switch (hti.Location)
            {
                case ListViewHitTestLocations.Image:
                    this.HitTestLocation = BrightIdeasSoftware.HitTestLocation.Image;
                    return;

                case ListViewHitTestLocations.Label:
                    this.HitTestLocation = BrightIdeasSoftware.HitTestLocation.Text;
                    return;

                case ListViewHitTestLocations.StateImage:
                    this.HitTestLocation = BrightIdeasSoftware.HitTestLocation.CheckBox;
                    return;
            }
            this.HitTestLocation = BrightIdeasSoftware.HitTestLocation.Nothing;
        }

        public OLVColumn Column
        {
            get
            {
                int columnIndex = this.ColumnIndex;
                if (columnIndex < 0)
                {
                    return null;
                }
                return this.ListView.GetColumn(columnIndex);
            }
        }

        public int ColumnIndex
        {
            get
            {
                if ((this.Item == null) || (this.SubItem == null))
                {
                    return -1;
                }
                return this.Item.SubItems.IndexOf(this.SubItem);
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
                if (this.Item == null)
                {
                    return null;
                }
                return (ObjectListView) this.Item.ListView;
            }
        }

        public ListViewHitTestLocations Location
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

        public int RowIndex
        {
            get
            {
                if (this.Item == null)
                {
                    return -1;
                }
                return this.Item.Index;
            }
        }

        public object RowObject
        {
            get
            {
                if (this.Item == null)
                {
                    return null;
                }
                return this.Item.RowObject;
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

