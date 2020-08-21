using Common;
using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.DisplayTableDefinitionView;
using DbSchemaDecoder.EditTableDefinitionView;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.DB;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DbSchemaDecoder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DbSchemaDecoder : UserControl
    {
        DbSchemaDecoderController _mainController;
        FileListController _fileListController;
        readonly Util.WindowState _windowState = new Util.WindowState();

        public DbSchemaDecoder(Game currentGame, SchemaManager schemaManager)
        {
            DispatcherHelper.Initialize();
            _windowState.CurrentGame = currentGame;
            _windowState.SchemaManager = schemaManager;
            InitializeComponent();
            Loaded += SettingsControl_Loaded;
        }

        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This function crashes winforms editing in visual studio, so return early if that is the case
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            DataGridItemSourceUpdater dbTableItemSourceUpdater = new DataGridItemSourceUpdater();

            _fileListController = new FileListController(_windowState);
            _mainController = new DbSchemaDecoderController(_windowState, dbTableItemSourceUpdater);

            // Hex stuff
            _windowState.OnFileSelected += OnFileSelected;
            _windowState.OnErrorParsingCompleted += (_0, _1) => { UpdateParsingErrors(); } ;
            _windowState.OnSelectedDbSchemaRowChanged +=(_0, _1) => { UpdateSelection(); };

            var parent = GetVisualChild(0);
            ControllerHelper.FindController<FileListView>(parent).DataContext = _fileListController;
            ControllerHelper.FindController<InformationView>(parent).DataContext = _mainController;
            ControllerHelper.FindController<TableDefinitionView>(parent).DataContext = _mainController;
            ControllerHelper.FindController<ConfigureTableRowsView>(parent).DataContext = _mainController;

            var dbParsedEntitiesTableView = ControllerHelper.FindController<DbParsedEntitiesTableView>(parent);
            dbParsedEntitiesTableView.DataContext = _mainController;
            dbTableItemSourceUpdater.Grid = dbParsedEntitiesTableView.DbEntriesViewDataGrid;
        }


        private void OnFileSelected(object sender, DataBaseFile selectedFile)
        {
            if (selectedFile != null)
            {
                HexEdit.Stream = new MemoryStream(selectedFile.DbFile.Data);
                UpdateSelection();
                UpdateParsingErrors();
            }
        }

        void UpdateSelection()
        {
            /*try
            {
                HexEdit.CustomBackgroundBlockItems.Clear();
                using (BinaryReader reader = new BinaryReader(new MemoryStream(_windowState.SelectedFile.DbFile.Data)))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                    HexEdit.CustomBackgroundBlockItems.Add(
                        new WpfHexaEditor.Core.CustomBackgroundBlock(0, header.Length, new SolidColorBrush(Color.FromRgb(254, 0, 0)), "Header"));
     
                    if (_windowState.SelectedDbSchemaRow != null && _windowState.DbSchemaFields != null)
                    {
                        for (int i = 0; i < header.EntryCount; i++)
                        {
                            foreach (var field in _windowState.DbSchemaFields)
                            {
                                var start = reader.BaseStream.Position;
                                var value = field.CreateInstance().TryDecode(reader);
                                var end = reader.BaseStream.Position;
                                if (field.Name == _windowState.SelectedDbSchemaRow.Name)
                                {
                                    HexEdit.CustomBackgroundBlockItems.Add(
                                    new WpfHexaEditor.Core.CustomBackgroundBlock()
                                    {
                                        Color = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                                        StartOffset = start ,
                                        Length = end - start
                                    });
                                }
                            }
                        }
                    }
                }
                HexEdit.RefreshView();
            }
            catch
            { }*/
        }


        void UpdateParsingErrors()
        {
            ErrorTreeView.Items.Clear();
            if (_windowState.SelectedFile == null || _windowState.FileParsingErrors == null)
                return;
            var errorItem = _windowState.FileParsingErrors.FirstOrDefault(x => x.TableType == _windowState.SelectedFile.TableType);
            if (errorItem == null)
                return;
            foreach (var item in errorItem.Errors)
                ErrorTreeView.Items.Add(item);
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

        private void OnCreateColumDefinitionsFromDepricatedData(object sender, RoutedEventArgs e)
        {
            var allFiles = _fileListController.ViewModel.FileList;
            var currentGame = _windowState.CurrentGame;
            var schemas = _windowState.SchemaManager.GetDepricated();
            if (schemas == null)
            {
                MessageBox.Show("Unable to get old table definitions");
                return;
            }

            if (currentGame == null)
            {
                MessageBox.Show("A game is not selected");
                return;
            }

            Dictionary<string, List<DbTableDefinition>> output = new Dictionary<string, List<DbTableDefinition>>();
            foreach (var file in allFiles)
            {
                if (schemas.ContainsKey(file.DataBaseFile.TableType))
                {
                    output.Add(file.DataBaseFile.TableType, new List<DbTableDefinition>());
                    foreach (var schema in schemas[file.DataBaseFile.TableType])
                    {
                        TableEntriesParser parser = new TableEntriesParser(file.DataBaseFile.DbFile.Data, 0);
                        var result = parser.CanParseTable(schema.ColumnDefinitions, 30);
                        if (result.HasError == false)
                        {
                            output[file.DataBaseFile.TableType].Add(schema);
                        }
                    }
                }
            }

            _fileListController.StartEvaluation();
        }
    }




    


}
