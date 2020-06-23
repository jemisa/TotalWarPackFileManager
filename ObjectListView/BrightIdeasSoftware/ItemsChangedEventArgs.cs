namespace BrightIdeasSoftware
{
    using System;

    public class ItemsChangedEventArgs : EventArgs
    {
        private int newObjectCount;
        private int oldObjectCount;

        public ItemsChangedEventArgs()
        {
        }

        public ItemsChangedEventArgs(int oldObjectCount, int newObjectCount)
        {
            this.oldObjectCount = oldObjectCount;
            this.newObjectCount = newObjectCount;
        }

        public int NewObjectCount
        {
            get
            {
                return this.newObjectCount;
            }
        }

        public int OldObjectCount
        {
            get
            {
                return this.oldObjectCount;
            }
        }
    }
}

