namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    public class HeaderControl : NativeWindow
    {
        private bool cachedNeedsCustomDraw;
        private int columnShowingTip = -1;
        private FontStyle fontStyle = FontStyle.Regular;
        private ObjectListView listView;
        private ToolTipControl toolTip;
        private bool wordWrap;

        public HeaderControl(ObjectListView olv)
        {
            this.ListView = olv;
            base.AssignHandle(BrightIdeasSoftware.NativeMethods.GetHeaderControl(olv));
        }

        private Font CalculateFont(OLVColumn column)
        {
            Font prototype = column.HeaderFont ?? (this.ListView.HeaderFont ?? this.ListView.Font);
            if ((this.HotFontStyle != FontStyle.Regular) && (column.Index == this.ColumnIndexUnderCursor))
            {
                prototype = new Font(prototype, this.HotFontStyle);
            }
            return prototype;
        }

        protected int CalculateHeight(Graphics g)
        {
            System.Windows.Forms.TextFormatFlags textFormatFlags = this.TextFormatFlags;
            float num = 0f;
            for (int i = 0; i < this.ListView.Columns.Count; i++)
            {
                OLVColumn column = this.ListView.GetColumn(i);
                Font font = this.CalculateFont(column);
                if (this.WordWrap)
                {
                    Rectangle itemRect = this.GetItemRect(i);
                    itemRect.Width -= 6;
                    if (this.HasNonThemedSortIndicator(column))
                    {
                        itemRect.Width -= 0x10;
                    }
                    SizeF ef = (SizeF) TextRenderer.MeasureText(g, column.Text, font, new Size(itemRect.Width, 100), textFormatFlags);
                    num = Math.Max(num, ef.Height);
                }
                else
                {
                    num = Math.Max(num, (float) font.Height);
                }
            }
            return (7 + ((int) num));
        }

        protected virtual void CreateToolTip()
        {
            this.ToolTip = new ToolTipControl();
            this.ToolTip.Create(this.Handle);
            this.ToolTip.AddTool(this);
            this.ToolTip.Showing += new EventHandler<ToolTipShowingEventArgs>(this.ListView.headerToolTip_Showing);
        }

        protected void CustomDrawHeaderCell(Graphics g, int columnIndex, int itemState)
        {
            Rectangle itemRect = this.GetItemRect(columnIndex);
            OLVColumn column = this.ListView.GetColumn(columnIndex);
            int columnIndexUnderCursor = this.ColumnIndexUnderCursor;
            if (VisualStyleRenderer.IsSupported && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.Item.Normal))
            {
                int part = 1;
                if ((columnIndex == 0) && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.ItemLeft.Normal))
                {
                    part = 2;
                }
                if ((columnIndex == (this.ListView.Columns.Count - 1)) && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.ItemRight.Normal))
                {
                    part = 3;
                }
                int state = 1;
                if ((itemState & 1) == 1)
                {
                    state = 3;
                }
                else if (columnIndex == this.ColumnIndexUnderCursor)
                {
                    state = 2;
                }
                new VisualStyleRenderer("HEADER", part, state).DrawBackground(g, itemRect);
            }
            else
            {
                ControlPaint.DrawBorder3D(g, itemRect, Border3DStyle.Raised);
            }
            if (this.HasSortIndicator(column))
            {
                if (VisualStyleRenderer.IsSupported && VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.SortArrow.SortedUp))
                {
                    VisualStyleRenderer renderer2 = null;
                    if (this.ListView.LastSortOrder == SortOrder.Ascending)
                    {
                        renderer2 = new VisualStyleRenderer(VisualStyleElement.Header.SortArrow.SortedUp);
                    }
                    if (this.ListView.LastSortOrder == SortOrder.Descending)
                    {
                        renderer2 = new VisualStyleRenderer(VisualStyleElement.Header.SortArrow.SortedDown);
                    }
                    if (renderer2 != null)
                    {
                        Size partSize = renderer2.GetPartSize(g, ThemeSizeType.True);
                        Point location = renderer2.GetPoint(PointProperty.Offset);
                        if ((location.X == 0) && (location.Y == 0))
                        {
                            location = new Point((itemRect.X + (itemRect.Width / 2)) - (partSize.Width / 2), itemRect.Y);
                        }
                        renderer2.DrawBackground(g, new Rectangle(location, partSize));
                    }
                }
                else
                {
                    Point point2 = new Point((itemRect.Right - 0x10) - 2, itemRect.Top + ((itemRect.Height - 0x10) / 2));
                    Point[] points = new Point[] { point2, point2, point2 };
                    if (this.ListView.LastSortOrder == SortOrder.Ascending)
                    {
                        points[0].Offset(2, 10);
                        points[1].Offset(8, 3);
                        points[2].Offset(14, 10);
                    }
                    else
                    {
                        points[0].Offset(2, 4);
                        points[1].Offset(8, 10);
                        points[2].Offset(14, 4);
                    }
                    g.FillPolygon(Brushes.SlateGray, points);
                    itemRect.Width -= 0x10;
                }
            }
            System.Windows.Forms.TextFormatFlags textFormatFlags = this.TextFormatFlags;
            if (column.TextAlign == HorizontalAlignment.Center)
            {
                textFormatFlags |= System.Windows.Forms.TextFormatFlags.HorizontalCenter;
            }
            if (column.TextAlign == HorizontalAlignment.Right)
            {
                textFormatFlags |= System.Windows.Forms.TextFormatFlags.Right;
            }
            Font font = this.CalculateFont(column);
            Color headerForeColor = column.HeaderForeColor;
            if (headerForeColor.IsEmpty)
            {
                headerForeColor = this.ListView.ForeColor;
            }
            itemRect.Inflate(-3, 0);
            itemRect.Y -= 2;
            TextRenderer.DrawText(g, column.Text, font, itemRect, headerForeColor, Color.Transparent, textFormatFlags);
        }

        public Rectangle GetItemRect(int itemIndex)
        {
            BrightIdeasSoftware.NativeMethods.RECT r = new BrightIdeasSoftware.NativeMethods.RECT();
            BrightIdeasSoftware.NativeMethods.SendMessageRECT(this.Handle, 0x1207, itemIndex, ref r);
            return Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);
        }

        protected bool HandleDestroy(ref Message m)
        {
            if (this.ToolTip != null)
            {
                this.ToolTip.Showing -= new EventHandler<ToolTipShowingEventArgs>(this.ListView.headerToolTip_Showing);
            }
            return false;
        }

        internal virtual bool HandleHeaderCustomDraw(ref Message m)
        {
            BrightIdeasSoftware.NativeMethods.NMCUSTOMDRAW lParam = (BrightIdeasSoftware.NativeMethods.NMCUSTOMDRAW) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMCUSTOMDRAW));
            switch (lParam.dwDrawStage)
            {
                case 1:
                    this.cachedNeedsCustomDraw = this.NeedsCustomDraw;
                    m.Result = (IntPtr) 0x20;
                    return true;

                case 0x10001:
                {
                    int index = lParam.dwItemSpec.ToInt32();
                    OLVColumn column = this.ListView.GetColumn(index);
                    if (!this.cachedNeedsCustomDraw)
                    {
                        Font prototype = column.HeaderFont ?? (this.ListView.HeaderFont ?? this.ListView.Font);
                        if ((this.HotFontStyle != FontStyle.Regular) && (index == this.ColumnIndexUnderCursor))
                        {
                            prototype = new Font(prototype, this.HotFontStyle);
                        }
                        BrightIdeasSoftware.NativeMethods.SelectObject(lParam.hdc, prototype.ToHfont());
                        m.Result = (IntPtr) 2;
                        break;
                    }
                    using (Graphics graphics = Graphics.FromHdc(lParam.hdc))
                    {
                        graphics.TextRenderingHint = ObjectListView.TextRendereringHint;
                        this.CustomDrawHeaderCell(graphics, index, lParam.uItemState);
                    }
                    m.Result = (IntPtr) 4;
                    break;
                }
                default:
                    return false;
            }
            return true;
        }

        protected bool HandleLayout(ref Message m)
        {
            if (this.ListView.HeaderStyle == ColumnHeaderStyle.None)
            {
                return true;
            }
            BrightIdeasSoftware.NativeMethods.HDLAYOUT lParam = (BrightIdeasSoftware.NativeMethods.HDLAYOUT) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.HDLAYOUT));
            BrightIdeasSoftware.NativeMethods.RECT structure = (BrightIdeasSoftware.NativeMethods.RECT) Marshal.PtrToStructure(lParam.prc, typeof(BrightIdeasSoftware.NativeMethods.RECT));
            BrightIdeasSoftware.NativeMethods.WINDOWPOS windowpos = (BrightIdeasSoftware.NativeMethods.WINDOWPOS) Marshal.PtrToStructure(lParam.pwpos, typeof(BrightIdeasSoftware.NativeMethods.WINDOWPOS));
            using (Graphics graphics = this.ListView.CreateGraphics())
            {
                graphics.TextRenderingHint = ObjectListView.TextRendereringHint;
                int num = this.CalculateHeight(graphics);
                windowpos.hwnd = this.Handle;
                windowpos.hwndInsertAfter = IntPtr.Zero;
                windowpos.flags = 0x20;
                windowpos.x = structure.left;
                windowpos.y = structure.top;
                windowpos.cx = structure.right - structure.left;
                windowpos.cy = num;
                structure.top = num;
                Marshal.StructureToPtr(structure, lParam.prc, false);
                Marshal.StructureToPtr(windowpos, lParam.pwpos, false);
            }
            this.ListView.BeginInvoke(delegate {
                this.Invalidate();
                this.ListView.Invalidate();
            });
            return false;
        }

        protected bool HandleMouseMove(ref Message m)
        {
            int columnIndexUnderCursor = this.ColumnIndexUnderCursor;
            if (columnIndexUnderCursor != this.columnShowingTip)
            {
                this.ToolTip.PopToolTip(this);
                this.columnShowingTip = columnIndexUnderCursor;
            }
            return true;
        }

        protected bool HandleNotify(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                BrightIdeasSoftware.NativeMethods.NMHDR lParam = (BrightIdeasSoftware.NativeMethods.NMHDR) m.GetLParam(typeof(BrightIdeasSoftware.NativeMethods.NMHDR));
                switch (lParam.code)
                {
                    case -522:
                        return this.ToolTip.HandlePop(ref m);

                    case -521:
                        return this.ToolTip.HandleShow(ref m);

                    case -530:
                        return this.ToolTip.HandleGetDispInfo(ref m);
                }
            }
            return false;
        }

        protected bool HandleSetCursor(ref Message m)
        {
            if (this.IsCursorOverLockedDivider)
            {
                m.Result = (IntPtr) 1;
                return false;
            }
            return true;
        }

        protected bool HasNonThemedSortIndicator(OLVColumn column)
        {
            if (VisualStyleRenderer.IsSupported)
            {
                return (!VisualStyleRenderer.IsElementDefined(VisualStyleElement.Header.SortArrow.SortedUp) && this.HasSortIndicator(column));
            }
            return this.HasSortIndicator(column);
        }

        protected bool HasSortIndicator(OLVColumn column)
        {
            return ((column == this.ListView.LastSortColumn) && (this.ListView.LastSortOrder != SortOrder.None));
        }

        public void Invalidate()
        {
            BrightIdeasSoftware.NativeMethods.InvalidateRect(this.Handle, 0, true);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                    if (!this.HandleNotify(ref m))
                    {
                        return;
                    }
                    break;

                case 0x200:
                    if (!this.HandleMouseMove(ref m))
                    {
                        return;
                    }
                    break;

                case 0x1205:
                    if (!this.HandleLayout(ref m))
                    {
                        return;
                    }
                    break;

                case 2:
                    if (!this.HandleDestroy(ref m))
                    {
                        return;
                    }
                    break;

                case 0x20:
                    if (!this.HandleSetCursor(ref m))
                    {
                        return;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        public int ColumnIndexUnderCursor
        {
            get
            {
                Point pt = this.ListView.PointToClient(Cursor.Position);
                pt.X += BrightIdeasSoftware.NativeMethods.GetScrollPosition(this.ListView, true);
                return BrightIdeasSoftware.NativeMethods.GetColumnUnderPoint(this.Handle, pt);
            }
        }

        public IntPtr Handle
        {
            get
            {
                return BrightIdeasSoftware.NativeMethods.GetHeaderControl(this.ListView);
            }
        }

        public FontStyle HotFontStyle
        {
            get
            {
                return this.fontStyle;
            }
            set
            {
                this.fontStyle = value;
            }
        }

        protected bool IsCursorOverLockedDivider
        {
            get
            {
                Point pt = this.ListView.PointToClient(Cursor.Position);
                pt.X += BrightIdeasSoftware.NativeMethods.GetScrollPosition(this.ListView, true);
                int dividerUnderPoint = BrightIdeasSoftware.NativeMethods.GetDividerUnderPoint(this.Handle, pt);
                if ((dividerUnderPoint >= 0) && (dividerUnderPoint < this.ListView.Columns.Count))
                {
                    OLVColumn column = this.ListView.GetColumn(dividerUnderPoint);
                    return (column.IsFixedWidth || column.FillsFreeSpace);
                }
                return false;
            }
        }

        protected ObjectListView ListView
        {
            get
            {
                return this.listView;
            }
            set
            {
                this.listView = value;
            }
        }

        protected bool NeedsCustomDraw
        {
            get
            {
                if (this.WordWrap)
                {
                    return true;
                }
                foreach (OLVColumn column in this.ListView.Columns)
                {
                    if (!column.HeaderForeColor.IsEmpty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private System.Windows.Forms.TextFormatFlags TextFormatFlags
        {
            get
            {
                System.Windows.Forms.TextFormatFlags flags = System.Windows.Forms.TextFormatFlags.NoPadding | System.Windows.Forms.TextFormatFlags.PreserveGraphicsTranslateTransform | System.Windows.Forms.TextFormatFlags.WordEllipsis | System.Windows.Forms.TextFormatFlags.EndEllipsis | System.Windows.Forms.TextFormatFlags.NoPrefix | System.Windows.Forms.TextFormatFlags.VerticalCenter;
                if (this.WordWrap)
                {
                    flags |= System.Windows.Forms.TextFormatFlags.WordBreak;
                }
                else
                {
                    flags |= System.Windows.Forms.TextFormatFlags.SingleLine;
                }
                if (this.ListView.RightToLeft == RightToLeft.Yes)
                {
                    flags |= System.Windows.Forms.TextFormatFlags.RightToLeft;
                }
                return flags;
            }
        }

        public ToolTipControl ToolTip
        {
            get
            {
                if (this.toolTip == null)
                {
                    this.CreateToolTip();
                }
                return this.toolTip;
            }
            protected set
            {
                this.toolTip = value;
            }
        }

        public bool WordWrap
        {
            get
            {
                return this.wordWrap;
            }
            set
            {
                this.wordWrap = value;
            }
        }
    }
}

