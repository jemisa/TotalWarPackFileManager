using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CommonDialogs {
    /*
     * Dialog to display instead of the the FolderBrowserDialog which is aweful.
     */
    public partial class DirectoryDialog : Form {
        public DirectoryDialog() {
            InitializeComponent();

            okButton.NotifyDefault(true);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        // the text to display to the user
        public string Description {
            set {
                label.Text = value;
            }
            private get {
                return label.Text;
            }
        }

        // the starting or chosen directory (will be empty string if cancelled)
        public string SelectedPath {
            get {
                return directory.Text;
            }
            set {
                directory.Text = value;
            }
        }

        // handle the "browse" button (show the folder browser dialog)
        private void Browse(object sender, EventArgs e) {
            FolderBrowserDialog extractFolderBrowserDialog = new FolderBrowserDialog {
                Description = Description,
                SelectedPath = SelectedPath
            };
            if (extractFolderBrowserDialog.ShowDialog() == DialogResult.OK) {
                SelectedPath = extractFolderBrowserDialog.SelectedPath;
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
            SelectedPath = "";
            Close();
        }
    }
}
