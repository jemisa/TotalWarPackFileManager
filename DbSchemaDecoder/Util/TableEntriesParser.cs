using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Models;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{
    class TableEntriesParser
    {
        readonly DataGridItemSourceUpdater _dataGridUpdater;
        readonly DbTableViewModel _viewModel;

        public TableEntriesParser(DataGridItemSourceUpdater dataGridUpdater, DbTableViewModel viewModel)
        {
            _dataGridUpdater = dataGridUpdater;
            _viewModel = viewModel;
        }

        public DataTable Parse(DataBaseFile baseFile, IEnumerable<FieldInfo> fields, int expectedEntries)
        {
            return null;
        }

        public void Update(DataTable table)
        { }
    }
}
