using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace DBTableControl
{
    public class DBError
    {
        int rowindex;
        public int RowIndex { get { return rowindex; } set { rowindex = value; } }

        DataRowView gridrow;
        public DataRowView GridRow { get { return gridrow; } }

        string columnname;
        public string ColumnName { get { return columnname; } }

        string errormessage;
        public string ErrorMessage { get { return errormessage; } }

        public DBError(int _rowindex, DataRowView _gridrow, string _columnname, string _errormessage)
        {
            rowindex = _rowindex;
            gridrow = _gridrow;
            columnname = _columnname;
            errormessage = _errormessage;
        }
    }
}
