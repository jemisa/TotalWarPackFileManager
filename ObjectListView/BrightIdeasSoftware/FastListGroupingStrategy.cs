namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;

    public class FastListGroupingStrategy : AbstractVirtualGroups
    {
        private List<int> indexToGroupMap;

        public override int GetGroup(int itemIndex)
        {
            return this.indexToGroupMap[itemIndex];
        }

        public override int GetGroupMember(OLVGroup group, int indexWithinGroup)
        {
            return (int) group.Contents[indexWithinGroup];
        }

        public override IList<OLVGroup> GetGroups(GroupingParameters parms)
        {
            Converter<object, int> converter = null;
            FastObjectListView folv = (FastObjectListView) parms.ListView;
            int capacity = 0;
            NullableDictionary<object, List<object>> dictionary = new NullableDictionary<object, List<object>>();
            foreach (object obj2 in folv.Objects)
            {
                object groupKey = parms.GroupByColumn.GetGroupKey(obj2);
                if (!dictionary.ContainsKey(groupKey))
                {
                    dictionary[groupKey] = new List<object>();
                }
                dictionary[groupKey].Add(obj2);
                capacity++;
            }
            OLVColumn col = parms.SortItemsByPrimaryColumn ? parms.ListView.GetColumn(0) : parms.PrimarySort;
            ModelObjectComparer comparer = new ModelObjectComparer(col, parms.PrimarySortOrder, parms.SecondarySort, parms.SecondarySortOrder);
            foreach (object obj3 in dictionary.Keys)
            {
                dictionary[obj3].Sort(comparer);
            }
            List<OLVGroup> list = new List<OLVGroup>();
            foreach (object obj3 in dictionary.Keys)
            {
                string str = parms.GroupByColumn.ConvertGroupKeyToTitle(obj3);
                if (!string.IsNullOrEmpty(parms.TitleFormat))
                {
                    int count = dictionary[obj3].Count;
                    str = string.Format((count == 1) ? parms.TitleSingularFormat : parms.TitleFormat, str, count);
                }
                OLVGroup group = new OLVGroup(str) {
                    Key = obj3,
                    SortValue = obj3 as IComparable
                };
                if (converter == null)
                {
                    converter = x => folv.IndexOf(x);
                }
                group.Contents = dictionary[obj3].ConvertAll<int>(converter);
                group.VirtualItemCount = dictionary[obj3].Count;
                if (parms.GroupByColumn.GroupFormatter != null)
                {
                    parms.GroupByColumn.GroupFormatter(group, parms);
                }
                list.Add(group);
            }
            list.Sort(new OLVGroupComparer(parms.PrimarySortOrder));
            this.indexToGroupMap = new List<int>(capacity);
            this.indexToGroupMap.AddRange(new int[capacity]);
            for (int i = 0; i < list.Count; i++)
            {
                OLVGroup group2 = list[i];
                List<int> contents = (List<int>) group2.Contents;
                foreach (int num4 in contents)
                {
                    this.indexToGroupMap[num4] = i;
                }
            }
            return list;
        }

        public override int GetIndexWithinGroup(OLVGroup group, int itemIndex)
        {
            return group.Contents.IndexOf(itemIndex);
        }
    }
}

