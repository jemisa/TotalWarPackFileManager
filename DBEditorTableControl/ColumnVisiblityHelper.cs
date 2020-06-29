using CommonDialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBEditorTableControl
{
    class ColumnVisiblityHelper
    {
        public static ListEditor Show(DBTableControl.DBEditorTableControl mainTable)
        {
            ListEditor hiddencolumnslisteditor = new ListEditor
            {
                LeftLabel = "Visible Columns:",
                RightLabel = "Hidden Columns:",
                OriginalOrder = mainTable.dbDataGrid.Columns.Select(n => (string)n.Header).ToList<string>(),
                LeftList = mainTable.CurrentTable.Columns.OfType<DataColumn>()
                                                                      .Where(n => !(bool)n.ExtendedProperties["Hidden"])
                                                                      .Select(n => n.ColumnName).ToList(),
                RightList = mainTable.CurrentTable.Columns.OfType<DataColumn>()
                                                                      .Where(n => (bool)n.ExtendedProperties["Hidden"])
                                                                      .Select(n => n.ColumnName).ToList()
            };

            return hiddencolumnslisteditor;
        }
    }
}
