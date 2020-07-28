using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    class SelectedFileHeaderInformation : NotifyPropertyChangedImpl
    {
        public void Update(DBFileHeader header, DataBaseFile item)
        {
            if (header != null)
            {
                TableName = item.TableType;
                Version = header.Version;
                HeaderSize = header.Length;
                ExpectedEntries = (int)header.EntryCount;
                TotalSize = item.DbFile.Data.Length;
            }
            else
            {
                TableName = "";
                Version = 0;
                HeaderSize = 0;
                ExpectedEntries = 0;
                TotalSize = 0;
            }

            NotifyPropertyChanged("TableName");
            NotifyPropertyChanged("Version");
            NotifyPropertyChanged("HeaderSize");
            NotifyPropertyChanged("ExpectedEntries");
            NotifyPropertyChanged("TotalSize");
        }
        public string TableName { get; set; }
        public int HeaderSize { get; set; }
        public int Version { get; set; }
        public int ExpectedEntries { get; set; }
        public int TotalSize { get;set;}
    }
}
