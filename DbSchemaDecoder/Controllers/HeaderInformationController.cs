﻿using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    class HeaderInformationController
    {
        public HeaderInformationViewModel ViewModel { get; set; } = new HeaderInformationViewModel();
        WindowState _eventHub;

        public ICommand ReloadCommand { get; private set; }
        public HeaderInformationController(WindowState eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnFileSelected += (sender, file) => { ParseDatabaseFile(file); };
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
                    _eventHub.DbSchemaFields = newVersion.TypeInfo.Fields;
                }
            }
        }

        void OnReloadTable()
        {
            var newVersion = ViewModel.Versions.FirstOrDefault(x => x.DisplayValue == ViewModel.SelectedVersion);
            if (newVersion != null)
                _eventHub.DbSchemaFields =  newVersion.TypeInfo.Fields;
            else
                _eventHub.DbSchemaFields = new List<FieldInfo>();
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
                ViewModel.Update(header, item);
                OnReloadTable();
            }
        }
    }
}
