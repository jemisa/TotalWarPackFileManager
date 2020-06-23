using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace CommonDialogs {
    /*
     * Simple form to ask the user for a string.
     */
    public partial class InputBox : Form {
        public InputBox() {
            InitializeComponent();
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }
        /*
         * The text entered in the input box.
         * Can be used before showing the dialog to set initial input.
         */
        public string Input {
            get {
                return valueField.Text;
            }
            set {
                valueField.Text = value;
            }
        }

        private void CloseWithOk(object sender = null, EventArgs e = null) {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void CloseWithCancel(object sender = null, EventArgs e = null) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
