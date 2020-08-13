using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Controllers
{
    class SelectedFileHeaderInformationController
    {
        public SelectedFileHeaderInformationViewModel ViewModel { get; set; } = new SelectedFileHeaderInformationViewModel();
        EventHub _eventHub;

        public SelectedFileHeaderInformationController(EventHub eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnFileSelected += (sender, file) => { ParseDatabaseFile(file); };
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
                if (header != null)
                {
                    _eventHub.TriggerOnHeaderVersionChanged(this, header.Version);
                }
                ViewModel.Update(header, item);
            }
        }
    }
}
