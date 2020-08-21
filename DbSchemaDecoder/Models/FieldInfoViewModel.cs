using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DbSchemaDecoder.Models
{
    public class FieldInfoViewModel : NotifyPropertyChangedImpl
    {

        public int Index { get { return _index; } }
        bool _use = true;
        int _index;
        public bool Use { get { return _use; } set { _use = value; NotifyPropertyChanged(); } }
        public string Name { get { return _fieldInfo.MetaData.Name; } set { _fieldInfo.MetaData.Name = value; NotifyPropertyChanged(); } }
        public DbTypesEnum Type
        {
            get { return _fieldInfo.Type; }
            set
            {
                _fieldInfo.Type = value;
                NotifyPropertyChanged();
            }
        }

        public bool PrimaryKey { get { return _fieldInfo.MetaData.IsKey; } set { _fieldInfo.MetaData.IsKey = value; NotifyPropertyChanged(); } }
        public bool Optional { get { return _fieldInfo.MetaData.IsOptional; } set { _fieldInfo.MetaData.IsOptional = value; NotifyPropertyChanged(); } }
        public string ReferencedField { get { return _fieldInfo.MetaData.FieldReference; } set { _fieldInfo.MetaData.FieldReference = value; NotifyPropertyChanged(); } }
        public string ReferencedTable { get { return _fieldInfo.MetaData.TableReference; } set { _fieldInfo.MetaData.TableReference = value; NotifyPropertyChanged(); } }

        public FieldInfoViewModel(DbColumnDefinition info, int idx)
        {
            _index = idx;
            _fieldInfo = info;
        }

        DbColumnDefinition _fieldInfo;
        public DbColumnDefinition GetFieldInfo() { return _fieldInfo; }
        public void SetIndex(int idx) { _index = idx; }
    }
}
