namespace MMS {
    partial class MainForm {
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
            System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.modsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setActiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreBackupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreOriginalDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.postprocessToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optimizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cleanUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.directoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modPackInPFMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assemblyKitDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mmsBackupDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shogunDataDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extrasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installationDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPFMPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.launchShogunButton = new System.Windows.Forms.Button();
            this.launchTweak = new System.Windows.Forms.Button();
            this.launchBobButton = new System.Windows.Forms.Button();
            this.modList = new System.Windows.Forms.ListBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(135, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(192, 6);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modsToolStripMenuItem,
            this.dataToolStripMenuItem,
            this.postprocessToolStripMenuItem,
            this.directoriesToolStripMenuItem,
            this.extrasToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(616, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // modsToolStripMenuItem
            // 
            this.modsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addModToolStripMenuItem,
            this.importModToolStripMenuItem,
            this.setActiveToolStripMenuItem,
            this.renameModToolStripMenuItem,
            toolStripSeparator1,
            this.deleteModToolStripMenuItem});
            this.modsToolStripMenuItem.Name = "modsToolStripMenuItem";
            this.modsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.modsToolStripMenuItem.Text = "Mod";
            // 
            // addModToolStripMenuItem
            // 
            this.addModToolStripMenuItem.Name = "addModToolStripMenuItem";
            this.addModToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.addModToolStripMenuItem.Text = "Add";
            this.addModToolStripMenuItem.Click += new System.EventHandler(this.AddMod);
            // 
            // importModToolStripMenuItem
            // 
            this.importModToolStripMenuItem.Name = "importModToolStripMenuItem";
            this.importModToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.importModToolStripMenuItem.Text = "Import pack";
            this.importModToolStripMenuItem.Click += new System.EventHandler(this.ImportExistingPack);
            // 
            // setActiveToolStripMenuItem
            // 
            this.setActiveToolStripMenuItem.Name = "setActiveToolStripMenuItem";
            this.setActiveToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.setActiveToolStripMenuItem.Text = "Set Active";
            this.setActiveToolStripMenuItem.Click += new System.EventHandler(this.SetMod);
            // 
            // deleteModToolStripMenuItem
            // 
            this.deleteModToolStripMenuItem.Name = "deleteModToolStripMenuItem";
            this.deleteModToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.deleteModToolStripMenuItem.Text = "Delete";
            this.deleteModToolStripMenuItem.Click += new System.EventHandler(this.DeleteMod);
            // 
            // renameModToolStripMenuItem
            // 
            this.renameModToolStripMenuItem.Name = "renameModToolStripMenuItem";
            this.renameModToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.renameModToolStripMenuItem.Text = "Rename";
            this.renameModToolStripMenuItem.Click += new System.EventHandler(this.RenameMod);
            // 
            // dataToolStripMenuItem
            // 
            this.dataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backupToolStripMenuItem,
            this.restoreBackupToolStripMenuItem,
            this.restoreOriginalDataToolStripMenuItem});
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.dataToolStripMenuItem.Text = "Data";
            // 
            // backupToolStripMenuItem
            // 
            this.backupToolStripMenuItem.Name = "backupToolStripMenuItem";
            this.backupToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.backupToolStripMenuItem.Text = "Backup changes";
            this.backupToolStripMenuItem.Click += new System.EventHandler(this.BackupCurrentMod);
            // 
            // restoreBackupToolStripMenuItem
            // 
            this.restoreBackupToolStripMenuItem.Name = "restoreBackupToolStripMenuItem";
            this.restoreBackupToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.restoreBackupToolStripMenuItem.Text = "Restore backup";
            this.restoreBackupToolStripMenuItem.Click += new System.EventHandler(this.RestoreBackupData);
            // 
            // restoreOriginalDataToolStripMenuItem
            // 
            this.restoreOriginalDataToolStripMenuItem.Name = "restoreOriginalDataToolStripMenuItem";
            this.restoreOriginalDataToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.restoreOriginalDataToolStripMenuItem.Text = "Restore original data";
            this.restoreOriginalDataToolStripMenuItem.Click += new System.EventHandler(this.RestoreOriginalData);
            // 
            // postprocessToolStripMenuItem
            // 
            this.postprocessToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optimizeToolStripMenuItem,
            this.cleanUpToolStripMenuItem,
            this.installToolStripMenuItem,
            this.uninstallToolStripMenuItem});
            this.postprocessToolStripMenuItem.Name = "postprocessToolStripMenuItem";
            this.postprocessToolStripMenuItem.Size = new System.Drawing.Size(82, 20);
            this.postprocessToolStripMenuItem.Text = "Postprocess";
            // 
            // optimizeToolStripMenuItem
            // 
            this.optimizeToolStripMenuItem.Name = "optimizeToolStripMenuItem";
            this.optimizeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.optimizeToolStripMenuItem.Text = "Optimize";
            this.optimizeToolStripMenuItem.Click += new System.EventHandler(this.OptimizePack);
            // 
            // cleanUpToolStripMenuItem
            // 
            this.cleanUpToolStripMenuItem.Name = "cleanUpToolStripMenuItem";
            this.cleanUpToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cleanUpToolStripMenuItem.Text = "Clean up";
            this.cleanUpToolStripMenuItem.Click += new System.EventHandler(this.CleanUp);
            // 
            // installToolStripMenuItem
            // 
            this.installToolStripMenuItem.Name = "installToolStripMenuItem";
            this.installToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.installToolStripMenuItem.Text = "Install";
            this.installToolStripMenuItem.Click += new System.EventHandler(this.InstallMod);
            // 
            // uninstallToolStripMenuItem
            // 
            this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
            this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.uninstallToolStripMenuItem.Text = "Uninstall";
            this.uninstallToolStripMenuItem.Click += new System.EventHandler(this.UninstallMod);
            // 
            // directoriesToolStripMenuItem
            // 
            this.directoriesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modPackInPFMToolStripMenuItem,
            toolStripSeparator2,
            this.assemblyKitDirectoryToolStripMenuItem,
            this.mmsBackupDirectoryToolStripMenuItem,
            this.shogunDataDirectoryToolStripMenuItem});
            this.directoriesToolStripMenuItem.Name = "directoriesToolStripMenuItem";
            this.directoriesToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.directoriesToolStripMenuItem.Text = "Open...";
            // 
            // modPackInPFMToolStripMenuItem
            // 
            this.modPackInPFMToolStripMenuItem.Name = "modPackInPFMToolStripMenuItem";
            this.modPackInPFMToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.modPackInPFMToolStripMenuItem.Text = "Mod Pack in PFM";
            this.modPackInPFMToolStripMenuItem.Click += new System.EventHandler(this.modPackInPFMToolStripMenuItem_Click);
            // 
            // assemblyKitDirectoryToolStripMenuItem
            // 
            this.assemblyKitDirectoryToolStripMenuItem.Name = "assemblyKitDirectoryToolStripMenuItem";
            this.assemblyKitDirectoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.assemblyKitDirectoryToolStripMenuItem.Text = "Assembly Kit Directory";
            this.assemblyKitDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // mmsBackupDirectoryToolStripMenuItem
            // 
            this.mmsBackupDirectoryToolStripMenuItem.Name = "mmsBackupDirectoryToolStripMenuItem";
            this.mmsBackupDirectoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.mmsBackupDirectoryToolStripMenuItem.Text = "MMS Backup Directory";
            this.mmsBackupDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // shogunDataDirectoryToolStripMenuItem
            // 
            this.shogunDataDirectoryToolStripMenuItem.Name = "shogunDataDirectoryToolStripMenuItem";
            this.shogunDataDirectoryToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.shogunDataDirectoryToolStripMenuItem.Text = "Shogun data Directory";
            this.shogunDataDirectoryToolStripMenuItem.Click += new System.EventHandler(this.OpenDirectory);
            // 
            // extrasToolStripMenuItem
            // 
            this.extrasToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installationDirectoryToolStripMenuItem,
            this.setPFMPathToolStripMenuItem});
            this.extrasToolStripMenuItem.Name = "extrasToolStripMenuItem";
            this.extrasToolStripMenuItem.Size = new System.Drawing.Size(49, 20);
            this.extrasToolStripMenuItem.Text = "Extras";
            // 
            // installationDirectoryToolStripMenuItem
            // 
            this.installationDirectoryToolStripMenuItem.Name = "installationDirectoryToolStripMenuItem";
            this.installationDirectoryToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.installationDirectoryToolStripMenuItem.Text = "Installation Directory";
            this.installationDirectoryToolStripMenuItem.Click += new System.EventHandler(this.SetInstallDirectory);
            // 
            // setPFMPathToolStripMenuItem
            // 
            this.setPFMPathToolStripMenuItem.Name = "setPFMPathToolStripMenuItem";
            this.setPFMPathToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.setPFMPathToolStripMenuItem.Text = "Set PFM path";
            this.setPFMPathToolStripMenuItem.Click += new System.EventHandler(this.BrowseForPfm);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 276);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(616, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.launchShogunButton);
            this.panel2.Controls.Add(this.launchTweak);
            this.panel2.Controls.Add(this.launchBobButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 243);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(616, 33);
            this.panel2.TabIndex = 3;
            // 
            // launchShogunButton
            // 
            this.launchShogunButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.launchShogunButton.Location = new System.Drawing.Point(504, 6);
            this.launchShogunButton.Name = "launchShogunButton";
            this.launchShogunButton.Size = new System.Drawing.Size(109, 23);
            this.launchShogunButton.TabIndex = 5;
            this.launchShogunButton.Text = "Launch Shogun";
            this.launchShogunButton.UseVisualStyleBackColor = true;
            this.launchShogunButton.Click += new System.EventHandler(this.LaunchShogun);
            // 
            // launchTweak
            // 
            this.launchTweak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.launchTweak.Location = new System.Drawing.Point(3, 6);
            this.launchTweak.Name = "launchTweak";
            this.launchTweak.Size = new System.Drawing.Size(106, 23);
            this.launchTweak.TabIndex = 4;
            this.launchTweak.Text = "Launch TWeak";
            this.launchTweak.UseVisualStyleBackColor = true;
            this.launchTweak.Click += new System.EventHandler(this.LaunchTweak);
            // 
            // launchBobButton
            // 
            this.launchBobButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.launchBobButton.Location = new System.Drawing.Point(115, 6);
            this.launchBobButton.Name = "launchBobButton";
            this.launchBobButton.Size = new System.Drawing.Size(108, 23);
            this.launchBobButton.TabIndex = 3;
            this.launchBobButton.Text = "Launch BOB";
            this.launchBobButton.UseVisualStyleBackColor = true;
            this.launchBobButton.Click += new System.EventHandler(this.LaunchBob);
            // 
            // modList
            // 
            this.modList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modList.FormattingEnabled = true;
            this.modList.Location = new System.Drawing.Point(0, 24);
            this.modList.Name = "modList";
            this.modList.Size = new System.Drawing.Size(616, 219);
            this.modList.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 298);
            this.Controls.Add(this.modList);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "MMS - MultiMod Support";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem extrasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installationDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addModToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameModToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteModToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importModToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ListBox modList;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button launchBobButton;
        private System.Windows.Forms.Button launchTweak;
        private System.Windows.Forms.ToolStripMenuItem setActiveToolStripMenuItem;
        private System.Windows.Forms.Button launchShogunButton;
        private System.Windows.Forms.ToolStripMenuItem directoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assemblyKitDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shogunDataDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modPackInPFMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPFMPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mmsBackupDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreBackupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreOriginalDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem postprocessToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cleanUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem installToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optimizeToolStripMenuItem;
    }
}

