namespace BrightIdeasSoftware
{
    using System;

    public class GroupTaskClickedEventArgs : EventArgs
    {
        private OLVGroup group;

        public GroupTaskClickedEventArgs(OLVGroup group)
        {
            this.group = group;
        }

        public OLVGroup Group
        {
            get
            {
                return this.group;
            }
        }
    }
}

