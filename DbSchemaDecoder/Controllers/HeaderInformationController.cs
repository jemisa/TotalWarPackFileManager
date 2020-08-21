using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.Codecs;
using Filetypes.DB;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    public class HeaderInformationController
    {
        public HeaderInformationViewModel ViewModel { get; set; } = new HeaderInformationViewModel();
        WindowState _windowState;

        public ICommand ReloadCommand { get; private set; }
        public HeaderInformationController(WindowState windowState)
        {
            _windowState = windowState;
            _windowState.OnFileSelected += (sender, file) => { ParseDatabaseFile(file); };
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            ReloadCommand = new RelayCommand(OnReloadTable);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.SelectedVersion))
            {
                var newVersion = ViewModel.Versions.FirstOrDefault(x => x.DisplayValue == ViewModel.SelectedVersion);
                if (newVersion != null)
                {
                    _windowState.DbSchemaFields = newVersion.TypeInfo.ColumnDefinitions;
                }
            }
        }

        void OnReloadTable()
        {
            var newVersion = ViewModel.Versions.FirstOrDefault(x => x.DisplayValue == ViewModel.SelectedVersion);
            if (newVersion != null)
                _windowState.DbSchemaFields =  newVersion.TypeInfo.ColumnDefinitions;
            else
                _windowState.DbSchemaFields = new List<DbColumnDefinition>();
        }

        void ParseDatabaseFile(DataBaseFile item)
        {
            if (item == null)
                return;

            var bytes = item.DbFile.Data;
            if (bytes == null)
                return;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(bytes)))
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                ViewModel.Update(header, item, _windowState.SchemaManager, _windowState.CurrentGame.GameType);
                OnReloadTable();
            }
        }
    }
}
