namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    public class AbstractVirtualListDataSource : IVirtualListDataSource
    {
        protected VirtualObjectListView listView;

        public AbstractVirtualListDataSource(VirtualObjectListView listView)
        {
            this.listView = listView;
        }

        public virtual void AddObjects(ICollection modelObjects)
        {
        }

        public static int DefaultSearchText(string value, int first, int last, OLVColumn column, IVirtualListDataSource source)
        {
            int num;
            if (first <= last)
            {
                for (num = first; num <= last; num++)
                {
                    if (column.GetStringValue(source.GetNthObject(num)).StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return num;
                    }
                }
            }
            else
            {
                for (num = first; num >= last; num--)
                {
                    if (column.GetStringValue(source.GetNthObject(num)).StartsWith(value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return num;
                    }
                }
            }
            return -1;
        }

        public virtual object GetNthObject(int n)
        {
            return null;
        }

        public virtual int GetObjectCount()
        {
            return -1;
        }

        public virtual int GetObjectIndex(object model)
        {
            return -1;
        }

        public virtual void PrepareCache(int from, int to)
        {
        }

        public virtual void RemoveObjects(ICollection modelObjects)
        {
        }

        public virtual int SearchText(string value, int first, int last, OLVColumn column)
        {
            return -1;
        }

        public virtual void SetObjects(IEnumerable collection)
        {
        }

        public virtual void Sort(OLVColumn column, SortOrder order)
        {
        }
    }
}

