namespace PackFileManager {
    partial class PackedEsfEditor {
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
            this.esfComponent = new EsfControl.EditEsfComponent();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.bookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // esfComponent
            // 
            this.esfComponent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.esfComponent.Location = new System.Drawing.Point(0, 24);
            this.esfComponent.Name = "esfComponent";
            this.esfComponent.RootNode = null;
            this.esfComponent.ShowCode = false;
            this.esfComponent.Size = new System.Drawing.Size(205, 166);
            this.esfComponent.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bookmarksToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip1";
            this.menuStrip.Size = new System.Drawing.Size(205, 24);
            this.menuStrip.TabIndex = 1;
            this.menuStrip.Text = "menuStrip1";
            // 
            // bookmarksToolStripMenuItem
            // 
            this.bookmarksToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolStripSeparator});
            this.bookmarksToolStripMenuItem.Name = "bookmarksToolStripMenuItem";
            this.bookmarksToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.bookmarksToolStripMenuItem.Text = "Bookmarks";
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.addToolStripMenuItem.Text = "Add Bookmark";
            this.addToolStripMenuItem.Click += HandleAddToolStripMenuItemhandleClick;
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.editToolStripMenuItem.Text = "Edit Bookmarks";
            this.editToolStripMenuItem.Click += EditBookmarks;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator.Name = "toolStripSeparator1";
            this.toolStripSeparator.Size = new System.Drawing.Size(153, 6);
            // 
            // PackedEsfEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.esfComponent);
            this.Controls.Add(this.menuStrip);
            this.Name = "PackedEsfEditor";
            this.Size = new System.Drawing.Size(205, 190);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private EsfControl.EditEsfComponent esfComponent;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem bookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
    }
}
