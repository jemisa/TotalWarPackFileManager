namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Design;
    using System.Windows.Forms;

    public class DataListView : ObjectListView
    {
        private CurrencyManager currencyManager = null;
        private string dataMember = "";
        private object dataSource;
        private bool isChangingIndex = false;

        public override void AddObjects(ICollection modelObjects)
        {
        }

        protected virtual void CreateColumnsFromSource()
        {
            if ((this.currencyManager != null) && (base.Columns.Count == 0))
            {
                PropertyDescriptorCollection itemProperties = this.currencyManager.GetItemProperties();
                if (itemProperties.Count != 0)
                {
                    bool flag = false;
                    for (int i = 0; i < itemProperties.Count; i++)
                    {
                        PropertyDescriptor descriptor = itemProperties[i];
                        if (descriptor.PropertyType != typeof(IBindingList))
                        {
                            OLVColumn column = new OLVColumn(descriptor.DisplayName, descriptor.Name);
                            if ((descriptor.PropertyType == typeof(bool)) || (descriptor.PropertyType == typeof(CheckState)))
                            {
                                flag = true;
                                column.TextAlign = HorizontalAlignment.Center;
                                column.Width = 0x20;
                                column.AspectName = descriptor.Name;
                                column.CheckBoxes = true;
                                if (descriptor.PropertyType == typeof(CheckState))
                                {
                                    column.TriStateCheckBoxes = true;
                                }
                            }
                            else
                            {
                                column.Width = 0;
                                if (descriptor.PropertyType == typeof(byte[]))
                                {
                                    column.Renderer = new ImageRenderer();
                                }
                            }
                            column.IsEditable = !descriptor.IsReadOnly;
                            base.Columns.Add(column);
                        }
                    }
                    if (flag)
                    {
                        this.SetupSubItemCheckBoxes();
                    }
                }
            }
        }

        protected virtual void CreateMissingAspectGettersAndPutters()
        {
            for (int i = 0; i < base.Columns.Count; i++)
            {
                AspectGetterDelegate delegate2 = null;
                AspectPutterDelegate delegate3 = null;
                OLVColumn column = this.GetColumn(i);
                if ((column.AspectGetter == null) && !string.IsNullOrEmpty(column.AspectName))
                {
                    if (delegate2 == null)
                    {
                        delegate2 = delegate (object row) {
                            DataRowView view = row as DataRowView;
                            if (view != null)
                            {
                                return view[column.AspectName];
                            }
                            return column.GetAspectByName(row);
                        };
                    }
                    column.AspectGetter = delegate2;
                }
                if ((column.IsEditable && (column.AspectPutter == null)) && !string.IsNullOrEmpty(column.AspectName))
                {
                    if (delegate3 == null)
                    {
                        delegate3 = delegate (object row, object newValue) {
                            DataRowView view = row as DataRowView;
                            if (view != null)
                            {
                                view[column.AspectName] = newValue;
                            }
                            else
                            {
                                column.PutAspectByName(row, newValue);
                            }
                        };
                    }
                    column.AspectPutter = delegate3;
                }
            }
        }

        protected virtual void currencyManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.Reset:
                    this.InitializeDataSource();
                    break;

                case ListChangedType.ItemAdded:
                {
                    object obj3 = this.currencyManager.List[e.NewIndex];
                    DataRowView view = obj3 as DataRowView;
                    if (!((view != null) && view.IsNew))
                    {
                        this.InitializeDataSource();
                    }
                    break;
                }
                case ListChangedType.ItemDeleted:
                    this.InitializeDataSource();
                    break;

                case ListChangedType.ItemMoved:
                    this.InitializeDataSource();
                    break;

                case ListChangedType.ItemChanged:
                {
                    object modelObject = this.currencyManager.List[e.NewIndex];
                    this.RefreshObject(modelObject);
                    break;
                }
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorDeleted:
                case ListChangedType.PropertyDescriptorChanged:
                    this.InitializeDataSource();
                    break;
            }
        }

        protected virtual void currencyManager_MetaDataChanged(object sender, EventArgs e)
        {
            this.InitializeDataSource();
        }

        protected virtual void currencyManager_PositionChanged(object sender, EventArgs e)
        {
            int position = this.currencyManager.Position;
            if (((position >= 0) && (position < base.Items.Count)) && !this.isChangingIndex)
            {
                try
                {
                    this.isChangingIndex = true;
                    this.SelectedObject = this.currencyManager.List[position];
                    if (base.SelectedItems.Count > 0)
                    {
                        base.SelectedItems[0].EnsureVisible();
                    }
                }
                finally
                {
                    this.isChangingIndex = false;
                }
            }
        }

        protected override void DoUnfreeze()
        {
            this.RebindDataSource(true);
        }

        protected virtual void InitializeDataSource()
        {
            if (!this.Frozen && (this.currencyManager != null))
            {
                this.CreateColumnsFromSource();
                this.CreateMissingAspectGettersAndPutters();
                this.SetObjects(this.currencyManager.List);
                if (base.Items.Count > 0)
                {
                    foreach (ColumnHeader header in base.Columns)
                    {
                        if (header.Width == 0)
                        {
                            base.AutoResizeColumn(header.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                        }
                    }
                }
            }
        }

        protected override void OnBindingContextChanged(EventArgs e)
        {
            base.OnBindingContextChanged(e);
            this.RebindDataSource(false);
        }

        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            base.OnParentBindingContextChanged(e);
            this.RebindDataSource(false);
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (!this.isChangingIndex && ((base.SelectedIndices.Count == 1) && (this.currencyManager != null)))
            {
                try
                {
                    this.isChangingIndex = true;
                    this.currencyManager.Position = this.currencyManager.List.IndexOf(this.SelectedObject);
                }
                finally
                {
                    this.isChangingIndex = false;
                }
            }
        }

        protected virtual void RebindDataSource()
        {
            this.RebindDataSource(false);
        }

        protected virtual void RebindDataSource(bool forceDataInitialization)
        {
            if (this.BindingContext != null)
            {
                CurrencyManager manager = null;
                if (this.DataSource != null)
                {
                    manager = (CurrencyManager) this.BindingContext[this.DataSource, this.DataMember];
                }
                if (this.currencyManager != manager)
                {
                    if (this.currencyManager != null)
                    {
                        this.currencyManager.MetaDataChanged -= new EventHandler(this.currencyManager_MetaDataChanged);
                        this.currencyManager.PositionChanged -= new EventHandler(this.currencyManager_PositionChanged);
                        this.currencyManager.ListChanged -= new ListChangedEventHandler(this.currencyManager_ListChanged);
                    }
                    this.currencyManager = manager;
                    if (this.currencyManager != null)
                    {
                        this.currencyManager.MetaDataChanged += new EventHandler(this.currencyManager_MetaDataChanged);
                        this.currencyManager.PositionChanged += new EventHandler(this.currencyManager_PositionChanged);
                        this.currencyManager.ListChanged += new ListChangedEventHandler(this.currencyManager_ListChanged);
                    }
                    forceDataInitialization = true;
                }
                if (forceDataInitialization)
                {
                    this.InitializeDataSource();
                }
            }
        }

        public override void RemoveObjects(ICollection modelObjects)
        {
        }

        [Category("Data"), Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor)), DefaultValue("")]
        public virtual string DataMember
        {
            get
            {
                return this.dataMember;
            }
            set
            {
                if (this.dataMember != value)
                {
                    this.dataMember = value;
                    this.RebindDataSource();
                }
            }
        }

        [TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design"), Category("Data")]
        public virtual object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                this.dataSource = value;
                this.RebindDataSource(true);
            }
        }
    }
}

