using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Controllers
{



    class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public SelectedFileHeaderInformation SelectedFileHeaderInformation { get; set; } = new SelectedFileHeaderInformation();
        public ObservableCollection<CaSchemaEntry> CaSchemaEntries { get; set; } = new ObservableCollection<CaSchemaEntry>();

        string _testValue;
        public string TestValue
        {
            get { return _testValue; }
            set
            {
                _testValue = value;
                NotifyPropertyChanged();
            }
        }

        // 
        CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();

        public DbSchemaDecoderController(FileListController fileListController)
        {

            fileListController.OnFileSelectedEvent += OnDbFileSelected;
            TestValue = "MyString is cool";




        }

        private void OnDbFileSelected(object sender, DataBaseFile e)
        {
            

            //NotifyPropertyChanged("CaSchemaEntries");

            TestValue = "MyString is cool2";
            // throw new NotImplementedException();

            var res = caSchemaFileParser.Load(e.TableType);
            CaSchemaEntries.Clear();
            foreach (var x in res.Entries)
                CaSchemaEntries.Add(x);
                //CaSchemaEntries = new ObservableCollection<CaSchemaEntry>(res.Entries);

            ParseDatabaseFile(e);
        }

        void ParseDatabaseFile(DataBaseFile item)
        {
            var bytes = item.DbFile.Data;
            if (bytes == null)
                return;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                SelectedFileHeaderInformation.Update(header, item);
            }
        }

        public void ExtractHeaderInformation()
        { 
        }
    }
}
