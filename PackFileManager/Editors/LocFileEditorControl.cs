using Common;
using Filetypes;
using DataGridViewAutoFilter;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using PackFileManager.Properties;
using Filetypes.Codecs;

namespace PackFileManager
{
    public class LocFileEditorControl : PackedFileEditor<LocFile>
    {
        private ToolStripButton addNewRowButton;
        private BindingSource bindingSource;
        private ToolStripButton cloneCurrentRow;
        private string[] columnHeaders = new string[] { "Tag", "Localised String", "Unknown" };
        private string[] columnNames = new string[] { "tag", "localised", "tooltip" };
        private int[] columnTypes;
        private int[] columnWidths;
        private IContainer components;
        private DataTable currentDataTable;
        private DataGridView dataGridView;
        private ToolStripButton deleteCurrentRow;
        private ToolStripButton exportButton;
        private ToolStripButton importButton;
        public OpenFileDialog openLocFileDialog;
        private ToolStrip toolStrip;
        private ToolStripSeparator toolStripSeparator1;

        public LocFileEditorControl() : base(LocCodec.Instance) {
            int[] numArray = new int[3];
            numArray [2] = 1;
            this.columnTypes = numArray;
            this.columnWidths = new int[] { 200, 400, 20 };
            this.components = null;
            this.InitializeComponent ();
            for (int i = 0; i < this.columnNames.Length; i++) {
                DataGridViewColumn column;
                int num2 = this.columnTypes [i];
                if (num2 == 1) {
                    column = new DataGridViewCheckBoxColumn ();
                } else {
                    column = new DataGridViewAutoFilterTextBoxColumn ();
                }
                column.DataPropertyName = this.columnNames [i];
                column.HeaderText = this.columnHeaders [i];
                column.Width = this.columnWidths [i];
                this.dataGridView.Columns.Add (column);
            }

            new GridViewCopyPaste(dataGridView);
        }

        static string[] EXTENSIONS = { ".loc" };
        public override bool CanEdit(PackedFile file) {
            return HasExtension(file, EXTENSIONS);
        }
        
        public override bool ReadOnly {
            get {
                return base.ReadOnly;
            }
            set {
                base.ReadOnly = value;
                foreach (DataGridViewRow row in dataGridView.Rows) {
                    foreach(DataGridViewCell cell in row.Cells) {
                        cell.ReadOnly = value;
                    }
                }
            }
        }
		
        private void addNewRowButton_Click(object sender, EventArgs e) {
			DataRow row = this.currentDataTable.NewRow ();
			row [0] = "tag";
			row [1] = "localised string";
			row [2] = false;
			this.currentDataTable.Rows.Add (row);
			this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
			this.dataGridView.Rows [this.dataGridView.Rows.Count - 2].Selected = true;
			this.DataChanged = true;
		}

        protected override void SetData() {
            if (DataChanged) {
                this.EditedFile.Entries.Clear();
                for (int i = 0; i < (this.dataGridView.Rows.Count - 1); i++) {
                    string tag = this.dataGridView.Rows[i].Cells[0].Value.ToString();
                    string localised = this.dataGridView.Rows[i].Cells[1].Value.ToString();
                    bool tooltip = Convert.ToBoolean(this.dataGridView.Rows[i].Cells[2].Value);
                    LocEntry newEntry = new LocEntry(tag, localised, tooltip);
                    this.EditedFile.Entries.Add(newEntry);
                }
            }
            base.SetData();
        }

        private void cloneCurrentRow_Click(object sender, EventArgs e)
        {
            DataRow row = this.currentDataTable.NewRow();
            int num = (this.dataGridView.SelectedRows.Count == 1) ? 
                this.dataGridView.SelectedRows[0].Index : this.dataGridView.SelectedCells[0].RowIndex;
            row[0] = this.dataGridView.Rows[num].Cells[0].Value;
            row[1] = this.dataGridView.Rows[num].Cells[1].Value;
            row[2] = this.dataGridView.Rows[num].Cells[2].Value;
            this.currentDataTable.Rows.Add(row);
            this.dataGridView.FirstDisplayedScrollingRowIndex = this.dataGridView.RowCount - 1;
            this.dataGridView.Rows[this.dataGridView.Rows.Count - 2].Selected = true;
            this.DataChanged = true;
        }

        private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            this.DataChanged = true;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            this.cloneCurrentRow.Enabled = this.cloneCurrentRow.Enabled = 
                (this.dataGridView.SelectedRows.Count == 1) || (this.dataGridView.SelectedCells.Count == 1);
            this.deleteCurrentRow.Enabled = this.cloneCurrentRow.Enabled = 
                (this.dataGridView.SelectedRows.Count == 1) || (this.dataGridView.SelectedCells.Count == 1);
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            this.DataChanged = true;
        }

        private void deleteCurrentRow_Click(object sender, EventArgs e)
        {
            int index = (this.dataGridView.SelectedRows.Count == 1) ? 
                this.dataGridView.SelectedRows[0].Index : this.dataGridView.SelectedCells[0].RowIndex;
            this.currentDataTable.Rows.RemoveAt(index);
            this.DataChanged = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                Utilities.DisposeHandlers(this);
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog {
                FileName = Settings.Default.TsvFile(this.EditedFile.Name)
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(dialog.FileName))
                {
                    this.EditedFile.Export(writer);
                }
            }
        }

        private DataTable getData()
        {
            int num;
            if (this.EditedFile == null)
            {
                return null;
            }
            DataTable table = new DataTable("locTable");
            for (num = 0; num < this.columnNames.Length; num++)
            {
                DataColumn column = new DataColumn(this.columnNames[num]);
                int num2 = this.columnTypes[num];
                if (num2 == 1)
                {
                    column.DataType = System.Type.GetType("System.Boolean");
                }
                else
                {
                    column.DataType = System.Type.GetType("System.String");
                }
                table.Columns.Add(column);
            }
            for (num = 0; num < this.EditedFile.NumEntries; num++)
            {
                DataRow row = table.NewRow();
                row[0] = this.EditedFile.Entries[num].Tag;
                row[1] = this.EditedFile.Entries[num].Localised;
                row[2] = this.EditedFile.Entries[num].Tooltip;
                table.Rows.Add(row);
            }
            return table;
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            this.openLocFileDialog.FileName = Settings.Default.TsvFile(this.EditedFile.Name);
            if (this.openLocFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader reader = new StreamReader(this.openLocFileDialog.FileName))
                {
                    LocFile imported = new LocFile();
                    imported.Import(reader);
                    EditedFile = imported;
                }
            }
            this.DataChanged = true;
        }

        public override LocFile EditedFile {
            get {
                return base.EditedFile;
            }
            set {
                base.EditedFile = value;
                this.bindingSource = new BindingSource();
                this.currentDataTable = this.getData();
                this.bindingSource.DataSource = this.currentDataTable;
                this.dataGridView.DataSource = this.bindingSource;
            }
        }


        private void InitializeComponent()
        {
            this.dataGridView = new DataGridView();
            this.toolStrip = new ToolStrip();
            this.addNewRowButton = new ToolStripButton();
            this.cloneCurrentRow = new ToolStripButton();
            this.deleteCurrentRow = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.exportButton = new ToolStripButton();
            this.importButton = new ToolStripButton();
            this.openLocFileDialog = new OpenFileDialog();
            ((ISupportInitialize)(this.dataGridView)).BeginInit();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(0, 28);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 100;
            this.dataGridView.ShowCellErrors = false;
            this.dataGridView.ShowEditingIcon = false;
            this.dataGridView.ShowRowErrors = false;
            this.dataGridView.Size = new System.Drawing.Size(876, 641);
            this.dataGridView.TabIndex = 1;
            this.dataGridView.VirtualMode = true;
            this.dataGridView.CellEndEdit += new DataGridViewCellEventHandler(this.dataGridView_CellEndEdit);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.UserDeletingRow += new DataGridViewRowCancelEventHandler(this.dataGridView_UserDeletingRow);
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new ToolStripItem[] {
            this.addNewRowButton,
            this.cloneCurrentRow,
            this.deleteCurrentRow,
            this.toolStripSeparator1,
            this.exportButton,
            this.importButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(876, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip";
            // 
            // addNewRowButton
            // 
            this.addNewRowButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.addNewRowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNewRowButton.Name = "addNewRowButton";
            this.addNewRowButton.Size = new System.Drawing.Size(86, 22);
            this.addNewRowButton.Text = "Add New Row";
            this.addNewRowButton.Click += new System.EventHandler(this.addNewRowButton_Click);
            // 
            // cloneCurrentRow
            // 
            this.cloneCurrentRow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.cloneCurrentRow.Enabled = false;
            this.cloneCurrentRow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cloneCurrentRow.Name = "cloneCurrentRow";
            this.cloneCurrentRow.Size = new System.Drawing.Size(111, 22);
            this.cloneCurrentRow.Text = "Clone Current Row";
            this.cloneCurrentRow.ToolTipText = "Clone Current Row";
            this.cloneCurrentRow.Click += new System.EventHandler(this.cloneCurrentRow_Click);
            // 
            // deleteCurrentRow
            // 
            this.deleteCurrentRow.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.deleteCurrentRow.Enabled = false;
            this.deleteCurrentRow.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteCurrentRow.Name = "deleteCurrentRow";
            this.deleteCurrentRow.Size = new System.Drawing.Size(113, 22);
            this.deleteCurrentRow.Text = "Delete Current Row";
            this.deleteCurrentRow.Click += new System.EventHandler(this.deleteCurrentRow_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // exportButton
            // 
            this.exportButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(67, 22);
            this.exportButton.Text = "Export TSV";
            this.exportButton.ToolTipText = "Export to tab-separated values";
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // importButton
            // 
            this.importButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.importButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(70, 22);
            this.importButton.Text = "Import TSV";
            this.importButton.ToolTipText = "Import from tab-separated values";
            this.importButton.Click += new System.EventHandler(this.importButton_Click);
            // 
            // openLocFileDialog
            // 
            this.openLocFileDialog.Filter = IOFunctions.TSV_FILTER;
            // 
            // LocFileEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.dataGridView);
            this.Name = "LocFileEditorControl";
            this.Size = new System.Drawing.Size(876, 669);
            ((ISupportInitialize)(this.dataGridView)).EndInit();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}

