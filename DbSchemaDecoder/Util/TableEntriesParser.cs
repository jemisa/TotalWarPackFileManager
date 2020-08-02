using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Models;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DbSchemaDecoder.Util
{
    class TableEntriesParser
    {
        readonly DataGridItemSourceUpdater _dataGridUpdater;
        readonly DbTableViewModel _viewModel;

        public TableEntriesParser(DataGridItemSourceUpdater dataGridUpdater, DbTableViewModel viewModel)
        {
            _dataGridUpdater = dataGridUpdater;
            _viewModel = viewModel;
        }

        public DbTableViewModel Update(DataBaseFile baseFile, IEnumerable<FieldInfo> fields, int expectedEntries)
        {
            var table = new DataTable();
            _viewModel.ParseResult = "";
            
            try
            {
                foreach (var field in fields)
                    table.Columns.Add(field.Name);

                using (var stream = new MemoryStream(baseFile.DbFile.Data))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        var res = ParseTable(fields, expectedEntries, table, reader);
                        if (res)
                        {
                            var bytesLeftInStream = stream.Capacity - (int)stream.Position;
                            if(bytesLeftInStream != 0)
                                _viewModel.ParseResult = $"Error: Bytes left in stream after parsing : {bytesLeftInStream * 4}";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _viewModel.ParseResult = $"Error:{e.Message}"; 
            }
            finally
            {
                if (_viewModel.ParseResult == "")
                    _viewModel.ResultColour = new SolidColorBrush(Colors.LightGreen);
                else
                    _viewModel.ResultColour = new SolidColorBrush(Colors.Red);

                _viewModel.EntityTable = table;
                if(_dataGridUpdater != null)
                    _dataGridUpdater.SetData(table);
            }
            return _viewModel;
        }

        private bool ParseTable(IEnumerable<FieldInfo> fields, int expectedEntries, DataTable table, BinaryReader reader)
        {
            DBFileHeader header = PackedFileDbCodec.readHeader(reader);
            if (fields.Count() != 0)
            {
                for (int i = 0; i < expectedEntries; i++)
                {
                    var rowResult = ParseRow(reader, fields, i);
                    table.Rows.Add(rowResult.Content.ToArray());

                    if (rowResult.HasError)
                    {
                        _viewModel.ParseResult = rowResult.Error;
                        return false;
                    }
                }
            }

            return true;
        }

        RowResult ParseRow(BinaryReader reader, IEnumerable<FieldInfo> fields, int rowIndex)
        {
            RowResult result = new RowResult();
            foreach (var field in fields)
            {
                var instance = field.CreateInstance();
                var res = instance.TryDecode(reader);
                if (res)
                {
                    var value = instance.Value;
                    result.Content.Add(value);
                }
                else
                {
                    result.Content.Add(instance.Value);
                    result.Error = $"Error parsing column {field.Name} for row {rowIndex + 1} : {instance.Value}";
                    break;
                }
            }
            return result;
        }

        class RowResult
        {
            public List<string> Content { get; set; } = new List<string>();
            public string Error { get; set; } = String.Empty;
            public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
        }

    }
}
