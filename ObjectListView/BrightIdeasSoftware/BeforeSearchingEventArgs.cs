namespace BrightIdeasSoftware
{
    using System;

    public class BeforeSearchingEventArgs : CancellableEventArgs
    {
        public int StartSearchFrom;
        public string StringToFind;

        public BeforeSearchingEventArgs(string stringToFind, int startSearchFrom)
        {
            this.StringToFind = stringToFind;
            this.StartSearchFrom = startSearchFrom;
        }
    }
}

