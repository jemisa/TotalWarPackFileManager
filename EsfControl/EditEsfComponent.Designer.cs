namespace EsfControl {
    partial class EditEsfComponent {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.esfNodeTree = new System.Windows.Forms.TreeView();
            this.nodeValueGridView = new System.Windows.Forms.DataGridView();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Code = new System.Windows.Forms.DataGridViewTextBoxColumn();
            //((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            //((System.ComponentModel.ISupportInitialize)(this.nodeValueGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.esfNodeTree);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.nodeValueGridView);
            this.splitContainer1.Size = new System.Drawing.Size(465, 371);
            this.splitContainer1.SplitterDistance = 154;
            this.splitContainer1.TabIndex = 1;
            // 
            // esfNodeTree
            // 
            this.esfNodeTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.esfNodeTree.Location = new System.Drawing.Point(0, 0);
            this.esfNodeTree.Name = "esfNodeTree";
            this.esfNodeTree.Size = new System.Drawing.Size(154, 371);
            this.esfNodeTree.TabIndex = 0;
            // 
            // nodeValueGridView
            // 
            this.nodeValueGridView.AllowUserToAddRows = false;
            this.nodeValueGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nodeValueGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.nodeValueGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.nodeValueGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.nodeValueGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.valueColumn,
            this.typeColumn,
            this.Code});
            this.nodeValueGridView.Location = new System.Drawing.Point(3, 3);
            this.nodeValueGridView.Name = "nodeValueGridView";
            this.nodeValueGridView.RowHeadersVisible = false;
            this.nodeValueGridView.Size = new System.Drawing.Size(301, 341);
            this.nodeValueGridView.TabIndex = 0;
            // 
            // valueColumn
            // 
            this.valueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.valueColumn.HeaderText = "Value";
            this.valueColumn.Name = "valueColumn";
            // 
            // typeColumn
            // 
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.ReadOnly = true;
            this.typeColumn.Width = 56;
            // 
            // Code
            // 
            this.Code.HeaderText = "TypeCode";
            this.Code.Name = "Code";
            this.Code.Visible = false;
            this.Code.Width = 81;
            // 
            // EditEsfComponent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "EditEsfComponent";
            this.Size = new System.Drawing.Size(465, 371);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            // ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            //((System.ComponentModel.ISupportInitialize)(this.nodeValueGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView esfNodeTree;
        private System.Windows.Forms.DataGridView nodeValueGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Code;
    }
}
