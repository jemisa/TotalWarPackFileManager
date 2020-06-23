namespace BrightIdeasSoftware
{
    using System;

    public class VirtualListVersion1DataSource : AbstractVirtualListDataSource
    {
        private RowGetterDelegate rowGetter;

        public VirtualListVersion1DataSource(VirtualObjectListView listView) : base(listView)
        {
        }

        public override object GetNthObject(int n)
        {
            if (this.RowGetter == null)
            {
                return null;
            }
            return this.RowGetter(n);
        }

        public override int SearchText(string value, int first, int last, OLVColumn column)
        {
            return AbstractVirtualListDataSource.DefaultSearchText(value, first, last, column, this);
        }

        public RowGetterDelegate RowGetter
        {
            get
            {
                return this.rowGetter;
            }
            set
            {
                this.rowGetter = value;
            }
        }
    }
}

