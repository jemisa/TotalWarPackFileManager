using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public DbTableViewModel DbTableViewModel { get; set; } = new DbTableViewModel();
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
        CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
        TableEntriesUpdater _tableEntriesParser;

        public ICommand DbDefinitionRemovedCommand { get; private set; }
        public ICommand DbDefinitionMovedUpCommand { get; private set; }
        public ICommand DbDefinitionMovedDownCommand { get; private set; }

        public ICommand ParseTbTableUsingCaSchemaCommand { get; private set; }

        public DbSchemaDecoderController(FileListController fileListController, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {
            _tableEntriesParser = new TableEntriesUpdater(dbTableItemSourceUpdater, DbTableViewModel);
            fileListController.OnFileSelectedEvent += OnDbFileSelected;
            TestValue = "MyString is cool";

            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
            ParseTbTableUsingCaSchemaCommand = new RelayCommand(OnTest);
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

            try 
            {
          
            // Converts
                List<FieldInfo> values = new List<FieldInfo>();



                using (var stream = new MemoryStream(_selectedFile.DbFile.Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                        foreach (var caField in CaSchemaEntries)
                        {

                            var types = Create(caField);
                            foreach (var type in types)
                            {
                                bool worked = true;
                                long showFrom = reader.BaseStream.Position;
                                


                                try
                                {



                                    var instance = type.CreateInstance();
                                    instance.Decode(reader);
                                    var value = instance.Value;

                                    byte[] bytes = Encoding.ASCII.GetBytes(value);
                                    var isAscii = bytes.All(b => b >= 32 && b <= 127);

                                }
                                catch (Exception e)
                                {
                                    reader.BaseStream.Position = showFrom;
                                    worked = false;
                                }

                                if (worked)
                                {
                                    type.Name = caField.name;
                                    values.Add(type);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Parse
               /* TableEntriesParser p = new TableEntriesParser(null, new DbTableViewModel());
                var x = p.Update(
                    _selectedFile,
                    values,
                    SelectedFileHeaderInformation.ExpectedEntries);


                if (x.ParseResult != "")
                    throw new Exception(x.ParseResult);*/
            }
            catch(Exception e)
            {


            }
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
            if (_selectedFile == null)
                return;

            ParseDatabaseFile(e);
            LoadCurrentTableDefinition(e.TableType, _currentVersion);
            LoadCaSchemaDefinition(e.TableType);
            _tableEntriesParser.Update(e, SelectedTableTypeInformations);
        }

        void LoadCurrentTableDefinition(string tableName, int currentVersion)
        {
            var allTableDefinitions = DBTypeMap.Instance.GetVersionedInfos(tableName, currentVersion);
            SelectedTableTypeInformations.Clear();

            var fieldCollection = allTableDefinitions.FirstOrDefault(x => x.Version == currentVersion);
            if (fieldCollection == null)
                return;

            foreach(var f in fieldCollection.Fields)
                SelectedTableTypeInformations.Add(f); 
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
        #endregion
    }
}
