namespace EditSF {
    partial class EditSF {
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showNodeTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bookmarkSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runSingleTestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runTestsStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.editEsfComponent = new EsfControl.EditEsfComponent();
            this.menuStrip1.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
                this.bookmarksToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.testToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(789, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem1_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // bookmarksToolStripMenuItem
            // 
            this.bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.addBookmarkToolStripMenuItem,
                this.editBookmarkToolStripMenuItem,
                this.bookmarkSeparator
            });
            this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
            this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.bookmarksToolStripMenuItem.Text = "Bookmarks";
            // 
            // addBookmarkToolStripMenuItem
            // 
            this.addBookmarkToolStripMenuItem.Enabled = false;
            this.addBookmarkToolStripMenuItem.Name = "addBookmarkToolStripMenuItem";
            this.addBookmarkToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addBookmarkToolStripMenuItem.Text = "Add Bookmark";
            this.addBookmarkToolStripMenuItem.Click += new System.EventHandler(this.AddBookmark);
            // 
            // editBookmarkToolStripMenuItem
            // 
            this.editBookmarkToolStripMenuItem.Enabled = true;
            this.editBookmarkToolStripMenuItem.Name = "editBookmarkToolStripMenuItem";
            this.editBookmarkToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.editBookmarkToolStripMenuItem.Text = "Edit Bookmarks";
            this.editBookmarkToolStripMenuItem.Click += new System.EventHandler(this.EditBookmarks);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.writeLogFileToolStripMenuItem,
                this.showNodeTypeToolStripMenuItem
            });
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // writeLogFileToolStripMenuItem
            // 
            this.writeLogFileToolStripMenuItem.CheckOnClick = true;
            this.writeLogFileToolStripMenuItem.Name = "writeLogFileToolStripMenuItem";
            this.writeLogFileToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.writeLogFileToolStripMenuItem.Text = "Write Log File";
            // 
            // showNodeTypeToolStripMenuItem
            // 
            this.showNodeTypeToolStripMenuItem.CheckOnClick = true;
            this.showNodeTypeToolStripMenuItem.Enabled = false;
            this.showNodeTypeToolStripMenuItem.Name = "showNodeTypeToolStripMenuItem";
            this.showNodeTypeToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.showNodeTypeToolStripMenuItem.Text = "Show Node Type";
            this.showNodeTypeToolStripMenuItem.Click += new System.EventHandler(this.showNodeTypeToolStripMenuItem_Click);
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runSingleTestToolStripMenuItem,
            this.runTestsStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.testToolStripMenuItem.Text = "Tests";
            this.testToolStripMenuItem.Visible = false;
            // 
            // runSingleTestToolStripMenuItem
            // 
            this.runSingleTestToolStripMenuItem.Name = "runSingleTestToolStripMenuItem";
            this.runSingleTestToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.runSingleTestToolStripMenuItem.Text = "Run Load/Save Test";
            this.runSingleTestToolStripMenuItem.Click += new System.EventHandler(this.runSingleTestToolStripMenuItem_Click);
            // 
            // runTestsStripMenuItem
            // 
            this.runTestsStripMenuItem.Name = "runTestsStripMenuItem";
            this.runTestsStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.runTestsStripMenuItem.Text = "Multiple Tests";
            this.runTestsStripMenuItem.Click += new System.EventHandler(this.runTestsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.statusLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 769);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(789, 22);
            this.statusBar.TabIndex = 2;
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(86, 17);
            this.statusLabel.Text = "No File Loaded";
            // 
            // editEsfComponent
            // 
            this.editEsfComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editEsfComponent.Location = new System.Drawing.Point(0, 24);
            this.editEsfComponent.Name = "editEsfComponent";
            this.editEsfComponent.RootNode = null;
            this.editEsfComponent.ShowCode = false;
            this.editEsfComponent.Size = new System.Drawing.Size(789, 745);
            this.editEsfComponent.TabIndex = 3;
            // 
            // EditSF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(789, 791);
            this.Controls.Add(this.editEsfComponent);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EditSF";
            this.Text = "EditSF";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeLogFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runTestsStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runSingleTestToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator bookmarkSeparator;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showNodeTypeToolStripMenuItem;
        private EsfControl.EditEsfComponent editEsfComponent;
    }
}

