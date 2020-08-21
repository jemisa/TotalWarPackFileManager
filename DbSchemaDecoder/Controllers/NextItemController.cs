using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using Filetypes.DB;
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

    public class NextItemControllerItem : NotifyPropertyChangedImpl
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

    public class NextItemController : NotifyPropertyChangedImpl
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


        WindowState _windowState;
        public List<NextItemControllerItem> Items { get; set; } = new List<NextItemControllerItem>();
        public NextItemController(WindowState windowState)
        {
            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Float);
            Create(DbTypesEnum.Boolean);

            _windowState = windowState;
            _windowState.OnFileSelected += (sender, file) => { Update(); };
            _windowState.OnSetDbSchema += (sender, schama) => {  Update(); };
            _windowState.OnSelectedDbSchemaRowChanged += (sender, item) => { Update(); };
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

        void UpdateViewModel(NextItemControllerItem viewModelRef, byte[] data, int index)
        {
            var parser = ParserFactory.Create(viewModelRef.EnumValue);
            var result = parser.TryDecode(data, index, out string value, out var _, out string error);
            if (result == false)
                viewModelRef.ValueText = "Error:" + error;
            else
                viewModelRef.ValueText = value;
        }

        public void OnButtonPressed(NextItemControllerItem val)
        {
            if (_windowState.SelectedDbSchemaRow != null)
                _windowState.SelectedDbSchemaRow.Type = val.EnumValue;
            else
                _windowState.TriggerNewDbSchemaRowCreated(this, val.EnumValue);
        }

        void Update()
        {
            if (_windowState.DbSchemaFields == null || _windowState.SelectedFile == null)
                return;

            if (_windowState.SelectedDbSchemaRow != null)
            {
                HelperText = $"Update type for field '{_windowState.SelectedDbSchemaRow.Name}' at Index '{_windowState.SelectedDbSchemaRow.Index}'";
                Items.ForEach(x => x.ButtonText = "Update");
            }
            else
            {
                HelperText = "Create a new field";
                Items.ForEach(x => x.ButtonText = "Add");
            }

            using (var stream = new MemoryStream(_windowState.SelectedFile.DbFile.Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                    int index = header.Length;
                    var endIndex = _windowState.DbSchemaFields.Count();
                    if (_windowState.SelectedDbSchemaRow != null)
                        endIndex = _windowState.SelectedDbSchemaRow.Index - 1;
                    for (int i = 0; i < endIndex; i++)
                    {
                        var byteParserType = _windowState.DbSchemaFields[i].Type;
                        var parser = ParserFactory.Create(byteParserType);
                        parser.TryDecode(_windowState.SelectedFile.DbFile.Data, index, out _, out var bytesRead, out _);
                        index += bytesRead;
                    }


                    for (int i = 0; i < Items.Count; i++)
                    {
                        UpdateViewModel(Items[i], _windowState.SelectedFile.DbFile.Data, index);
                    }
                }
            }
        }

    }
}
