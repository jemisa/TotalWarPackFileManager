namespace BrightIdeasSoftware
{
    using System;

    public class AfterSearchingEventArgs : EventArgs
    {
        public bool Handled;
        private int indexSelected;
        private string stringToFind;

        public AfterSearchingEventArgs(string stringToFind, int indexSelected)
        {
            this.stringToFind = stringToFind;
            this.indexSelected = indexSelected;
        }

        public int IndexSelected
        {
            get
            {
                return this.indexSelected;
            }
        }

        public string StringToFind
        {
            get
            {
                return this.stringToFind;
            }
        }
    }
}

