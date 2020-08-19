﻿using DbSchemaDecoder.Util;

namespace DbSchemaDecoder.Controllers
{
    public class DbSchemaDecoderController : NotifyPropertyChangedImpl
    {
        public NextItemController NextItemController { get; set; }
        public DbTableDefinitionController DbTableDefinitionController { get; set; }
        public BruteForceController BruteForceController { get; set; }
        public CaSchemaController CaSchemaController { get; set; }
        public TableEntriesController TableEntriesController { get; set; }
        public HeaderInformationController SelectedFileHeaderInformationController { get; set; }

        WindowState _windowState;

        public DbSchemaDecoderController(WindowState eventHub, DataGridItemSourceUpdater dbTableItemSourceUpdater)
        {
            _windowState = eventHub;

            TableEntriesController = new TableEntriesController(_windowState, dbTableItemSourceUpdater);
            NextItemController = new NextItemController(_windowState);
            BruteForceController = new BruteForceController(_windowState);
            CaSchemaController = new CaSchemaController(_windowState);
            DbTableDefinitionController = new DbTableDefinitionController(_windowState);
            SelectedFileHeaderInformationController = new HeaderInformationController(_windowState);


            DbSchemaVersionUpdater updater = new DbSchemaVersionUpdater();
            updater.Convert();
        }
    }
}
