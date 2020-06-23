namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class SimpleDragSource : IDragSource
    {
        private bool refreshAfterDrop;

        public SimpleDragSource()
        {
        }

        public SimpleDragSource(bool refreshAfterDrop)
        {
            this.RefreshAfterDrop = refreshAfterDrop;
        }

        protected virtual object CreateDataObject(ObjectListView olv)
        {
            OLVDataObject obj2 = new OLVDataObject(olv);
            obj2.CreateTextFormats();
            return obj2;
        }

        public virtual void EndDrag(object dragObject, DragDropEffects effect)
        {
            OLVDataObject obj2 = dragObject as OLVDataObject;
            if ((obj2 != null) && this.RefreshAfterDrop)
            {
                obj2.ListView.RefreshObjects(obj2.ModelObjects);
            }
        }

        public virtual DragDropEffects GetAllowedEffects(object data)
        {
            return (DragDropEffects.Link | DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll);
        }

        public virtual object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item)
        {
            if (button != MouseButtons.Left)
            {
                return null;
            }
            return this.CreateDataObject(olv);
        }

        public bool RefreshAfterDrop
        {
            get
            {
                return this.refreshAfterDrop;
            }
            set
            {
                this.refreshAfterDrop = value;
            }
        }
    }
}

