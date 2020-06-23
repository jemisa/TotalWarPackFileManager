namespace DataGridViewAutoFilter
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public class DataGridViewAutoFilterTextBoxColumn : DataGridViewTextBoxColumn
    {
        public DataGridViewAutoFilterTextBoxColumn()
        {
            base.DefaultHeaderCellType = typeof(DataGridViewAutoFilterColumnHeaderCell);
            base.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        public static string GetFilterStatus(DataGridView dataGridView)
        {
            return DataGridViewAutoFilterColumnHeaderCell.GetFilterStatus(dataGridView);
        }

        public static void RemoveFilter(DataGridView dataGridView)
        {
            DataGridViewAutoFilterColumnHeaderCell.RemoveFilter(dataGridView);
        }

        [DefaultValue(true)]
        public bool AutomaticSortingEnabled
        {
            get
            {
                return ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).AutomaticSortingEnabled;
            }
            set
            {
                ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).AutomaticSortingEnabled = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public new System.Type DefaultHeaderCellType
        {
            get
            {
                return typeof(DataGridViewAutoFilterColumnHeaderCell);
            }
        }

        [DefaultValue(20)]
        public int DropDownListBoxMaxLines
        {
            get
            {
                return ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).DropDownListBoxMaxLines;
            }
            set
            {
                ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).DropDownListBoxMaxLines = value;
            }
        }

        [DefaultValue(true)]
        public bool FilteringEnabled
        {
            get
            {
                return ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).FilteringEnabled;
            }
            set
            {
                ((DataGridViewAutoFilterColumnHeaderCell) base.HeaderCell).FilteringEnabled = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), DefaultValue(2), Browsable(false)]
        public new DataGridViewColumnSortMode SortMode
        {
            get
            {
                return base.SortMode;
            }
            set
            {
                if (value == DataGridViewColumnSortMode.Automatic)
                {
                    throw new InvalidOperationException("A SortMode value of Automatic is incompatible with the DataGridViewAutoFilterColumnHeaderCell type. Use the AutomaticSortingEnabled property instead.");
                }
                base.SortMode = value;
            }
        }
    }
}

