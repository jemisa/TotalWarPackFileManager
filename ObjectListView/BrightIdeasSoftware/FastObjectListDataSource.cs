namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class FastObjectListDataSource : AbstractVirtualListDataSource
    {
        private ArrayList objectList;
        private Dictionary<object, int> objectsToIndexMap;

        public FastObjectListDataSource(FastObjectListView listView) : base(listView)
        {
            this.objectList = new ArrayList();
            this.objectsToIndexMap = new Dictionary<object, int>();
        }

        public override void AddObjects(ICollection modelObjects)
        {
            foreach (object obj2 in modelObjects)
            {
                if (obj2 != null)
                {
                    this.objectList.Add(obj2);
                }
            }
            this.RebuildIndexMap();
        }

        public override object GetNthObject(int n)
        {
            return this.objectList[n];
        }

        public override int GetObjectCount()
        {
            return this.objectList.Count;
        }

        public override int GetObjectIndex(object model)
        {
            int num;
            if ((model != null) && this.objectsToIndexMap.TryGetValue(model, out num))
            {
                return num;
            }
            return -1;
        }

        protected void RebuildIndexMap()
        {
            this.objectsToIndexMap.Clear();
            for (int i = 0; i < this.objectList.Count; i++)
            {
                this.objectsToIndexMap[this.objectList[i]] = i;
            }
        }

        public override void RemoveObjects(ICollection modelObjects)
        {
            List<int> list = new List<int>();
            foreach (object obj2 in modelObjects)
            {
                int objectIndex = this.GetObjectIndex(obj2);
                if (objectIndex >= 0)
                {
                    list.Add(objectIndex);
                }
            }
            list.Sort();
            list.Reverse();
            foreach (int num in list)
            {
                base.listView.SelectedIndices.Remove(num);
            }
            foreach (int num in list)
            {
                this.objectList.RemoveAt(num);
            }
            this.RebuildIndexMap();
        }

        public override int SearchText(string value, int first, int last, OLVColumn column)
        {
            return AbstractVirtualListDataSource.DefaultSearchText(value, first, last, column, this);
        }

        public override void SetObjects(IEnumerable collection)
        {
            ArrayList list = new ArrayList();
            if (collection != null)
            {
                if (collection is ICollection)
                {
                    list = new ArrayList((ICollection) collection);
                }
                else
                {
                    foreach (object obj2 in collection)
                    {
                        list.Add(obj2);
                    }
                }
            }
            this.objectList = list;
            this.RebuildIndexMap();
        }

        public override void Sort(OLVColumn column, SortOrder sortOrder)
        {
            if (sortOrder != SortOrder.None)
            {
                this.objectList.Sort(new ModelObjectComparer(column, sortOrder, base.listView.SecondarySortColumn, base.listView.SecondarySortOrder));
            }
            this.RebuildIndexMap();
        }

        internal ArrayList ObjectList
        {
            get
            {
                return this.objectList;
            }
        }
    }
}

