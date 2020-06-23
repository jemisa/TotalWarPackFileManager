namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;

    public interface IVirtualGroups
    {
        void CacheHint(int fromGroupIndex, int fromIndex, int toGroupIndex, int toIndex);
        int GetGroup(int itemIndex);
        int GetGroupMember(OLVGroup group, int indexWithinGroup);
        IList<OLVGroup> GetGroups(GroupingParameters parameters);
        int GetIndexWithinGroup(OLVGroup group, int itemIndex);
    }
}

