namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;

    public class ItemsAddingEventArgs : CancellableEventArgs
    {
        public ICollection ObjectsToAdd;

        public ItemsAddingEventArgs(ICollection objectsToAdd)
        {
            this.ObjectsToAdd = objectsToAdd;
        }
    }
}

