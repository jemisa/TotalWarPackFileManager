namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public static class Generator
    {
        public static IList<OLVColumn> GenerateColumns(Type type)
        {
            List<OLVColumn> list = new List<OLVColumn>();
            foreach (PropertyInfo info in type.GetProperties())
            {
                OLVColumnAttribute customAttribute = Attribute.GetCustomAttribute(info, typeof(OLVColumnAttribute)) as OLVColumnAttribute;
                if (customAttribute != null)
                {
                    list.Add(MakeColumnFromAttribute(info.Name, customAttribute, info.CanWrite));
                }
            }
            list.Sort((x, y) => x.DisplayIndex.CompareTo(y.DisplayIndex));
            return list;
        }

        public static void GenerateColumns(ObjectListView olv, IEnumerable enumerable)
        {
            foreach (object obj2 in enumerable)
            {
                GenerateColumns(olv, obj2.GetType());
                return;
            }
            ReplaceColumns(olv, new List<OLVColumn>());
        }

        public static void GenerateColumns(ObjectListView olv, Type type)
        {
            IList<OLVColumn> columns = GenerateColumns(type);
            ReplaceColumns(olv, columns);
        }

        private static OLVColumn MakeColumnFromAttribute(string aspectName, OLVColumnAttribute attr, bool editable)
        {
            string title = attr.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = aspectName;
            }
            OLVColumn column = new OLVColumn(title, aspectName) {
                AspectToStringFormat = attr.AspectToStringFormat,
                CheckBoxes = attr.CheckBoxes,
                DisplayIndex = attr.DisplayIndex,
                GroupWithItemCountFormat = attr.GroupWithItemCountFormat,
                GroupWithItemCountSingularFormat = attr.GroupWithItemCountSingularFormat,
                Hyperlink = attr.Hyperlink,
                ImageAspectName = attr.ImageAspectName
            };
            if (attr.IsEditableSet)
            {
                column.IsEditable = attr.IsEditable;
            }
            else
            {
                column.IsEditable = editable;
            }
            column.IsTileViewColumn = attr.IsTileViewColumn;
            column.UseInitialLetterForGroup = attr.UseInitialLetterForGroup;
            column.ToolTipText = attr.ToolTipText;
            column.IsVisible = attr.IsVisible;
            column.FillsFreeSpace = attr.FillsFreeSpace;
            if (attr.FreeSpaceProportion.HasValue)
            {
                column.FreeSpaceProportion = attr.FreeSpaceProportion.Value;
            }
            column.MaximumWidth = attr.MaximumWidth;
            column.MinimumWidth = attr.MinimumWidth;
            column.Width = attr.Width;
            column.Text = attr.Title;
            column.TextAlign = attr.TextAlign;
            column.Tag = attr.Tag;
            column.TriStateCheckBoxes = attr.TriStateCheckBoxes;
            if ((attr.GroupCutoffs != null) && (attr.GroupDescriptions != null))
            {
                column.MakeGroupies(attr.GroupCutoffs, attr.GroupDescriptions);
            }
            return column;
        }

        private static void ReplaceColumns(ObjectListView olv, IList<OLVColumn> columns)
        {
            olv.Clear();
            olv.AllColumns.Clear();
            if (columns.Count > 0)
            {
                olv.AllColumns.AddRange(columns);
                olv.RebuildColumns();
            }
        }
    }
}

