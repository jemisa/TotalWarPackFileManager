namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;

    public class ItemsRemovingEventArgs : CancellableEventArgs
    {
        public ICollection ObjectsToRemove;

        public ItemsRemovingEventArgs(ICollection objectsToRemove)
        {
            this.ObjectsToRemove = objectsToRemove;
        }
    }
}

