using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.DisplayTableDefinitionView;
using DbSchemaDecoder.EditTableDefinitionView;
using DbSchemaDecoder.Util;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
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

            DataGridItemSourceUpdater dbTableItemSourceUpdater = new DataGridItemSourceUpdater();

            _fileListController = new FileListController();
            _mainController = new DbSchemaDecoderController(_fileListController, dbTableItemSourceUpdater);

            // Hex stuff
            _fileListController.OnFileSelectedEvent += _fileListController_MyEvent;

            var parent = GetVisualChild(0);
            ControllerHelper.FindController<FileListView>(parent).DataContext = _fileListController;
            //FindController<ConfigureTableRowsView>().DataContext = _configureTableRowsController;
            ControllerHelper.FindController<InformationView>(parent).DataContext = _mainController;
            ControllerHelper.FindController<ConfigureTableRowsView>(parent).DataContext = _mainController;

            var tableView = ControllerHelper.FindController<DisplayTableDefinitionView2>(parent);
            dbTableItemSourceUpdater.Grid = tableView.DbEntriesViewDataGrid;

            tableView.DataContext = _mainController;
            //FindController<DbTableView>().DataContext = _mainController;
        }

     


        //----Hex stuff
        private void _fileListController_MyEvent(object sender, DataBaseFile e)
        {
            if(e != null)
                HexEdit.Stream = new MemoryStream(e.DbFile.Data);
            // HexEdit.hex
         //   _temp.Add();
        }
        //---



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
