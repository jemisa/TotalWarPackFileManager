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
using System.Text;
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

            DbTableDefinitionController = new DbTableDefinitionController();
            DbTableDefinitionController.OnDefinitionChangedEvent += DbTableDefinitionController_OnDefinitionChangedEvent;

            //InformationView.CurrentVersionChanged += TableDefinitionController.UpdateVersion(name, version)
            // EntityTable.Update += TableDefinitionController.OnChanged
        }

        private void DbTableDefinitionController_OnDefinitionChangedEvent(object sender, List<FieldInfo> e)
        {
            _tableEntriesParser.Update(_selectedFile, e);

            NextItemController.Update(_selectedFile, e);
            //NextItemController.CustomDisplayText = _selectedFile.DbFile.Name;
        }


        class ParseHelper
        {

            public class ParseResult
            { 
                public FieldInfo FieldInfo { get; set; }
                public long OffsetAfter { get; set; }
            }

            public List<ParseResult> Parse(BinaryReader reader,  long startStreamPos)
            {
                List<ParseResult> output = new List<ParseResult>();

                //long startStreamPos = reader.BaseStream.Position;


                // OptStringTypeAscii
                try
                {
                    var instance = Types.OptStringTypeAscii().CreateInstance();
                    instance.Decode(reader);
                    var value = instance.Value;

                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                    var isAscii = bytes.All(b => b >= 32 && b <= 127);
                    if (!isAscii)
                        throw new Exception();

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }


                // StringTypeAscii
                try
                {
                    var instance = Types.StringTypeAscii().CreateInstance();
                    instance.Decode(reader);
                    var value = instance.Value;

                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                    var isAscii = bytes.All(b => b >= 32 && b <= 127);
                    if (!isAscii)
                        throw new Exception();

                    output.Add( new ParseResult()
                    {
                        FieldInfo = Types.StringTypeAscii(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }

                // OptStringType
                try
                {
                    var instance = Types.OptStringType().CreateInstance();
                    instance.Decode(reader);
                    var value = instance.Value;

                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                    var isAscii = bytes.All(b => b >= 32 && b <= 127);
                    if (isAscii)
                        throw new Exception();

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.OptStringType(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }

                // StringType
                try
                {
                    var instance = Types.StringType().CreateInstance();
                    instance.Decode(reader);
                    var value = instance.Value;

                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                    var isAscii = bytes.All(b => b >= 32 && b <= 127);
                    if (isAscii)
                        throw new Exception();

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.StringType(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }




                // BoolType
                try
                {
                    var instance = Types.BoolType().CreateInstance();
                    instance.Decode(reader);

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.BoolType(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }

                // IntType
                try
                {
                    var instance = Types.IntType().CreateInstance();
                    instance.Decode(reader);

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.IntType(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }

                // SingleType
                try
                {
                    var instance = Types.SingleType().CreateInstance();
                    instance.Decode(reader);

                    output.Add(new ParseResult()
                    {
                        FieldInfo = Types.SingleType(),
                        OffsetAfter = reader.BaseStream.Position
                    });
                }
                catch (Exception e)
                { }
                finally
                {
                    reader.BaseStream.Position = startStreamPos;
                }

                return output;
            }
        }


        //








        //
        private void OnTest()
        {

            try
            {
                if (_selectedFile != null)
                {

                    var currentTableInof = DBTypeMap.Instance.GetVersionedInfos(_selectedFile.TableType, _currentVersion);
                    var entry = currentTableInof.First();
                    int maxNumberOfFields = entry.Fields.Count();

                    var c = new BruteForceParser();
                    c.BruteForce(_selectedFile, maxNumberOfFields);
                }
            }
            catch (Exception e)
            { }
            return;
        }

        FieldInfo[] Create(CaSchemaEntry entry)
        {
            switch (entry.field_type)
            {
                case "yesno":
                    return new FieldInfo[] { Types.BoolType()};
                case "single":
                case "decimal":
                case "double":
                    return new FieldInfo[] { Types.SingleType()};

                case "autonumber":
                case "integer":
                    return new FieldInfo[] { Types.IntType() };
                case "text":
                    return new FieldInfo[] { Types.OptStringTypeAscii(), Types.StringTypeAscii(), Types.OptStringType(), Types.StringType() };
            }

            // case "autonumber": -> // Should be long, but long is not supported
            throw new Exception($"Field '{entry.name}' contains unkown type '{entry.field_type}'");
        }
   


        private void OnDbFileSelected(object sender, DataBaseFile e)
        {
            _selectedFile = e;
            if (_selectedFile == null)
                return;

            ParseDatabaseFile(e);
            DbTableDefinitionController.LoadCurrentTableDefinition(e.TableType, _currentVersion);
            LoadCaSchemaDefinition(e.TableType);
            _tableEntriesParser.Update(e, DbTableDefinitionController.TableTypeInformationRows.Select(x=>x.GetFieldInfo()).ToList());
            NextItemController.Update(_selectedFile, DbTableDefinitionController.TableTypeInformationRows.Select(x => x.GetFieldInfo()).ToList());
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

        #region Temp

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
            var fields = DbTableDefinitionController.TableTypeInformationRows;
            using (var reader = new BinaryReader(new MemoryStream(_selectedFile.DbFile.Data)))
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                foreach (var selection in fields)
                {
                    var instance = selection.GetFieldInfo().CreateInstance();
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
        #endregion
    }
}
