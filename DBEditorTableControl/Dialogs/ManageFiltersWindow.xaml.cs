using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for ManageFiltersWindow.xaml
    /// </summary>
    public partial class ManageFiltersWindow : Window
    {
        DBFilter filter;
        public DBFilter Filter 
        { 
            get { return filter; } 
            set 
            { 
                filter = value;

                filternameTextBox.Text = filter.Name;
                columnComboBox.SelectedIndex = columnheaders.IndexOf(filter.ApplyToColumn);

                if (filter.MatchMode == MatchType.Exact)
                {
                    matchtypeComboBox.SelectedIndex = 0;
                    filtervalueComboBox.SelectedIndex = columnvalues.IndexOf(filter.FilterValue);
                }
                else if (filter.MatchMode == MatchType.Partial)
                {
                    matchtypeComboBox.SelectedIndex = 1;
                    filtervalueComboBox.Text = filter.FilterValue;
                }
                else if (filter.MatchMode == MatchType.Regex)
                {
                    matchtypeComboBox.SelectedIndex = 2;
                    filtervalueTextBox.Text = filter.FilterValue;
                }
                
                originalname = filter.Name;
            } 
        }

        DataTable currenttable;
        public DataTable CurrentTable 
        {
            get { return currenttable; } 
            set 
            {
                currenttable = value;

                columnComboBox.ItemsSource = null;
                columnheaders.Clear();
                foreach (DataColumn column in currenttable.Columns)
                {
                    columnheaders.Add(column.ColumnName);
                }
                columnComboBox.ItemsSource = columnheaders;
            } 
        }

        List<string> filternames;
        List<string> columnheaders;
        List<string> columnvalues;
        List<string> matchtypes;
        string originalname;

        //public event EventHandler<FilterSavedEventArgs> FilterSaved;

        public ManageFiltersWindow(List<string> currentfilternames)
        {
            InitializeComponent();

            double width = this.Width;
            double actualwidth = this.ActualWidth;
            double height = this.Height;
            double actualheight = this.ActualHeight;

            columnheaders = new List<string>();
            columnvalues = new List<string>();
            matchtypes = new List<string>();

            columnComboBox.ItemsSource = columnheaders;
            filtervalueComboBox.ItemsSource = columnvalues;

            matchtypes.Add("Exact");
            matchtypes.Add("Partial");
            matchtypes.Add("Regex");
            matchtypeComboBox.ItemsSource = matchtypes;
            matchtypeComboBox.SelectedIndex = 0;

            filternames = currentfilternames;
        }

        private void RepopulateColumnValues()
        {
            // Only update column values when a column is actually selected.
            if (columnComboBox.SelectedItem != null)
            {
                filtervalueComboBox.ItemsSource = null;
                columnvalues.Clear();
                foreach (object value in currenttable.Columns[columnComboBox.SelectedItem.ToString()].GetItemArray())
                {
                    if (!columnvalues.Contains(value.ToString()))
                    {
                        columnvalues.Add(value.ToString());
                    }
                }

                filtervalueComboBox.ItemsSource = columnvalues;
            }
        }

        private void columnComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RepopulateColumnValues();
        }

        private void matchtypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (matchtypeComboBox.SelectedValue == null)
            {
                return;
            }
            if (matchtypeComboBox.SelectedValue.Equals("Exact"))
            {
                filtervalueComboBox.IsEditable = false;
                filtervalueComboBox.Visibility = System.Windows.Visibility.Visible;
                filtervalueTextBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (matchtypeComboBox.SelectedValue.Equals("Partial"))
            {
                filtervalueComboBox.IsEditable = true;
                filtervalueComboBox.Visibility = System.Windows.Visibility.Visible;
                filtervalueTextBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (matchtypeComboBox.SelectedValue.Equals("Regex"))
            {
                filtervalueComboBox.Visibility = System.Windows.Visibility.Collapsed;
                filtervalueTextBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            bool haserrors = false;
            // First, test the name, so we don't wind up with duplicate filter names.
            if (filternames.Contains(filternameTextBox.Text) && !filternameTextBox.Text.Equals(originalname))
            {
                filternameTextBox.Background = new SolidColorBrush(Colors.LightPink);
                filternameTextBox.ToolTip = String.Format("Cannot save filter: {0} is the name of another filter, please pick another.", filternameTextBox.Text);
                haserrors = true;
            }
            else if (String.IsNullOrEmpty(filternameTextBox.Text))
            {
                filternameTextBox.Background = new SolidColorBrush(Colors.LightPink);
                filternameTextBox.ToolTip = "Cannot save filter: Please enter a name for this filter.";
                haserrors = true;
            }

            if (columnComboBox.SelectedValue == null || String.IsNullOrEmpty(columnComboBox.SelectedValue.ToString()))
            {
                columnComboBox.Background = new SolidColorBrush(Colors.LightPink);
                columnComboBox.ToolTip = "Cannot save filter: Please specify a column for this filter.";
                haserrors = true;
            }

            if (filtervalueComboBox.Visibility == System.Windows.Visibility.Visible)
            {
                if (String.IsNullOrEmpty(filtervalueComboBox.Text))
                {
                    filtervalueComboBox.Background = new SolidColorBrush(Colors.LightPink);
                    filtervalueComboBox.ToolTip = "Cannot save filter: Please enter a value for the filter.";
                    haserrors = true;
                }
            }
            else if (filtervalueTextBox.Visibility == System.Windows.Visibility.Visible)
            {
                if (String.IsNullOrEmpty(filtervalueTextBox.Text) || filtervalueTextBox.Text == null)
                {
                    filtervalueTextBox.Background = new SolidColorBrush(Colors.LightPink);
                    filtervalueTextBox.ToolTip = "Cannot save filter: Invalid Regex string.";
                    haserrors = true;
                }
                else
                {
                    try
                    {
                        Regex testregex = new Regex(filtervalueTextBox.Text);
                    }
                    catch
                    {
                        filtervalueTextBox.Background = new SolidColorBrush(Colors.LightPink);
                        filtervalueTextBox.ToolTip = "Cannot save filter: Invalid Regex string.";
                        haserrors = true;
                    }
                }
            }
            
            if(!haserrors)
            {
                filter.Name = filternameTextBox.Text;
                filter.ApplyToColumn = columnComboBox.SelectedValue.ToString();

                if (matchtypeComboBox.SelectedValue.ToString().Equals("Exact"))
                {
                    filter.MatchMode = MatchType.Exact;
                    filter.FilterValue = filtervalueComboBox.Text;
                }
                else if (matchtypeComboBox.SelectedValue.ToString().Equals("Partial"))
                {
                    filter.MatchMode = MatchType.Partial;
                    filter.FilterValue = filtervalueComboBox.Text;
                }
                else if (matchtypeComboBox.SelectedValue.ToString().Equals("Regex"))
                {
                    filter.MatchMode = MatchType.Regex;
                    filter.FilterValue = filtervalueTextBox.Text;
                }

                this.DialogResult = true;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void filterListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            /*
            if (sender is ListViewItem && e.LeftButton == MouseButtonState.Pressed)
            {
                // Drag drop code, TODO: differentiate between a drag and a click some how.
                ListViewItem draggedItem = sender as ListViewItem;
                //DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                //draggedItem.IsSelected = true;
            }
             */
        }

        private void filterListView_Drop(object sender, DragEventArgs e)
        {
            /*
            DBFilter droppedData = e.Data.GetData(typeof(DBFilter)) as DBFilter;
            DBFilter target = ((ListBoxItem)(sender)).DataContext as DBFilter;

            // Ignore request until an actual drop is requested.
            if (droppedData == target)
            {
                return;
            }

            int removedIdx = filterListView.Items.IndexOf(droppedData);
            int targetIdx = filterListView.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                filterList.Insert(targetIdx + 1, droppedData);
                filterList.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (filterList.Count + 1 > remIdx)
                {
                    filterList.Insert(targetIdx, droppedData);
                    filterList.RemoveAt(remIdx);
                }
            }
             */
        }
    }

    public class FilterSavedEventArgs : EventArgs
    {
        public string originalfiltername;

        public FilterSavedEventArgs() : base()
        {
            
        }

        public FilterSavedEventArgs(string oldname) : base()
        {
            originalfiltername = oldname;
        }
    }
}
