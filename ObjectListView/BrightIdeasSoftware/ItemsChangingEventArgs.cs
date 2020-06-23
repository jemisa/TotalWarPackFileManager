namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;

    public class ItemsChangingEventArgs : CancellableEventArgs
    {
        public IEnumerable NewObjects;
        private IEnumerable oldObjects;

        public ItemsChangingEventArgs(IEnumerable oldObjects, IEnumerable newObjects)
        {
            this.oldObjects = oldObjects;
            this.NewObjects = newObjects;
        }

        public IEnumerable OldObjects
        {
            get
            {
                return this.oldObjects;
            }
        }
    }
}

