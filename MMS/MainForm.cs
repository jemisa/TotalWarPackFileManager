using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using MMS.Properties;
using Common;
using CommonDialogs;
using CommonUtilities;
using Filetypes;

namespace MMS {
    public partial class MainForm : Form {
        public MainForm() {
            InitializeComponent();

            if (string.IsNullOrEmpty(Settings.Default.ModToolPath) || !Directory.Exists(Settings.Default.ModToolPath)) {
                SetInstallDirectory();
                if (ModTools.Instance.InstallDirectory == null) {
                    throw new Exception("Need installation directory to continue.");
                }
            } else {
                string path = Settings.Default.ModToolPath;
                ModTools.Instance.InstallDirectory = path;
            }
            
            CheckShogunInstallation();

            SetInstallDirectoryLabelText();

            FillModList();
            MultiMods.Instance.ModListChanged += FillModList;
            MultiMods.Instance.CurrentModSet += FillModList;
            mmsBackupDirectoryToolStripMenuItem.Tag = Mod.MmsBaseDirectory;
            EnableMenuItems();
            MultiMods.Instance.CurrentModSet += EnableMenuItems;

            FormClosing += delegate(object o, FormClosingEventArgs args) {
                new Thread(Cleanup).Start();
#if DEBUG
                // to play around with no settings to imitate original start
                //Settings.Default.ModToolPath = "";
                //Settings.Default.ActiveMod = "";
#endif
                Settings.Default.Save();
            };

            Text = string.Format("MMS {0} - MultiMod Support", Application.ProductVersion);

            assemblyKitDirectoryToolStripMenuItem.Tag = ModTools.Instance.InstallDirectory;
            shogunDataDirectoryToolStripMenuItem.Tag = Game.STW.DataDirectory;
        }

        private void EnableMenuItems() {
            modPackInPFMToolStripMenuItem.Enabled = MultiMods.Instance.CurrentMod != null;
            backupToolStripMenuItem.Enabled = MultiMods.Instance.CurrentMod != null;
            restoreBackupToolStripMenuItem.Enabled = MultiMods.Instance.CurrentMod != null;
        }

        List<Process> startedProcesses = new List<Process>();

        #region Display Helpers
        void SetInstallDirectoryLabelText() {
            statusLabel.Text = string.Format("Mod Tools Location: {0}", ModTools.Instance.InstallDirectory);
        }

        void FillModList() {
            modList.Items.Clear();
            foreach (Mod mod in MultiMods.Instance.Mods) {
                modList.Items.Add(mod);
            }
            modList.SelectedItem = MultiMods.Instance.CurrentMod;
        }
        #endregion

        #region Mod Management
        // add new (prompt for name)
        private void AddMod(object sender, EventArgs e) {
            string newModName = PromptModName();
            if (newModName != null) {
                if (MultiMods.Instance.CurrentMod != null) {
                    statusLabel.Text = string.Format("Backing up {0}...", MultiMods.Instance.CurrentMod.Name);
                    Refresh();
                }
                MultiMods.Instance.AddMod(newModName);
            }
        }

        /*
         * Prompt the user for a mod name.
         * Returns null if user cancels.
         */
        string PromptModName(string initialInput = "") {
            string result = null;
            while (result == null) {
                InputBox inputBox = new InputBox {
                    Text = "Enter new Mod name",
                    Input = initialInput
                };
                if (inputBox.ShowDialog() != DialogResult.Cancel) {
                    if (inputBox.Input.Contains(" ")) {
                        string correctedName = inputBox.Input.Replace(" ", "_");
                        DialogResult query = 
                            MessageBox.Show(string.Format("Name cannot contain spaces. Want to use name \"{0}\" instead?\n"+
                                                          "Yes - rename\nNo - enter other name\nCancel - Abort", correctedName),
                                            "Invalid mod name", MessageBoxButtons.YesNoCancel);
                        if (query == DialogResult.Yes) {
                            // will terminate while loop
                            result = correctedName;
                        } else if (query == DialogResult.Cancel) {
                            // out of while; result stays null
                            break;
                        } // else query == No: re-enter
                        initialInput = inputBox.Input;
                    }
                } else {
                    break;
                }
            }
            return result;
        }

        // rename selected mod
        private void RenameMod(object sender, EventArgs e) {
            if (SelectedMod != null) {
                string previousName = SelectedMod.Name;
                string newName = PromptModName(previousName);
                if (newName != null && !newName.Equals(previousName)) {
                    try {
                        SelectedMod.Name = newName;
                    } catch (Exception ex) {
                        MessageBox.Show(ex.Message, "Rename failed", 
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        // delete selected mod
        private void DeleteMod(object sender, EventArgs e) {
            if (SelectedMod != null) {
                string message = string.Format("This will delete all changes you have made for mod {0}.\n" +
                    "Warning: this can not be undone!", SelectedMod);
                DialogResult query = MessageBox.Show(
                    message, "Are you sure?",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (query == DialogResult.OK) {
                    MultiMods.Instance.DeleteMod(SelectedMod);
                }
            }
        }

        // add from pack file
        private void ImportExistingPack(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                Filter = "Pack Files (*.pack)|*.pack|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == DialogResult.OK) {
                try {
                    new PackImporter().ImportExistingPack(dialog.FileName);
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Failed to import mod",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Active/Selected Mod
        /*
         * Retrieve the mod currently selected in the list view.
         * If none is selected, returns current mod (if any).
         */
        Mod SelectedMod {
            get {
                Mod result = modList.SelectedItem as Mod;
                if (result == null) {
                    result = MultiMods.Instance.CurrentMod;
                }
                return result;
            }
        }

        private void SetMod(object sender = null, EventArgs e = null) {
            if (SelectedMod != null) {
                statusLabel.Text = string.Format("Restoring data for {0}...", SelectedMod.Name);
                Refresh();
                MultiMods.Instance.CurrentMod = SelectedMod;
                modList.SelectedItem = MultiMods.Instance.CurrentMod;
                SetInstallDirectoryLabelText();
            }
        }
        #endregion

        #region Pack Recent
        bool ModPackRecent {
            get {
                // it's normal to have the rules.bob file edited.
                return SelectedMod.EditedAfterPackCreation.Count <= 1;
            }
        }

        bool QueryIgnoreEditedModPack() {
            bool result = ModPackRecent;
            if (!result) {
                string message = string.Join("\n", SelectedMod.EditedAfterPackCreation);
                message = "The following files were edited after pack creation:\n" + message + "\n" +
                    "Do you want to continue anyway?";
                result = MessageBox.Show(message, "Really install mod?", MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
            return result;
        }
        #endregion

        #region Installation
        private void InstallMod(object sender, EventArgs e) {
            if (SelectedMod != null) {
                try {
                    SetMod();

                    // check if data was edited or if to ignore this
                    if (!QueryIgnoreEditedModPack()) {
                        return;
                    }

                    if (File.Exists(SelectedMod.InstalledPackPath)) {
                        if (MessageBox.Show(string.Format("The mod file {0} already exists. Overwrite?", SelectedMod.InstalledPackPath),
                            "Overwrite existing pack?", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                            File.Delete(SelectedMod.InstalledPackPath);
                        } else {
                            return;
                        }
                    }
                    
                    SelectedMod.Install();
                    MessageBox.Show("Mod Installed!");
                } catch (Exception ex) {
                    MessageBox.Show(string.Format("Failed to install mod: {0}", ex.Message),
                        "Failed to install mod", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UninstallMod(object sender, EventArgs e) {
            if (SelectedMod != null) {
                SetMod();
                SelectedMod.Uninstall();
                MessageBox.Show("Mod Uninstalled!");
            }
        }
        #endregion

        #region External Processes
        private void LaunchBob(object sender, EventArgs e) {
            LaunchWithMod("BOB");
        }

        private void LaunchTweak(object sender, EventArgs e) {
            LaunchWithMod("TWeak");
        }

        private void LaunchShogun(object sender, EventArgs e) {
            List<string> notYetInstalled = new List<string>();
            if (SelectedMod != null && SelectedMod.EditedAfterPackCreation.Count > 1) {
                if (MessageBox.Show(
                    string.Format("The current mod ({0}) contains data that was not yet added to its pack.\nStart anyway?", SelectedMod.Name),
                    "Non-current mods detected", MessageBoxButtons.YesNo) == DialogResult.No) {
                    return;
                }
            }
            foreach (Mod mod in MultiMods.Instance.Mods) {
                if (File.GetLastWriteTime(mod.InstalledPackPath) < File.GetLastWriteTime(mod.PackFilePath)) {
                    notYetInstalled.Add(string.Format("- {0}", mod.Name));
                }
            }
            if (notYetInstalled.Count > 0 && MessageBox.Show(
                string.Format("The packs for the following mods are more recent than the one installed:\n{0}\nStart anyway?", 
                string.Join("\n", notYetInstalled)),
                "Non-current mods detected", MessageBoxButtons.YesNo) == DialogResult.No) {
                    return;
            }
            ProcessStartInfo info = new ProcessStartInfo {
                Arguments = "-applaunch 34330"
            };
            ExternalProcesses.Launch("Steam", info);
        }

        private void LaunchWithMod(string executable, string[] args = null) {
            if (SelectedMod != null) {
                SetMod();
                ProcessStartInfo info = new ProcessStartInfo(executable) {
                    WorkingDirectory = ModTools.Instance.BinariesPath
                };
                Process process = ExternalProcesses.Launch(executable, info);
                if (process != null) {
                    process.EnableRaisingEvents = true;
                    process.Exited += ProcessExited;
                    startedProcesses.Add(process);
                }
            } else {
                MessageBox.Show("Select or set a mod to work with.", "No active mod", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void ProcessExited(object o, EventArgs e) {
            Process p = o as Process;
            if (p != null) {
                startedProcesses.Remove(p);
            }
        }
        #endregion

        #region Pack Postprocessing
        private void OptimizePack(object sender, EventArgs e) {
            if (!DBTypeMap.Instance.Initialized) {
                DBTypeMap.Instance.InitializeTypeMap(Path.GetDirectoryName(Application.ExecutablePath));
            }
            if (SelectedMod != null) {
                if (!File.Exists(SelectedMod.PackFilePath)) {
                    MessageBox.Show(string.Format("Pack file for mod \"{0}\" not found ", SelectedMod.Name));
                    return;
                } else if (!QueryIgnoreEditedModPack()) {
                    return;
                }
                InputBox box = new InputBox {
                    Text = "Enter prefix for rename"
                };
                PackFileCodec codec = new PackFileCodec();
                PackFile packFile = codec.Open(SelectedMod.PackFilePath);
                if (box.ShowDialog() == DialogResult.OK) {
                    PackedFileRenamer renamer = new PackedFileRenamer(box.Input);
                    renamer.Rename(packFile.Files);
                }
                statusLabel.Text = "Optimizing DB files...";
                Refresh();
                new DbFileOptimizer(Game.STW).CreateOptimizedFile(packFile);
                codec.Save(packFile);
                SetInstallDirectoryLabelText();
            }
        }

        private void CleanUp(object sender, EventArgs e) {
            if (SelectedMod != null) {
                if (!QueryIgnoreEditedModPack()) {
                    return;
                }

                if (MessageBox.Show("This will delete the directories 'battleterrain' and 'variantmodels' from your pack. " +
                    "Do not do this if you changed anything there.\nContinue?", "Confirm cleanup",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) {
                        return;
                }

                PackFileCodec codec = new PackFileCodec();
                PackFile modPack = codec.Open(SelectedMod.PackFilePath);
                foreach (VirtualDirectory dir in modPack.Root.Subdirectories) {
                    if (dir.Name.Equals("battleterrain") || dir.Name.Equals("variantmodels")) {
                        dir.Deleted = true;
                    }
                }
                if (modPack.Root.Modified) {
                    string tempFilePath = Path.GetTempFileName();
                    codec.WriteToFile(tempFilePath, modPack);
                    File.Delete(SelectedMod.PackFilePath);
                    File.Move(tempFilePath, SelectedMod.PackFilePath);
                }
            }
        }
        #endregion

        #region Open Directories/pack
        private void OpenDirectory(object sender, EventArgs args) {
            string pathToOpen = ((ToolStripMenuItem)sender).Tag as string;
            if (pathToOpen != null && Directory.Exists(pathToOpen)) {
                Process.Start("explorer", pathToOpen);
            }
        }

        private void modPackInPFMToolStripMenuItem_Click(object sender, EventArgs e) {
            if (SelectedMod == null) {
                MessageBox.Show("No active mod");
                return;
            } else if (!File.Exists(SelectedMod.PackFilePath)) {
                MessageBox.Show(string.Format("Pack file {0} not found", SelectedMod.PackFilePath));
                return;
            }
            if (!File.Exists(Settings.Default.PfmPath)) {
                BrowseForPfm();
                if (!File.Exists(Settings.Default.PfmPath)) {
                    return;
                }
            }
            SetMod ();
            Process.Start(Settings.Default.PfmPath, SelectedMod.PackFilePath);
        }
        #endregion

        #region Locations in Filesystem
        private void BrowseForPfm(object sender = null, EventArgs e = null) {
            OpenFileDialog dialog = new OpenFileDialog {
                Title = "Please point to PFM path"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK) {
                Settings.Default.PfmPath = dialog.FileName;
            } else {
                return;
            }
        }
        /*
         * Lets the user choose the mod installation directory.
         */
        private void SetInstallDirectory(object sender = null, EventArgs e = null) {
            DirectoryDialog folderBrowser = new DirectoryDialog() {
                Description = "Please point to the location of your mod tools installation"
            };
            if (folderBrowser.ShowDialog() == DialogResult.OK) {
                ModTools.Instance.InstallDirectory = folderBrowser.SelectedPath;
                Settings.Default.ModToolPath = folderBrowser.SelectedPath;
                SetInstallDirectoryLabelText();
            }
        }

        void CheckShogunInstallation() {
            Game g = Game.STW;
            string gamePathFilename = Path.Combine(ModTools.Instance.BinariesPath, "game_path.txt");
            // prefer loaded from file so the user can force an installation location
            if (!g.IsInstalled) {
                string gamePath = "";

                // check if the game path file is set
                try {
                    // read path from file
                    gamePath = File.ReadAllText(gamePathFilename).Trim();
                    if (Directory.Exists(gamePath)) {
                        g.GameDirectory = gamePath;
                        return;
                    }
                } catch {}
                
                // ask user
                DirectoryDialog dlg = new DirectoryDialog() {
                    Description = string.Format("Please enter location of {0}.", g.Id),
                    SelectedPath = gamePath
                };
                if (dlg.ShowDialog() == DialogResult.OK) {
                    g.GameDirectory = dlg.SelectedPath;
                    
                } else {
                    // add empty entry to file for next time
                    g.GameDirectory = Game.NOT_INSTALLED;
                }
            } else if (g.GameDirectory.Equals(Game.NOT_INSTALLED)) {
                // mark as invalid
                g.GameDirectory = null;
            }
            if (!g.IsInstalled) {
                throw new InvalidOperationException("Cannot find Shogun installation directory.\n");
            }

            // write to gamepath file
            File.WriteAllText(gamePathFilename, g.GameDirectory);
        }
        #endregion

        #region Backup
        private void BackupCurrentMod(object sender, EventArgs e) {
            if (MultiMods.Instance.CurrentMod != null) {
                MultiMods.Instance.CurrentMod.Backup();
            }
        }

        private void RestoreBackupData(object sender, EventArgs e) {
            if (MultiMods.Instance.CurrentMod != null) {
                if (MessageBox.Show("This will undo all changes in this session or after the last backup.\n" +
                    "Are you sure?", "Restore Backup?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                        MultiMods.Instance.CurrentMod.Restore();
                }
            }
        }
        private void RestoreOriginalData(object sender, EventArgs e) {
            if (SelectedMod != null) {
                if (MessageBox.Show("This will undo all changes for the currently set mod.\nContinue?",
                                    "Really restore data?", MessageBoxButtons.OKCancel) == DialogResult.Cancel) {
                    return;
                }
            }
            ModTools.RestoreOriginalData();
        }
        void Cleanup() {
            // the user might close the MMS window before exiting a tool he started,
            // so wait for all processes to finish
            foreach (Process process in startedProcesses) {
                process.Exited -= ProcessExited;
                process.WaitForExit();
            }
            if (MultiMods.Instance.CurrentMod != null) {
                MultiMods.Instance.CurrentMod.IsActive = false;
            }
        }

        #endregion
    }
}
