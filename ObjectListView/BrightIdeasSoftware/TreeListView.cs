namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class TreeListView : VirtualObjectListView
    {
        private bool revealAfterExpand = true;
        protected Tree TreeModel;
        private BaseRenderer treeRenderer;
        private bool useWaitCursorWhenExpanding = true;

        public TreeListView()
        {
            this.TreeModel = new Tree(this);
            base.OwnerDraw = true;
            base.View = View.Details;
            this.DataSource = this.TreeModel;
            this.TreeColumnRenderer = new TreeRenderer();
            base.StateImageList = new ImageList();
        }

        public virtual void Collapse(object model)
        {
            if (this.GetItemCount() != 0)
            {
                IList selectedObjects = this.SelectedObjects;
                int startIndex = this.TreeModel.Collapse(model);
                if (startIndex >= 0)
                {
                    this.UpdateVirtualListSize();
                    this.SelectedObjects = selectedObjects;
                    base.RedrawItems(startIndex, this.GetItemCount() - 1, false);
                }
            }
        }

        public virtual void CollapseAll()
        {
            if (this.GetItemCount() != 0)
            {
                IList selectedObjects = this.SelectedObjects;
                int startIndex = this.TreeModel.CollapseAll();
                if (startIndex >= 0)
                {
                    this.UpdateVirtualListSize();
                    this.SelectedObjects = selectedObjects;
                    base.RedrawItems(startIndex, this.GetItemCount() - 1, false);
                }
            }
        }

        public virtual void DiscardAllState()
        {
            IEnumerable roots = this.Roots;
            CanExpandGetterDelegate canExpandGetter = this.CanExpandGetter;
            ChildrenGetterDelegate childrenGetter = this.ChildrenGetter;
            this.TreeModel = new Tree(this);
            this.DataSource = this.TreeModel;
            this.CanExpandGetter = canExpandGetter;
            this.ChildrenGetter = childrenGetter;
            this.Roots = roots;
        }

        public virtual void Expand(object model)
        {
            if (this.GetItemCount() != 0)
            {
                IList selectedObjects = this.SelectedObjects;
                int startIndex = this.TreeModel.Expand(model);
                if (startIndex >= 0)
                {
                    this.UpdateVirtualListSize();
                    this.SelectedObjects = selectedObjects;
                    base.RedrawItems(startIndex, this.GetItemCount() - 1, false);
                    if (this.RevealAfterExpand && (startIndex > 0))
                    {
                        base.BeginUpdate();
                        try
                        {
                            int countPerPage = BrightIdeasSoftware.NativeMethods.GetCountPerPage(this);
                            int visibleDescendentCount = this.TreeModel.GetVisibleDescendentCount(model);
                            if (visibleDescendentCount < countPerPage)
                            {
                                base.EnsureVisible(startIndex + visibleDescendentCount);
                            }
                            else
                            {
                                this.TopItemIndex = startIndex;
                            }
                        }
                        finally
                        {
                            base.EndUpdate();
                        }
                    }
                }
            }
        }

        public virtual void ExpandAll()
        {
            if (this.GetItemCount() != 0)
            {
                IList selectedObjects = this.SelectedObjects;
                int startIndex = this.TreeModel.ExpandAll();
                if (startIndex >= 0)
                {
                    this.UpdateVirtualListSize();
                    this.SelectedObjects = selectedObjects;
                    base.RedrawItems(startIndex, this.GetItemCount() - 1, false);
                }
            }
        }

        public virtual IEnumerable GetChildren(object model)
        {
            Branch branch = this.TreeModel.GetBranch(model);
            if (!((branch != null) && branch.CanExpand))
            {
                return new ArrayList();
            }
            return branch.Children;
        }

        public virtual object GetParent(object model)
        {
            Branch branch = this.TreeModel.GetBranch(model);
            if ((branch == null) || (branch.ParentBranch == null))
            {
                return null;
            }
            return branch.ParentBranch.Model;
        }

        public virtual bool IsExpanded(object model)
        {
            Branch branch = this.TreeModel.GetBranch(model);
            return ((branch != null) && branch.IsExpanded);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return ((((keyData & Keys.KeyCode) == Keys.Left) || ((keyData & Keys.KeyCode) == Keys.Right)) || base.IsInputKey(keyData));
        }

        public override OLVListItem MakeListViewItem(int itemIndex)
        {
            OLVListItem item = base.MakeListViewItem(itemIndex);
            Branch branch = this.TreeModel.GetBranch(item.RowObject);
            if (branch != null)
            {
                item.IndentCount = branch.Level - 1;
            }
            return item;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            OLVListItem focusedItem = base.FocusedItem as OLVListItem;
            if (focusedItem == null)
            {
                base.OnKeyDown(e);
                return;
            }
            object rowObject = focusedItem.RowObject;
            Branch branch = this.TreeModel.GetBranch(rowObject);
            switch (e.KeyCode)
            {
                case Keys.Left:
                    if (!branch.IsExpanded)
                    {
                        if ((branch.ParentBranch != null) && (branch.ParentBranch.Model != null))
                        {
                            this.SelectObject(branch.ParentBranch.Model, true);
                        }
                        break;
                    }
                    this.Collapse(rowObject);
                    break;

                case Keys.Right:
                    if (!branch.IsExpanded)
                    {
                        if (branch.CanExpand)
                        {
                            this.Expand(rowObject);
                        }
                    }
                    else if (branch.ChildBranches.Count > 0)
                    {
                        this.SelectObject(branch.ChildBranches[0].Model, true);
                    }
                    e.Handled = true;
                    goto Label_0110;

                default:
                    goto Label_0110;
            }
            e.Handled = true;
        Label_0110:
            base.OnKeyDown(e);
        }

        protected override bool ProcessLButtonDown(OlvListViewHitTestInfo hti)
        {
            if (hti.HitTestLocation == HitTestLocation.ExpandButton)
            {
                this.PossibleFinishCellEditing();
                this.ToggleExpansion(hti.RowObject);
                return true;
            }
            return base.ProcessLButtonDown(hti);
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
            else if (this.GetItemCount() != 0)
            {
                IList selectedObjects = this.SelectedObjects;
                int num = 0x7fffffff;
                foreach (object obj2 in modelObjects)
                {
                    if (obj2 != null)
                    {
                        int num2 = this.TreeModel.RebuildChildren(obj2);
                        if (num2 >= 0)
                        {
                            num = Math.Min(num, num2);
                        }
                    }
                }
                if (num < this.GetItemCount())
                {
                    this.UpdateVirtualListSize();
                    this.SelectedObjects = selectedObjects;
                    base.RedrawItems(num, this.GetItemCount() - 1, false);
                }
            }
        }

        public virtual void ToggleExpansion(object model)
        {
            if (this.IsExpanded(model))
            {
                this.Collapse(model);
            }
            else
            {
                this.Expand(model);
            }
        }

        protected override void WndProc(ref Message m)
        {
            int msg = m.Msg;
            base.WndProc(ref m);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual CanExpandGetterDelegate CanExpandGetter
        {
            get
            {
                return this.TreeModel.CanExpandGetter;
            }
            set
            {
                this.TreeModel.CanExpandGetter = value;
            }
        }

        [Browsable(false)]
        public override bool CanShowGroups
        {
            get
            {
                return false;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ChildrenGetterDelegate ChildrenGetter
        {
            get
            {
                return this.TreeModel.ChildrenGetter;
            }
            set
            {
                this.TreeModel.ChildrenGetter = value;
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(true), Description("Should a wait cursor be shown when a branch is being expaned?")]
        public bool RevealAfterExpand
        {
            get
            {
                return this.revealAfterExpand;
            }
            set
            {
                this.revealAfterExpand = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IEnumerable Roots
        {
            get
            {
                return this.TreeModel.RootObjects;
            }
            set
            {
                if (this.GetColumn(0).Renderer == null)
                {
                    this.GetColumn(0).Renderer = this.TreeColumnRenderer;
                }
                if (value == null)
                {
                    this.TreeModel.RootObjects = new ArrayList();
                }
                else
                {
                    this.TreeModel.RootObjects = value;
                }
                this.UpdateVirtualListSize();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual BaseRenderer TreeColumnRenderer
        {
            get
            {
                return this.treeRenderer;
            }
            set
            {
                this.treeRenderer = value;
                if (base.Columns.Count > 0)
                {
                    this.GetColumn(0).Renderer = value;
                }
            }
        }

        [Description("Should a wait cursor be shown when a branch is being expaned?"), DefaultValue(true), Category("Behavior - ObjectListView")]
        public virtual bool UseWaitCursorWhenExpanding
        {
            get
            {
                return this.useWaitCursorWhenExpanding;
            }
            set
            {
                this.useWaitCursorWhenExpanding = value;
            }
        }

        protected class Branch
        {
            private bool alreadyHasChildren = false;
            public List<TreeListView.Branch> ChildBranches = new List<TreeListView.Branch>();
            private BranchFlags flags;
            public bool IsExpanded = false;
            public object Model;
            public TreeListView.Branch ParentBranch;
            public BrightIdeasSoftware.TreeListView.Tree Tree;

            public Branch(TreeListView.Branch parent, BrightIdeasSoftware.TreeListView.Tree tree, object model)
            {
                this.ParentBranch = parent;
                this.Tree = tree;
                this.Model = model;
            }

            private void AddChild(object model)
            {
                TreeListView.Branch item = this.Tree.GetBranch(model);
                if (item == null)
                {
                    item = this.MakeBranch(model);
                }
                else
                {
                    item.ParentBranch = this;
                }
                this.ChildBranches.Add(item);
            }

            public virtual void ClearCachedInfo()
            {
                this.Children = new ArrayList();
                this.alreadyHasChildren = false;
            }

            public virtual void Collapse()
            {
                this.IsExpanded = false;
            }

            public virtual void Expand()
            {
                if (this.CanExpand)
                {
                    this.IsExpanded = true;
                    this.FetchChildren();
                }
            }

            public virtual void ExpandAll()
            {
                this.Expand();
                foreach (TreeListView.Branch branch in this.ChildBranches)
                {
                    branch.ExpandAll();
                }
            }

            public virtual void FetchChildren()
            {
                if (!this.alreadyHasChildren)
                {
                    this.alreadyHasChildren = true;
                    if (this.Tree.ChildrenGetter != null)
                    {
                        if (this.Tree.TreeView.UseWaitCursorWhenExpanding)
                        {
                            Cursor current = Cursor.Current;
                            try
                            {
                                Cursor.Current = Cursors.WaitCursor;
                                this.Children = this.Tree.ChildrenGetter(this.Model);
                            }
                            finally
                            {
                                Cursor.Current = current;
                            }
                        }
                        else
                        {
                            this.Children = this.Tree.ChildrenGetter(this.Model);
                        }
                    }
                }
            }

            public virtual IList Flatten()
            {
                ArrayList flatList = new ArrayList();
                if (this.IsExpanded)
                {
                    this.FlattenOnto(flatList);
                }
                return flatList;
            }

            public virtual void FlattenOnto(IList flatList)
            {
                foreach (TreeListView.Branch branch in this.ChildBranches)
                {
                    flatList.Add(branch.Model);
                    if (branch.IsExpanded)
                    {
                        branch.FlattenOnto(flatList);
                    }
                }
            }

            private TreeListView.Branch MakeBranch(object model)
            {
                TreeListView.Branch br = new TreeListView.Branch(this, this.Tree, model);
                this.Tree.RegisterBranch(br);
                return br;
            }

            private void PushAncestors(IList<TreeListView.Branch> list)
            {
                if (this.ParentBranch != null)
                {
                    this.ParentBranch.PushAncestors(list);
                    list.Add(this);
                }
            }

            public virtual void Sort(TreeListView.BranchComparer comparer)
            {
                if (this.ChildBranches.Count != 0)
                {
                    this.ChildBranches[this.ChildBranches.Count - 1].IsLastChild = false;
                    if (comparer != null)
                    {
                        this.ChildBranches.Sort(comparer);
                    }
                    this.ChildBranches[this.ChildBranches.Count - 1].IsLastChild = true;
                    foreach (TreeListView.Branch branch in this.ChildBranches)
                    {
                        branch.Sort(comparer);
                    }
                }
            }

            public virtual IList<TreeListView.Branch> Ancestors
            {
                get
                {
                    List<TreeListView.Branch> list = new List<TreeListView.Branch>();
                    if (this.ParentBranch != null)
                    {
                        this.ParentBranch.PushAncestors(list);
                    }
                    return list;
                }
            }

            public virtual bool CanExpand
            {
                get
                {
                    if ((this.Tree.CanExpandGetter == null) || (this.Model == null))
                    {
                        return false;
                    }
                    return this.Tree.CanExpandGetter(this.Model);
                }
            }

            public virtual IEnumerable Children
            {
                get
                {
                    ArrayList list = new ArrayList();
                    foreach (TreeListView.Branch branch in this.ChildBranches)
                    {
                        list.Add(branch.Model);
                    }
                    return list;
                }
                set
                {
                    if (this.ChildBranches.Count > 0)
                    {
                        this.ChildBranches[this.ChildBranches.Count - 1].IsLastChild = false;
                    }
                    this.ChildBranches.Clear();
                    foreach (object obj2 in value)
                    {
                        this.AddChild(obj2);
                    }
                    if (this.ChildBranches.Count > 0)
                    {
                        this.ChildBranches[this.ChildBranches.Count - 1].IsLastChild = true;
                    }
                }
            }

            public virtual bool IsFirstBranch
            {
                get
                {
                    return ((this.flags & BranchFlags.FirstBranch) != 0);
                }
                set
                {
                    if (value)
                    {
                        this.flags |= BranchFlags.FirstBranch;
                    }
                    else
                    {
                        this.flags &= ~BranchFlags.FirstBranch;
                    }
                }
            }

            public virtual bool IsLastChild
            {
                get
                {
                    return ((this.flags & BranchFlags.LastChild) != 0);
                }
                set
                {
                    if (value)
                    {
                        this.flags |= BranchFlags.LastChild;
                    }
                    else
                    {
                        this.flags &= ~BranchFlags.LastChild;
                    }
                }
            }

            public virtual bool IsOnlyBranch
            {
                get
                {
                    return ((this.flags & BranchFlags.OnlyBranch) != 0);
                }
                set
                {
                    if (value)
                    {
                        this.flags |= BranchFlags.OnlyBranch;
                    }
                    else
                    {
                        this.flags &= ~BranchFlags.OnlyBranch;
                    }
                }
            }

            public int Level
            {
                get
                {
                    if (this.ParentBranch == null)
                    {
                        return 0;
                    }
                    return (this.ParentBranch.Level + 1);
                }
            }

            public virtual int NumberVisibleDescendents
            {
                get
                {
                    if (!this.IsExpanded)
                    {
                        return 0;
                    }
                    int count = this.ChildBranches.Count;
                    foreach (TreeListView.Branch branch in this.ChildBranches)
                    {
                        count += branch.NumberVisibleDescendents;
                    }
                    return count;
                }
            }

            public virtual bool Visible
            {
                get
                {
                    return ((this.ParentBranch == null) || (this.ParentBranch.IsExpanded && this.ParentBranch.Visible));
                }
            }

            [Flags]
            public enum BranchFlags
            {
                FirstBranch = 1,
                LastChild = 2,
                OnlyBranch = 4
            }
        }

        protected class BranchComparer : IComparer<TreeListView.Branch>
        {
            private IComparer actualComparer;

            public BranchComparer(IComparer actualComparer)
            {
                this.actualComparer = actualComparer;
            }

            public int Compare(TreeListView.Branch x, TreeListView.Branch y)
            {
                return this.actualComparer.Compare(x.Model, y.Model);
            }
        }

        public delegate bool CanExpandGetterDelegate(object model);

        public delegate IEnumerable ChildrenGetterDelegate(object model);

        protected class Tree : IVirtualListDataSource
        {
            private TreeListView.CanExpandGetterDelegate canExpandGetter;
            private TreeListView.ChildrenGetterDelegate childrenGetter;
            private OLVColumn lastSortColumn;
            private SortOrder lastSortOrder;
            private Dictionary<object, TreeListView.Branch> mapObjectToBranch = new Dictionary<object, TreeListView.Branch>();
            private Dictionary<object, int> mapObjectToIndex = new Dictionary<object, int>();
            private ArrayList objectList = new ArrayList();
            private TreeListView treeView;
            private TreeListView.Branch trunk;

            public Tree(TreeListView treeView)
            {
                this.treeView = treeView;
                this.trunk = new TreeListView.Branch(null, this, null);
                this.trunk.IsExpanded = true;
            }

            public virtual void AddObjects(ICollection modelObjects)
            {
                ArrayList collection = new ArrayList();
                foreach (object obj2 in this.treeView.Roots)
                {
                    collection.Add(obj2);
                }
                foreach (object obj2 in modelObjects)
                {
                    collection.Add(obj2);
                }
                this.SetObjects(collection);
            }

            public virtual int Collapse(object model)
            {
                TreeListView.Branch branch = this.GetBranch(model);
                if (!(((branch != null) && branch.IsExpanded) && branch.Visible))
                {
                    return -1;
                }
                int numberVisibleDescendents = branch.NumberVisibleDescendents;
                branch.Collapse();
                int objectIndex = this.GetObjectIndex(model);
                this.objectList.RemoveRange(objectIndex + 1, numberVisibleDescendents);
                this.RebuildObjectMap(objectIndex + 1);
                return objectIndex;
            }

            public virtual int CollapseAll()
            {
                foreach (TreeListView.Branch branch in this.trunk.ChildBranches)
                {
                    if (branch.IsExpanded)
                    {
                        branch.Collapse();
                    }
                }
                this.RebuildList();
                return 0;
            }

            public virtual int Expand(object model)
            {
                TreeListView.Branch br = this.GetBranch(model);
                if (!((((br != null) && br.CanExpand) && !br.IsExpanded) && br.Visible))
                {
                    return -1;
                }
                int objectIndex = this.GetObjectIndex(model);
                this.InsertChildren(br, objectIndex + 1);
                return objectIndex;
            }

            public virtual int ExpandAll()
            {
                this.trunk.ExpandAll();
                this.Sort(this.lastSortColumn, this.lastSortOrder);
                return 0;
            }

            public virtual TreeListView.Branch GetBranch(object model)
            {
                TreeListView.Branch branch;
                if ((model != null) && this.mapObjectToBranch.TryGetValue(model, out branch))
                {
                    return branch;
                }
                return null;
            }

            protected virtual TreeListView.BranchComparer GetBranchComparer()
            {
                if (this.lastSortColumn == null)
                {
                    return null;
                }
                return new TreeListView.BranchComparer(new ModelObjectComparer(this.lastSortColumn, this.lastSortOrder, this.treeView.GetColumn(0), this.lastSortOrder));
            }

            public virtual object GetNthObject(int n)
            {
                return this.objectList[n];
            }

            public virtual int GetObjectCount()
            {
                return this.trunk.NumberVisibleDescendents;
            }

            public virtual int GetObjectIndex(object model)
            {
                int num;
                if ((model != null) && this.mapObjectToIndex.TryGetValue(model, out num))
                {
                    return num;
                }
                return -1;
            }

            public virtual int GetVisibleDescendentCount(object model)
            {
                TreeListView.Branch branch = this.GetBranch(model);
                if (!((branch != null) && branch.IsExpanded))
                {
                    return 0;
                }
                return branch.NumberVisibleDescendents;
            }

            protected virtual void InsertChildren(TreeListView.Branch br, int idx)
            {
                br.Expand();
                br.Sort(this.GetBranchComparer());
                this.objectList.InsertRange(idx, br.Flatten());
                this.RebuildObjectMap(idx);
            }

            public virtual void PrepareCache(int first, int last)
            {
            }

            public virtual int RebuildChildren(object model)
            {
                TreeListView.Branch br = this.GetBranch(model);
                if (!((br != null) && br.Visible))
                {
                    return -1;
                }
                int numberVisibleDescendents = br.NumberVisibleDescendents;
                br.ClearCachedInfo();
                int objectIndex = this.GetObjectIndex(model);
                if (numberVisibleDescendents > 0)
                {
                    this.objectList.RemoveRange(objectIndex + 1, numberVisibleDescendents);
                }
                if (br.IsExpanded)
                {
                    this.InsertChildren(br, objectIndex + 1);
                }
                return objectIndex;
            }

            protected virtual void RebuildList()
            {
                this.objectList = ArrayList.Adapter(this.trunk.Flatten());
                if (this.trunk.ChildBranches.Count > 0)
                {
                    this.trunk.ChildBranches[0].IsFirstBranch = true;
                    this.trunk.ChildBranches[0].IsOnlyBranch = this.trunk.ChildBranches.Count == 1;
                }
                this.RebuildObjectMap(0);
            }

            protected virtual void RebuildObjectMap(int startIndex)
            {
                for (int i = startIndex; i < this.objectList.Count; i++)
                {
                    this.mapObjectToIndex[this.objectList[i]] = i;
                }
            }

            public virtual void RegisterBranch(TreeListView.Branch br)
            {
                this.mapObjectToBranch[br.Model] = br;
            }

            public virtual void RemoveObjects(ICollection modelObjects)
            {
                ArrayList collection = new ArrayList();
                foreach (object obj2 in this.treeView.Roots)
                {
                    collection.Add(obj2);
                }
                foreach (object obj2 in modelObjects)
                {
                    collection.Remove(obj2);
                }
                this.SetObjects(collection);
            }

            public virtual int SearchText(string value, int first, int last, OLVColumn column)
            {
                return AbstractVirtualListDataSource.DefaultSearchText(value, first, last, column, this);
            }

            public virtual void SetObjects(IEnumerable collection)
            {
                this.treeView.Roots = collection;
            }

            public virtual void Sort(OLVColumn column, SortOrder order)
            {
                this.lastSortColumn = column;
                this.lastSortOrder = order;
                if (this.trunk.ChildBranches.Count > 0)
                {
                    this.trunk.ChildBranches[0].IsFirstBranch = false;
                }
                this.trunk.Sort(this.GetBranchComparer());
                this.RebuildList();
            }

            public TreeListView.CanExpandGetterDelegate CanExpandGetter
            {
                get
                {
                    return this.canExpandGetter;
                }
                set
                {
                    this.canExpandGetter = value;
                }
            }

            public TreeListView.ChildrenGetterDelegate ChildrenGetter
            {
                get
                {
                    return this.childrenGetter;
                }
                set
                {
                    this.childrenGetter = value;
                }
            }

            public IEnumerable RootObjects
            {
                get
                {
                    return this.trunk.Children;
                }
                set
                {
                    this.trunk.Children = value;
                    this.RebuildList();
                }
            }

            public TreeListView TreeView
            {
                get
                {
                    return this.treeView;
                }
            }
        }

        public class TreeRenderer : BaseRenderer
        {
            public bool IsShowLines = true;
            private Pen linePen;
            public static int PIXELS_PER_LEVEL = 0x11;

            public TreeRenderer()
            {
                this.LinePen = new Pen(Color.Blue, 1f);
                this.LinePen.DashStyle = DashStyle.Dot;
            }

            private void DrawLines(Graphics g, Rectangle r, Pen p, BrightIdeasSoftware.TreeListView.Branch br)
            {
                int num2;
                Rectangle rectangle = r;
                rectangle.Width = PIXELS_PER_LEVEL;
                int top = rectangle.Top;
                if ((p.DashStyle == DashStyle.Dot) && ((top & 1) == 1))
                {
                    top++;
                }
                IList<BrightIdeasSoftware.TreeListView.Branch> ancestors = br.Ancestors;
                foreach (BrightIdeasSoftware.TreeListView.Branch branch in ancestors)
                {
                    if (!branch.IsLastChild)
                    {
                        num2 = rectangle.Left + (rectangle.Width / 2);
                        g.DrawLine(p, num2, top, num2, rectangle.Bottom);
                    }
                    rectangle.Offset(PIXELS_PER_LEVEL, 0);
                }
                num2 = rectangle.Left + (rectangle.Width / 2);
                int num3 = rectangle.Top + (rectangle.Height / 2);
                g.DrawLine(p, num2, num3, rectangle.Right, num3);
                if (br.IsFirstBranch)
                {
                    if (!br.IsOnlyBranch)
                    {
                        g.DrawLine(p, num2, num3, num2, rectangle.Bottom);
                    }
                }
                else if (br.IsLastChild)
                {
                    g.DrawLine(p, num2, top, num2, num3);
                }
                else
                {
                    g.DrawLine(p, num2, top, num2, rectangle.Bottom);
                }
            }

            protected override Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex)
            {
                return base.StandardGetEditRectangle(g, cellBounds);
            }

            protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
            {
                BrightIdeasSoftware.TreeListView.Branch branch = this.Branch;
                Rectangle bounds = base.Bounds;
                if (branch.CanExpand)
                {
                    bounds.Offset((branch.Level - 1) * PIXELS_PER_LEVEL, 0);
                    bounds.Width = PIXELS_PER_LEVEL;
                    if (bounds.Contains(x, y))
                    {
                        hti.HitTestLocation = HitTestLocation.ExpandButton;
                        return;
                    }
                }
                bounds = base.Bounds;
                int num = branch.Level * PIXELS_PER_LEVEL;
                bounds.X += num;
                bounds.Width -= num;
                if (x < bounds.Left)
                {
                    hti.HitTestLocation = HitTestLocation.Nothing;
                }
                else
                {
                    base.StandardHitTest(g, hti, bounds, x, y);
                }
            }

            public override void Render(Graphics g, Rectangle r)
            {
                this.DrawBackground(g, r);
                BrightIdeasSoftware.TreeListView.Branch br = this.Branch;
                if (this.IsShowLines)
                {
                    this.DrawLines(g, r, this.LinePen, br);
                }
                if (br.CanExpand)
                {
                    Rectangle bounds = r;
                    bounds.Offset((br.Level - 1) * PIXELS_PER_LEVEL, 0);
                    bounds.Width = PIXELS_PER_LEVEL;
                    if (!base.IsPrinting && Application.RenderWithVisualStyles)
                    {
                        VisualStyleElement closed = VisualStyleElement.TreeView.Glyph.Closed;
                        if (br.IsExpanded)
                        {
                            closed = VisualStyleElement.TreeView.Glyph.Opened;
                        }
                        new VisualStyleRenderer(closed).DrawBackground(g, bounds);
                    }
                    else
                    {
                        int height = 8;
                        int width = 8;
                        int num3 = bounds.X + 4;
                        int y = (bounds.Y + (bounds.Height / 2)) - 4;
                        g.DrawRectangle(new Pen(SystemBrushes.ControlDark), num3, y, width, height);
                        g.FillRectangle(Brushes.White, (int) (num3 + 1), (int) (y + 1), (int) (width - 1), (int) (height - 1));
                        g.DrawLine(Pens.Black, (int) (num3 + 2), (int) (y + 4), (int) ((num3 + width) - 2), (int) (y + 4));
                        if (!br.IsExpanded)
                        {
                            g.DrawLine(Pens.Black, (int) (num3 + 4), (int) (y + 2), (int) (num3 + 4), (int) ((y + height) - 2));
                        }
                    }
                }
                int x = br.Level * PIXELS_PER_LEVEL;
                r.Offset(x, 0);
                r.Width -= x;
                this.DrawImageAndText(g, r);
            }

            private BrightIdeasSoftware.TreeListView.Branch Branch
            {
                get
                {
                    return this.TreeListView.TreeModel.GetBranch(base.RowObject);
                }
            }

            public Pen LinePen
            {
                get
                {
                    return this.linePen;
                }
                set
                {
                    this.linePen = value;
                }
            }

            public BrightIdeasSoftware.TreeListView TreeListView
            {
                get
                {
                    return (BrightIdeasSoftware.TreeListView) base.ListView;
                }
            }
        }
    }
}

