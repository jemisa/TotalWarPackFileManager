using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static PackFileManager.PackFileManagerSettings;

namespace PackFileManager.Dialogs.Settings
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        SettingsFormInput _settingsInput;
        List<GameTypeEnum> _changedGameDirectories = new List<GameTypeEnum>();

        List<CustomFileExtentionHighlightsMapping> _updatedCustomFileExtentionHighlightsMappings;
        bool _modDirectoryUpdated = false;
        bool _defaultGameChanged = false;
        
        public SettingsControl()
        {
            InitializeComponent();
            Loaded += SettingsControl_Loaded;
        }

        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This function crashes winforms editing in visual studio, so return early if that is the case
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            var allTextBoxes = FindVisualChildren<TextBox>(GetVisualChild(0));

            foreach (var game in Game.Games)
            {
                var gameKey = game.Id;
                var gameEnum = game.GameType;
                var gameDir = PackFileManagerSettingService.CurrentSettings.GameDirectories.FirstOrDefault(x => x.Game == gameKey);
                if (gameDir != null && !String.IsNullOrWhiteSpace(gameDir.Path))
                {
                    var matchingTextBox = allTextBoxes
                        .Where(x=>x.Tag != null)
                        .FirstOrDefault(x => (GameTypeEnum)x.Tag == gameEnum);
                    if (matchingTextBox != null)
                        matchingTextBox.Text = gameDir.Path;
                }
            }

            var defaultGame = PackFileManagerSettingService.CurrentSettings.CurrentGame;
            foreach (var currentGame in Game.Games)
            {
                var idx = _defaultGameComboBox.Items.Add(currentGame.Id);
                if (defaultGame != GameTypeEnum.Unknown)
                {
                    //var gameEnum = Game.GetByEnum(defaultGame).GameType;
                    if(defaultGame == currentGame.GameType)
                        _defaultGameComboBox.SelectedIndex = idx;
                }
                
            }
        }

        public void Configure(SettingsFormInput input)
        {
            _settingsInput = input;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void BrowseGameDirButtonClicked(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            if (button == null)
                return;

            var tagValue = (GameTypeEnum)button.Tag;
            var textBoxes = FindVisualChildren<TextBox>(this.GetVisualChild(0));
            var matchingTextBox = textBoxes
                .Where(x=>x.Tag != null)
                .FirstOrDefault(x => (GameTypeEnum)x.Tag == tagValue);
            if (matchingTextBox == null)
                return;

            using (var browserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(matchingTextBox.Text))
                    browserDialog.SelectedPath = matchingTextBox.Text;

                var res = browserDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    matchingTextBox.Text = browserDialog.SelectedPath;
                    if (_changedGameDirectories.Contains(tagValue))
                        return;
                    _changedGameDirectories.Add(tagValue);
                }
            }
        }

        private void BrowseModDirButtonClicked(object sender, RoutedEventArgs e)
        {
            using (var browserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (Directory.Exists(_modDirectoryTextBox.Text))
                    browserDialog.SelectedPath = _modDirectoryTextBox.Text;

                var res = browserDialog.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    _modDirectoryTextBox.Text = browserDialog.SelectedPath;
                    _modDirectoryUpdated = true;
                }
            }
        }

        private void OnShowCustomHighlightsButtonClick(object sender, RoutedEventArgs e)
        {
            var x = new FileExtentionSyntaxMappingForm(_settingsInput, PackFileManagerSettingService.CurrentSettings.CustomFileExtentionHighlightsMappings);
            var res = x.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                _updatedCustomFileExtentionHighlightsMappings = x.GetMappings();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var enumValue = (GameTypeEnum)textBox.Tag;
            if (_changedGameDirectories.Contains(enumValue))
                return;
            _changedGameDirectories.Add(enumValue);
        }

        private void OnModKeyUp(object sender, KeyEventArgs e)
        {
            _modDirectoryUpdated = true;
        }

        void ValidateBeforeSave()
        {
            var textBoxes = FindVisualChildren<TextBox>(this.GetVisualChild(0));

            // Game dirs
            foreach (var item in _changedGameDirectories)
            {
                var textBox = textBoxes
                    .Where(x => x.Tag != null)
                    .FirstOrDefault(x => (GameTypeEnum)x.Tag == item);

                if (Directory.Exists(textBox.Text) == false)
                {
                    MessageBox.Show(textBox.Text + " is not a valid file directory");
                    return;
                }
            }

            // Mod Dir
            if (_modDirectoryUpdated)
            {
                if (Directory.Exists(_modDirectoryTextBox.Text) == false)
                {
                    MessageBox.Show(_modDirectoryTextBox.Text + " is not a valid file directory");
                    return;
                }
            }
        }

        public void Save()
        {
            ValidateBeforeSave();

            var textBoxes = FindVisualChildren<TextBox>(this.GetVisualChild(0));
            foreach (var item in _changedGameDirectories)
            {
                var textBox = textBoxes
                    .Where(x => x.Tag != null)
                    .FirstOrDefault(x => (GameTypeEnum)x.Tag == item);

                var gameObj = Game.Games.FirstOrDefault(x => x.GameType == item);

                var savedPathInstance= PackFileManagerSettingService.CurrentSettings.GameDirectories.FirstOrDefault(x => x.Game == gameObj.Id);
                savedPathInstance.Path = textBox.Text;
            }

            if (_modDirectoryUpdated)
                PackFileManagerSettingService.CurrentSettings.MyModDirectory = _modDirectoryTextBox.Text;

            if (_updatedCustomFileExtentionHighlightsMappings != null)
                PackFileManagerSettingService.CurrentSettings.CustomFileExtentionHighlightsMappings = _updatedCustomFileExtentionHighlightsMappings;

            if (_defaultGameChanged)
            {
                var id = _defaultGameComboBox.SelectedItem as string;
                if (id != null)
                {
                    var game = Game.ById(id);
                    PackFileManagerSettingService.CurrentSettings.CurrentGame = game.GameType;
                }
            }

            PackFileManagerSettingService.Save();
        }

        private void OnDefaultGameChanged(object sender, SelectionChangedEventArgs e)
        {
            _defaultGameChanged = true;
        }
    }
}
