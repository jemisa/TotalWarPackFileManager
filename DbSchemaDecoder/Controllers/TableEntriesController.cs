﻿using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace DbSchemaDecoder.Controllers
{
    class TableEntriesController
    {
        public DbTableViewModel ViewModel { get; set; } = new DbTableViewModel();

        readonly DataGridItemSourceUpdater _dataGridUpdater;
        readonly TableEntriesParser _parser = new TableEntriesParser();


        DataBaseFile _seletedFile;
        EventHub _eventHub;
        public TableEntriesController(EventHub eventHub, DataGridItemSourceUpdater dataGridUpdater)
        {
            _eventHub = eventHub;
            _dataGridUpdater = dataGridUpdater;

            _eventHub.OnFileSelected += (sender, file) => { _seletedFile = file; };
            _eventHub.OnDbSchemaChanged += (sender, schama) => { Update(schama); };
        }

        void Update(IEnumerable<FieldInfo> schama)
        {
            if (_seletedFile == null)
                return;

            DataTable table = new DataTable();
            ViewModel.ParseResult = "";
            try
            {
                using (var stream = new MemoryStream(_seletedFile.DbFile.Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                        var parseResult = _parser.ParseTable(reader, stream.Capacity, schama.ToList(), (int)header.EntryCount);
                        if (parseResult.HasError)
                            ViewModel.ParseResult = parseResult.Error;

                        try
                        {
                            if (parseResult.ColumnNames != null)
                            {
                                foreach (var columnName in parseResult.ColumnNames)
                                    table.Columns.Add(columnName);

                                if (parseResult.DataRows != null)
                                {
                                    foreach (var row in parseResult.DataRows)
                                        table.Rows.Add(row);
                                }
                            }
                        }
                        catch
                        { 
                            
                        }
                       
                    }
                }
            }
            catch (Exception e)
            {
                ViewModel.ParseResult = $"Error:{e.Message}"; 
            }
            finally
            {
                if (ViewModel.ParseResult == "")
                    ViewModel.ResultColour = new SolidColorBrush(Colors.LightGreen);
                else
                    ViewModel.ResultColour = new SolidColorBrush(Colors.Red);

                ViewModel.EntityTable = table;
                if(_dataGridUpdater != null)
                    _dataGridUpdater.SetData(table);
            }
        }
    }
}
