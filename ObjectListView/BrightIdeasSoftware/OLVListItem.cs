namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;

    public class OLVListItem : ListViewItem
    {
        private System.Windows.Forms.CheckState checkState;
        private IList<IDecoration> decorations;
        private object imageSelector;
        private object rowObject;

        public OLVListItem(object rowObject)
        {
            this.rowObject = rowObject;
        }

        public OLVListItem(object rowObject, string text, object image) : base(text, -1)
        {
            this.rowObject = rowObject;
            this.imageSelector = image;
        }

        public virtual OLVListSubItem GetSubItem(int index)
        {
            if ((index >= 0) && (index < base.SubItems.Count))
            {
                return (OLVListSubItem) base.SubItems[index];
            }
            return null;
        }

        public virtual Rectangle GetSubItemBounds(int subItemIndex)
        {
            if (subItemIndex == 0)
            {
                Rectangle bounds = this.Bounds;
                Point scrolledColumnSides = BrightIdeasSoftware.NativeMethods.GetScrolledColumnSides(base.ListView, subItemIndex);
                bounds.X = scrolledColumnSides.X + 1;
                bounds.Width = scrolledColumnSides.Y - scrolledColumnSides.X;
                return bounds;
            }
            OLVListSubItem subItem = this.GetSubItem(subItemIndex);
            if (subItem == null)
            {
                return new Rectangle();
            }
            return subItem.Bounds;
        }

        public Rectangle Bounds
        {
            get
            {
                try
                {
                    return base.Bounds;
                }
                catch (ArgumentException)
                {
                    return Rectangle.Empty;
                }
            }
        }

        public System.Windows.Forms.CheckState CheckState
        {
            get
            {
                switch (base.StateImageIndex)
                {
                    case 0:
                        return System.Windows.Forms.CheckState.Unchecked;

                    case 1:
                        return System.Windows.Forms.CheckState.Checked;

                    case 2:
                        return System.Windows.Forms.CheckState.Indeterminate;
                }
                return System.Windows.Forms.CheckState.Unchecked;
            }
            set
            {
                if (this.checkState != value)
                {
                    this.checkState = value;
                    switch (value)
                    {
                        case System.Windows.Forms.CheckState.Unchecked:
                            base.StateImageIndex = 0;
                            break;

                        case System.Windows.Forms.CheckState.Checked:
                            base.StateImageIndex = 1;
                            break;

                        case System.Windows.Forms.CheckState.Indeterminate:
                            base.StateImageIndex = 2;
                            break;
                    }
                }
            }
        }

        public IDecoration Decoration
        {
            get
            {
                if (this.HasDecoration)
                {
                    return this.Decorations[0];
                }
                return null;
            }
            set
            {
                this.Decorations.Clear();
                if (value != null)
                {
                    this.Decorations.Add(value);
                }
            }
        }

        public IList<IDecoration> Decorations
        {
            get
            {
                if (this.decorations == null)
                {
                    this.decorations = new List<IDecoration>();
                }
                return this.decorations;
            }
        }

        public bool HasDecoration
        {
            get
            {
                return ((this.decorations != null) && (this.decorations.Count > 0));
            }
        }

        public object ImageSelector
        {
            get
            {
                return this.imageSelector;
            }
            set
            {
                this.imageSelector = value;
                if (value is int)
                {
                    base.ImageIndex = (int) value;
                }
                else if (value is string)
                {
                    base.ImageKey = (string) value;
                }
                else
                {
                    base.ImageIndex = -1;
                }
            }
        }

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
    }
}

