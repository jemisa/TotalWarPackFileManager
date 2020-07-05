using DBTableControl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DBEditorTableControl
{
    class FindReplaceHelper
    {
        DBTableControl.DBEditorTableControl _parentDbEdtiorTable;
        FindAndReplaceWindow _findReplaceWindow;

        public FindReplaceHelper(DBTableControl.DBEditorTableControl mainTable)
        {
            _parentDbEdtiorTable = mainTable;

            // Register for FindAndReplaceWindowEvents
            _findReplaceWindow = new FindAndReplaceWindow();
            _findReplaceWindow.FindNext += new EventHandler(findWindow_FindNext);
            _findReplaceWindow.FindAll += new EventHandler(findReplaceWindow_FindAll);
            _findReplaceWindow.Replace += new EventHandler(replaceWindow_Replace);
            _findReplaceWindow.ReplaceAll += new EventHandler(replaceWindow_ReplaceAll);

            // Enable keyboard interop for the findReplaceWindow, otherwise WinForms will intercept all keyboard input.
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(_findReplaceWindow);
        }

        public void ShowSearch()
        {
            if (_parentDbEdtiorTable.dbDataGrid.SelectedCells.Count == 1 && _parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item is DataRowView)
            {
                _findReplaceWindow.UpdateFindText(_parentDbEdtiorTable.CurrentTable.Rows[_parentDbEdtiorTable.dbDataGrid.Items.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item)]
                                                                  [(string)_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Column.Header].ToString());
            }

            _findReplaceWindow.CurrentMode = FindAndReplaceWindow.FindReplaceMode.FindMode;
            _findReplaceWindow.ReadOnly = _parentDbEdtiorTable.ReadOnly;
            _findReplaceWindow.Show();
        }

        public void ShowReplace()
        {
            _findReplaceWindow.CurrentMode = FindAndReplaceWindow.FindReplaceMode.ReplaceMode;
            _findReplaceWindow.Show();
        }

        public void dbDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Look for Ctrl-F, for Find shortcut.
            if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                ShowSearch();
            }

            // Look for Ctrl-H, for Replace shortcut.
            if (e.Key == Key.H && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && !_parentDbEdtiorTable.ReadOnly)
            {
                
            }

            // Look for F3, shortcut for Find Next.
            if (e.Key == Key.F3)
            {
                FindNext(_findReplaceWindow.FindValue);
            }
        }

        private void findWindow_FindNext(object sender, EventArgs e)
        {
            FindNext(_findReplaceWindow.FindValue);
        }

        private void findReplaceWindow_FindAll(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void replaceWindow_Replace(object sender, EventArgs e)
        {
            // If nothing is selected, then find something to replace first.
            if (_parentDbEdtiorTable.dbDataGrid.SelectedCells.Count == 0)
            {
                // If we fail to find a match, return.
                if (!FindNext(_findReplaceWindow.FindValue))
                {
                    return;
                }
            }

            // If nothing is STILL selected, then we found nothing to replace.
            // Or, if more than 1 cell is selected, we have a problem.
            if (_parentDbEdtiorTable.dbDataGrid.SelectedCells.Count == 0 || _parentDbEdtiorTable.dbDataGrid.SelectedCells.Count > 1)
            {
                return;
            }

            int rowindex = _parentDbEdtiorTable.dbDataGrid.Items.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item);
            int colindex = _parentDbEdtiorTable.dbDataGrid.Columns.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Column);

            while (_findReplaceWindow.ReplaceValue.Equals(_parentDbEdtiorTable.CurrentTable.Rows[rowindex][colindex].ToString()))
            {
                // If what is selected has already been replaced, move on to the next match, returning if we fail.
                if (!FindNext(_findReplaceWindow.FindValue))
                {
                    return;
                }

                // Update current coordinates.
                rowindex = _parentDbEdtiorTable.dbDataGrid.Items.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item);
                colindex = _parentDbEdtiorTable.dbDataGrid.Columns.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Column);
            }

            if (_findReplaceWindow.FindValue.Equals(_parentDbEdtiorTable.CurrentTable.Rows[rowindex][colindex].ToString()))
            {
                // Test for a combobox comlumn.
                if (_parentDbEdtiorTable.dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    if (ComboBoxColumnContainsValue((DataGridComboBoxColumn)_parentDbEdtiorTable.dbDataGrid.Columns[colindex], _findReplaceWindow.ReplaceValue))
                    {
                        // The value in the Replace field is not valid for this column, alert user and return.
                        MessageBox.Show(String.Format("The value '{0}', is not a valid value for Column '{1}'",
                                                      _findReplaceWindow.ReplaceValue,
                                                      (string)_parentDbEdtiorTable.dbDataGrid.Columns[colindex].Header));

                        return;
                    }
                }

                // Assign the value, and update the UI
                _parentDbEdtiorTable.CurrentTable.Rows[rowindex][colindex] = _findReplaceWindow.ReplaceValue;
                _parentDbEdtiorTable.RefreshCell(rowindex, colindex);
            }
        }

        private void replaceWindow_ReplaceAll(object sender, EventArgs e)
        {
            // Clear selection, so that FindNext() starts at the beginning of the table.
            _parentDbEdtiorTable.dbDataGrid.SelectedCells.Clear();

            int rowindex;
            int colindex;

            while (FindNext(_findReplaceWindow.FindValue))
            {
                // Update current coordinates.
                rowindex = _parentDbEdtiorTable.dbDataGrid.Items.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item);
                colindex = _parentDbEdtiorTable.dbDataGrid.Columns.IndexOf(_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Column);

                if (_parentDbEdtiorTable.dbDataGrid.Columns[colindex] is DataGridComboBoxColumn)
                {
                    if (ComboBoxColumnContainsValue((DataGridComboBoxColumn)_parentDbEdtiorTable.dbDataGrid.Columns[colindex], _findReplaceWindow.ReplaceValue))
                    {
                        // The value in the Replace field is not valid for this column, alert user and continue.
                        MessageBox.Show(String.Format("The value '{0}', is not a valid value for Column '{1}'",
                                                      _findReplaceWindow.ReplaceValue,
                                                      (string)_parentDbEdtiorTable.dbDataGrid.Columns[colindex].Header));
                        continue;
                    }
                }

                // Assign the value, and update the UI
                _parentDbEdtiorTable.CurrentTable.Rows[rowindex][colindex] = _findReplaceWindow.ReplaceValue;
                _parentDbEdtiorTable.RefreshCell(rowindex, colindex);
            }
        }

        private bool FindNext(string findthis)
        {
            if (String.IsNullOrEmpty(findthis))
            {
                MessageBox.Show("Nothing entered in Find bar!");
                return false;
            }

            // Set starting point at table upper left.
            int rowstartindex = 0;
            int colstartindex = 0;

            // If the user has a single cell selected, assume this as starting point.
            if (_parentDbEdtiorTable.dbDataGrid.SelectedCells.Count == 1)
            {
                rowstartindex = _parentDbEdtiorTable.CurrentTable.Rows.IndexOf((_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Item as DataRowView).Row);
                colstartindex = _parentDbEdtiorTable.CurrentTable.Columns.IndexOf((string)_parentDbEdtiorTable.dbDataGrid.SelectedCells.First().Column.Header);
            }

            bool foundmatch = false;
            bool atstart = true;
            for (int i = rowstartindex; i < _parentDbEdtiorTable.dbDataGrid.Items.Count; i++)
            {
                // Additional error checking on the visibleRows internal list.
                if (i >= _parentDbEdtiorTable._visibleRows.Count)
                {
                    _parentDbEdtiorTable.UpdateVisibleRows();
                }

                // Ignore the blank row, and any collapsed (filtered) rows.
                if (!(_parentDbEdtiorTable.dbDataGrid.Items[i] is DataRowView) || _parentDbEdtiorTable._visibleRows[i] == System.Windows.Visibility.Collapsed)
                {
                    continue;
                }

                for (int j = 0; j < _parentDbEdtiorTable.dbDataGrid.Columns.Count; j++)
                {
                    if (atstart)
                    {
                        j = colstartindex;
                        atstart = false;
                    }

                    // Skip current cell.
                    if (i == rowstartindex && j == colstartindex)
                    {
                        continue;
                    }

                    foundmatch = DBUtil.isMatch(_parentDbEdtiorTable.CurrentTable.Rows[i][j].ToString(), findthis);

                    if (foundmatch)
                    {
                        // Clears current selection for new selection.
                        _parentDbEdtiorTable.dbDataGrid.SelectedCells.Clear();
                        _parentDbEdtiorTable.SelectCell(i, j, true);
                        break;
                    }
                }

                if (foundmatch)
                {
                    break;
                }
            }

            if (!foundmatch)
            {
                MessageBox.Show("No More Matches Found.");
            }

            return foundmatch;
        }
        private bool ComboBoxColumnContainsValue(DataGridComboBoxColumn column, string tocheck)
        {
            if (column.ItemsSource.OfType<object>().Count(n => n.ToString().Equals(tocheck)) != 0)
            {
                return true;
            }

            return false;
        }
    }
}
