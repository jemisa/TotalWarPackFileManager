namespace BrightIdeasSoftware
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("44C09D56-8D3B-419D-A462-7B956B105B47"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOwnerDataCallback
    {
        void GetItemPosition(int i, out BrightIdeasSoftware.NativeMethods.POINT pt);
        void SetItemPosition(int t, BrightIdeasSoftware.NativeMethods.POINT pt);
        void GetItemInGroup(int groupIndex, int n, out int itemIndex);
        void GetItemGroup(int itemIndex, int occurrenceCount, out int groupIndex);
        void GetItemGroupCount(int itemIndex, out int occurrenceCount);
        void OnCacheHint(BrightIdeasSoftware.NativeMethods.LVITEMINDEX i, BrightIdeasSoftware.NativeMethods.LVITEMINDEX j);
    }
}

