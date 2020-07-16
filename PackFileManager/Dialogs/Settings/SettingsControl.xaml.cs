using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PackFileManager.PackFileManagerSettings;

namespace PackFileManager.Dialogs.Settings
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        SettingsFormInput _settingsInput;

        List<CustomFileExtentionHighlightsMapping> UpdatedCustomFileExtentionHighlightsMapping;

        public SettingsControl()
        {
            InitializeComponent();
        }

        public void Configure(SettingsFormInput input)
        {
            _settingsInput = input;
            var x = new FileExtentionSyntaxMappingForm(_settingsInput, PackFileManagerSettingManager.CurrentSettings.CustomFileExtentionHighlightsMappings);
            var res = x.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                UpdatedCustomFileExtentionHighlightsMapping = x.GetMappings();
                PackFileManagerSettingManager.CurrentSettings.CustomFileExtentionHighlightsMappings = UpdatedCustomFileExtentionHighlightsMapping;
                PackFileManagerSettingManager.Save();
            }
        }

        private void BrowseGameDirButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseModDirButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var x = new FileExtentionSyntaxMappingForm(_settingsInput, PackFileManagerSettingManager.CurrentSettings.CustomFileExtentionHighlightsMappings);
            var res = x.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                UpdatedCustomFileExtentionHighlightsMapping = x.GetMappings();
                PackFileManagerSettingManager.CurrentSettings.CustomFileExtentionHighlightsMappings = UpdatedCustomFileExtentionHighlightsMapping;
                PackFileManagerSettingManager.Save();
            }
        }
    }
}
