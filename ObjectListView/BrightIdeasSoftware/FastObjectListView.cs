namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    public class FastObjectListView : VirtualObjectListView
    {
        public FastObjectListView()
        {
            this.DataSource = new FastObjectListDataSource(this);
            base.GroupingStrategy = new FastListGroupingStrategy();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override IEnumerable Objects
        {
            get
            {
                return ((FastObjectListDataSource) this.DataSource).ObjectList;
            }
            set
            {
                base.Objects = value;
            }
        }
    }
}

