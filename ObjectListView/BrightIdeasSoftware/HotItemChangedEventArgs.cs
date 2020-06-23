namespace BrightIdeasSoftware
{
    using System;

    public class HotItemChangedEventArgs : EventArgs
    {
        public bool Handled;
        private HitTestLocation newHotCellHitLocation;
        private int newHotColumnIndex;
        private int newHotRowIndex;
        private HitTestLocation oldHotCellHitLocation;
        private int oldHotColumnIndex;
        private int oldHotRowIndex;

        public HitTestLocation HotCellHitLocation
        {
            get
            {
                return this.newHotCellHitLocation;
            }
            internal set
            {
                this.newHotCellHitLocation = value;
            }
        }

        public int HotColumnIndex
        {
            get
            {
                return this.newHotColumnIndex;
            }
            internal set
            {
                this.newHotColumnIndex = value;
            }
        }

        public int HotRowIndex
        {
            get
            {
                return this.newHotRowIndex;
            }
            internal set
            {
                this.newHotRowIndex = value;
            }
        }

        public HitTestLocation OldHotCellHitLocation
        {
            get
            {
                return this.oldHotCellHitLocation;
            }
            internal set
            {
                this.oldHotCellHitLocation = value;
            }
        }

        public int OldHotColumnIndex
        {
            get
            {
                return this.oldHotColumnIndex;
            }
            internal set
            {
                this.oldHotColumnIndex = value;
            }
        }

        public int OldHotRowIndex
        {
            get
            {
                return this.oldHotRowIndex;
            }
            internal set
            {
                this.oldHotRowIndex = value;
            }
        }
    }
}

