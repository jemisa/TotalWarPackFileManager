using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{

    class NextItemControllerItem : NotifyPropertyChangedImpl
    {
        string _CustomDisplayText;
        public string CustomDisplayText
        {
            get { return _CustomDisplayText; }
            set
            {
                _CustomDisplayText = value;
                NotifyPropertyChanged();
            }
        }

        string _valueText;
        public string ValueText
        {
            get { return _valueText; }
            set
            {
                _valueText = value;
                NotifyPropertyChanged();
            }
        }

        string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                _buttonText = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand CustomButtonPressedCommand { get;  set; }
        public DbTypesEnum EnumValue { get; set; }

    }

    class NextItemController : NotifyPropertyChangedImpl
    {
        FieldInfoViewModel _selectedRowFieldItem;
        
        string _helperText;
        public string HelperText
        {
            get { return _helperText; }
            set
            {
                _helperText = value;
                NotifyPropertyChanged();
            }
        }


        EventHub _eventHub;
        DataBaseFile _selectedFile;
        List<FieldInfo> _dbSchemaDefinition;
        public List<NextItemControllerItem> Items { get; set; } = new List<NextItemControllerItem>();
        public NextItemController(EventHub eventHub)
        {
            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Float);
            Create(DbTypesEnum.Boolean);

            _eventHub = eventHub;
            _eventHub.OnFileSelected += (sender, file) => { _selectedFile = file; Update(); };
            _eventHub.OnDbSchemaChanged += (sender, schama) => { _dbSchemaDefinition = schama; Update(); };
            _eventHub.OnSelectedDbSchemaRowChanged += (sender, item) => { _selectedRowFieldItem = item; Update(); };
        }


        void Create(DbTypesEnum enumValue)
        {
            var type = Types.FromEnum(enumValue);
            NextItemControllerItem viewModel = new NextItemControllerItem();
            viewModel.EnumValue = enumValue;
            viewModel.CustomDisplayText = type.TypeName;
            viewModel.ButtonText = "Add";
            viewModel.CustomButtonPressedCommand = new RelayCommand<NextItemControllerItem>(OnButtonPressed);
            Items.Add(viewModel);
        }

        void UpdateViewModel(NextItemControllerItem viewModelRef, BinaryReader reader)
        {
            var type = Types.FromEnum(viewModelRef.EnumValue);
            var instanece = type.CreateInstance();
            var result = instanece.TryDecode(reader);
            if(result == false)
                viewModelRef.ValueText = "Error:" + instanece.Value;
            else
                viewModelRef.ValueText = instanece.Value;
        }

        public void OnButtonPressed(NextItemControllerItem val)
        {
            if (_selectedRowFieldItem != null)
                _selectedRowFieldItem.Type = val.EnumValue;
            else
                _eventHub.TriggerNewDbSchemaRowCreated(this, val.EnumValue);
        }

        void Update()
        {
            if (_dbSchemaDefinition == null || _selectedFile == null)
                return;

            if (_selectedRowFieldItem != null)
            {
                HelperText = $"Update type for field '{_selectedRowFieldItem.Name}' at Index '{_selectedRowFieldItem.Index}'";
                Items.ForEach(x => x.ButtonText = "Update");
            }
            else
            {
                HelperText = "Create a new field";
                Items.ForEach(x => x.ButtonText = "Add");
            }

            using (var stream = new MemoryStream(_selectedFile.DbFile.Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);

                    var endIndex = _dbSchemaDefinition.Count();
                    if (_selectedRowFieldItem != null)
                        endIndex = _selectedRowFieldItem.Index - 1;
                    for (int i = 0; i < endIndex; i++)
                    {
                        _dbSchemaDefinition[i].CreateInstance().TryDecode(reader);
                    }

                    var refPos = reader.BaseStream.Position;
                    for (int i = 0; i < Items.Count; i++)
                    {
                        UpdateViewModel(Items[i], reader);
                        reader.BaseStream.Position = refPos;
                    }
                }
            }
        }

    }
}
