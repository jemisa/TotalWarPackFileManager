using CommonDialogs;
using DBTableControl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DBEditorTableControl
{
    class ContextMenuHelper
    {
        public static void Open(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            ContextMenu currentmenu = (ContextMenu)sender;
            DataGridColumn col = (currentmenu.PlacementTarget as DataGridColumnHeader).Column;

            string colname = (string)col.Header;
            var currentColumn = mainTable.CurrentTable.Columns.IndexOf(colname);
            var typeCode = mainTable.EditedFile.CurrentType.Fields[currentColumn].TypeCode;
            Type columntype = DBTableControl.DBEditorTableControl.GetTypeFromCode(typeCode); ;
            
            foreach (MenuItem item in currentmenu.Items.OfType<MenuItem>())
            {
                // Enable/Disable Remove Sorting item based on if the column is actually sorted or not.
                if (item.Header.Equals("Remove Sorting"))
                {
                    item.IsEnabled = col.SortDirection != null;
                }

                // Enable/Disable Apply expression and renumber based on column type.
                if (item.Header.Equals("Apply Expression") || item.Header.Equals("Renumber Cells"))
                {
                    if (!col.IsReadOnly && (columntype.Name.Equals("Single") || columntype.Name.Equals("Int32") || columntype.Name.Equals("Int16")))
                    {
                        item.IsEnabled = true;
                    }
                    else
                    {
                        item.IsEnabled = false;
                    }
                }

                // Enable.Disable Mass edit, limiting it to string columns only.
                if (item.Header.Equals("Mass Edit"))
                {
                    var state = !mainTable.ReadOnly && typeCode == TypeCode.String;
                    item.IsEnabled = state;
                }
            }
        }


        public static void SelectColumn(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            var dbDataGrid = mainTable.dbDataGrid;

            for (int i = 0; i < dbDataGrid.Items.Count; i++)
            {
                // Test if the cell is already contained in SelectedCells
                DataGridCellInfo cellinfo = new DataGridCellInfo(dbDataGrid.Items[i], col);
                if (!dbDataGrid.SelectedCells.Contains(cellinfo))
                {
                    dbDataGrid.SelectedCells.Add(cellinfo);
                }
            }
        }

        public static void RemoveSorting(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            col.SortDirection = null;
            mainTable.Refresh();
        }
        
        public static void ColumnApplyExpression(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;

            ApplyExpressionWindow getexpwindow = new ApplyExpressionWindow();
            getexpwindow.ShowDialog();

            if (getexpwindow.DialogResult != null && (bool)getexpwindow.DialogResult)
            {
                for (int i = 0; i < mainTable.CurrentTable.Rows.Count; i++)
                {
                    // Skip any filtered rows, or any rows with an error in the column we are computing.
                    mainTable.UpdateVisibleRows();
                    if (mainTable._visibleRows[i] == System.Windows.Visibility.Collapsed ||
                        mainTable._errorList.Count(n => n.RowIndex == i && n.ColumnName.Equals(colname)) > 0)
                    {
                        continue;
                    }

                    // Grab the given expression, modifying it for each cell.
                    string expression = getexpwindow.EnteredExpression.Replace("x", string.Format("{0}", mainTable.CurrentTable.Rows[i][colname]));
                    object newvalue = mainTable.CurrentTable.Compute(expression, "");
                    int colindex = mainTable.CurrentTable.Columns.IndexOf(colname);

                    // For integer based columns, do a round first if necessary.
                    if (mainTable.EditedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int32 ||
                        mainTable.EditedFile.CurrentType.Fields[colindex].TypeCode == TypeCode.Int16)
                    {
                        int newintvalue;
                        if (!Int32.TryParse(newvalue.ToString(), out newintvalue))
                        {
                            double tempvalue = Double.Parse(newvalue.ToString());
                            tempvalue = Math.Round(tempvalue, 0);
                            newintvalue = (int)tempvalue;
                        }

                        newvalue = newintvalue;
                    }
                    mainTable.CurrentTable.Rows[i][colname] = newvalue;
                }
            }

            mainTable.RefreshColumn(mainTable.dbDataGrid.Columns.IndexOf(col));
        }
        
        public static void ColumnMassEdit(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;

            InputBox stringeditbox = new InputBox();
            stringeditbox.Input = "{cell}";
            stringeditbox.ShowDialog();

            if (stringeditbox.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int datacolindex = mainTable.CurrentTable.Columns.IndexOf(colname);
                for (int i = 0; i < mainTable.CurrentTable.Rows.Count; i++)
                {
                    // Make sure our visible rows are up to date and skip any currently filtered rows.
                    if (mainTable._visibleRows.Count <= mainTable.CurrentTable.Rows.Count)
                    {
                        mainTable.UpdateVisibleRows();
                    }
                    if (mainTable._visibleRows[i] == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    mainTable.CurrentTable.Rows[i][datacolindex] = stringeditbox.Input.Replace("{cell}", mainTable.CurrentTable.Rows[i][datacolindex].ToString());

                    // Refresh the cell in the UI, using its visual coordinates.
                    mainTable.RefreshCell(mainTable.dbDataGrid.Items.IndexOf(mainTable.CurrentTable.DefaultView[i]), mainTable.dbDataGrid.Columns.IndexOf(col));
                }
            }
        }
        
        public static void RenumberMenuItem(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            // Get the column index this context menu was called from.
            DataGridColumn col = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as DataGridColumnHeader).Column;
            string colname = (string)col.Header;
            List<int> visualroworder = new List<int>();

            InputBox renumberInputBox = new InputBox { Text = "Re-Number from", Input = "1" };
            if (renumberInputBox.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    int parsedNumber = int.Parse(renumberInputBox.Input);
                    for (int i = 0; i < mainTable.dbDataGrid.Items.Count; i++)
                    {
                        // Skip any non DataRowView, which should only be the blank row at the bottom.
                        if (!(mainTable.dbDataGrid.Items[i] is DataRowView))
                        {
                            continue;
                        }
                        // Store the data row index associated with the current visual row to account for column sorting.
                        visualroworder.Add(mainTable.CurrentTable.Rows.IndexOf((mainTable.dbDataGrid.Items[i] as DataRowView).Row));
                    }

                    // Now that we have a set order, we can assign values.
                    for (int i = 0; i < visualroworder.Count; i++)
                    {
                        mainTable.CurrentTable.Rows[visualroworder[i]][colname] = parsedNumber + i;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Could not apply values: {0}", ex.Message), "You fail!");
                }
            }

            mainTable.RefreshColumn(mainTable.dbDataGrid.Columns.IndexOf(col));
        }

        
        public static void EditVisibleListMenuItem(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            var hiddencolumnslisteditor = ColumnVisiblityHelper.Show(mainTable);
            System.Windows.Forms.DialogResult result = hiddencolumnslisteditor.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                mainTable._hiddenColumns.Clear();
                mainTable._hiddenColumns = hiddencolumnslisteditor.RightList;

                foreach (DataColumn column in mainTable.CurrentTable.Columns)
                {
                    if (hiddencolumnslisteditor.LeftList.Contains(column.ColumnName))
                    {
                        column.ExtendedProperties["Hidden"] = false;
                        mainTable.dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        column.ExtendedProperties["Hidden"] = true;

                        if (mainTable.ShowAllColumns)
                        {
                            mainTable.dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            mainTable.dbDataGrid.Columns[column.Ordinal].Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }

                mainTable.UpdateConfig();

                if (mainTable.ShowAllColumns)
                {
                    mainTable.Refresh();
                }
            }
        }
        
        public static void ClearTableHiddenMenuItem(DBTableControl.DBEditorTableControl mainTable)
        {
            foreach (DataGridColumn column in mainTable.dbDataGrid.Columns)
            {
                DataColumn datacolumn = mainTable.CurrentTable.Columns[(string)column.Header];

                if (!datacolumn.ExtendedProperties.ContainsKey("Hidden"))
                {
                    datacolumn.ExtendedProperties.Add("Hidden", false);
                }
                datacolumn.ExtendedProperties["Hidden"] = false;

                column.Visibility = Visibility.Visible;
            }

            // Clear the internal hidden columns list.
            mainTable._hiddenColumns.Clear();
            mainTable.UpdateConfig();

            if (mainTable.ShowAllColumns)
            {
                mainTable.Refresh();
            }
        }
        
        public static void ClearAllHiddenMenuItem(DBTableControl.DBEditorTableControl mainTable)
        {
            //Prompt for confirmation.
            string text = "Are you sure you want to clear all saved hidden column information?";
            string caption = "Clear Confirmation";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage image = MessageBoxImage.Question;

            MessageBoxResult result = MessageBox.Show(text, caption, button, image);

            if (result == MessageBoxResult.Yes)
            {
                // Clear internal list.
                mainTable._hiddenColumns.Clear();

                // Clear saved list.
                mainTable._savedconfig.HiddenColumns.Clear();
            }
            mainTable.UpdateConfig();

            if (mainTable.ShowAllColumns)
            {
                mainTable.Refresh();
            }
        }
        
        public static void RowHeaderContextMenu(object sender)
        {
            if (sender is ContextMenu)
            {
                bool insertrowsavailable = true;
                if ((sender as ContextMenu).DataContext is DataRowView)
                {
                    insertrowsavailable = true;
                }

                foreach (MenuItem item in (sender as ContextMenu).Items)
                {
                    string itemheader = item.Header.ToString();
                    if (itemheader.Equals("Insert Row") || itemheader.Equals("Insert Multiple Rows"))
                    {
                        item.IsEnabled = insertrowsavailable;
                    }
                }
            }
        }
        
        public static void RowHeaderInsertRow(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            if (!mainTable.ReadOnly && sender is MenuItem)
            {
                // Double check that whant triggered the event is what we expect.
                if ((sender as MenuItem).DataContext is DataRowView)
                {
                    // Determine visual and data index of calling row.
                    int datarowindex = mainTable.dbDataGrid.Items.IndexOf(((sender as MenuItem).DataContext as DataRowView));
                    mainTable.InsertRow(datarowindex);
                }
                else
                {
                    // We'll end up here if the user is attempting to insert a row in front of the blank row, so we will simply add a new row
                    // at the end of the table, still we'll use a try block in case something unforseen happens.
                    try
                    {
                        mainTable.InsertRow();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        ErrorDialog.ShowDialog(ex);
#endif
                    }
                }
            }
        }

        public static void RowHeaderInsertManyRows(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            if (!mainTable.ReadOnly && sender is MenuItem)
            {
                // Request how many rows should be inserted from the user.
                InputBox insertrowsInputBox = new InputBox();
                insertrowsInputBox.ShowDialog();

                // Double check that whant triggered the event is what we expect.
                if (insertrowsInputBox.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    // Determine how many rows the user wants to add.
                    int numrows = 0;
                    try
                    {
                        numrows = int.Parse(insertrowsInputBox.Input);
                    }
                    catch (Exception ex)
                    {
                        if (ex is FormatException)
                        {
                            MessageBox.Show(String.Format("Input: {0}, is not a valid number of rows, please enter a whole number.", insertrowsInputBox.Input));
                        }
                        else
                        {
#if DEBUG
                            ErrorDialog.ShowDialog(ex);
#endif
                        }
                    }

                    for (int i = 0; i < numrows; i++)
                    {
                        DataRow newrow = mainTable.CurrentTable.NewRow();
                        if ((sender as MenuItem).DataContext is DataRowView)
                        {
                            mainTable.InsertRow(i);
                        }
                        else
                        {
                            try
                            {
                                // If the blank row is calling us, add to the end of the table.
                                mainTable.InsertRow();
                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                ErrorDialog.ShowDialog(ex);
#endif
                            }
                        }
                    }

                    mainTable.UpdateVisibleRows();
                    mainTable.dbDataGrid.Items.Refresh();
                }
            }
        }
        
        public static void DataGridContextMenu(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            bool cellsselected = false;

            if (mainTable.dbDataGrid.SelectedCells.Count > 0)
            {
                cellsselected = true;
            }

            ContextMenu menu = (ContextMenu)sender;

            foreach (MenuItem item in menu.Items.OfType<MenuItem>())
            {
                if (item.Header.Equals("Copy"))
                {
                    item.IsEnabled = cellsselected;
                }
                else if (item.Header.Equals("Paste"))
                {
                    if (mainTable.ReadOnly)
                    {
                        item.IsEnabled = false;
                    }
                    else
                    {
                        item.IsEnabled = cellsselected;
                    }
                }
                else if (item.Header.Equals("Apply Expression to Selected Cells"))
                {
                    item.IsEnabled = cellsselected;
                    if (cellsselected)
                    {
                        Type columntype;
                        foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells)
                        {
                            columntype = DBTableControl.DBEditorTableControl.GetTypeFromCode(mainTable.EditedFile.CurrentType.Fields[mainTable.CurrentTable.Columns.IndexOf((string)cellinfo.Column.Header)].TypeCode);
                            if (mainTable.ReadOnly || !(columntype.Name.Equals("Single") || columntype.Name.Equals("Int32") || columntype.Name.Equals("Int16")))
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }
                else if (item.Header.Equals("Mass Edit String Cells"))
                {
                    item.IsEnabled = cellsselected;
                    if (cellsselected)
                    {
                        foreach (int colindex in mainTable.dbDataGrid.SelectedCells.Select(n => mainTable.CurrentTable.Columns[(string)n.Column.Header].Ordinal).Distinct())
                        {
                            if (mainTable.EditedFile.CurrentType.Fields[colindex].TypeCode != TypeCode.String)
                            {
                                item.IsEnabled = false;
                                break;
                            }
                        }
                    }
                }
                else if (item.Header.Equals("Revert Cell to Original Value"))
                {
                    if (mainTable.ReadOnly)
                    {
                        item.IsEnabled = false;
                    }
                    else
                    {
                        item.IsEnabled = cellsselected;
                    }
                }
            }
        }
        
        public static void DataGridCopyMenuItem()
        {
            // Programmatically send a copy shortcut key event.
            System.Windows.Forms.SendKeys.Send("^c");
        }

        public static void DataGridPasteMenuItem()
        {
            // Programmatically send a paste shortcut key event.
            System.Windows.Forms.SendKeys.Send("^v");
        }
        
        public static void DataGridApplyExpressionMenuItem(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            ApplyExpressionWindow getexpwindow = new ApplyExpressionWindow();
            getexpwindow.ShowDialog();

            if (getexpwindow.DialogResult != null && (bool)getexpwindow.DialogResult)
            {
                foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells)
                {
                    // Determine current cells indecies, row and column
                    int columnindex = mainTable.CurrentTable.Columns.IndexOf((string)cellinfo.Column.Header);
                    int rowindex = mainTable.CurrentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);

                    // Skip any filtered rows, or any rows with an error in the column we are computing.
                    mainTable.UpdateVisibleRows();
                    if (mainTable._visibleRows[rowindex] == System.Windows.Visibility.Collapsed ||
                        mainTable._errorList.Count(n => n.RowIndex == rowindex && n.ColumnName.Equals((string)cellinfo.Column.Header)) > 0)
                    {
                        continue;
                    }

                    // Get the expression, replacing x for the current cell's value.
                    string expression = getexpwindow.EnteredExpression.Replace("x", string.Format("{0}", mainTable.CurrentTable.Rows[rowindex][columnindex]));

                    // Compute spits out the new value after the current value is applied to the expression given.
                    object newvalue = mainTable.CurrentTable.Compute(expression, "");

                    // For integer based columns, do a round first if necessary.
                    if (mainTable.EditedFile.CurrentType.Fields[columnindex].TypeCode == TypeCode.Int32 ||
                        mainTable.EditedFile.CurrentType.Fields[columnindex].TypeCode == TypeCode.Int16)
                    {
                        int newintvalue;
                        if (!Int32.TryParse(newvalue.ToString(), out newintvalue))
                        {
                            double tempvalue = Double.Parse(newvalue.ToString());
                            tempvalue = Math.Round(tempvalue, 0);
                            newintvalue = (int)tempvalue;
                        }

                        newvalue = newintvalue;
                    }
                    mainTable.CurrentTable.Rows[rowindex][columnindex] = newvalue;

                    // Refresh the cell in the UI, using its visual coordinates.
                    mainTable.RefreshCell(mainTable.dbDataGrid.Items.IndexOf(cellinfo.Item), mainTable.dbDataGrid.Columns.IndexOf(cellinfo.Column));
                }
            }
        }
        
        public static void DataGridMassEditStringsMenuItem(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            InputBox stringeditbox = new InputBox();
            stringeditbox.Input = "{cell}";
            stringeditbox.ShowDialog();

            if (stringeditbox.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                int datarowindex = -1;
                int datacolindex = -1;
                foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells.Where(n => n.Item is DataRowView))
                {
                    datarowindex = mainTable.CurrentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);
                    datacolindex = mainTable.CurrentTable.Columns.IndexOf((string)cellinfo.Column.Header);

                    // Make sure our visible rows are up to date and skip any currently filtered rows.
                    if (mainTable._visibleRows.Count <= mainTable.CurrentTable.Rows.Count)
                    {
                        mainTable.UpdateVisibleRows();
                    }
                    if (mainTable._visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    mainTable.CurrentTable.Rows[datarowindex][datacolindex] = stringeditbox.Input.Replace("{cell}", mainTable.CurrentTable.Rows[datarowindex][datacolindex].ToString());

                    // Refresh the cell in the UI, using its visual coordinates.
                    mainTable.RefreshCell(mainTable.dbDataGrid.Items.IndexOf(cellinfo.Item), mainTable.dbDataGrid.Columns.IndexOf(cellinfo.Column));
                }
            }
        }
       
        public static void DataGridRevertCellMenuItem(object sender, DBTableControl.DBEditorTableControl mainTable)
        {
            foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells.Where(n => n.Item is DataRowView))
            {
                // Ignore added or detached cells since they have no original values and will instead throw an error.
                if ((cellinfo.Item as DataRowView).Row.RowState == DataRowState.Detached ||
                    (cellinfo.Item as DataRowView).Row.RowState == DataRowState.Added)
                {
                    continue;
                }

                int datarowindex = mainTable.CurrentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);
                int datacolindex = mainTable.CurrentTable.Columns.IndexOf((string)cellinfo.Column.Header);

                // Make sure our visible rows are up to date and skip any currently filtered rows.
                if (mainTable._visibleRows.Count <= mainTable.CurrentTable.Rows.Count)
                {
                    mainTable.UpdateVisibleRows();
                }
                if (mainTable._visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                {
                    continue;
                }

                mainTable.CurrentTable.Rows[datarowindex][datacolindex] = mainTable.CurrentTable.Rows[datarowindex][datacolindex, DataRowVersion.Original];
            }
        }
    }
}
