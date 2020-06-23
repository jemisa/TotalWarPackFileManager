namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class AbstractDragSource : IDragSource
    {
        public virtual void EndDrag(object dragObject, DragDropEffects effect)
        {
        }

        public virtual DragDropEffects GetAllowedEffects(object data)
        {
            return DragDropEffects.None;
        }

        public virtual object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item)
        {
            return null;
        }
    }
}

