using Common;
using System.Windows.Forms;

namespace PackFileManager
{
    partial class DBFileEditorControl {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private ToolStripButton addNewRowButton;
        private CheckBox useFirstColumnAsRowHeader;
        private ToolStripButton copyToolStripButton;
        private DataGridViewExtended dataGridView;
        private ToolStripButton exportButton;
        private ToolStripButton importButton;
        private ToolStripButton pasteToolStripButton;
        private ToolStrip toolStrip;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private TextBox unsupportedDBErrorTextBox;
        private CheckBox useComboBoxCells;
        private CheckBox showAllColumns;

        protected override void Dispose(bool disposing) 
        {
            if (disposing && (components != null)) 
            {
                Utilities.DisposeHandlers(this);
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.addNewRowButton = new System.Windows.Forms.ToolStripButton();
            this.cloneRowsButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exportButton = new System.Windows.Forms.ToolStripButton();
            this.importButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.unsupportedDBErrorTextBox = new System.Windows.Forms.TextBox();
            this.useFirstColumnAsRowHeader = new System.Windows.Forms.CheckBox();
            this.showAllColumns = new System.Windows.Forms.CheckBox();
            this.useComboBoxCells = new System.Windows.Forms.CheckBox();
            this.dataGridView = new PackFileManager.DataGridViewExtended();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewRowButton,
            this.cloneRowsButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.toolStripSeparator1,
            this.exportButton,
            this.importButton,
            this.toolStripSeparator2});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(876, 25);
            this.toolStrip.TabIndex = 2;
            this.toolStrip.Text = "toolStrip";
            // 
            // addNewRowButton
            // 
            this.addNewRowButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.addNewRowButton.Enabled = false;
            this.addNewRowButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNewRowButton.Name = "addNewRowButton";
            this.addNewRowButton.Size = new System.Drawing.Size(59, 22);
            this.addNewRowButton.Text = "Add Row";
            this.addNewRowButton.ToolTipText = "Add New Row";
            this.addNewRowButton.Click += new System.EventHandler(this.addNewRowButton_Click);
            // 
            // cloneRowsButton
            // 
            this.cloneRowsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.cloneRowsButton.Enabled = false;
            this.cloneRowsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cloneRowsButton.Name = "cloneRowsButton";
            this.cloneRowsButton.Size = new System.Drawing.Size(81, 22);
            this.cloneRowsButton.Text = "Clone Row(s)";
            this.cloneRowsButton.ToolTipText = "Add New Row";
            this.cloneRowsButton.Click += new System.EventHandler(this.cloneRowsButton_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(39, 22);
            this.copyToolStripButton.Text = "&Copy";
            this.copyToolStripButton.ToolTipText = "Copy Current Row";
            this.copyToolStripButton.Click += new System.EventHandler(this.copyToolStripButton_Click);
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.pasteToolStripButton.Enabled = false;
            this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Size = new System.Drawing.Size(39, 22);
            this.pasteToolStripButton.Text = "&Paste";
            this.pasteToolStripButton.ToolTipText = "Paste Row from Clipboard";
            this.pasteToolStripButton.Click += new System.EventHandler(this.pasteToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // exportButton
            // 
            this.exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exportButton.Enabled = false;
            this.exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(67, 22);
            this.exportButton.Text = "Export TSV";
            this.exportButton.ToolTipText = "Export to tab-separated values";
            this.exportButton.Click += new System.EventHandler(this.ExportData);
            // 
            // importButton
            // 
            this.importButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.importButton.Enabled = false;
            this.importButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(70, 22);
            this.importButton.Text = "Import TSV";
            this.importButton.ToolTipText = "Import from tab-separated values";
            this.importButton.Click += new System.EventHandler(this.ImportData);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // unsupportedDBErrorTextBox
            // 
            this.unsupportedDBErrorTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.unsupportedDBErrorTextBox.Location = new System.Drawing.Point(0, 28);
            this.unsupportedDBErrorTextBox.Multiline = true;
            this.unsupportedDBErrorTextBox.Name = "unsupportedDBErrorTextBox";
            this.unsupportedDBErrorTextBox.ReadOnly = true;
            this.unsupportedDBErrorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.unsupportedDBErrorTextBox.Size = new System.Drawing.Size(876, 641);
            this.unsupportedDBErrorTextBox.TabIndex = 3;
            this.unsupportedDBErrorTextBox.Visible = false;
            // 
            // useFirstColumnAsRowHeader
            // 
            this.useFirstColumnAsRowHeader.AutoSize = true;
            this.useFirstColumnAsRowHeader.Location = new System.Drawing.Point(422, 4);
            this.useFirstColumnAsRowHeader.Name = "useFirstColumnAsRowHeader";
            this.useFirstColumnAsRowHeader.Size = new System.Drawing.Size(183, 17);
            this.useFirstColumnAsRowHeader.TabIndex = 4;
            this.useFirstColumnAsRowHeader.Text = "Use First Column As Row Header";
            this.useFirstColumnAsRowHeader.UseVisualStyleBackColor = true;
            this.useFirstColumnAsRowHeader.CheckedChanged += new System.EventHandler(this.useFirstColumnAsRowHeader_CheckedChanged);
            // 
            // showAllColumns
            // 
            this.showAllColumns.AutoSize = true;
            this.showAllColumns.Location = new System.Drawing.Point(741, 4);
            this.showAllColumns.Name = "showAllColumns";
            this.showAllColumns.Size = new System.Drawing.Size(108, 17);
            this.showAllColumns.TabIndex = 6;
            this.showAllColumns.Text = "Show all columns";
            this.showAllColumns.UseVisualStyleBackColor = true;
            this.showAllColumns.CheckedChanged += new System.EventHandler(this.showAllColumns_CheckedChanged);
            // 
            // useComboBoxCells
            // 
            this.useComboBoxCells.AutoSize = true;
            this.useComboBoxCells.Checked = true;
            this.useComboBoxCells.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useComboBoxCells.Location = new System.Drawing.Point(611, 4);
            this.useComboBoxCells.Name = "useComboBoxCells";
            this.useComboBoxCells.Size = new System.Drawing.Size(124, 17);
            this.useComboBoxCells.TabIndex = 5;
            this.useComboBoxCells.Text = "Use ComboBox Cells";
            this.useComboBoxCells.UseVisualStyleBackColor = true;
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(0, 28);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersWidth = 100;
            this.dataGridView.ShowCellErrors = false;
            this.dataGridView.ShowEditingIcon = false;
            this.dataGridView.ShowRowErrors = false;
            this.dataGridView.Size = new System.Drawing.Size(876, 641);
            this.dataGridView.TabIndex = 1;
            this.dataGridView.VirtualMode = true;
            // 
            // DBFileEditorControl
            // 
            this.Controls.Add(this.showAllColumns);
            this.Controls.Add(this.useComboBoxCells);
            this.Controls.Add(this.useFirstColumnAsRowHeader);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.unsupportedDBErrorTextBox);
            this.Name = "DBFileEditorControl";
            this.Size = new System.Drawing.Size(876, 669);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }
}

