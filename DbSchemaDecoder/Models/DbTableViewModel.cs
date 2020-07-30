using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    class DbTableViewModel
    {
        public DataTable EntityTable { get; set; } = new DataTable();
    }
}
