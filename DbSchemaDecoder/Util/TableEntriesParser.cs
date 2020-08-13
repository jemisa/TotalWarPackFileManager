using Filetypes;
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
        /// This function expects that the header of the file is already read!
        public TableResult ParseTable(BinaryReader reader, int streamCapacity, List<FieldInfo> fields, int expectedEntries)
        {
            TableResult output = new TableResult();
            if (fields.Count() != 0)
            {
                output.ColumnNames = new string[fields.Count];
                for (int i = 0; i < fields.Count; i++)
                    output.ColumnNames[i] = fields[i].Name;

                var fieldInstances = fields.Select(x => x.CreateInstance()).ToArray();

                output.DataRows = new string[expectedEntries][];
                for (int i = 0; i < expectedEntries; i++)
                {
                    var rowResult = ParseRow(reader, fieldInstances, i);
                    output.DataRows[i] = rowResult.Content;
                    if (rowResult.HasError)
                    {
                        output.Error = rowResult.Error;
                    }
                }
            }

            CheckForEndOfTableError(reader, streamCapacity, output);
            return output;
        }

        RowResult ParseRow(BinaryReader reader, FieldInstance[] fields, int rowIndex)
        {
            int fieldsCount = fields.Count();
            RowResult result = new RowResult()
            {
                Content = new string[fieldsCount],
            };

            for (int i = 0; i < fieldsCount; i++)
            {
                var parseResult = fields[i].TryDecode(reader);
                result.Content[i] = fields[i].Value;
                if (!parseResult)
                {
                    result.Error = $"Error parsing column {fields[i].Name} for row {rowIndex + 1} : {fields[i].Value}";
                    break;
                }
            }
            return result;
        }

        // A much faster version that just checks if the table can be parsed using the current field configuration
        public TableResult CanParseTable(BinaryReader reader, int streamCapacity, List<FieldInfo> fields, int expectedEntries)
        {
            TableResult output = new TableResult();
            if (fields.Count() != 0)
            {
                var fieldInstances = fields.Select(x => x.CreateInstance()).ToArray();
                for (int rowIndex = 0; rowIndex < expectedEntries; rowIndex++)
                {
                    foreach (var field in fieldInstances)
                    {
                        //string errorMessage = "";
                        var parseResult = field.TryDecode(reader);
                        if (!parseResult)
                        {
                            output.Error = $"Error parsing column {field.Name} for row {rowIndex + 1} : {field.Value}";
                            break;
                        }
                    }
                    if (output.HasError)
                        break;
                }
            }

            CheckForEndOfTableError(reader, streamCapacity, output);
            return output;
        }

        private static void CheckForEndOfTableError(BinaryReader reader, int streamCapacity, TableResult output)
        {
            if (!output.HasError)
            {
                var bytesLeftInStream = streamCapacity - (int)reader.BaseStream.Position;
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
