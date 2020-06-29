using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Common;
using CommonDialogs;
using CommonUtilities;
using DBEditorTableControl;
using DBEditorTableControl.Dialogs;
using Filetypes;
using Filetypes.Codecs;

namespace DBTableControl
{
    /// <summary>
    /// Interaction logic for DBEditorTableControl.xaml
    /// </summary>
    public partial class DBEditorTableControl : UserControl, INotifyPropertyChanged, IPackedFileEditor
    {
        
        DataSet _loadedDataSet;

        // Data Source Properties
        DataTable currentTable;
        public DataTable CurrentTable
        {
            get { return currentTable; }
            set
            {
                dbDataGrid.ItemsSource = null;

                currentTable = value;

                // Reset event handlers
                CurrentTable.ColumnChanged -= new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
                CurrentTable.ColumnChanged += new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
                CurrentTable.RowDeleting -= new DataRowChangeEventHandler(CurrentTable_RowDeleting);
                CurrentTable.RowDeleting += new DataRowChangeEventHandler(CurrentTable_RowDeleting);
                CurrentTable.RowDeleted -= new DataRowChangeEventHandler(CurrentTable_RowDeleted);
                CurrentTable.RowDeleted += new DataRowChangeEventHandler(CurrentTable_RowDeleted);
                CurrentTable.TableNewRow -= new DataTableNewRowEventHandler(CurrentTable_TableNewRow);
                CurrentTable.TableNewRow += new DataTableNewRowEventHandler(CurrentTable_TableNewRow);

                // Generate a new table format for the new table.
                GenerateColumns();

                // Re-enable export control if it was disabled
                exportAsButton.IsEnabled = true;

                // Clear and regenerate the observable error list for the new table.
                errorList.Clear();
                UpdateErrorView();

                // Filters
                var filterController = filterDockPanel.Children[0] as FilterController;
                filterController.LoadFilters(this);

                // Make sure the control knows it's table has changed.
                NotifyPropertyChanged(this, "CurrentTable");

                dbDataGrid.ItemsSource = CurrentTable.DefaultView;
            }
        }

        bool moveAndFreezeKeys;
        public bool MoveAndFreezeKeys
        {
            get { return moveAndFreezeKeys; }
            set
            {
                moveAndFreezeKeys = value;
                NotifyPropertyChanged(this, "MoveAndFreezeKeys");
                UpdateConfig();

                if (editedFile != null)
                {
                    if (moveAndFreezeKeys)
                    {
                        FreezeKeys();
                    }
                    else
                    {
                        UnfreezeKeys();
                    }
                }
            }
        }

        bool useComboBoxes;
        public bool UseComboBoxes
        {
            get { return useComboBoxes; }
            set 
            { 
                useComboBoxes = value; 
                NotifyPropertyChanged(this, "UseComboBoxes");
                UpdateConfig();

                if (EditedFile != null)
                {
                    // Generate new columns
                    GenerateColumns(false);
                }
            }
        }

        bool showAllColumns;
        public bool ShowAllColumns
        {
            get { return showAllColumns; }
            set
            {
                showAllColumns = value; 
                NotifyPropertyChanged(this, "ShowAllColumns");
                UpdateConfig();

                // Set all columns to visible, but do not reset currentTable's extended properties.
                foreach (DataGridColumn col in dbDataGrid.Columns)
                {
                    if (currentTable.Columns[(string)col.Header].ExtendedProperties.ContainsKey("Hidden"))
                    {
                        bool ishidden = (bool)currentTable.Columns[(string)col.Header].ExtendedProperties["Hidden"];

                        if (ishidden && !showAllColumns)
                        {
                            col.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            col.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        bool readOnly;
        public bool ReadOnly
        {
            get { return readOnly; }
            set 
            { 
                readOnly = false; 
                NotifyPropertyChanged(this, "TableReadOnly");

                // Set whether we can add rows via button based on readonly.
                addRowButton.IsEnabled = !readOnly;
                importFromButton.IsEnabled = !readOnly;
                dbDataGrid.CanUserAddRows = !readOnly;
                dbDataGrid.CanUserDeleteRows = !readOnly;

                if (findButton.IsEnabled)
                {
                    replaceButton.IsEnabled = !readOnly;
                }

                BuiltTablesSetReadOnly(readOnly);
            }
        }

        bool showFilters;
        public bool ShowFilters 
        { 
            get { return showFilters; } 
            set 
            { 
                showFilters = value;
                NotifyPropertyChanged(this, "ShowFilters");
                UpdateConfig();

                if (showFilters)
                {
                    filterDockPanel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    filterDockPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
            } 
        }

        // PFM needed Properties
        PackedFile currentPackedFile;
        public PackedFile CurrentPackedFile
        {
            get { return currentPackedFile; }
            set
            {
                if (currentPackedFile != null && DataChanged)
                {
                    Commit();
                }

                if (editedFile != null)
                {
                    // Save off the editor configuration.
                    UpdateConfig();
                }
                
                dataChanged = false;
                currentPackedFile = value;

                if (currentPackedFile != null)
                {
                    try
                    {
                        // Reset and re-register for packedfile events.
                        CurrentPackedFile.RenameEvent -= new PackEntry.Renamed(CurrentPackedFile_RenameEvent);
                        CurrentPackedFile.RenameEvent += new PackEntry.Renamed(CurrentPackedFile_RenameEvent);

                        codec = PackedFileDbCodec.FromFilename(currentPackedFile.FullPath);
                        editedFile = PackedFileDbCodec.Decode(currentPackedFile);
                    }
                    catch (DBFileNotSupportedException exception)
                    {
                        showDBFileNotSupportedMessage(exception.Message);
                    }
                }

                // Create and set CurrentTable
                CurrentTable = CreateTable(editedFile);

                NotifyPropertyChanged(this, "CurrentPackedFile");

                // cannot edit contained complex types
                foreach(FieldInfo f in EditedFile.CurrentType.Fields) {
                    if (f is ListType) {
                        Console.WriteLine("cannot edit this");
                        ReadOnly = true;
                        break;
                    }
                }
            }
        }

        PackedFileDbCodec codec;

        DBFile editedFile;
        public DBFile EditedFile { get { return editedFile; } }

        bool dataChanged;
        public bool DataChanged { get { return dataChanged; } }

        string importDirectory;
        public string ImportDirectory 
        { 
            get { return importDirectory; } 
            set 
            { 
                importDirectory = value; 
                UpdateConfig(); 
            } 
        }

        string exportDirectory;
        public string ExportDirectory
        {
            get { return exportDirectory; }
            set
            {
                exportDirectory = value;
                UpdateConfig();
            }
        }

        // Configuration data
        public DBTableEditorConfig savedconfig;

        private List<string> hiddenColumns;
        public List<Visibility> visibleRows;
       
        public List<DBFilter> autofilterList;
		private ObservableCollection<DBError> errorList;

        FindAndReplaceWindow findReplaceWindow;

        public DBEditorTableControl()
        {
            InitializeComponent();

            // Attempt to load configuration settings, loading default values if config file doesn't exist.
            savedconfig = new DBTableEditorConfig();
            savedconfig.Load();

            // Instantiate default datatable, and others.
            currentTable = new DataTable();
            _loadedDataSet = new DataSet("Loaded Tables");
            _loadedDataSet.EnforceConstraints = false;
            hiddenColumns = new List<string>();
            visibleRows = new List<System.Windows.Visibility>();
            autofilterList = new List<DBFilter>();
			errorList = new ObservableCollection<DBError>();
            dberrorsListView.ItemsSource = errorList;

            // Transfer saved settings.
            moveAndFreezeKeys = savedconfig.FreezeKeyColumns;
            useComboBoxes = savedconfig.UseComboBoxes;
            showAllColumns = savedconfig.ShowAllColumns;
            importDirectory = savedconfig.ImportDirectory;
            exportDirectory = savedconfig.ExportDirectory;
            ShowFilters = savedconfig.ShowFilters;

            // Set Initial checked status.
            moveAndFreezeKeysCheckBox.IsChecked = moveAndFreezeKeys;
            useComboBoxesCheckBox.IsChecked = useComboBoxes;
            showAllColumnsCheckBox.IsChecked = showAllColumns;

            // Register for Datatable events
            CurrentTable.ColumnChanged += new DataColumnChangeEventHandler(CurrentTable_ColumnChanged);
            CurrentTable.RowDeleting += new DataRowChangeEventHandler(CurrentTable_RowDeleting);
            CurrentTable.RowDeleted += new DataRowChangeEventHandler(CurrentTable_RowDeleted);
            CurrentTable.TableNewRow += new DataTableNewRowEventHandler(CurrentTable_TableNewRow);

            // Register for FindAndReplaceWindowEvents
            findReplaceWindow = new FindAndReplaceWindow();
            findReplaceWindow.FindNext += new EventHandler(findWindow_FindNext);
            findReplaceWindow.FindAll += new EventHandler(findReplaceWindow_FindAll);
            findReplaceWindow.Replace += new EventHandler(replaceWindow_Replace);
            findReplaceWindow.ReplaceAll += new EventHandler(replaceWindow_ReplaceAll);

            // Enable keyboard interop for the findReplaceWindow, otherwise WinForms will intercept all keyboard input.
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(findReplaceWindow);

            // Default the below buttons to false for all tables.
            cloneRowButton.IsEnabled = false;
            insertRowButton.IsEnabled = false;
            findButton.IsEnabled = false;
            replaceButton.IsEnabled = false; ;

            // Route the Paste event here so we can do it ourselves.
            CommandManager.RegisterClassCommandBinding(typeof(DataGrid), 
                new CommandBinding(ApplicationCommands.Paste, 
                    new ExecutedRoutedEventHandler(OnExecutedPaste), 
                    new CanExecuteRoutedEventHandler(OnCanExecutePaste)));

            
        }

        #region IPackedFileEditor Implementation
        public bool CanEdit(PackedFile file)
        {
            return PackedFileDbCodec.CanDecode(file, out _);
        }

        public void Commit()
        {
            // Ignore Commit call if there is nothing to commit, or if the user simply wandered to another table.
            if (EditedFile == null || (!dataChanged))
            {
                return;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    codec.Encode(stream, editedFile);
                    currentPackedFile.Data = stream.ToArray();
                }

                // Also save off the configuration.
                UpdateConfig();

                dataChanged = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                ErrorDialog.ShowDialog(ex);
#endif
            }
        }
        #endregion

        /********************************************************************************************
         * This function uses the schema of currentTable to generate dbDataGrid's columns           *
         * programmatically.  This is necessary to not only create the combo box cells properly     *
         * but to set up more complex bindings than are allowed in the XAML.                        *
         ********************************************************************************************/
        void GenerateColumns(bool clearHidden = true)
        {
            dbDataGrid.Columns.Clear();

            if (clearHidden)
            {
                hiddenColumns.Clear();

                if (savedconfig.HiddenColumns.ContainsKey(editedFile.CurrentType.Name))
                {
                    hiddenColumns = new List<string>(savedconfig.HiddenColumns[editedFile.CurrentType.Name]);
                }
            }

            foreach (DataColumn column in CurrentTable.Columns)
            {
                bool isRelated = false;
                List<string> referencevalues = new List<string>();
                Visibility columnvisibility = System.Windows.Visibility.Visible;

                // Set initial column visibility
                if (!column.ExtendedProperties.ContainsKey("Hidden"))
                {
                    column.ExtendedProperties.Add("Hidden", false);
                }

                if (hiddenColumns.Contains(column.ColumnName))
                {
                    if (!showAllColumns)
                    {
                        columnvisibility = System.Windows.Visibility.Hidden;
                    }

                    column.ExtendedProperties["Hidden"] = true;
                }

                // Determine relations as assigned by PFM
                if (column.ExtendedProperties.ContainsKey("FKey") && useComboBoxes && !readOnly)
                {
                    referencevalues = DBReferenceMap.Instance.ResolveReference(column.ExtendedProperties["FKey"].ToString()).ToList();

                    if (referencevalues.Count() > 0 && ReferenceContainsAllValues(referencevalues, column))
                    {
                        isRelated = true;
                    }
                }

                if (isRelated && !column.ReadOnly && UseComboBoxes)
                {
                    // Combobox Column
                    DataGridComboBoxColumn constructionColumn = new DataGridComboBoxColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}\nReference Table: {1}\nReference Column: {2}",
                                                    editedFile.CurrentType.Fields[CurrentTable.Columns.IndexOf(column)].TypeCode.ToString(),
                                                    column.ExtendedProperties["FKey"].ToString().Split('.')[0],
                                                    column.ExtendedProperties["FKey"].ToString().Split('.')[1]);

                    // Set the combo boxes items source to the already tested list.
                    constructionColumn.ItemsSource = referencevalues;

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    constructionBinding.Mode = BindingMode.TwoWay;
                    constructionBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    constructionColumn.SelectedItemBinding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
                else if(editedFile.CurrentType.Fields.First(n => n.Name.Equals(column.ColumnName)).TypeCode == TypeCode.Boolean)
                {
                    // Checkbox Column
                    DataGridCheckBoxColumn constructionColumn = new DataGridCheckBoxColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}",
                                                    editedFile.CurrentType.Fields[CurrentTable.Columns.IndexOf(column)].TypeCode.ToString());

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    if (!column.ReadOnly)
                    {
                        constructionBinding.Mode = BindingMode.TwoWay;
                    }
                    else
                    {
                        constructionBinding.Mode = BindingMode.OneWay;
                    }

                    constructionBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                    constructionColumn.Binding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
                else
                {
                    // Textbox Column
                    DataGridTextColumn constructionColumn = new DataGridTextColumn();
                    constructionColumn.Header = column.ColumnName;
                    constructionColumn.IsReadOnly = column.ReadOnly;

                    // Setup the column header's tooltip.
                    string headertooltip = String.Format("Column Data Type: {0}",
                                                    editedFile.CurrentType.Fields[CurrentTable.Columns.IndexOf(column)].TypeCode.ToString());

                    Binding constructionBinding = new Binding(String.Format("{0}", column.ColumnName));
                    if (!column.ReadOnly)
                    {
                        constructionBinding.Mode = BindingMode.TwoWay;
                    }
                    else
                    {
                        constructionBinding.Mode = BindingMode.OneWay;
                    }

                    constructionColumn.Binding = constructionBinding;

                    // Setup the column context menu
                    // TODO: programatically create context menu so hidden item can be bound to current state.
                    Style tempstyle = new System.Windows.Style(typeof(DataGridColumnHeader), (Style)this.Resources["GridHeaderStyle"]);
                    tempstyle.Setters.Add(new Setter(ToolTipService.ToolTipProperty, headertooltip));
                    constructionColumn.HeaderStyle = tempstyle;

                    // Setup the AutoFilter
                    DataTemplate temptemplate = (DataTemplate)this.Resources["AutoFilterGridHeader"];
                    constructionColumn.HeaderTemplate = temptemplate;

                    // Set visibility
                    constructionColumn.Visibility = columnvisibility;

                    dbDataGrid.Columns.Add(constructionColumn);
                }
            }

            // Finally, based on whether we are moving and freezing columns, rearrange their order.
            if (editedFile != null)
            {
                if (moveAndFreezeKeys)
                {
                    FreezeKeys();
                }
            }
        }

        /********************************************************************************************
         * This function constructs the System.Data.DataTable we use to not only store our data,    *
         * but to bind as our visuals data source.                                                  *
         ********************************************************************************************/
        private DataTable CreateTable(DBFile table)
        {
            DataTable constructionTable = new DataTable(currentPackedFile.Name);

            // Clear the dataset and create the data table.
            _loadedDataSet.Tables.Clear();
            _loadedDataSet.Tables.Add(constructionTable);

            DataColumn constructionColumn;
            List<DataColumn> keyList = new List<DataColumn>();
            constructionTable.BeginLoadData();

            foreach (FieldInfo columnInfo in table.CurrentType.Fields)
            {
                // Create the new column, using object as the data type for all columns, this way we avoid the WPF DataGrid's built in
                // data validation abilities in favor of our own implementation.
                constructionColumn = new DataColumn(columnInfo.Name, typeof(string));

                if (columnInfo.TypeCode == TypeCode.Int16 || columnInfo.TypeCode == TypeCode.Int32 || columnInfo.TypeCode == TypeCode.Single)
                {
                    constructionColumn = new DataColumn(columnInfo.Name, typeof(double));
                }

                constructionColumn.AllowDBNull = true;
                constructionColumn.Unique = false;
                constructionColumn.ReadOnly = readOnly;

                // Save the FKey if it exists
                if (!String.IsNullOrEmpty(columnInfo.ForeignReference))
                {
                    constructionColumn.ExtendedProperties.Add("FKey", columnInfo.ForeignReference);
                }

                // If the column is a primary key, save it for later adding
                if (columnInfo.PrimaryKey)
                {
                    keyList.Add(constructionColumn);
                }

                constructionTable.Columns.Add(constructionColumn);
            }

            // If the table has primary keys, set them.
            if (keyList.Count > 0)
            {
                constructionTable.PrimaryKey = keyList.ToArray();
            }

            // Now that the DataTable schema is constructed, add in all the data.
            foreach (List<FieldInstance> rowentry in table.Entries)
            {
                constructionTable.Rows.Add(rowentry.Select(n => n.Value).ToArray<object>());
            }

            constructionTable.EndLoadData();
            constructionTable.AcceptChanges();

            // Finally, generate the visibleRows list based on total number of rows in the new table.
            visibleRows.Clear();
            for (int i = 0; i < constructionTable.Rows.Count; i++)
            {
                visibleRows.Add(System.Windows.Visibility.Visible);
            }

            return constructionTable;
        }

        private void Import(DBFile importfile)
        {
            // If we are here, then importfile has already been imported into editfile, so no need to do any type checking.
            // The old DBE would check for matching keys and overwrite any it found, data validation means this is no longer
            // necessary to maintain GUI integrity.
            
            // Unbind the GUI datasource, and tell currentTable to get ready for new data.
            dbDataGrid.ItemsSource = null;
            currentTable.BeginLoadData();

			MessageBoxResult question = MessageBox.Show("Replace the current data?", "Replace data?", 
                                                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (question == MessageBoxResult.Cancel) {
                return;
            } else if (question == MessageBoxResult.Yes) {
                EditedFile.Entries.Clear();
                currentTable.Clear();
            }

            // Since Data.Rows lacks an AddRange method, enumerate through the entries manually.
            foreach (List<FieldInstance> entry in importfile.Entries)
            {
                DataRow row = currentTable.NewRow();
                row.ItemArray = entry.Select(n => n.Value).ToArray();
                CurrentTable.Rows.Add(row);
            }

            currentTable.EndLoadData();
            dbDataGrid.ItemsSource = CurrentTable.DefaultView;
        }

        private bool ReferenceContainsAllValues(List<string> referencevalues, DataColumn column)
        {
            foreach (object item in column.GetItemArray())
            {
                if (!referencevalues.Contains(item.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        #region PackedFile Events

        // When a packed file is renamed, update the cached table name.
        void CurrentPackedFile_RenameEvent(PackEntry dir, string newName)
        {
            currentTable.TableName = newName;
        }

        #endregion

        #region UserControl Events

        private void dbeControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DebugHelper.PreviewKeyDown(this, e);
        }

        private void TableControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UpdateConfig();
        }

        #endregion

        #region Toolbar Events
        private void AddRowButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Add a new row with default values.
            DataRow row = currentTable.NewRow();
            List<object> items = new List<object>();
            
            for (int i = 0; i < editedFile.CurrentType.Fields.Count; i++)
            {
                items.Add(GetDefaultValue(i));
            }

            row.ItemArray = items.ToArray();
            currentTable.Rows.Add(row);
            CheckRowForErrors(currentTable.Rows.Count - 1);

            dataChanged = true;
            SendDataChanged();
        }

        private void CloneRowButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Only do anything if atleast 1 row is selected.
            if (dbDataGrid.SelectedItems.Count > 0)
            {
                foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                {
                    DataRow row = currentTable.NewRow();
                    List<object> items = new List<object>();
                    items.AddRange(rowview.Row.ItemArray);

                    // If the current row has an error logged, modify that column to default value before attempting to put it in editedFile.
                    if (rowview.Row.HasErrors && rowview.Row.GetColumnsInError().Count(n => rowview.Row.GetColumnError(n).Contains("Error:")) > 0)
                    {
                        foreach (int colindex in rowview.Row.GetColumnsInError().Where(n => rowview.Row.GetColumnError(n).Contains("Error:"))
                                                                                .Select(n => n.Ordinal))
                        {
                            items[colindex] = GetDefaultValue(colindex);
                        }
                    }

                    row.ItemArray = items.ToArray();
                    currentTable.Rows.Add(row);
                    CheckRowForErrors(currentTable.Rows.Count - 1);
                }
            }

            dataChanged = true;
            SendDataChanged();
        }

        private void insertRowButton_Click(object sender, RoutedEventArgs e)
        {
            DataRow newrow = CurrentTable.NewRow();

            if (dbDataGrid.SelectedItems.OfType<DataRowView>().Count() > 0)
            {
                int datarowindex = -1;
                int visualrowindex = -1;

                // First, find the lowest visual row index.
                foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                {
                    if (visualrowindex == -1)
                    {
                        visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                        datarowindex = currentTable.Rows.IndexOf(rowview.Row);
                        continue;
                    }

                    if (visualrowindex > dbDataGrid.Items.IndexOf(rowview))
                    {
                        visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                        datarowindex = currentTable.Rows.IndexOf(rowview.Row);
                    }
                }

                // Now that we have the lowest selected row index, and it's corresponding location in our data source, we can insert.
                InsertRow(datarowindex);
            }
            else if (dbDataGrid.SelectedItems.Count == 1 && !(dbDataGrid.SelectedItems[0] is DataRowView))
            {
                // We should only hit this code if the user is attempting to insert rows with only the blank row selected
                // in this case we want to simply add the rows on to the end of the table.
                InsertRow();
            }
        }

        private void findButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SendKeys.Send("^f");
        }

        private void replaceButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SendKeys.Send("^h");
        }

        private void ExportAsButton_Clicked(object sender, RoutedEventArgs e)
        {
            ExportContextMenu.PlacementTarget = (Button)sender;
            ExportContextMenu.IsOpen = true;
        }

        private void ImportFromButton_Clicked(object sender, RoutedEventArgs e)
        {
            ImportContextMenu.PlacementTarget = (Button)sender;
            ImportContextMenu.IsOpen = true;
        }

        private void ExportTSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string extractTo = null;
            // TODO: Add support for ModManager
            //extractTo = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentModDirectory : null;
            if (extractTo == null)
            {
                DirectoryDialog dialog = new DirectoryDialog
                {
                    Description = "Please point to folder to extract to",
                    SelectedPath = String.IsNullOrEmpty(exportDirectory)
                                    ? System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                                    : exportDirectory
                };
                extractTo = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
                exportDirectory = dialog.SelectedPath;
            }
            if (!string.IsNullOrEmpty(extractTo))
            {
                List<PackedFile> files = new List<PackedFile>();
                files.Add(CurrentPackedFile);
                FileExtractor extractor = new FileExtractor(extractTo) { Preprocessor = new TsvExtractionPreprocessor() };
                extractor.ExtractFiles(files);
                MessageBox.Show(string.Format("File exported to TSV."));
            }
        }


        private void ExportBinaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string extractTo = null;
            // TODO: Add support for ModManager
            //extractTo = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentModDirectory : null;
            if (extractTo == null)
            {
                DirectoryDialog dialog = new DirectoryDialog
                {
                    Description = "Please point to folder to extract to",
                    SelectedPath = String.IsNullOrEmpty(exportDirectory) 
                                    ? System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) 
                                    : exportDirectory
                };
                extractTo = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
                exportDirectory = dialog.SelectedPath;
            }
            if (!string.IsNullOrEmpty(extractTo))
            {
                List<PackedFile> files = new List<PackedFile>();
                files.Add(CurrentPackedFile);
                FileExtractor extractor = new FileExtractor(extractTo);
                extractor.ExtractFiles(files);
                MessageBox.Show(string.Format("File exported as binary."));
            }
        }

        private void ExportCAXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Write PackedFile EncodeasCAXml()
            Refresh();
        }

        private void ImportTSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string initialDirectory = exportDirectory;
            System.Windows.Forms.OpenFileDialog openDBFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = initialDirectory,
                FileName = String.Format("{0}.tsv", EditedFile.CurrentType.Name)
            };

            DBFile loadedfile = null;
            bool tryAgain = false;
            if (openDBFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                importDirectory = System.IO.Path.GetDirectoryName(openDBFileDialog.FileName);
                do
                {
                    try
                    {
                        try
                        {
                            using (var stream = new MemoryStream(File.ReadAllBytes(openDBFileDialog.FileName)))
                            {
                                loadedfile = new TextDbCodec().Decode(stream);
                                // No need to import to editedFile directly, since it will be handled in the 
                                // CurrentTable_TableNewRow event handler.
                                //editedFile.Import(loadedfile);
                                Import(loadedfile);
                            }

                        }
                        catch (DBFileNotSupportedException exception)
                        {
                            showDBFileNotSupportedMessage(exception.Message);
                        }

                        currentPackedFile.Data = (codec.Encode(EditedFile));
                    }
                    catch (Exception ex)
                    {
                        tryAgain = (System.Windows.Forms.MessageBox.Show(string.Format("Import failed: {0}", ex.Message),
                            "Import failed",
                            System.Windows.Forms.MessageBoxButtons.RetryCancel)
                            == System.Windows.Forms.DialogResult.Retry);
                    }
                } while (tryAgain);
            }
        }

        private void ImportCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openDBFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = exportDirectory,
                FileName = String.Format("{0}.csv", EditedFile.CurrentType.Name)
            };

            DBFile loadedfile = null;
            bool tryAgain = false;
            if (openDBFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                importDirectory = System.IO.Path.GetDirectoryName(openDBFileDialog.FileName);
                do
                {
                    try
                    {
                        try
                        {
                            using (var stream = new MemoryStream(File.ReadAllBytes(openDBFileDialog.FileName)))
                            {
                                loadedfile = new TextDbCodec().Decode(stream);
                                // No need to import to editedFile directly, since it will be handled in the 
                                // CurrentTable_TableNewRow event handler.
                                //editedFile.Import(loadedfile);
                                Import(loadedfile);
                            }

                        }
                        catch (DBFileNotSupportedException exception)
                        {
                            showDBFileNotSupportedMessage(exception.Message);
                        }

                        currentPackedFile.Data = (codec.Encode(EditedFile));
                    }
                    catch (Exception ex)
                    {
                        tryAgain = (System.Windows.Forms.MessageBox.Show(string.Format("Import failed: {0}", ex.Message),
                            "Import failed",
                            System.Windows.Forms.MessageBoxButtons.RetryCancel)
                            == System.Windows.Forms.DialogResult.Retry);
                    }
                } while (tryAgain);
            }
        }

        private void ImportBinaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openDBFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = exportDirectory,
                FileName = EditedFile.CurrentType.Name
            };

            DBFile loadedfile = null;
            bool tryAgain = false;
            if (openDBFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                importDirectory = System.IO.Path.GetDirectoryName(openDBFileDialog.FileName);
                do
                {
                    try
                    {
                        try
                        {
                            using (var stream = new MemoryStream(File.ReadAllBytes(openDBFileDialog.FileName)))
                            {
                                loadedfile = codec.Decode(stream);
                                // No need to import to editedFile directly, since it will be handled in the 
                                // CurrentTable_TableNewRow event handler.
                                //editedFile.Import(loadedfile);
                                Import(loadedfile);
                            }

                        }
                        catch (DBFileNotSupportedException exception)
                        {
                            showDBFileNotSupportedMessage(exception.Message);
                        }

                        currentPackedFile.Data = (codec.Encode(EditedFile));
                    }
                    catch (Exception ex)
                    {
                        tryAgain = (System.Windows.Forms.MessageBox.Show(string.Format("Import failed: {0}", ex.Message),
                            "Import failed",
                            System.Windows.Forms.MessageBoxButtons.RetryCancel)
                            == System.Windows.Forms.DialogResult.Retry);
                    }
                } while (tryAgain);
            }
        }

        private void ImportCAXmlMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement
        }

        private void checktableforerrorsButton_Click(object sender, RoutedEventArgs e)
        {
            errorList.Clear();
            currentTable.ClearErrors();
            CheckTableForErrors();

            if (errorList.Count == 0)
            {
                // We didn't find any errors, inform the user.
                MessageBox.Show("No errors found in the current table!");
            }
        }

        #endregion

        #region DataGrid Events

        private void dbDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
            DataGridCell cell = e.EditingElement.Parent as DataGridCell;

            // Used to circumvent the DataTable limitation of only updating cells after the row finishes an edit operation
            // causing the visual grid to only commit changes the editedFile and table state once the user navigates to another row.
            if (e.Row.Item is DataRowView)
            {
                int colindex = currentTable.Columns.IndexOf((string)e.Column.Header);
                int rowindex = currentTable.Rows.IndexOf((e.Row.Item as DataRowView).Row);
                string proposedvalue = "";

                if (cell.Content is TextBox)
                {
                    proposedvalue = (cell.Content as TextBox).Text;
                }
                else if (cell.Content is ComboBox)
                {
                    proposedvalue = (cell.Content as ComboBox).SelectedValue.ToString();
                }
                else if (cell.Content is CheckBox)
                {
                    proposedvalue = (cell.Content as CheckBox).IsChecked.Value.ToString();
                }

                // If the user is editing the blank row to create a new one, do not attempt to edit the value directly since the
                // blank row has not been added to the currentTable's row collection yet.
                if ((e.Row.Item as DataRowView).Row.RowState != DataRowState.Detached)
                {
                    if (ValueIsValid(proposedvalue, colindex))
                    {
                        currentTable.Rows[rowindex].BeginEdit();
                        currentTable.Rows[rowindex][colindex] = proposedvalue;
                        currentTable.Rows[rowindex].EndEdit();
                    }
                    else
                    {
                        // If the proposed value for this cell is invalid we should add an error here manually for the user since it will not
                        // always generate one if the DataGrid's data validation catches it.
                        AddError(rowindex, colindex, String.Format("'{0}' is not a valid value for '{1}'", proposedvalue,
                                                                                                        currentTable.Columns[colindex].ColumnName));
                    }
                }
            }

            Style TempStyle = cell.Style;
            cell.Style = null;
            cell.Style = TempStyle;
        }

        private void dbDataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Used to set up the 2-click edit for ComboBox and CheckBox cells.
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly && !CurrentTable.HasErrors)
            {
                if (cell.Content is CheckBox || cell.Content is ComboBox)
                {
                    cell.Focus();
                    cell.IsEditing = true;
                }
            }

            // Set the find and replace button IsEnabled, once user clicks on grid.
            if (!findButton.IsEnabled)
            {
                findButton.IsEnabled = true;
                replaceButton.IsEnabled = !readOnly;
            }
        }

        private void dbDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer)
            {
                // User clicked 'outside' the datagrid, deselect everthing.
                dbDataGrid.UnselectAll();
                dbDataGrid.UnselectAllCells();
            }
        }

        private void dbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable/Disable Clone Row button based on current selection.
            if (!readOnly)
            {
                cloneRowButton.IsEnabled = dbDataGrid.SelectedItems.Count > 0;
                insertRowButton.IsEnabled = dbDataGrid.SelectedItems.Count > 0;
            }
        }

        private void dbDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var typetest = e.Row.Item;

            if (!(e.Row.Item is DataRowView))
            {
                e.Row.Header = "*";
            }
            else if((e.Row.Item as DataRowView).Row.RowState != DataRowState.Detached)
            {
                int datarowindex = CurrentTable.Rows.IndexOf((e.Row.Item as DataRowView).Row);
                e.Row.Header = datarowindex + 1;

                // Additional error checking on the visibleRows internal list.
                if (datarowindex >= visibleRows.Count)
                {
                    UpdateVisibleRows();
                }
                e.Row.Visibility = visibleRows[datarowindex];
            }// The user just tried to modify the blank row, adding a new detached row to our collection, act accordingly.
            else if ((e.Row.Item as DataRowView).Row.RowState == DataRowState.Detached)
            {
                int datarowindex = currentTable.Rows.Count;
                e.Row.Header = datarowindex + 1;

                // Additional error checking on the visibleRows internal list.
                if (datarowindex >= visibleRows.Count)
                {
                    UpdateVisibleRows();
                }
                e.Row.Visibility = visibleRows[datarowindex];
            }
        }

        private void dbDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Look for Ctrl-F, for Find shortcut.
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (dbDataGrid.SelectedCells.Count == 1 && dbDataGrid.SelectedCells.First().Item is DataRowView)
                {
                    findReplaceWindow.UpdateFindText(currentTable.Rows[dbDataGrid.Items.IndexOf(dbDataGrid.SelectedCells.First().Item)]
                                                                      [(string)dbDataGrid.SelectedCells.First().Column.Header].ToString());
                }

                findReplaceWindow.CurrentMode = FindAndReplaceWindow.FindReplaceMode.FindMode;
                findReplaceWindow.ReadOnly = readOnly;
                findReplaceWindow.Show();
            }

            // Look for Ctrl-H, for Replace shortcut.
            if (e.Key == Key.H && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && !readOnly)
            {
                findReplaceWindow.CurrentMode = FindAndReplaceWindow.FindReplaceMode.ReplaceMode;
                findReplaceWindow.Show();
            }

            // Look for F3, shortcut for Find Next.
            if (e.Key == Key.F3)
            {
                FindNext(findReplaceWindow.FindValue);
            }

            // Look for Insert key press, and check if a row is selected.
            if (!readOnly && e.Key == Key.Insert && dbDataGrid.SelectedItems.Count > 0)
            {
                DataRow newrow = currentTable.NewRow();

                if (dbDataGrid.SelectedItems.OfType<DataRowView>().Count() > 0)
                {
                    int datarowindex = -1;
                    int visualrowindex = -1;

                    // First, find the lowest visual row index.
                    foreach (DataRowView rowview in dbDataGrid.SelectedItems.OfType<DataRowView>())
                    {
                        if (visualrowindex == -1)
                        {
                            visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                            datarowindex = currentTable.Rows.IndexOf(rowview.Row);
                            continue;
                        }

                        if (visualrowindex > dbDataGrid.Items.IndexOf(rowview))
                        {
                            visualrowindex = dbDataGrid.Items.IndexOf(rowview);
                            datarowindex = currentTable.Rows.IndexOf(rowview.Row);
                        }
                    }

                    InsertRow(datarowindex);
                }
                else if (dbDataGrid.SelectedItems.Count == 1 && !(dbDataGrid.SelectedItems[0] is DataRowView))
                {
                    // We should only hit this code if the user is attempting to insert rows with only the blank row selected
                    // in this case we want to simply add the rows on to the end of the table.
                    try
                    {
                        InsertRow();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        ErrorDialog.ShowDialog(ex);
#endif
                    }
                }
            }

            // F6 and F7 will be the shortcut to check for errors.
            if (!readOnly && (e.Key == Key.F6 || e.Key == Key.F7))
            {
                errorList.Clear();
                currentTable.ClearErrors();
                CheckTableForErrors();
            }
        }

        private void dbDataGrid_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            if (e.Item is DataRowView)
            {
                // Clear copy data if row is collapsed from filtering.
                int datarowindex = currentTable.Rows.IndexOf((e.Item as DataRowView).Row);
                if (datarowindex >= visibleRows.Count)
                {
                    UpdateVisibleRows();
                }

                if (visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                {
                    e.ClipboardRowContent.Clear();
                }
            }
        }

        private void dbDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            /*
            DataGridColumn column = e.Column;

            // Prevent the built-in sort from sorting
            e.Handled = true;

            // Determine the direction of the sort.
            ListSortDirection direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            // Set the sort order on the column
            column.SortDirection = direction;

            // Determine what kind of column this is.
            ColumnType coltype;
            int colindex = dbDataGrid.Columns.IndexOf(column);
            TypeCode coltypecode = editedFile.CurrentType.Fields[colindex].TypeCode;
            if (coltypecode == TypeCode.Boolean || coltypecode == TypeCode.String)
            {
                coltype = ColumnType.Text;
            }
            else
            {
                coltype = ColumnType.Number;
            }

            // Use a ListCollectionView to do the sort.
            BindingListCollectionView view = (BindingListCollectionView)CollectionViewSource.GetDefaultView(dbDataGrid.ItemsSource);
            var test = CollectionViewSource.GetDefaultView(dbDataGrid.ItemsSource);

            // Instantiate custom comparer and assign as sort algorithm.
            ColumnComparer customcomparer = new ColumnComparer(direction, coltype);
            //view.CustomSort = customcomparer;
            */
        }

        private void dbDataGridColumnFilterComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox autofilter = (sender as ComboBox);
            string colname = (string)autofilter.DataContext;

            // Create the values we will use for the filter, along with a blank (disable) and an Any and No value filter.
            List<string> filtervalues = new List<string>();
            filtervalues.Add("");

            // If we are dealing with an optional column, add the options to filter by cells that either have any, or no value.
            if (editedFile.CurrentType.Fields[currentTable.Columns.IndexOf(colname)].TypeName.Contains("optstring"))
            {
                filtervalues.Add("Any Value");
                filtervalues.Add("No Value");
            }

            // Now that we have the column name, determine if we need to get the possible values from the table, or it's reference.
            if (dbDataGrid.Columns.Single(n => colname.Equals((string)n.Header)) is DataGridComboBoxColumn)
            {
                // We have a combo box column, use all possible values.
                filtervalues.AddRange((dbDataGrid.Columns.Single(n => colname.Equals((string)n.Header)) as DataGridComboBoxColumn).ItemsSource.Cast<string>().Distinct());
            }
            else
            {
                // We have a regular column, get the existing values for the list.
                List<object> possiblevalues = currentTable.Columns[colname].GetItemArray(false).Distinct().ToList();
                filtervalues.AddRange(possiblevalues.Select(n => n.ToString()));
            }

            autofilter.ItemsSource = filtervalues;
        }

        private void dbDataGridColumnFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Grab the information we'll need.
            ComboBox autofilter = (sender as ComboBox);
            string colname = (string)autofilter.DataContext;
            string filtervalue = (string)autofilter.SelectedValue;

            // Remove any previously set autofilters.
            if (autofilterList.Count(n => n.ApplyToColumn.Equals(colname)) > 0)
            {
                autofilterList.RemoveAll(n => n.ApplyToColumn.Equals(colname));
            }

            // If the user selects the blank item, simply remove the existing filter.
            if (!String.IsNullOrEmpty(filtervalue))
            {
                // Construct a new filter and add it to the autofilterList.
                DBFilter newfilter = new DBFilter();
                newfilter.ApplyToColumn = colname;
                newfilter.IsActive = true;
                newfilter.Name = String.Format("Auto_{0}_{1}", colname, filtervalue);
                newfilter.FilterValue = filtervalue;
                newfilter.MatchMode = MatchType.Exact;

                if (filtervalue.Equals("Any Value"))
                {
                    newfilter.MatchMode = MatchType.NotEmpty;
                }
                else if (filtervalue.Equals("No Value"))
                {
                    newfilter.MatchMode = MatchType.Empty;
                }
                else
                {
                    
                }

                autofilterList.Add(newfilter);
            }

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        #endregion

        #region Find and Replace

        private void findWindow_FindNext(object sender, EventArgs e)
        {
            FindNext(findReplaceWindow.FindValue);
        }

        private void findReplaceWindow_FindAll(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void replaceWindow_Replace(object sender, EventArgs e)
        {
            // If nothing is selected, then find something to replace first.
            if (dbDataGrid.SelectedCells.Count == 0)
            {
                // If we fail to find a match, return.
                if (!FindNext(findReplaceWindow.FindValue))
                {
                    return;
                }
            }

            // If nothing is STILL selected, then we found nothing to replace.
            // Or, if more than 1 cell is selected, we have a problem.
            if (dbDataGrid.SelectedCells.Count == 0 || dbDataGrid.SelectedCells.Count > 1)
            {
                return;
            }

            int rowindex = dbDataGrid.Items.IndexOf(dbDataGrid.SelectedCells.First().Item);
            int colindex = dbDataGrid.Columns.IndexOf(dbDataGrid.SelectedCells.First().Column);

            while (findReplaceWindow.ReplaceValue.Equals(currentTable.Rows[rowindex][colindex].ToString()))
            {
                // If what is selected has already been replaced, move on to the next match, returning if we fail.
                if (!FindNext(findReplaceWindow.FindValue))
                {
                    return;
                }

                // Update current coordinates.
                rowindex = dbDataGrid.Items.IndexOf(dbDataGrid.SelectedCells.First().Item);
                colindex = dbDataGrid.Columns.IndexOf(dbDataGrid.SelectedCells.First().Column);
            }
            
            if (findReplaceWindow.FindValue.Equals(currentTable.Rows[rowindex][colindex].ToString()))
            {
                // Test for a combobox comlumn.
                if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    if (ComboBoxColumnContainsValue((DataGridComboBoxColumn)dbDataGrid.Columns[colindex], findReplaceWindow.ReplaceValue))
                    {
                        // The value in the Replace field is not valid for this column, alert user and return.
                        MessageBox.Show(String.Format("The value '{0}', is not a valid value for Column '{1}'", 
                                                      findReplaceWindow.ReplaceValue, 
                                                      (string)dbDataGrid.Columns[colindex].Header));

                        return;
                    }
                }

                // Assign the value, and update the UI
                currentTable.Rows[rowindex][colindex] = findReplaceWindow.ReplaceValue;
                RefreshCell(rowindex, colindex);
            }
        }

        private void replaceWindow_ReplaceAll(object sender, EventArgs e)
        {
            // Clear selection, so that FindNext() starts at the beginning of the table.
            dbDataGrid.SelectedCells.Clear();

            int rowindex;
            int colindex;

            while (FindNext(findReplaceWindow.FindValue))
            {
                // Update current coordinates.
                rowindex = dbDataGrid.Items.IndexOf(dbDataGrid.SelectedCells.First().Item);
                colindex = dbDataGrid.Columns.IndexOf(dbDataGrid.SelectedCells.First().Column);

                if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    if (ComboBoxColumnContainsValue((DataGridComboBoxColumn)dbDataGrid.Columns[colindex], findReplaceWindow.ReplaceValue))
                    {
                        // The value in the Replace field is not valid for this column, alert user and continue.
                        MessageBox.Show(String.Format("The value '{0}', is not a valid value for Column '{1}'",
                                                      findReplaceWindow.ReplaceValue,
                                                      (string)dbDataGrid.Columns[colindex].Header));
                        continue;
                    }
                }

                // Assign the value, and update the UI
                currentTable.Rows[rowindex][colindex] = findReplaceWindow.ReplaceValue;
                RefreshCell(rowindex, colindex);
            }
        }

        private bool FindNext(string findthis)
        {
            if (String.IsNullOrEmpty(findthis))
            {
                MessageBox.Show("Nothing entered in Find bar!");
                return false;
            }

            // Set starting point at table upper left.
            int rowstartindex = 0;
            int colstartindex = 0;

            // If the user has a single cell selected, assume this as starting point.
            if (dbDataGrid.SelectedCells.Count == 1)
            {
                rowstartindex = currentTable.Rows.IndexOf((dbDataGrid.SelectedCells.First().Item as DataRowView).Row);
                colstartindex = currentTable.Columns.IndexOf((string)dbDataGrid.SelectedCells.First().Column.Header);
            }

            bool foundmatch = false;
            bool atstart = true;
            for (int i = rowstartindex; i < dbDataGrid.Items.Count; i++)
            {
                // Additional error checking on the visibleRows internal list.
                if (i >= visibleRows.Count)
                {
                    UpdateVisibleRows();
                }

                // Ignore the blank row, and any collapsed (filtered) rows.
                if (!(dbDataGrid.Items[i] is DataRowView) || visibleRows[i] == System.Windows.Visibility.Collapsed)
                {
                    continue;
                }

                for (int j = 0; j < dbDataGrid.Columns.Count; j++)
                {
                    if (atstart)
                    {
                        j = colstartindex;
                        atstart = false;
                    }

                    // Skip current cell.
                    if (i == rowstartindex && j == colstartindex)
                    {
                        continue;
                    }

                    foundmatch = DBUtil.isMatch(currentTable.Rows[i][j].ToString(), findthis);

                    if (foundmatch)
                    {
                        // Clears current selection for new selection.
                        dbDataGrid.SelectedCells.Clear();
                        SelectCell(i, j, true);
                        break;
                    }
                }

                if (foundmatch)
                {
                    break;
                }
            }

            if (!foundmatch)
            {
                MessageBox.Show("No More Matches Found.");
            }

            return foundmatch;
        }

        #endregion

        #region Paste Code

        protected virtual void OnExecutedPaste(object sender, ExecutedRoutedEventArgs args)
        {
            ClipboardHelper.OnExecutedPaste(this);
        }

        protected virtual void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs args)
        {
            ClipboardHelper.OnCanExecutePaste(this, args);
        }

        #endregion

        #region Context Menu Events

        private void ColumnHeaderContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            DataGridColumn col = ((sender as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;
            ContextMenu currentmenu = (ContextMenu)sender;

            Type columntype = GetTypeFromCode(editedFile.CurrentType.Fields[currentTable.Columns.IndexOf(colname)].TypeCode);
            foreach (MenuItem item in currentmenu.Items.OfType<MenuItem>())
            {
                // Enable/Disable Remove Sorting item based on if the column is actually sorted or not.
                if(item.Header.Equals("Remove Sorting"))
                {
                    item.IsEnabled = col.SortDirection != null;
                }

                // Enable/Disable Apply expression and renumber based on column type.
                if (item.Header.Equals("Apply Expression") || item.Header.Equals("Renumber Cells"))
                {
                    if (!col.IsReadOnly && (columntype.Name.Equals("Single") || columntype.Name.Equals("Int32") || columntype.Name.Equals("Int16")))
                    {
                        item.IsEnabled = true;
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }
                }

                // Enable.Disable Mass edit, limiting it to string columns only.
                if (item.Header.Equals("Mass Edit"))
                {
                    if (!readOnly && editedFile.CurrentType.Fields[currentTable.Columns.IndexOf(colname)].TypeCode == TypeCode.String)
                    {
                        item.IsEnabled = true;
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }

        private void SelectColumnMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;

            int columnindex = dbDataGrid.Columns.IndexOf(col);
            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                // Test if the cell is already contained in SelectedCells
                DataGridCellInfo cellinfo = new DataGridCellInfo(dbDataGrid.Items[i], col);
                if (!dbDataGrid.SelectedCells.Contains(cellinfo))
                {
                    dbDataGrid.SelectedCells.Add(cellinfo);
                }
            }
        }

        private void RemoveSortingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            col.SortDirection = null;
            Refresh();
        }

        private void ColumnApplyExpressionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;

            ApplyExpressionWindow getexpwindow = new ApplyExpressionWindow();
            getexpwindow.ShowDialog();

            if (getexpwindow.DialogResult != null && (bool)getexpwindow.DialogResult)
            {
                for (int i = 0; i < currentTable.Rows.Count; i++)
                {
                    // Skip any filtered rows, or any rows with an error in the column we are computing.
                    UpdateVisibleRows();
                    if (visibleRows[i] == System.Windows.Visibility.Collapsed ||
                        errorList.Count(n => n.RowIndex == i && n.ColumnName.Equals(colname)) > 0)
                    {
                        continue;
                    }

                    // Grab the given expression, modifying it for each cell.
                    string expression = getexpwindow.EnteredExpression.Replace("x", string.Format("{0}", currentTable.Rows[i][colname]));
                    object newvalue = currentTable.Compute(expression, "");
                    int colindex = currentTable.Columns.IndexOf(colname);

                    // For integer based columns, do a round first if necessary.
                    if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int32 || 
                        editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int16)
                    {
                        int newintvalue;
                        if(!Int32.TryParse(newvalue.ToString(), out newintvalue))
                        {
                            double tempvalue = Double.Parse(newvalue.ToString());
                            tempvalue = Math.Round(tempvalue, 0);
                            newintvalue = (int)tempvalue;
                        }

                        newvalue = newintvalue;
                    }
                    currentTable.Rows[i][colname] = newvalue;
                }
            }

            RefreshColumn(dbDataGrid.Columns.IndexOf(col));
        }

        private void ColumnMassEditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;

            InputBox stringeditbox = new InputBox();
            stringeditbox.Input = "{cell}";
            stringeditbox.ShowDialog();

            if (stringeditbox.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int datacolindex = currentTable.Columns.IndexOf(colname);
                for (int i = 0; i < currentTable.Rows.Count; i++)
                {
                    // Make sure our visible rows are up to date and skip any currently filtered rows.
                    if (visibleRows.Count <= currentTable.Rows.Count)
                    {
                        UpdateVisibleRows();
                    }
                    if (visibleRows[i] == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    currentTable.Rows[i][datacolindex] = stringeditbox.Input.Replace("{cell}", currentTable.Rows[i][datacolindex].ToString());

                    // Refresh the cell in the UI, using its visual coordinates.
                    RefreshCell(dbDataGrid.Items.IndexOf(currentTable.DefaultView[i]), dbDataGrid.Columns.IndexOf(col));
                }
            }
        }

        private void RenumberMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Get the column index this context menu was called from.
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;
            List<int> visualroworder = new List<int>();

            InputBox renumberInputBox = new InputBox { Text = "Re-Number from", Input = "1" };
            if (renumberInputBox.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    int parsedNumber = int.Parse(renumberInputBox.Input);
                    for (int i = 0; i < dbDataGrid.Items.Count; i++)
                    {
                        // Skip any non DataRowView, which should only be the blank row at the bottom.
                        if (!(dbDataGrid.Items[i] is DataRowView))
                        {
                            continue;
                        }
                        // Store the data row index associated with the current visual row to account for column sorting.
                        visualroworder.Add(currentTable.Rows.IndexOf((dbDataGrid.Items[i] as DataRowView).Row));
                    }

                    // Now that we have a set order, we can assign values.
                    for (int i = 0; i < visualroworder.Count; i++)
                    {
                        currentTable.Rows[visualroworder[i]][colname] = parsedNumber + i;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Could not apply values: {0}", ex.Message), "You fail!");
                }
            }

            RefreshColumn(dbDataGrid.Columns.IndexOf(col));
        }


        private void EditVisibleListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var hiddencolumnslisteditor = ColumnVisiblityHelper.Show(this);
            System.Windows.Forms.DialogResult result = hiddencolumnslisteditor.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                hiddenColumns.Clear();
                hiddenColumns = hiddencolumnslisteditor.RightList;

                foreach (DataColumn column in CurrentTable.Columns)
                {
                    if (hiddencolumnslisteditor.LeftList.Contains(column.ColumnName))
                    {
                        column.ExtendedProperties["Hidden"] = false;
                        dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        column.ExtendedProperties["Hidden"] = true;

                        if (showAllColumns)
                        {
                            dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }

                UpdateConfig();

                if (showAllColumns)
                {
                    Refresh();
                }
            }
        }

        private void ClearTableHiddenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataGridColumn column in dbDataGrid.Columns)
            {
                DataColumn datacolumn = currentTable.Columns[(string)column.Header];

                if (!datacolumn.ExtendedProperties.ContainsKey("Hidden"))
                {
                    datacolumn.ExtendedProperties.Add("Hidden", false);
                }
                datacolumn.ExtendedProperties["Hidden"] = false;

                column.Visibility = Visibility.Visible;
            }

            // Clear the internal hidden columns list.
            hiddenColumns.Clear();
            UpdateConfig();

            if (showAllColumns)
            {
                Refresh();
            }
        }

        private void ClearAllHiddenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Prompt for confirmation.
            string text = "Are you sure you want to clear all saved hidden column information?";
            string caption = "Clear Confirmation";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Question;

            MessageBoxResult result = MessageBox.Show(text, caption, button, image);

            if (result == MessageBoxResult.Yes)
            {
                // Clear internal list.
                hiddenColumns.Clear();

                // Clear saved list.
                savedconfig.HiddenColumns.Clear();
            }
            UpdateConfig();

            if (showAllColumns)
            {
                Refresh();
            }
        }

        private void RowHeaderContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu)
            {
                bool insertrowsavailable = true;
                if ((sender as ContextMenu).DataContext is DataRowView)
                {
                    insertrowsavailable = true;
                }

                foreach (MenuItem item in (sender as ContextMenu).Items)
                {
                    string itemheader = item.Header.ToString();
                    if (itemheader.Equals("Insert Row") || itemheader.Equals("Insert Multiple Rows"))
                    {
                        item.IsEnabled = insertrowsavailable;
                    }
                }
            }
        }

        private void RowHeaderInsertRow_Click(object sender, RoutedEventArgs e)
        {
            if (!readOnly && sender is MenuItem)
            {
                // Double check that whant triggered the event is what we expect.
                if ((sender as MenuItem).DataContext is DataRowView)
                {
                    // Determine visual and data index of calling row.
                    int datarowindex = dbDataGrid.Items.IndexOf(((sender as MenuItem).DataContext as DataRowView));
                    InsertRow(datarowindex);
                }
                else
                {
                    // We'll end up here if the user is attempting to insert a row in front of the blank row, so we will simply add a new row
                    // at the end of the table, still we'll use a try block in case something unforseen happens.
                    try
                    {
                        InsertRow();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        ErrorDialog.ShowDialog(ex);
#endif
                    }
                }
            }
        }

        private void RowHeaderInsertManyRows_Click(object sender, RoutedEventArgs e)
        {
            if (!readOnly && sender is MenuItem)
            {
                // Request how many rows should be inserted from the user.
                InputBox insertrowsInputBox = new InputBox();
                insertrowsInputBox.ShowDialog();

                // Double check that whant triggered the event is what we expect.
                if (insertrowsInputBox.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    // Determine how many rows the user wants to add.
                    int numrows = 0;
                    try
                    {
                        numrows = int.Parse(insertrowsInputBox.Input);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException)
                        {
                            MessageBox.Show(String.Format("Input: {0}, is not a valid number of rows, please enter a whole number.", insertrowsInputBox.Input));
                        }
                        else
                        {
#if DEBUG
                            ErrorDialog.ShowDialog(ex);
#endif
                        }
                    }

                    for (int i = 0; i < numrows; i++)
                    {
                        DataRow newrow = currentTable.NewRow();
                        if ((sender as MenuItem).DataContext is DataRowView)
                        {
                            InsertRow(i);
                        }
                        else
                        {
                            try
                            {
                                // If the blank row is calling us, add to the end of the table.
                                InsertRow();
                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                ErrorDialog.ShowDialog(ex);
#endif
                            }
                        }
                    }

                    UpdateVisibleRows();
                    dbDataGrid.Items.Refresh();
                }
            }
        }

        void DataGridContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            bool cellsselected = false;

            if (dbDataGrid.SelectedCells.Count > 0)
            {
                cellsselected = true;
            }

            ContextMenu menu = (ContextMenu)sender;

            foreach (MenuItem item in menu.Items.OfType<MenuItem>())
            {
                if (item.Header.Equals("Copy"))
                {
                    item.IsEnabled = cellsselected;
                }
                else if (item.Header.Equals("Paste"))
                {
                    if (readOnly)
                    {
                        item.IsEnabled = false;
                    }
                    else
                    {
                        item.IsEnabled = cellsselected;
                    }
                }
                else if (item.Header.Equals("Apply Expression to Selected Cells"))
                {
                    item.IsEnabled = cellsselected;
                    if (cellsselected)
                    {
                        Type columntype;
                        foreach (DataGridCellInfo cellinfo in dbDataGrid.SelectedCells)
                        {
                            columntype = GetTypeFromCode(editedFile.CurrentType.Fields[currentTable.Columns.IndexOf((string)cellinfo.Column.Header)].TypeCode);
                            if (readOnly || !(columntype.Name.Equals("Single") || columntype.Name.Equals("Int32") || columntype.Name.Equals("Int16")))
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }
                else if (item.Header.Equals("Mass Edit String Cells"))
                {
                    item.IsEnabled = cellsselected;
                    if (cellsselected)
                    {
                        foreach (int colindex in dbDataGrid.SelectedCells.Select(n => currentTable.Columns[(string)n.Column.Header].Ordinal).Distinct())
                        {
                            if (editedFile.CurrentType.Fields[colindex].TypeCode != TypeCode.String)
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }
                else if (item.Header.Equals("Revert Cell to Original Value"))
                {
                    if (readOnly)
                    {
                        item.IsEnabled = false;
                    }
                    else
                    {
                        item.IsEnabled = cellsselected;
                    }
                }
            }
        }

        private void DataGridCopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Programmatically send a copy shortcut key event.
            System.Windows.Forms.SendKeys.Send("^c");
        }

        private void DataGridPasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Programmatically send a paste shortcut key event.
            System.Windows.Forms.SendKeys.Send("^v");
        }

        private void DataGridApplyExpressionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyExpressionWindow getexpwindow = new ApplyExpressionWindow();
            getexpwindow.ShowDialog();

            if (getexpwindow.DialogResult != null && (bool)getexpwindow.DialogResult)
            {
                foreach (DataGridCellInfo cellinfo in dbDataGrid.SelectedCells)
                {
                    // Determine current cells indecies, row and column
                    int columnindex = currentTable.Columns.IndexOf((string)cellinfo.Column.Header);
                    int rowindex = currentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);

                    // Skip any filtered rows, or any rows with an error in the column we are computing.
                    UpdateVisibleRows();
                    if (visibleRows[rowindex] == System.Windows.Visibility.Collapsed ||
                        errorList.Count(n => n.RowIndex == rowindex && n.ColumnName.Equals((string)cellinfo.Column.Header)) > 0)
                    {
                        continue;
                    }

                    // Get the expression, replacing x for the current cell's value.
                    string expression = getexpwindow.EnteredExpression.Replace("x", string.Format("{0}", currentTable.Rows[rowindex][columnindex]));

                    // Compute spits out the new value after the current value is applied to the expression given.
                    object newvalue = currentTable.Compute(expression, "");

                    // For integer based columns, do a round first if necessary.
                    if (editedFile.CurrentType.Fields[columnindex].TypeCode == TypeCode.Int32 ||
                        editedFile.CurrentType.Fields[columnindex].TypeCode == TypeCode.Int16)
                    {
                        int newintvalue;
                        if (!Int32.TryParse(newvalue.ToString(), out newintvalue))
                        {
                            double tempvalue = Double.Parse(newvalue.ToString());
                            tempvalue = Math.Round(tempvalue, 0);
                            newintvalue = (int)tempvalue;
                        }

                        newvalue = newintvalue;
                    }
                    currentTable.Rows[rowindex][columnindex] = newvalue;

                    // Refresh the cell in the UI, using its visual coordinates.
                    RefreshCell(dbDataGrid.Items.IndexOf(cellinfo.Item), dbDataGrid.Columns.IndexOf(cellinfo.Column));
                }
            }
        }

        private void DataGridMassEditStringsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            InputBox stringeditbox = new InputBox();
            stringeditbox.Input = "{cell}";
            stringeditbox.ShowDialog();

            if (stringeditbox.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int datarowindex = -1;
                int datacolindex = -1;
                foreach (DataGridCellInfo cellinfo in dbDataGrid.SelectedCells.Where(n => n.Item is DataRowView))
                {
                    datarowindex = currentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);
                    datacolindex = currentTable.Columns.IndexOf((string)cellinfo.Column.Header);

                    // Make sure our visible rows are up to date and skip any currently filtered rows.
                    if (visibleRows.Count <= currentTable.Rows.Count)
                    {
                        UpdateVisibleRows();
                    }
                    if (visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    currentTable.Rows[datarowindex][datacolindex] = stringeditbox.Input.Replace("{cell}", currentTable.Rows[datarowindex][datacolindex].ToString());

                    // Refresh the cell in the UI, using its visual coordinates.
                    RefreshCell(dbDataGrid.Items.IndexOf(cellinfo.Item), dbDataGrid.Columns.IndexOf(cellinfo.Column));
                }
            }
        }

        private void DataGridRevertCellMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataGridCellInfo cellinfo in dbDataGrid.SelectedCells.Where(n => n.Item is DataRowView))
            {
                // Ignore added or detached cells since they have no original values and will instead throw an error.
                if ((cellinfo.Item as DataRowView).Row.RowState == DataRowState.Detached ||
                    (cellinfo.Item as DataRowView).Row.RowState == DataRowState.Added)
                {
                    continue;
                }

                int datarowindex = currentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);
                int datacolindex = currentTable.Columns.IndexOf((string)cellinfo.Column.Header);

                // Make sure our visible rows are up to date and skip any currently filtered rows.
                if (visibleRows.Count <= currentTable.Rows.Count)
                {
                    UpdateVisibleRows();
                }
                if (visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                {
                    continue;
                }

                currentTable.Rows[datarowindex][datacolindex] =  currentTable.Rows[datarowindex][datacolindex, DataRowVersion.Original];
            }
        }

        #endregion

        #region Datatable Events

        void CurrentTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            // Add the new row to editedfile.
            List<FieldInstance> dbfileconstructionRow = new List<FieldInstance>();
            for (int i = 0; i < e.Row.ItemArray.Length; i++)
            {
                dbfileconstructionRow.Add(editedFile.CurrentType.Fields[i].CreateInstance());
            }

            // Modify the new row to have default data initially.
            List<object> vals = new List<object>();
            for (int i = 0; i < editedFile.CurrentType.Fields.Count; i++)
            {
                vals.Add(GetDefaultValue(i));
            }

            // Since we have no idea where a new row should go yet, append it and rely on others to move it.
            editedFile.Entries.Add(new DBRow(editedFile.CurrentType, dbfileconstructionRow));
            visibleRows.Add(System.Windows.Visibility.Visible);

            // Do not set the new itemarray until editedFile has been updated.
            e.Row.ItemArray = vals.ToArray();

            dataChanged = true;
            SendDataChanged();
        }

        private int deletingindex = -1;
        void CurrentTable_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            deletingindex = currentTable.Rows.IndexOf(e.Row);

            editedFile.Entries.RemoveAt(deletingindex);

            // Additional error checking for the visibleRows internal list.
            if (deletingindex >= visibleRows.Count)
            {
                UpdateVisibleRows();
            }

            visibleRows.RemoveAt(deletingindex);

            dataChanged = true;
            SendDataChanged();
        }

        void CurrentTable_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            if (e.Row.RowState != DataRowState.Detached)
            {
                int removalindex = currentTable.Rows.IndexOf(e.Row);
                // Remove the row, because otherwise there will be indexing issues due to how the DataTable class handles row deletion.
                currentTable.Rows.Remove(e.Row);
            }

            // Grab the deleting error's error message if it is a key warning.
            string errormessage = "";
            if (errorList.Count(n => n.ErrorMessage.Contains("key sequence") && n.RowIndex == deletingindex) > 0)
            {
                errormessage = errorList.First(n => n.ErrorMessage.Contains("key sequence") && n.RowIndex == deletingindex).ErrorMessage;
            }

            // Next remove any errors generated by the deleting row in our observable collection.
            List<int> errorindicies = new List<int>();
            errorindicies.AddRange(errorList.Where(n => n.RowIndex == deletingindex).Select(n => errorList.IndexOf(n)).Distinct());
            errorindicies.Sort();
            errorindicies.Reverse();
            errorindicies.ForEach(n => errorList.RemoveAt(n));

            // If we have a key warning in the row, update it and test the original key for uniqueness, but only if it hasn't already been deleted.
            if (!String.IsNullOrEmpty(errormessage) && errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != deletingindex) > 0)
            {
                // We then need to call CheckCellForWarnings again to test if the original (first appearance of) key is now unique.
                int firstrowindex = errorList.Where(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != deletingindex).Select(n => n.RowIndex).Min();
                if (firstrowindex < deletingindex)
                {
                    // Only check for warnings if the deleting row isn't the first occurence of the key.
                    int firstcolindex = currentTable.Columns.IndexOf(currentTable.PrimaryKey[0]);
                    CheckCellForWarnings(firstrowindex, firstcolindex);
                }
            }

            // Update every error whose row index just changed.
            foreach (DBError err in errorList.Where(n => n.RowIndex > deletingindex))
            {
                err.RowIndex = err.RowIndex - 1;
            }

            deletingindex = -1;
            dataChanged = true;
            SendDataChanged();
            RefreshRowHeaders();
        }

        void CurrentTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            // Set row index as either the last row in edited file if we are creating a new row.
            int rowIndex = e.Row.RowState == DataRowState.Detached ? EditedFile.Entries.Count - 1 : e.Row.Table.Rows.IndexOf(e.Row);
            int colIndex = e.Column.Ordinal;

            // If the row is detached, then is recently added or cloned and not part of currentTable, so if it contains an error use the default value.
            if (e.Row.RowState == DataRowState.Detached)
            {
                if (ValueIsValid(e.ProposedValue, colIndex))
                {
                    editedFile.Entries[rowIndex][colIndex].Value = e.ProposedValue.ToString();
                }
                else
                {
                    editedFile.Entries[rowIndex][colIndex].Value = GetDefaultValue(colIndex).ToString();
                    List<object> templist = new List<object>();
                    templist.AddRange(e.Row.ItemArray);
                    templist[colIndex] = GetDefaultValue(colIndex);
                    e.Row.ItemArray = templist.ToArray();
                }
            }
            else
            {
                // Only modify editedFile if the cell does not have an error.
                if (!CheckCellForErrors(rowIndex, colIndex))
                {
                    editedFile.Entries[rowIndex][colIndex].Value = e.ProposedValue.ToString();
                }
            }

            dataChanged = true;
            SendDataChanged();
            RefreshCell(rowIndex, colIndex);
        }



        #endregion

        #region INotifyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private void SendDataChanged()
        {
            // This method is used to trip the packedFile's data changed notification, so that the PFM tree list updates
            // when data is changed, instead of once a user navigates away.
            if (dataChanged)
            {
                currentPackedFile.Modified = true;
            }
        }
        #endregion

        #region UI Helper Methods

        public DataGridCell GetCell(int row, int column, bool onlyvisible = false)
        {
            DataGridRow rowContainer = GetRow(row, onlyvisible);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // UI Virtualization may interfere with the presenter, so scroll to the item and try again.
                if (presenter == null)
                {
                    // If vitrualized and not in view, ignore based on optional paramater.
                    if (onlyvisible)
                    {
                        return null;
                    }

                    dbDataGrid.ScrollIntoView(rowContainer, dbDataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // If virtualized possibly ignore
                    if (onlyvisible)
                    {
                        return null;
                    }

                    // now try to bring into view and retreive the cell
                    dbDataGrid.ScrollIntoView(rowContainer, dbDataGrid.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        public DataGridRow GetRow(int index, bool onlyvisible = false)
        {
            DataGridRow row = (DataGridRow)dbDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null && !onlyvisible)
            {
                // may be virtualized, bring into view and try again
                dbDataGrid.ScrollIntoView(dbDataGrid.Items[index]);
                row = (DataGridRow)dbDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            var test = LogicalTreeHelper.GetChildren(parent);
            LogicalTreeHelper.BringIntoView(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }

                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        static List<T> GetVisualChildren<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            List<T> returnlist = new List<T>();
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            var jtest = LogicalTreeHelper.GetChildren(parent);
            LogicalTreeHelper.BringIntoView(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    var test = GetVisualChildren<T>(v);
                    if (test == null)
                    {
                        continue;
                    }
                    returnlist.AddRange(GetVisualChildren<T>(v));
                }

                if (child != null)
                {
                    returnlist.Add(child as T);
                }
            }

            return returnlist;
        }

        public DependencyObject FindFirstControlInChildren(DependencyObject obj, string controlType)
        {
            if (obj == null)
                return null;

            // Get a list of all occurrences of a particular type of control (eg "CheckBox") 
            IEnumerable<DependencyObject> ctrls = FindInVisualTreeDown(obj, controlType);
            if (ctrls.Count() == 0)
                return null;

            return ctrls.First();
        }

        public IEnumerable<DependencyObject> FindInVisualTreeDown(DependencyObject obj, string type)
        {
            if (obj != null)
            {
                if (obj.GetType().ToString().EndsWith(type))
                {
                    yield return obj;
                }

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    foreach (var child in FindInVisualTreeDown(VisualTreeHelper.GetChild(obj, i), type))
                    {
                        if (child != null)
                        {
                            yield return child;
                        }
                    }
                }
            }
            yield break;
        }

        public void Refresh(bool onlyvisible = false)
        {
            if (!onlyvisible)
            {
                // TODO: Try to find another way to force refresh the screen.
                var selectedItems = dbDataGrid.SelectedItems;
                List<DataGridCellInfo> selectedCells = dbDataGrid.SelectedCells.ToList();
                List<int> selectedItemsIndicies = dbDataGrid.SelectedItems.OfType<DataRowView>().Select(n => dbDataGrid.Items.IndexOf(n)).ToList();

                dbDataGrid.ItemsSource = null;
                dbDataGrid.ItemsSource = CurrentTable.DefaultView;

                foreach (int index in selectedItemsIndicies)
                {
                    dbDataGrid.SelectedItems.Add(dbDataGrid.Items[index]);
                }

                foreach (DataGridCellInfo cellinfo in selectedCells)
                {
                    DataGridCellInfo cellToAdd = new DataGridCellInfo(cellinfo.Item, cellinfo.Column);
                    if (!dbDataGrid.SelectedCells.Contains(cellToAdd))
                    {
                        dbDataGrid.SelectedCells.Add(cellToAdd);
                    }
                }
            }
            else // Refresh only visible elements, column by column.
            {
                for (int i = 0; i < currentTable.Columns.Count; i++)
                {
                    RefreshColumn(i);
                }
            }
        }

        private void RefreshColumn(int column)
        {
            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                DataGridCell cell = GetCell(i, column, true);

                if (cell != null)
                {
                    // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
                    Style TempStyle = cell.Style;
                    cell.Style = null;
                    cell.Style = TempStyle;
                }
            }
        }

        public void RefreshCell(int row, int column)
        {
            DataGridCell cell = GetCell(row, column, true);

            if (cell != null)
            {
                // Resetting the cell's Style forces a UI ReDraw to occur without disturbing the rest of the datagrid.
                Style TempStyle = cell.Style;
                cell.Style = null;
                cell.Style = TempStyle;
            }
        }

        private void SelectCell(int rowindex, int colindex, bool scrollview = false)
        {
            // Add the cell to the selected cells list.
            DataGridCellInfo cellinfo = new DataGridCellInfo(dbDataGrid.Items[rowindex], dbDataGrid.Columns[colindex]);
            if (!dbDataGrid.SelectedCells.Contains(cellinfo))
            {
                dbDataGrid.SelectedCells.Add(cellinfo);
            }

            // Scroll cell into view if asked.
            if (scrollview)
            {
                dbDataGrid.ScrollIntoView(dbDataGrid.Items[rowindex], dbDataGrid.Columns[colindex]);
            }
        }

        private void RefreshRowHeaders()
        {
            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                DataGridRow row = GetRow(i, true);

                if (row == null)
                {
                    continue;
                }

                if (dbDataGrid.Items[i] is DataRowView)
                {
                    row.Header = currentTable.Rows.IndexOf((dbDataGrid.Items[i] as DataRowView).Row) + 1;
                }
                else
                {
                    row.Header = "*";
                }
            }
        }

        #endregion

        #region Utility Functions

        private List<string> GetPrimaryKeySequence(int rowindex)
        {
            List<string> pksequence = new List<string>();

            for (int i = 0; i < currentTable.Columns.Count; i++)
            {
                if (editedFile.CurrentType.Fields[i].PrimaryKey)
                {
                    pksequence.Add(currentTable.Rows[rowindex][i].ToString());
                }
            }

            return pksequence;
        }

        private bool TableContainsKeySequence(List<string> pksequence)
        {
            List<List<string>> pklist = new List<List<string>>();

            // Construct a list of pk sequences in the table.
            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                pklist.Add(GetPrimaryKeySequence(i));
            }

            // Test to see if there are more than 1 occurence of a key sequence.
            if (pklist.Count(n => n.SequenceEqual(pksequence)) > 1)
            {
                return true;
            }

            return false;
        }

        private IEnumerable<int> GetMatchingKeyIndicies(List<string> pksequence)
        {
            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                if (GetPrimaryKeySequence(i).SequenceEqual(pksequence))
                {
                    yield return i;
                }
            }
        }

        public void UpdateConfig()
        {
            if (EditedFile == null)
            {
                return;
            }

            // Save off any required information before changing anything.
            savedconfig.FreezeKeyColumns = moveAndFreezeKeys;
            savedconfig.UseComboBoxes = useComboBoxes;
            savedconfig.ShowAllColumns = showAllColumns;
            savedconfig.ImportDirectory = importDirectory;
            savedconfig.ExportDirectory = exportDirectory;
            savedconfig.ShowFilters = showFilters;
            savedconfig.Save();
        }

        private void showDBFileNotSupportedMessage(string message)
        {
            // Set the warning box as visible.
            dbDataGrid.Visibility = System.Windows.Visibility.Hidden;
            unsupportedDBErrorTextBox.Visibility = System.Windows.Visibility.Visible;

            // Set the message
            unsupportedDBErrorTextBox.Text = string.Format("{0}{1}", message, string.Join("\r\n", DBTypeMap.Instance.DBFileTypes));

            // Modify controls accordingly
            // Most controls useability are bound by TableReadOnly, so set it.
            readOnly = true;
            // Modify the remaining controls manually.
            exportAsButton.IsEnabled = false;
        }

        private Type GetTypeFromCode(TypeCode code)
        {
            switch (code)
            {
                case TypeCode.Boolean:
                    return typeof(bool);

                case TypeCode.Byte:
                    return typeof(byte);

                case TypeCode.Char:
                    return typeof(char);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Decimal:
                    return typeof(decimal);

                case TypeCode.Double:
                    return typeof(double);

                case TypeCode.Empty:
                    return typeof(string);

                case TypeCode.Int16:
                    return typeof(short);

                case TypeCode.Int32:
                    return typeof(int);

                case TypeCode.Int64:
                    return typeof(long);

                case TypeCode.Object:
                    return typeof(object);

                case TypeCode.SByte:
                    return typeof(sbyte);

                case TypeCode.Single:
                    return typeof(Single);

                case TypeCode.String:
                    return typeof(string);

                case TypeCode.UInt16:
                    return typeof(UInt16);

                case TypeCode.UInt32:
                    return typeof(UInt32);

                case TypeCode.UInt64:
                    return typeof(UInt64);
            }

            return null;
        }

        private void BuiltTablesSetReadOnly(bool tablesreadonly)
        {
            foreach (DataTable table in _loadedDataSet.Tables)
            {
                foreach (DataColumn column in table.Columns)
                {
                    column.ReadOnly = tablesreadonly;
                }
            }
        }

        private bool ComboBoxColumnContainsValue(DataGridComboBoxColumn column, string tocheck)
        {
            if (column.ItemsSource.OfType<object>().Count(n => n.ToString().Equals(tocheck)) != 0)
            {
                return true;
            }

            return false;
        }

        private object GetDefaultValue(int colindex)
        {
            if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.String)
            {
                if (editedFile.CurrentType.Fields[colindex].TypeName.Contains("optstring"))
                {
                    return "";
                }// We have a combo box column, so default any non optional string to the first value in the list.
                else if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    List<string> templist = (List<string>)(dbDataGrid.Columns[colindex] as DataGridComboBoxColumn).ItemsSource;
                    return templist[0];
                }
                else
                {
                    return "";
                }
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Boolean)
            {
                return false;
            }
            else
            {
                return 0;
            }
        }

        private void InsertRow(int rowindex = -1)
        {
            // Create a new row with default values.
            DataRow newrow = currentTable.NewRow();
            List<object> defaultvalues = new List<object>();
            for (int i = 0; i < editedFile.CurrentType.Fields.Count; i++)
            {
                defaultvalues.Add(GetDefaultValue(i));
            }

            newrow.ItemArray = defaultvalues.ToArray();

            if (rowindex > -1)
            {
                currentTable.Rows.InsertAt(newrow, rowindex);

                // Once the new row is added to the table, we need to move the row in the editedFile.
                DBRow temprow = editedFile.Entries[editedFile.Entries.Count - 1];
                editedFile.Entries.Remove(temprow);
                editedFile.Entries.Insert(rowindex, temprow);

                // Now that everything is in its proper place, check for errors.
                CheckRowForErrors(rowindex);
            }
            else
            {
                // If the blank row is calling us, add to the end of the table.
                currentTable.Rows.Add(newrow);
                CheckRowForErrors(currentTable.Rows.Count - 1);
            }

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        #endregion

        #region Filter Methods
        public void UpdateVisibleRows()
        {
            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                if (i >= visibleRows.Count)
                {
                    // We have a problem. Append additional items until we are ok again.
                    while (i >= visibleRows.Count)
                    {
                        visibleRows.Add(System.Windows.Visibility.Visible);
                    }
                }

                // If there are more rows in the datagrid than currentTable we are adding a new row by modifying the blank row, so
                // we should add rows to the list until we max out.
                while (dbDataGrid.Items.Count > visibleRows.Count)
                {
                    visibleRows.Add(System.Windows.Visibility.Visible);
                }

                if (FilterTestRow(i))
                {
                    visibleRows[i] = System.Windows.Visibility.Visible;
                }
                else
                {
                    visibleRows[i] = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private bool FilterTestRow(int rowindex)
        {
            int colindex;

            // Test row against active advanced filters.
            foreach (DBFilter filter in (filterDockPanel.Children[0] as FilterController).filterList)
            {
                if (!filter.IsActive)
                {
                    continue;
                }

                colindex = currentTable.Columns.IndexOf(filter.ApplyToColumn);

                if (!FilterTestValue(currentTable.Rows[rowindex][colindex], filter.FilterValue, filter.MatchMode))
                {
                    return false;
                }
            }

            // Test row against currently active column autofilters.
            foreach (DBFilter filter in autofilterList)
            {
                if (!filter.IsActive)
                {
                    continue;
                }

                colindex = currentTable.Columns.IndexOf(filter.ApplyToColumn);

                if (!FilterTestValue(currentTable.Rows[rowindex][colindex], filter.FilterValue, filter.MatchMode))
                {
                    return false;
                }
            }

            // If either the error or warning filter is engaged test the row agains them as well.
            if (dberrorsDockPanel.Visibility == System.Windows.Visibility.Visible)
            {
                // If both filters are engaged either or will pass.
                if (dberrorfilterCheckBox.IsChecked.Value && dbwarningfilterCheckBox.IsChecked.Value)
                {
                    if (errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Error:")) == 0 &&
                        errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Warning:")) == 0)
                    {
                        return false;
                    }
                }// If only the Warnings filter is engaged, only warnings pass.
                else if (!dberrorfilterCheckBox.IsChecked.Value && dbwarningfilterCheckBox.IsChecked.Value)
                {
                    if (errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Warning:")) == 0)
                    {
                        return false;
                    }
                }// If only the Errors filter is engaged, only errors pass.
                else if (dberrorfilterCheckBox.IsChecked.Value && !dbwarningfilterCheckBox.IsChecked.Value)
                {
                    if (errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Error:")) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool FilterTestValue(object totest, string filtervalue, MatchType matchtype)
        {
            // Match the value exactly.
            if (matchtype == MatchType.Exact)
            {
                if (!totest.ToString().Equals(filtervalue))
                {
                    return false;
                }
            }// Check for partial match.
            else if (matchtype == MatchType.Partial)
            {
                if (!totest.ToString().Contains(filtervalue))
                {
                    return false;
                }
            }// Run a Regex match.
            else if (matchtype == MatchType.Regex)
            {
                if (!Regex.IsMatch(totest.ToString(), filtervalue))
                {
                    return false;
                }
            }// Check for empty values.
            else if (matchtype == MatchType.Empty)
            {
                if (!String.IsNullOrEmpty(totest.ToString()))
                {
                    return false;
                }
            }// Check for not empty values.
            else if (matchtype == MatchType.NotEmpty)
            {
                if (String.IsNullOrEmpty(totest.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Frozen Key Column Methods

        private void FreezeKeys()
        {
            // If there are no keys columns specified, return.
            if (currentTable.PrimaryKey.Count() == 0)
            {
                return;
            }

            // Figure out which columns are key columns.
            List<string> keycolumns = new List<string>();
            keycolumns.AddRange(currentTable.PrimaryKey.Select(n => n.ColumnName));

            for (int i = 0; i < keycolumns.Count; i++)
            {
                // Set the display index of the columns to left most column, retaining key order.
                dbDataGrid.Columns.Single(n => keycolumns[i].Equals((string)n.Header)).DisplayIndex = i;
            }

            dbDataGrid.FrozenColumnCount = keycolumns.Count;
        }

        private void UnfreezeKeys()
        {
            // If there are no keys columns specified, return.
            if (currentTable.PrimaryKey.Count() == 0)
            {
                return;
            }

            // Figure out which columns are key columns.
            List<string> keycolumns = new List<string>();
            keycolumns.AddRange(currentTable.PrimaryKey.Select(n => n.ColumnName));

            for (int i = 0; i < keycolumns.Count; i++)
            {
                // Reset the display index of the key columns back to their original positions.
                dbDataGrid.Columns.Single(n => keycolumns[i].Equals((string)n.Header)).DisplayIndex = currentTable.Columns.IndexOf(keycolumns[i]);
            }

            dbDataGrid.FrozenColumnCount = 0;
        }

        #endregion

        #region Error Checking and Data Validation Methods

        // This method will be called when a table is loaded, generating an observable error list from the cached error data.
        private void RegenerateErrorList()
        {
            // If the table has no errors it either hasn't been checked or is fine, so skip it.
            if (!currentTable.HasErrors)
            {
                return;
            }

            foreach (DataRow row in currentTable.Rows)
            {
                if (!row.HasErrors)
                {
                    continue;
                }

                int rowindex = currentTable.Rows.IndexOf(row);
                bool handledduplicatekey = false;
                foreach (DataColumn column in row.GetColumnsInError())
                {
                    DBError error = new DBError(rowindex, currentTable.DefaultView[rowindex], column.ColumnName, row.GetColumnError(column));

                    if (error.ErrorMessage.Contains("key sequence"))
                    {
                        if (!handledduplicatekey)
                        {
                            errorList.Add(error);
                        }

                        handledduplicatekey = true;
                    }
                    else
                    {
                        errorList.Add(error);
                    }
                }
            }
        }

        private bool CheckTableForErrors(bool checkwarnings = true)
        {
            bool haserrors = false;
            currentTable.ClearErrors();
            errorList.Clear();

            for (int i = 0; i < currentTable.Rows.Count; i++)
            {
                for (int j = 0; j < currentTable.Columns.Count; j++)
                {
                    if (CheckCellForErrors(i, j, checkwarnings))
                    {
                        haserrors = true;
                    }
                    RefreshCell(i, j);
                }
            }

            return haserrors;
        }

        private bool CheckRowForErrors(int rowindex, bool checkwarnings = true)
        {
            bool haserrors = false;

            for (int i = 0; i < currentTable.Columns.Count; i++)
            {
                if (CheckCellForErrors(rowindex, i, checkwarnings))
                {
                    haserrors = true;
                }
                RefreshCell(rowindex, i);
            }

            return haserrors;
        }

        // This methods is used to check for errors only, meaning: invalid data types and missing entries.
        private bool CheckCellForErrors(int rowindex, int colindex, bool checkwarnings = true)
        {
            bool haserrors = false;
            object currentvalue = currentTable.Rows[rowindex][colindex];

            // Test value against required data type.
            if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int16)
            {
                short test;
                haserrors = !short.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a whole number between -32,768 and 32,767.",
                                                        currentvalue, currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int32)
            {
                int test;
                haserrors = !int.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a whole number.",
                                                        currentvalue, currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Single)
            {
                float test;
                haserrors = !float.TryParse(currentvalue.ToString(), out test);

                if (haserrors)
                {
                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs a number (can have decimal point).",
                                                        currentvalue, currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Boolean)
            {
                if(!(currentvalue.ToString().Equals("True") || currentvalue.ToString().Equals("False")))
                {
                    haserrors = true;

                    // Add an error to our internal list for display.
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', it needs 'True' or 'False'.",
                                                        currentvalue, currentTable.Columns[colindex].ColumnName);
                    AddError(rowindex, colindex, errormessage);
                }
            }

            // Next up, if we have a ComboBox column, make sure the current value exists.
            if (dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
            {
                if (!((dbDataGrid.Columns[colindex] as DataGridComboBoxColumn).ItemsSource as List<string>).Contains(currentvalue.ToString()))
                {
                    haserrors = true;

                    // Add an error to our internal list for display.
                    List<string> fkey = currentTable.Columns[colindex].ExtendedProperties["FKey"].ToString().Split('.').ToList();
                    string errormessage = String.Format("Error: '{0}' is not a valid value for '{1}', no matching value in column:'{3}' of '{2}' tables found.",
                                                        currentvalue, currentTable.Columns[colindex].ColumnName, fkey[0], fkey[1]);
                    AddError(rowindex, colindex, errormessage);
                }
            }

            if (checkwarnings)
            {
                // Test the cell for warnings next, passing along our current cell's error state.
                CheckCellForWarnings(rowindex, colindex, haserrors);
            }

            // Lastly, update the visual error list and refresh the cell.
            UpdateErrorView();
            RefreshCell(rowindex, colindex);

            return haserrors;
		}

        // This method is designed to test for warnings only, meaning: duplicate keys.
        private bool CheckCellForWarnings(int rowindex, int colindex, bool haserrors = false)
        {
            // Check for duplications if this cell is a key.
            if (currentTable.PrimaryKey.Contains(currentTable.Columns[colindex]))
            {
                if (TableContainsKeySequence(GetPrimaryKeySequence(rowindex)))
                {
                    // Add an error to our internal list for display.
                    string keysequence = "";
                    foreach (string key in GetPrimaryKeySequence(rowindex))
                    {
                        keysequence += String.Format("'{0}' ", key);
                    }
                    string errormessage = String.Format("Warning: The key sequence {0} is already in this table.", keysequence);

                    // Log a warning for each row with a matching key.
                    foreach (int keyindex in GetMatchingKeyIndicies(GetPrimaryKeySequence(rowindex)))
                    {
                        // If the observable error list already has this error leave it be.
                        if (errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex == keyindex) == 0)
                        {
                            AddError(keyindex, colindex, errormessage);

                            // There are more than 1 primary key, we need to create an internal error for each.
                            if (currentTable.PrimaryKey.Count() > 1)
                            {
                                // Enumerate through the key columns that we have not added an error to yet.
                                foreach (DataColumn column in currentTable.PrimaryKey)
                                {
                                    if (column.Ordinal != colindex)
                                    {
                                        currentTable.Rows[keyindex].SetColumnError(column, errormessage);
                                    }

                                    // Also, refresh those additional cells.
                                    RefreshCell(keyindex, column.Ordinal);
                                }
                            }
                        }
                    }
                    haserrors = true;
                }
            }

            if (!haserrors)
            {
                // The cell is fine, make sure to remove any current errors.
                RemoveError(rowindex, colindex);
            }

            // Lastly, update the visual error list and refresh the cell.
            UpdateErrorView();
            RefreshCell(rowindex, colindex);

            return haserrors;
        }

        private bool ValueIsValid(object testval, int colindex)
        {
            // Test value against required data type.
            if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int16)
            {
                short test;
                return short.TryParse(testval.ToString(), out test);
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int32)
            {
                int test;
                return int.TryParse(testval.ToString(), out test);
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Single)
            {
                float test;
                return float.TryParse(testval.ToString(), out test);
            }
            else if (editedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Boolean)
            {
                if (!(testval.ToString().Equals("True") || testval.ToString().Equals("False")))
                {
                    return false;
                }
            }

            return true;
        }
		
		private void dberrorsListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dberrorsListView.SelectedItems.Count == 1)
            {
                DBError error = (DBError)dberrorsListView.SelectedItem;
                int rowindex = dbDataGrid.Items.IndexOf(error.GridRow);
                int colindex = dbDataGrid.Columns.IndexOf(dbDataGrid.Columns.Single(n => error.ColumnName.Equals((string)n.Header)));

                dbDataGrid.UnselectAllCells();
                dbDataGrid.ScrollIntoView(error.GridRow, dbDataGrid.Columns.Single(n => error.ColumnName.Equals((string)n.Header)));
                SelectCell(rowindex, colindex, true);
            }
        }

        private void AddError(int rowindex, int colindex, string errormessage)
        {
            // First, determine we have an error already logged for this cell and clear it.
            if (currentTable.Rows[rowindex].GetColumnsInError().Contains(currentTable.Columns[colindex]) &&
                errorList.Count(n => n.ColumnName.Equals(currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex) > 0)
            {
                int errorindex = errorList.IndexOf(errorList.First(n => n.ColumnName.Equals(currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex));
                currentTable.Rows[rowindex].RemoveColumnError(currentTable.Columns[colindex].ColumnName);
                errorList.RemoveAt(errorindex);

                // If we have an error, re-add the new error to the same location as the old one.
                DBError error = new DBError(rowindex, currentTable.DefaultView[rowindex], currentTable.Columns[colindex].ColumnName, errormessage);
                errorList.Insert(errorindex, error);
            }
            else
            {
                // Add an error to our internal list for display.
                DBError error = new DBError(rowindex, currentTable.DefaultView[rowindex], currentTable.Columns[colindex].ColumnName, errormessage);
                errorList.Add(error);
            }

            // Add an error into currentTable so that the background is updated accordingly.
            currentTable.Rows[rowindex].SetColumnError(currentTable.Columns[colindex], errormessage);

            UpdateErrorView();
        }

        private void RemoveError(int rowindex, int colindex)
        {
            // If this cell has an error, remove it.
            if (currentTable.Rows[rowindex].GetColumnsInError().Contains(currentTable.Columns[colindex]) &&
                (errorList.Count(n => currentTable.Rows[rowindex].GetColumnsInError().Count(t => t.ColumnName.Equals(n.ColumnName) && t.Ordinal == colindex) > 0) > 0))
            {
                string errormessage = currentTable.Rows[rowindex].GetColumnError(colindex);
                if (currentTable.PrimaryKey.Count() > 1 && errormessage.Contains("key sequence"))
                {
                    // If we are dealing with a duplicate key error in a table with multiple primary keys, we need to clear all key errors from the row.
                    foreach (string columnname in currentTable.PrimaryKey.Select(n => n.ColumnName))
                    {
                        currentTable.Rows[rowindex].RemoveColumnError(columnname);
                        RefreshCell(rowindex, currentTable.Columns.IndexOf(columnname));
                    }
                }
                else
                {
                    // Other wise just remove the error.
                    currentTable.Rows[rowindex].RemoveColumnError(currentTable.Columns[colindex].ColumnName);
                    RefreshCell(rowindex, colindex);
                }

                // Check to see if there are any observable errors that match our current error and check them again.
                if (errorList.Count(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != rowindex) > 0)
                {
                    // We then need to call CheckCellForWarnings again to test if the original (first appearance of) key is now unique.
                    int firstrowindex = errorList.Where(n => n.ErrorMessage.Equals(errormessage) && n.RowIndex != rowindex).Select(n => n.RowIndex).Min();
                    int firstcolindex = currentTable.Columns.IndexOf(currentTable.PrimaryKey[0]);
                    CheckCellForWarnings(firstrowindex, firstcolindex);
                }

                // Remove the error from the observable list.
                errorList.Remove(errorList.First(n => n.ColumnName.Equals(currentTable.Columns[colindex].ColumnName) && n.RowIndex == rowindex));
            }
        }

        private void RemoveRowErrors(int rowindex)
        {
            for (int i = 0; i < editedFile.CurrentType.Fields.Count; i++)
            {
                RemoveError(rowindex, i);
            }
        }

        private void UpdateErrorView()
        {
            // Update our visual error list, collapsing and expanding it as necessary.
            if (errorList.Count == 0)
            {
                errornumberTextBlock.Text = "0";
                warningnumberTextBlock.Text = "0";
                dberrorsDockPanel.Visibility = System.Windows.Visibility.Collapsed;
                dberrorfilterCheckBox.IsChecked = false;
                dbwarningfilterCheckBox.IsChecked = false;
            }
            else
            {
                errornumberTextBlock.Text = errorList.Count(n => n.ErrorMessage.Contains("Error:")).ToString();
                warningnumberTextBlock.Text = errorList.Count(n => n.ErrorMessage.Contains("Warning:")).ToString();
                dberrorsDockPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void dberrorwarningfilterCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // Update the error list visibility properties.
            UpdateErrorView();

            UpdateVisibleRows();
            dbDataGrid.Items.Refresh();
        }

        private void minimizeerrorlistCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            dberrorsListView.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void minimizeerrorlistCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            dberrorsListView.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion
    }

    public static class Extensions
    {
        public static List<object> GetItemArray(this DataColumn column, bool getblanks = true)
        {
            DataTable table = column.Table;
            List<object> Items = new List<object>();

            foreach (DataRow row in table.Rows)
            {
                if (String.IsNullOrEmpty(row[column].ToString()) && !getblanks)
                {
                    continue;
                }
                Items.Add(row[column]);
            }

            Items.Sort();

            return Items;
        }

        public static void RemoveColumnError(this DataRow row, string columnName)
        {
            // Grab a list of columns in error for this row.
            List<DataColumn> columnsinerror = new List<DataColumn>();
            columnsinerror = row.GetColumnsInError().ToList();
            columnsinerror.Remove(row.Table.Columns[columnName]);

            // Since we are not working with a reference, we need to clear and re-add every error in the row.
            List<KeyValuePair<DataColumn, string>> columnanderrorlist = new List<KeyValuePair<DataColumn, string>>();
            foreach (DataColumn column in columnsinerror)
            {
                columnanderrorlist.Add(new KeyValuePair<DataColumn, string>(column, row.GetColumnError(column)));
            }

            // Now that we have this list, clear all errors and re-add.
            row.ClearErrors();

            foreach (KeyValuePair<DataColumn, string> columnanderror in columnanderrorlist)
            {
                row.SetColumnError(columnanderror.Key, columnanderror.Value);
            }
        }

        public static void ClearErrors(this DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                row.ClearErrors();
            }
        }
    }
}
