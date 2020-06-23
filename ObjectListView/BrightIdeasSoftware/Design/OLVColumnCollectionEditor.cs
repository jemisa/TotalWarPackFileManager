namespace BrightIdeasSoftware.Design
{
    using BrightIdeasSoftware;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Windows.Forms;

    internal class OLVColumnCollectionEditor : CollectionEditor
    {
        public OLVColumnCollectionEditor(System.Type t) : base(t)
        {
        }

        protected override System.Type CreateCollectionItemType()
        {
            return typeof(OLVColumn);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ObjectListView instance = null;
            if (context != null)
            {
                instance = context.Instance as ObjectListView;
            }
            if (instance == null)
            {
                Debug.WriteLine("context.Instance was NOT an ObjectListView");
                ListView.ColumnHeaderCollection headers = (ListView.ColumnHeaderCollection) value;
                if (headers.Count == 0)
                {
                    headers.Add(new OLVColumn());
                    instance = (ObjectListView) headers[0].ListView;
                    headers.Clear();
                    instance.AllColumns.Clear();
                }
                else
                {
                    instance = (ObjectListView) headers[0].ListView;
                }
            }
            base.EditValue(context, provider, instance.AllColumns);
            List<OLVColumn> filteredColumns = instance.GetFilteredColumns(View.Details);
            instance.Columns.Clear();
            instance.Columns.AddRange(filteredColumns.ToArray());
            return instance.Columns;
        }

        protected override string GetDisplayText(object value)
        {
            OLVColumn column = value as OLVColumn;
            if ((column == null) || string.IsNullOrEmpty(column.AspectName))
            {
                return base.GetDisplayText(value);
            }
            return string.Format("{0} ({1})", base.GetDisplayText(value), column.AspectName);
        }
    }
}

