using Common;
using Filetypes;
using DataGridViewAutoFilter;
using PackFileManager.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using CommonDialogs;
using CommonUtilities;
using Filetypes.Codecs;

namespace PackFileManager {
    public partial class DBFileEditorControl : UserControl, IPackedFileEditor {
        private string referenceTarget = null;
        
        PackedFileDbCodec Codec;

        #region Members
        private DataTable currentDataTable;
        private DBFile EditedFile;
        private PackedFile currentPackedFile;
        public PackedFile CurrentPackedFile { 
            get {
                return currentPackedFile;
            }
            set {
                currentPackedFile = value;
                Open ();
            }
        }
        private bool TableColumnChanged;
        private List<List<FieldInstance>> copiedRows = new List<List<FieldInstance>>();
        private ToolStripButton cloneRowsButton;
        #endregion
        
        #region PackedFileEditor implementation
        public bool CanEdit(PackedFile file) {
            bool result = file.FullPath.StartsWith("db");
            try {
                if (result) {
                    string type = DBFile.Typename(file.FullPath);
                    DBFileHeader header = PackedFileDbCodec.readHeader(file);
                    result = (DBTypeMap.Instance.GetVersionedInfos(type, header.Version).Count > 0);
#if DEBUG
                    if (!result) 
                        Console.WriteLine("Can't edit that!");
#endif
                    return result;
                }
            } catch {
                result = false;
            }
            return result;
        }
        public void Commit() {
            // auto-commits on edit... should change that
        }
        #endregion

        private GridViewCopyPaste copyPaste;

        bool readOnly = true;
        public bool ReadOnly {
            get {
                return readOnly;
            }
            set {
                
                readOnly = value;
                pasteToolStripButton.Enabled = !value;
                cloneRowsButton.Enabled = !value;
                addNewRowButton.Enabled = !value;
                dataGridView.ReadOnly = value;
                Color cellBgColor = value ? Color.LightGray : Color.White;
                foreach (DataGridViewRow row in dataGridView.Rows) {
                    foreach(DataGridViewCell cell in row.Cells) {
                        cell.ReadOnly = value;
                        cell.Style.BackColor = cellBgColor;
                    }
                }
                foreach (DataGridViewColumn column in dataGridView.Columns) {
                    column.DefaultCellStyle.BackColor = cellBgColor;
                }
            }
        }

        public DBFileEditorControl () {
            InitializeComponent ();
            dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridView.ColumnHeaderMouseClick += CreateColumnHeaderPopup;
            copyPaste = new GridViewCopyPaste(dataGridView);
            copyPaste.Copied += delegate() {
                pasteToolStripButton.Enabled = !ReadOnly;
            };
            copyPaste.Pasted += delegate() {
                CurrentPackedFile.Data = Codec.Encode(EditedFile);
                dataGridView.Refresh();
            };

            dataGridView.SelectionChanged += new EventHandler(delegate(object sender, EventArgs args) 
                { cloneRowsButton.Enabled = dataGridView.SelectedRows.Count > 0; });

            dataGridView.DataError += new DataGridViewDataErrorEventHandler(CellErrorHandler);
            this.useComboBoxCells.CheckedChanged += new System.EventHandler(this.useComboBoxCells_CheckedChanged);            
            this.dataGridView.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(CellCopyPaste);

            try {
                FirstColumnAsRowHeader = Settings.Default.UseFirstColumnAsRowHeader;
                useComboBoxCells.Checked = Settings.Default.UseComboboxCells;
                showAllColumns.Checked = Settings.Default.ShowAllColumns;
            } catch {
                // TODO: Should not need to swallow an exception.
            }

            // Double buffer the control to reduce flickering.
            dataGridView.DoubleBuffered(true);
        }

        #region Open/Fill with Data
        public void Open() {
#if DEBUG
            Console.WriteLine("Opening {0}", CurrentPackedFile.FullPath);
#endif
            string key = DBFile.Typename(CurrentPackedFile.FullPath);

            if (!DBTypeMap.Instance.IsSupported(key)) {
                ShowDBFileNotSupportedMessage("Sorry, this db file isn't supported yet.\r\n\r\nCurrently supported files:\r\n");
                if (Settings.Default.ShowDecodeToolOnError) {
                    var decoder = new DecodeTool.DecodeTool { TypeName = key, Bytes = CurrentPackedFile.Data };
                    decoder.ShowDialog();
                    if (!DBTypeMap.Instance.IsSupported(key)) {
                        return;
                    }
                } else {
                    return;
                }
            }
            try {
                EditedFile = PackedFileDbCodec.Decode(CurrentPackedFile);
            } catch {
                if (Settings.Default.ShowDecodeToolOnError) {
                    var decoder = new DecodeTool.DecodeTool { TypeName = key, Bytes = CurrentPackedFile.Data };
                    decoder.ShowDialog();
                    try {
                        EditedFile = PackedFileDbCodec.Decode(CurrentPackedFile);
                    } catch {
                        return;
                    }
                } else {
                    return;
                }
            }
            
            if (EditedFile == null) {
                return;
            }

            Codec = PackedFileDbCodec.FromFilename(CurrentPackedFile.FullPath);
            TypeInfo info = EditedFile.CurrentType;
   
            dataGridView.EndEdit();
            dataGridView.Columns.Clear();
            if (currentDataTable != null) {
                currentDataTable.DataSet.Clear();
                currentDataTable.Clear();
            }
            
            CreateDataTable();
            for(int i = 0; i < EditedFile.CurrentType.Fields.Count; i++) {
                dataGridView.Columns.Add(CreateColumn(i));
            }

            DataSet currentDataSet = new DataSet(info.Name + "_DataSet");
            currentDataSet.Tables.Add(currentDataTable);
            dataGridView.DataSource = new BindingSource(currentDataSet, info.Name + "_DataTable");

            FirstColumnAsRowHeader = Settings.Default.UseFirstColumnAsRowHeader;
            FillRowHeaders();
            addNewRowButton.Enabled = true;
            importButton.Enabled = true;
            exportButton.Enabled = true;

            dataGridView.Visible = true;
            unsupportedDBErrorTextBox.Visible = false;

            // cannot edit contained complex types
            foreach(FieldInfo f in EditedFile.CurrentType.Fields) {
                if (f is ListType) {
                    Console.WriteLine("cannot edit this");
                    ReadOnly = true;
                    break;
                }
            }
        }
        void CreateDataTable() {
            TypeInfo info = EditedFile.CurrentType;
            currentDataTable = new DataTable(info.Name + "_DataTable");
            //currentDataTable.Locale = CultureInfo.InvariantCulture;

            for (int columnIndex = 0; columnIndex < info.Fields.Count; columnIndex++) {
                string columnName = info.Fields[columnIndex].Name; // columnIndex.ToString();
                var column = new DataColumn(columnName) {
                    DataType =
                        info.Fields[columnIndex].TypeCode == TypeCode.Empty
                            ? Type.GetType("System.String")
                            : Type.GetType("System." + info.Fields[columnIndex].TypeCode)
                };
                currentDataTable.Columns.Add(column);
            }

            currentDataTable.ColumnChanged += ColumnChanged;
            currentDataTable.RowDeleting += RowDeleted;

#if DEBUG
            Console.WriteLine("Filling data rows ({0} entries)", EditedFile.Entries.Count);
#endif
            for (int rowIndex = 0; rowIndex < EditedFile.Entries.Count; rowIndex++) {
                DataRow row = currentDataTable.NewRow();
                for (int i = 0; i < currentDataTable.Columns.Count; i++) {
                    row[i] = EditedFile.Entries[rowIndex][i].Value;
                }
                currentDataTable.Rows.Add(row);
            }
        }
        void FillRowHeaders(int start = 0, int offset = 0) {
            for (int i = start; i < dataGridView.Rows.Count; i++) {
                this.dataGridView.Rows[i].HeaderCell.Value = (i + offset + 1).ToString();
            }
        }
        private void ShowDBFileNotSupportedMessage(string message) {
            dataGridView.Visible = false;
            unsupportedDBErrorTextBox.Visible = true;
            unsupportedDBErrorTextBox.Text = string.Format("{0}{1}", message, string.Join("\r\n", DBTypeMap.Instance.DBFileTypes));
            addNewRowButton.Enabled = false;
            importButton.Enabled = false;
            exportButton.Enabled = false;
        }
        DataGridViewColumn CreateColumn(int columnIndex) {
            FieldInfo fieldInfo = EditedFile.CurrentType.Fields[columnIndex];
            DataGridViewColumn column = null;
            if (Settings.Default.UseComboboxCells) {
                try {
                    ICollection<string> items = DBReferenceMap.Instance.ResolveReference(fieldInfo.ForeignReference);

                    if (items != null) {
                        column = new DataGridViewComboBoxColumn();
                        var cbItems = (column as DataGridViewComboBoxColumn).Items;
                        cbItems.Add(string.Empty);

                        foreach (string item in items) {
                            cbItems.Add(item);
                        }
                    }
                } catch {
                    MessageBox.Show("A table reference could not be resolved.\nDisabling combo cells.");
                    Settings.Default.UseComboboxCells = false;
                    useComboBoxCells.CheckedChanged -= useComboBoxCells_CheckedChanged;
                    useComboBoxCells.CheckState = CheckState.Unchecked;
                    useComboBoxCells.CheckedChanged += useComboBoxCells_CheckedChanged;
                }
            }

            if (column == null) {
                column = new DataGridViewAutoFilterTextBoxColumn();
            }
            column.Name = fieldInfo.Name;
            column.SortMode = DataGridViewColumnSortMode.Programmatic;
            column.HeaderText = fieldInfo.Name + (Settings.Default.IsColumnIgnored(CurrentPackedFile.FullPath, fieldInfo.Name) ? "*" : "");
            column.Tag = fieldInfo;
            column.DataPropertyName = fieldInfo.Name; //columnIndex.ToString();
            column.Visible = !Settings.Default.IsColumnIgnored(CurrentPackedFile.FullPath, fieldInfo.Name);
            column.DefaultCellStyle.BackColor = ReadOnly ? Color.LightGray : Color.White;
            
            if (column.Visible) {
                int visibleColumnCount = Math.Min(EditedFile.CurrentType.Fields.Count, 10);
                int columnWidth = Width / visibleColumnCount;
                column.Width = columnWidth;
            }
            return column;
        }
        #endregion
  
        private void CellErrorHandler(object sender, DataGridViewDataErrorEventArgs args) {
            if (Settings.Default.UseComboboxCells) {
                MessageBox.Show(string.Format("A table reference could not be resolved; disabling combo box cells\nColumn={0}, Value '{1}'", 
                    dataGridView.Columns[args.ColumnIndex].HeaderText,
                    dataGridView[args.ColumnIndex, args.RowIndex].Value));
                Settings.Default.UseComboboxCells = false;
            }
            args.ThrowException = true;
            useComboBoxCells.Checked = false;
        }
  
        #region Adding Rows
        private void cloneCurrentRow_Click(object sender, EventArgs e) 
        {
            int num = (dataGridView.SelectedRows.Count == 1) 
                ? dataGridView.SelectedRows[0].Index 
                    : dataGridView.SelectedCells[0].RowIndex;
            List<FieldInstance> newEntry = EditedFile.GetNewEntry();

            createRow(newEntry, num);
        }
        private void addNewRowButton_Click(object sender, EventArgs e) 
        {
            var newEntry = EditedFile.GetNewEntry();
            int insertAtColumn = dataGridView.Rows.Count;
            if (dataGridView.CurrentCell != null) 
            {
                insertAtColumn = dataGridView.CurrentCell.RowIndex + 1;
            }
            createRow(newEntry, insertAtColumn);
        }
        private void createRow(List<FieldInstance> newEntry, int index) 
        {
            var row = currentDataTable.NewRow();
            for (int i = 0; i < currentDataTable.Columns.Count; i++) 
            {
                foreach(FieldInstance instance in newEntry) {
                    if (instance.Name.Equals(currentDataTable.Columns[i].ColumnName)) {
                        row[i] = Convert.ChangeType(instance.Value, currentDataTable.Columns[i].DataType);
                        break;
                    }
                }
            }
            
            EditedFile.Entries.Insert(index, new DBRow(EditedFile.CurrentType, newEntry));
            currentDataTable.Rows.InsertAt(row, index);
            FillRowHeaders();
            dataGridView.FirstDisplayedScrollingRowIndex = index;

            CurrentPackedFile.Data = Codec.Encode(EditedFile);
        }
        #endregion
  
        #region Display Options
        private void useFirstColumnAsRowHeader_CheckedChanged(object sender, EventArgs e) 
        {
            FirstColumnAsRowHeader = useFirstColumnAsRowHeader.Checked;
        }
        bool FirstColumnAsRowHeader {
            set {
                if (dataGridView.Columns.Count > 0) {
                    if (dataGridView.ColumnCount > 1) {
                        dataGridView.Columns[1].Frozen = value;
                    } else {
                        dataGridView.Columns[0].Frozen = value;
                    }
                    dataGridView.TopLeftHeaderCell.Value = value ? EditedFile.Entries[0][0].Info.Name : "";
                }

                dataGridView.RowHeadersVisible = !value;
                Settings.Default.UseFirstColumnAsRowHeader = useFirstColumnAsRowHeader.Checked;
            }
        }

        private void useComboBoxCells_CheckedChanged(object sender, EventArgs e) 
        {
            Settings.Default.UseComboboxCells = useComboBoxCells.Checked;
            if (CurrentPackedFile != null) 
            {
                // rebuild table
                Open();
            }
        }
        private void ApplyColumnVisibility() 
        {
            if (CurrentPackedFile == null)
                return;

            for (int i = 0; i < dataGridView.ColumnCount; i++)
            {
                DataGridViewColumn column = dataGridView.Columns[i];
                if (column != null && column.Tag != null) 
                {
                    var info = ((FieldInfo)column.Tag);
                    bool show = !Settings.Default.IsColumnIgnored(CurrentPackedFile.FullPath, info.Name);
                    column.Visible = (Settings.Default.ShowAllColumns || show);
                    column.HeaderText = info.Name + (show ? "" : "*");
                } 
                else 
                {
                    // TODO: Why would we be writing to the console?
                    Console.WriteLine("no column?");
                }
            }
        }
        private void showAllColumns_CheckedChanged(object sender, EventArgs e) {
            Settings.Default.ShowAllColumns = showAllColumns.Checked;
            if (CurrentPackedFile != null) {
                ApplyColumnVisibility();
            }
        }
        #endregion

        #region Copy/Paste of Several Cells
        //private void copyToolStripButton_Click(object sender, EventArgs e) {
        //    copyEvent();
        //}
        //private void pasteToolStripButton_Click(object sender, EventArgs e) {
        //    pasteEvent();
        //}

        private void DBFileEditorControl_Enter(object sender, EventArgs e) {
            pasteToolStripButton.Enabled = copiedRows.Count != 0;
        }
        private void cloneRowsButton_Click(object sender, EventArgs e) {
            DataGridViewSelectedRowCollection selectedRows = dataGridView.SelectedRows;
            if (selectedRows.Count != 0) {
                foreach (DataGridViewRow row in selectedRows) {
                    DBRow toCopy = EditedFile.Entries[row.Index];
                    var copy = new DBRow(EditedFile.CurrentType, toCopy);
                    createRow(copy, row.Index);
                }
            } else {
                MessageBox.Show("Please select the Row(s) to Clone!", "Please select Rows", 
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void copyToolStripButton_Click(object sender, EventArgs e) {
            copyPaste.CopyEvent();
        }

        private void pasteToolStripButton_Click(object sender, EventArgs e) {
            copyPaste.PasteEvent();
        }
        #endregion
 
        #region Copy/Paste within GridView Cell
        void CellCopyPaste(object s, DataGridViewEditingControlShowingEventArgs args) {
            Console.WriteLine("editing control showing: {0}", args.Control);
            args.Control.KeyDown -= CopyPasteString;
            args.Control.KeyDown += CopyPasteString;
        }
        void CopyPasteString(object s, KeyEventArgs args) {
            UserControl editor = s as UserControl;
            Console.WriteLine("key in {0}", s);
            if (args.Control && editor != null) {
                switch(args.KeyCode) {
                case Keys.C:
                    Console.WriteLine("copying {0}", editor.Text);
                    Clipboard.SetText(editor.Text);
                    args.Handled = true;
                    break;
                case Keys.V:
                    if (ReadOnly) {
                        break;
                    }
                    Console.WriteLine("pasting {0}", Clipboard.GetText());
                    editor.Text = Clipboard.GetText();
                    args.Handled = true;
                    break;
                }
            }
        }
        #endregion

        #region Data Table change handlers
        private void ColumnChanged(object sender, DataColumnChangeEventArgs e) 
        {
            if ((dataGridView.DataSource != null) && (e.Row.RowState != DataRowState.Detached)) {
                object proposedValue = e.ProposedValue;
                string fieldName = e.Column.ColumnName;
                List<FieldInstance> list = EditedFile.Entries[currentDataTable.Rows.IndexOf(e.Row)];
                FieldInstance instance = null;
                foreach(FieldInstance candidate in list) {
                    if (candidate.Info.Name.Equals(fieldName)) {
                        instance = candidate;
                        break;
                    }
                }
                if (instance == null) {
                    return;
                }
                string str = (proposedValue == null) ? "" : proposedValue.ToString();
                instance.Value = str;
#if DEBUG
                Console.WriteLine("Data column changed");
#endif
                CurrentPackedFile.Data = Codec.Encode(EditedFile);
            }
        }

        private void RowDeleted(object sender, DataRowChangeEventArgs e) {
            if (e.Action != DataRowAction.Delete) {
                throw new InvalidDataException("wtf?");
            }
            int removeIndex = currentDataTable.Rows.IndexOf(e.Row);
            EditedFile.Entries.RemoveAt(removeIndex);
            CurrentPackedFile.Data = Codec.Encode(EditedFile);
            FillRowHeaders(removeIndex, -1);
        }
        #endregion

        #region File Import/Export
        private void ExportData(object sender, EventArgs e) {
            string extractTo = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentModDirectory : null;
            if (extractTo == null) {
                DirectoryDialog dialog = new DirectoryDialog {
                    Description = "Please point to folder to extract to",
                    SelectedPath = Settings.Default.LastPackDirectory
                };
                extractTo = dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
            }
            if (!string.IsNullOrEmpty(extractTo)) {
                List<PackedFile> files = new List<PackedFile>();
                files.Add(CurrentPackedFile);
                FileExtractor extractor = new FileExtractor(null, null, extractTo) { Preprocessor = new TsvExtractionPreprocessor() };
                extractor.ExtractFiles(files);
                MessageBox.Show(string.Format("File exported to TSV."));
            }
        }

        private void ImportData(object sender, EventArgs e) {
#if DEBUG
            Console.WriteLine("Importing into file with {0} entries", EditedFile.Entries.Count);
#endif
            OpenFileDialog openDBFileDialog = new OpenFileDialog {
                InitialDirectory = Settings.Default.ImportExportDirectory,
                FileName = Settings.Default.TsvFile(EditedFile.CurrentType.Name)
            };

            bool tryAgain = false;
            if (openDBFileDialog.ShowDialog() == DialogResult.OK) {
                Settings.Default.ImportExportDirectory = Path.GetDirectoryName(openDBFileDialog.FileName);
                do {
                    try {
                        DialogResult question = MessageBox.Show("Replace the current data?", "Replace data?", 
                                                                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (question == DialogResult.Cancel) {
                            return;
                        }
                        DBFile imported;
                        try {
                            using (var stream = new MemoryStream(File.ReadAllBytes(openDBFileDialog.FileName))) {
                                imported = new TextDbCodec().Decode(stream);
                            }
                            if (question == DialogResult.Yes) {
                                EditedFile = imported;
                            } else {
                                EditedFile.Import(imported);
                            }
                        } catch (DBFileNotSupportedException exception) {
                            ShowDBFileNotSupportedMessage(exception.Message);
                        }

                        CurrentPackedFile.Data = (Codec.Encode(EditedFile));
                        Open();
                    } catch (Exception ex) {
                        tryAgain = (MessageBox.Show(string.Format("Import failed: {0}", ex.Message),
                            "Import failed",
                            MessageBoxButtons.RetryCancel)
                            == DialogResult.Retry);
                    }
                } while (tryAgain);
            }
        }
        #endregion
  
        #region Type Editing
        private void promptHeaderDescription(DataGridViewColumn newColumn)
        {
            var info = (FieldInfo)newColumn.Tag;
            var box = new InputBox { Text = "Enter new description", Input = info.Name };
            if (box.ShowDialog() == DialogResult.OK) 
            {
                info.Name = box.Input;
                newColumn.HeaderText = info.Name;

                if (!TableColumnChanged) 
                {
                    TableColumnChanged = true;
                    MessageBox.Show("Don't forget to save your changes (DB Definitions->Save to Directory)");
                }
            }
        }
        private void setReferenceTarget(DataGridViewColumn column) {
            var info = (FieldInfo)column.Tag;
            string tableName = EditedFile.CurrentType.Name;
            if (!tableName.EndsWith("_tables")) {
                tableName += "_tables";
            }
            referenceTarget = string.Format("{0}.{1}", tableName, info.Name);
        }
        private void applyReferenceTarget(DataGridViewColumn column) {
            var info = (FieldInfo)column.Tag;
            if (referenceTarget != null) {
                info.ForeignReference = referenceTarget;
                info.Name = string.Format("{0}Ref", referenceTarget.Substring(referenceTarget.LastIndexOf('.')+1));
            }
            // rebuild table to see combo boxes if applicable
            Open();
        }
        private void CreateColumnHeaderPopup(object sender, DataGridViewCellMouseEventArgs e) {
            DataGridViewColumn newColumn = dataGridView.Columns[e.ColumnIndex];

            if (e.Button == MouseButtons.Right) {
                var menu = new ContextMenu();
                var item = new MenuItem("Change Column Description", delegate {
                    promptHeaderDescription(newColumn);
                });
                menu.MenuItems.Add(item);
                var info = newColumn.Tag as FieldInfo;

                // edit number items
                if (info != null && (info.TypeCode == TypeCode.Int16 || info.TypeCode == TypeCode.Int32 ||
                                    info.TypeCode == TypeCode.UInt32 || info.TypeCode == TypeCode.Single)) {
                    item = new MenuItem("Edit...");
                    item.MenuItems.Add(new MenuItem("Add value to All", delegate {
                        AddToAll(e.ColumnIndex);
                    }));
                    item.MenuItems.Add(new MenuItem("Renumber...", delegate {
                        RenumberFrom(e.ColumnIndex);
                    }));
                    item.MenuItems.Add(new MenuItem("Apply Expression", delegate {
                        ApplyValueExpression(e.ColumnIndex);
                    }));
                    menu.MenuItems.Add(item);
                }

                item = new MenuItem("Set as Reference Target", delegate {
                    setReferenceTarget(newColumn);
                });
                menu.MenuItems.Add(item);

                if (referenceTarget != null) {
                    item = new MenuItem(string.Format("Apply Reference Target ({0})", referenceTarget), delegate {
                        applyReferenceTarget(newColumn);
                    });
                    menu.MenuItems.Add(item);
                }

                if (info != null && info.ForeignReference != "") {
                    item = new MenuItem(string.Format("Clear Reference Target ({0})", info.ForeignReference), delegate {
                        info.ForeignReference = "";
                        // rebuild table to remove combo boxes if applicable
                        Open();
                    });
                    menu.MenuItems.Add(item);
                }

                string ignoreField = ((FieldInfo)newColumn.Tag).Name;
                bool ignored = Settings.Default.IsColumnIgnored(CurrentPackedFile.FullPath, ignoreField);
                string itemText = ignored ? "Show Column" : "Hide Column";

                item = new MenuItem(itemText, delegate {
                    if (ignored) {
                        Settings.Default.UnignoreColumn(CurrentPackedFile.FullPath, ignoreField);
                    } else {
                        Settings.Default.IgnoreColumn(CurrentPackedFile.FullPath, ignoreField);
                    }
                    Settings.Default.Save();
                    ApplyColumnVisibility();
                });
                menu.MenuItems.Add(item);

                item = new MenuItem("Edit Visible List", delegate {
                    EditVisibleLists();
                });
                menu.MenuItems.Add(item);

                item = new MenuItem("Clear Hide list for this table", delegate {
                    Settings.Default.ResetIgnores(CurrentPackedFile.FullPath);
                    Settings.Default.Save();
                    ApplyColumnVisibility();
                });

                menu.MenuItems.Add(item);
                item = new MenuItem("Clear Hide list for all tables", delegate {
                    Settings.Default.IgnoreColumns = "";
                    Settings.Default.Save();
                    ApplyColumnVisibility();
                });
                menu.MenuItems.Add(item);
                menu.Show(dataGridView, e.Location);
                return;
            }
        }
        private void EditVisibleLists() {
            List<string> ignored = new List<string>(Settings.Default.IgnoredColumns(CurrentPackedFile.FullPath));
            List<string> all = new List<string>();
            EditedFile.CurrentType.Fields.ForEach(f => all.Add(f.Name));
            List<string> shown = new List<string>(all);
            shown.RemoveAll(e => ignored.Contains(e));
            ListEditor editor = new ListEditor {
                OriginalOrder = all,
                LeftLabel = "Shown Columns",
                RightLabel = "Hidden Columns",
                LeftList = shown,
                RightList = ignored
            };
            if (editor.ShowDialog() == DialogResult.OK) {
                Settings.Default.ResetIgnores(CurrentPackedFile.FullPath);
                editor.RightList.ForEach(i => Settings.Default.IgnoreColumn(CurrentPackedFile.FullPath, i));
                ApplyColumnVisibility();
            }
        }
        #endregion
  
        #region Bulk Data Editing
        private void AddToAll(int columnIndex) {
            InputBox box = new InputBox { Text = "Enter number to add", Input = "0" };
            if (box.ShowDialog() == DialogResult.OK) {
                try {
                    float addValue = float.Parse(box.Input);
                    for (int i = 0; i < dataGridView.RowCount; i++) {
                        DataRow row = currentDataTable.Rows[i];
                        float newValue = float.Parse(row[columnIndex].ToString()) + addValue;
                        row[columnIndex] = newValue;
                    }
                } catch (Exception ex) {
                    MessageBox.Show(string.Format("Could not apply value: {0}", ex.Message), "You fail!");
                }
            }
        }
        private void RenumberFrom(int columnIndex) {
            InputBox box = new InputBox { Text = "Enter number to start from", Input = "1" };
            if (box.ShowDialog() == DialogResult.OK) {
                try {
                    int setValue = int.Parse(box.Input);
                    for (int i = 0; i < dataGridView.RowCount; i++) {
                        DataRow row = currentDataTable.Rows[i];
                        int newValue = setValue + i;
                        row[columnIndex] = newValue;
                    }
                } catch (Exception ex) {
                    MessageBox.Show(string.Format("Could not apply values: {0}", ex.Message), "You fail!");
                }
            }
        }
        private void ApplyValueExpression(int index) {
            try {
                string fieldName = EditedFile.CurrentType.Fields[index].Name;
                if (index != -1) {
                    InputBox input = new InputBox {
                        Text = "Enter expression (use \"x\" for current field value)",
                        Input = "expression"
                    };
                    if (input.ShowDialog() == DialogResult.OK) {
                        for (int rowIndex = 0; rowIndex < currentDataTable.Rows.Count; rowIndex++) {
                            string fieldValue = Convert.ToString(currentDataTable.Rows[rowIndex][fieldName], CultureInfo.InvariantCulture);
                            string expression = input.Input.Replace("x", fieldValue);
                            string result = currentDataTable.Compute(expression, "").ToString();
                            currentDataTable.Rows[rowIndex][fieldName] = result;
                        }
                    }
                }
            } catch (Exception e) {
                MessageBox.Show(string.Format("Failed to apply expression: {0}", e));
            }
        }
        #endregion
    }
}
namespace System.Data
{
    public static class Extensions
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
}
