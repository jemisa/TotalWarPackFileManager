using Common;
using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{

    public class DataBaseFile
    {
        public string TableType { get; set; }
        public PackedFile DbFile { get; set; }
        public bool HasDefinitionError { get; set; }
        public string ErrorMessage { get; set; }
    }

    class DatabaseFileViewModel : NotifyPropertyChangedImpl
    { 
        public DataBaseFile DataBaseFile { get; set; }

        string _errorMessage;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasError
        {
            get { return !string.IsNullOrWhiteSpace(ErrorMessage); }
        }

        Color _backgroundColour;
        public Color BackgroundColour
        {
            get
            {
                return _backgroundColour;
            }
            set
            {
                _backgroundColour = value;
                NotifyPropertyChanged();
            }
        }
    }

    class FileListController : NotifyPropertyChangedImpl
    {
        string _searchFilter;
        public string SearchFilter
        {
            get
            {
                return _searchFilter;
            }
            set
            {
                _searchFilter = value;
                NotifyPropertyChanged();
            }
        }

        int _totalDbFiles;
        public int TotalDbFiles
        {
            get
            {
                return _totalDbFiles;
            }
            set
            {
                _totalDbFiles = value;
                NotifyPropertyChanged();
            }
        }


        int _dbFilesWithError;
        public int DbFilesWithError
        {
            get
            {
                return _dbFilesWithError;
            }
            set
            {
                _dbFilesWithError = value;
                NotifyPropertyChanged();
            }
        }

        bool _onlyShowTablesWithErrors;
        public bool OnlyShowTablesWithErrors
        {
            get
            {
                return _onlyShowTablesWithErrors;
            }
            set
            {
                _onlyShowTablesWithErrors = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand FileSelectedCommand { get; private set; }
        public ICommand FilterButtonCommand { get; private set; }
        public ICommand OnlyShowTablesWithError { get; private set; }

        public ObservableCollection<DatabaseFileViewModel> FileList { get; set; } = new ObservableCollection<DatabaseFileViewModel>();
        public event EventHandler<DataBaseFile> OnFileSelectedEvent;
        List<BatchEvaluator.Result> _errorParsingResult = null;

        // Internal
        List<DataBaseFile> _internalFileList = new List<DataBaseFile>();


        Thread _evaluateThradHandle;
        public FileListController()
        {
            DbFilesWithError = 123;
            TotalDbFiles = 1024;
            Load(@"C:\Program Files (x86)\Steam\steamapps\common\Total War WARHAMMER II");
            BatchEvaluator batchEvaluator = new BatchEvaluator(_internalFileList);
            batchEvaluator.OnCompleted += BatchEvaluator_OnCompleted;

            FileSelectedCommand = new RelayCommand<DatabaseFileViewModel>(OnFileSelected);
            FilterButtonCommand = new RelayCommand(OnFilter);
            OnlyShowTablesWithError = new RelayCommand(OnShowOnlyTablesWithErrors);

            // Evaluate in a new thread
            _evaluateThradHandle = new Thread(new ThreadStart(batchEvaluator.Evaluate));
            _evaluateThradHandle.Start();
        }

        private void BatchEvaluator_OnCompleted(object sender, List<BatchEvaluator.Result> e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _errorParsingResult = e;
                BuildFileList();
            });
        }

        private void OnFileSelected(DatabaseFileViewModel state)
        {
            OnFileSelectedEvent?.Invoke(this, state.DataBaseFile);
        }

        private void OnShowOnlyTablesWithErrors()
        {
            BuildFileList();
        }

        private void OnFilter()
        {
            BuildFileList();
        }

        void BuildFileList()
        {
            IEnumerable<DataBaseFile> items;

            if (OnlyShowTablesWithErrors)
                items = _internalFileList.Where(x => x.HasDefinitionError);
            else
                items = _internalFileList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchFilter))
                items = items.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.TableType, SearchFilter, CompareOptions.IgnoreCase) >= 0);

            FileList.Clear();
            foreach (var item in items)
            {
                var parseRessult = _errorParsingResult?.FirstOrDefault(x => x.TableType == item.TableType);
                DatabaseFileViewModel model = new DatabaseFileViewModel();
                if (parseRessult != null)
                {
                    //System.Windows.Media.SolidColorBrush a = new System.Windows.Media.SolidColorBrush();
                  //  a.Color = System.Windows.Media.Color.Red;

                    model.ErrorMessage = string.Join("\n", parseRessult.Errors);
                   // model.BackgroundColour =  = 
                }
                model.DataBaseFile = item;
                FileList.Add(model);
            }
        }

        public void Load(string gameDir)
        {
            PackLoadSequence allFiles = new PackLoadSequence
            {
                IncludePacksContaining = delegate (string s) { return true; }
            };

            List<string> packPaths = allFiles.GetPacksLoadedFrom(gameDir);
            packPaths.Reverse();

            PackFileCodec codec = new PackFileCodec();
            foreach (string path in packPaths)
            {
                PackFile pack = codec.Open(path);
                foreach (var f in pack.Files)
                {
                    var dbEntry = IsDb(f);
                    if (dbEntry.HasValue)
                    {
                        bool canDecode = PackedFileDbCodec.CanDecode(dbEntry.Value.Item2, out string errorMessage);
                        _internalFileList.Add(new DataBaseFile()
                        {
                            TableType = dbEntry.Value.Item1,
                            DbFile = dbEntry.Value.Item2,
                            //HasDefinitionError = !canDecode,
                            //ErrorMessage = errorMessage
                        });
                    }
                }
            }

            TotalDbFiles = _internalFileList.Count();
            DbFilesWithError = _internalFileList.Count(x => x.HasDefinitionError == true);
            BuildFileList();
        }

        (string, PackedFile)? IsDb(PackedFile file)
        {
            bool hasParent = file.Parent != null;
            if (hasParent)
            {
                bool hasParentParnet = file.Parent.Parent != null;
                if (hasParentParnet)
                {
                    if (file.Parent.Parent.Name == "db")
                    {
                        return (file.Parent.Name, file);
                    }
                }
            }
            return null;
        }
    }
}
