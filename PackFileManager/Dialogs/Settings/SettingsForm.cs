using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PackFileManager.Dialogs.Settings
{
    public partial class SettingsForm : Form
    {
        public SettingsForm(SettingsFormInput settings)
        {
            InitializeComponent();
            this.settingsControl1.Configure(settings);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            settingsControl1.Save();
        }
    }
}
