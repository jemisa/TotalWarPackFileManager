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

    //



    class NextItemController : NotifyPropertyChangedImpl
    {
        //public NextItemControllerItem StringItem { get; set; } = new NextItemControllerItem();
        //public NextItemControllerItem IntItem { get; set; } = new NextItemControllerItem();

        public List<NextItemControllerItem> Items { get; set; } = new List<NextItemControllerItem>();
        public NextItemController()
        {
            /*StringItem.CustomDisplayText = "String";
            StringItem.CustomButtonPressedCommand = new RelayCommand<object>(OnTest);
            IntItem.CustomDisplayText = "Int";*/

            
            Create(DbTypesEnum.String_ascii);
            Create(DbTypesEnum.Optstring_ascii);
            Create(DbTypesEnum.String);
            Create(DbTypesEnum.Optstring);
            Create(DbTypesEnum.Integer);
            Create(DbTypesEnum.Float);
            Create(DbTypesEnum.Boolean);
        }

        void Create(DbTypesEnum enumValue)
        {
            var type = Types.FromEnum(enumValue);
            NextItemControllerItem viewModel = new NextItemControllerItem();
            viewModel.EnumValue = enumValue;
            viewModel.CustomDisplayText = type.TypeName;
            viewModel.ButtonText = "Flygplan";
            viewModel.CustomButtonPressedCommand = new RelayCommand<object>(OnTest);
            //UpdateViewModel(viewModel);
            Items.Add(viewModel);
        }

        void UpdateViewModel(NextItemControllerItem viewModelRef, BinaryReader reader)
        {
            var type = Types.FromEnum(viewModelRef.EnumValue);
            var instanece = type.CreateInstance();
            var res = instanece.TryDecode(reader);
            viewModelRef.ValueText = instanece.Value;
        }

        public void OnTest(object val)
        { 
        
        }

        public void Update( DataBaseFile baseFile, IEnumerable<FieldInfo> fields)
        {
            using (var stream = new MemoryStream(baseFile.DbFile.Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);

                    foreach (var existingField in fields)
                        existingField.CreateInstance().TryDecode(reader);

                    var refPos = reader.BaseStream.Position;

                    foreach (var item in Items)
                    {
                        UpdateViewModel(item, reader);
                        reader.BaseStream.Position = refPos;
                    }
                }
            }
        }

    }
}
