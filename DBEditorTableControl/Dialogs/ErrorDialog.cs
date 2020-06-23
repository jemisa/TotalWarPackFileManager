using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DBTableControl
{
    public partial class ErrorDialog : Form
    {
        private ErrorDialog(Exception ex)
        {
            InitializeComponent();

            Text = ex.GetType().Name;
            errorTextBox.Text = String.Format("{0}\r\n\r\nStack trace:\r\n{1}", ex.Message, ex.StackTrace);
        }

        public static void ShowDialog(Exception ex)
        {
            new ErrorDialog(ex).ShowDialog();
        }
    }
}
