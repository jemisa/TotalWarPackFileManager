using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PackFileManager {
    public partial class PackBrowseDialog : Form {
        public PackBrowseDialog() {
            InitializeComponent();
            AcceptButton = okButton;
            CancelButton = cancelButton;
            
            /*
             * Add/Remove to/from selected file list upon double click.
             */
            packFileTree.NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
                PackedFile selected = e.Node.Tag as PackedFile;
                if (selected != null) {
                    if (selectedFiles.Contains(selected)) {
                        selectedFiles.Remove(selected);
                        statusLabel.Text = string.Format("{0} removed", selected.FullPath);
                    } else {
                        selectedFiles.Add(selected);
                        statusLabel.Text = string.Format("{0} added", selected.FullPath);
                    }
                }
            };
            /*
             * Add all files below a directory upon right click.
             */
            packFileTree.NodeMouseClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
                VirtualDirectory directory = e.Node.Tag as VirtualDirectory;
                if (e.Button == MouseButtons.Right && directory != null) {
                    directory.AllFiles.ForEach(f => {
                        if (!selectedFiles.Contains(f)) {
                            selectedFiles.Add(f);
                            statusLabel.Text = string.Format("{0} added", f.FullPath);
                        }
                    });
                }
            };
        }

        /*
         * The pack file to browse.
         */
        private PackFile pack;
        public PackFile PackFile {
            private get { return pack; }
            set {
                statusLabel.Text = "Double click to select file; right-click directory to add all files below";
                pack = value;
                packFileTree.Nodes.Clear();
                selectedFiles.Clear();
                if (value != null) {
                    packFileTree.Nodes.Add(new DirEntryNode(pack.Root));
                }
            }
        }
  
        /*
         * The files that were selected while browsing.
         * Selection happens by double-clicking on a file node.
         */
        private List<PackedFile> selectedFiles = new List<PackedFile>();
        public List<PackedFile> SelectedFiles {
            get {
                if (selectedFiles.Count == 0) {
                    TreeNode node = packFileTree.SelectedNode;
                    if (node != null) {
                        PackedFile selected = node.Tag as PackedFile;
                        if (selected != null) {
                            selectedFiles.Add(selected);
                        }
                    }
                }
                return selectedFiles;
            }
        }

        // close dialog with result "OK"
        private void CloseWithOk(object sender = null, EventArgs e = null) {
            DialogResult = DialogResult.OK;
            Close();
        }

        // close dialog with result "Cancel"
        private void CloseWithCancel(object sender = null, EventArgs e = null) {
            DialogResult = DialogResult.Cancel;
            selectedFiles.Clear();
            Close();
        }
    }
}
