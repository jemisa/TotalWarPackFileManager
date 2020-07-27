using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    class FileListModel : INotifyPropertyChanged
    {
        PackFile _tabel;
        string _fileName;
        bool _validState;

        public PackFile Table
        {
            get
            {
                return _tabel;
            }
            set
            {
                _tabel = value;
                OnPropertyChanged("Table");
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public bool ValidState
        {
            get
            {
                return _validState;
            }
            set
            {
                _validState = value;
                OnPropertyChanged("ValidState");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FileListModel SelectedItem
        {
            get { return null; }
            set
            {

                OnPropertyChanged("SelectedItem");

                // selection changed - do something special
            }
        }
    }
}
