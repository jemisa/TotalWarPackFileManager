namespace PackFileManager.PackedTreeView
{
    partial class PackedTreeView
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
            this._filterLabel = new System.Windows.Forms.Label();
            this._treeViewSearchBox = new System.Windows.Forms.TextBox();
            this.treeViewAdv1 = new Aga.Controls.Tree.TreeViewAdv();
            this.nodeIcon1 = new Aga.Controls.Tree.NodeControls.NodeIcon();
            this.nodeTextBox1 = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._extentionDropDown = new CommonDialogs.CheckedComboBox();
            this._clearFilterButton = new System.Windows.Forms.Button();
            this._clearExtentionFilterButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _filterLabel
            // 
            this._filterLabel.AutoSize = true;
            this._filterLabel.Location = new System.Drawing.Point(3, 6);
            this._filterLabel.Name = "_filterLabel";
            this._filterLabel.Size = new System.Drawing.Size(29, 13);
            this._filterLabel.TabIndex = 7;
            this._filterLabel.Text = "Filter";
            // 
            // _treeViewSearchBox
            // 
            this._treeViewSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._treeViewSearchBox.Location = new System.Drawing.Point(60, 3);
            this._treeViewSearchBox.Name = "_treeViewSearchBox";
            this._treeViewSearchBox.Size = new System.Drawing.Size(186, 20);
            this._treeViewSearchBox.TabIndex = 6;
            this._treeViewSearchBox.TextChanged += new System.EventHandler(this._treeViewSearchBox_TextChanged);
            // 
            // treeViewAdv1
            // 
            this.treeViewAdv1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewAdv1.BackColor = System.Drawing.SystemColors.Window;
            this.treeViewAdv1.ColumnHeaderHeight = 0;
            this.treeViewAdv1.DefaultToolTipProvider = null;
            this.treeViewAdv1.DragDropMarkColor = System.Drawing.Color.Black;
            this.treeViewAdv1.FullRowSelectActiveColor = System.Drawing.Color.Empty;
            this.treeViewAdv1.FullRowSelectInactiveColor = System.Drawing.Color.Empty;
            this.treeViewAdv1.LineColor = System.Drawing.SystemColors.ControlDark;
            this.treeViewAdv1.LoadOnDemand = true;
            this.treeViewAdv1.Location = new System.Drawing.Point(3, 53);
            this.treeViewAdv1.Model = null;
            this.treeViewAdv1.Name = "treeViewAdv1";
            this.treeViewAdv1.NodeControls.Add(this.nodeIcon1);
            this.treeViewAdv1.NodeControls.Add(this.nodeTextBox1);
            this.treeViewAdv1.NodeFilter = null;
            this.treeViewAdv1.SelectedNode = null;
            this.treeViewAdv1.ShowNodeToolTips = true;
            this.treeViewAdv1.Size = new System.Drawing.Size(294, 544);
            this.treeViewAdv1.TabIndex = 8;
            this.treeViewAdv1.Text = "treeViewAdv1";
            // 
            // nodeIcon1
            // 
            this.nodeIcon1.DataPropertyName = "Image";
            this.nodeIcon1.LeftMargin = 1;
            this.nodeIcon1.ParentColumn = null;
            this.nodeIcon1.ScaleMode = Aga.Controls.Tree.ImageScaleMode.ScaleDown;
            // 
            // nodeTextBox1
            // 
            this.nodeTextBox1.DataPropertyName = "Text";
            this.nodeTextBox1.IncrementalSearchEnabled = true;
            this.nodeTextBox1.LeftMargin = 3;
            this.nodeTextBox1.ParentColumn = null;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Extention";
            // 
            // _extentionDropDown
            // 
            this._extentionDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._extentionDropDown.CheckOnClick = true;
            this._extentionDropDown.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this._extentionDropDown.DropDownHeight = 1;
            this._extentionDropDown.FormattingEnabled = true;
            this._extentionDropDown.IntegralHeight = false;
            this._extentionDropDown.Location = new System.Drawing.Point(60, 26);
            this._extentionDropDown.Name = "_extentionDropDown";
            this._extentionDropDown.Size = new System.Drawing.Size(186, 21);
            this._extentionDropDown.TabIndex = 9;
            this._extentionDropDown.ValueSeparator = ", ";
            this._extentionDropDown.DropDownClosed += new System.EventHandler(this.OnExtentionFilterChanged);
            // 
            // _clearFilterButton
            // 
            this._clearFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._clearFilterButton.Location = new System.Drawing.Point(252, 1);
            this._clearFilterButton.Name = "_clearFilterButton";
            this._clearFilterButton.Size = new System.Drawing.Size(44, 23);
            this._clearFilterButton.TabIndex = 11;
            this._clearFilterButton.Text = "Clear";
            this._clearFilterButton.UseVisualStyleBackColor = true;
            // 
            // _clearExtentionFilterButton
            // 
            this._clearExtentionFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._clearExtentionFilterButton.Location = new System.Drawing.Point(252, 24);
            this._clearExtentionFilterButton.Name = "_clearExtentionFilterButton";
            this._clearExtentionFilterButton.Size = new System.Drawing.Size(44, 23);
            this._clearExtentionFilterButton.TabIndex = 12;
            this._clearExtentionFilterButton.Text = "Clear";
            this._clearExtentionFilterButton.UseVisualStyleBackColor = true;
            this._clearExtentionFilterButton.Click += new System.EventHandler(this.OnClearFilterExtention);
            // 
            // PackedTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._clearExtentionFilterButton);
            this.Controls.Add(this._clearFilterButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._extentionDropDown);
            this.Controls.Add(this.treeViewAdv1);
            this.Controls.Add(this._filterLabel);
            this.Controls.Add(this._treeViewSearchBox);
            this.Name = "PackedTreeView";
            this.Size = new System.Drawing.Size(300, 600);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _filterLabel;
        private System.Windows.Forms.TextBox _treeViewSearchBox;
        private Aga.Controls.Tree.TreeViewAdv treeViewAdv1;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nodeTextBox1;
        private Aga.Controls.Tree.NodeControls.NodeIcon nodeIcon1;
        private CommonDialogs.CheckedComboBox _extentionDropDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _clearFilterButton;
        private System.Windows.Forms.Button _clearExtentionFilterButton;
    }
}
