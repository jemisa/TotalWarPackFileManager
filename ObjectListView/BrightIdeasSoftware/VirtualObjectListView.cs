namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;

    public class VirtualObjectListView : ObjectListView
    {
        private Dictionary<object, CheckState> checkStateMap = new Dictionary<object, CheckState>();
        private IVirtualListDataSource dataSource;
        private IVirtualGroups groupingStrategy;
        private OLVListItem lastRetrieveVirtualItem;
        private int lastRetrieveVirtualItemIndex = -1;
        private OwnerDataCallbackImpl ownerDataCallbackImpl;
        private bool showGroups;

        public VirtualObjectListView()
        {
            base.VirtualMode = true;
            base.CacheVirtualItems += new CacheVirtualItemsEventHandler(this.HandleCacheVirtualItems);
            base.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this.HandleRetrieveVirtualItem);
            base.SearchForVirtualItem += new SearchForVirtualItemEventHandler(this.HandleSearchForVirtualItem);
            this.DataSource = new VirtualListVersion1DataSource(this);
        }

        public override void AddObjects(ICollection modelObjects)
        {
            if (this.DataSource != null)
            {
                ItemsAddingEventArgs e = new ItemsAddingEventArgs(modelObjects);
                this.OnItemsAdding(e);
                if (!e.Canceled)
                {
                    this.ClearCachedInfo();
                    this.DataSource.AddObjects(e.ObjectsToAdd);
                    base.Sort();
                    this.UpdateVirtualListSize();
                }
            }
        }

        public override void BuildList(bool shouldPreserveSelection)
        {
            this.UpdateVirtualListSize();
            this.ClearCachedInfo();
            if (this.ShowGroups)
            {
                this.BuildGroups();
            }
            base.Invalidate();
        }

        public virtual void ClearCachedInfo()
        {
            this.lastRetrieveVirtualItemIndex = -1;
        }

        public override void ClearObjects()
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new MethodInvoker(this.ClearObjects));
            }
            else
            {
                this.SetObjects(new ArrayList());
            }
        }

        protected override void CreateGroups(IList<OLVGroup> groups)
        {
            BrightIdeasSoftware.NativeMethods.ClearGroups(this);
            this.EnableVirtualGroups();
            foreach (OLVGroup group in groups)
            {
                Debug.Assert(group.Items.Count == 0, "Groups in virtual lists cannot set Items. Use VirtualItemCount instead.");
                Debug.Assert(group.VirtualItemCount > 0, "VirtualItemCount must be greater than 0.");
                group.InsertGroupNewStyle(this);
            }
        }

        protected void DisableVirtualGroups()
        {
            Debug.WriteLine(BrightIdeasSoftware.NativeMethods.ClearGroups(this));
            Debug.WriteLine(BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x109d, 0, 0));
            Debug.WriteLine(BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x10bb, 0, 0));
        }

        protected void EnableVirtualGroups()
        {
            if (this.ownerDataCallbackImpl == null)
            {
                this.ownerDataCallbackImpl = new OwnerDataCallbackImpl(this);
            }
            IntPtr comInterfaceForObject = Marshal.GetComInterfaceForObject(this.ownerDataCallbackImpl, typeof(IOwnerDataCallback));
            IntPtr ptr2 = BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x10bb, comInterfaceForObject, 0);
            Marshal.Release(comInterfaceForObject);
            Debug.WriteLine(BrightIdeasSoftware.NativeMethods.SendMessage(base.Handle, 0x109d, 1, 0));
        }

        protected override int FindMatchInRange(string text, int first, int last, OLVColumn column)
        {
            return this.DataSource.SearchText(text, first, last, column);
        }

        protected override CheckState? GetCheckState(object modelObject)
        {
            if (this.CheckStateGetter != null)
            {
                return base.GetCheckState(modelObject);
            }
            CheckState @unchecked = CheckState.Unchecked;
            if (modelObject != null)
            {
                this.checkStateMap.TryGetValue(modelObject, out @unchecked);
            }
            return new CheckState?(@unchecked);
        }

        public override int GetItemCount()
        {
            return base.VirtualListSize;
        }

        public virtual int GetItemIndexInDisplayOrder(int itemIndex)
        {
            if (!this.ShowGroups)
            {
                return itemIndex;
            }
            int group = this.GroupingStrategy.GetGroup(itemIndex);
            int num2 = 0;
            for (int i = 0; i < (group - 1); i++)
            {
                num2 += base.OLVGroups[i].VirtualItemCount;
            }
            return (num2 + this.GroupingStrategy.GetIndexWithinGroup(base.OLVGroups[group], itemIndex));
        }

        public override object GetModelObject(int index)
        {
            if ((this.DataSource != null) && (index >= 0))
            {
                return this.DataSource.GetNthObject(index);
            }
            return null;
        }

        protected virtual void HandleCacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if (this.DataSource != null)
            {
                this.DataSource.PrepareCache(e.StartIndex, e.EndIndex);
            }
        }

        protected override void HandleColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (this.PossibleFinishCellEditing())
            {
                SortOrder ascending = SortOrder.Ascending;
                if ((this.LastSortColumn != null) && (e.Column == this.LastSortColumn.Index))
                {
                    ascending = (this.LastSortOrder == SortOrder.Descending) ? SortOrder.Ascending : SortOrder.Descending;
                }
                base.BeginUpdate();
                try
                {
                    this.Sort(this.GetColumn(e.Column), ascending);
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        protected virtual void HandleRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (this.lastRetrieveVirtualItemIndex != e.ItemIndex)
            {
                this.lastRetrieveVirtualItemIndex = e.ItemIndex;
                this.lastRetrieveVirtualItem = this.MakeListViewItem(e.ItemIndex);
            }
            e.Item = this.lastRetrieveVirtualItem;
        }

        protected virtual void HandleSearchForVirtualItem(object sender, SearchForVirtualItemEventArgs e)
        {
            if (this.DataSource != null)
            {
                int startSearchFrom = Math.Min(e.StartIndex, this.DataSource.GetObjectCount() - 1);
                BeforeSearchingEventArgs args = new BeforeSearchingEventArgs(e.Text, startSearchFrom);
                this.OnBeforeSearching(args);
                if (!args.Canceled)
                {
                    int indexSelected = this.FindMatchingRow(args.StringToFind, args.StartSearchFrom, e.Direction);
                    AfterSearchingEventArgs args2 = new AfterSearchingEventArgs(args.StringToFind, indexSelected);
                    this.OnAfterSearching(args2);
                    if (indexSelected != -1)
                    {
                        e.Index = indexSelected;
                    }
                }
            }
        }

        public override int IndexOf(object modelObject)
        {
            if ((this.DataSource == null) || (modelObject == null))
            {
                return -1;
            }
            return this.DataSource.GetObjectIndex(modelObject);
        }

        protected override IList<OLVGroup> MakeGroups(GroupingParameters parms)
        {
            if (this.GroupingStrategy == null)
            {
                return new List<OLVGroup>();
            }
            return this.GroupingStrategy.GetGroups(parms);
        }

        public virtual OLVListItem MakeListViewItem(int itemIndex)
        {
            OLVListItem lvi = new OLVListItem(this.GetModelObject(itemIndex));
            this.FillInValues(lvi, lvi.RowObject);
            this.PostProcessOneRow(itemIndex, this.GetItemIndexInDisplayOrder(itemIndex), lvi);
            if (this.HotRowIndex == itemIndex)
            {
                this.UpdateHotRow(lvi);
            }
            return lvi;
        }

        public override OLVListItem ModelToItem(object modelObject)
        {
            if ((this.DataSource != null) && (modelObject != null))
            {
                int objectIndex = this.DataSource.GetObjectIndex(modelObject);
                if (objectIndex >= 0)
                {
                    return this.GetItem(objectIndex);
                }
            }
            return null;
        }

        protected override void PostProcessRows()
        {
        }

        protected override void PrepareAlternateBackColors()
        {
        }

        protected override CheckState PutCheckState(object modelObject, CheckState state)
        {
            state = base.PutCheckState(modelObject, state);
            this.checkStateMap[modelObject] = state;
            return state;
        }

        public override void RefreshItem(OLVListItem olvi)
        {
            this.ClearCachedInfo();
            base.RedrawItems(olvi.Index, olvi.Index, false);
        }

        public override void RefreshObjects(IList modelObjects)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.RefreshObjects(modelObjects);
                    };
                }
                base.Invoke(method);
            }
            else if (this.DataSource != null)
            {
                this.ClearCachedInfo();
                foreach (object obj2 in modelObjects)
                {
                    int objectIndex = this.DataSource.GetObjectIndex(obj2);
                    if (objectIndex >= 0)
                    {
                        base.RedrawItems(objectIndex, objectIndex, true);
                    }
                }
            }
        }

        public override void RefreshSelectedObjects()
        {
            foreach (int num in base.SelectedIndices)
            {
                base.RedrawItems(num, num, true);
            }
        }

        public override void RemoveObjects(ICollection modelObjects)
        {
            if (this.DataSource != null)
            {
                ItemsRemovingEventArgs e = new ItemsRemovingEventArgs(modelObjects);
                this.OnItemsRemoving(e);
                if (!e.Canceled)
                {
                    this.ClearCachedInfo();
                    this.DataSource.RemoveObjects(e.ObjectsToRemove);
                    this.UpdateVirtualListSize();
                }
            }
        }

        public override void SelectObject(object modelObject, bool setFocus)
        {
            if (this.DataSource != null)
            {
                int objectIndex = this.DataSource.GetObjectIndex(modelObject);
                if (((objectIndex >= 0) && (objectIndex < base.VirtualListSize)) && ((base.SelectedIndices.Count != 1) || (base.SelectedIndices[0] != objectIndex)))
                {
                    base.SelectedIndices.Clear();
                    base.SelectedIndices.Add(objectIndex);
                    if (setFocus)
                    {
                        this.SelectedItem.Focused = true;
                    }
                }
            }
        }

        public override void SelectObjects(IList modelObjects)
        {
            if (this.DataSource != null)
            {
                base.SelectedIndices.Clear();
                if (modelObjects != null)
                {
                    foreach (object obj2 in modelObjects)
                    {
                        int objectIndex = this.DataSource.GetObjectIndex(obj2);
                        if ((objectIndex >= 0) && (objectIndex < base.VirtualListSize))
                        {
                            base.SelectedIndices.Add(objectIndex);
                        }
                    }
                }
            }
        }

        public override void SetObjects(IEnumerable collection)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.SetObjects(collection);
                    };
                }
                base.Invoke(method);
            }
            else if (this.DataSource != null)
            {
                base.BeginUpdate();
                try
                {
                    ItemsChangingEventArgs e = new ItemsChangingEventArgs(null, collection);
                    this.OnItemsChanging(e);
                    if (!e.Canceled)
                    {
                        this.DataSource.SetObjects(e.NewObjects);
                        this.UpdateVirtualListSize();
                        base.Sort();
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        protected virtual void SetVirtualListSize(int newSize)
        {
            if ((newSize >= 0) && (base.VirtualListSize != newSize))
            {
                int virtualListSize = base.VirtualListSize;
                this.ClearCachedInfo();
                try
                {
                    if ((newSize == 0) && (this.TopItemIndex > 0))
                    {
                        this.TopItemIndex = 0;
                    }
                }
                catch (Exception)
                {
                }
                try
                {
                    base.VirtualListSize = newSize;
                }
                catch (ArgumentOutOfRangeException)
                {
                }
                catch (NullReferenceException)
                {
                }
                this.OnItemsChanged(new ItemsChangedEventArgs(virtualListSize, base.VirtualListSize));
            }
        }

        protected override void TakeOwnershipOfObjects()
        {
        }

        public virtual void UpdateVirtualListSize()
        {
            if (this.DataSource != null)
            {
                this.SetVirtualListSize(this.DataSource.GetObjectCount());
            }
        }

        [Browsable(false)]
        public override bool CanShowGroups
        {
            get
            {
                return (ObjectListView.IsVista && (this.GroupingStrategy != null));
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override IList CheckedObjects
        {
            get
            {
                ArrayList list = new ArrayList();
                if (base.CheckBoxes)
                {
                    if (this.CheckStateGetter != null)
                    {
                        return base.CheckedObjects;
                    }
                    foreach (KeyValuePair<object, CheckState> pair in this.checkStateMap)
                    {
                        if (((CheckState) pair.Value) == CheckState.Checked)
                        {
                            list.Add(pair.Key);
                        }
                    }
                }
                return list;
            }
            set
            {
                if (base.CheckBoxes)
                {
                    if (value == null)
                    {
                        value = new ArrayList();
                    }
                    object[] array = new object[this.checkStateMap.Count];
                    this.checkStateMap.Keys.CopyTo(array, 0);
                    foreach (object obj2 in array)
                    {
                        if (value.Contains(obj2))
                        {
                            this.SetObjectCheckedness(obj2, CheckState.Checked);
                        }
                        else
                        {
                            this.SetObjectCheckedness(obj2, CheckState.Unchecked);
                        }
                    }
                    foreach (object obj3 in value)
                    {
                        this.SetObjectCheckedness(obj3, CheckState.Checked);
                    }
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IVirtualListDataSource DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;
                this.CustomSorter = delegate (OLVColumn column, SortOrder sortOrder) {
                    this.ClearCachedInfo();
                    this.dataSource.Sort(column, sortOrder);
                };
                this.UpdateVirtualListSize();
                base.Invalidate();
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVirtualGroups GroupingStrategy
        {
            get
            {
                return this.groupingStrategy;
            }
            set
            {
                this.groupingStrategy = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override IEnumerable Objects
        {
            get
            {
                for (int i = 0; i < this.GetItemCount(); i++)
                {
                    yield return this.GetModelObject(i);
                }
            }
            set
            {
                base.Objects = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual RowGetterDelegate RowGetter
        {
            get
            {
                return ((VirtualListVersion1DataSource) this.dataSource).RowGetter;
            }
            set
            {
                ((VirtualListVersion1DataSource) this.dataSource).RowGetter = value;
            }
        }

        [DefaultValue(true), Category("Appearance"), Description("Should the list view show items in groups?")]
        public override bool ShowGroups
        {
            get
            {
                return (ObjectListView.IsVista && this.showGroups);
            }
            set
            {
                this.showGroups = value;
                if (!(!base.Created || value))
                {
                    this.DisableVirtualGroups();
                }
            }
        }

/*        [CompilerGenerated]
        private sealed class <get_Objects>d__4f : IEnumerable<object>, IEnumerable, IEnumerator<object>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private object <>2__current;
            public VirtualObjectListView <>4__this;
            private int <>l__initialThreadId;
            public int <i>5__50;

            [DebuggerHidden]
            public <get_Objects>d__4f(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<i>5__50 = 0;
                        while (this.<i>5__50 < this.<>4__this.GetItemCount())
                        {
                            this.<>2__current = this.<>4__this.GetModelObject(this.<i>5__50);
                            this.<>1__state = 1;
                            return true;
                        Label_0050:
                            this.<>1__state = -1;
                            this.<i>5__50++;
                        }
                        break;

                    case 1:
                        goto Label_0050;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    return this;
                }
                return new VirtualObjectListView.<get_Objects>d__4f(0) { <>4__this = this.<>4__this };
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
        */
    }
}

