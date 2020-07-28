using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.DisplayTableDefinitionView;
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

        DbSchemaDecoderController _mainController;

        FileListController _fileListController;

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
            _mainController = new DbSchemaDecoderController(_fileListController);

            // Hex stuff
            _fileListController.OnFileSelectedEvent += _fileListController_MyEvent;

            FindController<FileListView>().DataContext = _fileListController;
            //FindController<ConfigureTableRowsView>().DataContext = _configureTableRowsController;
            FindController<InformationView>().DataContext = _mainController;
            FindController<ConfigureTableRowsView>().DataContext = _mainController;
            FindController<DisplayTableDefinitionView2>().DataContext = _mainController;
        }

     


        //----Hex stuff
        private void _fileListController_MyEvent(object sender, DataBaseFile e)
        {
            HexEdit.Stream = new MemoryStream(e.DbFile.Data);
           // HexEdit.hex
        }
        //---

        T FindController<T>() where T : DependencyObject
        {
            // Helper function to work around the issue that we can not set names in the xaml file becouse of ?

            var allControllers = FindVisualChildren<T>(GetVisualChild(0));
            return allControllers.First();
        }

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
                FileListColumn.Width = new GridLength(350);
            else
                FileListColumn.Width = new GridLength(0);
        }

        private void OnShowHexClick(object sender, RoutedEventArgs e)
        {
            if (HexViewColumn.Width.Value == 0)
                HexViewColumn.Width = new GridLength(600);
            else
                HexViewColumn.Width = new GridLength(0);
        }
    }
}
