using DbSchemaDecoder.Util;
using System.Data;

namespace DbSchemaDecoder.Models
{
    class DbTableViewModel : NotifyPropertyChangedImpl
    {
        public DataTable EntityTable { get; set; } = new DataTable();

        string _parseResult;
        public string ParseResult
        {
            get { return _parseResult; }
            set
            {
                _parseResult = value;
               // NotifyPropertyChanged();
            }
        }

        System.Windows.Media.Brush _resultColour;
        public System.Windows.Media.Brush ResultColour
        {
            get { return _resultColour; }
            set
            {
                _resultColour = value;
                //NotifyPropertyChanged();
            }
        }


    }
}
