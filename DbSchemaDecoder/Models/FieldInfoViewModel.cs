using DbSchemaDecoder.Util;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    class FieldInfoViewModel : NotifyPropertyChangedImpl
    {

        public int Index { get { return _index; } }
        bool _use = true;
        int _index;
        public bool Use { get { return _use; } set { _use = value; NotifyPropertyChanged(); } }
        public string Name { get { return _fieldInfo.Name; } set { _fieldInfo.Name = value; NotifyPropertyChanged(); } }
        public DbTypesEnum Type
        {
            get { return _fieldInfo.TypeEnum; }
            set
            {
                var newInstance = Types.FromEnum(value);
                newInstance.Name = _fieldInfo.Name;
                newInstance.FieldReference = _fieldInfo.FieldReference;
                newInstance.Optional = _fieldInfo.Optional;
                newInstance.PrimaryKey = _fieldInfo.PrimaryKey;
                _fieldInfo = newInstance;
                NotifyPropertyChanged();
            }
        }

        public bool PrimaryKey { get { return _fieldInfo.PrimaryKey; } set { _fieldInfo.PrimaryKey = value; } }
        public bool Optional { get { return _fieldInfo.Optional; } set { _fieldInfo.Optional = value; } }
        public string ReferencedTable { get { return _fieldInfo.ReferencedTable; } }
        public string ReferencedField { get { return _fieldInfo.ReferencedField; } set { _fieldInfo.ReferencedField = value; } }

        public FieldInfoViewModel(FieldInfo info, int idx)
        {
            _index = idx;
            _fieldInfo = info;
        }

        FieldInfo _fieldInfo;
        public FieldInfo GetFieldInfo() { return _fieldInfo; }
        public void SetIndex(int idx) { _index = idx; }
    }
}
