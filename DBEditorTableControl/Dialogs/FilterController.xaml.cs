using DBTableControl;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DBEditorTableControl.Dialogs
{
    /// <summary>
    /// Interaction logic for FilterController.xaml
    /// </summary>
    public partial class FilterController : UserControl
    {
        DBTableControl.DBEditorTableControl _parentDbController;
        public ObservableCollection<DBFilter> filterList = new ObservableCollection<DBFilter>();

        public FilterController()
        {
            InitializeComponent();
            filterListView.ItemsSource = filterList;
        }

        public void LoadFilters(DBTableControl.DBEditorTableControl parentDbController)
        {
            _parentDbController = parentDbController;
            filterList.Clear();
            _parentDbController._autofilterList.Clear();

            string tableName = parentDbController.EditedFile.CurrentType.Name;

            // If the saved config has not filters, skip.
            if (!_parentDbController._savedconfig.Filters.ContainsKey(tableName))
                return;

            // Load saved filters, attaching activation listeners for each one.
            foreach (DBFilter filter in _parentDbController._savedconfig.Filters[tableName])
            {
                // Always load filters as inactive.
                filter.IsActive = false;
                filter.FilterToggled += new EventHandler(filter_FilterToggled);
                filterList.Add(filter);
            }
        }

        void SaveFilters()
        {
            var tableName = _parentDbController.EditedFile.CurrentType.Name;
            if (_parentDbController._savedconfig.Filters.ContainsKey(tableName))
            {
                _parentDbController._savedconfig.Filters[tableName].Clear();
                _parentDbController._savedconfig.Filters[tableName].AddRange(filterList);
            }
            else
            {
                // Create a new list for the table.
                _parentDbController._savedconfig.Filters.Add(new DBTableControl.KeyValuePair<string, List<DBFilter>>(tableName, new List<DBFilter>(filterList)));
            }
            _parentDbController._savedconfig.Save();
        }

        private void addFilterButton_Click(object sender, RoutedEventArgs e)
        {
            ManageFiltersWindow filterWindow = new ManageFiltersWindow(filterList.Select(n => n.Name).ToList());
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(filterWindow);
            filterWindow.CurrentTable = _parentDbController.CurrentTable;
            filterWindow.Filter = new DBFilter();
            filterWindow.ShowDialog();

            if (filterWindow.DialogResult.Value)
            {
                DBFilter filter = filterWindow.Filter;
                // Attach event handler for checked/unchecked toggle.
                filter.FilterToggled += new EventHandler(filter_FilterToggled);
                // Only add the filter if the name is unique.
                filterList.Add(filter);

                SaveFilters();
            }
        }

        private void filter_FilterToggled(object sender, EventArgs e)
        {
            Refresh();
        }

        private void cloneFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (filterListView.SelectedItems.Count == 1)
            {
                Clone((DBFilter)filterListView.SelectedItem);
            }
        }

        private void filterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var buttonState = filterListView.SelectedItems.Count == 1;
            deleteFilterButton.IsEnabled = buttonState;
            editFilterButton.IsEnabled = buttonState;
            cloneFilterButton.IsEnabled = buttonState;
        }

        private void filterListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && filterListView.SelectedItems.Count == 1)
            {
                Delete((DBFilter)filterListView.SelectedItem);
            }
        }

        private void deleteFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (filterListView.SelectedItems.Count == 1)
            {
                Delete((DBFilter)filterListView.SelectedItem);
            }
        }

        private void filterListView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && filterListView.SelectedItems.Count == 1)
            {
                Edit((DBFilter)filterListView.SelectedItem);
            }
        }

        private void editFilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (filterListView.SelectedItems.Count == 1)
            {
                Edit((DBFilter)filterListView.SelectedItem);
            }
        }

        void Clone(DBFilter filter)
        {
            var index = 0;
            string clonePrefix = string.Format("{0}_clone_", filter.Name);
            foreach (var item in filterList)
            {
                var indexOfStr = item.Name.IndexOf(clonePrefix);
                if (indexOfStr == 0)
                {
                    var numberStr = item.Name.Substring(clonePrefix.Count(), item.Name.Length - clonePrefix.Count());
                    var isLastPartNumbers = int.TryParse(numberStr, out var number);
                    if (isLastPartNumbers && number > index)
                    {
                        index = number;
                    }
                }
            }

            index++;

            DBFilter newfilter = new DBFilter
            {
                Name = $"{clonePrefix}{index}",
                IsActive = false,
                ApplyToColumn = filter.ApplyToColumn,
                MatchMode = filter.MatchMode,
                FilterValue = filter.FilterValue
            };

            newfilter.FilterToggled += new EventHandler(filter_FilterToggled);
            filterList.Add(newfilter);

            SaveFilters();
        }

        void Edit(DBFilter filter)
        {
            ManageFiltersWindow filterWindow = new ManageFiltersWindow(filterList.Select(n => n.Name).ToList());
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(filterWindow);
            filterWindow.CurrentTable = _parentDbController.CurrentTable;

            if (filterListView.SelectedItems.Count == 1)
            {
                filterWindow.Filter = filter;
            }

            filterWindow.ShowDialog();

            if (filterWindow.DialogResult.Value)
            {
                DBFilter editedfilter = filterWindow.Filter;
                // Save the edited filter data.
                int index = filterList.IndexOf((DBFilter)filterListView.SelectedItem);
                filterList[index].Name = editedfilter.Name;
                filterList[index].ApplyToColumn = editedfilter.ApplyToColumn;
                filterList[index].FilterValue = editedfilter.FilterValue;
                filterList[index].MatchMode = editedfilter.MatchMode;

                if (editedfilter.IsActive)
                {
                    _parentDbController.UpdateVisibleRows();
                    _parentDbController.dbDataGrid.Items.Refresh();
                }

                SaveFilters();
            }
        }

        void Delete(DBFilter filter)
        {
            filterList.RemoveAt(filterList.IndexOf(filter));
            if (filter.IsActive)
            {
                Refresh();
            }

            SaveFilters();
        }
    

        void Refresh()
        {
            _parentDbController.UpdateVisibleRows();
            _parentDbController.dbDataGrid.Items.Refresh();
        }

       
    }
}
