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
        public ObservableCollection<FieldInfo> SelectedTableTypeInformations { get; set; } = new ObservableCollection<FieldInfo>();

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

        int _currentVersion = 0;
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

            ParseDatabaseFile(e);

            LoadCurrentTableDefinition(e.TableType, _currentVersion);
            LoadCaSchemaDefinition(e.TableType);
            
        }

        void LoadCurrentTableDefinition(string tableName, int currentVersion)
        {
            var x = DBTypeMap.Instance.GetVersionedInfos(tableName, currentVersion);
            SelectedTableTypeInformations.Clear();

            if (x == null || x.FirstOrDefault() == null)
                return;
        
            foreach(var i in x.First().Fields)
                SelectedTableTypeInformations.Add(i); 
        }

        void LoadCaSchemaDefinition(string tableName)
        {
            var res = caSchemaFileParser.Load(tableName);
            CaSchemaEntries.Clear();
            foreach (var x in res.Entries)
                CaSchemaEntries.Add(x);
        }

        void ParseDatabaseFile(DataBaseFile item)
        {
            var bytes = item.DbFile.Data;
            if (bytes == null)
                return;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                if (header != null)
                {
                    _currentVersion = header.Version;
                }
                SelectedFileHeaderInformation.Update(header, item);
            }
        }


        void shitshat()
        {
            var fields = SelectedTableTypeInformations;


            /*TypeSelection[] selections = new TypeSelection[] {
                intType, stringType, boolType, singleType, optStringType, byteType
            };*/
        }
        /*
         * 
         * 
         *             unicode = encodeUnicode;
            
            #region Type Selection Listener Initialization
            // Add to the TypeSelection listeners. 
            if (unicode) {
                stringType.Factory = Types.StringType;
                optStringType.Factory = Types.OptStringType;
            } else {
                stringType.Factory = Types.StringTypeAscii;
                optStringType.Factory = Types.OptStringTypeAscii;
            }
stringType.Selected += AddType;

            intType.Factory = Types.IntType;
            intType.Selected += AddType;

            boolType.Factory = Types.BoolType;
            boolType.Selected += AddType;

            singleType.Factory = Types.SingleType;
            singleType.Selected += AddType;
   
            optStringType.Selected += AddType;

            byteType.Factory = Types.ByteType;
            byteType.Selected += AddType;
         * 
         * 
         
                     if (Bytes == null) {
                return;
            }
           
            long showFrom = CurrentCursorPosition;
#if DEBUG
            Console.WriteLine("parser position {0}", showFrom);
#endif
            using (var reader = new BinaryReader(new MemoryStream(Bytes))) {
                foreach (TypeSelection selection in selections) {
                    reader.BaseStream.Position = showFrom;
                    selection.ShowPreview(reader);
                }
            }
         */
    }
}
