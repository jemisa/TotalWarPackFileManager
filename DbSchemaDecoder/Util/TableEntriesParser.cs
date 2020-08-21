using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{
    class TableEntriesParser
    {
        byte[] _tableData;
        int _dataIndex;
        public TableEntriesParser(byte[] tableData, int indexOfTableData)
        {
            _tableData = tableData;
            _dataIndex = indexOfTableData;
        }

        /// This function expects that the header of the file is already read!
        public TableResult ParseTable(List<DbColumnDefinition> columnDefinitions, int expectedEntries)
        {
            TableResult output = new TableResult();
            if (columnDefinitions.Count() != 0)
            {
                // Create the headers
                output.ColumnNames = new string[columnDefinitions.Count];
                for (int i = 0; i < columnDefinitions.Count; i++)
                    output.ColumnNames[i] = columnDefinitions[i].MetaData.Name;

                // Find the actuall parsers
                var fieldInstances = columnDefinitions.Select(x => ParserFactory.Create(x.Type)).ToArray();

                // Parse the table
                output.DataRows = new string[expectedEntries][];
                for (int i = 0; i < expectedEntries; i++)
                {
                    var rowResult = ParseRow(fieldInstances, columnDefinitions, i);
                    output.DataRows[i] = rowResult.Content;
                    if (rowResult.HasError)
                    {
                        output.Error = rowResult.Error;
                    }
                }
            }

            CheckForEndOfTableError(output);
            return output;
        }

        RowResult ParseRow( ByteParser[] fields, List<DbColumnDefinition> columnDefinitions, int rowIndex)
        {
            int fieldsCount = fields.Count();
            RowResult result = new RowResult()
            {
                Content = new string[fieldsCount],
            };

            for (int i = 0; i < fieldsCount; i++)
            {
                var parseResult = fields[i].TryDecode(_tableData, _dataIndex, out string value, out int bytesReas, out string error);
                
                result.Content[i] = value;
                if (!parseResult)
                {
                    result.Error = $"Error parsing column {columnDefinitions[i].MetaData.Name} for row {rowIndex + 1} : {error}";
                    break;
                }
                _dataIndex += bytesReas;
            }
            return result;
        }

        // A much faster version that just checks if the table can be parsed using the current field configuration
        public TableResult CanParseTable(List<DbColumnDefinition> columnDefinitions, int expectedEntries)
        {
            TableResult output = new TableResult();
            if (columnDefinitions.Count() != 0)
            {
                // Find the actuall parsers
                var fieldInstances = columnDefinitions.Select(x => ParserFactory.Create(x.Type)).ToArray();

                for (int rowIndex = 0; rowIndex < expectedEntries; rowIndex++)
                {
                    for (int i = 0; i < fieldInstances.Length; i++)
                    {
                        //string errorMessage = "";
                        var parseResult = fieldInstances[i].CanDecode(_tableData, _dataIndex, out int bytesRead, out string error);
                        if (!parseResult)
                        {
                            output.Error = $"Error parsing column {columnDefinitions[i].MetaData.Name} for row {rowIndex + 1} - Error : {error}";
                            break;
                        }

                        _dataIndex += bytesRead;
                    }
                    if (output.HasError)
                        break;
                }
            }

            CheckForEndOfTableError(output);
            return output;
        }

        private void CheckForEndOfTableError( TableResult output)
        {
            if (!output.HasError)
            {
                var bytesLeftInStream = _tableData.Length - _dataIndex;
                if (bytesLeftInStream != 0)
                    output.Error = $"Error: Bytes left in stream after parsing : {bytesLeftInStream * 4}";
            }
        }

        public class TableResult
        {
            public string[][] DataRows { get; set; }
            public string[] ColumnNames { get; set; }
            public string Error { get; set; }
            public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
        }

        class RowResult
        {
            public string[] Content { get; set; }
            public string Error { get; set; }
            public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
        }

    }
}
