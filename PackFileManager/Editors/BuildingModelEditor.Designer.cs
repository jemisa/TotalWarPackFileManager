namespace PackFileManager {
    partial class BuildingModelEditor {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.modelSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.modelGridView = new System.Windows.Forms.DataGridView();
            this.entries = new System.Windows.Forms.GroupBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.entryGridView = new System.Windows.Forms.DataGridView();
            this.entrySource = new System.Windows.Forms.BindingSource(this.components);
            this.coordinatesSource = new System.Windows.Forms.BindingSource(this.components);
            this.angles1Box = new System.Windows.Forms.GroupBox();
            this.coordinatesGridView = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.texturePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unknown = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.entryName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.entryUnknown = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.xCoordinate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yCoordinate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zCoordinate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.modelSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modelGridView)).BeginInit();
            this.entries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.entryGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.entrySource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.coordinatesSource)).BeginInit();
            this.angles1Box.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.coordinatesGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.modelGridView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.entries);
            this.splitContainer1.Size = new System.Drawing.Size(948, 640);
            this.splitContainer1.SplitterDistance = 299;
            this.splitContainer1.TabIndex = 0;
            // 
            // modelGridView
            // 
            this.modelGridView.AutoGenerateColumns = false;
            this.modelGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.modelGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.modelGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.texturePath,
            this.unknown});
            this.modelGridView.DataSource = this.modelSource;
            this.modelGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modelGridView.Location = new System.Drawing.Point(0, 0);
            this.modelGridView.MultiSelect = false;
            this.modelGridView.Name = "modelGridView";
            this.modelGridView.Size = new System.Drawing.Size(948, 299);
            this.modelGridView.TabIndex = 10;
            // 
            // entries
            // 
            this.entries.Controls.Add(this.splitContainer2);
            this.entries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entries.Location = new System.Drawing.Point(0, 0);
            this.entries.Name = "entries";
            this.entries.Size = new System.Drawing.Size(948, 337);
            this.entries.TabIndex = 0;
            this.entries.TabStop = false;
            this.entries.Text = "Entries";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 16);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.entryGridView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.angles1Box);
            this.splitContainer2.Size = new System.Drawing.Size(942, 318);
            this.splitContainer2.SplitterDistance = 532;
            this.splitContainer2.TabIndex = 0;
            // 
            // entryGridView
            // 
            this.entryGridView.AutoGenerateColumns = false;
            this.entryGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.entryGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.entryGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.entryName,
            this.entryUnknown});
            this.entryGridView.DataSource = this.entrySource;
            this.entryGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entryGridView.Location = new System.Drawing.Point(0, 0);
            this.entryGridView.MultiSelect = false;
            this.entryGridView.Name = "entryGridView";
            this.entryGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.entryGridView.Size = new System.Drawing.Size(532, 318);
            this.entryGridView.TabIndex = 0;
            // 
            // angles1Box
            // 
            this.angles1Box.Controls.Add(this.coordinatesGridView);
            this.angles1Box.Dock = System.Windows.Forms.DockStyle.Fill;
            this.angles1Box.Location = new System.Drawing.Point(0, 0);
            this.angles1Box.Name = "angles1Box";
            this.angles1Box.Size = new System.Drawing.Size(406, 318);
            this.angles1Box.TabIndex = 1;
            this.angles1Box.TabStop = false;
            this.angles1Box.Text = "Coordinates";
            // 
            // coordinatesGridView
            // 
            this.coordinatesGridView.AllowUserToAddRows = false;
            this.coordinatesGridView.AllowUserToDeleteRows = false;
            this.coordinatesGridView.AutoGenerateColumns = false;
            this.coordinatesGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.coordinatesGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.xCoordinate,
            this.yCoordinate,
            this.zCoordinate});
            this.coordinatesGridView.DataSource = this.coordinatesSource;
            this.coordinatesGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coordinatesGridView.Location = new System.Drawing.Point(3, 16);
            this.coordinatesGridView.Name = "coordinatesGridView";
            this.coordinatesGridView.Size = new System.Drawing.Size(400, 299);
            this.coordinatesGridView.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.HeaderText = "Name";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.DataPropertyName = "TexturePath";
            this.dataGridViewTextBoxColumn2.HeaderText = "Texture Path";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Unknown";
            this.dataGridViewTextBoxColumn3.HeaderText = "Unknown";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Width = 78;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn4.HeaderText = "Name";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Unknown";
            this.dataGridViewTextBoxColumn5.HeaderText = "Unknown";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Width = 78;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn6.HeaderText = "X";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn7.HeaderText = "Y";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn8
            // 
            this.dataGridViewTextBoxColumn8.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn8.HeaderText = "Z";
            this.dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            this.dataGridViewTextBoxColumn8.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // name
            // 
            this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.name.DataPropertyName = "Name";
            this.name.HeaderText = "Name";
            this.name.Name = "name";
            // 
            // texturePath
            // 
            this.texturePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.texturePath.DataPropertyName = "TexturePath";
            this.texturePath.HeaderText = "Texture Path";
            this.texturePath.Name = "texturePath";
            // 
            // unknown
            // 
            this.unknown.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.unknown.DataPropertyName = "Unknown";
            this.unknown.HeaderText = "Unknown";
            this.unknown.Name = "unknown";
            this.unknown.Width = 78;
            // 
            // entryName
            // 
            this.entryName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.entryName.DataPropertyName = "Name";
            this.entryName.HeaderText = "Name";
            this.entryName.Name = "entryName";
            // 
            // entryUnknown
            // 
            this.entryUnknown.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.entryUnknown.DataPropertyName = "Unknown";
            this.entryUnknown.HeaderText = "Unknown";
            this.entryUnknown.Name = "entryUnknown";
            this.entryUnknown.Width = 78;
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn12.DataPropertyName = "XCoordinate";
            this.dataGridViewTextBoxColumn12.HeaderText = "X";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn13.DataPropertyName = "YCoordinate";
            this.dataGridViewTextBoxColumn13.HeaderText = "Y";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            // 
            // dataGridViewTextBoxColumn14
            // 
            this.dataGridViewTextBoxColumn14.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn14.DataPropertyName = "ZCoordinate";
            this.dataGridViewTextBoxColumn14.HeaderText = "Z";
            this.dataGridViewTextBoxColumn14.Name = "dataGridViewTextBoxColumn14";
            // 
            // xCoordinate
            // 
            this.xCoordinate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.xCoordinate.DataPropertyName = "XCoordinate";
            this.xCoordinate.HeaderText = "X";
            this.xCoordinate.Name = "xCoordinate";
            this.xCoordinate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // yCoordinate
            // 
            this.yCoordinate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.yCoordinate.DataPropertyName = "YCoordinate";
            this.yCoordinate.HeaderText = "Y";
            this.yCoordinate.Name = "yCoordinate";
            this.yCoordinate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // zCoordinate
            // 
            this.zCoordinate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.zCoordinate.DataPropertyName = "ZCoordinate";
            this.zCoordinate.HeaderText = "Z";
            this.zCoordinate.Name = "zCoordinate";
            this.zCoordinate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // BuildingModelEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "BuildingModelEditor";
            this.Size = new System.Drawing.Size(948, 640);
            ((System.ComponentModel.ISupportInitialize)(this.modelSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.modelGridView)).EndInit();
            this.entries.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.entryGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.entrySource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.coordinatesSource)).EndInit();
            this.angles1Box.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.coordinatesGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.BindingSource modelSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox entries;
        private System.Windows.Forms.BindingSource entrySource;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView modelGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn texturePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn unknown;
        private System.Windows.Forms.DataGridView entryGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn entryName;
        private System.Windows.Forms.DataGridViewTextBoxColumn entryUnknown;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.BindingSource coordinatesSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn14;
        private System.Windows.Forms.GroupBox angles1Box;
        private System.Windows.Forms.DataGridView coordinatesGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn xCoordinate;
        private System.Windows.Forms.DataGridViewTextBoxColumn yCoordinate;
        private System.Windows.Forms.DataGridViewTextBoxColumn zCoordinate;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
    }
}
