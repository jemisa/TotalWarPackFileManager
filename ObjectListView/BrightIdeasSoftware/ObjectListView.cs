namespace BrightIdeasSoftware
{
    using BrightIdeasSoftware.Design;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class ObjectListView : System.Windows.Forms.ListView, ISupportInitialize
    {
        private List<OLVColumn> allColumns = new List<OLVColumn>();
        private Color alternateRowBackColor = Color.Empty;
        private OLVColumn alwaysGroupByColumn;
        private SortOrder alwaysGroupBySortOrder = SortOrder.None;
        private CellEditActivateMode cellEditActivation = CellEditActivateMode.None;
        private CellEditEventArgs cellEditEventArgs;
        private Control cellEditor;
        private ToolTipControl cellToolTip;
        private CellToolTipGetterDelegate cellToolTipGetter;
        public const string CHECKED_KEY = "checkbox-checked";
        private Munger checkedAspectMunger;
        private string checkedAspectName;
        private CheckStateGetterDelegate checkStateGetter;
        private CheckStatePutterDelegate checkStatePutter;
        private SortDelegate customSorter;
        private List<IDecoration> decorations = new List<IDecoration>();
        private IRenderer defaultRenderer = new BaseRenderer();
        private IDragSource dragSource;
        private List<OLVListItem> drawnItems;
        private IDropSink dropSink;
        public static BrightIdeasSoftware.EditorRegistry EditorRegistry = new BrightIdeasSoftware.EditorRegistry();
        private IOverlay emptyListMsgOverlay;
        private int freezeCount;
        private List<GlassPanelForm> glassPanels = new List<GlassPanelForm>();
        private ImageList groupImageList;
        private string groupWithItemCountFormat;
        private string groupWithItemCountSingularFormat;
        private bool hasCollapsibleGroups = true;
        private bool hasIdleHandler;
        private BrightIdeasSoftware.HeaderControl headerControl;
        private Font headerFont;
        private HeaderToolTipGetterDelegate headerToolTipGetter;
        private bool headerWordWrap;
        private Color highlightBackgroundColor = Color.Empty;
        private Color highlightForegroundColor = Color.Empty;
        private HitTestLocation hotCellHitLocation;
        private int hotColumnIndex;
        private BrightIdeasSoftware.HotItemStyle hotItemStyle;
        private int hotRowIndex;
        private BrightIdeasSoftware.HyperlinkStyle hyperlinkStyle;
        private ImageOverlay imageOverlay;
        public const string INDETERMINATE_KEY = "checkbox-indeterminate";
        private bool isAfterItemPaint;
        private bool isInWmPaintEvent;
        private bool isMarqueSelecting;
        private bool isOwnerOfObjects;
        private bool isSearchOnSortColumn = true;
        private static bool? isVista;
        private IRenderer itemRenderer;
        private ToolStripMenuItem lastMenuItemClicked;
        private int lastMouseDownClickCount;
        private string lastSearchString;
        private Rectangle lastUpdateRectangle;
        private int lastValidatingEvent = 0;
        private string menuLabelGroupBy = "Group by '{0}'";
        private string menuLabelLockGroupingOn = "Lock grouping on '{0}'";
        private string menuLabelSortAscending = "Sort ascending by '{0}'";
        private string menuLabelSortDescending = "Sort descending by '{0}'";
        private string menuLabelTurnOffGroups = "Turn off groups";
        private string menuLabelUnlockGroupingOn = "Unlock grouping from '{0}'";
        private string menuLabelUnsort = "Unsort";
        private IEnumerable objects;
        private IList<OLVGroup> olvGroups;
        private List<IOverlay> overlays = new List<IOverlay>();
        private int overlayTransparency = 0x80;
        private bool ownerDrawnHeader;
        private int prePaintLevel;
        private OLVColumn primarySortColumn;
        private SortOrder primarySortOrder;
        private RowFormatterDelegate rowFormatter;
        private int rowHeight = -1;
        private OLVColumn secondarySortColumn;
        private SortOrder secondarySortOrder = SortOrder.None;
        private bool selectColumnsMenuStaysOpen = true;
        private bool selectColumnsOnRightClick = true;
        private OLVColumn selectedColumn;
        private TintedColumnDecoration selectedColumnDecoration = new TintedColumnDecoration();
        private Color selectedColumnTint = Color.Empty;
        private IDecoration selectedRowDecoration;
        private ImageList shadowedImageList;
        private bool shouldDoCustomDrawing;
        private bool showCommandMenuOnRightClick = false;
        private bool showHeaderInAllViews = true;
        private bool showImagesOnSubItems;
        private bool showItemCountOnGroups;
        private bool showSortIndicators;
        public const string SORT_INDICATOR_DOWN_KEY = "sort-indicator-down";
        public const string SORT_INDICATOR_UP_KEY = "sort-indicator-up";
        private bool sortGroupItemsByPrimaryColumn = true;
        private int spaceBetweenGroups;
        private TextOverlay textOverlay;
        private static TextRenderingHint textRendereringHint = TextRenderingHint.SystemDefault;
        private int timeLastCharEvent;
        private bool tintSortColumn;
        private bool triStateCheckBoxes;
        public const string UNCHECKED_KEY = "checkbox-unchecked";
        private Color unfocusedHighlightBackgroundColor = Color.Empty;
        private Color unfocusedHighlightForegroundColor = Color.Empty;
        private bool updateSpaceFillingColumnsWhenDraggingColumnDivider = true;
        private bool useAlternatingBackColors;
        private bool useCellFormatEvents;
        private bool useCustomSelectionColors;
        private bool useExplorerTheme;
        private bool useHotItem;
        private bool useHyperlinks;
        private bool useSubItemCheckBoxes;
        private bool useTranslucentHotItem;
        private bool useTranslucentSelection;
        private Dictionary<string, bool> visitedUrlMap = new Dictionary<string, bool>();

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the groups are just about to be created.")]
        public event EventHandler<CreateGroupsEventArgs> AboutToCreateGroups;

        [Description("This event is triggered after the groups are created."), Category("Behavior - ObjectListView")]
        public event EventHandler<CreateGroupsEventArgs> AfterCreatingGroups;

        [Description("This event is triggered after the control has done a search-by-typing action."), Category("Behavior - ObjectListView")]
        public event EventHandler<AfterSearchingEventArgs> AfterSearching;

        [Description("This event is triggered after the items in the list have been sorted."), Category("Behavior - ObjectListView")]
        public event EventHandler<AfterSortingEventArgs> AfterSorting;

        [Description("This event is triggered before the groups are created."), Category("Behavior - ObjectListView")]
        public event EventHandler<CreateGroupsEventArgs> BeforeCreatingGroups;

        [Category("Behavior - ObjectListView"), Description("This event is triggered before the control does a search-by-typing action.")]
        public event EventHandler<BeforeSearchingEventArgs> BeforeSearching;

        [Category("Behavior - ObjectListView"), Description("This event is triggered before the items in the list are sorted.")]
        public event EventHandler<BeforeSortingEventArgs> BeforeSorting;

        [Description("Can the user drop the currently dragged items at the current mouse location?"), Category("Behavior - ObjectListView")]
        public event EventHandler<OlvDropEventArgs> CanDrop;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the user left clicks a cell.")]
        public event EventHandler<CellClickEventArgs> CellClick;

        [Category("Behavior - ObjectListView"), Description("This event is triggered cell edit operation is finishing.")]
        public event CellEditEventHandler CellEditFinishing;

        [Description("This event is triggered when cell edit is about to begin."), Category("Behavior - ObjectListView")]
        public event CellEditEventHandler CellEditStarting;

        [Description("This event is triggered when a cell editor is about to lose focus and its new contents need to be validated."), Category("Behavior - ObjectListView")]
        public event CellEditEventHandler CellEditValidating;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the mouse is over a cell.")]
        public event EventHandler<CellOverEventArgs> CellOver;

        [Description("This event is triggered when the user right clicks a cell."), Category("Behavior - ObjectListView")]
        public event EventHandler<CellRightClickEventArgs> CellRightClick;

        [Description("This event is triggered when a cell needs a tool tip."), Category("Behavior - ObjectListView")]
        public event EventHandler<ToolTipShowingEventArgs> CellToolTipShowing;

        [Description("This event is triggered when the user right clicks a column header."), Category("Behavior - ObjectListView")]
        public event ColumnRightClickEventHandler ColumnRightClick;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the user dropped items onto the control.")]
        public event EventHandler<OlvDropEventArgs> Dropped;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when a cell needs to be formatted.")]
        public event EventHandler<FormatCellEventArgs> FormatCell;

        [Description("This event is triggered when a row needs to be formatted."), Category("Behavior - ObjectListView")]
        public event EventHandler<FormatRowEventArgs> FormatRow;

        [Description("This event is triggered when the task text of a group is clicked."), Category("Behavior - ObjectListView")]
        public event EventHandler<GroupTaskClickedEventArgs> GroupTaskClicked;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when a header needs a tool tip.")]
        public event EventHandler<ToolTipShowingEventArgs> HeaderToolTipShowing;

        [Description("This event is triggered when the hot item changed."), Category("Behavior - ObjectListView")]
        public event EventHandler<HotItemChangedEventArgs> HotItemChanged;

        [Description("This event is triggered when a hyperlink cell is clicked."), Category("Behavior - ObjectListView")]
        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the control needs to know if a given cell contains a hyperlink.")]
        public event EventHandler<IsHyperlinkEventArgs> IsHyperlink;

        [Description("This event is triggered when objects are about to be added to the control"), Category("Behavior - ObjectListView")]
        public event EventHandler<ItemsAddingEventArgs> ItemsAdding;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when the contents of the control have changed.")]
        public event EventHandler<ItemsChangedEventArgs> ItemsChanged;

        [Description("This event is triggered when the contents of the control changes."), Category("Behavior - ObjectListView")]
        public event EventHandler<ItemsChangingEventArgs> ItemsChanging;

        [Category("Behavior - ObjectListView"), Description("This event is triggered when objects are removed from the control.")]
        public event EventHandler<ItemsRemovingEventArgs> ItemsRemoving;

        [Category("Behavior - ObjectListView"), Description("Can the dragged collection of model objects be dropped at the current mouse location")]
        public event EventHandler<ModelDropEventArgs> ModelCanDrop;

        [Category("Behavior - ObjectListView"), Description("A collection of model objects from a ObjectListView has been dropped on this control")]
        public event EventHandler<ModelDropEventArgs> ModelDropped;

        [Description("This event is triggered when the contents of the ObjectListView has scrolled."), Category("Behavior - ObjectListView")]
        public event EventHandler<ScrollEventArgs> Scroll;

        [Description("This event is triggered once per user action that changes the selection state of one or more rows."), Category("Behavior - ObjectListView")]
        public event EventHandler SelectionChanged;

        public ObjectListView()
        {
            base.ColumnClick += new ColumnClickEventHandler(this.HandleColumnClick);
            base.Layout += new LayoutEventHandler(this.HandleLayout);
            base.ColumnWidthChanging += new ColumnWidthChangingEventHandler(this.HandleColumnWidthChanging);
            base.ColumnWidthChanged += new ColumnWidthChangedEventHandler(this.HandleColumnWidthChanged);
            base.View = System.Windows.Forms.View.Details;
            this.DoubleBuffered = true;
            this.ShowSortIndicators = true;
            this.InitializeStandardOverlays();
            this.InitializeEmptyListMsgOverlay();
        }

        private void AddCheckStateBitmap(ImageList il, string key, CheckBoxState boxState)
        {
            Bitmap image = new Bitmap(il.ImageSize.Width, il.ImageSize.Height);
            Graphics g = Graphics.FromImage(image);
            g.Clear(il.TransparentColor);
            Point glyphLocation = new Point((image.Width / 2) - 5, (image.Height / 2) - 6);
            CheckBoxRenderer.DrawCheckBox(g, glyphLocation, boxState);
            il.Images.Add(key, image);
        }

        public virtual void AddDecoration(IDecoration decoration)
        {
            if (decoration != null)
            {
                this.Decorations.Add(decoration);
                base.Invalidate();
            }
        }

        public virtual void AddObject(object modelObject)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(delegate {
                    this.AddObject(modelObject);
                });
            }
            else
            {
                this.AddObjects(new object[] { modelObject });
            }
        }

        public virtual void AddObjects(ICollection modelObjects)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.AddObjects(modelObjects);
                    };
                }
                base.Invoke(method);
            }
            else
            {
                this.InsertObjects(this.GetItemCount(), modelObjects);
                this.Sort(this.LastSortColumn, this.LastSortOrder);
            }
        }

        public virtual void AddOverlay(IOverlay overlay)
        {
            if (overlay != null)
            {
                this.Overlays.Add(overlay);
                base.Invalidate();
            }
        }

        protected virtual void ApplyCellStyle(OLVListItem olvi, int columnIndex, IItemStyle style)
        {
            if ((style != null) && ((this.View == System.Windows.Forms.View.Details) || (columnIndex <= 0)))
            {
                olvi.UseItemStyleForSubItems = false;
                ListViewItem.ListViewSubItem item = olvi.SubItems[columnIndex];
                if (style.Font != null)
                {
                    item.Font = style.Font;
                }
                if (style.FontStyle != FontStyle.Regular)
                {
                    item.Font = new Font(item.Font ?? (olvi.Font ?? this.Font), style.FontStyle);
                }
                if (!style.ForeColor.IsEmpty)
                {
                    item.ForeColor = style.ForeColor;
                }
                if (!style.BackColor.IsEmpty)
                {
                    item.BackColor = style.BackColor;
                }
            }
        }

        protected virtual void ApplyExtendedStyles()
        {
            int style = 0;
            if (!(!this.ShowImagesOnSubItems || base.VirtualMode))
            {
                style ^= 2;
            }
            if (this.ShowHeaderInAllViews)
            {
                style ^= 0x2000000;
            }
            BrightIdeasSoftware.NativeMethods.SetExtendedStyle(this, style, 0x2400002);
        }

        private void ApplyHyperlinkStyle(int rowIndex, OLVListItem olvi)
        {
            olvi.UseItemStyleForSubItems = false;
            Color backColor = olvi.BackColor;
            for (int i = 0; i < this.Columns.Count; i++)
            {
                OLVListSubItem subItem = olvi.GetSubItem(i);
                OLVColumn column = this.GetColumn(i);
                subItem.BackColor = backColor;
                if (column.Hyperlink && !string.IsNullOrEmpty(subItem.Url))
                {
                    if (this.IsUrlVisited(subItem.Url))
                    {
                        this.ApplyCellStyle(olvi, i, this.HyperlinkStyle.Visited);
                    }
                    else
                    {
                        this.ApplyCellStyle(olvi, i, this.HyperlinkStyle.Normal);
                    }
                }
            }
        }

        protected virtual void ApplyRowStyle(OLVListItem olvi, IItemStyle style)
        {
            if (style != null)
            {
                if (base.FullRowSelect || (this.View != System.Windows.Forms.View.Details))
                {
                    if (style.Font != null)
                    {
                        olvi.Font = style.Font;
                    }
                    if (style.FontStyle != FontStyle.Regular)
                    {
                        olvi.Font = new Font(olvi.Font ?? this.Font, style.FontStyle);
                    }
                    if (!style.ForeColor.IsEmpty)
                    {
                        if (olvi.UseItemStyleForSubItems)
                        {
                            olvi.ForeColor = style.ForeColor;
                        }
                        else
                        {
                            foreach (ListViewItem.ListViewSubItem item in olvi.SubItems)
                            {
                                item.ForeColor = style.ForeColor;
                            }
                        }
                    }
                    if (!style.BackColor.IsEmpty)
                    {
                        if (olvi.UseItemStyleForSubItems)
                        {
                            olvi.BackColor = style.BackColor;
                        }
                        else
                        {
                            foreach (ListViewItem.ListViewSubItem item in olvi.SubItems)
                            {
                                item.BackColor = style.BackColor;
                            }
                        }
                    }
                }
                else
                {
                    olvi.UseItemStyleForSubItems = false;
                    foreach (ListViewItem.ListViewSubItem item in olvi.SubItems)
                    {
                        if (style.BackColor.IsEmpty)
                        {
                            item.BackColor = olvi.BackColor;
                        }
                        else
                        {
                            item.BackColor = style.BackColor;
                        }
                    }
                    this.ApplyCellStyle(olvi, 0, style);
                }
            }
        }

        private BeforeSortingEventArgs BuildBeforeSortingEventArgs(OLVColumn column, SortOrder order)
        {
            OLVColumn groupColumn = this.AlwaysGroupByColumn ?? (column ?? this.GetColumn(0));
            SortOrder alwaysGroupBySortOrder = this.AlwaysGroupBySortOrder;
            if (order == SortOrder.None)
            {
                order = base.Sorting;
                if (order == SortOrder.None)
                {
                    order = SortOrder.Ascending;
                }
            }
            if (alwaysGroupBySortOrder == SortOrder.None)
            {
                alwaysGroupBySortOrder = order;
            }
            return new BeforeSortingEventArgs(groupColumn, alwaysGroupBySortOrder, column, order, this.SecondarySortColumn ?? this.GetColumn(0), (this.SecondarySortOrder == SortOrder.None) ? order : this.SecondarySortOrder);
        }

        private void BuildCellEvent(CellEventArgs args, Point location)
        {
            OlvListViewHitTestInfo info = this.OlvHitTest(location.X, location.Y);
            args.HitTest = info;
            args.ListView = this;
            args.Location = location;
            args.Item = info.Item;
            args.SubItem = info.SubItem;
            args.Model = info.RowObject;
            args.ColumnIndex = info.ColumnIndex;
            args.Column = info.Column;
            if (info.Item != null)
            {
                args.RowIndex = info.Item.Index;
            }
            args.ModifierKeys = Control.ModifierKeys;
            if ((args.Item != null) && (args.ListView.View != System.Windows.Forms.View.Details))
            {
                args.ColumnIndex = 0;
                args.Column = args.ListView.GetColumn(0);
                args.SubItem = args.Item.GetSubItem(0);
            }
        }

        public virtual void BuildGroups()
        {
            this.BuildGroups(this.LastSortColumn, (this.LastSortOrder == SortOrder.None) ? SortOrder.Ascending : this.LastSortOrder);
        }

        public virtual void BuildGroups(OLVColumn column, SortOrder order)
        {
            BeforeSortingEventArgs e = this.BuildBeforeSortingEventArgs(column, order);
            this.OnBeforeSorting(e);
            if (!e.Canceled)
            {
                this.BuildGroups(e.ColumnToGroupBy, e.GroupByOrder, e.ColumnToSort, e.SortOrder, e.SecondaryColumnToSort, e.SecondarySortOrder);
                this.OnAfterSorting(new AfterSortingEventArgs(e));
            }
        }

        public virtual void BuildGroups(OLVColumn groupByColumn, SortOrder groupByOrder, OLVColumn column, SortOrder order, OLVColumn secondaryColumn, SortOrder secondaryOrder)
        {
            if (groupByColumn != null)
            {
                int count = base.Items.Count;
                GroupingParameters parms = this.CollectGroupingParameters(groupByColumn, groupByOrder, column, order, secondaryColumn, secondaryOrder);
                CreateGroupsEventArgs e = new CreateGroupsEventArgs(parms);
                this.OnBeforeCreatingGroups(e);
                if (!e.Canceled)
                {
                    if (e.Groups == null)
                    {
                        e.Groups = this.MakeGroups(parms);
                    }
                    this.OnAboutToCreateGroups(e);
                    if (!e.Canceled)
                    {
                        this.OLVGroups = e.Groups;
                        this.CreateGroups(e.Groups);
                        this.OnAfterCreatingGroups(e);
                    }
                }
            }
        }

        public virtual void BuildList()
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new MethodInvoker(this.BuildList));
            }
            else
            {
                this.BuildList(true);
            }
        }

        public virtual void BuildList(bool shouldPreserveState)
        {
            if (!this.Frozen)
            {
                this.ApplyExtendedStyles();
                this.ClearHotItem();
                int topItemIndex = this.TopItemIndex;
                IList selectedObjects = new ArrayList();
                object modelObject = null;
                if (shouldPreserveState && (this.objects != null))
                {
                    selectedObjects = this.SelectedObjects;
                    OLVListItem focusedItem = base.FocusedItem as OLVListItem;
                    if (focusedItem != null)
                    {
                        modelObject = focusedItem.RowObject;
                    }
                }
                base.BeginUpdate();
                try
                {
                    base.Items.Clear();
                    base.ListViewItemSorter = null;
                    if (this.objects != null)
                    {
                        List<OLVListItem> list2 = new List<OLVListItem>();
                        foreach (object obj3 in this.objects)
                        {
                            OLVListItem lvi = new OLVListItem(obj3);
                            this.FillInValues(lvi, obj3);
                            list2.Add(lvi);
                        }
                        base.Items.AddRange(list2.ToArray());
                        this.Sort();
                        if (shouldPreserveState)
                        {
                            this.SelectedObjects = selectedObjects;
                            base.FocusedItem = this.ModelToItem(modelObject);
                        }
                        this.RefreshHotItem();
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
                if (shouldPreserveState)
                {
                    this.TopItemIndex = topItemIndex;
                    this.RefreshHotItem();
                }
            }
        }

        public virtual Rectangle CalculateCellBounds(OLVListItem item, int subItemIndex)
        {
            return this.CalculateCellBounds(item, subItemIndex, ItemBoundsPortion.Label);
        }

        private Rectangle CalculateCellBounds(OLVListItem item, int subItemIndex, ItemBoundsPortion portion)
        {
            if (subItemIndex > 0)
            {
                return item.SubItems[subItemIndex].Bounds;
            }
            Rectangle itemRect = base.GetItemRect(item.Index, portion);
            if (this.View == System.Windows.Forms.View.Details)
            {
                Point scrolledColumnSides = BrightIdeasSoftware.NativeMethods.GetScrolledColumnSides(this, 0);
                itemRect.X = scrolledColumnSides.X + 4;
                itemRect.Width = (scrolledColumnSides.Y - scrolledColumnSides.X) - 5;
            }
            return itemRect;
        }

        public Rectangle CalculateCellEditorBounds(OLVListItem item, int subItemIndex)
        {
            Rectangle subItemBounds;
            if (this.View == System.Windows.Forms.View.Details)
            {
                subItemBounds = item.GetSubItemBounds(subItemIndex);
            }
            else
            {
                subItemBounds = base.GetItemRect(item.Index, ItemBoundsPortion.Label);
            }
            if (base.OwnerDraw)
            {
                return this.CalculateCellEditorBoundsOwnerDrawn(item, subItemIndex, subItemBounds);
            }
            return this.CalculateCellEditorBoundsStandard(item, subItemIndex, subItemBounds);
        }

        protected Rectangle CalculateCellEditorBoundsOwnerDrawn(OLVListItem item, int subItemIndex, Rectangle r)
        {
            IRenderer itemRenderer = null;
            if (this.View == System.Windows.Forms.View.Details)
            {
                itemRenderer = this.GetColumn(subItemIndex).Renderer ?? this.DefaultRenderer;
            }
            else
            {
                itemRenderer = this.ItemRenderer;
            }
            if (itemRenderer == null)
            {
                return r;
            }
            return itemRenderer.GetEditRectangle(base.CreateGraphics(), r, item, subItemIndex);
        }

        protected Rectangle CalculateCellEditorBoundsStandard(OLVListItem item, int subItemIndex, Rectangle cellBounds)
        {
            if (this.View == System.Windows.Forms.View.Details)
            {
                int num = 0;
                object imageSelector = item.ImageSelector;
                if (subItemIndex > 0)
                {
                    imageSelector = ((OLVListSubItem) item.SubItems[subItemIndex]).ImageSelector;
                }
                if (this.GetActualImageIndex(imageSelector) != -1)
                {
                    num += this.SmallImageSize.Width + 2;
                }
                if ((this.CheckBoxes && (base.StateImageList != null)) && (subItemIndex == 0))
                {
                    num += base.StateImageList.ImageSize.Width + 2;
                }
                if ((subItemIndex == 0) && (item.IndentCount > 0))
                {
                    num += this.SmallImageSize.Width * item.IndentCount;
                }
                if (num > 0)
                {
                    cellBounds.X += num;
                    cellBounds.Width -= num;
                }
            }
            return cellBounds;
        }

        public virtual Rectangle CalculateCellTextBounds(OLVListItem item, int subItemIndex)
        {
            return this.CalculateCellBounds(item, subItemIndex, ItemBoundsPortion.ItemOnly);
        }

        protected virtual void CalculateOwnerDrawnHitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            if ((hti.Item != null) && ((this.View != System.Windows.Forms.View.Details) || (hti.Column != null)))
            {
                IRenderer itemRenderer = null;
                if (this.View == System.Windows.Forms.View.Details)
                {
                    itemRenderer = hti.Column.Renderer ?? this.DefaultRenderer;
                }
                else
                {
                    itemRenderer = this.ItemRenderer;
                }
                if (itemRenderer != null)
                {
                    itemRenderer.HitTest(hti, x, y);
                }
            }
        }

        public virtual void CalculateReasonableTileSize()
        {
            if (this.Columns.Count > 0)
            {
                List<OLVColumn> list = this.AllColumns.FindAll(x => (x.Index == 0) || x.IsTileViewColumn);
                int num = (base.LargeImageList == null) ? 0x10 : base.LargeImageList.ImageSize.Height;
                int num2 = (this.Font.Height + 1) * list.Count;
                int width = (base.TileSize.Width == 0) ? 200 : base.TileSize.Width;
                int height = Math.Max(base.TileSize.Height, Math.Max(num, num2));
                base.TileSize = new Size(width, height);
            }
        }

        protected virtual void CalculateStandardHitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            if ((((this.View == System.Windows.Forms.View.Details) && (hti.ColumnIndex != 0)) && (hti.SubItem != null)) && (hti.Column != null))
            {
                Rectangle bounds = hti.SubItem.Bounds;
                bool flag = this.GetActualImageIndex(hti.SubItem.ImageSelector) != -1;
                hti.HitTestLocation = HitTestLocation.InCell;
                Rectangle rectangle2 = bounds;
                rectangle2.Width = this.SmallImageSize.Width;
                if (rectangle2.Contains(x, y))
                {
                    if (hti.Column.CheckBoxes)
                    {
                        hti.HitTestLocation = HitTestLocation.CheckBox;
                        return;
                    }
                    if (flag)
                    {
                        hti.HitTestLocation = HitTestLocation.Image;
                        return;
                    }
                }
                Rectangle rectangle3 = bounds;
                rectangle3.X += 4;
                if (flag)
                {
                    rectangle3.X += this.SmallImageSize.Width;
                }
                Size proposedSize = new Size(rectangle3.Width, rectangle3.Height);
                Size size2 = TextRenderer.MeasureText(hti.SubItem.Text, this.Font, proposedSize, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                rectangle3.Width = size2.Width;
                switch (hti.Column.TextAlign)
                {
                    case HorizontalAlignment.Right:
                        rectangle3.X = bounds.Right - size2.Width;
                        break;

                    case HorizontalAlignment.Center:
                        rectangle3.X += ((bounds.Right - bounds.Left) - size2.Width) / 2;
                        break;
                }
                if (rectangle3.Contains(x, y))
                {
                    hti.HitTestLocation = HitTestLocation.Text;
                }
            }
        }

        private CheckState CalculateState(int state)
        {
            switch (((state & 0xf000) >> 12))
            {
                case 1:
                    return CheckState.Unchecked;

                case 2:
                    return CheckState.Checked;

                case 3:
                    return CheckState.Indeterminate;
            }
            return CheckState.Checked;
        }

        public virtual void CancelCellEdit()
        {
            if (this.IsCellEditing)
            {
                this.cellEditEventArgs.Cancel = true;
                this.OnCellEditFinishing(this.cellEditEventArgs);
                this.CleanupCellEdit();
            }
        }

        protected virtual void CellEditor_Validating(object sender, CancelEventArgs e)
        {
            this.cellEditEventArgs.Cancel = false;
            this.OnCellEditorValidating(this.cellEditEventArgs);
            if (this.cellEditEventArgs.Cancel)
            {
                this.cellEditEventArgs.Control.Select();
                e.Cancel = true;
            }
            else
            {
                this.FinishCellEdit();
            }
        }

        public virtual void ChangeToFilteredColumns(System.Windows.Forms.View view)
        {
            IList selectedObjects = this.SelectedObjects;
            int topItemIndex = this.TopItemIndex;
            this.Freeze();
            base.Clear();
            List<OLVColumn> filteredColumns = this.GetFilteredColumns(view);
            if (view == System.Windows.Forms.View.Details)
            {
                foreach (OLVColumn column in filteredColumns)
                {
                    if ((column.LastDisplayIndex == -1) || (column.LastDisplayIndex > (filteredColumns.Count - 1)))
                    {
                        column.DisplayIndex = filteredColumns.Count - 1;
                    }
                    else
                    {
                        column.DisplayIndex = column.LastDisplayIndex;
                    }
                }
            }
            this.Columns.AddRange(filteredColumns.ToArray());
            if (view == System.Windows.Forms.View.Details)
            {
                this.ShowSortIndicator();
            }
            this.BuildList();
            this.Unfreeze();
            this.SelectedObjects = selectedObjects;
            this.TopItemIndex = topItemIndex;
        }

        public virtual void CheckIndeterminateObject(object modelObject)
        {
            this.SetObjectCheckedness(modelObject, CheckState.Indeterminate);
        }

        public virtual void CheckIndeterminateSubItem(object rowObject, OLVColumn column)
        {
            if (((column != null) && (rowObject != null)) && column.CheckBoxes)
            {
                column.PutCheckState(rowObject, CheckState.Indeterminate);
                this.RefreshObject(rowObject);
            }
        }

        public virtual void CheckObject(object modelObject)
        {
            this.SetObjectCheckedness(modelObject, CheckState.Checked);
        }

        public virtual void CheckSubItem(object rowObject, OLVColumn column)
        {
            if (((column != null) && (rowObject != null)) && column.CheckBoxes)
            {
                column.PutCheckState(rowObject, CheckState.Checked);
                this.RefreshObject(rowObject);
            }
        }

        protected virtual void CleanupCellEdit()
        {
            if (this.cellEditor != null)
            {
                this.cellEditor.Validating -= new CancelEventHandler(this.CellEditor_Validating);
                base.Controls.Remove(this.cellEditor);
                this.cellEditor = null;
                base.Select();
                this.PauseAnimations(false);
            }
        }

        public virtual void ClearHotItem()
        {
            this.UpdateHotItem(new Point(-1, -1));
        }

        public virtual void ClearObjects()
        {
            if (base.InvokeRequired)
            {
                base.Invoke(new MethodInvoker(this.ClearObjects));
            }
            else
            {
                this.SetObjects(null);
            }
        }

        public virtual void ClearUrlVisited()
        {
            this.visitedUrlMap = new Dictionary<string, bool>();
        }

        protected virtual GroupingParameters CollectGroupingParameters(OLVColumn groupByColumn, SortOrder groupByOrder, OLVColumn column, SortOrder order, OLVColumn secondaryColumn, SortOrder secondaryOrder)
        {
            string titleFormat = this.ShowItemCountOnGroups ? groupByColumn.GroupWithItemCountFormatOrDefault : null;
            return new GroupingParameters(this, groupByColumn, groupByOrder, column, order, secondaryColumn, secondaryOrder, titleFormat, this.ShowItemCountOnGroups ? groupByColumn.GroupWithItemCountSingularFormatOrDefault : null, this.SortGroupItemsByPrimaryColumn);
        }

        private void ColumnSelectMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = ((this.SelectColumnsMenuStaysOpen && (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)) && (this.lastMenuItemClicked != null)) && (this.lastMenuItemClicked.Tag is OLVColumn);
        }

        private void ColumnSelectMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.lastMenuItemClicked = (ToolStripMenuItem) e.ClickedItem;
            OLVColumn tag = this.lastMenuItemClicked.Tag as OLVColumn;
            if (tag != null)
            {
                this.lastMenuItemClicked.Checked = !this.lastMenuItemClicked.Checked;
                tag.IsVisible = this.lastMenuItemClicked.Checked;
                base.BeginInvoke(new MethodInvoker(this.RebuildColumns));
            }
        }

        public void ConfigureAutoComplete(System.Windows.Forms.TextBox tb, OLVColumn column)
        {
            this.ConfigureAutoComplete(tb, column, 0x3e8);
        }

        public void ConfigureAutoComplete(System.Windows.Forms.TextBox tb, OLVColumn column, int maxRows)
        {
            maxRows = Math.Min(this.GetItemCount(), maxRows);
            tb.AutoCompleteCustomSource.Clear();
            Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
            List<string> list = new List<string>();
            for (int i = 0; i < maxRows; i++)
            {
                string stringValue = column.GetStringValue(this.GetModelObject(i));
                if (!(string.IsNullOrEmpty(stringValue) || dictionary.ContainsKey(stringValue)))
                {
                    list.Add(stringValue);
                    dictionary[stringValue] = true;
                }
            }
            tb.AutoCompleteCustomSource.AddRange(list.ToArray());
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
            tb.AutoCompleteMode = AutoCompleteMode.Append;
        }

        protected virtual void ConfigureControl()
        {
            this.cellEditor.Validating += new CancelEventHandler(this.CellEditor_Validating);
            this.cellEditor.Select();
        }

        public virtual void CopyObjectsToClipboard(IList objectsToCopy)
        {
            if (objectsToCopy.Count != 0)
            {
                OLVDataObject data = new OLVDataObject(this, objectsToCopy);
                data.CreateTextFormats();
                Clipboard.SetDataObject(data);
            }
        }

        public virtual void CopySelectionToClipboard()
        {
            this.CopyObjectsToClipboard(this.SelectedObjects);
        }

        protected virtual void CorrectSubItemColors(ListViewItem olvi)
        {
        }

        protected virtual void CreateCellToolTip()
        {
            this.cellToolTip = new ToolTipControl();
            this.cellToolTip.AssignHandle(BrightIdeasSoftware.NativeMethods.GetTooltipControl(this));
            this.cellToolTip.Showing += new EventHandler<ToolTipShowingEventArgs>(this.HandleCellToolTipShowing);
            this.cellToolTip.SetMaxWidth();
            BrightIdeasSoftware.NativeMethods.MakeTopMost(this.cellToolTip);
        }

        protected virtual void CreateGroups(IList<OLVGroup> groups)
        {
            base.Groups.Clear();
            foreach (OLVGroup group in groups)
            {
                group.InsertGroupOldStyle(this);
                group.SetItemsOldStyle();
            }
        }

        public virtual void DeselectAll()
        {
            BrightIdeasSoftware.NativeMethods.DeselectAllItems(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (GlassPanelForm form in this.glassPanels)
                {
                    form.Unbind();
                    form.Dispose();
                }
                this.glassPanels.Clear();
            }
        }

        private void DoSort(OLVColumn columnToSort, SortOrder order)
        {
            if ((this.GetItemCount() != 0) && (this.Columns.Count != 0))
            {
                if (this.ShowGroups)
                {
                    columnToSort = columnToSort ?? this.GetColumn(0);
                    if (order == SortOrder.None)
                    {
                        order = base.Sorting;
                        if (order == SortOrder.None)
                        {
                            order = SortOrder.Ascending;
                        }
                    }
                }
                BeforeSortingEventArgs e = this.BuildBeforeSortingEventArgs(columnToSort, order);
                this.OnBeforeSorting(e);
                if (!e.Canceled && ((e.ColumnToSort != null) && (e.SortOrder != SortOrder.None)))
                {
                    IList selectedObjects = new ArrayList();
                    if (base.VirtualMode)
                    {
                        selectedObjects = this.SelectedObjects;
                    }
                    this.ClearHotItem();
                    if (!e.Handled)
                    {
                        if (this.ShowGroups)
                        {
                            this.BuildGroups(e.ColumnToGroupBy, e.GroupByOrder, e.ColumnToSort, e.SortOrder, e.SecondaryColumnToSort, e.SecondarySortOrder);
                        }
                        else if (this.CustomSorter != null)
                        {
                            this.CustomSorter(columnToSort, order);
                        }
                        else
                        {
                            base.ListViewItemSorter = new ColumnComparer(e.ColumnToSort, e.SortOrder, e.SecondaryColumnToSort, e.SecondarySortOrder);
                        }
                    }
                    if (this.ShowSortIndicators)
                    {
                        this.ShowSortIndicator(e.ColumnToSort, e.SortOrder);
                    }
                    if (this.UseAlternatingBackColors && (this.View == System.Windows.Forms.View.Details))
                    {
                        this.PrepareAlternateBackColors();
                    }
                    this.LastSortColumn = e.ColumnToSort;
                    this.LastSortOrder = e.SortOrder;
                    if (selectedObjects.Count > 0)
                    {
                        this.SelectedObjects = selectedObjects;
                    }
                    this.RefreshHotItem();
                    this.OnAfterSorting(new AfterSortingEventArgs(e));
                }
            }
        }

        protected virtual void DoUnfreeze()
        {
            this.ResizeFreeSpaceFillingColumns();
            this.BuildList();
        }

        protected virtual void DrawAllDecorations(Graphics g, List<OLVListItem> drawnItems)
        {
            g.TextRenderingHint = TextRendereringHint;
            g.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle contentRectangle = this.ContentRectangle;
            if (this.HasEmptyListMsg && (this.GetItemCount() == 0))
            {
                this.EmptyListMsgOverlay.Draw(this, g, contentRectangle);
            }
            if (this.DropSink != null)
            {
                this.DropSink.DrawFeedback(g, contentRectangle);
            }
            foreach (OLVListItem item in drawnItems)
            {
                if (item.HasDecoration)
                {
                    foreach (IDecoration decoration in item.Decorations)
                    {
                        decoration.ListItem = item;
                        decoration.SubItem = null;
                        decoration.Draw(this, g, contentRectangle);
                    }
                }
                foreach (OLVListSubItem item2 in item.SubItems)
                {
                    if (item2.HasDecoration)
                    {
                        foreach (IDecoration decoration in item2.Decorations)
                        {
                            decoration.ListItem = item;
                            decoration.SubItem = item2;
                            decoration.Draw(this, g, contentRectangle);
                        }
                    }
                }
                if ((this.SelectedRowDecoration != null) && item.Selected)
                {
                    this.SelectedRowDecoration.ListItem = item;
                    this.SelectedRowDecoration.SubItem = null;
                    this.SelectedRowDecoration.Draw(this, g, contentRectangle);
                }
            }
            foreach (IDecoration decoration2 in this.Decorations)
            {
                decoration2.ListItem = null;
                decoration2.SubItem = null;
                decoration2.Draw(this, g, contentRectangle);
            }
            if ((this.UseHotItem && (this.HotItemStyle != null)) && (this.HotItemStyle.Decoration != null))
            {
                IDecoration decoration3 = this.HotItemStyle.Decoration;
                decoration3.ListItem = this.GetItem(this.HotRowIndex);
                if (decoration3.ListItem == null)
                {
                    decoration3.SubItem = null;
                }
                else
                {
                    decoration3.SubItem = decoration3.ListItem.GetSubItem(this.HotColumnIndex);
                }
                decoration3.Draw(this, g, contentRectangle);
            }
            if (base.DesignMode)
            {
                foreach (IOverlay overlay in this.Overlays)
                {
                    overlay.Draw(this, g, contentRectangle);
                }
            }
        }

        private void dropSink_CanDrop(object sender, OlvDropEventArgs e)
        {
            this.OnCanDrop(e);
        }

        private void dropSink_Dropped(object sender, OlvDropEventArgs e)
        {
            this.OnDropped(e);
        }

        private void dropSink_ModelCanDrop(object sender, ModelDropEventArgs e)
        {
            this.OnModelCanDrop(e);
        }

        private void dropSink_ModelDropped(object sender, ModelDropEventArgs e)
        {
            this.OnModelDropped(e);
        }

        public virtual void EditSubItem(OLVListItem item, int subItemIndex)
        {
            if (((item != null) && ((subItemIndex >= 0) || (subItemIndex < item.SubItems.Count))) && ((this.CellEditActivation != CellEditActivateMode.None) && this.GetColumn(subItemIndex).IsEditable))
            {
                this.StartCellEdit(item, subItemIndex);
            }
        }

        public virtual void EnableCustomSelectionColors()
        {
            this.UseCustomSelectionColors = true;
        }

        public virtual void EnsureGroupVisible(ListViewGroup lvg)
        {
            if (this.ShowGroups && (lvg != null))
            {
                int num2;
                int index = base.Groups.IndexOf(lvg);
                if (index <= 0)
                {
                    num2 = -BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, false);
                    BrightIdeasSoftware.NativeMethods.Scroll(this, 0, num2);
                }
                else
                {
                    ListViewGroup group = base.Groups[index - 1];
                    ListViewItem item = group.Items[group.Items.Count - 1];
                    Rectangle itemRect = base.GetItemRect(item.Index);
                    num2 = itemRect.Y + (itemRect.Height / 2);
                    BrightIdeasSoftware.NativeMethods.Scroll(this, 0, num2);
                }
            }
        }

        public virtual void EnsureModelVisible(object modelObject)
        {
            int index = this.IndexOf(modelObject);
            if (index >= 0)
            {
                base.EnsureVisible(index);
            }
        }

        protected virtual void FillInValues(OLVListItem lvi, object rowObject)
        {
            if (this.Columns.Count != 0)
            {
                int num;
                OLVListSubItem item = this.MakeSubItem(rowObject, this.GetColumn(0));
                lvi.SubItems[0] = item;
                lvi.ImageSelector = item.ImageSelector;
                if (this.View == System.Windows.Forms.View.Details)
                {
                    for (num = 1; num < this.Columns.Count; num++)
                    {
                        lvi.SubItems.Add(this.MakeSubItem(rowObject, this.GetColumn(num)));
                    }
                }
                else if (this.View == System.Windows.Forms.View.Tile)
                {
                    for (num = 1; num < this.Columns.Count; num++)
                    {
                        OLVColumn column = this.GetColumn(num);
                        if (column.IsTileViewColumn)
                        {
                            lvi.SubItems.Add(this.MakeSubItem(rowObject, column));
                        }
                    }
                }
                lvi.Font = this.Font;
                lvi.BackColor = this.BackColor;
                lvi.ForeColor = this.ForeColor;
                if (this.CheckBoxes)
                {
                    CheckState? checkState = this.GetCheckState(lvi.RowObject);
                    if (checkState.HasValue)
                    {
                        lvi.CheckState = checkState.Value;
                    }
                }
                if (this.RowFormatter != null)
                {
                    this.RowFormatter(lvi);
                }
            }
        }

        private GlassPanelForm FindGlassPanelForOverlay(IOverlay overlay)
        {
            return this.glassPanels.Find(x => x.Overlay == overlay);
        }

        public virtual int FindMatchingRow(string text, int start, SearchDirectionHint direction)
        {
            int num2;
            int itemCount = this.GetItemCount();
            if (itemCount == 0)
            {
                return -1;
            }
            OLVColumn lastSortColumn = this.GetColumn(0);
            if ((this.IsSearchOnSortColumn && (this.View == System.Windows.Forms.View.Details)) && (this.LastSortColumn != null))
            {
                lastSortColumn = this.LastSortColumn;
            }
            if (direction == SearchDirectionHint.Down)
            {
                num2 = this.FindMatchInRange(text, start, itemCount - 1, lastSortColumn);
                if ((num2 == -1) && (start > 0))
                {
                    num2 = this.FindMatchInRange(text, 0, start - 1, lastSortColumn);
                }
                return num2;
            }
            num2 = this.FindMatchInRange(text, start, 0, lastSortColumn);
            if ((num2 == -1) && (start != itemCount))
            {
                num2 = this.FindMatchInRange(text, itemCount - 1, start + 1, lastSortColumn);
            }
            return num2;
        }

        protected virtual int FindMatchInRange(string text, int first, int last, OLVColumn column)
        {
            int num;
            if (first <= last)
            {
                for (num = first; num <= last; num++)
                {
                    if (column.GetStringValue(this.GetNthItemInDisplayOrder(num).RowObject).StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return num;
                    }
                }
            }
            else
            {
                for (num = first; num >= last; num--)
                {
                    if (column.GetStringValue(this.GetNthItemInDisplayOrder(num).RowObject).StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return num;
                    }
                }
            }
            return -1;
        }

        public virtual void FinishCellEdit()
        {
            if (this.IsCellEditing)
            {
                this.cellEditEventArgs.Cancel = false;
                this.OnCellEditFinishing(this.cellEditEventArgs);
                if (!this.cellEditEventArgs.Cancel)
                {
                    object controlValue = this.GetControlValue(this.cellEditor);
                    this.cellEditEventArgs.Column.PutValue(this.cellEditEventArgs.RowObject, controlValue);
                    this.RefreshItem(this.cellEditEventArgs.ListViewItem);
                }
                this.CleanupCellEdit();
            }
        }

        protected virtual void ForceSubItemImagesExStyle()
        {
            if (!base.VirtualMode)
            {
                BrightIdeasSoftware.NativeMethods.ForceSubItemImagesExStyle(this);
            }
        }

        public virtual void Freeze()
        {
            this.freezeCount++;
        }

        protected virtual int GetActualImageIndex(object imageSelector)
        {
            if (imageSelector != null)
            {
                if (imageSelector is int)
                {
                    return (int) imageSelector;
                }
                string key = imageSelector as string;
                if ((key != null) && (this.SmallImageList != null))
                {
                    return this.SmallImageList.Images.IndexOfKey(key);
                }
            }
            return -1;
        }

        protected virtual Control GetCellEditor(OLVListItem item, int subItemIndex)
        {
            OLVColumn column = this.GetColumn(subItemIndex);
            object obj2 = column.GetValue(item.RowObject);
            for (int i = 0; (obj2 == null) && (i < Math.Min(this.GetItemCount(), 0x3e8)); i++)
            {
                obj2 = column.GetValue(this.GetModelObject(i));
            }
            Control control = EditorRegistry.GetEditor(item.RowObject, column, obj2);
            if (control == null)
            {
                control = this.MakeDefaultCellEditor(column);
            }
            return control;
        }

        public virtual string GetCellToolTip(int columnIndex, int rowIndex)
        {
            if (this.CellToolTipGetter != null)
            {
                return this.CellToolTipGetter(this.GetColumn(columnIndex), this.GetModelObject(rowIndex));
            }
            if (columnIndex >= 0)
            {
                OLVListSubItem subItem = this.GetSubItem(rowIndex, columnIndex);
                if ((((subItem != null) && !string.IsNullOrEmpty(subItem.Url)) && (subItem.Url != subItem.Text)) && (this.HotCellHitLocation == HitTestLocation.Text))
                {
                    return subItem.Url;
                }
            }
            return null;
        }

        [Obsolete("Use CheckedObject property instead of this method")]
        public virtual object GetCheckedObject()
        {
            return this.CheckedObject;
        }

        [Obsolete("Use CheckedObjects property instead of this method")]
        public virtual ArrayList GetCheckedObjects()
        {
            return (ArrayList) this.CheckedObjects;
        }

        protected virtual CheckState? GetCheckState(object modelObject)
        {
            if (this.CheckStateGetter == null)
            {
                return null;
            }
            return new CheckState?(this.CheckStateGetter(modelObject));
        }

        public virtual OLVColumn GetColumn(int index)
        {
            return (OLVColumn) this.Columns[index];
        }

        public virtual OLVColumn GetColumn(string name)
        {
            foreach (ColumnHeader header in this.Columns)
            {
                if (header.Text == name)
                {
                    return (OLVColumn) header;
                }
            }
            return null;
        }

        protected virtual object GetControlValue(Control control)
        {
            if (control is System.Windows.Forms.TextBox)
            {
                return ((System.Windows.Forms.TextBox) control).Text;
            }
            if (control is System.Windows.Forms.ComboBox)
            {
                return ((System.Windows.Forms.ComboBox) control).SelectedValue;
            }
            if (control is System.Windows.Forms.CheckBox)
            {
                return ((System.Windows.Forms.CheckBox) control).Checked;
            }
            try
            {
                return control.GetType().InvokeMember("Value", BindingFlags.GetProperty, null, control, null);
            }
            catch (MissingMethodException)
            {
                return control.Text;
            }
            catch (MissingFieldException)
            {
                return control.Text;
            }
        }

        public virtual List<OLVColumn> GetFilteredColumns(System.Windows.Forms.View view)
        {
            int index;
            switch (view)
            {
                case System.Windows.Forms.View.Details:
                case System.Windows.Forms.View.Tile:
                    index = 0;
                    return this.AllColumns.FindAll(x => (index++ == 0) || x.IsVisible);
            }
            return new List<OLVColumn>();
        }

        public virtual string GetHeaderToolTip(int columnIndex)
        {
            OLVColumn column = this.GetColumn(columnIndex);
            if (column == null)
            {
                return null;
            }
            string toolTipText = column.ToolTipText;
            if (this.HeaderToolTipGetter != null)
            {
                toolTipText = this.HeaderToolTipGetter(column);
            }
            return toolTipText;
        }

        public virtual OLVListItem GetItem(int index)
        {
            if ((index >= 0) && (index < this.GetItemCount()))
            {
                return (OLVListItem) base.Items[index];
            }
            return null;
        }

        public virtual OLVListItem GetItemAt(int x, int y, out OLVColumn selectedColumn)
        {
            selectedColumn = null;
            ListViewHitTestInfo info = this.HitTest(x, y);
            if (info.Item == null)
            {
                return null;
            }
            if (info.SubItem != null)
            {
                int index = info.Item.SubItems.IndexOf(info.SubItem);
                selectedColumn = this.GetColumn(index);
            }
            return (OLVListItem) info.Item;
        }

        public virtual int GetItemCount()
        {
            return base.Items.Count;
        }

        public virtual int GetItemIndexInDisplayOrder(ListViewItem value)
        {
            if (!this.ShowGroups)
            {
                return value.Index;
            }
            int num = 0;
            foreach (ListViewGroup group in base.Groups)
            {
                foreach (ListViewItem item in group.Items)
                {
                    if (item == value)
                    {
                        return num;
                    }
                    num++;
                }
            }
            return -1;
        }

        public virtual OLVListItem GetLastItemInDisplayOrder()
        {
            if (!this.ShowGroups)
            {
                return this.GetItem(this.GetItemCount() - 1);
            }
            if (base.Groups.Count > 0)
            {
                ListViewGroup group = base.Groups[base.Groups.Count - 1];
                if (group.Items.Count > 0)
                {
                    return (OLVListItem) group.Items[group.Items.Count - 1];
                }
            }
            return null;
        }

        public virtual object GetModelObject(int index)
        {
            OLVListItem item = this.GetItem(index);
            if (item == null)
            {
                return null;
            }
            return item.RowObject;
        }

        public virtual ListViewItem GetNextItem(ListViewItem itemToFind)
        {
            if (this.ShowGroups)
            {
                bool flag = itemToFind == null;
                foreach (ListViewGroup group in base.Groups)
                {
                    foreach (ListViewItem item in group.Items)
                    {
                        if (flag)
                        {
                            return item;
                        }
                        flag = item == itemToFind;
                    }
                }
                return null;
            }
            if (this.GetItemCount() == 0)
            {
                return null;
            }
            if (itemToFind == null)
            {
                return this.GetItem(0);
            }
            if (itemToFind.Index == (this.GetItemCount() - 1))
            {
                return null;
            }
            return this.GetItem(itemToFind.Index + 1);
        }

        public virtual OLVListItem GetNthItemInDisplayOrder(int n)
        {
            if (!this.ShowGroups)
            {
                return this.GetItem(n);
            }
            foreach (ListViewGroup group in base.Groups)
            {
                if (n < group.Items.Count)
                {
                    return (OLVListItem) group.Items[n];
                }
                n -= group.Items.Count;
            }
            return null;
        }

        public virtual ListViewItem GetPreviousItem(ListViewItem itemToFind)
        {
            if (this.ShowGroups)
            {
                ListViewItem item = null;
                foreach (ListViewGroup group in base.Groups)
                {
                    foreach (ListViewItem item2 in group.Items)
                    {
                        if (item2 == itemToFind)
                        {
                            return item;
                        }
                        item = item2;
                    }
                }
                if (itemToFind == null)
                {
                    return item;
                }
                return null;
            }
            if (this.GetItemCount() == 0)
            {
                return null;
            }
            if (itemToFind == null)
            {
                return this.GetItem(this.GetItemCount() - 1);
            }
            if (itemToFind.Index == 0)
            {
                return null;
            }
            return this.GetItem(itemToFind.Index - 1);
        }

        public virtual object GetSelectedObject()
        {
            if (base.SelectedIndices.Count == 1)
            {
                return this.GetModelObject(base.SelectedIndices[0]);
            }
            return null;
        }

        public virtual ArrayList GetSelectedObjects()
        {
            ArrayList list = new ArrayList(base.SelectedIndices.Count);
            foreach (int num in base.SelectedIndices)
            {
                list.Add(this.GetModelObject(num));
            }
            return list;
        }

        public virtual OLVListSubItem GetSubItem(int index, int columnIndex)
        {
            OLVListItem item = this.GetItem(index);
            if (item == null)
            {
                return null;
            }
            return item.GetSubItem(columnIndex);
        }

        protected virtual void HandleApplicationIdle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(this.HandleApplicationIdle);
            this.hasIdleHandler = false;
            this.OnSelectionChanged(new EventArgs());
        }

        protected virtual bool HandleBeginScroll(ref Message m)
        {
            ScrollEventArgs args;
            BrightIdeasSoftware.NativeMethods.NMLVSCROLL lParam = (BrightIdeasSoftware.NativeMethods.NMLVSCROLL) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMLVSCROLL));
            if (lParam.dx != 0)
            {
                int scrollPosition = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, true);
                args = new ScrollEventArgs(ScrollEventType.EndScroll, scrollPosition - lParam.dx, scrollPosition, ScrollOrientation.HorizontalScroll);
                this.OnScroll(args);
                if (this.GetItemCount() == 0)
                {
                    base.Invalidate();
                }
            }
            if (lParam.dy != 0)
            {
                int newValue = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, false);
                args = new ScrollEventArgs(ScrollEventType.EndScroll, newValue - lParam.dy, newValue, ScrollOrientation.VerticalScroll);
                this.OnScroll(args);
            }
            return false;
        }

        protected virtual void HandleCellToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            this.BuildCellEvent(e, base.PointToClient(Cursor.Position));
            if (e.Item != null)
            {
                e.Text = this.GetCellToolTip(e.ColumnIndex, e.RowIndex);
                this.OnCellToolTip(e);
            }
        }

        protected virtual bool HandleChar(ref Message m)
        {
            if (!this.ProcessKeyEventArgs(ref m))
            {
                char ch = (char) m.WParam.ToInt32();
                if (ch == '\b')
                {
                    this.timeLastCharEvent = 0;
                    return true;
                }
                if (Environment.TickCount < (this.timeLastCharEvent + 0x3e8))
                {
                    this.lastSearchString = this.lastSearchString + ch;
                }
                else
                {
                    this.lastSearchString = ch.ToString();
                }
                if (this.CheckBoxes && (this.lastSearchString == " "))
                {
                    this.timeLastCharEvent = 0;
                    return true;
                }
                int startSearchFrom = 0;
                ListViewItem focusedItem = base.FocusedItem;
                if (focusedItem != null)
                {
                    startSearchFrom = this.GetItemIndexInDisplayOrder(focusedItem);
                    if (this.lastSearchString.Length == 1)
                    {
                        startSearchFrom++;
                        if (startSearchFrom == this.GetItemCount())
                        {
                            startSearchFrom = 0;
                        }
                    }
                }
                BeforeSearchingEventArgs e = new BeforeSearchingEventArgs(this.lastSearchString, startSearchFrom);
                this.OnBeforeSearching(e);
                if (e.Canceled)
                {
                    return true;
                }
                string stringToFind = e.StringToFind;
                startSearchFrom = e.StartSearchFrom;
                int n = this.FindMatchingRow(stringToFind, startSearchFrom, SearchDirectionHint.Down);
                if (n >= 0)
                {
                    base.BeginUpdate();
                    try
                    {
                        base.SelectedIndices.Clear();
                        ListViewItem nthItemInDisplayOrder = this.GetNthItemInDisplayOrder(n);
                        nthItemInDisplayOrder.Selected = true;
                        nthItemInDisplayOrder.Focused = true;
                        base.EnsureVisible(nthItemInDisplayOrder.Index);
                    }
                    finally
                    {
                        base.EndUpdate();
                    }
                }
                AfterSearchingEventArgs args2 = new AfterSearchingEventArgs(stringToFind, n);
                this.OnAfterSearching(args2);
                if (!args2.Handled && (n < 0))
                {
                    SystemSounds.Beep.Play();
                }
                this.timeLastCharEvent = Environment.TickCount;
            }
            return true;
        }

        protected virtual void HandleColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (this.PossibleFinishCellEditing())
            {
                if ((this.LastSortColumn != null) && (e.Column == this.LastSortColumn.Index))
                {
                    this.LastSortOrder = (this.LastSortOrder == SortOrder.Descending) ? SortOrder.Ascending : SortOrder.Descending;
                }
                else
                {
                    this.LastSortOrder = SortOrder.Ascending;
                }
                base.BeginUpdate();
                try
                {
                    this.Sort(e.Column);
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        protected virtual void HandleColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!this.GetColumn(e.ColumnIndex).FillsFreeSpace)
            {
                this.ResizeFreeSpaceFillingColumns();
            }
        }

        protected virtual void HandleColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (this.UpdateSpaceFillingColumnsWhenDraggingColumnDivider && !this.GetColumn(e.ColumnIndex).FillsFreeSpace)
            {
                int width = this.GetColumn(e.ColumnIndex).Width;
                if (e.NewWidth > width)
                {
                    this.ResizeFreeSpaceFillingColumns(base.ClientSize.Width - (e.NewWidth - width));
                }
                else
                {
                    this.ResizeFreeSpaceFillingColumns();
                }
            }
        }

        protected virtual bool HandleContextMenu(ref Message m)
        {
            if (base.DesignMode)
            {
                return false;
            }
            if (((int) m.LParam) == -1)
            {
                return false;
            }
            if (m.WParam != this.HeaderControl.Handle)
            {
                return false;
            }
            if (!this.PossibleFinishCellEditing())
            {
                return true;
            }
            int columnIndexUnderCursor = this.HeaderControl.ColumnIndexUnderCursor;
            return this.HandleHeaderRightClick(columnIndexUnderCursor);
        }

        protected virtual bool HandleCustomDraw(ref Message m)
        {
            BrightIdeasSoftware.NativeMethods.NMLVCUSTOMDRAW lParam = (BrightIdeasSoftware.NativeMethods.NMLVCUSTOMDRAW) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMLVCUSTOMDRAW));
            if (this.isInWmPaintEvent)
            {
                Graphics graphics;
                if (!this.shouldDoCustomDrawing)
                {
                    return true;
                }
                switch (lParam.nmcd.dwDrawStage)
                {
                    case 1:
                        if (this.prePaintLevel == 0)
                        {
                            this.drawnItems = new List<OLVListItem>();
                        }
                        this.isAfterItemPaint = this.GetItemCount() == 0;
                        this.prePaintLevel++;
                        base.WndProc(ref m);
                        m.Result = (IntPtr) ((((int) m.Result) | 0x10) | 0x40);
                        return true;

                    case 2:
                        this.prePaintLevel--;
                        if ((this.prePaintLevel == 0) && (this.isMarqueSelecting || this.isAfterItemPaint))
                        {
                            this.shouldDoCustomDrawing = false;
                            using (graphics = Graphics.FromHdc(lParam.nmcd.hdc))
                            {
                                this.DrawAllDecorations(graphics, this.drawnItems);
                            }
                        }
                        goto Label_0334;

                    case 3:
                    case 4:
                    case 0x10003:
                    case 0x10004:
                    case 0x30002:
                        goto Label_0334;

                    case 0x10001:
                        this.isAfterItemPaint = true;
                        if (this.View != System.Windows.Forms.View.Tile)
                        {
                            base.WndProc(ref m);
                            break;
                        }
                        if (base.OwnerDraw && (this.ItemRenderer != null))
                        {
                            base.WndProc(ref m);
                        }
                        break;

                    case 0x10002:
                    {
                        OLVListItem item = this.GetItem((int) lParam.nmcd.dwItemSpec);
                        if (item != null)
                        {
                            this.drawnItems.Add(item);
                        }
                        goto Label_0334;
                    }
                    case 0x30001:
                        if (base.OwnerDraw)
                        {
                            if (lParam.iSubItem != 0)
                            {
                                return false;
                            }
                            if (this.Columns[0].DisplayIndex == 0)
                            {
                                return false;
                            }
                            int dwItemSpec = (int) lParam.nmcd.dwItemSpec;
                            OLVListItem item2 = this.GetItem(dwItemSpec);
                            if (item2 == null)
                            {
                                return false;
                            }
                            using (graphics = Graphics.FromHdc(lParam.nmcd.hdc))
                            {
                                Rectangle subItemBounds = item2.GetSubItemBounds(0);
                                DrawListViewSubItemEventArgs e = new DrawListViewSubItemEventArgs(graphics, subItemBounds, item2, item2.SubItems[0], dwItemSpec, 0, this.Columns[0], (ListViewItemStates) lParam.nmcd.uItemState);
                                this.OnDrawSubItem(e);
                            }
                            m.Result = (IntPtr) 4;
                            return true;
                        }
                        return false;

                    default:
                        goto Label_0334;
                }
                m.Result = (IntPtr) ((((int) m.Result) | 0x10) | 0x40);
            }
            return true;
        Label_0334:
            return false;
        }

        protected virtual bool HandleDestroy(ref Message m)
        {
            MethodInvoker method = null;
            base.BeginInvoke(delegate {
                this.headerControl = null;
                this.HeaderControl.WordWrap = this.HeaderWordWrap;
            });
            if (this.cellToolTip == null)
            {
                return false;
            }
            this.cellToolTip.PushSettings();
            base.WndProc(ref m);
            if (method == null)
            {
                method = delegate {
                    this.UpdateCellToolTipHandle();
                    this.cellToolTip.PopSettings();
                };
            }
            base.BeginInvoke(method);
            return true;
        }

        protected virtual bool HandleEndScroll(ref Message m)
        {
            if ((!IsVista && (Control.MouseButtons == MouseButtons.Left)) && base.GridLines)
            {
                base.Invalidate();
                base.Update();
            }
            return false;
        }

        protected virtual bool HandleFindItem(ref Message m)
        {
            BrightIdeasSoftware.NativeMethods.LVFINDINFO lParam = (BrightIdeasSoftware.NativeMethods.LVFINDINFO) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.LVFINDINFO));
            if ((lParam.flags & 2) != 2)
            {
                return false;
            }
            int start = m.WParam.ToInt32();
            m.Result = (IntPtr) this.FindMatchingRow(lParam.psz, start, SearchDirectionHint.Down);
            return true;
        }

        [Obsolete("Use HandleHeaderRightClick(int) instead")]
        protected virtual bool HandleHeaderRightClick()
        {
            return false;
        }

        protected virtual bool HandleHeaderRightClick(int columnIndex)
        {
            ColumnClickEventArgs e = new ColumnClickEventArgs(columnIndex);
            this.OnColumnRightClick(e);
            if (this.ShowCommandMenuOnRightClick)
            {
                this.ShowColumnCommandMenu(columnIndex, Cursor.Position);
                return true;
            }
            if (this.SelectColumnsOnRightClick)
            {
                this.ShowColumnSelectMenu(Cursor.Position);
                return true;
            }
            return false;
        }

        protected virtual void HandleHeaderToolTipShowing(object sender, ToolTipShowingEventArgs e)
        {
            e.ColumnIndex = this.HeaderControl.ColumnIndexUnderCursor;
            if (e.ColumnIndex >= 0)
            {
                e.RowIndex = -1;
                e.Model = null;
                e.Column = this.GetColumn(e.ColumnIndex);
                e.Text = this.GetHeaderToolTip(e.ColumnIndex);
                e.ListView = this;
                this.OnHeaderToolTip(e);
            }
        }

        protected virtual bool HandleKeyDown(ref Message m)
        {
            if ((this.CheckBoxes && (m.WParam.ToInt32() == 0x20)) && (base.SelectedIndices.Count > 0))
            {
                this.ToggleSelectedRowCheckBoxes();
                return true;
            }
            int scrollPosition = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, true);
            int oldValue = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, false);
            base.WndProc(ref m);
            if (!base.IsDisposed)
            {
                ScrollEventArgs args;
                int newValue = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, true);
                int num4 = BrightIdeasSoftware.NativeMethods.GetScrollPosition(this, false);
                if (scrollPosition != newValue)
                {
                    args = new ScrollEventArgs(ScrollEventType.EndScroll, scrollPosition, newValue, ScrollOrientation.HorizontalScroll);
                    this.OnScroll(args);
                    this.RefreshHotItem();
                }
                if (oldValue != num4)
                {
                    args = new ScrollEventArgs(ScrollEventType.EndScroll, oldValue, num4, ScrollOrientation.VerticalScroll);
                    this.OnScroll(args);
                    this.RefreshHotItem();
                }
            }
            return true;
        }

        protected virtual void HandleLayout(object sender, LayoutEventArgs e)
        {
            if (base.Created)
            {
                base.BeginInvoke(new MethodInvoker(this.ResizeFreeSpaceFillingColumns));
            }
        }

        protected virtual bool HandleLButtonDoubleClick(ref Message m)
        {
            int x = m.LParam.ToInt32() & 0xffff;
            int y = (m.LParam.ToInt32() >> 0x10) & 0xffff;
            return this.ProcessLButtonDoubleClick(this.OlvHitTest(x, y));
        }

        protected virtual bool HandleLButtonDown(ref Message m)
        {
            int x = m.LParam.ToInt32() & 0xffff;
            int y = (m.LParam.ToInt32() >> 0x10) & 0xffff;
            return this.ProcessLButtonDown(this.OlvHitTest(x, y));
        }

        protected virtual bool HandleLinkClick(ref Message m)
        {
            BrightIdeasSoftware.NativeMethods.NMLVLINK lParam = (BrightIdeasSoftware.NativeMethods.NMLVLINK) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMLVLINK));
            foreach (OLVGroup group in this.OLVGroups)
            {
                if (group.GroupId == lParam.iSubItem)
                {
                    this.OnGroupTaskClicked(new GroupTaskClickedEventArgs(group));
                    return true;
                }
            }
            return false;
        }

        protected bool HandleNotify(ref Message m)
        {
            OLVColumn column;
            BrightIdeasSoftware.NativeMethods.HDITEM hditem;
            bool flag = false;
            BrightIdeasSoftware.NativeMethods.NMHEADER lParam = (BrightIdeasSoftware.NativeMethods.NMHEADER) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMHEADER));
            int code = lParam.nhdr.code;
            if (code <= -521)
            {
                switch (code)
                {
                    case -522:
                        return this.CellToolTip.HandlePop(ref m);

                    case -521:
                        return this.CellToolTip.HandleShow(ref m);

                    case -530:
                        return this.CellToolTip.HandleGetDispInfo(ref m);
                }
                return flag;
            }
            switch (code)
            {
                case -328:
                case -308:
                    if ((lParam.iItem >= 0) && (lParam.iItem < this.Columns.Count))
                    {
                        hditem = (BrightIdeasSoftware.NativeMethods.HDITEM) Marshal.PtrToStructure(lParam.pHDITEM, typeof(BrightIdeasSoftware.NativeMethods.HDITEM));
                        column = this.GetColumn(lParam.iItem);
                        if (hditem.cxy < column.MinimumWidth)
                        {
                            hditem.cxy = column.MinimumWidth;
                        }
                        else if ((column.MaximumWidth != -1) && (hditem.cxy > column.MaximumWidth))
                        {
                            hditem.cxy = column.MaximumWidth;
                        }
                        Marshal.StructureToPtr(hditem, lParam.pHDITEM, false);
                    }
                    return flag;

                case -327:
                case -324:
                case -323:
                case -321:
                    return flag;

                case -326:
                case -325:
                case -306:
                case -305:
                    if (!this.PossibleFinishCellEditing())
                    {
                        m.Result = (IntPtr) 1;
                        return true;
                    }
                    if (((lParam.iItem >= 0) && (lParam.iItem < this.Columns.Count)) && this.GetColumn(lParam.iItem).FillsFreeSpace)
                    {
                        m.Result = (IntPtr) 1;
                        flag = true;
                    }
                    return flag;

                case -322:
                case -302:
                    if (!this.PossibleFinishCellEditing())
                    {
                        m.Result = (IntPtr) 1;
                        flag = true;
                    }
                    return flag;

                case -320:
                case -300:
                    lParam = (BrightIdeasSoftware.NativeMethods.NMHEADER) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMHEADER));
                    if ((lParam.iItem >= 0) && (lParam.iItem < this.Columns.Count))
                    {
                        hditem = (BrightIdeasSoftware.NativeMethods.HDITEM) Marshal.PtrToStructure(lParam.pHDITEM, typeof(BrightIdeasSoftware.NativeMethods.HDITEM));
                        column = this.GetColumn(lParam.iItem);
                        if ((hditem.mask & 1) != 1)
                        {
                            return flag;
                        }
                        if ((hditem.cxy < column.MinimumWidth) || ((column.MaximumWidth != -1) && (hditem.cxy > column.MaximumWidth)))
                        {
                            m.Result = (IntPtr) 1;
                            flag = true;
                        }
                    }
                    return flag;

                case -307:
                case -304:
                case -303:
                case -301:
                    return flag;

                case -12:
                    if (!this.OwnerDrawnHeader)
                    {
                        flag = this.HeaderControl.HandleHeaderCustomDraw(ref m);
                    }
                    return flag;
            }
            return flag;
        }

        protected virtual bool HandlePaint(ref Message m)
        {
            this.isInWmPaintEvent = true;
            this.shouldDoCustomDrawing = true;
            this.prePaintLevel = 0;
            this.ShowOverlays();
            this.HandlePrePaint();
            base.WndProc(ref m);
            this.HandlePostPaint();
            this.isInWmPaintEvent = false;
            return true;
        }

        protected virtual void HandlePostPaint()
        {
        }

        protected virtual void HandlePrePaint()
        {
            this.lastUpdateRectangle = BrightIdeasSoftware.NativeMethods.GetUpdateRect(this);
        }

        protected virtual bool HandleReflectNotify(ref Message m)
        {
            bool flag = false;
            BrightIdeasSoftware.NativeMethods.NMHDR lParam = (BrightIdeasSoftware.NativeMethods.NMHDR) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMHDR));
            int code = lParam.code;
            if (code <= -100)
            {
                CheckState state;
                CheckState state2;
                switch (code)
                {
                    case -184:
                        return this.HandleLinkClick(ref m);

                    case -183:
                    case -182:
                        return flag;

                    case -181:
                        return this.HandleEndScroll(ref m);

                    case -180:
                        return this.HandleBeginScroll(ref m);

                    case -156:
                        this.isMarqueSelecting = true;
                        return flag;

                    case -101:
                    {
                        BrightIdeasSoftware.NativeMethods.NMLISTVIEW structure = (BrightIdeasSoftware.NativeMethods.NMLISTVIEW) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMLISTVIEW));
                        if ((structure.uChanged & 8) != 0)
                        {
                            state = this.CalculateState(structure.uOldState);
                            state2 = this.CalculateState(structure.uNewState);
                            if (state != state2)
                            {
                                structure.uOldState &= 0xfff;
                                structure.uNewState &= 0xfff;
                                Marshal.StructureToPtr(structure, m.LParam, false);
                            }
                        }
                        return flag;
                    }
                    case -100:
                    {
                        BrightIdeasSoftware.NativeMethods.NMLISTVIEW nmlistview2 = (BrightIdeasSoftware.NativeMethods.NMLISTVIEW) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMLISTVIEW));
                        if ((nmlistview2.uChanged & 8) != 0)
                        {
                            state = this.CalculateState(nmlistview2.uOldState);
                            state2 = this.CalculateState(nmlistview2.uNewState);
                            if (state != state2)
                            {
                                nmlistview2.uChanged &= -9;
                                Marshal.StructureToPtr(nmlistview2, m.LParam, false);
                            }
                        }
                        return flag;
                    }
                }
                return flag;
            }
            switch (code)
            {
                case -16:
                    this.isMarqueSelecting = false;
                    base.Invalidate();
                    return flag;

                case -12:
                    return this.HandleCustomDraw(ref m);

                case -3:
                    if (this.CheckBoxes)
                    {
                        lParam.code = -6;
                        Marshal.StructureToPtr(lParam, m.LParam, false);
                    }
                    return flag;
            }
            return flag;
        }

        protected virtual bool HandleWindowPosChanging(ref Message m)
        {
            BrightIdeasSoftware.NativeMethods.WINDOWPOS lParam = (BrightIdeasSoftware.NativeMethods.WINDOWPOS) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.WINDOWPOS));
            if (((lParam.flags & 1) == 0) && (lParam.cx < base.Bounds.Width))
            {
                this.ResizeFreeSpaceFillingColumns(lParam.cx - (base.Bounds.Width - base.ClientSize.Width));
            }
            return false;
        }

        public virtual bool HasDecoration(IDecoration decoration)
        {
            return this.Decorations.Contains(decoration);
        }

        public virtual bool HasOverlay(IOverlay overlay)
        {
            return this.Overlays.Contains(overlay);
        }

        internal void headerToolTip_Showing(object sender, ToolTipShowingEventArgs e)
        {
            this.HandleHeaderToolTipShowing(sender, e);
        }

        public virtual void HideOverlays()
        {
            foreach (GlassPanelForm form in this.glassPanels)
            {
                form.HideGlass();
            }
        }

        public ListViewHitTestInfo HitTest(int x, int y)
        {
            try
            {
                return base.HitTest(x, y);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ListViewHitTestInfo(null, null, ListViewHitTestLocations.None);
            }
        }

        public virtual int IndexOf(object modelObject)
        {
            for (int i = 0; i < this.GetItemCount(); i++)
            {
                if (this.GetModelObject(i) == modelObject)
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual void InitializeCheckBoxImages()
        {
            if (!base.DesignMode)
            {
                ImageList smallImageList = this.SmallImageList;
                if (smallImageList == null)
                {
                    smallImageList = new ImageList {
                        ImageSize = new Size(0x10, 0x10)
                    };
                }
                this.AddCheckStateBitmap(smallImageList, "checkbox-checked", CheckBoxState.CheckedNormal);
                this.AddCheckStateBitmap(smallImageList, "checkbox-unchecked", CheckBoxState.UncheckedNormal);
                this.AddCheckStateBitmap(smallImageList, "checkbox-indeterminate", CheckBoxState.MixedNormal);
                this.SmallImageList = smallImageList;
            }
        }

        protected virtual void InitializeEmptyListMsgOverlay()
        {
            TextOverlay overlay = new TextOverlay {
                Alignment = System.Drawing.ContentAlignment.MiddleCenter,
                TextColor = SystemColors.ControlDarkDark,
                BackColor = Color.BlanchedAlmond,
                BorderColor = SystemColors.ControlDark,
                BorderWidth = 2f
            };
            this.EmptyListMsgOverlay = overlay;
        }

        protected virtual void InitializeStandardOverlays()
        {
            this.OverlayImage = new ImageOverlay();
            this.AddOverlay(this.OverlayImage);
            this.OverlayText = new TextOverlay();
            this.AddOverlay(this.OverlayText);
        }

        protected virtual void InitializeStateImageList()
        {
            if (!base.DesignMode)
            {
                if (base.StateImageList == null)
                {
                    base.StateImageList = new ImageList();
                    base.StateImageList.ImageSize = new Size(0x10, 0x10);
                }
                if (((this.RowHeight != -1) && (this.View == System.Windows.Forms.View.Details)) && (base.StateImageList.ImageSize.Height != this.RowHeight))
                {
                    base.StateImageList = new ImageList();
                    base.StateImageList.ImageSize = new Size(0x10, this.RowHeight);
                }
                if (base.StateImageList.Images.Count == 0)
                {
                    this.AddCheckStateBitmap(base.StateImageList, "checkbox-unchecked", CheckBoxState.UncheckedNormal);
                }
                if (base.StateImageList.Images.Count <= 1)
                {
                    this.AddCheckStateBitmap(base.StateImageList, "checkbox-checked", CheckBoxState.CheckedNormal);
                }
                if (this.TriStateCheckBoxes && (base.StateImageList.Images.Count <= 2))
                {
                    this.AddCheckStateBitmap(base.StateImageList, "checkbox-indeterminate", CheckBoxState.MixedNormal);
                }
                else if (base.StateImageList.Images.ContainsKey("checkbox-indeterminate"))
                {
                    base.StateImageList.Images.RemoveByKey("checkbox-indeterminate");
                }
            }
        }

        public virtual void InsertObjects(int index, ICollection modelObjects)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.InsertObjects(index, modelObjects);
                    };
                }
                base.Invoke(method);
            }
            else if (modelObjects != null)
            {
                base.BeginUpdate();
                try
                {
                    ItemsAddingEventArgs e = new ItemsAddingEventArgs(modelObjects);
                    this.OnItemsAdding(e);
                    if (!e.Canceled)
                    {
                        OLVListItem item;
                        modelObjects = e.ObjectsToAdd;
                        base.ListViewItemSorter = null;
                        this.TakeOwnershipOfObjects();
                        ArrayList objects = (ArrayList) this.Objects;
                        index = Math.Max(0, Math.Min(index, this.GetItemCount()));
                        int num = index;
                        foreach (object obj2 in modelObjects)
                        {
                            if (obj2 != null)
                            {
                                objects.Insert(num, obj2);
                                item = new OLVListItem(obj2);
                                this.FillInValues(item, obj2);
                                base.Items.Insert(num, item);
                                num++;
                            }
                        }
                        for (num = index; num < this.GetItemCount(); num++)
                        {
                            item = this.GetItem(num);
                            this.SetSubItemImages(item.Index, item);
                        }
                        if (((this.LastSortColumn == null) && this.UseAlternatingBackColors) && (this.View == System.Windows.Forms.View.Details))
                        {
                            this.PrepareAlternateBackColors();
                        }
                        this.OnItemsChanged(new ItemsChangedEventArgs());
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        public virtual bool IsChecked(object modelObject)
        {
            OLVListItem item = this.ModelToItem(modelObject);
            if (item == null)
            {
                return false;
            }
            return (item.CheckState == CheckState.Checked);
        }

        public virtual bool IsCheckedIndeterminate(object modelObject)
        {
            OLVListItem item = this.ModelToItem(modelObject);
            if (item == null)
            {
                return false;
            }
            return (item.CheckState == CheckState.Indeterminate);
        }

        public bool IsSelected(object model)
        {
            OLVListItem item = this.ModelToItem(model);
            if (item == null)
            {
                return false;
            }
            return item.Selected;
        }

        public virtual bool IsSubItemChecked(object rowObject, OLVColumn column)
        {
            return ((((column != null) && (rowObject != null)) && column.CheckBoxes) && (column.GetCheckState(rowObject) == CheckState.Checked));
        }

        public virtual bool IsUrlVisited(string url)
        {
            return this.visitedUrlMap.ContainsKey(url);
        }

        internal void LowLevelScroll(int dx, int dy)
        {
            BrightIdeasSoftware.NativeMethods.Scroll(this, dx, dy);
        }

        public virtual ToolStripDropDown MakeColumnCommandMenu(ToolStripDropDown strip, int columnIndex)
        {
            EventHandler onClick = null;
            EventHandler handler2 = null;
            EventHandler handler3 = null;
            EventHandler handler4 = null;
            EventHandler handler5 = null;
            EventHandler handler6 = null;
            EventHandler handler7 = null;
            OLVColumn column = this.GetColumn(columnIndex);
            if (column != null)
            {
                string str = string.Format(this.MenuLabelSortAscending, column.Text);
                if (!string.IsNullOrEmpty(str))
                {
                    if (onClick == null)
                    {
                        onClick = delegate (object sender, EventArgs args) {
                            this.Sort(column, SortOrder.Ascending);
                        };
                    }
                    strip.Items.Add(str, null, onClick);
                }
                str = string.Format(this.MenuLabelSortDescending, column.Text);
                if (!string.IsNullOrEmpty(str))
                {
                    if (handler2 == null)
                    {
                        handler2 = delegate (object sender, EventArgs args) {
                            this.Sort(column, SortOrder.Descending);
                        };
                    }
                    strip.Items.Add(str, null, handler2);
                }
                if (this.CanShowGroups)
                {
                    str = string.Format(this.MenuLabelGroupBy, column.Text);
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (handler3 == null)
                        {
                            handler3 = delegate (object sender, EventArgs args) {
                                this.ShowGroups = true;
                                this.PrimarySortColumn = column;
                                this.PrimarySortOrder = SortOrder.Ascending;
                                this.BuildList();
                            };
                        }
                        strip.Items.Add(str, null, handler3);
                    }
                }
                if (this.ShowGroups)
                {
                    if (this.AlwaysGroupByColumn == column)
                    {
                        str = string.Format(this.MenuLabelUnlockGroupingOn, column.Text);
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (handler4 == null)
                            {
                                handler4 = delegate (object sender, EventArgs args) {
                                    this.AlwaysGroupByColumn = null;
                                    this.AlwaysGroupBySortOrder = SortOrder.Ascending;
                                    this.BuildList();
                                };
                            }
                            strip.Items.Add(str, null, handler4);
                        }
                    }
                    else
                    {
                        str = string.Format(this.MenuLabelLockGroupingOn, column.Text);
                        if (!string.IsNullOrEmpty(str))
                        {
                            if (handler5 == null)
                            {
                                handler5 = delegate (object sender, EventArgs args) {
                                    this.ShowGroups = true;
                                    this.AlwaysGroupByColumn = column;
                                    this.AlwaysGroupBySortOrder = SortOrder.Ascending;
                                    this.BuildList();
                                };
                            }
                            strip.Items.Add(str, null, handler5);
                        }
                    }
                    str = string.Format(this.MenuLabelTurnOffGroups, column.Text);
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (handler6 == null)
                        {
                            handler6 = delegate (object sender, EventArgs args) {
                                this.ShowGroups = false;
                                this.BuildList();
                            };
                        }
                        strip.Items.Add(str, null, handler6);
                    }
                    return strip;
                }
                str = string.Format(this.MenuLabelUnsort, column.Text);
                if (!string.IsNullOrEmpty(str))
                {
                    if (handler7 == null)
                    {
                        handler7 = delegate (object sender, EventArgs args) {
                            this.ShowGroups = false;
                            this.PrimarySortColumn = null;
                            this.PrimarySortOrder = SortOrder.None;
                            this.BuildList();
                        };
                    }
                    strip.Items.Add(str, null, handler7);
                }
            }
            return strip;
        }

        public virtual ToolStripDropDown MakeColumnSelectMenu(ToolStripDropDown strip)
        {
            strip.ItemClicked += new ToolStripItemClickedEventHandler(this.ColumnSelectMenu_ItemClicked);
            strip.Closing += new ToolStripDropDownClosingEventHandler(this.ColumnSelectMenu_Closing);
            List<OLVColumn> list = new List<OLVColumn>(this.AllColumns);
            if ((this.AllColumns.Count > 0) && (this.AllColumns[0].LastDisplayIndex == -1))
            {
                this.RememberDisplayIndicies();
            }
            list.Sort((x, y) => x.LastDisplayIndex - y.LastDisplayIndex);
            foreach (OLVColumn column in list)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(column.Text) {
                    Checked = column.IsVisible,
                    Tag = column,
                    Enabled = !column.IsVisible ? true : (column.Index > 0)
                };
                strip.Items.Add(item);
            }
            return strip;
        }

        protected virtual Control MakeDefaultCellEditor(OLVColumn column)
        {
            System.Windows.Forms.TextBox tb = new System.Windows.Forms.TextBox();
            if (column.AutoCompleteEditor)
            {
                this.ConfigureAutoComplete(tb, column);
            }
            return tb;
        }

        protected virtual IList<OLVGroup> MakeGroups(GroupingParameters parms)
        {
            NullableDictionary<object, List<OLVListItem>> dictionary = new NullableDictionary<object, List<OLVListItem>>();
            foreach (OLVListItem item in parms.ListView.Items)
            {
                object groupKey = parms.GroupByColumn.GetGroupKey(item.RowObject);
                if (!dictionary.ContainsKey(groupKey))
                {
                    dictionary[groupKey] = new List<OLVListItem>();
                }
                dictionary[groupKey].Add(item);
            }
            OLVColumn col = parms.SortItemsByPrimaryColumn ? parms.ListView.GetColumn(0) : parms.PrimarySort;
            ColumnComparer comparer = new ColumnComparer(col, parms.PrimarySortOrder, parms.SecondarySort, parms.SecondarySortOrder);
            foreach (object obj2 in dictionary.Keys)
            {
                dictionary[obj2].Sort(comparer);
            }
            List<OLVGroup> list = new List<OLVGroup>();
            foreach (object obj2 in dictionary.Keys)
            {
                string str = parms.GroupByColumn.ConvertGroupKeyToTitle(obj2);
                if (!string.IsNullOrEmpty(parms.TitleFormat))
                {
                    int count = dictionary[obj2].Count;
                    str = string.Format((count == 1) ? parms.TitleSingularFormat : parms.TitleFormat, str, count);
                }
                OLVGroup group = new OLVGroup(str) {
                    Collapsible = this.HasCollapsibleGroups,
                    Key = obj2,
                    SortValue = obj2 as IComparable,
                    Items = dictionary[obj2]
                };
                if (parms.GroupByColumn.GroupFormatter != null)
                {
                    parms.GroupByColumn.GroupFormatter(group, parms);
                }
                list.Add(group);
            }
            list.Sort(new OLVGroupComparer(parms.PrimarySortOrder));
            return list;
        }

        private Bitmap MakeResizedImage(int width, int height, Image image, Color transparent)
        {
            Bitmap bitmap = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(transparent);
            int x = Math.Max(0, (bitmap.Size.Width - image.Size.Width) / 2);
            int y = Math.Max(0, (bitmap.Size.Height - image.Size.Height) / 2);
            graphics.DrawImage(image, x, y, image.Size.Width, image.Size.Height);
            return bitmap;
        }

        private ImageList MakeResizedImageList(int width, int height, ImageList source)
        {
            ImageList list = new ImageList {
                ImageSize = new Size(width, height)
            };
            if (source != null)
            {
                list.TransparentColor = source.TransparentColor;
                list.ColorDepth = source.ColorDepth;
                for (int i = 0; i < source.Images.Count; i++)
                {
                    Bitmap bitmap = this.MakeResizedImage(width, height, source.Images[i], source.TransparentColor);
                    list.Images.Add(bitmap);
                }
                foreach (string str in source.Images.Keys)
                {
                    list.Images.SetKeyName(source.Images.IndexOfKey(str), str);
                }
            }
            return list;
        }

        protected virtual void MakeSortIndicatorImages()
        {
            if (!base.DesignMode)
            {
                Point point;
                Point point2;
                Point point3;
                Point[] pointArray;
                ImageList smallImageList = this.SmallImageList;
                if (smallImageList == null)
                {
                    smallImageList = new ImageList {
                        ImageSize = new Size(0x10, 0x10)
                    };
                }
                int x = smallImageList.ImageSize.Width / 2;
                int num2 = (smallImageList.ImageSize.Height / 2) - 1;
                int num3 = x - 2;
                int num4 = num3 / 2;
                if (smallImageList.Images.IndexOfKey("sort-indicator-up") == -1)
                {
                    point = new Point(x - num3, num2 + num4);
                    point2 = new Point(x, (num2 - num4) - 1);
                    point3 = new Point(x + num3, num2 + num4);
                    pointArray = new Point[] { point, point2, point3 };
                    smallImageList.Images.Add("sort-indicator-up", this.MakeTriangleBitmap(smallImageList.ImageSize, pointArray));
                }
                if (smallImageList.Images.IndexOfKey("sort-indicator-down") == -1)
                {
                    point = new Point(x - num3, num2 - num4);
                    point2 = new Point(x, num2 + num4);
                    point3 = new Point(x + num3, num2 - num4);
                    pointArray = new Point[] { point, point2, point3 };
                    smallImageList.Images.Add("sort-indicator-down", this.MakeTriangleBitmap(smallImageList.ImageSize, pointArray));
                }
                this.SmallImageList = smallImageList;
            }
        }

        private OLVListSubItem MakeSubItem(object rowObject, OLVColumn column)
        {
            OLVListSubItem item = new OLVListSubItem(column.GetStringValue(rowObject), column.GetImage(rowObject));
            if (this.UseHyperlinks && column.Hyperlink)
            {
                IsHyperlinkEventArgs e = new IsHyperlinkEventArgs {
                    ListView = this,
                    Model = rowObject,
                    Column = column,
                    Text = item.Text,
                    Url = item.Text
                };
                this.OnIsHyperlink(e);
                item.Url = e.Url;
            }
            return item;
        }

        private Bitmap MakeTriangleBitmap(Size sz, Point[] pts)
        {
            Bitmap image = new Bitmap(sz.Width, sz.Height);
            Graphics.FromImage(image).FillPolygon(new SolidBrush(Color.Gray), pts);
            return image;
        }

        public virtual void MarkUrlVisited(string url)
        {
            this.visitedUrlMap[url] = true;
        }

        public virtual OLVListItem ModelToItem(object modelObject)
        {
            if (modelObject != null)
            {
                foreach (OLVListItem item in base.Items)
                {
                    if ((item.RowObject != null) && item.RowObject.Equals(modelObject))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public virtual void MoveObjects(int index, ICollection modelObjects)
        {
            this.TakeOwnershipOfObjects();
            ArrayList objects = (ArrayList) this.Objects;
            List<int> list2 = new List<int>();
            foreach (object obj2 in modelObjects)
            {
                if (obj2 != null)
                {
                    int item = this.IndexOf(obj2);
                    if (item >= 0)
                    {
                        list2.Add(item);
                        objects.Remove(obj2);
                        if (item <= index)
                        {
                            index--;
                        }
                    }
                }
            }
            list2.Sort();
            list2.Reverse();
            try
            {
                base.BeginUpdate();
                foreach (int num in list2)
                {
                    base.Items.RemoveAt(num);
                }
                this.InsertObjects(index, modelObjects);
            }
            finally
            {
                base.EndUpdate();
            }
        }

        public virtual OlvListViewHitTestInfo OlvHitTest(int x, int y)
        {
            ListViewHitTestInfo hti = this.HitTest(x, y);
            OlvListViewHitTestInfo info2 = new OlvListViewHitTestInfo(hti);
            if (((hti.Item == null) && !base.FullRowSelect) && (this.View == System.Windows.Forms.View.Details))
            {
                Point scrolledColumnSides = BrightIdeasSoftware.NativeMethods.GetScrolledColumnSides(this, 0);
                if ((x >= scrolledColumnSides.X) && (x <= scrolledColumnSides.Y))
                {
                    hti = this.HitTest(scrolledColumnSides.Y + 4, y);
                    if (hti.Item == null)
                    {
                        hti = this.HitTest(scrolledColumnSides.X - 4, y);
                    }
                    if (hti.Item == null)
                    {
                        hti = this.HitTest(4, y);
                    }
                    if (hti.Item != null)
                    {
                        info2.Item = (OLVListItem) hti.Item;
                        info2.SubItem = info2.Item.GetSubItem(0);
                        info2.Location = ListViewHitTestLocations.None;
                        info2.HitTestLocation = HitTestLocation.InCell;
                    }
                }
            }
            if (base.OwnerDraw)
            {
                this.CalculateOwnerDrawnHitTest(info2, x, y);
                return info2;
            }
            this.CalculateStandardHitTest(info2, x, y);
            return info2;
        }

        protected virtual void OnAboutToCreateGroups(CreateGroupsEventArgs e)
        {
            if (this.AboutToCreateGroups != null)
            {
                this.AboutToCreateGroups(this, e);
            }
        }

        protected virtual void OnAfterCreatingGroups(CreateGroupsEventArgs e)
        {
            if (this.AfterCreatingGroups != null)
            {
                this.AfterCreatingGroups(this, e);
            }
        }

        protected virtual void OnAfterSearching(AfterSearchingEventArgs e)
        {
            if (this.AfterSearching != null)
            {
                this.AfterSearching(this, e);
            }
        }

        protected virtual void OnAfterSorting(AfterSortingEventArgs e)
        {
            if (this.AfterSorting != null)
            {
                this.AfterSorting(this, e);
            }
        }

        protected virtual void OnBeforeCreatingGroups(CreateGroupsEventArgs e)
        {
            if (this.BeforeCreatingGroups != null)
            {
                this.BeforeCreatingGroups(this, e);
            }
        }

        protected virtual void OnBeforeSearching(BeforeSearchingEventArgs e)
        {
            if (this.BeforeSearching != null)
            {
                this.BeforeSearching(this, e);
            }
        }

        protected virtual void OnBeforeSorting(BeforeSortingEventArgs e)
        {
            if (this.BeforeSorting != null)
            {
                this.BeforeSorting(this, e);
            }
        }

        protected virtual void OnCanDrop(OlvDropEventArgs args)
        {
            if (this.CanDrop != null)
            {
                this.CanDrop(this, args);
            }
        }

        protected virtual void OnCellClick(CellClickEventArgs args)
        {
            if (this.CellClick != null)
            {
                this.CellClick(this, args);
            }
        }

        protected virtual void OnCellEditFinishing(CellEditEventArgs e)
        {
            if (this.CellEditFinishing != null)
            {
                this.CellEditFinishing(this, e);
            }
        }

        protected virtual void OnCellEditorValidating(CellEditEventArgs e)
        {
            if ((Environment.TickCount - this.lastValidatingEvent) < 500)
            {
                e.Cancel = true;
            }
            else
            {
                this.lastValidatingEvent = Environment.TickCount;
                if (this.CellEditValidating != null)
                {
                    this.CellEditValidating(this, e);
                }
            }
            this.lastValidatingEvent = Environment.TickCount;
        }

        protected virtual void OnCellEditStarting(CellEditEventArgs e)
        {
            if (this.CellEditStarting != null)
            {
                this.CellEditStarting(this, e);
            }
        }

        protected virtual void OnCellOver(CellOverEventArgs args)
        {
            if (this.CellOver != null)
            {
                this.CellOver(this, args);
            }
        }

        protected virtual void OnCellRightClick(CellRightClickEventArgs args)
        {
            if (this.CellRightClick != null)
            {
                this.CellRightClick(this, args);
            }
        }

        protected virtual void OnCellToolTip(ToolTipShowingEventArgs args)
        {
            if (this.CellToolTipShowing != null)
            {
                this.CellToolTipShowing(this, args);
            }
        }

        protected override void OnColumnReordered(ColumnReorderedEventArgs e)
        {
            base.OnColumnReordered(e);
            base.BeginInvoke(new MethodInvoker(this.RememberDisplayIndicies));
        }

        protected virtual void OnColumnRightClick(ColumnClickEventArgs e)
        {
            if (this.ColumnRightClick != null)
            {
                this.ColumnRightClick(this, e);
            }
        }

        protected virtual void OnControlCreated()
        {
            this.HeaderControl.WordWrap = this.HeaderWordWrap;
            this.HotItemStyle = this.HotItemStyle;
            BrightIdeasSoftware.NativeMethods.SetGroupImageList(this, this.GroupImageList);
            this.UseExplorerTheme = this.UseExplorerTheme;
        }

        protected override void OnDragDrop(DragEventArgs args)
        {
            base.OnDragDrop(args);
            if (this.DropSink != null)
            {
                this.DropSink.Drop(args);
            }
        }

        protected override void OnDragEnter(DragEventArgs args)
        {
            base.OnDragEnter(args);
            if (this.DropSink != null)
            {
                this.DropSink.Enter(args);
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            if (this.DropSink != null)
            {
                this.DropSink.Leave();
            }
        }

        protected override void OnDragOver(DragEventArgs args)
        {
            base.OnDragOver(args);
            if (this.DropSink != null)
            {
                this.DropSink.Over(args);
            }
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            if (this.View == System.Windows.Forms.View.Details)
            {
                e.DrawDefault = false;
            }
            else if (this.ItemRenderer == null)
            {
                e.DrawDefault = true;
            }
            else
            {
                object rowObject = ((OLVListItem) e.Item).RowObject;
                e.DrawDefault = !this.ItemRenderer.RenderItem(e, e.Graphics, e.Bounds, rowObject);
            }
            if (e.DrawDefault)
            {
                base.OnDrawItem(e);
            }
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            if (base.DesignMode)
            {
                e.DrawDefault = true;
            }
            else
            {
                Rectangle bounds = e.Bounds;
                if (bounds.IntersectsWith(this.lastUpdateRectangle))
                {
                    IRenderer renderer = this.GetColumn(e.ColumnIndex).Renderer ?? this.DefaultRenderer;
                    Graphics g = e.Graphics;
                    BufferedGraphics graphics2 = null;
                    if (true)
                    {
                        graphics2 = BufferedGraphicsManager.Current.Allocate(e.Graphics, bounds);
                        g = graphics2.Graphics;
                    }
                    g.TextRenderingHint = TextRendereringHint;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    e.DrawDefault = !renderer.RenderSubItem(e, g, bounds, ((OLVListItem) e.Item).RowObject);
                    if (graphics2 != null)
                    {
                        if (!e.DrawDefault)
                        {
                            graphics2.Render();
                        }
                        graphics2.Dispose();
                    }
                }
            }
        }

        protected virtual void OnDropped(OlvDropEventArgs args)
        {
            if (this.Dropped != null)
            {
                this.Dropped(this, args);
            }
        }

        protected virtual void OnFormatCell(FormatCellEventArgs args)
        {
            if (this.FormatCell != null)
            {
                this.FormatCell(this, args);
            }
        }

        protected virtual void OnFormatRow(FormatRowEventArgs args)
        {
            if (this.FormatRow != null)
            {
                this.FormatRow(this, args);
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs args)
        {
            base.OnGiveFeedback(args);
            if (this.DropSink != null)
            {
                this.DropSink.GiveFeedback(args);
            }
        }

        protected virtual void OnGroupTaskClicked(GroupTaskClickedEventArgs e)
        {
            if (this.GroupTaskClicked != null)
            {
                this.GroupTaskClicked(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.Invoke(new MethodInvoker(this.OnControlCreated));
        }

        protected virtual void OnHeaderToolTip(ToolTipShowingEventArgs args)
        {
            if (this.HeaderToolTipShowing != null)
            {
                this.HeaderToolTipShowing(this, args);
            }
        }

        protected virtual void OnHotItemChanged(HotItemChangedEventArgs e)
        {
            if (this.HotItemChanged != null)
            {
                this.HotItemChanged(this, e);
            }
        }

        protected virtual void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
        {
            if (this.HyperlinkClicked != null)
            {
                this.HyperlinkClicked(this, e);
            }
        }

        protected virtual void OnIsHyperlink(IsHyperlinkEventArgs e)
        {
            if (this.IsHyperlink != null)
            {
                this.IsHyperlink(this, e);
            }
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            if (this.DragSource != null)
            {
                object data = this.DragSource.StartDrag(this, e.Button, (OLVListItem) e.Item);
                if (data != null)
                {
                    DragDropEffects effect = base.DoDragDrop(data, this.DragSource.GetAllowedEffects(data));
                    this.DragSource.EndDrag(data, effect);
                }
            }
        }

        protected virtual void OnItemsAdding(ItemsAddingEventArgs e)
        {
            if (this.ItemsAdding != null)
            {
                this.ItemsAdding(this, e);
            }
        }

        protected virtual void OnItemsChanged(ItemsChangedEventArgs e)
        {
            if (this.ItemsChanged != null)
            {
                this.ItemsChanged(this, e);
            }
        }

        protected virtual void OnItemsChanging(ItemsChangingEventArgs e)
        {
            if (this.ItemsChanging != null)
            {
                this.ItemsChanging(this, e);
            }
        }

        protected virtual void OnItemsRemoving(ItemsRemovingEventArgs e)
        {
            if (this.ItemsRemoving != null)
            {
                this.ItemsRemoving(this, e);
            }
        }

        protected virtual void OnModelCanDrop(ModelDropEventArgs args)
        {
            if (this.ModelCanDrop != null)
            {
                this.ModelCanDrop(this, args);
            }
        }

        protected virtual void OnModelDropped(ModelDropEventArgs args)
        {
            if (this.ModelDropped != null)
            {
                this.ModelDropped(this, args);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            this.lastMouseDownClickCount = e.Clicks;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.UpdateHotItem(new Point(-1, -1));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            CellOverEventArgs args = new CellOverEventArgs();
            this.BuildCellEvent(args, e.Location);
            this.OnCellOver(args);
            if (!args.Handled)
            {
                this.UpdateHotItem(args.HitTest);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
            {
                this.OnRightMouseUp(e);
            }
            else
            {
                CellClickEventArgs args = new CellClickEventArgs();
                this.BuildCellEvent(args, e.Location);
                args.ClickCount = this.lastMouseDownClickCount;
                this.OnCellClick(args);
                if (!args.Handled)
                {
                    if (!(((!this.UseHyperlinks || (args.HitTest.HitTestLocation != HitTestLocation.Text)) || (args.SubItem == null)) || string.IsNullOrEmpty(args.SubItem.Url)))
                    {
                        this.ProcessHyperlinkClicked(args);
                    }
                    if ((this.ShouldStartCellEdit(e) && (args.HitTest.HitTestLocation != HitTestLocation.Nothing)) && ((this.CellEditActivation != CellEditActivateMode.SingleClick) || (args.ColumnIndex > 0)))
                    {
                        this.EditSubItem(args.Item, args.ColumnIndex);
                    }
                }
            }
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs args)
        {
            base.OnQueryContinueDrag(args);
            if (this.DropSink != null)
            {
                this.DropSink.QueryContinue(args);
            }
        }

        protected virtual void OnRightMouseUp(MouseEventArgs e)
        {
            CellRightClickEventArgs args = new CellRightClickEventArgs();
            this.BuildCellEvent(args, e.Location);
            this.OnCellRightClick(args);
            if (!args.Handled && (args.MenuStrip != null))
            {
                args.MenuStrip.Show(this, args.Location);
            }
        }

        protected virtual void OnScroll(ScrollEventArgs e)
        {
            if (this.Scroll != null)
            {
                this.Scroll(this, e);
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (!this.hasIdleHandler)
            {
                this.hasIdleHandler = true;
                Application.Idle += new EventHandler(this.HandleApplicationIdle);
            }
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, e);
            }
        }

        public virtual void PauseAnimations(bool isPause)
        {
            for (int i = 0; i < this.Columns.Count; i++)
            {
                OLVColumn column = this.GetColumn(i);
                if (column.Renderer is ImageRenderer)
                {
                    ((ImageRenderer) column.Renderer).Paused = isPause;
                }
            }
        }

        public virtual bool PossibleFinishCellEditing()
        {
            if (this.IsCellEditing)
            {
                this.cellEditEventArgs.Cancel = false;
                this.OnCellEditorValidating(this.cellEditEventArgs);
                if (this.cellEditEventArgs.Cancel)
                {
                    return false;
                }
                this.FinishCellEdit();
            }
            return true;
        }

        protected virtual void PostProcessOneRow(int rowIndex, int displayIndex, OLVListItem olvi)
        {
            if (this.UseAlternatingBackColors)
            {
                if ((displayIndex % 2) == 1)
                {
                    olvi.BackColor = this.AlternateRowBackColorOrDefault;
                }
                else
                {
                    olvi.BackColor = this.BackColor;
                }
            }
            if (!(!this.ShowImagesOnSubItems || base.VirtualMode))
            {
                this.SetSubItemImages(rowIndex, olvi);
            }
            if (this.UseHyperlinks)
            {
                this.ApplyHyperlinkStyle(rowIndex, olvi);
            }
            this.TriggerFormatRowEvent(rowIndex, displayIndex, olvi);
        }

        protected virtual void PostProcessRows()
        {
            int count = base.Items.Count;
            int displayIndex = 0;
            if (this.ShowGroups)
            {
                foreach (ListViewGroup group in base.Groups)
                {
                    foreach (OLVListItem item in group.Items)
                    {
                        this.PostProcessOneRow(item.Index, displayIndex, item);
                        displayIndex++;
                    }
                }
            }
            else
            {
                foreach (OLVListItem item in base.Items)
                {
                    this.PostProcessOneRow(item.Index, displayIndex, item);
                    displayIndex++;
                }
            }
        }

        protected virtual void PrepareAlternateBackColors()
        {
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((((keyData & Keys.KeyCode) == Keys.Tab) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)) && this.IsCellEditing)
            {
                if (!this.PossibleFinishCellEditing())
                {
                    return true;
                }
                if (this.View != System.Windows.Forms.View.Details)
                {
                    return true;
                }
                OLVListItem listViewItem = this.cellEditEventArgs.ListViewItem;
                int subItemIndex = this.cellEditEventArgs.SubItemIndex;
                int displayIndex = this.GetColumn(subItemIndex).DisplayIndex;
                List<OLVColumn> columnsInDisplayOrder = this.ColumnsInDisplayOrder;
                do
                {
                    if ((keyData & Keys.Shift) == Keys.Shift)
                    {
                        displayIndex = ((listViewItem.SubItems.Count + displayIndex) - 1) % listViewItem.SubItems.Count;
                    }
                    else
                    {
                        displayIndex = (displayIndex + 1) % listViewItem.SubItems.Count;
                    }
                }
                while (!columnsInDisplayOrder[displayIndex].IsEditable);
                subItemIndex = columnsInDisplayOrder[displayIndex].Index;
                if (this.cellEditEventArgs.SubItemIndex != subItemIndex)
                {
                    this.StartCellEdit(listViewItem, subItemIndex);
                    return true;
                }
            }
            if (!((keyData != Keys.F2) || this.IsCellEditing))
            {
                this.EditSubItem((OLVListItem) base.FocusedItem, 0);
                return true;
            }
            if (((keyData == Keys.Return) || (keyData == Keys.Return)) && this.IsCellEditing)
            {
                this.PossibleFinishCellEditing();
                return true;
            }
            if ((keyData == Keys.Escape) && this.IsCellEditing)
            {
                this.CancelCellEdit();
                return true;
            }
            if (((keyData & Keys.Control) == Keys.Control) && ((keyData & Keys.KeyCode) == Keys.C))
            {
                this.CopySelectionToClipboard();
            }
            return base.ProcessDialogKey(keyData);
        }

        protected virtual void ProcessHyperlinkClicked(CellClickEventArgs e)
        {
            HyperlinkClickedEventArgs args = new HyperlinkClickedEventArgs {
                HitTest = e.HitTest,
                ListView = this,
                Location = new Point(-1, -1),
                Item = e.Item,
                SubItem = e.SubItem,
                Model = e.Model,
                ColumnIndex = e.ColumnIndex,
                Column = e.Column,
                RowIndex = e.RowIndex,
                ModifierKeys = Control.ModifierKeys,
                Url = e.SubItem.Url
            };
            this.OnHyperlinkClicked(args);
            if (!args.Handled)
            {
                this.StandardHyperlinkClickedProcessing(args);
            }
        }

        protected virtual bool ProcessLButtonDoubleClick(OlvListViewHitTestInfo hti)
        {
            return (hti.HitTestLocation == HitTestLocation.CheckBox);
        }

        protected virtual bool ProcessLButtonDown(OlvListViewHitTestInfo hti)
        {
            if (hti.Item == null)
            {
                return false;
            }
            if ((this.View != System.Windows.Forms.View.Details) || (hti.HitTestLocation != HitTestLocation.CheckBox))
            {
                return false;
            }
            if (hti.Column.Index > 0)
            {
                this.ToggleSubItemCheckBox(hti.RowObject, hti.Column);
                return true;
            }
            this.ToggleCheckObject(hti.RowObject);
            if (hti.Item.Selected)
            {
                CheckState? checkState = this.GetCheckState(hti.RowObject);
                if (checkState.HasValue)
                {
                    foreach (object obj2 in this.SelectedObjects)
                    {
                        this.SetObjectCheckedness(obj2, checkState.Value);
                    }
                }
            }
            return true;
        }

        protected virtual CheckState PutCheckState(object modelObject, CheckState state)
        {
            if (this.CheckStatePutter == null)
            {
                return state;
            }
            return this.CheckStatePutter(modelObject, state);
        }

        public virtual void RebuildColumns()
        {
            this.ChangeToFilteredColumns(this.View);
        }

        public virtual void RefreshHotItem()
        {
            this.UpdateHotItem(base.PointToClient(Cursor.Position));
        }

        public virtual void RefreshItem(OLVListItem olvi)
        {
            olvi.UseItemStyleForSubItems = true;
            olvi.SubItems.Clear();
            this.FillInValues(olvi, olvi.RowObject);
            this.PostProcessOneRow(olvi.Index, this.GetItemIndexInDisplayOrder(olvi), olvi);
        }

        public virtual void RefreshObject(object modelObject)
        {
            this.RefreshObjects(new object[] { modelObject });
        }

        public virtual void RefreshObjects(IList modelObjects)
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
            else
            {
                foreach (object obj2 in modelObjects)
                {
                    OLVListItem olvi = this.ModelToItem(obj2);
                    if (olvi != null)
                    {
                        this.RefreshItem(olvi);
                    }
                }
            }
        }

        public virtual void RefreshOverlays()
        {
            foreach (GlassPanelForm form in this.glassPanels)
            {
                form.Invalidate();
            }
        }

        public virtual void RefreshSelectedObjects()
        {
            foreach (ListViewItem item in base.SelectedItems)
            {
                this.RefreshItem((OLVListItem) item);
            }
        }

        private void RememberDisplayIndicies()
        {
            foreach (OLVColumn column in this.AllColumns)
            {
                column.LastDisplayIndex = column.DisplayIndex;
            }
        }

        public virtual void RemoveDecoration(IDecoration decoration)
        {
            if (decoration != null)
            {
                this.Decorations.Remove(decoration);
                base.Invalidate();
            }
        }

        public virtual void RemoveObject(object modelObject)
        {
            if (base.InvokeRequired)
            {
                base.Invoke(delegate {
                    this.RemoveObject(modelObject);
                });
            }
            else
            {
                this.RemoveObjects(new object[] { modelObject });
            }
        }

        public virtual void RemoveObjects(ICollection modelObjects)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.RemoveObjects(modelObjects);
                    };
                }
                base.Invoke(method);
            }
            else if (modelObjects != null)
            {
                base.BeginUpdate();
                try
                {
                    ItemsRemovingEventArgs e = new ItemsRemovingEventArgs(modelObjects);
                    this.OnItemsRemoving(e);
                    if (!e.Canceled)
                    {
                        modelObjects = e.ObjectsToRemove;
                        this.TakeOwnershipOfObjects();
                        ArrayList objects = (ArrayList) this.Objects;
                        foreach (object obj2 in modelObjects)
                        {
                            if (obj2 != null)
                            {
                                objects.Remove(obj2);
                                int index = this.IndexOf(obj2);
                                if (index >= 0)
                                {
                                    base.Items.RemoveAt(index);
                                }
                            }
                        }
                        if (((this.LastSortColumn == null) && this.UseAlternatingBackColors) && (this.View == System.Windows.Forms.View.Details))
                        {
                            this.PrepareAlternateBackColors();
                        }
                        this.OnItemsChanged(new ItemsChangedEventArgs());
                    }
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        public virtual void RemoveOverlay(IOverlay overlay)
        {
            if (overlay != null)
            {
                this.Overlays.Remove(overlay);
                GlassPanelForm item = this.FindGlassPanelForOverlay(overlay);
                if (item != null)
                {
                    this.glassPanels.Remove(item);
                    item.Unbind();
                    item.Dispose();
                }
            }
        }

        protected virtual void ResizeFreeSpaceFillingColumns()
        {
            this.ResizeFreeSpaceFillingColumns(base.ClientSize.Width);
        }

        protected virtual void ResizeFreeSpaceFillingColumns(int freeSpace)
        {
            if (!base.DesignMode && !this.Frozen)
            {
                int num = 0;
                List<OLVColumn> list = new List<OLVColumn>();
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    column = this.GetColumn(i);
                    if (column.FillsFreeSpace)
                    {
                        list.Add(column);
                        num += column.FreeSpaceProportion;
                    }
                    else
                    {
                        freeSpace -= column.Width;
                    }
                }
                freeSpace = Math.Max(0, freeSpace);
                foreach (OLVColumn column in list.ToArray())
                {
                    int minimumWidth = (freeSpace * column.FreeSpaceProportion) / num;
                    if ((column.MinimumWidth != -1) && (minimumWidth < column.MinimumWidth))
                    {
                        minimumWidth = column.MinimumWidth;
                    }
                    else if ((column.MaximumWidth != -1) && (minimumWidth > column.MaximumWidth))
                    {
                        minimumWidth = column.MaximumWidth;
                    }
                    else
                    {
                        minimumWidth = 0;
                    }
                    if (minimumWidth > 0)
                    {
                        column.Width = minimumWidth;
                        freeSpace -= minimumWidth;
                        num -= column.FreeSpaceProportion;
                        list.Remove(column);
                    }
                }
                foreach (OLVColumn column in list)
                {
                    column.Width = (freeSpace * column.FreeSpaceProportion) / num;
                }
            }
        }

        public virtual bool RestoreState(byte[] state)
        {
            using (MemoryStream stream = new MemoryStream(state))
            {
                ObjectListViewState state2;
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    state2 = formatter.Deserialize(stream) as ObjectListViewState;
                }
                catch (SerializationException)
                {
                    return false;
                }
                if (state2.NumberOfColumns != this.AllColumns.Count)
                {
                    return false;
                }
                if (state2.SortColumn == -1)
                {
                    this.LastSortColumn = null;
                    this.LastSortOrder = SortOrder.None;
                }
                else
                {
                    this.LastSortColumn = this.AllColumns[state2.SortColumn];
                    this.LastSortOrder = state2.LastSortOrder;
                }
                for (int i = 0; i < state2.NumberOfColumns; i++)
                {
                    OLVColumn column = this.AllColumns[i];
                    column.Width = (int) state2.ColumnWidths[i];
                    column.IsVisible = (bool) state2.ColumnIsVisible[i];
                    column.LastDisplayIndex = (int) state2.ColumnDisplayIndicies[i];
                }
                if (state2.IsShowingGroups != this.ShowGroups)
                {
                    this.ShowGroups = state2.IsShowingGroups;
                }
                if (this.View == state2.CurrentView)
                {
                    this.RebuildColumns();
                }
                else
                {
                    this.View = state2.CurrentView;
                }
            }
            return true;
        }

        public virtual byte[] SaveState()
        {
            ObjectListViewState graph = new ObjectListViewState {
                VersionNumber = 1,
                NumberOfColumns = this.AllColumns.Count,
                CurrentView = this.View
            };
            if (this.LastSortColumn != null)
            {
                graph.SortColumn = this.AllColumns.IndexOf(this.LastSortColumn);
            }
            graph.LastSortOrder = this.LastSortOrder;
            graph.IsShowingGroups = this.ShowGroups;
            if ((this.AllColumns.Count > 0) && (this.AllColumns[0].LastDisplayIndex == -1))
            {
                this.RememberDisplayIndicies();
            }
            foreach (OLVColumn column in this.AllColumns)
            {
                graph.ColumnIsVisible.Add(column.IsVisible);
                graph.ColumnDisplayIndicies.Add(column.LastDisplayIndex);
                graph.ColumnWidths.Add(column.Width);
            }
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, graph);
                return stream.ToArray();
            }
        }

        public virtual void SelectAll()
        {
            BrightIdeasSoftware.NativeMethods.SelectAllItems(this);
        }

        public virtual void SelectObject(object modelObject)
        {
            this.SelectObject(modelObject, false);
        }

        public virtual void SelectObject(object modelObject, bool setFocus)
        {
            if ((base.SelectedItems.Count != 1) || !((OLVListItem) base.SelectedItems[0]).RowObject.Equals(modelObject))
            {
                base.SelectedItems.Clear();
                OLVListItem item = this.ModelToItem(modelObject);
                if (item != null)
                {
                    item.Selected = true;
                    if (setFocus)
                    {
                        item.Focused = true;
                    }
                }
            }
        }

        public virtual void SelectObjects(IList modelObjects)
        {
            base.SelectedItems.Clear();
            if (modelObjects != null)
            {
                foreach (object obj2 in modelObjects)
                {
                    OLVListItem item = this.ModelToItem(obj2);
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        [Obsolete("This method is not longer maintained and will be removed", false)]
        protected virtual void SetAllSubItemImages()
        {
        }

        protected virtual void SetControlValue(Control control, object value, string stringValue)
        {
            if (control is System.Windows.Forms.ComboBox)
            {
                System.Windows.Forms.ComboBox cb = (System.Windows.Forms.ComboBox) control;
                if (cb.Created)
                {
                    cb.SelectedValue = value;
                }
                else
                {
                    base.BeginInvoke(delegate {
                        cb.SelectedValue = value;
                    });
                }
            }
            else
            {
                PropertyInfo property = null;
                try
                {
                    property = control.GetType().GetProperty("Value");
                }
                catch (AmbiguousMatchException)
                {
                    property = control.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }
                if (property != null)
                {
                    try
                    {
                        property.SetValue(control, value, null);
                        return;
                    }
                    catch (TargetInvocationException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                try
                {
                    string str = value as string;
                    if (str == null)
                    {
                        control.Text = stringValue;
                    }
                    else
                    {
                        control.Text = str;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        protected virtual void SetObjectCheckedness(object modelObject, CheckState state)
        {
            OLVListItem olvi = this.ModelToItem(modelObject);
            if ((olvi != null) && (olvi.CheckState != state))
            {
                ItemCheckEventArgs ice = new ItemCheckEventArgs(olvi.Index, state, olvi.CheckState);
                this.OnItemCheck(ice);
                if (ice.NewValue != olvi.CheckState)
                {
                    olvi.CheckState = this.PutCheckState(modelObject, state);
                    this.RefreshItem(olvi);
                    this.OnItemChecked(new ItemCheckedEventArgs(olvi));
                }
            }
        }

        public virtual void SetObjects(IEnumerable collection)
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
            else
            {
                ItemsChangingEventArgs e = new ItemsChangingEventArgs(this.objects, collection);
                this.OnItemsChanging(e);
                if (!e.Canceled)
                {
                    collection = e.NewObjects;
                    if (this.isOwnerOfObjects && (this.objects != collection))
                    {
                        this.isOwnerOfObjects = false;
                    }
                    this.objects = collection;
                    this.BuildList(false);
                    this.OnItemsChanged(new ItemsChangedEventArgs());
                }
            }
        }

        public virtual void SetSubItemImage(int rowIndex, int subItemIndex, OLVListSubItem subItem, bool shouldClearImages)
        {
            int actualImageIndex = this.GetActualImageIndex(subItem.ImageSelector);
            if (shouldClearImages || (actualImageIndex != -1))
            {
                BrightIdeasSoftware.NativeMethods.SetSubItemImage(this, rowIndex, subItemIndex, actualImageIndex);
            }
        }

        protected virtual void SetSubItemImages(int rowIndex, OLVListItem item)
        {
            this.SetSubItemImages(rowIndex, item, false);
        }

        protected virtual void SetSubItemImages(int rowIndex, OLVListItem item, bool shouldClearImages)
        {
            if (this.ShowImagesOnSubItems && !base.OwnerDraw)
            {
                for (int i = 1; i < item.SubItems.Count; i++)
                {
                    this.SetSubItemImage(rowIndex, i, item.GetSubItem(i), shouldClearImages);
                }
            }
        }

        private void SetupBaseImageList()
        {
            if (((this.rowHeight == -1) || (this.View != System.Windows.Forms.View.Details)) || ((this.shadowedImageList != null) && (this.shadowedImageList.ImageSize.Height == this.rowHeight)))
            {
                this.BaseSmallImageList = this.shadowedImageList;
            }
            else
            {
                int width = (this.shadowedImageList == null) ? 0x10 : this.shadowedImageList.ImageSize.Width;
                this.BaseSmallImageList = this.MakeResizedImageList(width, this.rowHeight, this.shadowedImageList);
            }
        }

        public virtual void SetupSubItemCheckBoxes()
        {
            this.ShowImagesOnSubItems = true;
            if (!((this.SmallImageList != null) && this.SmallImageList.Images.ContainsKey("checkbox-checked")))
            {
                this.InitializeCheckBoxImages();
            }
        }

        protected virtual bool ShouldStartCellEdit(MouseEventArgs e)
        {
            if (this.IsCellEditing)
            {
                return false;
            }
            if (e.Button != MouseButtons.Left)
            {
                return false;
            }
            if ((Control.ModifierKeys & (Keys.Alt | Keys.Control | Keys.Shift)) != Keys.None)
            {
                return false;
            }
            return (((this.lastMouseDownClickCount == 1) && (this.CellEditActivation == CellEditActivateMode.SingleClick)) || ((this.lastMouseDownClickCount == 2) && (this.CellEditActivation == CellEditActivateMode.DoubleClick)));
        }

        protected virtual void ShowColumnCommandMenu(int columnIndex, Point pt)
        {
            ToolStripDropDown strip = this.MakeColumnCommandMenu(new ContextMenuStrip(), columnIndex);
            if (this.ShowCommandMenuOnRightClick)
            {
                if (strip.Items.Count > 0)
                {
                    strip.Items.Add(new ToolStripSeparator());
                }
                this.MakeColumnSelectMenu(strip);
            }
            strip.Show(pt);
        }

        protected virtual void ShowColumnSelectMenu(Point pt)
        {
            this.MakeColumnSelectMenu(new ContextMenuStrip()).Show(pt);
        }

        public virtual void ShowOverlays()
        {
            if (!base.DesignMode && this.HasOverlays)
            {
                if (this.Overlays.Count != this.glassPanels.Count)
                {
                    foreach (IOverlay overlay in this.Overlays)
                    {
                        if (this.FindGlassPanelForOverlay(overlay) == null)
                        {
                            GlassPanelForm item = new GlassPanelForm();
                            item.Bind(this, overlay);
                            this.glassPanels.Add(item);
                        }
                    }
                }
                foreach (GlassPanelForm form in this.glassPanels)
                {
                    form.ShowGlass();
                }
            }
        }

        public virtual void ShowSortIndicator()
        {
            if (this.ShowSortIndicators && (this.LastSortOrder != SortOrder.None))
            {
                this.ShowSortIndicator(this.LastSortColumn, this.LastSortOrder);
            }
        }

        protected virtual void ShowSortIndicator(OLVColumn columnToSort, SortOrder sortOrder)
        {
            int imageIndex = -1;
            if (!BrightIdeasSoftware.NativeMethods.HasBuiltinSortIndicators())
            {
                if (!((this.SmallImageList != null) && this.SmallImageList.Images.ContainsKey("sort-indicator-up")))
                {
                    this.MakeSortIndicatorImages();
                }
                if (sortOrder == SortOrder.Ascending)
                {
                    imageIndex = this.SmallImageList.Images.IndexOfKey("sort-indicator-up");
                }
                else if (sortOrder == SortOrder.Descending)
                {
                    imageIndex = this.SmallImageList.Images.IndexOfKey("sort-indicator-down");
                }
            }
            for (int i = 0; i < this.Columns.Count; i++)
            {
                if (i == columnToSort.Index)
                {
                    BrightIdeasSoftware.NativeMethods.SetColumnImage(this, i, sortOrder, imageIndex);
                }
                else
                {
                    BrightIdeasSoftware.NativeMethods.SetColumnImage(this, i, SortOrder.None, -1);
                }
            }
        }

        public void Sort()
        {
            this.Sort(this.LastSortColumn, this.LastSortOrder);
        }

        public virtual void Sort(OLVColumn columnToSort)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.Sort(columnToSort);
                    };
                }
                base.Invoke(method);
            }
            else
            {
                this.Sort(columnToSort, this.LastSortOrder);
            }
        }

        public virtual void Sort(int columnToSortIndex)
        {
            if ((columnToSortIndex >= 0) && (columnToSortIndex < this.Columns.Count))
            {
                this.Sort(this.GetColumn(columnToSortIndex), this.LastSortOrder);
            }
        }

        public virtual void Sort(string columnToSortName)
        {
            this.Sort(this.GetColumn(columnToSortName), this.LastSortOrder);
        }

        public virtual void Sort(OLVColumn columnToSort, SortOrder order)
        {
            MethodInvoker method = null;
            if (base.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.Sort(columnToSort, order);
                    };
                }
                base.Invoke(method);
            }
            else
            {
                this.DoSort(columnToSort, order);
                this.PostProcessRows();
            }
        }

        protected virtual void StandardHyperlinkClickedProcessing(HyperlinkClickedEventArgs args)
        {
            Cursor cursor = this.Cursor;
            try
            {
                this.Cursor = Cursors.WaitCursor;
                Process.Start(args.Url);
            }
            catch (Win32Exception)
            {
                SystemSounds.Beep.Play();
            }
            finally
            {
                this.Cursor = cursor;
            }
            this.MarkUrlVisited(args.Url);
            this.RefreshHotItem();
        }

        protected virtual void StartCellEdit(OLVListItem item, int subItemIndex)
        {
            OLVColumn column = this.GetColumn(subItemIndex);
            Rectangle r = this.CalculateCellEditorBounds(item, subItemIndex);
            Control cellEditor = this.GetCellEditor(item, subItemIndex);
            cellEditor.Bounds = r;
            PropertyInfo property = cellEditor.GetType().GetProperty("TextAlign");
            if (property != null)
            {
                property.SetValue(cellEditor, column.TextAlign, null);
            }
            this.SetControlValue(cellEditor, column.GetValue(item.RowObject), column.GetStringValue(item.RowObject));
            this.cellEditEventArgs = new CellEditEventArgs(column, cellEditor, r, item, subItemIndex);
            this.OnCellEditStarting(this.cellEditEventArgs);
            if (!this.cellEditEventArgs.Cancel)
            {
                this.cellEditor = this.cellEditEventArgs.Control;
                if ((this.View != System.Windows.Forms.View.Tile) && (this.cellEditor.Height != r.Height))
                {
                    this.cellEditor.Top += (r.Height - this.cellEditor.Height) / 2;
                }
                base.Controls.Add(this.cellEditor);
                this.ConfigureControl();
                this.PauseAnimations(true);
            }
        }

        void ISupportInitialize.BeginInit()
        {
            this.Frozen = true;
        }

        void ISupportInitialize.EndInit()
        {
            if (this.RowHeight != -1)
            {
                this.SmallImageList = this.SmallImageList;
                if (this.CheckBoxes)
                {
                    this.InitializeStateImageList();
                }
            }
            if (this.UseCustomSelectionColors)
            {
                this.EnableCustomSelectionColors();
            }
            if (this.UseSubItemCheckBoxes || (base.VirtualMode && this.CheckBoxes))
            {
                this.SetupSubItemCheckBoxes();
            }
            this.Frozen = false;
        }

        protected virtual void TakeOwnershipOfObjects()
        {
            if (!this.isOwnerOfObjects)
            {
                this.isOwnerOfObjects = true;
                if (this.objects == null)
                {
                    this.objects = new ArrayList();
                }
                else if (this.objects is ICollection)
                {
                    this.objects = new ArrayList((ICollection) this.objects);
                }
                else
                {
                    ArrayList list = new ArrayList();
                    foreach (object obj2 in this.objects)
                    {
                        list.Add(obj2);
                    }
                    this.objects = list;
                }
            }
        }

        public virtual void ToggleCheckObject(object modelObject)
        {
            OLVListItem item = this.ModelToItem(modelObject);
            if (item != null)
            {
                CheckState indeterminate = CheckState.Checked;
                if (item.CheckState == CheckState.Checked)
                {
                    if (this.TriStateCheckBoxes)
                    {
                        indeterminate = CheckState.Indeterminate;
                    }
                    else
                    {
                        indeterminate = CheckState.Unchecked;
                    }
                }
                else if ((item.CheckState == CheckState.Indeterminate) && this.TriStateCheckBoxes)
                {
                    indeterminate = CheckState.Unchecked;
                }
                this.SetObjectCheckedness(modelObject, indeterminate);
            }
        }

        private void ToggleSelectedRowCheckBoxes()
        {
            object rowObject = this.GetItem(base.SelectedIndices[0]).RowObject;
            this.ToggleCheckObject(rowObject);
            CheckState? checkState = this.GetCheckState(rowObject);
            if (checkState.HasValue)
            {
                foreach (object obj3 in this.SelectedObjects)
                {
                    this.SetObjectCheckedness(obj3, checkState.Value);
                }
            }
        }

        public virtual void ToggleSubItemCheckBox(object rowObject, OLVColumn column)
        {
            if (column.TriStateCheckBoxes)
            {
                if (column.GetCheckState(rowObject) == CheckState.Checked)
                {
                    this.CheckIndeterminateSubItem(rowObject, column);
                }
                else if (column.GetCheckState(rowObject) == CheckState.Indeterminate)
                {
                    this.UncheckSubItem(rowObject, column);
                }
                else
                {
                    this.CheckSubItem(rowObject, column);
                }
            }
            else if (this.IsSubItemChecked(rowObject, column))
            {
                this.UncheckSubItem(rowObject, column);
            }
            else
            {
                this.CheckSubItem(rowObject, column);
            }
        }

        protected virtual void TriggerFormatRowEvent(int rowIndex, int displayIndex, OLVListItem olvi)
        {
            FormatRowEventArgs args = new FormatRowEventArgs {
                ListView = this,
                RowIndex = rowIndex,
                DisplayIndex = displayIndex,
                Item = olvi,
                UseCellFormatEvents = this.UseCellFormatEvents
            };
            this.OnFormatRow(args);
            if (args.UseCellFormatEvents && (this.View == System.Windows.Forms.View.Details))
            {
                int num;
                olvi.UseItemStyleForSubItems = false;
                Color backColor = olvi.BackColor;
                for (num = 0; num < this.Columns.Count; num++)
                {
                    olvi.SubItems[num].BackColor = backColor;
                }
                FormatCellEventArgs args2 = new FormatCellEventArgs {
                    ListView = this,
                    RowIndex = rowIndex,
                    DisplayIndex = displayIndex,
                    Item = olvi
                };
                for (num = 0; num < this.Columns.Count; num++)
                {
                    args2.ColumnIndex = num;
                    args2.Column = this.GetColumn(num);
                    args2.SubItem = olvi.GetSubItem(num);
                    this.OnFormatCell(args2);
                }
            }
        }

        protected virtual void UnapplyHotItem(int index)
        {
            this.Cursor = Cursors.Default;
            if (base.VirtualMode)
            {
                if (index < base.VirtualListSize)
                {
                    base.RedrawItems(index, index, true);
                }
            }
            else
            {
                OLVListItem olvi = this.GetItem(index);
                if (olvi != null)
                {
                    this.RefreshItem(olvi);
                }
            }
        }

        public virtual void UncheckObject(object modelObject)
        {
            this.SetObjectCheckedness(modelObject, CheckState.Unchecked);
        }

        public virtual void UncheckSubItem(object rowObject, OLVColumn column)
        {
            if (((column != null) && (rowObject != null)) && column.CheckBoxes)
            {
                column.PutCheckState(rowObject, CheckState.Unchecked);
                this.RefreshObject(rowObject);
            }
        }

        public virtual void Unfreeze()
        {
            if (this.freezeCount > 0)
            {
                this.freezeCount--;
                if (this.freezeCount == 0)
                {
                    this.DoUnfreeze();
                }
            }
        }

        protected virtual void UpdateCellToolTipHandle()
        {
            if ((this.cellToolTip != null) && (this.cellToolTip.Handle == IntPtr.Zero))
            {
                this.cellToolTip.AssignHandle(BrightIdeasSoftware.NativeMethods.GetTooltipControl(this));
            }
        }

        protected virtual void UpdateHotItem(OlvListViewHitTestInfo hti)
        {
            int rowIndex = hti.RowIndex;
            int columnIndex = hti.ColumnIndex;
            HitTestLocation hitTestLocation = hti.HitTestLocation;
            if ((rowIndex >= 0) && (this.View != System.Windows.Forms.View.Details))
            {
                columnIndex = 0;
            }
            if (((this.HotRowIndex != rowIndex) || (this.HotColumnIndex != columnIndex)) || (this.HotCellHitLocation != hitTestLocation))
            {
                HotItemChangedEventArgs e = new HotItemChangedEventArgs {
                    HotCellHitLocation = hitTestLocation,
                    HotColumnIndex = columnIndex,
                    HotRowIndex = rowIndex,
                    OldHotCellHitLocation = this.HotCellHitLocation,
                    OldHotColumnIndex = this.HotColumnIndex,
                    OldHotRowIndex = this.HotRowIndex
                };
                this.OnHotItemChanged(e);
                this.HotRowIndex = rowIndex;
                this.HotColumnIndex = columnIndex;
                this.HotCellHitLocation = hitTestLocation;
                if (!e.Handled)
                {
                    base.BeginUpdate();
                    try
                    {
                        base.Invalidate();
                        if (e.OldHotRowIndex != -1)
                        {
                            this.UnapplyHotItem(e.OldHotRowIndex);
                        }
                        if (this.HotRowIndex != -1)
                        {
                            if (base.VirtualMode)
                            {
                                base.RedrawItems(this.HotRowIndex, this.HotRowIndex, true);
                            }
                            else
                            {
                                this.UpdateHotRow(this.HotRowIndex, this.HotColumnIndex, this.HotCellHitLocation, hti.Item);
                            }
                        }
                        if ((this.UseHotItem && (this.HotItemStyle != null)) && (this.HotItemStyle.Overlay != null))
                        {
                            this.RefreshOverlays();
                        }
                    }
                    finally
                    {
                        base.EndUpdate();
                    }
                }
            }
        }

        protected virtual void UpdateHotItem(Point pt)
        {
            this.UpdateHotItem(this.OlvHitTest(pt.X, pt.Y));
        }

        protected virtual void UpdateHotRow(OLVListItem olvi)
        {
            this.UpdateHotRow(this.HotRowIndex, this.HotColumnIndex, this.HotCellHitLocation, olvi);
        }

        protected virtual void UpdateHotRow(int rowIndex, int columnIndex, HitTestLocation hitLocation, OLVListItem olvi)
        {
            if ((rowIndex >= 0) && (columnIndex >= 0))
            {
                if (this.UseHyperlinks)
                {
                    OLVColumn column = this.GetColumn(columnIndex);
                    OLVListSubItem subItem = olvi.GetSubItem(columnIndex);
                    if ((column.Hyperlink && (hitLocation == HitTestLocation.Text)) && !string.IsNullOrEmpty(subItem.Url))
                    {
                        this.ApplyCellStyle(olvi, columnIndex, this.HyperlinkStyle.Over);
                        this.Cursor = this.HyperlinkStyle.OverCursor ?? Cursors.Default;
                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
                if (this.UseHotItem && !olvi.Selected)
                {
                    this.ApplyRowStyle(olvi, this.HotItemStyle);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 70:
                    if (!this.HandleWindowPosChanging(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x4e:
                    if (!this.HandleNotify(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x7b:
                    if (!this.HandleContextMenu(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 2:
                    if (!this.HandleDestroy(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 15:
                    if (!this.HandlePaint(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x100:
                    if (!this.HandleKeyDown(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x102:
                    if (!this.HandleChar(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x114:
                case 0x115:
                    if (this.PossibleFinishCellEditing())
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x201:
                    if (!(!this.PossibleFinishCellEditing() || this.HandleLButtonDown(ref m)))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x202:
                    if (IsVista && this.HasCollapsibleGroups)
                    {
                        base.DefWndProc(ref m);
                    }
                    base.WndProc(ref m);
                    return;

                case 0x203:
                    if (!(!this.PossibleFinishCellEditing() || this.HandleLButtonDoubleClick(ref m)))
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x20a:
                case 0x20e:
                    if (this.PossibleFinishCellEditing())
                    {
                        base.WndProc(ref m);
                    }
                    return;

                case 0x204e:
                    if (!this.HandleReflectNotify(ref m))
                    {
                        base.WndProc(ref m);
                    }
                    return;
            }
            base.WndProc(ref m);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public virtual List<OLVColumn> AllColumns
        {
            get
            {
                return this.allColumns;
            }
            set
            {
                if (value == null)
                {
                    this.allColumns = new List<OLVColumn>();
                }
                else
                {
                    this.allColumns = value;
                }
            }
        }

        [DefaultValue(typeof(Color), ""), Description("If using alternate colors, what foregroundColor should alterate rows be?"), Category("Appearance - ObjectListView")]
        public Color AlternateRowBackColor
        {
            get
            {
                return this.alternateRowBackColor;
            }
            set
            {
                this.alternateRowBackColor = value;
            }
        }

        [Browsable(false)]
        public virtual Color AlternateRowBackColorOrDefault
        {
            get
            {
                if (this.alternateRowBackColor == Color.Empty)
                {
                    return Color.LemonChiffon;
                }
                return this.alternateRowBackColor;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual OLVColumn AlwaysGroupByColumn
        {
            get
            {
                return this.alwaysGroupByColumn;
            }
            set
            {
                this.alwaysGroupByColumn = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder AlwaysGroupBySortOrder
        {
            get
            {
                return this.alwaysGroupBySortOrder;
            }
            set
            {
                this.alwaysGroupBySortOrder = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual ImageList BaseSmallImageList
        {
            get
            {
                return base.SmallImageList;
            }
            set
            {
                base.SmallImageList = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual BooleanCheckStateGetterDelegate BooleanCheckStateGetter
        {
            set
            {
                if (value == null)
                {
                    this.CheckStateGetter = null;
                }
                else
                {
                    this.CheckStateGetter = x => value(x) ? ((CheckStateGetterDelegate) CheckState.Checked) : ((CheckStateGetterDelegate) CheckState.Unchecked);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual BooleanCheckStatePutterDelegate BooleanCheckStatePutter
        {
            set
            {
                if (value == null)
                {
                    this.CheckStatePutter = null;
                }
                else
                {
                    this.CheckStatePutter = (x, state) => value(x, state == CheckState.Checked) ? ((CheckStatePutterDelegate) CheckState.Checked) : ((CheckStatePutterDelegate) CheckState.Unchecked);
                }
            }
        }

        [Browsable(false)]
        public virtual bool CanShowGroups
        {
            get
            {
                return true;
            }
        }

        [DefaultValue(0), Category("Behavior - ObjectListView"), Description("How does the user indicate that they want to edit a cell?")]
        public virtual CellEditActivateMode CellEditActivation
        {
            get
            {
                return this.cellEditActivation;
            }
            set
            {
                this.cellEditActivation = value;
            }
        }

        [Browsable(false)]
        public ToolTipControl CellToolTip
        {
            get
            {
                if (this.cellToolTip == null)
                {
                    this.CreateCellToolTip();
                }
                return this.cellToolTip;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CellToolTipGetterDelegate CellToolTipGetter
        {
            get
            {
                return this.cellToolTipGetter;
            }
            set
            {
                this.cellToolTipGetter = value;
            }
        }

        public bool CheckBoxes
        {
            get
            {
                return base.CheckBoxes;
            }
            set
            {
                base.CheckBoxes = value;
                this.InitializeStateImageList();
            }
        }

        [DefaultValue((string) null), Description("The name of the property or field that holds the 'checkedness' of the model"), Category("Behavior - ObjectListView")]
        public virtual string CheckedAspectName
        {
            get
            {
                return this.checkedAspectName;
            }
            set
            {
                CheckStateGetterDelegate delegate2 = null;
                CheckStatePutterDelegate delegate3 = null;
                this.checkedAspectName = value;
                if (string.IsNullOrEmpty(this.checkedAspectName))
                {
                    this.checkedAspectMunger = null;
                    this.CheckStateGetter = null;
                    this.CheckStatePutter = null;
                }
                else
                {
                    this.checkedAspectMunger = new Munger(this.checkedAspectName);
                    if (delegate2 == null)
                    {
                        delegate2 = delegate (object modelObject) {
                            bool? nullable = this.checkedAspectMunger.GetValue(modelObject) as bool?;
                            if (nullable.HasValue)
                            {
                                if (nullable.Value)
                                {
                                    return CheckState.Checked;
                                }
                                return CheckState.Unchecked;
                            }
                            if (this.TriStateCheckBoxes)
                            {
                                return CheckState.Indeterminate;
                            }
                            return CheckState.Unchecked;
                        };
                    }
                    this.CheckStateGetter = delegate2;
                    if (delegate3 == null)
                    {
                        delegate3 = delegate (object modelObject, CheckState newValue) {
                            if (this.TriStateCheckBoxes && (newValue == CheckState.Indeterminate))
                            {
                                this.checkedAspectMunger.PutValue(modelObject, null);
                            }
                            else
                            {
                                this.checkedAspectMunger.PutValue(modelObject, newValue == CheckState.Checked);
                            }
                            return this.CheckStateGetter(modelObject);
                        };
                    }
                    this.CheckStatePutter = delegate3;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual object CheckedObject
        {
            get
            {
                IList checkedObjects = this.CheckedObjects;
                if (checkedObjects.Count == 1)
                {
                    return checkedObjects[0];
                }
                return null;
            }
            set
            {
                this.CheckedObjects = new ArrayList(new object[] { value });
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual IList CheckedObjects
        {
            get
            {
                ArrayList list = new ArrayList();
                if (this.CheckBoxes)
                {
                    for (int i = 0; i < this.GetItemCount(); i++)
                    {
                        OLVListItem item = this.GetItem(i);
                        if (item.CheckState == CheckState.Checked)
                        {
                            list.Add(item.RowObject);
                        }
                    }
                }
                return list;
            }
            set
            {
                if (this.CheckBoxes)
                {
                    if (value == null)
                    {
                        value = new ArrayList();
                    }
                    foreach (object obj2 in this.Objects)
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
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CheckStateGetterDelegate CheckStateGetter
        {
            get
            {
                return this.checkStateGetter;
            }
            set
            {
                this.checkStateGetter = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual CheckStatePutterDelegate CheckStatePutter
        {
            get
            {
                return this.checkStatePutter;
            }
            set
            {
                this.checkStatePutter = value;
            }
        }

        [Editor(typeof(OLVColumnCollectionEditor), typeof(UITypeEditor))]
        public System.Windows.Forms.ListView.ColumnHeaderCollection Columns
        {
            get
            {
                return base.Columns;
            }
        }

        [Browsable(false), Obsolete("Use GetFilteredColumns() and OLVColumn.IsTileViewColumn instead"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<OLVColumn> ColumnsForTileView
        {
            get
            {
                return this.GetFilteredColumns(System.Windows.Forms.View.Tile);
            }
        }

        [Browsable(false)]
        public virtual List<OLVColumn> ColumnsInDisplayOrder
        {
            get
            {
                int num;
                List<OLVColumn> list = new List<OLVColumn>(this.Columns.Count);
                for (num = 0; num < this.Columns.Count; num++)
                {
                    list.Add(null);
                }
                for (num = 0; num < this.Columns.Count; num++)
                {
                    OLVColumn column = this.GetColumn(num);
                    list[column.DisplayIndex] = column;
                }
                return list;
            }
        }

        [Browsable(false)]
        public Rectangle ContentRectangle
        {
            get
            {
                Rectangle clientRectangle = base.ClientRectangle;
                if ((this.View == System.Windows.Forms.View.Details) && (this.HeaderControl != null))
                {
                    Rectangle r = new Rectangle();
                    BrightIdeasSoftware.NativeMethods.GetClientRect(this.HeaderControl.Handle, ref r);
                    clientRectangle.Y = r.Height;
                    clientRectangle.Height -= r.Height;
                }
                return clientRectangle;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual SortDelegate CustomSorter
        {
            get
            {
                return this.customSorter;
            }
            set
            {
                this.customSorter = value;
            }
        }

        [Browsable(false)]
        protected IList<IDecoration> Decorations
        {
            get
            {
                return this.decorations;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRenderer DefaultRenderer
        {
            get
            {
                return this.defaultRenderer;
            }
            set
            {
                if (value == null)
                {
                    this.defaultRenderer = new BaseRenderer();
                }
                else
                {
                    this.defaultRenderer = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IDragSource DragSource
        {
            get
            {
                return this.dragSource;
            }
            set
            {
                this.dragSource = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IDropSink DropSink
        {
            get
            {
                return this.dropSink;
            }
            set
            {
                if (this.dropSink != value)
                {
                    SimpleDropSink dropSink = this.dropSink as SimpleDropSink;
                    if (dropSink != null)
                    {
                        dropSink.CanDrop -= new EventHandler<OlvDropEventArgs>(this.dropSink_CanDrop);
                        dropSink.Dropped -= new EventHandler<OlvDropEventArgs>(this.dropSink_Dropped);
                        dropSink.ModelCanDrop -= new EventHandler<ModelDropEventArgs>(this.dropSink_ModelCanDrop);
                        dropSink.ModelDropped -= new EventHandler<ModelDropEventArgs>(this.dropSink_ModelDropped);
                    }
                    this.dropSink = value;
                    this.AllowDrop = value != null;
                    if (this.dropSink != null)
                    {
                        this.dropSink.ListView = this;
                    }
                    SimpleDropSink sink2 = value as SimpleDropSink;
                    if (sink2 != null)
                    {
                        sink2.CanDrop += new EventHandler<OlvDropEventArgs>(this.dropSink_CanDrop);
                        sink2.Dropped += new EventHandler<OlvDropEventArgs>(this.dropSink_Dropped);
                        sink2.ModelCanDrop += new EventHandler<ModelDropEventArgs>(this.dropSink_ModelCanDrop);
                        sink2.ModelDropped += new EventHandler<ModelDropEventArgs>(this.dropSink_ModelDropped);
                    }
                }
            }
        }

        [Description("When the list has no items, show this m in the control"), DefaultValue((string) null), Category("Appearance - ObjectListView"), Localizable(true)]
        public virtual string EmptyListMsg
        {
            get
            {
                TextOverlay emptyListMsgOverlay = this.EmptyListMsgOverlay as TextOverlay;
                if (emptyListMsgOverlay == null)
                {
                    return null;
                }
                return emptyListMsgOverlay.Text;
            }
            set
            {
                TextOverlay emptyListMsgOverlay = this.EmptyListMsgOverlay as TextOverlay;
                if (emptyListMsgOverlay != null)
                {
                    emptyListMsgOverlay.Text = value;
                }
            }
        }

        [Category("Appearance - ObjectListView"), DefaultValue((string) null), Description("What font should the 'list empty' message be drawn in?")]
        public virtual Font EmptyListMsgFont
        {
            get
            {
                TextOverlay emptyListMsgOverlay = this.EmptyListMsgOverlay as TextOverlay;
                if (emptyListMsgOverlay == null)
                {
                    return null;
                }
                return emptyListMsgOverlay.Font;
            }
            set
            {
                TextOverlay emptyListMsgOverlay = this.EmptyListMsgOverlay as TextOverlay;
                if (emptyListMsgOverlay != null)
                {
                    emptyListMsgOverlay.Font = value;
                }
            }
        }

        [Browsable(false)]
        public virtual Font EmptyListMsgFontOrDefault
        {
            get
            {
                if (this.EmptyListMsgFont == null)
                {
                    return new Font("Tahoma", 14f);
                }
                return this.EmptyListMsgFont;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual IOverlay EmptyListMsgOverlay
        {
            get
            {
                return this.emptyListMsgOverlay;
            }
            set
            {
                if (this.emptyListMsgOverlay != value)
                {
                    this.emptyListMsgOverlay = value;
                    base.Invalidate();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual bool Frozen
        {
            get
            {
                return (this.freezeCount > 0);
            }
            set
            {
                if (value)
                {
                    this.Freeze();
                }
                else if (this.freezeCount > 0)
                {
                    this.freezeCount = 1;
                    this.Unfreeze();
                }
            }
        }

        [DefaultValue((string) null), Category("Appearance - ObjectListView"), Description("The image list from which group header will take their images")]
        public ImageList GroupImageList
        {
            get
            {
                return this.groupImageList;
            }
            set
            {
                this.groupImageList = value;
                BrightIdeasSoftware.NativeMethods.SetGroupImageList(this, value);
            }
        }

        [Localizable(true), Category("Behavior - ObjectListView"), Description("The format to use when suffixing item counts to group titles"), DefaultValue((string) null)]
        public virtual string GroupWithItemCountFormat
        {
            get
            {
                return this.groupWithItemCountFormat;
            }
            set
            {
                this.groupWithItemCountFormat = value;
            }
        }

        [Browsable(false)]
        public virtual string GroupWithItemCountFormatOrDefault
        {
            get
            {
                if (string.IsNullOrEmpty(this.GroupWithItemCountFormat))
                {
                    return "{0} [{1} items]";
                }
                return this.GroupWithItemCountFormat;
            }
        }

        [Description("The format to use when suffixing item counts to group titles"), Category("Behavior - ObjectListView"), DefaultValue((string) null), Localizable(true)]
        public virtual string GroupWithItemCountSingularFormat
        {
            get
            {
                return this.groupWithItemCountSingularFormat;
            }
            set
            {
                this.groupWithItemCountSingularFormat = value;
            }
        }

        [Browsable(false)]
        public virtual string GroupWithItemCountSingularFormatOrDefault
        {
            get
            {
                if (string.IsNullOrEmpty(this.GroupWithItemCountSingularFormat))
                {
                    return "{0} [{1} item]";
                }
                return this.GroupWithItemCountSingularFormat;
            }
        }

        [Category("Appearance - ObjectListView"), Description("Should the groups in this control be collapsible (Vista only)."), DefaultValue(true), Browsable(true)]
        public bool HasCollapsibleGroups
        {
            get
            {
                return this.hasCollapsibleGroups;
            }
            set
            {
                this.hasCollapsibleGroups = value;
            }
        }

        [Browsable(false)]
        public virtual bool HasEmptyListMsg
        {
            get
            {
                return !string.IsNullOrEmpty(this.EmptyListMsg);
            }
        }

        [Browsable(false)]
        public bool HasOverlays
        {
            get
            {
                return (((this.Overlays.Count > 2) || (this.imageOverlay.Image != null)) || !string.IsNullOrEmpty(this.textOverlay.Text));
            }
        }

        public BrightIdeasSoftware.HeaderControl HeaderControl
        {
            get
            {
                if (this.headerControl == null)
                {
                    this.headerControl = new BrightIdeasSoftware.HeaderControl(this);
                }
                return this.headerControl;
            }
        }

        [Category("Appearance - ObjectListView"), Description("What font will be used to draw the text of the column headers"), DefaultValue((string) null)]
        public Font HeaderFont
        {
            get
            {
                return this.headerFont;
            }
            set
            {
                this.headerFont = value;
            }
        }

        [Browsable(false)]
        public ToolTipControl HeaderToolTip
        {
            get
            {
                return this.HeaderControl.ToolTip;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual HeaderToolTipGetterDelegate HeaderToolTipGetter
        {
            get
            {
                return this.headerToolTipGetter;
            }
            set
            {
                this.headerToolTipGetter = value;
            }
        }

        [DefaultValue(false), Category("Appearance - ObjectListView"), Description("Will the text of the column headers be word wrapped?")]
        public bool HeaderWordWrap
        {
            get
            {
                return this.headerWordWrap;
            }
            set
            {
                this.headerWordWrap = value;
                if (this.headerControl != null)
                {
                    this.headerControl.WordWrap = value;
                }
            }
        }

        [DefaultValue(typeof(Color), ""), Category("Appearance - ObjectListView"), Description("The background foregroundColor of selected rows when the control is owner drawn")]
        public virtual Color HighlightBackgroundColor
        {
            get
            {
                return this.highlightBackgroundColor;
            }
            set
            {
                this.highlightBackgroundColor = value;
            }
        }

        [Browsable(false)]
        public virtual Color HighlightBackgroundColorOrDefault
        {
            get
            {
                if (this.HighlightBackgroundColor.IsEmpty)
                {
                    return SystemColors.Highlight;
                }
                return this.HighlightBackgroundColor;
            }
        }

        [Description("The foreground foregroundColor of selected rows when the control is owner drawn"), Category("Appearance - ObjectListView"), DefaultValue(typeof(Color), "")]
        public virtual Color HighlightForegroundColor
        {
            get
            {
                return this.highlightForegroundColor;
            }
            set
            {
                this.highlightForegroundColor = value;
            }
        }

        [Browsable(false)]
        public virtual Color HighlightForegroundColorOrDefault
        {
            get
            {
                if (this.HighlightForegroundColor.IsEmpty)
                {
                    return SystemColors.HighlightText;
                }
                return this.HighlightForegroundColor;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual HitTestLocation HotCellHitLocation
        {
            get
            {
                return this.hotCellHitLocation;
            }
            protected set
            {
                this.hotCellHitLocation = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual int HotColumnIndex
        {
            get
            {
                return this.hotColumnIndex;
            }
            protected set
            {
                this.hotColumnIndex = value;
            }
        }

        [Obsolete("Use HotRowIndex instead", false), Browsable(false)]
        public virtual int HotItemIndex
        {
            get
            {
                return this.HotRowIndex;
            }
        }

        [Category("Appearance - ObjectListView"), Description("How should the row under the cursor be highlighted"), DefaultValue((string) null)]
        public virtual BrightIdeasSoftware.HotItemStyle HotItemStyle
        {
            get
            {
                return this.hotItemStyle;
            }
            set
            {
                if (this.HotItemStyle != null)
                {
                    this.RemoveOverlay(this.HotItemStyle.Overlay);
                }
                this.hotItemStyle = value;
                if (this.HotItemStyle != null)
                {
                    this.AddOverlay(this.HotItemStyle.Overlay);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int HotRowIndex
        {
            get
            {
                return this.hotRowIndex;
            }
            protected set
            {
                this.hotRowIndex = value;
            }
        }

        [Category("Appearance - ObjectListView"), Description("How should hyperlinks be drawn"), DefaultValue((string) null)]
        public virtual BrightIdeasSoftware.HyperlinkStyle HyperlinkStyle
        {
            get
            {
                return this.hyperlinkStyle;
            }
            set
            {
                this.hyperlinkStyle = value;
            }
        }

        [Browsable(false)]
        public virtual bool IsCellEditing
        {
            get
            {
                return (this.cellEditor != null);
            }
        }

        [Description("When the user types into a list, should the values in the current sort column be searched to find a match?"), DefaultValue(true), Category("Behavior - ObjectListView")]
        public virtual bool IsSearchOnSortColumn
        {
            get
            {
                return this.isSearchOnSortColumn;
            }
            set
            {
                this.isSearchOnSortColumn = value;
            }
        }

        [DefaultValue(false), Category("Behavior - ObjectListView"), Description("Should this control use a SimpleDragSource to initiate drags out from this control")]
        public virtual bool IsSimpleDragSource
        {
            get
            {
                return (this.DragSource != null);
            }
            set
            {
                if (value)
                {
                    this.DragSource = new SimpleDragSource();
                }
                else
                {
                    this.DragSource = null;
                }
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(false), Description("Should this control will use a SimpleDropSink to receive drops.")]
        public virtual bool IsSimpleDropSink
        {
            get
            {
                return (this.DropSink != null);
            }
            set
            {
                if (value)
                {
                    this.DropSink = new SimpleDropSink();
                }
                else
                {
                    this.DropSink = null;
                }
            }
        }

        public static bool IsVista
        {
            get
            {
                if (!isVista.HasValue)
                {
                    isVista = new bool?(Environment.OSVersion.Version.Major >= 6);
                }
                return isVista.Value;
            }
        }

        [DefaultValue((string) null), Description("The owner drawn renderer that draws items when the list is in non-Details view."), Category("Appearance - ObjectListView")]
        public IRenderer ItemRenderer
        {
            get
            {
                return this.itemRenderer;
            }
            set
            {
                this.itemRenderer = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OLVColumn LastSortColumn
        {
            get
            {
                return this.PrimarySortColumn;
            }
            set
            {
                this.PrimarySortColumn = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual SortOrder LastSortOrder
        {
            get
            {
                return this.PrimarySortOrder;
            }
            set
            {
                this.PrimarySortOrder = value;
            }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Group by '{0}'"), Localizable(true)]
        public string MenuLabelGroupBy
        {
            get
            {
                return this.menuLabelGroupBy;
            }
            set
            {
                this.menuLabelGroupBy = value;
            }
        }

        [Category("Labels - ObjectListView"), Localizable(true), DefaultValue("Lock grouping on '{0}'")]
        public string MenuLabelLockGroupingOn
        {
            get
            {
                return this.menuLabelLockGroupingOn;
            }
            set
            {
                this.menuLabelLockGroupingOn = value;
            }
        }

        [Category("Labels - ObjectListView"), DefaultValue("Sort ascending by '{0}'"), Localizable(true)]
        public string MenuLabelSortAscending
        {
            get
            {
                return this.menuLabelSortAscending;
            }
            set
            {
                this.menuLabelSortAscending = value;
            }
        }

        [Localizable(true), Category("Labels - ObjectListView"), DefaultValue("Sort descending by '{0}'")]
        public string MenuLabelSortDescending
        {
            get
            {
                return this.menuLabelSortDescending;
            }
            set
            {
                this.menuLabelSortDescending = value;
            }
        }

        [DefaultValue("Turn off groups"), Category("Labels - ObjectListView"), Localizable(true)]
        public string MenuLabelTurnOffGroups
        {
            get
            {
                return this.menuLabelTurnOffGroups;
            }
            set
            {
                this.menuLabelTurnOffGroups = value;
            }
        }

        [Localizable(true), DefaultValue("Unlock grouping from '{0}'"), Category("Labels - ObjectListView")]
        public string MenuLabelUnlockGroupingOn
        {
            get
            {
                return this.menuLabelUnlockGroupingOn;
            }
            set
            {
                this.menuLabelUnlockGroupingOn = value;
            }
        }

        [DefaultValue("Unsort"), Category("Labels - ObjectListView"), Localizable(true)]
        public string MenuLabelUnsort
        {
            get
            {
                return this.menuLabelUnsort;
            }
            set
            {
                this.menuLabelUnsort = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual IEnumerable Objects
        {
            get
            {
                return this.objects;
            }
            set
            {
                base.BeginUpdate();
                try
                {
                    IList selectedObjects = this.SelectedObjects;
                    this.SetObjects(value);
                    this.SelectedObjects = selectedObjects;
                }
                finally
                {
                    base.EndUpdate();
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<OLVGroup> OLVGroups
        {
            get
            {
                return this.olvGroups;
            }
            set
            {
                this.olvGroups = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Description("The image that will be drawn over the top of the ListView"), Category("Appearance - ObjectListView")]
        public ImageOverlay OverlayImage
        {
            get
            {
                return this.imageOverlay;
            }
            set
            {
                if (this.imageOverlay != value)
                {
                    this.RemoveOverlay(this.imageOverlay);
                    this.imageOverlay = value;
                    this.AddOverlay(this.imageOverlay);
                }
            }
        }

        [Browsable(false)]
        protected IList<IOverlay> Overlays
        {
            get
            {
                return this.overlays;
            }
        }

        [Description("The text that will be drawn over the top of the ListView"), Category("Appearance - ObjectListView"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TextOverlay OverlayText
        {
            get
            {
                return this.textOverlay;
            }
            set
            {
                if (this.textOverlay != value)
                {
                    this.RemoveOverlay(this.textOverlay);
                    this.textOverlay = value;
                    this.AddOverlay(this.textOverlay);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int OverlayTransparency
        {
            get
            {
                return this.overlayTransparency;
            }
            set
            {
                this.overlayTransparency = Math.Min(0xff, Math.Max(0, value));
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(false), Description("Should the DrawColumnHeader event be triggered")]
        public bool OwnerDrawnHeader
        {
            get
            {
                return this.ownerDrawnHeader;
            }
            set
            {
                this.ownerDrawnHeader = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OLVColumn PrimarySortColumn
        {
            get
            {
                return this.primarySortColumn;
            }
            set
            {
                this.primarySortColumn = value;
                if (this.TintSortColumn)
                {
                    this.SelectedColumn = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual SortOrder PrimarySortOrder
        {
            get
            {
                return this.primarySortOrder;
            }
            set
            {
                this.primarySortOrder = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual RowFormatterDelegate RowFormatter
        {
            get
            {
                return this.rowFormatter;
            }
            set
            {
                this.rowFormatter = value;
            }
        }

        [Description("Specify the height of each row in pixels. -1 indicates default height"), Category("Appearance - ObjectListView"), DefaultValue(-1)]
        public virtual int RowHeight
        {
            get
            {
                return this.rowHeight;
            }
            set
            {
                if (value < 1)
                {
                    this.rowHeight = -1;
                }
                else
                {
                    this.rowHeight = value;
                }
                this.SetupBaseImageList();
                if (this.CheckBoxes)
                {
                    this.InitializeStateImageList();
                }
            }
        }

        [Browsable(false)]
        public virtual int RowHeightEffective
        {
            get
            {
                switch (this.View)
                {
                    case System.Windows.Forms.View.LargeIcon:
                        if (base.LargeImageList != null)
                        {
                            return Math.Max(base.LargeImageList.ImageSize.Height, this.Font.Height);
                        }
                        return this.Font.Height;

                    case System.Windows.Forms.View.Details:
                    case System.Windows.Forms.View.SmallIcon:
                    case System.Windows.Forms.View.List:
                        return Math.Max(this.SmallImageSize.Height, this.Font.Height);

                    case System.Windows.Forms.View.Tile:
                        return base.TileSize.Height;
                }
                return 0;
            }
        }

        [Browsable(false)]
        public virtual int RowsPerPage
        {
            get
            {
                return BrightIdeasSoftware.NativeMethods.GetCountPerPage(this);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual OLVColumn SecondarySortColumn
        {
            get
            {
                return this.secondarySortColumn;
            }
            set
            {
                this.secondarySortColumn = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual SortOrder SecondarySortOrder
        {
            get
            {
                return this.secondarySortOrder;
            }
            set
            {
                this.secondarySortOrder = value;
            }
        }

        [Category("Behavior - ObjectListView"), Description("When the column select menu is open, should it stay open after an item is selected?"), DefaultValue(true)]
        public virtual bool SelectColumnsMenuStaysOpen
        {
            get
            {
                return this.selectColumnsMenuStaysOpen;
            }
            set
            {
                this.selectColumnsMenuStaysOpen = value;
            }
        }

        [Description("When the user right clicks on the column headers, should a menu be presented which will allow them to choose which columns will be shown in the view?"), DefaultValue(true), Category("Behavior - ObjectListView")]
        public virtual bool SelectColumnsOnRightClick
        {
            get
            {
                return this.selectColumnsOnRightClick;
            }
            set
            {
                this.selectColumnsOnRightClick = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OLVColumn SelectedColumn
        {
            get
            {
                return this.selectedColumn;
            }
            set
            {
                this.selectedColumn = value;
                if (value == null)
                {
                    this.RemoveDecoration(this.selectedColumnDecoration);
                }
                else if (!this.HasDecoration(this.selectedColumnDecoration))
                {
                    this.AddDecoration(this.selectedColumnDecoration);
                }
            }
        }

        [DefaultValue(typeof(Color), ""), Description("The color that will be used to tint the selected column"), Category("Appearance - ObjectListView")]
        public virtual Color SelectedColumnTint
        {
            get
            {
                return this.selectedColumnTint;
            }
            set
            {
                if (value.A == 0xff)
                {
                    this.selectedColumnTint = Color.FromArgb(15, value);
                }
                else
                {
                    this.selectedColumnTint = value;
                }
                this.selectedColumnDecoration.Tint = this.selectedColumnTint;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int SelectedIndex
        {
            get
            {
                if (base.SelectedIndices.Count == 1)
                {
                    return base.SelectedIndices[0];
                }
                return -1;
            }
            set
            {
                base.SelectedIndices.Clear();
                if ((value >= 0) && (value < base.Items.Count))
                {
                    base.SelectedIndices.Add(value);
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual ListViewItem SelectedItem
        {
            get
            {
                if (base.SelectedIndices.Count == 1)
                {
                    return this.GetItem(base.SelectedIndices[0]);
                }
                return null;
            }
            set
            {
                base.SelectedIndices.Clear();
                if (value != null)
                {
                    base.SelectedIndices.Add(value.Index);
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual object SelectedObject
        {
            get
            {
                return this.GetSelectedObject();
            }
            set
            {
                this.SelectObject(value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IList SelectedObjects
        {
            get
            {
                return this.GetSelectedObjects();
            }
            set
            {
                this.SelectObjects(value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IDecoration SelectedRowDecoration
        {
            get
            {
                return this.selectedRowDecoration;
            }
            set
            {
                this.selectedRowDecoration = value;
            }
        }

        [Description("When the user right clicks on the column headers, should a menu be presented which will allow them to perform common tasks on the listview?"), Category("Behavior - ObjectListView"), DefaultValue(false)]
        public virtual bool ShowCommandMenuOnRightClick
        {
            get
            {
                return this.showCommandMenuOnRightClick;
            }
            set
            {
                this.showCommandMenuOnRightClick = value;
            }
        }

        [DefaultValue(true), Description("Should the list view show items in groups?"), Category("Appearance")]
        public virtual bool ShowGroups
        {
            get
            {
                return base.ShowGroups;
            }
            set
            {
                base.ShowGroups = value;
            }
        }

        [DefaultValue(true), Description("Will the control will show column headers in all views?"), Category("Behavior - ObjectListView")]
        public bool ShowHeaderInAllViews
        {
            get
            {
                return this.showHeaderInAllViews;
            }
            set
            {
                this.showHeaderInAllViews = value;
                this.ApplyExtendedStyles();
            }
        }

        [DefaultValue(false), Description("Should the list view show images on subitems?"), Category("Appearance - ObjectListView")]
        public virtual bool ShowImagesOnSubItems
        {
            get
            {
                return this.showImagesOnSubItems;
            }
            set
            {
                this.showImagesOnSubItems = value;
                this.ApplyExtendedStyles();
                if (value && base.VirtualMode)
                {
                    base.OwnerDraw = true;
                }
            }
        }

        [Description("Will group titles be suffixed with a count of the items in the group?"), Category("Behavior - ObjectListView"), DefaultValue(false)]
        public virtual bool ShowItemCountOnGroups
        {
            get
            {
                return this.showItemCountOnGroups;
            }
            set
            {
                this.showItemCountOnGroups = value;
            }
        }

        [DefaultValue(true), Description("Should the list view show sort indicators in the column headers?"), Category("Appearance - ObjectListView")]
        public virtual bool ShowSortIndicators
        {
            get
            {
                return this.showSortIndicators;
            }
            set
            {
                this.showSortIndicators = value;
            }
        }

        public ImageList SmallImageList
        {
            get
            {
                return this.shadowedImageList;
            }
            set
            {
                this.shadowedImageList = value;
                if (this.UseSubItemCheckBoxes)
                {
                    this.SetupSubItemCheckBoxes();
                }
                this.SetupBaseImageList();
            }
        }

        [Browsable(false)]
        public virtual Size SmallImageSize
        {
            get
            {
                if (this.SmallImageList == null)
                {
                    return new Size(0x10, 0x10);
                }
                return this.SmallImageList.ImageSize;
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(true), Description("When the listview is grouped, should the items be sorted by the primary column? If this is false, the items will be sorted by the same column as they are grouped.")]
        public virtual bool SortGroupItemsByPrimaryColumn
        {
            get
            {
                return this.sortGroupItemsByPrimaryColumn;
            }
            set
            {
                this.sortGroupItemsByPrimaryColumn = value;
            }
        }

        [Category("Appearance - ObjectListView"), DefaultValue(0), Description("How many pixels of space will be between groups")]
        public virtual int SpaceBetweenGroups
        {
            get
            {
                return this.spaceBetweenGroups;
            }
            set
            {
                this.spaceBetweenGroups = value;
            }
        }

        public static TextRenderingHint TextRendereringHint
        {
            get
            {
                return textRendereringHint;
            }
            set
            {
                textRendereringHint = value;
            }
        }

        [DefaultValue(false), Category("Appearance - ObjectListView"), Description("Should the sort column show a slight tinting?")]
        public virtual bool TintSortColumn
        {
            get
            {
                return this.tintSortColumn;
            }
            set
            {
                this.tintSortColumn = value;
                if (value && (this.LastSortColumn != null))
                {
                    this.SelectedColumn = this.LastSortColumn;
                }
                else
                {
                    this.SelectedColumn = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public virtual int TopItemIndex
        {
            get
            {
                if ((this.View == System.Windows.Forms.View.Details) && (base.TopItem != null))
                {
                    return base.TopItem.Index;
                }
                return -1;
            }
            set
            {
                int index = Math.Min(value, this.GetItemCount() - 1);
                if ((this.View == System.Windows.Forms.View.Details) && (index >= 0))
                {
                    base.TopItem = base.Items[index];
                    if ((base.TopItem != null) && (base.TopItem.Index != index))
                    {
                        base.TopItem = this.GetItem(index);
                    }
                }
            }
        }

        [Category("Appearance - ObjectListView"), Description("Should the primary column have a checkbox that behaves as a tri-state checkbox?"), DefaultValue(false)]
        public virtual bool TriStateCheckBoxes
        {
            get
            {
                return this.triStateCheckBoxes;
            }
            set
            {
                this.triStateCheckBoxes = value;
                if (!(!value || this.CheckBoxes))
                {
                    this.CheckBoxes = true;
                }
                this.InitializeStateImageList();
            }
        }

        [Category("Appearance - ObjectListView"), Description("The background color of selected rows when the control is owner drawn and doesn't have the focus"), DefaultValue(typeof(Color), "")]
        public virtual Color UnfocusedHighlightBackgroundColor
        {
            get
            {
                return this.unfocusedHighlightBackgroundColor;
            }
            set
            {
                this.unfocusedHighlightBackgroundColor = value;
            }
        }

        [Browsable(false)]
        public virtual Color UnfocusedHighlightBackgroundColorOrDefault
        {
            get
            {
                if (this.UnfocusedHighlightBackgroundColor.IsEmpty)
                {
                    return SystemColors.Control;
                }
                return this.UnfocusedHighlightBackgroundColor;
            }
        }

        [Category("Appearance - ObjectListView"), Description("The foreground color of selected rows when the control is owner drawn and doesn't have the focus"), DefaultValue(typeof(Color), "")]
        public virtual Color UnfocusedHighlightForegroundColor
        {
            get
            {
                return this.unfocusedHighlightForegroundColor;
            }
            set
            {
                this.unfocusedHighlightForegroundColor = value;
            }
        }

        [Browsable(false)]
        public virtual Color UnfocusedHighlightForegroundColorOrDefault
        {
            get
            {
                if (this.UnfocusedHighlightForegroundColor.IsEmpty)
                {
                    return SystemColors.ControlText;
                }
                return this.UnfocusedHighlightForegroundColor;
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(true), Description("When resizing a column by dragging its divider, should any space filling columns be resized at each mouse move?")]
        public virtual bool UpdateSpaceFillingColumnsWhenDraggingColumnDivider
        {
            get
            {
                return this.updateSpaceFillingColumnsWhenDraggingColumnDivider;
            }
            set
            {
                this.updateSpaceFillingColumnsWhenDraggingColumnDivider = value;
            }
        }

        [Category("Appearance - ObjectListView"), Description("Should the list view use a different backcolor to alternate rows?"), DefaultValue(false)]
        public virtual bool UseAlternatingBackColors
        {
            get
            {
                return this.useAlternatingBackColors;
            }
            set
            {
                this.useAlternatingBackColors = value;
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(false), Description("Should FormatCell events be triggered to every cell that is built?")]
        public bool UseCellFormatEvents
        {
            get
            {
                return this.useCellFormatEvents;
            }
            set
            {
                this.useCellFormatEvents = value;
            }
        }

        [Description("Should the selected row be drawn with non-standard foreground and background colors?"), Category("Appearance - ObjectListView"), DefaultValue(false)]
        public bool UseCustomSelectionColors
        {
            get
            {
                return this.useCustomSelectionColors;
            }
            set
            {
                this.useCustomSelectionColors = value;
                if (!(base.DesignMode || !value))
                {
                    base.OwnerDraw = true;
                }
            }
        }

        [Category("Appearance - ObjectListView"), Description("Should the list use the same hot item and selection mechanism as Vista?"), DefaultValue(false)]
        public bool UseExplorerTheme
        {
            get
            {
                return this.useExplorerTheme;
            }
            set
            {
                this.useExplorerTheme = value;
                if (base.Created)
                {
                    BrightIdeasSoftware.NativeMethods.SetWindowTheme(base.Handle, value ? "explorer" : "", null);
                }
            }
        }

        [DefaultValue(false), Category("Appearance - ObjectListView"), Description("Should HotTracking be used? Hot tracking applies special formatting to the row under the cursor")]
        public bool UseHotItem
        {
            get
            {
                return this.useHotItem;
            }
            set
            {
                this.useHotItem = value;
                if (this.HotItemStyle != null)
                {
                    if (value)
                    {
                        this.AddOverlay(this.HotItemStyle.Overlay);
                    }
                    else
                    {
                        this.RemoveOverlay(this.HotItemStyle.Overlay);
                    }
                }
            }
        }

        [DefaultValue(false), Category("Behavior - ObjectListView"), Description("Should hyperlinks be shown on this control?")]
        public bool UseHyperlinks
        {
            get
            {
                return this.useHyperlinks;
            }
            set
            {
                this.useHyperlinks = value;
                if (value && (this.HyperlinkStyle == null))
                {
                    this.HyperlinkStyle = new BrightIdeasSoftware.HyperlinkStyle();
                }
            }
        }

        [Category("Behavior - ObjectListView"), DefaultValue(false), Description("Should this control be configured to show check boxes on subitems.")]
        public bool UseSubItemCheckBoxes
        {
            get
            {
                return this.useSubItemCheckBoxes;
            }
            set
            {
                this.useSubItemCheckBoxes = value;
                if (value)
                {
                    this.SetupSubItemCheckBoxes();
                }
            }
        }

        [DefaultValue(false), Category("Appearance - ObjectListView"), Description("Should the list use a translucent hot row highlighting mechanism (like Vista)")]
        public bool UseTranslucentHotItem
        {
            get
            {
                return this.useTranslucentHotItem;
            }
            set
            {
                this.useTranslucentHotItem = value;
                if (value)
                {
                    this.HotItemStyle = new BrightIdeasSoftware.HotItemStyle();
                    RowBorderDecoration decoration = new RowBorderDecoration {
                        BorderPen = new Pen(Color.FromArgb(0x80, Color.LightSeaGreen), 1f),
                        FillBrush = new SolidBrush(Color.FromArgb(0x30, Color.LightSeaGreen)),
                        BoundsPadding = new Size(0, 0),
                        CornerRounding = 4f
                    };
                    this.HotItemStyle.Decoration = decoration;
                }
                else
                {
                    this.HotItemStyle = null;
                }
                this.UseHotItem = value;
            }
        }

        [DefaultValue(false), Description("Should the list use a translucent selection mechanism (like Vista)"), Category("Appearance - ObjectListView")]
        public bool UseTranslucentSelection
        {
            get
            {
                return this.useTranslucentSelection;
            }
            set
            {
                this.useTranslucentSelection = value;
                if (value)
                {
                    RowBorderDecoration decoration = new RowBorderDecoration {
                        BorderPen = new Pen(Color.FromArgb(0x80, Color.Turquoise), 1f),
                        FillBrush = new SolidBrush(Color.FromArgb(0x40, Color.Turquoise)),
                        BoundsPadding = new Size(0, -1),
                        CornerRounding = 12f
                    };
                    this.SelectedRowDecoration = decoration;
                }
                else
                {
                    this.SelectedRowDecoration = null;
                }
            }
        }

        public System.Windows.Forms.View View
        {
            get
            {
                return base.View;
            }
            set
            {
                if (base.View != value)
                {
                    if (this.Frozen)
                    {
                        base.View = value;
                        this.SetupBaseImageList();
                    }
                    else
                    {
                        this.Freeze();
                        if (value == System.Windows.Forms.View.Tile)
                        {
                            this.CalculateReasonableTileSize();
                        }
                        base.View = value;
                        this.SetupBaseImageList();
                        this.Unfreeze();
                    }
                }
            }
        }

        public enum CellEditActivateMode
        {
            None,
            SingleClick,
            DoubleClick,
            F2Only
        }

        [Serializable]
        internal class ObjectListViewState
        {
            public ArrayList ColumnDisplayIndicies = new ArrayList();
            public ArrayList ColumnIsVisible = new ArrayList();
            public ArrayList ColumnWidths = new ArrayList();
            public View CurrentView;
            public bool IsShowingGroups;
            public SortOrder LastSortOrder = SortOrder.None;
            public int NumberOfColumns = 1;
            public int SortColumn = -1;
            public int VersionNumber = 1;
        }
    }
}

