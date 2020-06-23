namespace DbDecoding
{
    partial class SchemaFieldNameChooser
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.leftFieldListBox = new System.Windows.Forms.ListBox();
            this.rightFieldListBox = new System.Windows.Forms.ListBox();
            this.resultFieldListBox = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.okButton);
            this.panel1.Controls.Add(this.infoLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(676, 33);
            this.panel1.TabIndex = 0;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(589, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 9);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(35, 13);
            this.infoLabel.TabIndex = 0;
            this.infoLabel.Text = "label1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.resultFieldListBox);
            this.splitContainer1.Size = new System.Drawing.Size(676, 524);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.leftFieldListBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.rightFieldListBox);
            this.splitContainer2.Size = new System.Drawing.Size(676, 240);
            this.splitContainer2.SplitterDistance = 333;
            this.splitContainer2.TabIndex = 0;
            // 
            // leftFieldListBox
            // 
            this.leftFieldListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftFieldListBox.FormattingEnabled = true;
            this.leftFieldListBox.Location = new System.Drawing.Point(0, 0);
            this.leftFieldListBox.Name = "leftFieldListBox";
            this.leftFieldListBox.Size = new System.Drawing.Size(333, 240);
            this.leftFieldListBox.TabIndex = 0;
            this.leftFieldListBox.DoubleClick += onListDoubleClick;
            // 
            // rightFieldListBox
            // 
            this.rightFieldListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightFieldListBox.FormattingEnabled = true;
            this.rightFieldListBox.Location = new System.Drawing.Point(0, 0);
            this.rightFieldListBox.Name = "rightFieldListBox";
            this.rightFieldListBox.Size = new System.Drawing.Size(339, 240);
            this.rightFieldListBox.TabIndex = 0;
            this.rightFieldListBox.DoubleClick += onListDoubleClick;
            // 
            // resultFieldListBox
            // 
            this.resultFieldListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultFieldListBox.FormattingEnabled = true;
            this.resultFieldListBox.Location = new System.Drawing.Point(0, 0);
            this.resultFieldListBox.Name = "resultFieldListBox";
            this.resultFieldListBox.Size = new System.Drawing.Size(676, 280);
            this.resultFieldListBox.TabIndex = 0;
            // 
            // SchemaFieldNameChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 557);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "SchemaFieldNameChooser";
            this.Text = "SchemaFieldNameChooser";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ListBox leftFieldListBox;
        private System.Windows.Forms.ListBox rightFieldListBox;
        private System.Windows.Forms.ListBox resultFieldListBox;
    }
}