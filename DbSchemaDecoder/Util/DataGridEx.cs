using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DbSchemaDecoder.Util
{
    public class DataGridEx : DataGrid
    {
        public DataGridEx()
        {
            BoundCellLevel = true;
        }

        public bool BoundCellLevel
        {
            get { return (bool)GetValue(BoundCellLevelProperty); }
            set { SetValue(BoundCellLevelProperty, value); }
        }

        public static readonly DependencyProperty BoundCellLevelProperty =
            DependencyProperty.Register("BoundCellLevel", typeof(bool), typeof(DataGridEx), new UIPropertyMetadata(false));

        protected override Size MeasureOverride(Size availableSize)
        {
            var desiredSize = base.MeasureOverride(availableSize);
            if (BoundCellLevel)
                ClearBindingGroup();
            return desiredSize;
        }

        private void ClearBindingGroup()
        {
            // Clear ItemBindingGroup so it isn't applied to new rows
            ItemBindingGroup = null;
            // Clear BindingGroup on already created rows
            foreach (var item in Items)
            {
                var row = ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if(row != null)
                    row.BindingGroup = null;
            }
        }
    }
}
