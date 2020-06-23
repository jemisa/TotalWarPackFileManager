namespace BrightIdeasSoftware.Design
{
    using BrightIdeasSoftware;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class ListViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;
        private BrightIdeasSoftware.NativeMethods.HDHITTESTINFO hdrhit = new BrightIdeasSoftware.NativeMethods.HDHITTESTINFO();

        protected override bool GetHitTest(Point point)
        {
            ObjectListView component = (ObjectListView) base.Component;
            return (component.HeaderControl.ColumnIndexUnderCursor >= 0);
        }

        public override void Initialize(IComponent component)
        {
            ListView view = (ListView) component;
            this.OwnerDraw = view.OwnerDraw;
            view.OwnerDraw = false;
            view.UseCompatibleStateImageBehavior = false;
            base.AutoResizeHandles = true;
            base.Initialize(component);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["OwnerDraw"];
            if (oldPropertyDescriptor != null)
            {
                properties["OwnerDraw"] = TypeDescriptor.CreateProperty(typeof(BrightIdeasSoftware.Design.ListViewDesigner), oldPropertyDescriptor, new Attribute[0]);
            }
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["View"];
            if (descriptor2 != null)
            {
                properties["View"] = TypeDescriptor.CreateProperty(typeof(BrightIdeasSoftware.Design.ListViewDesigner), descriptor2, new Attribute[0]);
            }
            base.PreFilterProperties(properties);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                case 0x204e:
                {
                    BrightIdeasSoftware.NativeMethods.NMHDR nmhdr = (BrightIdeasSoftware.NativeMethods.NMHDR) Marshal.PtrToStructure(m.LParam, typeof(BrightIdeasSoftware.NativeMethods.NMHDR));
                    if (nmhdr.code == -327)
                    {
                        try
                        {
                            ((IComponentChangeService) this.GetService(typeof(IComponentChangeService))).OnComponentChanged(base.Component, null, null, null);
                        }
                        catch (InvalidOperationException)
                        {
                            return;
                        }
                    }
                    break;
                }
            }
            base.WndProc(ref m);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new BrightIdeasSoftware.Design.ListViewActionList(this));
                }
                return this._actionLists;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ObjectListView control = this.Control as ObjectListView;
                if (control != null)
                {
                    return control.AllColumns;
                }
                return base.AssociatedComponents;
            }
        }

        private bool OwnerDraw
        {
            get
            {
                return (bool) base.ShadowProperties["OwnerDraw"];
            }
            set
            {
                base.ShadowProperties["OwnerDraw"] = value;
            }
        }

        private System.Windows.Forms.View View
        {
            get
            {
                return ((ListView) base.Component).View;
            }
            set
            {
                ((ListView) base.Component).View = value;
            }
        }
    }
}

