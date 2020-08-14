using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DbSchemaDecoder.DisplayTableDefinitionView
{
    /// <summary>
    /// Interaction logic for TableDefinitionView.xaml
    /// </summary>
    public partial class TableDefinitionView : UserControl
    {
        public TableDefinitionView()
        {
            InitializeComponent();
        }

        public void Menu_Click(object sender, RoutedEventArgs args)
        {
            DbSchemaDecoderController controller = (this.DataContext as DbSchemaDecoderController);
            var menuItem = args.Source as MenuItem;
            controller.DbTableDefinitionController.DbMetaDataAppliedCommand.Execute(menuItem.DataContext as CaSchemaEntry);
        }
    }

    public class IndexConverter : IValueConverter
    {
        public object Convert(object value, Type TargetType, object parameter, CultureInfo culture)
        {
            var item = (ListViewItem)value;
            var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index = listView.ItemContainerGenerator.IndexFromContainer(item) + 1;
            return index.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
