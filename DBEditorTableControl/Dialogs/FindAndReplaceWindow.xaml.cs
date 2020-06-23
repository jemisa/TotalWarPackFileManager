using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBTableControl
{
    /// <summary>
    /// Interaction logic for FindAndReplace.xaml
    /// </summary>
    public partial class FindAndReplaceWindow : Window
    {
        public string FindValue { get { return FindInputBox.Text; } }
        public string ReplaceValue { get { return ReplaceInputBox.Text; } }

        private FindReplaceMode currentMode;
        public FindReplaceMode CurrentMode
        {
            get { return currentMode; }
            set
            {
                currentMode = value;

                if (readOnly)
                {
                    currentMode = FindReplaceMode.FindMode;
                }

                Visibility newvisibility = System.Windows.Visibility.Visible;
                if (currentMode == FindReplaceMode.FindMode)
                {
                    newvisibility = System.Windows.Visibility.Hidden;

                    // Modify button toggle and which can be toggled.
                    FindToggle.IsChecked = true;
                    ReplaceToggle.IsChecked = false;
                }
                else
                {
                    FindToggle.IsChecked = false;
                    ReplaceToggle.IsChecked = true;
                }

                // Set visibility.
                ReplaceLabel.Visibility = newvisibility;
                ReplaceInputBox.Visibility = newvisibility;
                ReplaceStackPanel.Visibility = newvisibility;
                ReplaceButton.Visibility = newvisibility;
                ReplaceAllButton.Visibility = newvisibility;
            }
        }

        private bool readOnly;
        public bool ReadOnly
        {
            get { return readOnly; }
            set
            {
                readOnly = value;

                if (readOnly)
                {
                    CurrentMode = FindReplaceMode.FindMode;
                    ReplaceToggle.IsEnabled = false;
                }
                else
                {
                    ReplaceToggle.IsEnabled = true;
                }
            }
        }

        public event EventHandler FindNext;
        public event EventHandler FindAll;
        public event EventHandler Replace;
        public event EventHandler ReplaceAll;

        public FindAndReplaceWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) => SetFocus();

            CurrentMode = FindReplaceMode.FindMode;
        }

        public FindAndReplaceWindow(FindReplaceMode mode)
        {
            InitializeComponent();
            Loaded += (sender, e) => SetFocus();

            CurrentMode = mode;
        }

        private void SetFocus()
        {
            if (String.IsNullOrEmpty(FindValue))
            {
                FocusManager.SetFocusedElement(this, FindInputBox);
            }
        }

        public void UpdateFindText(string findme)
        {
            FindInputBox.Text = findme;
        }

        public void UpdateNumMatches(int numMatches)
        {

        }

        public void UpdateMatchesList(ICollection<KeyValuePair<string, object>> matches)
        {

        }

        private void FindNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindNext != null)
            {
                FindNext(this, null);
            }
        }

        private void FindAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (FindAll != null)
            {
                FindAll(this, null);
            }
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (Replace != null)
            {
                Replace(this, null);
            }
        }

        private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReplaceAll != null)
            {
                ReplaceAll(this, null);
            }
        }

        private void FindToggle_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == FindReplaceMode.ReplaceMode)
            {
                CurrentMode = FindReplaceMode.FindMode;
            }
            else
            {
                FindToggle.IsChecked = true;
            }
        }

        private void ReplaceToggle_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == FindReplaceMode.FindMode)
            {
                CurrentMode = FindReplaceMode.ReplaceMode;
            }
            else
            {
                ReplaceToggle.IsChecked = true;
            }
        }

        private void FindReplaceWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Intercept a close request, and simply hide the window instead.
            e.Cancel = true;

            this.Hide();
        }

        private void FindReplaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset the default button, since that seems to want to change on occasion.
            FindNextButton.IsDefault = true;
        }

        public enum FindReplaceMode
        {
            FindMode,
            ReplaceMode
        }
    }
}
