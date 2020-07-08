namespace PackFileManager.ExtentedTreeView
{
    partial class ExtendedTreeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._filterLabel = new System.Windows.Forms.Label();
            this._treeViewSearchBox = new System.Windows.Forms.TextBox();
            this._treeView = new System.Windows.Forms.TreeView();
            this.packTreeViewToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // _filterLabel
            // 
            this._filterLabel.AutoSize = true;
            this._filterLabel.Location = new System.Drawing.Point(3, 4);
            this._filterLabel.Name = "_filterLabel";
            this._filterLabel.Size = new System.Drawing.Size(29, 13);
            this._filterLabel.TabIndex = 7;
            this._filterLabel.Text = "Filter";
            // 
            // _treeViewSearchBox
            // 
            this._treeViewSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._treeViewSearchBox.Location = new System.Drawing.Point(36, 1);
            this._treeViewSearchBox.Name = "_treeViewSearchBox";
            this._treeViewSearchBox.Size = new System.Drawing.Size(264, 20);
            this._treeViewSearchBox.TabIndex = 6;
            // 
            // _treeView
            // 
            this._treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._treeView.ForeColor = System.Drawing.SystemColors.WindowText;
            this._treeView.HideSelection = false;
            this._treeView.Indent = 19;
            this._treeView.Location = new System.Drawing.Point(0, 27);
            this._treeView.Name = "_treeView";
            this._treeView.Size = new System.Drawing.Size(300, 570);
            this._treeView.TabIndex = 5;
            // 
            // ExtendedTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._filterLabel);
            this.Controls.Add(this._treeViewSearchBox);
            this.Controls.Add(this._treeView);
            this.Name = "ExtendedTreeView";
            this.Size = new System.Drawing.Size(300, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _filterLabel;
        private System.Windows.Forms.TextBox _treeViewSearchBox;
        public System.Windows.Forms.TreeView _treeView;
        private System.Windows.Forms.ToolTip packTreeViewToolTip;
    }
}
