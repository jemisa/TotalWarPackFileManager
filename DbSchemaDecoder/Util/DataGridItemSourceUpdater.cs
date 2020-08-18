using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DbSchemaDecoder.Util
{
    public class DataGridItemSourceUpdater
    {
        public DataGrid Grid { get; set; }

        public void SetData(DataTable table)
        {
            Grid.ItemsSource = table.DefaultView;
        }
    }
}
