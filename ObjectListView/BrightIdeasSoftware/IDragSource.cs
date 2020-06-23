namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public interface IDragSource
    {
        void EndDrag(object dragObject, DragDropEffects effect);
        DragDropEffects GetAllowedEffects(object dragObject);
        object StartDrag(ObjectListView olv, MouseButtons button, OLVListItem item);
    }
}

