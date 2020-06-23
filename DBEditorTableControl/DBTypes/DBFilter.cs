using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DBTableControl
{
    public class DBFilter : INotifyPropertyChanged
    {
        bool isActive;
        public bool IsActive 
        {
            get { return isActive; } 
            set 
            {
                isActive = value;

                if (FilterToggled != null)
                {
                    FilterToggled(this, null);
                }
            } 
        }

        string name;
        public string Name 
        { 
            get { return name; } 
            set 
            { 
                name = value;
                NotifyPropertyChanged(this, "Name");
            } 
        }

        string applyToColumn;
        public string ApplyToColumn { get { return applyToColumn; } set { applyToColumn = value; } }

        string filterValue;
        public string FilterValue { get { return filterValue; } set { filterValue = value; } }

        MatchType matchMode;
        public MatchType MatchMode { get { return matchMode; } set { matchMode = value; } }

        public event EventHandler FilterToggled;
        public event PropertyChangedEventHandler PropertyChanged;

        public DBFilter()
        {
            isActive = false;
            name = "";
            applyToColumn = "";
            filterValue = "";
        }

        public DBFilter(bool _ischecked, string _text, string _applytocolumn, string _filtervalue)
        {
            isActive = _ischecked;
            name = _text;
            applyToColumn = _applytocolumn;
            filterValue = _filtervalue;
        }

        private void NotifyPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public enum MatchType
    {
        Exact, 
        Partial,
        Regex,
        NotEmpty,
        Empty
    }
}
