namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public class SimpleDropSink : AbstractDropSink
    {
        private BrightIdeasSoftware.DropTargetLocation acceptableLocations;
        private bool autoScroll = true;
        private BillboardOverylay billboard;
        private ModelDropEventArgs dropEventArgs;
        private int dropTargetIndex = -1;
        private BrightIdeasSoftware.DropTargetLocation dropTargetLocation;
        private int dropTargetSubItemIndex = -1;
        private Color feedbackColor;
        private int keyState;
        private bool originalFullRowSelect;
        private int scrollAmount;
        private Timer timer = new Timer();

        public event EventHandler<OlvDropEventArgs> CanDrop;

        public event EventHandler<OlvDropEventArgs> Dropped;

        public event EventHandler<ModelDropEventArgs> ModelCanDrop;

        public event EventHandler<ModelDropEventArgs> ModelDropped;

        public SimpleDropSink()
        {
            this.timer.Interval = 250;
            this.timer.Tick += new EventHandler(this.timer_Tick);
            this.CanDropOnItem = true;
            this.FeedbackColor = Color.FromArgb(180, Color.MediumBlue);
            this.billboard = new BillboardOverylay();
        }

        public virtual DragDropEffects CalculateDropAction(DragEventArgs args, Point pt)
        {
            this.CalculateDropTarget(this.dropEventArgs, pt);
            this.dropEventArgs.MouseLocation = pt;
            this.dropEventArgs.InfoMessage = null;
            this.dropEventArgs.Handled = false;
            if (this.dropEventArgs.SourceListView != null)
            {
                this.dropEventArgs.TargetModel = this.ListView.GetModelObject(this.dropEventArgs.DropTargetIndex);
                this.OnModelCanDrop(this.dropEventArgs);
            }
            if (!this.dropEventArgs.Handled)
            {
                this.OnCanDrop(this.dropEventArgs);
            }
            this.UpdateAfterCanDropEvent(this.dropEventArgs);
            return this.dropEventArgs.Effect;
        }

        protected virtual void CalculateDropTarget(OlvDropEventArgs args, Point pt)
        {
            BrightIdeasSoftware.DropTargetLocation none = BrightIdeasSoftware.DropTargetLocation.None;
            int index = -1;
            int num2 = 0;
            if (this.CanDropOnBackground)
            {
                none = BrightIdeasSoftware.DropTargetLocation.Background;
            }
            OlvListViewHitTestInfo info = this.ListView.OlvHitTest(pt.X, pt.Y);
            if ((info.Item != null) && this.CanDropOnItem)
            {
                none = BrightIdeasSoftware.DropTargetLocation.Item;
                index = info.Item.Index;
                if ((info.SubItem != null) && this.CanDropOnSubItem)
                {
                    num2 = info.Item.SubItems.IndexOf(info.SubItem);
                }
            }
            if (this.CanDropBetween && (this.ListView.GetItemCount() > 0))
            {
                if (none == BrightIdeasSoftware.DropTargetLocation.Item)
                {
                    if ((pt.Y - 3) <= info.Item.Bounds.Top)
                    {
                        none = BrightIdeasSoftware.DropTargetLocation.AboveItem;
                    }
                    if ((pt.Y + 3) >= info.Item.Bounds.Bottom)
                    {
                        none = BrightIdeasSoftware.DropTargetLocation.BelowItem;
                    }
                }
                else
                {
                    info = this.ListView.OlvHitTest(pt.X, pt.Y + 3);
                    if (info.Item != null)
                    {
                        index = info.Item.Index;
                        none = BrightIdeasSoftware.DropTargetLocation.AboveItem;
                    }
                    else
                    {
                        info = this.ListView.OlvHitTest(pt.X, pt.Y - 3);
                        if (info.Item != null)
                        {
                            index = info.Item.Index;
                            none = BrightIdeasSoftware.DropTargetLocation.BelowItem;
                        }
                    }
                }
            }
            args.DropTargetLocation = none;
            args.DropTargetIndex = index;
            args.DropTargetSubItemIndex = num2;
        }

        protected virtual Rectangle CalculateDropTargetRectangle(OLVListItem item, int subItem)
        {
            if (subItem > 0)
            {
                return item.SubItems[subItem].Bounds;
            }
            Rectangle rectangle = this.ListView.CalculateCellTextBounds(item, subItem);
            if (item.IndentCount > 0)
            {
                int width = this.ListView.SmallImageSize.Width;
                rectangle.X += width * item.IndentCount;
                rectangle.Width -= width * item.IndentCount;
            }
            return rectangle;
        }

        public DragDropEffects CalculateStandardDropActionFromKeys()
        {
            if (this.IsControlDown)
            {
                if (this.IsShiftDown)
                {
                    return DragDropEffects.Link;
                }
                return DragDropEffects.Copy;
            }
            return DragDropEffects.Move;
        }

        protected virtual void CheckScrolling(Point pt)
        {
            if (this.AutoScroll)
            {
                Rectangle contentRectangle = this.ListView.ContentRectangle;
                int rowHeightEffective = this.ListView.RowHeightEffective;
                int num2 = rowHeightEffective;
                if (this.ListView.View == View.Tile)
                {
                    num2 /= 2;
                }
                if (pt.Y <= (contentRectangle.Top + num2))
                {
                    this.timer.Interval = (pt.Y <= (contentRectangle.Top + (num2 / 2))) ? 100 : 350;
                    this.timer.Start();
                    this.scrollAmount = -rowHeightEffective;
                }
                else if (pt.Y >= (contentRectangle.Bottom - num2))
                {
                    this.timer.Interval = (pt.Y >= (contentRectangle.Bottom - (num2 / 2))) ? 100 : 350;
                    this.timer.Start();
                    this.scrollAmount = rowHeightEffective;
                }
                else
                {
                    this.timer.Stop();
                }
            }
        }

        protected override void Cleanup()
        {
            this.DropTargetLocation = BrightIdeasSoftware.DropTargetLocation.None;
            this.ListView.FullRowSelect = this.originalFullRowSelect;
            this.Billboard.Text = null;
        }

        protected virtual void DrawBetweenLine(Graphics g, int x1, int y1, int x2, int y2)
        {
            using (Brush brush = new SolidBrush(this.FeedbackColor))
            {
                GraphicsPath path;
                int num = x1;
                int num2 = y1;
                using (path = new GraphicsPath())
                {
                    path.AddLine(num, num2 + 5, num, num2 - 5);
                    path.AddBezier(num, num2 - 6, num + 3, num2 - 2, num + 6, num2 - 1, num + 11, num2);
                    path.AddBezier(num + 11, num2, num + 6, num2 + 1, num + 3, num2 + 2, num, num2 + 6);
                    path.CloseFigure();
                    g.FillPath(brush, path);
                }
                num = x2;
                num2 = y2;
                using (path = new GraphicsPath())
                {
                    path.AddLine(num, num2 + 6, num, num2 - 6);
                    path.AddBezier(num, num2 - 7, num - 3, num2 - 2, num - 6, num2 - 1, num - 11, num2);
                    path.AddBezier(num - 11, num2, num - 6, num2 + 1, num - 3, num2 + 2, num, num2 + 7);
                    path.CloseFigure();
                    g.FillPath(brush, path);
                }
            }
            using (Pen pen = new Pen(this.FeedbackColor, 3f))
            {
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        public override void DrawFeedback(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            switch (this.DropTargetLocation)
            {
                case BrightIdeasSoftware.DropTargetLocation.Background:
                    this.DrawFeedbackBackgroundTarget(g, bounds);
                    break;

                case BrightIdeasSoftware.DropTargetLocation.Item:
                    this.DrawFeedbackItemTarget(g, bounds);
                    break;

                case BrightIdeasSoftware.DropTargetLocation.AboveItem:
                    this.DrawFeedbackAboveItemTarget(g, bounds);
                    break;

                case BrightIdeasSoftware.DropTargetLocation.BelowItem:
                    this.DrawFeedbackBelowItemTarget(g, bounds);
                    break;
            }
            if (this.Billboard != null)
            {
                this.Billboard.Draw(this.ListView, g, bounds);
            }
        }

        protected virtual void DrawFeedbackAboveItemTarget(Graphics g, Rectangle bounds)
        {
            if (this.DropTargetItem != null)
            {
                Rectangle rectangle = this.CalculateDropTargetRectangle(this.DropTargetItem, this.DropTargetSubItemIndex);
                this.DrawBetweenLine(g, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
            }
        }

        protected virtual void DrawFeedbackBackgroundTarget(Graphics g, Rectangle bounds)
        {
            float width = 12f;
            Rectangle rect = bounds;
            rect.Inflate(((int) -width) / 2, ((int) -width) / 2);
            using (Pen pen = new Pen(Color.FromArgb(0x80, this.FeedbackColor), width))
            {
                using (GraphicsPath path = this.GetRoundedRect(rect, 30f))
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        protected virtual void DrawFeedbackBelowItemTarget(Graphics g, Rectangle bounds)
        {
            if (this.DropTargetItem != null)
            {
                Rectangle rectangle = this.CalculateDropTargetRectangle(this.DropTargetItem, this.DropTargetSubItemIndex);
                this.DrawBetweenLine(g, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom);
            }
        }

        protected virtual void DrawFeedbackItemTarget(Graphics g, Rectangle bounds)
        {
            if (this.DropTargetItem != null)
            {
                Rectangle rect = this.CalculateDropTargetRectangle(this.DropTargetItem, this.DropTargetSubItemIndex);
                rect.Inflate(1, 1);
                float diameter = rect.Height / 3;
                using (GraphicsPath path = this.GetRoundedRect(rect, diameter))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(0x30, this.FeedbackColor)))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(this.FeedbackColor, 3f))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }
        }

        public override void Drop(DragEventArgs args)
        {
            this.TriggerDroppedEvent(args);
            this.timer.Stop();
            this.Cleanup();
        }

        public override void Enter(DragEventArgs args)
        {
            this.originalFullRowSelect = this.ListView.FullRowSelect;
            this.ListView.FullRowSelect = false;
            this.dropEventArgs = new ModelDropEventArgs();
            this.dropEventArgs.DropSink = this;
            this.dropEventArgs.ListView = this.ListView;
            this.dropEventArgs.DataObject = args.Data;
            OLVDataObject data = args.Data as OLVDataObject;
            if (data != null)
            {
                this.dropEventArgs.SourceListView = data.ListView;
                this.dropEventArgs.SourceModels = data.ModelObjects;
            }
            this.Over(args);
        }

        protected GraphicsPath GetRoundedRect(Rectangle rect, float diameter)
        {
            GraphicsPath path = new GraphicsPath();
            RectangleF ef = new RectangleF((float) rect.X, (float) rect.Y, diameter, diameter);
            path.AddArc(ef, 180f, 90f);
            ef.X = rect.Right - diameter;
            path.AddArc(ef, 270f, 90f);
            ef.Y = rect.Bottom - diameter;
            path.AddArc(ef, 0f, 90f);
            ef.X = rect.Left;
            path.AddArc(ef, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        protected virtual void HandleTimerTick()
        {
            if (((this.IsLeftMouseButtonDown && ((Control.MouseButtons & MouseButtons.Left) != MouseButtons.Left)) || (this.IsMiddleMouseButtonDown && ((Control.MouseButtons & MouseButtons.Middle) != MouseButtons.Middle))) || (this.IsRightMouseButtonDown && ((Control.MouseButtons & MouseButtons.Right) != MouseButtons.Right)))
            {
                this.timer.Stop();
                this.Cleanup();
            }
            else
            {
                Point pt = this.ListView.PointToClient(Cursor.Position);
                Rectangle clientRectangle = this.ListView.ClientRectangle;
                clientRectangle.Inflate(30, 30);
                if (clientRectangle.Contains(pt))
                {
                    this.ListView.LowLevelScroll(0, this.scrollAmount);
                }
            }
        }

        protected virtual void OnCanDrop(OlvDropEventArgs args)
        {
            if (this.CanDrop != null)
            {
                this.CanDrop(this, args);
            }
        }

        protected virtual void OnDropped(OlvDropEventArgs args)
        {
            if (this.Dropped != null)
            {
                this.Dropped(this, args);
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

        public override void Over(DragEventArgs args)
        {
            this.KeyState = args.KeyState;
            Point pt = this.ListView.PointToClient(new Point(args.X, args.Y));
            args.Effect = this.CalculateDropAction(args, pt);
            this.CheckScrolling(pt);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.HandleTimerTick();
        }

        protected virtual void TriggerDroppedEvent(DragEventArgs args)
        {
            this.dropEventArgs.Handled = false;
            if (this.dropEventArgs.SourceListView != null)
            {
                this.OnModelDropped(this.dropEventArgs);
            }
            if (!this.dropEventArgs.Handled)
            {
                this.OnDropped(this.dropEventArgs);
            }
        }

        protected virtual void UpdateAfterCanDropEvent(OlvDropEventArgs args)
        {
            this.DropTargetIndex = args.DropTargetIndex;
            this.DropTargetLocation = args.DropTargetLocation;
            this.DropTargetSubItemIndex = args.DropTargetSubItemIndex;
            if (this.Billboard != null)
            {
                Point mouseLocation = args.MouseLocation;
                mouseLocation.Offset(5, 5);
                if ((this.Billboard.Text != this.dropEventArgs.InfoMessage) || (this.Billboard.Location != mouseLocation))
                {
                    this.Billboard.Text = this.dropEventArgs.InfoMessage;
                    this.Billboard.Location = mouseLocation;
                    this.ListView.Invalidate();
                }
            }
        }

        public BrightIdeasSoftware.DropTargetLocation AcceptableLocations
        {
            get
            {
                return this.acceptableLocations;
            }
            set
            {
                this.acceptableLocations = value;
            }
        }

        public bool AutoScroll
        {
            get
            {
                return this.autoScroll;
            }
            set
            {
                this.autoScroll = value;
            }
        }

        public BillboardOverylay Billboard
        {
            get
            {
                return this.billboard;
            }
            set
            {
                this.billboard = value;
            }
        }

        public bool CanDropBetween
        {
            get
            {
                return ((this.AcceptableLocations & BrightIdeasSoftware.DropTargetLocation.BetweenItems) == BrightIdeasSoftware.DropTargetLocation.BetweenItems);
            }
            set
            {
                if (value)
                {
                    this.AcceptableLocations |= BrightIdeasSoftware.DropTargetLocation.BetweenItems;
                }
                else
                {
                    this.AcceptableLocations &= ~BrightIdeasSoftware.DropTargetLocation.BetweenItems;
                }
            }
        }

        public bool CanDropOnBackground
        {
            get
            {
                return ((this.AcceptableLocations & BrightIdeasSoftware.DropTargetLocation.Background) == BrightIdeasSoftware.DropTargetLocation.Background);
            }
            set
            {
                if (value)
                {
                    this.AcceptableLocations |= BrightIdeasSoftware.DropTargetLocation.Background;
                }
                else
                {
                    this.AcceptableLocations &= ~BrightIdeasSoftware.DropTargetLocation.Background;
                }
            }
        }

        public bool CanDropOnItem
        {
            get
            {
                return ((this.AcceptableLocations & BrightIdeasSoftware.DropTargetLocation.Item) == BrightIdeasSoftware.DropTargetLocation.Item);
            }
            set
            {
                if (value)
                {
                    this.AcceptableLocations |= BrightIdeasSoftware.DropTargetLocation.Item;
                }
                else
                {
                    this.AcceptableLocations &= ~BrightIdeasSoftware.DropTargetLocation.Item;
                }
            }
        }

        public bool CanDropOnSubItem
        {
            get
            {
                return ((this.AcceptableLocations & BrightIdeasSoftware.DropTargetLocation.SubItem) == BrightIdeasSoftware.DropTargetLocation.SubItem);
            }
            set
            {
                if (value)
                {
                    this.AcceptableLocations |= BrightIdeasSoftware.DropTargetLocation.SubItem;
                }
                else
                {
                    this.AcceptableLocations &= ~BrightIdeasSoftware.DropTargetLocation.SubItem;
                }
            }
        }

        public int DropTargetIndex
        {
            get
            {
                return this.dropTargetIndex;
            }
            set
            {
                if (this.dropTargetIndex != value)
                {
                    this.dropTargetIndex = value;
                    this.ListView.Invalidate();
                }
            }
        }

        public OLVListItem DropTargetItem
        {
            get
            {
                return this.ListView.GetItem(this.DropTargetIndex);
            }
        }

        public BrightIdeasSoftware.DropTargetLocation DropTargetLocation
        {
            get
            {
                return this.dropTargetLocation;
            }
            set
            {
                if (this.dropTargetLocation != value)
                {
                    this.dropTargetLocation = value;
                    this.ListView.Invalidate();
                }
            }
        }

        public int DropTargetSubItemIndex
        {
            get
            {
                return this.dropTargetSubItemIndex;
            }
            set
            {
                if (this.dropTargetSubItemIndex != value)
                {
                    this.dropTargetSubItemIndex = value;
                    this.ListView.Invalidate();
                }
            }
        }

        public Color FeedbackColor
        {
            get
            {
                return this.feedbackColor;
            }
            set
            {
                this.feedbackColor = value;
            }
        }

        public bool IsAltDown
        {
            get
            {
                return ((this.KeyState & 0x20) == 0x20);
            }
        }

        public bool IsAnyModifierDown
        {
            get
            {
                return ((this.KeyState & 0x2c) != 0);
            }
        }

        public bool IsControlDown
        {
            get
            {
                return ((this.KeyState & 8) == 8);
            }
        }

        public bool IsLeftMouseButtonDown
        {
            get
            {
                return ((this.KeyState & 1) == 1);
            }
        }

        public bool IsMiddleMouseButtonDown
        {
            get
            {
                return ((this.KeyState & 0x10) == 0x10);
            }
        }

        public bool IsRightMouseButtonDown
        {
            get
            {
                return ((this.KeyState & 2) == 2);
            }
        }

        public bool IsShiftDown
        {
            get
            {
                return ((this.KeyState & 4) == 4);
            }
        }

        public int KeyState
        {
            get
            {
                return this.keyState;
            }
            set
            {
                this.keyState = value;
            }
        }
    }
}

