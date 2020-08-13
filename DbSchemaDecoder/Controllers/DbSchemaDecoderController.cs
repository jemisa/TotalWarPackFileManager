using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    /*class HexViewSelectionModel
    { 
        public int HeaderLength { get; set; }
        public List<HighLighedByteItem> HighLightedBytes { get; set; } = new List<HighLighedByteItem>();

        public class HighLighedByteItem
        { 
            public int StartByte { get; set; }
            public int Length { get; set; }
        }
    };*/
  
    class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public DbTableViewModel DbTableViewModel { get; set; } = new DbTableViewModel();
        public SelectedFileHeaderInformation SelectedFileHeaderInformation { get; set; } = new SelectedFileHeaderInformation();
        public ObservableCollection<CaSchemaEntry> CaSchemaEntries { get; set; } = new ObservableCollection<CaSchemaEntry>();

        public NextItemController NextItemController { get; set; }
        public DbTableDefinitionController DbTableDefinitionController { get; set; }

        public BruteForceController BruteForceController { get; set; }
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
        DataBaseFile _selectedFile;
        CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
        TableEntriesUpdater _tableEntriesParser;



        public ICommand ParseTbTableUsingCaSchemaCommand { get; private set; }

        public DbSchemaDecoderController(FileListController fileListController, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {




            _tableEntriesParser = new TableEntriesUpdater(dbTableItemSourceUpdater, DbTableViewModel);
            fileListController.OnFileSelectedEvent += OnDbFileSelected;
            TestValue = "MyString is cool";

            ParseTbTableUsingCaSchemaCommand = new RelayCommand(OnTest);

            NextItemController = new NextItemController();
            BruteForceController = new BruteForceController();

            DbTableDefinitionController = new DbTableDefinitionController();
            DbTableDefinitionController.OnDefinitionChangedEvent += DbTableDefinitionController_OnDefinitionChangedEvent;
            DbTableDefinitionController.OnSelectedRowChangedEvent += DbTableDefinitionController_OnSelectedRowChangedEvent;
            NextItemController.OnNewDefinitionCreated += DbTableDefinitionController.AppendRowOfTypeEventHandler;

            BruteForceController.OnNewDefinitionApplied += (s, a) => { DbTableDefinitionController.Set(a); };
        }

        private void DbTableDefinitionController_OnSelectedRowChangedEvent(object sender, FieldInfoViewModel e)
        {
            NextItemController.Update(_selectedFile, DbTableDefinitionController.TableTypeInformationTypes, DbTableDefinitionController.SelectedTypeInformationRow);
        }

        private void DbTableDefinitionController_OnDefinitionChangedEvent(object sender, List<FieldInfo> e)
        {
            _tableEntriesParser.Update(_selectedFile, DbTableDefinitionController.TableTypeInformationTypes);
            NextItemController.Update(_selectedFile, DbTableDefinitionController.TableTypeInformationTypes, DbTableDefinitionController.SelectedTypeInformationRow);
        }

        private void OnTest()
        {

            try
            {
                if (_selectedFile != null)
                {

                    var currentTableInof = DBTypeMap.Instance.GetVersionedInfos(_selectedFile.TableType, _currentVersion);
                    var entry = currentTableInof.First();
                    int maxNumberOfFields = entry.Fields.Count();

                    //var c = new BruteForceParser();
                    //c.BruteForce(_selectedFile, maxNumberOfFields);
                }
            }
            catch (Exception e)
            { }
            return;
        }




        private void OnDbFileSelected(object sender, DataBaseFile e)
        {
            _selectedFile = e;
            if (_selectedFile == null)
                return;

            ParseDatabaseFile(e);
            DbTableDefinitionController.LoadCurrentTableDefinition(e.TableType, _currentVersion);
            LoadCaSchemaDefinition(e.TableType);
            NextItemController.Update(_selectedFile, DbTableDefinitionController.TableTypeInformationTypes, DbTableDefinitionController.SelectedTypeInformationRow);



            BruteForceController.SelectedFile = e;
            BruteForceController.TabelCount = DbTableDefinitionController.TableTypeInformationTypes.Count();
            BruteForceController.CaSchemaEntryList = CaSchemaEntries;


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
    }
}
