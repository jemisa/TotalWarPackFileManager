using DbSchemaDecoder.Util;
using System.Collections.ObjectModel;
using System.Linq;

namespace DbSchemaDecoder.Controllers
{
    class CaSchemaController
    {
        public ObservableCollection<CaSchemaEntry> CaSchemaEntries { get; set; } = new ObservableCollection<CaSchemaEntry>();

        WindowState _eventHub;
        
        public CaSchemaController(WindowState eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnFileSelected += (sender, file) => { LoadCaSchemaDefinition(file.TableType); };
        }

        void LoadCaSchemaDefinition(string tableName)
        {
            CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
            var res = caSchemaFileParser.Load(tableName);
            CaSchemaEntries.Clear();
            foreach (var x in res.Entries)
                CaSchemaEntries.Add(x);

            _eventHub.CaSchema = CaSchemaEntries.ToList();
        }
    }
}
