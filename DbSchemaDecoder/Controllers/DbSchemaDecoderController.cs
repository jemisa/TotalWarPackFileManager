using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{




    class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {

       // public PersonsViewModel EntityTableModel { get; set; }

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

        FieldInfo _selectedDbDefinition;
        public FieldInfo SelectedDbDefinition
        {
            get { return _selectedDbDefinition; }
            set
            {
                _selectedDbDefinition = value;
                NotifyPropertyChanged();
            }
        }

        int _currentVersion = 0;
        DataBaseFile _selectedFile;
        // 
        CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();

        public ICommand DbDefinitionRemovedCommand { get; private set; }
        public ICommand DbDefinitionMovedUpCommand { get; private set; }
        public ICommand DbDefinitionMovedDownCommand { get; private set; }

        public DbSchemaDecoderController(FileListController fileListController)
        {
            //EntityTableModel = new PersonsViewModel();

       
            fileListController.OnFileSelectedEvent += OnDbFileSelected;
            TestValue = "MyString is cool";

            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
        }

        private void OnDbDefinitionRemoved()
        {
            var x = DBTypeMap.Instance.GetVersionedInfos(_selectedFile.TableType, _currentVersion);
            var entry = x.First();
            var index = entry.Fields.IndexOf(SelectedDbDefinition);
            entry.Fields.RemoveAt(index);
            LoadCurrentTableDefinition(_selectedFile.TableType, _currentVersion);
        }

        private void OnDbDefinitionMovedUp()
        {
            var x = DBTypeMap.Instance.GetVersionedInfos(_selectedFile.TableType, _currentVersion);
            var entry = x.First();
            var index = entry.Fields.IndexOf(SelectedDbDefinition);
            if (index == 0)
                return;
            entry.Fields.RemoveAt(index);
            entry.Fields.Insert(index - 1, SelectedDbDefinition);
            LoadCurrentTableDefinition(_selectedFile.TableType, _currentVersion);
        }

        private void OnDbDefinitionMovedDown()
        {
            var x = DBTypeMap.Instance.GetVersionedInfos(_selectedFile.TableType, _currentVersion);
            var entry = x.First();
            var index = entry.Fields.IndexOf(SelectedDbDefinition);
            if (index == entry.Fields.Count)
                return;
            entry.Fields.RemoveAt(index);
            entry.Fields.Insert(index + 1, SelectedDbDefinition);
            LoadCurrentTableDefinition(_selectedFile.TableType, _currentVersion);
        }

        private void OnDbFileSelected(object sender, DataBaseFile e)
        {
            _selectedFile = e;

            //NotifyPropertyChanged("CaSchemaEntries");

            TestValue = "MyString is cool2";
            // throw new NotImplementedException();

            ParseDatabaseFile(e);

            LoadCurrentTableDefinition(e.TableType, _currentVersion);
            LoadCaSchemaDefinition(e.TableType);
            //CreateEntityTable();
            shitshat();

            
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


        void ParseUntilFieldEnd()
        {
            // Create stuff
            bool unicode = true;
            NextValue a = new NextValue();
            // Add to the TypeSelection listeners. 
            if (unicode)
            {
                a.StringType = new OutputSuff() { Instance = Types.StringType };
                a.OptStringType = new OutputSuff() { Instance = Types.OptStringType };
            }
            else
            {
                a.StringType = new OutputSuff() { Instance = Types.StringTypeAscii };
                a.OptStringType = new OutputSuff() { Instance = Types.OptStringTypeAscii };
            }

            a.IntType = new OutputSuff() { Instance = Types.IntType };
            a.BoolType = new OutputSuff() { Instance = Types.BoolType };
            a.SingleType = new OutputSuff() { Instance = Types.SingleType };
            a.ByteType = new OutputSuff() { Instance = Types.ByteType };
            //


            // Parse to current pos
            var fields = SelectedTableTypeInformations;
            using (var reader = new BinaryReader(new MemoryStream(_selectedFile.DbFile.Data)))
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                foreach (var selection in fields)
                {
                    var instance = selection.CreateInstance();
                    instance.Decode(reader);
                }

                long showFrom = reader.BaseStream.Position;
                var allDecoders = a.GetAll();
                foreach (var decoder in allDecoders)
                {
                    decoder.Update(reader);
                    reader.BaseStream.Position = showFrom;
                }
            }
        }

        public delegate FieldInfo TypeFactory();
        class OutputSuff
        {
            public TypeFactory Instance;
            public string DisplayValue;

            public void Update(BinaryReader reader)
            {
                var instance = Instance().CreateInstance();
                try
                {
                    instance.Decode(reader);
                    DisplayValue = instance.Value;
                }
                catch (Exception e)
                {
                    DisplayValue = e.Message;
                }
            }
        }

        class NextValue
        {
            public OutputSuff StringType;
            public OutputSuff OptStringType;
            public OutputSuff IntType;
            public OutputSuff BoolType;
            public OutputSuff SingleType;
            public OutputSuff ByteType;

            public OutputSuff[] GetAll()
            { 
                OutputSuff[] all = { StringType, OptStringType, IntType , BoolType, SingleType, ByteType};
                return all;
            }
        }

        void shitshat()
        {
            return;
            int dataLeftInStram = 0;
            List<List<string>> tableData = new List<List<string>>();
            int currentIndex = 0;
            FieldInfo currentField = null;
            try
            {
                var fields = SelectedTableTypeInformations;
                var expectedEntries = SelectedFileHeaderInformation.ExpectedEntries;

              
                using (var stream = new MemoryStream(_selectedFile.DbFile.Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                        for (currentIndex = 0; currentIndex < expectedEntries; currentIndex++)
                        {
                            var tableRow = new List<string>();
                            foreach (var selection in fields)
                            {
                                currentField = selection;
                                var instance = selection.CreateInstance();
                                instance.Decode(reader);
                                var v = instance.Value;
                                tableRow.Add(v);
                            }

                            tableData.Add(tableRow);
                        }
                    }

                    dataLeftInStram = stream.Capacity - (int)stream.Position;
                }
            }
            catch (Exception e)
            { 
            
            }
            return;

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

    public class ColumnConfig
    {
        public ObservableCollection<Column> Columns { get; set; } = new ObservableCollection<Column>();
    }

    public class Column
    {
        public string Header { get; set; }
        public string DataField { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public string Attributes { get; set; }
    }
}
