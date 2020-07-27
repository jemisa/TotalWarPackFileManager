using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.EditTableDefinitionView;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace DbSchemaDecoder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DbSchemaDecoder : UserControl
    {
        FileListController _fileListController;
        ConfigureTableRowsController _configureTableRowsController;
        MemoryStream _currentStream;

        public DbSchemaDecoder()
        {
            InitializeComponent();
            Loaded += SettingsControl_Loaded;
        }

        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This function crashes winforms editing in visual studio, so return early if that is the case
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _fileListController = new FileListController();
            _configureTableRowsController = new ConfigureTableRowsController(_fileListController);

            var allListViews = FindVisualChildren<FileListView>(GetVisualChild(0));
            var listView = allListViews.First();

            listView.DataContext = _fileListController;

            // Hex stuff
            _fileListController.MyEvent += _fileListController_MyEvent;

            var allConfigureTableRowsView = FindVisualChildren<ConfigureTableRowsView>(GetVisualChild(0));
            var configureView = allConfigureTableRowsView.First();
            configureView.DataContext = _configureTableRowsController;
        }




        //----Hex stuff
        private void _fileListController_MyEvent(object sender, TestItem e)
        {
            HexEdit.Stream = new MemoryStream(e.DbFile.Data);
        }
        //---

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void OnShowFileListClick(object sender, RoutedEventArgs e)
        {
            if(FileListColumn.Width.Value == 0)
                FileListColumn.Width = new GridLength(200);
            else
                FileListColumn.Width = new GridLength(0);
        }
    }
}
