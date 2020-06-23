namespace BrightIdeasSoftware
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("6FC61F50-80E8-49b4-B200-3F38D3865ABD")]
    internal class OwnerDataCallbackImpl : IOwnerDataCallback
    {
        private VirtualObjectListView olv;

        public OwnerDataCallbackImpl(VirtualObjectListView olv)
        {
            this.olv = olv;
        }

        public void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex)
        {
            groupIndex = this.olv.GroupingStrategy.GetGroup(itemIndex);
        }

        public void GetItemGroupCount(int itemIndex, out int occurrenceCount)
        {
            occurrenceCount = 1;
        }

        public void GetItemInGroup(int groupIndex, int n, out int itemIndex)
        {
            itemIndex = this.olv.GroupingStrategy.GetGroupMember(this.olv.OLVGroups[groupIndex], n);
        }

        public void GetItemPosition(int i, out BrightIdeasSoftware.NativeMethods.POINT pt)
        {
            throw new NotSupportedException();
        }

        public void OnCacheHint(BrightIdeasSoftware.NativeMethods.LVITEMINDEX from, BrightIdeasSoftware.NativeMethods.LVITEMINDEX to)
        {
            this.olv.GroupingStrategy.CacheHint(from.iGroup, from.iItem, to.iGroup, to.iItem);
        }

        public void SetItemPosition(int t, BrightIdeasSoftware.NativeMethods.POINT pt)
        {
            throw new NotSupportedException();
        }
    }
}

