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
    class ClipboardHelper
    {
        public static string GetClipboardText()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    return Clipboard.GetText(TextDataFormat.Text);
                }
                catch
                {

                }
                System.Threading.Thread.Sleep(10);
            }

            return null;
        }

        private static bool ClipboardIsEmpty()
        {
            string clipboardText = GetClipboardText();

            foreach (string line in clipboardText.Split('\n'))
            {
                foreach (string cell in line.Split('\t'))
                {
                    if (!String.IsNullOrEmpty(cell))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ClipboardContainsSingleCell()
        {
            return !GetClipboardText().Contains('\t') && GetClipboardText().Count(n => n == '\n') == 1;
        }

        private static bool TryPasteValue(DataTable CurrentTable, int rowIndex, int columnIndex, string value)
        {
            bool retval = true;

            try
            {
                CurrentTable.Rows[rowIndex][columnIndex] = value.Trim();
            }
            catch (Exception ex)
            {
#if DEBUG
                ErrorDialog.ShowDialog(ex);
#endif
            }

            return retval;
        }

        private static bool ClipboardContainsOnlyRows(DBTableControl.DBEditorTableControl mainTable)
        {
            string clipboardText = GetClipboardText();

            foreach (string line in clipboardText.Split('\n'))
            {
                if (String.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (!IsLineARow(mainTable, line))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLineARow(DBTableControl.DBEditorTableControl mainTable, string line)
        {
            if (line.Count(n => n == '\t') >= mainTable.EditedFile.CurrentType.Fields.Count - 1)
            {
                bool fullrow = true;
                string[] cells = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToArray();
                for (int i = 0; i < cells.Length; i++)
                {
                    if (String.IsNullOrEmpty(cells[i]) &&
                        (mainTable.EditedFile.CurrentType.Fields[i].TypeCode != TypeCode.String) && !mainTable.EditedFile.CurrentType.Fields[i].TypeName.Contains("optstring"))
                    {
                        fullrow = false;
                        break;
                    }
                }

                if (fullrow)
                {
                    return true;
                }
            }

            return false;
        }

   

        private static bool PasteMatchesSelection(DBTableControl.DBEditorTableControl mainTable, string clipboardData)
        {
            // Build a blank clipboard copy from selected cells to compare to clipboardData
            string testSelection = "";
            for (int i = 0; i < mainTable.CurrentTable.Rows.Count; i++)
            {
                // Additional error checking for the visibleRows internal list.
                if (i >= mainTable._visibleRows.Count)
                {
                    mainTable.UpdateVisibleRows();
                }

                // Ignore collapsed (filtered) rows when building our test string.
                if (mainTable._visibleRows[i] == System.Windows.Visibility.Collapsed)
                {
                    continue;
                }

                bool writeEndofLine = false;
                int minColumnIndex = mainTable.dbDataGrid.SelectedCells.Min(n => n.Column.DisplayIndex);
                int maxColumnIndex = mainTable.dbDataGrid.SelectedCells.Max(n => n.Column.DisplayIndex);

                foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells.Where(n => mainTable.CurrentTable.Rows.IndexOf((n.Item as DataRowView).Row) == i))
                {
                    for (int j = minColumnIndex; j < maxColumnIndex; j++)
                    {
                        testSelection += "\t";
                    }
                    writeEndofLine = true;
                    break;
                }

                if (writeEndofLine)
                {
                    testSelection += "\r\n";
                }
            }

            // If the number of lines don't match return false
            if (testSelection.Count(n => n == '\n') != clipboardData.Count(n => n == '\n'))
            {
                return false;
            }

            // If the number of lines match, test each line for the same number of 'cells'
            foreach (string line in clipboardData.Split('\n'))
            {
                if (testSelection.Count(n => n == '\t') != clipboardData.Count(n => n == '\t'))
                {
                    return false;
                }
            }

            return true;
        }

        private static void PasteError(string error)
        {
            MessageBox.Show(error, "Paste Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static bool IsRowValid(DBTableControl.DBEditorTableControl mainTable, string line)
        {
            List<string> fields = line.Split('\t').ToList();
            List<int> fieldorder = new List<int>();

            if (fields.Count < mainTable.CurrentTable.Columns.Count)
            {
                PasteError("Error: could not paste following row into table, too few fields.\n\n" + line);
                return false;
            }

            if (mainTable.MoveAndFreezeKeys)
            {
                // Fields may be in altered visual order from data source.
                fieldorder.AddRange(mainTable.dbDataGrid.Columns.OrderBy(n => n.DisplayIndex).Select(n => mainTable.dbDataGrid.Columns.IndexOf(n)));
            }
            else
            {
                // Fields are displayed in original data order.
                for (int i = 0; i < mainTable.CurrentTable.Columns.Count; i++)
                {
                    fieldorder.Add(i);
                }
            }

            for (int i = 0; i < mainTable.CurrentTable.Columns.Count; i++)
            {
                TypeCode fieldtypecode = mainTable.EditedFile.CurrentType.Fields[fieldorder[i]].TypeCode;

                if (fieldtypecode == TypeCode.String)
                {
                    continue;
                }
                else if (fieldtypecode == TypeCode.Single)
                {
                    float temp;
                    if (!float.TryParse(fields[i], out temp))
                    {
                        PasteError(String.Format("Error: could not paste line into table, as '{0}' is not a valid value for '{1}'.\n\n{2}",
                                                    fields[i], mainTable.CurrentTable.Columns[fieldorder[i]].ColumnName, line));
                        return false;
                    }
                }
                else if (fieldtypecode == TypeCode.Int32)
                {
                    int temp;
                    if (!int.TryParse(fields[i], out temp))
                    {
                        PasteError(String.Format("Error: could not paste line into table, as '{0}' is not a valid value for '{1}'.\n\n{2}",
                                                    fields[i], mainTable.CurrentTable.Columns[fieldorder[i]].ColumnName, line));
                        return false;
                    }
                }
                else if (fieldtypecode == TypeCode.Int16)
                {
                    short temp;
                    if (!short.TryParse(fields[i], out temp))
                    {
                        PasteError(String.Format("Error: could not paste line into table, as '{0}' is not a valid value for '{1}'.\n\n{2}",
                                                    fields[i], mainTable.CurrentTable.Columns[fieldorder[i]].ColumnName, line));
                        return false;
                    }
                }
            }

            return true;
        }

        public static void OnCanExecutePaste(DBTableControl.DBEditorTableControl mainTable, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = mainTable.dbDataGrid.CurrentCell != null;
            args.CanExecute = !mainTable.ReadOnly;
            args.Handled = true;
        }

        public static void OnExecutedPaste(DBTableControl.DBEditorTableControl mainTable)
        {
            string clipboarddata = GetClipboardText();
            int rowindex;
            int datarowindex;
            int columnindex;
            int datacolumnindex;

            if (ClipboardIsEmpty())
            {
                // Clipboard Empty
            }
            else if (ClipboardContainsSingleCell())
            {
                // Single Cell Paste
                string pastevalue = clipboarddata.Trim();

                foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells)
                {
                    // Get both visible and data row index
                    rowindex = mainTable.dbDataGrid.Items.IndexOf(cellinfo.Item);
                    datarowindex = mainTable.CurrentTable.Rows.IndexOf((cellinfo.Item as DataRowView).Row);

                    // Additional error checking for the visibleRows internal list.
                    if (datarowindex >= mainTable._visibleRows.Count)
                    {
                        mainTable.UpdateVisibleRows();
                    }

                    // Selecting cells while the table is filtered will select collapsed rows, so skip them here.
                    if (mainTable._visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                    {
                        continue;
                    }

                    // Get both visible and data column index
                    columnindex = mainTable.dbDataGrid.Columns.IndexOf(cellinfo.Column);
                    datacolumnindex = mainTable.CurrentTable.Columns.IndexOf((string)cellinfo.Column.Header);

                    mainTable.CurrentTable.Rows[datarowindex].BeginEdit();

                    if (!TryPasteValue(mainTable.CurrentTable, datarowindex, datacolumnindex, pastevalue))
                    {
                        // Paste Error
                    }

                    mainTable.CurrentTable.Rows[datarowindex].EndEdit();
                    mainTable.RefreshCell(rowindex, columnindex);
                }
            }
            else if (!ClipboardContainsOnlyRows(mainTable))
            {
                if (mainTable.dbDataGrid.SelectedItems.OfType<DataRowView>().Count() != mainTable.dbDataGrid.SelectedItems.Count)
                {
                    // The blank row is selected, abort.
                    MessageBox.Show("Only select the blank row when pasting rows.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Use -1 values to indicate that nothing is selected, and to paste any full rows the
                // clipboard might contain as new rows.
                int basecolumnindex = -1;
                rowindex = -1;
                if (mainTable.dbDataGrid.SelectedCells.Count > 1)
                {
                    // User has more than 1 cells selected, therefore the selection must match the clipboard data.
                    if (!PasteMatchesSelection(mainTable, clipboarddata))
                    {
                        // Warn user
                        if (MessageBox.Show("Warning! Cell selection does not match copied data, attempt to paste anyway?",
                                           "Selection Error", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    // Set values to the first cell's coordinates
                    // Get both visible and data row index
                    rowindex = mainTable.dbDataGrid.Items.IndexOf(mainTable.dbDataGrid.SelectedCells[0].Item);
                    basecolumnindex = mainTable.dbDataGrid.SelectedCells[0].Column.DisplayIndex;

                    // Determine upper left corner of selection
                    foreach (DataGridCellInfo cellinfo in mainTable.dbDataGrid.SelectedCells)
                    {
                        rowindex = Math.Min(rowindex, mainTable.dbDataGrid.Items.IndexOf(cellinfo.Item));
                        basecolumnindex = Math.Min(basecolumnindex, cellinfo.Column.DisplayIndex);
                    }
                }
                else if (mainTable.dbDataGrid.SelectedCells.Count == 1)
                {
                    // User has 1 cell selected, assume it is the top left corner and attempt to paste.
                    rowindex = mainTable.dbDataGrid.Items.IndexOf(mainTable.dbDataGrid.SelectedCells[0].Item);
                    basecolumnindex = mainTable.dbDataGrid.SelectedCells[0].Column.DisplayIndex;
                }

                List<string> pasteinstructions = new List<string>();

                foreach (string line in clipboarddata.Split('\n'))
                {
                    columnindex = basecolumnindex;

                    if (rowindex > mainTable.CurrentTable.Rows.Count - 1 || columnindex == -1)
                    {
                        if (IsLineARow(mainTable, line))
                        {
                            // We have a full row, but no where to paste it, so add it as a new row.
                            DataRow newrow = mainTable.CurrentTable.NewRow();

                            if (mainTable.MoveAndFreezeKeys)
                            {
                                // Data is being displayed with keys moved, so assume the clipboard data matches the visual appearance and not
                                // the order of the data source.
                                object tempitem;
                                List<object> itemarray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToList<object>();
                                for (int i = mainTable.CurrentTable.PrimaryKey.Count() - 1; i >= 0; i--)
                                {
                                    tempitem = itemarray[i];
                                    itemarray.RemoveAt(i);
                                    itemarray.Insert(mainTable.CurrentTable.Columns.IndexOf(mainTable.CurrentTable.PrimaryKey[i]), tempitem);
                                }

                                // Once we have reordered the clipboard data to match the data source, convert to an object[]
                                newrow.ItemArray = itemarray.ToArray();
                            }
                            else
                            {
                                // Data is displayed as it is stored, so assume the clipboard data is ordered the same way.
                                newrow.ItemArray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToArray<object>();
                            }

                            mainTable.CurrentTable.Rows.Add(newrow);
                        }

                        rowindex++;
                        continue;
                    }

                    if (String.IsNullOrEmpty(line.Trim()))
                    {
                        rowindex++;
                        continue;
                    }

                    // Convert visual row and column index to data row and column index.
                    datarowindex = mainTable.CurrentTable.Rows.IndexOf((mainTable.dbDataGrid.Items[rowindex] as DataRowView).Row);

                    // Additional error checking for the visibleRows internal list.
                    if (datarowindex >= mainTable._visibleRows.Count)
                    {
                        mainTable.UpdateVisibleRows();
                    }

                    // Skip past any collapsed (filtered) rows.
                    while (mainTable._visibleRows[datarowindex] == System.Windows.Visibility.Collapsed)
                    {
                        rowindex++;
                        datarowindex = mainTable.CurrentTable.Rows.IndexOf((mainTable.dbDataGrid.Items[rowindex] as DataRowView).Row);

                        if (rowindex >= mainTable.CurrentTable.Rows.Count)
                        {
                            break;
                        }
                    }

                    foreach (string cell in line.Replace("\r", "").Split('\t'))
                    {
                        if (columnindex > mainTable.CurrentTable.Columns.Count - 1)
                        {
                            break;
                        }

                        if (String.IsNullOrEmpty(cell.Trim()))
                        {
                            columnindex++;
                            continue;
                        }

                        // Convert visual column index, the display index not its location in the datagrid's collection, to data column index.
                        datacolumnindex = mainTable.CurrentTable.Columns.IndexOf((string)mainTable.dbDataGrid.Columns.Single(n => n.DisplayIndex == columnindex).Header);

                        // Since refresh works on the visual tree, and the visual tree is not affected by DisplayIndex, find the columns location
                        // in the datagrid's column collection to pass on.
                        int refreshcolumnindex = mainTable.dbDataGrid.Columns.IndexOf(mainTable.dbDataGrid.Columns.Single(n => n.DisplayIndex == columnindex));

                        // Rather than attempting to modify cells as we go, we should modify them in batches
                        // since any kind of sorting may interfere with paste order in real time.
                        pasteinstructions.Add(String.Format("{0};{1};{2};{3};{4}", datarowindex, rowindex, datacolumnindex, refreshcolumnindex, cell));

                        columnindex++;
                    }

                    rowindex++;
                }

                // Now that we have a list of paste instructions, execute them simultaneously across the data source
                // to avoid interference from any visual resorting.
                // Instruction Format: Data Row index;Visual Row index;Data Column index;Visual Column index;value
                foreach (string instruction in pasteinstructions)
                {
                    // Parse out the instructions.
                    string[] parameters = instruction.Split(';');
                    datarowindex = int.Parse(parameters[0]);
                    rowindex = int.Parse(parameters[1]);
                    datacolumnindex = int.Parse(parameters[2]);
                    columnindex = int.Parse(parameters[3]);

                    // Edit currentTable
                    mainTable.CurrentTable.Rows[datarowindex].BeginEdit();
                    mainTable.CurrentTable.Rows[datarowindex][datacolumnindex] = parameters[4];
                    mainTable.CurrentTable.Rows[datarowindex].EndEdit();

                    // Refresh the visual cell
                    mainTable.RefreshCell(rowindex, columnindex);
                }
            }
            else
            {
                // Paste Rows, with no floater cells.
                if (mainTable.dbDataGrid.SelectedCells.Count == (mainTable.dbDataGrid.SelectedItems.Count * mainTable.EditedFile.CurrentType.Fields.Count))
                {
                    // Only rows are selected.
                    // Since the SelectedItems list is in the order of selection and NOT in the order of appearance we need
                    // to create a custom sorted list of indicies to paste to.
                    List<int> indiciesToPaste = new List<int>();
                    List<int> dataindiciesToPaste = new List<int>();
                    int testindex;
                    foreach (DataRowView rowview in mainTable.dbDataGrid.SelectedItems.OfType<DataRowView>())
                    {
                        testindex = mainTable.CurrentTable.Rows.IndexOf(rowview.Row);

                        // Additional error checking for the visibleRows internal list.
                        if (testindex >= mainTable._visibleRows.Count)
                        {
                            mainTable.UpdateVisibleRows();
                        }

                        // Skip any collapsed (filtered) rows.
                        if (mainTable._visibleRows[testindex] == System.Windows.Visibility.Visible)
                        {
                            indiciesToPaste.Add(mainTable.dbDataGrid.Items.IndexOf(rowview));
                        }
                    }
                    indiciesToPaste.Sort();

                    // Now that we have the selected rows visual locations, we need to determine their locations in our data source.
                    foreach (int i in indiciesToPaste)
                    {
                        dataindiciesToPaste.Add(mainTable.CurrentTable.Rows.IndexOf((mainTable.dbDataGrid.Items[i] as DataRowView).Row));
                    }

                    // We now have a list of data indicies sorted in visual order.
                    int currentindex = 0;

                    foreach (string line in clipboarddata.Replace("\r", "").Split('\n'))
                    {
                        if (!IsLineARow(mainTable, line) || String.IsNullOrEmpty(line) || !IsRowValid(mainTable, line))
                        {
                            currentindex++;
                            continue;
                        }

                        if (currentindex >= dataindiciesToPaste.Count)
                        {
                            // Add new row
                            DataRow newrow = mainTable.CurrentTable.NewRow();
                            if (mainTable.MoveAndFreezeKeys)
                            {
                                // Data is being displayed with keys moved, so assume the clipboard data matches the visual appearance and not
                                // the order of the data source.
                                object tempitem;
                                List<object> itemarray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToList<object>();
                                for (int i = mainTable.CurrentTable.PrimaryKey.Count() - 1; i >= 0; i--)
                                {
                                    tempitem = itemarray[i];
                                    itemarray.RemoveAt(i);
                                    itemarray.Insert(mainTable.CurrentTable.Columns.IndexOf(mainTable.CurrentTable.PrimaryKey[i]), tempitem);
                                }

                                // Once we have reordered the clipboard data to match the data source, convert to an object[]
                                newrow.ItemArray = itemarray.ToArray();
                            }
                            else
                            {
                                // Data is displayed as it is stored, so assume the clipboard data is ordered the same way.
                                newrow.ItemArray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToArray<object>();
                            }

                            mainTable.CurrentTable.Rows.Add(newrow);

                            currentindex++;
                            continue;
                        }

                        mainTable.CurrentTable.Rows[dataindiciesToPaste[currentindex]].BeginEdit();
                        if (mainTable.MoveAndFreezeKeys)
                        {
                            // Data is being displayed with keys moved, so assume the clipboard data matches the visual appearance and not
                            // the order of the data source.
                            object tempitem;
                            List<object> itemarray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToList<object>();
                            for (int i = mainTable.CurrentTable.PrimaryKey.Count() - 1; i >= 0; i--)
                            {
                                tempitem = itemarray[i];
                                itemarray.RemoveAt(i);
                                itemarray.Insert(mainTable.CurrentTable.Columns.IndexOf(mainTable.CurrentTable.PrimaryKey[i]), tempitem);
                            }

                            // Once we have reordered the clipboard data to match the data source, convert to an object[]
                            mainTable.CurrentTable.Rows[dataindiciesToPaste[currentindex]].ItemArray = itemarray.ToArray();
                        }
                        else
                        {
                            // Data is displayed as it is stored, so assume the clipboard data is ordered the same way.
                            mainTable.CurrentTable.Rows[dataindiciesToPaste[currentindex]].ItemArray = line.Split('\t').Take(mainTable.EditedFile.CurrentType.Fields.Count).ToArray<object>();
                        }

                        mainTable.CurrentTable.Rows[dataindiciesToPaste[currentindex]].EndEdit();
                        currentindex++;
                    }

                    mainTable.Refresh(true);
                }
                else
                {
                    // Please select rows.
                    MessageBox.Show("When pasting rows, please use the row header button to select entire rows only.",
                                    "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
