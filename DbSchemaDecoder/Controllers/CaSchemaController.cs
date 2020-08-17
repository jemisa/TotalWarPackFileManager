using DbSchemaDecoder.Util;
using System.Collections.ObjectModel;
using System.Linq;

namespace DbSchemaDecoder.Controllers
{
    class CaSchemaController
    {
        public ObservableCollection<CaSchemaEntry> CaSchemaEntries { get; set; } = new ObservableCollection<CaSchemaEntry>();

        WindowState _windowState;
        
        public CaSchemaController(WindowState windowState)
        {
            _windowState = windowState;
            _windowState.OnFileSelected += (sender, file) => { LoadCaSchemaDefinition(file.TableType); };
        }

        void LoadCaSchemaDefinition(string tableName)
        {
            CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
            var res = caSchemaFileParser.Load(tableName);
            CaSchemaEntries.Clear();
            foreach (var x in res.Entries)
                CaSchemaEntries.Add(x);

            _windowState.CaSchema = CaSchemaEntries.ToList();
        }
    }
}
