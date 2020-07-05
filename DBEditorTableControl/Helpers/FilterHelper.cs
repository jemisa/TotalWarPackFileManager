using DBTableControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBEditorTableControl
{
    class FilterHelper
    {

        public class FilterSettings
        {
            public bool ErrorDockPanelVisible { get; set; } = false;
            public bool WarningFilter { get; set; } = false;
            public bool ErrorFilter { get; set; } = false;
        }


        #region Filter Methods
        public static void UpdateVisibleRows(DBTableControl.DBEditorTableControl mainTable, IList<DBFilter> filterList, FilterSettings filterSettings)
        {
            for (int i = 0; i < mainTable.CurrentTable.Rows.Count; i++)
            {
                if (i >= mainTable._visibleRows.Count)
                {
                    // We have a problem. Append additional items until we are ok again.
                    while (i >= mainTable._visibleRows.Count)
                    {
                        mainTable._visibleRows.Add(System.Windows.Visibility.Visible);
                    }
                }

                // If there are more rows in the datagrid than currentTable we are adding a new row by modifying the blank row, so
                // we should add rows to the list until we max out.
                while (mainTable.dbDataGrid.Items.Count > mainTable._visibleRows.Count)
                {
                    mainTable._visibleRows.Add(System.Windows.Visibility.Visible);
                }

                if (FilterTestRow(i, mainTable, filterList, filterSettings))
                {
                    mainTable._visibleRows[i] = System.Windows.Visibility.Visible;
                }
                else
                {
                    mainTable._visibleRows[i] = System.Windows.Visibility.Collapsed;
                }
            }
        }



        private static bool FilterTestRow(int rowindex, DBTableControl.DBEditorTableControl mainTable, IList<DBFilter> filterList, FilterSettings filterSettings)
        {
            int colindex;

            // Test row against active advanced filters.
            foreach (DBFilter filter in filterList)
            {
                if (!filter.IsActive)
                {
                    continue;
                }

                colindex = mainTable.CurrentTable.Columns.IndexOf(filter.ApplyToColumn);

                if (!FilterTestValue(mainTable.CurrentTable.Rows[rowindex][colindex], filter.FilterValue, filter.MatchMode))
                {
                    return false;
                }
            }

            // Test row against currently active column autofilters.
            foreach (DBFilter filter in mainTable._autofilterList)
            {
                if (!filter.IsActive)
                {
                    continue;
                }

                colindex = mainTable.CurrentTable.Columns.IndexOf(filter.ApplyToColumn);

                if (!FilterTestValue(mainTable.CurrentTable.Rows[rowindex][colindex], filter.FilterValue, filter.MatchMode))
                {
                    return false;
                }
            }

            // If either the error or warning filter is engaged test the row agains them as well.
            if (filterSettings.ErrorDockPanelVisible)
            {
                // If both filters are engaged either or will pass.
                if (filterSettings.ErrorFilter && filterSettings.WarningFilter)
                {
                    if (mainTable._errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Error:")) == 0 &&
                        mainTable._errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Warning:")) == 0)
                    {
                        return false;
                    }
                }// If only the Warnings filter is engaged, only warnings pass.
                else if (!filterSettings.ErrorFilter && filterSettings.WarningFilter)
                {
                    if (mainTable._errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Warning:")) == 0)
                    {
                        return false;
                    }
                }// If only the Errors filter is engaged, only errors pass.
                else if (filterSettings.ErrorFilter && !filterSettings.WarningFilter)
                {
                    if (mainTable._errorList.Count(n => n.RowIndex == rowindex && n.ErrorMessage.Contains("Error:")) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool FilterTestValue(object totest, string filtervalue, MatchType matchtype)
        {
            // Match the value exactly.
            if (matchtype == MatchType.Exact)
            {
                if (!totest.ToString().Equals(filtervalue))
                {
                    return false;
                }
            }// Check for partial match.
            else if (matchtype == MatchType.Partial)
            {
                if (!totest.ToString().Contains(filtervalue))
                {
                    return false;
                }
            }// Run a Regex match.
            else if (matchtype == MatchType.Regex)
            {
                if (!Regex.IsMatch(totest.ToString(), filtervalue))
                {
                    return false;
                }
            }// Check for empty values.
            else if (matchtype == MatchType.Empty)
            {
                if (!String.IsNullOrEmpty(totest.ToString()))
                {
                    return false;
                }
            }// Check for not empty values.
            else if (matchtype == MatchType.NotEmpty)
            {
                if (String.IsNullOrEmpty(totest.ToString()))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
