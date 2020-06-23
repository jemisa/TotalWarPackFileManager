using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PackFileManager
{
	public partial class InputBox : Form
	{
		public InputBox()
		{
            InitializeComponent();
            AcceptButton = okButton;
		}
        public string Input
        {
            get
            {
                return inputField.Text;
            }
            set 
            {
                inputField.Text = value;
            }
        }
        private void closeDialog(DialogResult result)
        {
            DialogResult = result;
            Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            closeDialog(DialogResult.OK);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            closeDialog(DialogResult.Cancel);
        }
	}
}
