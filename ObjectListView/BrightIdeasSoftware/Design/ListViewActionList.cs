namespace BrightIdeasSoftware.Design
{
    using BrightIdeasSoftware;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class ListViewActionList : DesignerActionList
    {
        private ComponentDesigner _designer;

        public ListViewActionList(ComponentDesigner designer) : base(designer.Component)
        {
            this._designer = designer;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "InvokeColumnsDialog", "Edit Columns", "Properties", "Edit the columns of this ObjectListView", true));
            items.Add(new DesignerActionPropertyItem("View", "View", "Properties", "View"));
            items.Add(new DesignerActionPropertyItem("SmallImageList", "Small Image List", "Properties", "Small Image List"));
            items.Add(new DesignerActionPropertyItem("LargeImageList", "Large Image List", "Properties", "Large Image List"));
            return items;
        }

        public void InvokeColumnsDialog()
        {
            EditorServiceContext.EditValue(this._designer, base.Component, "Columns");
        }

        public ImageList LargeImageList
        {
            get
            {
                return ((ObjectListView) base.Component).LargeImageList;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["LargeImageList"].SetValue(base.Component, value);
            }
        }

        public ImageList SmallImageList
        {
            get
            {
                return ((ObjectListView) base.Component).SmallImageList;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["SmallImageList"].SetValue(base.Component, value);
            }
        }

        public System.Windows.Forms.View View
        {
            get
            {
                return ((ListView) base.Component).View;
            }
            set
            {
                TypeDescriptor.GetProperties(base.Component)["View"].SetValue(base.Component, value);
            }
        }
    }
}

