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

        int _version;
        int _headerSize;
        uint _expectedEntries;
        string _tableName;
        int _totalSize;
        bool _hasHeaderData = false;

        public void Update(DBFileHeader header, DataBaseFile item)
        {
            _hasHeaderData = header != null;
            _tableName = item.TableType;
            _version = header.Version;
            _headerSize = header.Length;
            _expectedEntries = header.EntryCount;
            _totalSize = item.DbFile.Data.Length * 4;


            NotifyPropertyChanged("TableName");
            NotifyPropertyChanged("Version");
            NotifyPropertyChanged("HeaderSize");
            NotifyPropertyChanged("ExpectedEntries");
            NotifyPropertyChanged("TotalSize");
        }
        public string TableName
        {
            get { return GetStr("Name: ", _tableName); }
        }

        public string HeaderSize
        {
            get { return GetStr("Header Size: ", _headerSize); }
        }

        public string Version
        {
            get { return GetStr("Version: ", _version); }
        }

        public string ExpectedEntries
        {
            get { return GetStr("Exected Entries: ", _expectedEntries); }
        }

        public string TotalSize
        {
            get { return GetStr("Total Size: ", _totalSize); }
        }

        string GetStr<T>(string text, T value)
        {
            if (_hasHeaderData)
                return text + value;
            return text;
        }
    }
}
