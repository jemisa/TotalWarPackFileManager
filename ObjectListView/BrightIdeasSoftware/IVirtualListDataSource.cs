namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    public interface IVirtualListDataSource
    {
        void AddObjects(ICollection modelObjects);
        object GetNthObject(int n);
        int GetObjectCount();
        int GetObjectIndex(object model);
        void PrepareCache(int first, int last);
        void RemoveObjects(ICollection modelObjects);
        int SearchText(string value, int first, int last, OLVColumn column);
        void SetObjects(IEnumerable collection);
        void Sort(OLVColumn column, SortOrder order);
    }
}

