namespace BrightIdeasSoftware
{
    using System;

    public class CellClickEventArgs : CellEventArgs
    {
        private int clickCount;

        public int ClickCount
        {
            get
            {
                return this.clickCount;
            }
            set
            {
                this.clickCount = value;
            }
        }
    }
}

