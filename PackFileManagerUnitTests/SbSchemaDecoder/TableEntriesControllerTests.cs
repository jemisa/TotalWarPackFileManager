using Common;
using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PackFileManagerUnitTests.Utility;

namespace PackFileManagerUnitTests.SbSchemaDecoder
{
    [TestClass]
    public class TableEntriesControllerTests
    {
        [TestMethod]
        public void ConvertTabele_validDefinition()
        {
            var table = DbHelper.CreateTestPeopleTable();
            DbHelper.AddRow(table, new string[]{ "Ole", "Kjærsti", "21", "178.4" });
            DbHelper.AddRow(table, new string[]{ "Line", "Burito", "21", "158.4" });
            DbHelper.AddRow(table, new string[]{ "Jonny", "boop", "0", "88.4" });
            var bytes = DbHelper.GetBytes(table);

            WindowState state = new WindowState();
            TableEntriesController controller = new TableEntriesController(state, null);

            state.SelectedFile = new DataBaseFile()
            {
                TableType = table.CurrentType.TableName,
                DbFile = new PackedFile()
                {
                    Data = bytes
                }
            };
            //state.DbSchemaFields = table.CurrentType.Fields;

            Assert.AreEqual(4, controller.ViewModel.EntityTable.Columns.Count);
            Assert.AreEqual(3, controller.ViewModel.EntityTable.Rows.Count);

            var row = controller.ViewModel.EntityTable.Rows[1];
            Assert.AreEqual("Line", row.ItemArray[0]);
            Assert.AreEqual("Burito", row.ItemArray[1]);
            Assert.AreEqual("21", row.ItemArray[2]);
            Assert.AreEqual("158.4", row.ItemArray[3]);
        }

      
    }
}
