namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    [ToolboxItem(true), Browsable(true)]
    public class BaseRenderer : AbstractRenderer
    {
        private object aspect;
        private Rectangle bounds;
        private bool canWrap;
        private OLVColumn column;
        private DrawListViewItemEventArgs drawItemEventArgs;
        private DrawListViewSubItemEventArgs eventArgs;
        private System.Drawing.Font font;
        private System.Windows.Forms.ImageList imageList;
        private bool isItemSelected;
        private bool isPrinting;
        private OLVListItem listItem;
        private OLVListSubItem listSubItem;
        private ObjectListView objectListView;
        private object rowObject;
        private int spacing = 1;
        private Brush textBrush;
        private bool useGdiTextRendering = true;

        protected virtual Rectangle AlignRectangle(Rectangle outer, Rectangle inner)
        {
            Rectangle rectangle = new Rectangle(outer.Location, inner.Size);
            if (inner.Width < outer.Width)
            {
                switch (this.Column.TextAlign)
                {
                    case HorizontalAlignment.Left:
                        rectangle.X = outer.Left;
                        break;

                    case HorizontalAlignment.Right:
                        rectangle.X = (outer.Right - inner.Width) - 1;
                        break;

                    case HorizontalAlignment.Center:
                        rectangle.X = outer.Left + ((outer.Width - inner.Width) / 2);
                        break;
                }
            }
            if (inner.Height < outer.Height)
            {
                rectangle.Y = outer.Top + ((outer.Height - inner.Height) / 2);
            }
            return rectangle;
        }

        protected virtual Rectangle CalculateAlignedRectangle(Graphics g, Rectangle r)
        {
            if (this.Column.TextAlign == HorizontalAlignment.Left)
            {
                return r;
            }
            int width = this.CalculateCheckBoxWidth(g) + this.CalculateImageWidth(g, this.GetImageSelector());
            width += this.CalculateTextWidth(g, this.GetText());
            if (width >= r.Width)
            {
                return r;
            }
            return this.AlignRectangle(r, new Rectangle(0, 0, width, r.Height));
        }

        protected virtual int CalculateCheckBoxWidth(Graphics g)
        {
            if (this.ListView.CheckBoxes && (this.Column.Index == 0))
            {
                return (CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.UncheckedNormal).Width + 6);
            }
            return 0;
        }

        protected virtual int CalculateImageWidth(Graphics g, object imageSelector)
        {
            if ((imageSelector != null) && (imageSelector != DBNull.Value))
            {
                System.Windows.Forms.ImageList imageListOrDefault = this.ImageListOrDefault;
                if (imageListOrDefault != null)
                {
                    int num = -1;
                    if (imageSelector is int)
                    {
                        num = (int) imageSelector;
                    }
                    else
                    {
                        string key = imageSelector as string;
                        if (key != null)
                        {
                            num = imageListOrDefault.Images.IndexOfKey(key);
                        }
                    }
                    if (num >= 0)
                    {
                        return imageListOrDefault.ImageSize.Width;
                    }
                }
                Image image = imageSelector as Image;
                if (image != null)
                {
                    return image.Width;
                }
            }
            return 0;
        }

        protected virtual int CalculateTextWidth(Graphics g, string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return 0;
            }
            if (this.UseGdiTextRendering)
            {
                Size proposedSize = new Size(0x7fffffff, 0x7fffffff);
                return TextRenderer.MeasureText(g, txt, this.Font, proposedSize, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix).Width;
            }
            using (StringFormat format = new StringFormat())
            {
                format.Trimming = StringTrimming.EllipsisCharacter;
                return (1 + ((int) g.MeasureString(txt, this.Font, 0x7fffffff, format).Width));
            }
        }

        private void ClearState()
        {
            this.Event = null;
            this.DrawItemEvent = null;
            this.Aspect = null;
            this.Font = null;
            this.TextBrush = null;
        }

        protected virtual void DrawAlignedImage(Graphics g, Rectangle r, Image image)
        {
            if (image != null)
            {
                Rectangle inner = new Rectangle(r.Location, image.Size);
                if (image.Height > r.Height)
                {
                    float num = ((float) r.Height) / ((float) image.Height);
                    inner.Width = (int) (image.Width * num);
                    inner.Height = r.Height - 1;
                }
                g.DrawImage(image, this.AlignRectangle(r, inner));
            }
        }

        protected virtual void DrawAlignedImageAndText(Graphics g, Rectangle r)
        {
            this.DrawImageAndText(g, this.CalculateAlignedRectangle(g, r));
        }

        protected virtual void DrawBackground(Graphics g, Rectangle r)
        {
            if (this.IsDrawBackground)
            {
                using (Brush brush = new SolidBrush(this.GetBackgroundColor()))
                {
                    g.FillRectangle(brush, (int) (r.X - 1), (int) (r.Y - 1), (int) (r.Width + 2), (int) (r.Height + 2));
                }
            }
        }

        protected virtual int DrawCheckBox(Graphics g, Rectangle r)
        {
            int stateImageIndex = this.ListItem.StateImageIndex;
            if (this.IsPrinting)
            {
                if ((this.ListView.StateImageList == null) || (stateImageIndex < 0))
                {
                    return 0;
                }
                return (this.DrawImage(g, r, this.ListView.StateImageList.Images[stateImageIndex]) + 4);
            }
            CheckBoxState uncheckedNormal = CheckBoxState.UncheckedNormal;
            switch ((stateImageIndex << 4))
            {
                case 0:
                    uncheckedNormal = CheckBoxState.UncheckedNormal;
                    break;

                case 1:
                    uncheckedNormal = CheckBoxState.UncheckedHot;
                    break;

                case 0x10:
                    uncheckedNormal = CheckBoxState.CheckedNormal;
                    break;

                case 0x11:
                    uncheckedNormal = CheckBoxState.CheckedHot;
                    break;

                case 0x20:
                    uncheckedNormal = CheckBoxState.MixedNormal;
                    break;

                case 0x21:
                    uncheckedNormal = CheckBoxState.MixedHot;
                    break;
            }
            CheckBoxRenderer.DrawCheckBox(g, new Point(r.X + 3, (r.Y + (r.Height / 2)) - 6), uncheckedNormal);
            return (CheckBoxRenderer.GetGlyphSize(g, uncheckedNormal).Width + 6);
        }

        protected virtual int DrawImage(Graphics g, Rectangle r, object imageSelector)
        {
            if ((imageSelector != null) && (imageSelector != DBNull.Value))
            {
                System.Windows.Forms.ImageList baseSmallImageList = this.ListView.BaseSmallImageList;
                if (baseSmallImageList != null)
                {
                    int index = -1;
                    if (imageSelector is int)
                    {
                        index = (int) imageSelector;
                    }
                    else
                    {
                        string key = imageSelector as string;
                        if (key != null)
                        {
                            index = baseSmallImageList.Images.IndexOfKey(key);
                        }
                    }
                    if (index >= 0)
                    {
                        if (!this.IsPrinting)
                        {
                            Rectangle rectangle = new Rectangle(r.X - this.Bounds.X, r.Y - this.Bounds.Y, r.Width, r.Height);
                            baseSmallImageList.Draw(g, rectangle.Location, index);
                            return baseSmallImageList.ImageSize.Width;
                        }
                        imageSelector = baseSmallImageList.Images[index];
                    }
                }
                Image image = imageSelector as Image;
                if (image != null)
                {
                    int y = r.Y;
                    if (image.Size.Height < r.Height)
                    {
                        y += (r.Height - image.Size.Height) / 2;
                    }
                    g.DrawImageUnscaled(image, r.X, y);
                    return image.Width;
                }
            }
            return 0;
        }

        protected virtual void DrawImageAndText(Graphics g, Rectangle r)
        {
            int num = 0;
            if (this.ListView.CheckBoxes && (this.Column.Index == 0))
            {
                num = this.DrawCheckBox(g, r);
                r.X += num;
                r.Width -= num;
            }
            num = this.DrawImage(g, r, this.GetImageSelector());
            r.X += num;
            r.Width -= num;
            this.DrawText(g, r, this.GetText());
        }

        protected virtual void DrawImages(Graphics g, Rectangle r, ICollection imageSelectors)
        {
            List<Image> list = new List<Image>();
            foreach (object obj2 in imageSelectors)
            {
                Image item = this.GetImage(obj2);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            int width = 0;
            int num2 = 0;
            foreach (Image image in list)
            {
                width += image.Width + this.Spacing;
                num2 = Math.Max(num2, image.Height);
            }
            Point location = this.AlignRectangle(r, new Rectangle(0, 0, width, num2)).Location;
            foreach (Image image in list)
            {
                g.DrawImage(image, location);
                location.X += image.Width + this.Spacing;
            }
        }

        protected virtual void DrawText(Graphics g, Rectangle r, string txt)
        {
            if (!string.IsNullOrEmpty(txt))
            {
                if (this.UseGdiTextRendering)
                {
                    this.DrawTextGdi(g, r, txt);
                }
                else
                {
                    this.DrawTextGdiPlus(g, r, txt);
                }
            }
        }

        protected virtual void DrawTextGdi(Graphics g, Rectangle r, string txt)
        {
            Color transparent = Color.Transparent;
            if (!(((!this.IsDrawBackground || !this.IsItemSelected) || (this.Column.Index != 0)) || this.ListView.FullRowSelect))
            {
                transparent = this.GetTextBackgroundColor();
            }
            TextFormatFlags flags = TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter;
            if (!this.CanWrap)
            {
                flags |= TextFormatFlags.SingleLine;
            }
            TextRenderer.DrawText(g, txt, this.Font, r, this.GetForegroundColor(), transparent, flags);
        }

        protected virtual void DrawTextGdiPlus(Graphics g, Rectangle r, string txt)
        {
            using (StringFormat format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.Alignment = this.Column.TextStringAlign;
                if (!this.CanWrap)
                {
                    format.FormatFlags = StringFormatFlags.NoWrap;
                }
                System.Drawing.Font font = this.Font;
                if (((this.IsDrawBackground && this.IsItemSelected) && (this.Column.Index == 0)) && !this.ListView.FullRowSelect)
                {
                    SizeF ef = g.MeasureString(txt, font, r.Width, format);
                    Rectangle rect = r;
                    rect.Width = ((int) ef.Width) + 1;
                    using (Brush brush = new SolidBrush(this.ListView.HighlightBackgroundColorOrDefault))
                    {
                        g.FillRectangle(brush, rect);
                    }
                }
                RectangleF layoutRectangle = r;
                g.DrawString(txt, font, this.TextBrush, layoutRectangle, format);
            }
        }

        protected virtual Color GetBackgroundColor()
        {
            if (!this.ListView.Enabled)
            {
                return SystemColors.Control;
            }
            if ((this.IsItemSelected && !this.ListView.UseTranslucentSelection) && this.ListView.FullRowSelect)
            {
                if (this.ListView.Focused)
                {
                    return this.ListView.HighlightBackgroundColorOrDefault;
                }
                if (!this.ListView.HideSelection)
                {
                    return this.ListView.UnfocusedHighlightBackgroundColorOrDefault;
                }
            }
            if ((this.SubItem == null) || this.ListItem.UseItemStyleForSubItems)
            {
                return this.ListItem.BackColor;
            }
            return this.SubItem.BackColor;
        }

        public override Rectangle GetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex)
        {
            this.ClearState();
            this.ListView = (ObjectListView) item.ListView;
            this.ListItem = item;
            this.SubItem = item.GetSubItem(subItemIndex);
            this.Column = this.ListView.GetColumn(subItemIndex);
            this.RowObject = item.RowObject;
            this.IsItemSelected = this.ListItem.Selected;
            this.Bounds = cellBounds;
            return this.HandleGetEditRectangle(g, cellBounds, item, subItemIndex);
        }

        protected virtual Color GetForegroundColor()
        {
            if ((this.IsItemSelected && !this.ListView.UseTranslucentSelection) && ((this.Column.Index == 0) || this.ListView.FullRowSelect))
            {
                if (this.ListView.Focused)
                {
                    return this.ListView.HighlightForegroundColorOrDefault;
                }
                if (!this.ListView.HideSelection)
                {
                    return this.ListView.UnfocusedHighlightForegroundColorOrDefault;
                }
            }
            if ((this.SubItem == null) || this.ListItem.UseItemStyleForSubItems)
            {
                return this.ListItem.ForeColor;
            }
            return this.SubItem.ForeColor;
        }

        protected virtual Image GetImage()
        {
            return this.GetImage(this.GetImageSelector());
        }

        protected virtual Image GetImage(object imageSelector)
        {
            if ((imageSelector == null) || (imageSelector == DBNull.Value))
            {
                return null;
            }
            System.Windows.Forms.ImageList imageListOrDefault = this.ImageListOrDefault;
            if (imageListOrDefault != null)
            {
                if (imageSelector is int)
                {
                    int num = (int) imageSelector;
                    if ((num < 0) || (num >= imageListOrDefault.Images.Count))
                    {
                        return null;
                    }
                    return imageListOrDefault.Images[num];
                }
                string key = imageSelector as string;
                if (key != null)
                {
                    if (imageListOrDefault.Images.ContainsKey(key))
                    {
                        return imageListOrDefault.Images[key];
                    }
                    return null;
                }
            }
            return (imageSelector as Image);
        }

        protected virtual object GetImageSelector()
        {
            if (this.Column.Index == 0)
            {
                return this.ListItem.ImageSelector;
            }
            return this.OLVSubItem.ImageSelector;
        }

        protected virtual string GetText()
        {
            if (this.SubItem == null)
            {
                return this.ListItem.Text;
            }
            return this.SubItem.Text;
        }

        protected virtual Color GetTextBackgroundColor()
        {
            if ((this.IsItemSelected && !this.ListView.UseTranslucentSelection) && ((this.Column.Index == 0) || this.ListView.FullRowSelect))
            {
                if (this.ListView.Focused)
                {
                    return this.ListView.HighlightBackgroundColorOrDefault;
                }
                if (!this.ListView.HideSelection)
                {
                    return this.ListView.UnfocusedHighlightBackgroundColorOrDefault;
                }
            }
            if ((this.SubItem == null) || this.ListItem.UseItemStyleForSubItems)
            {
                return this.ListItem.BackColor;
            }
            return this.SubItem.BackColor;
        }

        protected virtual Rectangle HandleGetEditRectangle(Graphics g, Rectangle cellBounds, OLVListItem item, int subItemIndex)
        {
            if (base.GetType() == typeof(BaseRenderer))
            {
                return this.StandardGetEditRectangle(g, cellBounds);
            }
            return cellBounds;
        }

        protected virtual void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            Rectangle bounds = this.CalculateAlignedRectangle(g, this.Bounds);
            this.StandardHitTest(g, hti, bounds, x, y);
        }

        public override void HitTest(OlvListViewHitTestInfo hti, int x, int y)
        {
            this.ClearState();
            this.ListView = hti.ListView;
            this.ListItem = hti.Item;
            this.SubItem = hti.SubItem;
            this.Column = hti.Column;
            this.RowObject = hti.RowObject;
            this.IsItemSelected = this.ListItem.Selected;
            if (this.SubItem == null)
            {
                this.Bounds = this.ListItem.Bounds;
            }
            else
            {
                this.Bounds = this.ListItem.GetSubItemBounds(this.Column.Index);
            }
            this.HandleHitTest(this.ListView.CreateGraphics(), hti, x, y);
        }

        public virtual bool OptionalRender(Graphics g, Rectangle r)
        {
            if (this.ListView.View == View.Details)
            {
                this.Render(g, r);
                return true;
            }
            return false;
        }

        public virtual void Render(Graphics g, Rectangle r)
        {
            this.StandardRender(g, r);
        }

        public override bool RenderItem(DrawListViewItemEventArgs e, Graphics g, Rectangle itemBounds, object rowObject)
        {
            this.ClearState();
            this.DrawItemEvent = e;
            this.ListItem = (OLVListItem) e.Item;
            this.SubItem = null;
            this.ListView = (ObjectListView) this.ListItem.ListView;
            this.Column = this.ListView.GetColumn(0);
            this.RowObject = rowObject;
            this.Bounds = itemBounds;
            this.IsItemSelected = this.ListItem.Selected;
            return this.OptionalRender(g, itemBounds);
        }

        public override bool RenderSubItem(DrawListViewSubItemEventArgs e, Graphics g, Rectangle cellBounds, object rowObject)
        {
            this.ClearState();
            this.Event = e;
            this.ListItem = (OLVListItem) e.Item;
            this.SubItem = (OLVListSubItem) e.SubItem;
            this.ListView = (ObjectListView) this.ListItem.ListView;
            this.Column = (OLVColumn) e.Header;
            this.RowObject = rowObject;
            this.Bounds = cellBounds;
            this.IsItemSelected = this.ListItem.Selected;
            return this.OptionalRender(g, cellBounds);
        }

        protected Rectangle StandardGetEditRectangle(Graphics g, Rectangle cellBounds)
        {
            Rectangle rectangle = this.CalculateAlignedRectangle(g, cellBounds);
            int num = this.CalculateCheckBoxWidth(g) + this.CalculateImageWidth(g, this.GetImageSelector());
            if ((this.Column.Index == 0) && (this.ListItem.IndentCount > 0))
            {
                int width = this.ListView.SmallImageSize.Width;
                num += width * this.ListItem.IndentCount;
            }
            if (num == 0)
            {
                return cellBounds;
            }
            rectangle.X += num;
            rectangle.Width = Math.Max(rectangle.Width - num, 40);
            return rectangle;
        }

        protected void StandardHitTest(Graphics g, OlvListViewHitTestInfo hti, Rectangle bounds, int x, int y)
        {
            Rectangle rectangle = bounds;
            int num = this.CalculateCheckBoxWidth(g);
            Rectangle rectangle2 = rectangle;
            rectangle2.Width = num;
            if (rectangle2.Contains(x, y))
            {
                hti.HitTestLocation = HitTestLocation.CheckBox;
            }
            else
            {
                rectangle.X += num;
                rectangle.Width -= num;
                num = this.CalculateImageWidth(g, this.GetImageSelector());
                rectangle2 = rectangle;
                rectangle2.Width = num;
                if (rectangle2.Contains(x, y))
                {
                    if ((this.Column.Index > 0) && this.Column.CheckBoxes)
                    {
                        hti.HitTestLocation = HitTestLocation.CheckBox;
                    }
                    else
                    {
                        hti.HitTestLocation = HitTestLocation.Image;
                    }
                }
                else
                {
                    rectangle.X += num;
                    rectangle.Width -= num;
                    num = this.CalculateTextWidth(g, this.GetText());
                    rectangle2 = rectangle;
                    rectangle2.Width = num;
                    if (rectangle2.Contains(x, y))
                    {
                        hti.HitTestLocation = HitTestLocation.Text;
                    }
                    else
                    {
                        hti.HitTestLocation = HitTestLocation.InCell;
                    }
                }
            }
        }

        protected void StandardRender(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            if (this.Column.Index == 0)
            {
                r.X += 3;
                r.Width--;
            }
            this.DrawAlignedImageAndText(g, r);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object Aspect
        {
            get
            {
                if (this.aspect == null)
                {
                    this.aspect = this.column.GetValue(this.rowObject);
                }
                return this.aspect;
            }
            set
            {
                this.aspect = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
            set
            {
                this.bounds = value;
            }
        }

        [DefaultValue(false), Category("Appearance"), Description("Can the renderer wrap text that does not fit completely within the cell")]
        public bool CanWrap
        {
            get
            {
                return this.canWrap;
            }
            set
            {
                this.canWrap = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OLVColumn Column
        {
            get
            {
                return this.column;
            }
            set
            {
                this.column = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DrawListViewItemEventArgs DrawItemEvent
        {
            get
            {
                return this.drawItemEventArgs;
            }
            set
            {
                this.drawItemEventArgs = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public DrawListViewSubItemEventArgs Event
        {
            get
            {
                return this.eventArgs;
            }
            set
            {
                this.eventArgs = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Font Font
        {
            get
            {
                if ((this.font != null) || (this.ListItem == null))
                {
                    return this.font;
                }
                if ((this.SubItem == null) || this.ListItem.UseItemStyleForSubItems)
                {
                    return this.ListItem.Font;
                }
                return this.SubItem.Font;
            }
            set
            {
                this.font = value;
            }
        }

        [Description("The image list from which keyed images will be fetched for drawing."), DefaultValue((string) null), Category("Appearance")]
        public System.Windows.Forms.ImageList ImageList
        {
            get
            {
                return this.imageList;
            }
            set
            {
                this.imageList = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Windows.Forms.ImageList ImageListOrDefault
        {
            get
            {
                return (this.ImageList ?? this.ListView.BaseSmallImageList);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsDrawBackground
        {
            get
            {
                return !this.IsPrinting;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsItemSelected
        {
            get
            {
                return this.isItemSelected;
            }
            set
            {
                this.isItemSelected = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsPrinting
        {
            get
            {
                return this.isPrinting;
            }
            set
            {
                this.isPrinting = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public OLVListItem ListItem
        {
            get
            {
                return this.listItem;
            }
            set
            {
                this.listItem = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ObjectListView ListView
        {
            get
            {
                return this.objectListView;
            }
            set
            {
                this.objectListView = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OLVListSubItem OLVSubItem
        {
            get
            {
                return this.listSubItem;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public object RowObject
        {
            get
            {
                return this.rowObject;
            }
            set
            {
                this.rowObject = value;
            }
        }

        [Category("Appearance"), Description("When rendering multiple images, how many pixels should be between each image?"), DefaultValue(1)]
        public int Spacing
        {
            get
            {
                return this.spacing;
            }
            set
            {
                this.spacing = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OLVListSubItem SubItem
        {
            get
            {
                return this.listSubItem;
            }
            set
            {
                this.listSubItem = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Brush TextBrush
        {
            get
            {
                if (this.textBrush == null)
                {
                    return new SolidBrush(this.GetForegroundColor());
                }
                return this.textBrush;
            }
            set
            {
                this.textBrush = value;
            }
        }

        [Description("Should text be rendered using GDI routines?"), Category("Appearance"), DefaultValue(true)]
        public bool UseGdiTextRendering
        {
            get
            {
                if (this.IsPrinting)
                {
                    return false;
                }
                return this.useGdiTextRendering;
            }
            set
            {
                this.useGdiTextRendering = value;
            }
        }
    }
}

