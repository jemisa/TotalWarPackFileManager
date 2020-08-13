using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.DisplayTableDefinitionView;
using DbSchemaDecoder.EditTableDefinitionView;
using DbSchemaDecoder.Util;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
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
            DispatcherHelper.Initialize();

            InitializeComponent();
            Loaded += SettingsControl_Loaded;
        }

        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This function crashes winforms editing in visual studio, so return early if that is the case
            if (DesignerProperties.GetIsInDesignMode(this))
                return;


            EventHub eventHub = new EventHub();

            DataGridItemSourceUpdater dbTableItemSourceUpdater = new DataGridItemSourceUpdater();

            _fileListController = new FileListController(eventHub);
            _mainController = new DbSchemaDecoderController(eventHub, dbTableItemSourceUpdater);

            // Hex stuff
            eventHub.OnFileSelected += OnFileSelected;

            var parent = GetVisualChild(0);
            ControllerHelper.FindController<FileListView>(parent).DataContext = _fileListController;
            ControllerHelper.FindController<InformationView>(parent).DataContext = _mainController;
            ControllerHelper.FindController<TableDefinitionView>(parent).DataContext = _mainController;
            ControllerHelper.FindController<ConfigureTableRowsView>(parent).DataContext = _mainController;

            var dbParsedEntitiesTableView = ControllerHelper.FindController<DbParsedEntitiesTableView>(parent);
            dbParsedEntitiesTableView.DataContext = _mainController;
            dbTableItemSourceUpdater.Grid = dbParsedEntitiesTableView.DbEntriesViewDataGrid;
        }

        private void OnFileSelected(object sender, DataBaseFile e)
        {
            if (e != null)
            {
                HexEdit.Stream = new MemoryStream(e.DbFile.Data);
                HexEdit.CustomBackgroundBlockItems.Add(
                    new WpfHexaEditor.Core.CustomBackgroundBlock() 
                    {
                        Color = new SolidColorBrush(Color.FromRgb(244,0,0)),
                        StartOffset = 10, 
                        Length = 50
                    });

                HexEdit.CustomBackgroundBlockItems.Add(
                    new WpfHexaEditor.Core.CustomBackgroundBlock()
                    {
                        Color = new SolidColorBrush(Color.FromRgb(0, 244, 0)),
                        StartOffset = 80,
                        Length = 200
                    });
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

        private void OnInformationViewButtonClicked(object sender, RoutedEventArgs e)
        {
            ToggleShowHideAuto(InformationRow);
        }

        private void OnDbParsedEntitiesTablesButtonClicked(object sender, RoutedEventArgs e)
        {
            ToggleShowHide(DbParsedEntitiesRow);
        }

        private void OnTableDefinitionViewButtonClicked(object sender, RoutedEventArgs e)
        {
            ToggleShowHide(TableDefinitionRow);
        }

        private void OnTableAnalyticsViewButtonClicked(object sender, RoutedEventArgs e)
        {
            ToggleShowHide(TableAnalyticsRow);
        }

        void ToggleShowHide(RowDefinition rowDef)
        {
            if (rowDef.Height.Value == 0 && rowDef.Height.GridUnitType == GridUnitType.Pixel)
                rowDef.Height = new GridLength(1, GridUnitType.Star);
            else
                rowDef.Height = new GridLength(0, GridUnitType.Pixel);
        }

        void ToggleShowHideAuto(RowDefinition rowDef)
        {
            if (rowDef.Height.Value == 0 && rowDef.Height.GridUnitType == GridUnitType.Pixel)
                rowDef.Height = new GridLength(1, GridUnitType.Auto);
            else
                rowDef.Height = new GridLength(0, GridUnitType.Pixel);
        }


    }




    


}
