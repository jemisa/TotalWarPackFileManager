namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;

    public class AbstractVirtualGroups : IVirtualGroups
    {
        public virtual void CacheHint(int fromGroupIndex, int fromIndex, int toGroupIndex, int toIndex)
        {
        }

        public virtual int GetGroup(int itemIndex)
        {
            return -1;
        }

        public virtual int GetGroupMember(OLVGroup group, int indexWithinGroup)
        {
            return -1;
        }

        public virtual IList<OLVGroup> GetGroups(GroupingParameters parameters)
        {
            return new List<OLVGroup>();
        }

        public virtual int GetIndexWithinGroup(OLVGroup group, int itemIndex)
        {
            return -1;
        }
    }
}

