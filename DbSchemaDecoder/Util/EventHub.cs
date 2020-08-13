using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Models;
using Filetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DbSchemaDecoder.Util.BatchEvaluator;

namespace DbSchemaDecoder.Util
{
    class EventHub
    {
        public void TriggerOnFileSelected(object sender, DataBaseFile file)
        {
            OnFileSelected?.Invoke(sender, file);
        }

        // File handling
        public event EventHandler<DataBaseFile> OnFileSelected;
        // public event EventHandler<List<Result>> OnFilesValidated; not needed?

        // Ca schema
        public void TriggerCaSchemaLoaded(object sender, List<CaSchemaEntry> caSchemaEntries)
        {
            OnCaSchemaLoaded?.Invoke(sender, caSchemaEntries);
        }

        public event EventHandler<List<CaSchemaEntry>> OnCaSchemaLoaded;

        // DataBase schema
        public void TriggerSetDbSchema(object sender, List<FieldInfo> newDbSchema)
        {
            OnSetDbSchema?.Invoke(sender, newDbSchema);
        }

        public event EventHandler<List<FieldInfo>> OnSetDbSchema;

        public void TriggerOnDbSchemaChanged(object sender, List<FieldInfo> newDbSchema)
        {
            OnDbSchemaChanged?.Invoke(sender, newDbSchema);
        }
        public event EventHandler<List<FieldInfo>> OnDbSchemaChanged;

        public void TriggerOnSelectedDbSchemaRowChanged(object sender, FieldInfoViewModel selectedField)
        {
            OnSelectedDbSchemaRowChanged?.Invoke(sender, selectedField);
        }
        public event EventHandler<FieldInfoViewModel> OnSelectedDbSchemaRowChanged;



        public void TriggerNewDbSchemaRowCreated(object sender, DbTypesEnum newRowType)
        {
            OnNewDbSchemaRowCreated?.Invoke(sender, newRowType);
        }
        public event EventHandler<DbTypesEnum> OnNewDbSchemaRowCreated;





        public void TriggerOnHeaderVersionChanged(object sender, int headerVersion)
        {
            OnHeaderVersionChanged?.Invoke(sender, headerVersion);
        }
        public event EventHandler<int> OnHeaderVersionChanged;
    }
}
