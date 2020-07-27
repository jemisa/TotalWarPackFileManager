using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Controllers
{
    class ConfigureTableRowsController
    {
        public ConfigureTableRowsController(FileListController fileListController)
        {

            fileListController.MyEvent += FileListController_MyEvent;
        }

        private void FileListController_MyEvent(object sender, TestItem e)
        {
           // throw new NotImplementedException();
        }
    }
}
