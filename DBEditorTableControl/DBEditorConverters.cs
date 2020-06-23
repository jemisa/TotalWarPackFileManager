using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace DBTableControl
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BoolConverter : MarkupExtension, IValueConverter
    {
        private static BoolConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new BoolConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return false;
            }
            else if (!(bool)value)
            {
                return true;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return true;
            }
            else if (!(bool)value)
            {
                return false;
            }

            return DependencyProperty.UnsetValue;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityConverter : MarkupExtension, IValueConverter
    {
        private static VisibilityConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new VisibilityConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Hidden)
            {
                return true;
            }
            else if ((Visibility)value == Visibility.Visible)
            {
                return false;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Hidden;
            }
            else if (!(bool)value)
            {
                return Visibility.Visible;
            }

            return DependencyProperty.UnsetValue;
        }
    }

    public class BackgroundConverter : IMultiValueConverter
    {
        #region Implementation of BGConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush newColor = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
            //NewColor = null;
            DataGridCell cell = null;
            DataRow row = null;
            string columnName = null;
            DataColumn column = null;
            Type cellType = null;

            if (values[1] is DataRow)
            {
                cell = (DataGridCell)values[0];
                cell.ToolTip = null;
                cell.BorderBrush = null;
                cell.BorderThickness = new Thickness(0);
                row = (DataRow)values[1];

                // Skip deleted rows, since they disappear.
                if (row.RowState == DataRowState.Deleted || row.RowState == DataRowState.Detached)
                {
                    return newColor;
                }

                columnName = (string)cell.Column.Header;
                column = row.Table.Columns[columnName];
                cellType = row[columnName].GetType();
            }
            else
            {
                //throw new ArgumentException("BackgroundConverter not given DataRow");
                return newColor;
            }

            if (column.ReadOnly)
            {
                newColor = new SolidColorBrush(Color.FromArgb(255, 210, 210, 210));
            }

            if (IsKeyColumn(row.Table, columnName))
            {
                newColor = new SolidColorBrush(Colors.LightGoldenrodYellow);
            }

            if (row.RowState == DataRowState.Modified && !row[column, DataRowVersion.Current].Equals(row[column, DataRowVersion.Original]))
            {
                newColor = new SolidColorBrush(Colors.LightPink);
                cell.ToolTip = String.Format("Original Value: {0}", row[column, DataRowVersion.Original]);
            }

            if (column.ExtendedProperties.ContainsKey("Hidden") && (bool)column.ExtendedProperties["Hidden"])
            {
                newColor = new SolidColorBrush(Colors.LightSteelBlue);
            }

            if (row.RowState == DataRowState.Added)
            {
                newColor = new SolidColorBrush(Colors.LightGreen);
            }

            if (row.HasErrors)
            {
                if (row.GetColumnsInError().Contains(column))
                {
                    newColor = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
                    cell.BorderBrush = new SolidColorBrush(Colors.Red);
                    cell.BorderThickness = new Thickness(5);
                    cell.ToolTip = String.Format("{0}\nOriginal Value: {1}", row.GetColumnError(column), row.RowState == DataRowState.Added ? "None, new row." : row[column, DataRowVersion.Original]);
                }
            }

            return newColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        private bool IsKeyColumn(DataTable table, string columnName)
        {
            foreach (DataColumn column in table.PrimaryKey)
            {
                if (column.ColumnName.Equals(columnName))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class ImageSourceConverter : MarkupExtension, IValueConverter
    {
        private static ImageSourceConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new ImageSourceConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().Contains("Error:"))
            {
                return "Resources\\dberror.ico";
            }
            else if (value.ToString().Contains("Warning:"))
            {
                return "Resources\\dbwarning.ico";
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    [ValueConversion(typeof(int), typeof(int))]
    public class RowIndexConverter : MarkupExtension, IValueConverter
    {
        private static RowIndexConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new RowIndexConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
