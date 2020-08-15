using DbSchemaDecoder.Util;

namespace DbSchemaDecoder.Controllers
{
    class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public NextItemController NextItemController { get; set; }
        public DbTableDefinitionController DbTableDefinitionController { get; set; }
        public BruteForceController BruteForceController { get; set; }
        public CaSchemaController CaSchemaController { get; set; }
        public TableEntriesController TableEntriesController { get; set; }
        public HeaderInformationController SelectedFileHeaderInformationController { get; set; }

        EventHub _eventHub;

        public DbSchemaDecoderController(EventHub eventHub, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {
            _eventHub = eventHub;

            TableEntriesController = new TableEntriesController(_eventHub, dbTableItemSourceUpdater);
            NextItemController = new NextItemController(_eventHub);
            BruteForceController = new BruteForceController(_eventHub);
            CaSchemaController = new CaSchemaController(_eventHub);
            DbTableDefinitionController = new DbTableDefinitionController(_eventHub);
            SelectedFileHeaderInformationController = new HeaderInformationController(_eventHub);
        }
    }
}
