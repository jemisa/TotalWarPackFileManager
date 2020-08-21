using Common;
using DbSchemaDecoder.Controllers;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{
    public class BatchEvaluator
    {
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
        SchemaManager _schemaManager;
        GameTypeEnum _currentGame;
        public BatchEvaluator(IEnumerable<DataBaseFile> files, SchemaManager schemaManager, GameTypeEnum currentGame)
        {
            _files = files;
            _schemaManager = schemaManager;
            _currentGame = currentGame;
        }

        public void Evaluate()
        {
            List<Result> results = new List<Result>();
            foreach (var file in _files)
            {
                var result = EvaluateFile(file);
                results.Add(result);
            }

            OnCompleted?.Invoke(this, results);
        }

        Result EvaluateFile(DataBaseFile file)
        {
            Result result = new Result();
            result.TableType = file.TableType;

            try
            {
                var nemoryStream = new MemoryStream(file.DbFile.Data);
                var dbStreamReader = new BinaryReader(nemoryStream);

                DBFileHeader header = PackedFileDbCodec.readHeader(dbStreamReader);

                CaSchemaFileParser caSchemaFileParser = new CaSchemaFileParser();
                var caSchemaResult = caSchemaFileParser.Load(file.TableType);
                if (result.HasError)
                {
                    result.Errors.Add($"CA schama parsing failed: {caSchemaResult.Error}");
                }

                result.CaTableColumnCount = caSchemaResult.Entries.Count();

                var allTableDefinitions = _schemaManager.GetTableDefinitionsForTable(_currentGame, file.TableType);
                var fieldCollections = allTableDefinitions.Where(x => x.Version == header.Version);
                if (fieldCollections.Count() == 0)
                {
                    result.Errors.Add($"No definition for current version");
                }
                else if (fieldCollections.Count() != 1)
                {
                    result.Errors.Add($"More then one definition for current version");
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
                        result.Errors.Add($"Unable to parse table: {parseResult.Error}");
                    }
                }
            }
            catch (Exception e)
            {
                result.Errors.Add("Unknown error:" + e.Message);
            }

            return result;
        }
    }
}
