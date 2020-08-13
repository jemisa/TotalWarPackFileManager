﻿using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DbSchemaDecoder.Models
{
    class DatabaseFileViewModel : NotifyPropertyChangedImpl
    {
        public DataBaseFile DataBaseFile { get; set; }

        string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        Color _backgroundColour;
        public Color BackgroundColour
        {
            get
            {
                return _backgroundColour;
            }
            set
            {
                _backgroundColour = value;
                NotifyPropertyChanged();
            }
        }
    }
}
