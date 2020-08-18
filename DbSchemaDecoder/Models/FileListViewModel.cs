using Common;
using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    public class FileListViewModel : NotifyPropertyChangedImpl
    {
        string _searchFilter;
        public string SearchFilter
        {
            get
            {
                return _searchFilter;
            }
            set
            {
                _searchFilter = value;
                NotifyPropertyChanged();
            }
        }

        int _totalDbFiles;
        public int TotalDbFiles
        {
            get
            {
                return _totalDbFiles;
            }
            set
            {
                _totalDbFiles = value;
                NotifyPropertyChanged();
            }
        }


        int _dbFilesWithError;
        public int DbFilesWithError
        {
            get
            {
                return _dbFilesWithError;
            }
            set
            {
                _dbFilesWithError = value;
                NotifyPropertyChanged();
            }
        }

        bool _onlyShowTablesWithErrors;
        public bool OnlyShowTablesWithErrors
        {
            get
            {
                return _onlyShowTablesWithErrors;
            }
            set
            {
                _onlyShowTablesWithErrors = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<DatabaseFileViewModel> FileList { get; set; } = new ObservableCollection<DatabaseFileViewModel>();

    }
}
