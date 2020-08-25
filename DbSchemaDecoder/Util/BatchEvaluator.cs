using Common;
using DbSchemaDecoder.Controllers;
using Filetypes;
using Filetypes.Codecs;
using Filetypes.DB;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbSchemaDecoder.Util
{
    public class BatchEvaluator
    {
        ILogger _logger = Logging.Create<BatchEvaluator>();

        public event EventHandler<List<Result>> OnCompleted;
        public class Result
        {
            public string TableType { get; set; }
            public List<string> Errors { get; private set; } = new List<string>();
            public List<string> Warnings { get; private set; } = new List<string>();
            public bool HasWarning { get { return Warnings.Count() != 0; } }
            public bool HasError { get { return Errors.Count() != 0; } }
            public int TabelColumnCount { get; set; }
            public int CaTableColumnCount { get; set; }
        }

        IEnumerable<DataBaseFile> _files;
        public BatchEvaluator(IEnumerable<DataBaseFile> files)
        {
            _files = files;
        }

        public void Evaluate()
        {
            _logger.Information("Starting evaluation");
            List<Result> results = new List<Result>();
            foreach (var file in _files)
            {
                var result = EvaluateFile(file);
                results.Add(result);
            }

            _logger.Information($"Starting completed {results.Count} errors");
            OnCompleted?.Invoke(this, results);
        }

        Result EvaluateFile(DataBaseFile file)
        {
            Result result = new Result();
            result.TableType = file.TableType;

            try
            {
                DBFileHeader header = PackedFileDbCodec.readHeader(file.DbFile);

                CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
                var caSchemaResult = caSchemaFileParser.Load(file.TableType);
                if (result.HasError)
                {
                    var error = $"CA schama parsing failed: {caSchemaResult.Error}";
                    _logger.Error(error);
                    result.Errors.Add(error);
                }

                result.CaTableColumnCount = caSchemaResult.Entries.Count();

                var allTableDefinitions = SchemaManager.Instance.GetTableDefinitionsForTable(file.TableType);
                var fieldCollections = allTableDefinitions.Where(x => x.Version == header.Version);
                if (fieldCollections.Count() == 0)
                {
                    var error = $"No definition for current version";
                    _logger.Error(error);
                    result.Errors.Add(error);
                }
                else if (fieldCollections.Count() != 1)
                {

                    var error = $"More then one definition for current version";
                    _logger.Error(error);
                    result.Errors.Add(error);
                }
                else
                {
                    var fieldCollection = fieldCollections.First();
                    result.TabelColumnCount = fieldCollection.ColumnDefinitions.Count;

                    // Read table
                    TableEntriesParser tableParser = new TableEntriesParser(file.DbFile.Data, header.Length);
                    var parseResult = tableParser.CanParseTable(
                        fieldCollection.ColumnDefinitions,
                        (int)header.EntryCount);

                    if (parseResult.HasError)
                    {
                        var error = $"Unable to parse table: {parseResult.Error}";
                        _logger.Error(error);
                        result.Errors.Add(error);
                    }
                }
            }
            catch (Exception e)
            {
                var error = "Unknown error:" + e.Message;
                _logger.Error(error);
                result.Errors.Add(error);
            }

            return result;
        }
    }
}
