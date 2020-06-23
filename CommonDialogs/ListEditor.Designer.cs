namespace CommonDialogs {
    partial class ListEditor {
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

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.toTheLeftButton = new System.Windows.Forms.Button();
            this.toTheRightButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rightListBox = new System.Windows.Forms.ListBox();
            this.rightListLabel = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.leftListBox = new System.Windows.Forms.ListBox();
            this.leftListLabel = new System.Windows.Forms.Label();
            this.bindingSourceLeft = new System.Windows.Forms.BindingSource(this.components);
            this.bindingSourceRight = new System.Windows.Forms.BindingSource(this.components);
            this.moveAllToTheRightButton = new System.Windows.Forms.Button();
            this.moveAllToTheLeftButton = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRight)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Location = new System.Drawing.Point(168, 521);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Location = new System.Drawing.Point(249, 521);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(458, 503);
            this.panel1.TabIndex = 2;
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.moveAllToTheLeftButton);
            this.panel4.Controls.Add(this.moveAllToTheRightButton);
            this.panel4.Controls.Add(this.toTheLeftButton);
            this.panel4.Controls.Add(this.toTheRightButton);
            this.panel4.Location = new System.Drawing.Point(188, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(44, 500);
            this.panel4.TabIndex = 8;
            // 
            // toTheLeftButton
            // 
            this.toTheLeftButton.Location = new System.Drawing.Point(3, 232);
            this.toTheLeftButton.Name = "toTheLeftButton";
            this.toTheLeftButton.Size = new System.Drawing.Size(36, 29);
            this.toTheLeftButton.TabIndex = 11;
            this.toTheLeftButton.Text = "<";
            this.toTheLeftButton.UseVisualStyleBackColor = true;
            this.toTheLeftButton.Click += new System.EventHandler(this.toTheLeftButton_Click);
            // 
            // toTheRightButton
            // 
            this.toTheRightButton.Location = new System.Drawing.Point(3, 195);
            this.toTheRightButton.Name = "toTheRightButton";
            this.toTheRightButton.Size = new System.Drawing.Size(36, 29);
            this.toTheRightButton.TabIndex = 10;
            this.toTheRightButton.Text = ">";
            this.toTheRightButton.UseVisualStyleBackColor = true;
            this.toTheRightButton.Click += new System.EventHandler(this.toTheRightButton_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.rightListBox);
            this.panel2.Controls.Add(this.rightListLabel);
            this.panel2.Location = new System.Drawing.Point(238, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(224, 500);
            this.panel2.TabIndex = 7;
            // 
            // rightListBox
            // 
            this.rightListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rightListBox.FormattingEnabled = true;
            this.rightListBox.Location = new System.Drawing.Point(3, 21);
            this.rightListBox.Name = "rightListBox";
            this.rightListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.rightListBox.Size = new System.Drawing.Size(214, 459);
            this.rightListBox.TabIndex = 1;
            // 
            // rightListLabel
            // 
            this.rightListLabel.AutoSize = true;
            this.rightListLabel.Location = new System.Drawing.Point(4, 4);
            this.rightListLabel.Name = "rightListLabel";
            this.rightListLabel.Size = new System.Drawing.Size(51, 13);
            this.rightListLabel.TabIndex = 0;
            this.rightListLabel.Text = "Right List";
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.leftListBox);
            this.panel3.Controls.Add(this.leftListLabel);
            this.panel3.Location = new System.Drawing.Point(5, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(179, 500);
            this.panel3.TabIndex = 6;
            // 
            // leftListBox
            // 
            this.leftListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.leftListBox.FormattingEnabled = true;
            this.leftListBox.Location = new System.Drawing.Point(7, 21);
            this.leftListBox.Name = "leftListBox";
            this.leftListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.leftListBox.Size = new System.Drawing.Size(164, 459);
            this.leftListBox.TabIndex = 1;
            // 
            // leftListLabel
            // 
            this.leftListLabel.AutoSize = true;
            this.leftListLabel.Location = new System.Drawing.Point(4, 4);
            this.leftListLabel.Name = "leftListLabel";
            this.leftListLabel.Size = new System.Drawing.Size(44, 13);
            this.leftListLabel.TabIndex = 0;
            this.leftListLabel.Text = "Left List";
            // 
            // moveAllToTheRightButton
            // 
            this.moveAllToTheRightButton.Location = new System.Drawing.Point(3, 160);
            this.moveAllToTheRightButton.Name = "moveAllToTheRightButton";
            this.moveAllToTheRightButton.Size = new System.Drawing.Size(36, 29);
            this.moveAllToTheRightButton.TabIndex = 12;
            this.moveAllToTheRightButton.Text = ">>";
            this.moveAllToTheRightButton.UseVisualStyleBackColor = true;
            this.moveAllToTheRightButton.Click += new System.EventHandler(this.moveAllToTheRightButton_Click);
            // 
            // moveAllToTheLeftButton
            // 
            this.moveAllToTheLeftButton.Location = new System.Drawing.Point(5, 267);
            this.moveAllToTheLeftButton.Name = "moveAllToTheLeftButton";
            this.moveAllToTheLeftButton.Size = new System.Drawing.Size(36, 29);
            this.moveAllToTheLeftButton.TabIndex = 13;
            this.moveAllToTheLeftButton.Text = "<<";
            this.moveAllToTheLeftButton.UseVisualStyleBackColor = true;
            this.moveAllToTheLeftButton.Click += new System.EventHandler(this.moveAllToTheLeftButton_Click);
            // 
            // ListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 556);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Name = "ListEditor";
            this.Text = "ListEditor";
            this.panel1.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRight)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ListBox rightListBox;
        private System.Windows.Forms.Label rightListLabel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ListBox leftListBox;
        private System.Windows.Forms.Label leftListLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button toTheLeftButton;
        private System.Windows.Forms.Button toTheRightButton;
        private System.Windows.Forms.BindingSource bindingSourceLeft;
        private System.Windows.Forms.BindingSource bindingSourceRight;
        private System.Windows.Forms.Button moveAllToTheLeftButton;
        private System.Windows.Forms.Button moveAllToTheRightButton;
    }
}