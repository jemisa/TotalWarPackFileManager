using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Controllers
{
    class CaSchemaController
    {
        public ObservableCollection<CaSchemaEntry> CaSchemaEntries { get; set; } = new ObservableCollection<CaSchemaEntry>();

        EventHub _eventHub;
        
        public CaSchemaController(EventHub eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnFileSelected += _eventHub_OnFileSelected;
        }

        private void _eventHub_OnFileSelected(object sender, DataBaseFile e)
        {
            LoadCaSchemaDefinition(e.TableType);
        }

        void LoadCaSchemaDefinition(string tableName)
        {
            CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
            var res = caSchemaFileParser.Load(tableName);
            CaSchemaEntries.Clear();
            foreach (var x in res.Entries)
                CaSchemaEntries.Add(x);

            _eventHub.TriggerCaSchemaLoaded(this, CaSchemaEntries.ToList());
        }
    }
}
