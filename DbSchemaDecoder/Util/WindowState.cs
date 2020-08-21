using Common;
using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Models;
using Filetypes;
using Filetypes.ByteParsing;
using Filetypes.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DbSchemaDecoder.Util.BatchEvaluator;

namespace DbSchemaDecoder.Util
{
    public class WindowState
    {
        // File handling
        DataBaseFile _selectedFile;
        public DataBaseFile SelectedFile{ get { return _selectedFile; } set { _selectedFile = value; OnFileSelected?.Invoke(null, _selectedFile); } }
        public event EventHandler<DataBaseFile> OnFileSelected;

        // Ca schema
        List<CaSchemaEntry> _caSchema = new List<CaSchemaEntry>();
        public List<CaSchemaEntry> CaSchema { get { return _caSchema; } set { _caSchema = value; OnCaSchemaLoaded?.Invoke(null, _caSchema); } }
        public event EventHandler<List<CaSchemaEntry>> OnCaSchemaLoaded;


        // DataBase schema
        List<DbColumnDefinition> _DbSchema;
        public List<DbColumnDefinition> DbSchemaFields { get { return _DbSchema; } set { _DbSchema = value; OnSetDbSchema?.Invoke(null, _DbSchema); } }
        public event EventHandler<List<DbColumnDefinition>> OnSetDbSchema;


        FieldInfoViewModel _selectedDbSchemaRow;
        public FieldInfoViewModel SelectedDbSchemaRow { get { return _selectedDbSchemaRow; } set { _selectedDbSchemaRow = value; OnSelectedDbSchemaRowChanged?.Invoke(null, _selectedDbSchemaRow); } }
        public event EventHandler<FieldInfoViewModel> OnSelectedDbSchemaRowChanged;

        public void TriggerNewDbSchemaRowCreated(object sender, DbTypesEnum newRowType){ OnNewDbSchemaRowCreated?.Invoke(sender, newRowType); }
        public event EventHandler<DbTypesEnum> OnNewDbSchemaRowCreated;


        // Error handling
        List<Result> _parsingErrors;
        public List<Result> FileParsingErrors { get { return _parsingErrors; } set { _parsingErrors = value; OnErrorParsingCompleted?.Invoke(null, _parsingErrors); } }
        public event EventHandler<List<Result>> OnErrorParsingCompleted;

        // Game
        public Game CurrentGame { get; set; }
        public SchemaManager SchemaManager { get; set; }
    }
}
