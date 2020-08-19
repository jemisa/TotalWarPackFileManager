using Filetypes;
using Filetypes.Codecs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackFileManagerUnitTests.Utility
{
    class DbHelper
    {
        public static DBFile CreateTestPeopleTable()
        {
            var fields = new List<FieldInfo>()
            {
                new StringTypeAscii() { Name = "FirstName"},
                new StringTypeAscii() { Name = "LastName"},
                new IntType() { Name = "Age"},
                new SingleType() { Name = "Height"},
            };

            TypeInfo type = new TypeInfo(fields)
            {
                Name = "TestPeople",
                Version = 4,
            };

            DBFileHeader header = new DBFileHeader(Guid.NewGuid().ToString(), type.Version, 0, false);
            DBFile dbFile = new DBFile(header, type);
            return dbFile;
        }


        public static void AddRow(DBFile file, string[] values)
        {
            Assert.AreEqual(file.CurrentType.Fields.Count, values.Count());

            var row = new DBRow(file.CurrentType);
            for (int i = 0; i < file.CurrentType.Fields.Count; i++)
            {
                row[i].Value = values[i];
            }
            file.Entries.Add(row);
        }

        public static byte[] GetBytes(DBFile file)
        {
            PackedFileDbCodec codec = new PackedFileDbCodec(file.CurrentType.Name);
            var bytes = codec.Encode(file);
            return bytes;
        }

    }
}
