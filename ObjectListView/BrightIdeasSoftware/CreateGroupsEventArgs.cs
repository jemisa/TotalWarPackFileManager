namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;

    public class CreateGroupsEventArgs : EventArgs
    {
        public bool Canceled;
        private IList<OLVGroup> groups;
        private GroupingParameters parameters;

        public CreateGroupsEventArgs(GroupingParameters parms)
        {
            this.parameters = parms;
        }

        public IList<OLVGroup> Groups
        {
            get
            {
                return this.groups;
            }
            set
            {
                this.groups = value;
            }
        }

        public GroupingParameters Parameters
        {
            get
            {
                return this.parameters;
            }
        }
    }
}

