using Common;
using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackFileManagerUnitTests.SbSchemaDecoder
{
    [TestClass]
    public class TableEntriesControllerTests
    {
        void LoadTestSchameas()
        { 
        
        }

        void CreateRow(DBFile file, string[] values)
        {
            var row = new DBRow(file.CurrentType);
            for (int i = 0; i < file.CurrentType.Fields.Count; i++)
            {
                row[i].Value = values[i];
            }
            file.Entries.Add(row);
        }

        [TestMethod]
        public void IntParser()
        {

            var fields = new List<FieldInfo>()
            {
                new StringTypeAscii() { Name = "FirstName"},
                new StringTypeAscii() { Name = "LastName"},
                new IntType() { Name = "Age"},
                new SingleType() { Name = "Height"},
            };
            TypeInfo myType = new TypeInfo(fields)
            {
                Name = "TestPeople",
                Version = 4,
            };

            DBFileHeader header = new DBFileHeader(Guid.NewGuid().ToString(), myType.Version, 0, false);
            DBFile dbFile = new DBFile(header, myType);

            CreateRow(dbFile, new string[]{ "Ole", "Kristian", "21", "178.4" });
            CreateRow(dbFile, new string[]{ "Line", "Homelien", "21", "158.4" });
            CreateRow(dbFile, new string[]{ "Jenny", "boop", "0", "88.4" });



            PackedFileDbCodec a = new PackedFileDbCodec(myType.Name);
            var bytes = a.Encode(dbFile);



            WindowState state = new WindowState();
            TableEntriesController controller = new TableEntriesController(state, null);

            state.SelectedFile = new DataBaseFile()
            {
                TableType = myType.Name,
                DbFile = new PackedFile()
                {
                    Data = bytes
                }
            };
            state.DbSchemaFields = fields;
        }
        //
    }
}
