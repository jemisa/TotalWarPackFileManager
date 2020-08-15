﻿using Common;
using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;

namespace DbSchemaDecoder.Controllers
{

    public class DataBaseFile
    {
        public string TableType { get; set; }
        public PackedFile DbFile { get; set; }
    }

    

    class FileListController : NotifyPropertyChangedImpl
    {
        // Ui related variables
        public FileListViewModel ViewModel { get; set; } = new FileListViewModel();
        public ICommand FileSelectedCommand { get; private set; }
        public ICommand FilterButtonCommand { get; private set; }
        public ICommand OnlyShowTablesWithErrorCommand { get; private set; }

        // Internal variables
        List<BatchEvaluator.Result> _errorParsingResult = null;
        List<DataBaseFile> _internalFileList = new List<DataBaseFile>();
        EventHub _eventHub;
        Thread _evaluateThradHandle;

        public FileListController(EventHub eventHub)
        {
            _eventHub = eventHub;
            Load(@"C:\Program Files (x86)\Steam\steamapps\common\Total War WARHAMMER II");
            BatchEvaluator batchEvaluator = new BatchEvaluator(_internalFileList);
            batchEvaluator.OnCompleted += BatchEvaluator_OnCompleted;

            FileSelectedCommand = new RelayCommand<DatabaseFileViewModel>(OnFileSelected);
            FilterButtonCommand = new RelayCommand(OnFilter);
            OnlyShowTablesWithErrorCommand = new RelayCommand(OnShowOnlyTablesWithErrors);

            // Evaluate db files in a new thread to await waiting
            _evaluateThradHandle = new Thread(new ThreadStart(batchEvaluator.Evaluate));
            _evaluateThradHandle.Start();
        }

        private void BatchEvaluator_OnCompleted(object sender, List<BatchEvaluator.Result> e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                _errorParsingResult = e;
                ViewModel.DbFilesWithError = _errorParsingResult.Count(x => x.HasError == true);
                _eventHub.TriggerErrorParsingCompleted(this, _errorParsingResult);
                BuildFileList();
            });
        }

        private void OnFileSelected(DatabaseFileViewModel state)
        {
            if(state != null)
                _eventHub.TriggerOnFileSelected(null, state.DataBaseFile);
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

            if (ViewModel.OnlyShowTablesWithErrors && _errorParsingResult != null)
                items = _internalFileList.Where(x => _errorParsingResult.First(e => e.TableType == x.TableType).HasError);
            else
                items = _internalFileList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ViewModel.SearchFilter))
                items = items.Where(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.TableType, ViewModel.SearchFilter, CompareOptions.IgnoreCase) >= 0);

            ViewModel.FileList.Clear();
            foreach (var item in items)
            {
                var parseRessult = _errorParsingResult?.FirstOrDefault(x => x.TableType == item.TableType);
                DatabaseFileViewModel model = new DatabaseFileViewModel();
                if (parseRessult != null && parseRessult.HasError)
                {
                    model.Color = Colors.Red;
                }
                model.DataBaseFile = item;  
                ViewModel.FileList.Add(model);
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

            ViewModel.TotalDbFiles = _internalFileList.Count();

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
