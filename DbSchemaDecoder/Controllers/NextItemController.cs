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


        WindowState _eventHub;
        public List<NextItemControllerItem> Items { get; set; } = new List<NextItemControllerItem>();
        public NextItemController(WindowState eventHub)
        {
            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Float);
            Create(DbTypesEnum.Boolean);

            _eventHub = eventHub;
            _eventHub.OnFileSelected += (sender, file) => { Update(); };
            _eventHub.OnSetDbSchema += (sender, schama) => {  Update(); };
            _eventHub.OnSelectedDbSchemaRowChanged += (sender, item) => { Update(); };
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
            if (_eventHub.SelectedDbSchemaRow != null)
                _eventHub.SelectedDbSchemaRow.Type = val.EnumValue;
            else
                _eventHub.TriggerNewDbSchemaRowCreated(this, val.EnumValue);
        }

        void Update()
        {
            if (_eventHub.DbSchemaFields == null || _eventHub.SelectedFile == null)
                return;

            if (_eventHub.SelectedDbSchemaRow != null)
            {
                HelperText = $"Update type for field '{_eventHub.SelectedDbSchemaRow.Name}' at Index '{_eventHub.SelectedDbSchemaRow.Index}'";
                Items.ForEach(x => x.ButtonText = "Update");
            }
            else
            {
                HelperText = "Create a new field";
                Items.ForEach(x => x.ButtonText = "Add");
            }

            using (var stream = new MemoryStream(_eventHub.SelectedFile.DbFile.Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);

                    var endIndex = _eventHub.DbSchemaFields.Count();
                    if (_eventHub.SelectedDbSchemaRow != null)
                        endIndex = _eventHub.SelectedDbSchemaRow.Index - 1;
                    for (int i = 0; i < endIndex; i++)
                    {
                        _eventHub.DbSchemaFields[i].CreateInstance().TryDecode(reader);
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
