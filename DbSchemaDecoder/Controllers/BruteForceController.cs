using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using static DbSchemaDecoder.Models.BruteForceViewModel;

namespace DbSchemaDecoder.Controllers
{
   /* public class BruteForceController : NotifyPropertyChangedImpl
    {
        public enum BruteForceCalculatorType
        { 
            BruteForceUsingCaSchama = 0,
            BruteForce,
            BruteForceUsingExistingTables,
            BruteForceUnknownTableCount,
            BruteForceUsingExistingTableUnknownTableCount
        };

        public BruteForceViewModel ViewModel { get; set; } = new BruteForceViewModel();
        Util.WindowState _windowState;
        TimedThreadProcess<BruteForceParser>[] _timedProcess;

        public ICommand ComputeBruteForceCommand { get; private set; }
        public ICommand SaveResultCommand { get; private set; }
        public ICommand OnClickCommand { get; private set; }

        public BruteForceController(Util.WindowState windowState)
        {
            _windowState = windowState;
            _windowState.OnCaSchemaLoaded += (sender, caSchema) => { ViewModel.ColumnCount = caSchema.Count(); };

            _windowState.OnFileSelected += (sender, file) => 
            { 
                Cancel(); 
                ViewModel.Values.Clear(); 
            };

            ComputeBruteForceCommand = new RelayCommand(OnCompute);
            SaveResultCommand = new RelayCommand(OnSave);
            OnClickCommand = new RelayCommand<ItemView>(OnItemDoubleClicked);

            ViewModel.RunningStatus = "Not run";
            ViewModel.CalculateButtonText = "Calculate";
            UpdateBruteForceDisplay();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ComputeType))
                UpdateBruteForceDisplay();
        }

        void UpdateBruteForceDisplay()
        {
            if(_windowState.CaSchema != null)
                ViewModel.ColumnCount = _windowState.CaSchema.Count();

            var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
            if (bruteForceMethod == BruteForceCalculatorType.BruteForce)
            {
                ViewModel.BruteForceColumnCountText = "No. Table Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "Max Columns:";
                ViewModel.ColumnCountEditable = true;
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                return;
            }
            else if(bruteForceMethod == BruteForceCalculatorType.BruteForceUsingCaSchama)
            {
                ViewModel.BruteForceColumnCountText = "No. Ca Columns:";
                ViewModel.ColumnCountEditable = false;
                return;
            }
            else if(bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTables)
            {
                ViewModel.BruteForceColumnCountText = "No.Total Table Columns:";
                if(_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }
            else if (bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount)
            {
                ViewModel.BruteForceColumnCountText = "No.Total Max Table Columns:";
                if (_windowState.DbSchemaFields != null)
                    ViewModel.ColumnCount = _windowState.DbSchemaFields.Count();
                ViewModel.ColumnCountEditable = true;
                return;
            }

            throw new NotImplementedException("Unknown compute type");
        }

        void OnItemDoubleClicked(ItemView clickedItem)
        {
            var items = clickedItem.Enums.Select(x => FieldParser.CreateFromEnum(x).Instance()).ToList();
            for (int i = 0; i < items.Count(); i++)
                items[i].Name = "Unknown" + i;

            _windowState.DbSchemaFields = items;
        }

        void OnCompute()
        {
            if (Cancel())
                return;


            var bruteForceMethod = (BruteForceCalculatorType)ViewModel.ComputeType;
            IBruteForceCombinationProvider combinationProvider = GetCombinationProvider(bruteForceMethod);
            if (combinationProvider == null)
                return;

            ViewModel.Values.Clear();
            ViewModel.PossibleCombinationsFound = "0";
            ViewModel.RunTime = "";
            ViewModel.RunningStatus = "Running";
            ViewModel.TotalPossibleCombinations = "";
            ViewModel.EvaluatedCombinations = "";
            ViewModel.SkippedEarlyCombinations = "";
            ViewModel.ComputedPerSec = "";
            ViewModel.PossibleFirstRows = "";
            ViewModel.CalculateButtonText = "Cancel";

            _viewHolder = new Dictionary<BruteForceParser, TempDisplayValues>();
     
            if (bruteForceMethod == BruteForceCalculatorType.BruteForceUnknownTableCount ||
                bruteForceMethod == BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount)
            {
                _possibleCombinations = 0;
                _timedProcess = new TimedThreadProcess<BruteForceParser>[ViewModel.ColumnCount];
                for (int i = 1; i < ViewModel.ColumnCount+1; i++)
                    _possibleCombinations += BruteForceParser.PossibleCombinations(i);

                for (int i = 1; i < ViewModel.ColumnCount + 1; i++)
                    BruteForce(_windowState.SelectedFile, i, combinationProvider, i - 1);

                ViewModel.ColumnVariationsCompleted = "0/" + ViewModel.ColumnCount;
                ViewModel.TotalPossibleCombinations = _possibleCombinations.ToString("N0");
            }
            else
            {
                _possibleCombinations = BruteForceParser.PossibleCombinations(ViewModel.ColumnCount);
                _timedProcess = new TimedThreadProcess<BruteForceParser>[1];
                BruteForce(_windowState.SelectedFile, ViewModel.ColumnCount, combinationProvider, 0);

                ViewModel.ColumnVariationsCompleted = "0/1";
                ViewModel.TotalPossibleCombinations = _possibleCombinations.ToString("N0"); ;
            }
        }

        void OnSave()
        {
            File.WriteAllLines(@"C:\temp\output.text", ViewModel.Values.Select(x => x.Value));
        }

        void BruteForce(DataBaseFile file, int maxNumberOfFields, IBruteForceCombinationProvider combinationProvider, int threadIndex)
        {
            var bruteForceparser = new BruteForceParser(file, combinationProvider, maxNumberOfFields);
            bruteForceparser.OnCombintionFoundEvent += CombinationFoundEventHandler;
            _viewHolder.Add(bruteForceparser, new TempDisplayValues());
            _timedProcess[threadIndex] = new TimedThreadProcess<BruteForceParser>(bruteForceparser);
            _timedProcess[threadIndex].OnThreadCompletedEvent += BruteForceController_OnThreadCompletedEvent;
            _timedProcess[threadIndex].OnUpdate += BruteForceController_OnUpdate;
            _timedProcess[threadIndex].Start(bruteForceparser.Compute);

        }

        private void BruteForceController_OnUpdate(object sender, TimedThreadEvent<BruteForceParser> arg)
        {
            Update(arg);
        }
        private void BruteForceController_OnThreadCompletedEvent(object sender, TimedThreadEvent<BruteForceParser> arg)
        {
            var doneProcesses = _timedProcess
                .Where(x =>x != null)
                .Count(x => x.IsRunning == false);
            if (doneProcesses == _timedProcess.Length)
            {
                ViewModel.RunningStatus = "Done";
                ViewModel.CalculateButtonText = "Calculate";
            }
            ViewModel.ColumnVariationsCompleted = $"{doneProcesses}/{_timedProcess.Length}";
            Update(arg);
        }

        IBruteForceCombinationProvider GetCombinationProvider(BruteForceCalculatorType type)
        {
            try
            {
                switch (type)
                {
                    case BruteForceCalculatorType.BruteForce:
                    case BruteForceCalculatorType.BruteForceUnknownTableCount:
                        return new AllCombinations();

                    case BruteForceCalculatorType.BruteForceUsingCaSchama:
                        return new CaTableCombinations(_windowState.CaSchema);

                    case BruteForceCalculatorType.BruteForceUsingExistingTables:
                    case BruteForceCalculatorType.BruteForceUsingExistingTableUnknownTableCount:
                        return new AppendTableCombinations(_windowState.DbSchemaFields.Select(x => x.TypeEnum).ToArray());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

            throw new NotImplementedException("Unknown compute type");
        }

        void CombinationFoundEventHandler(object sender, FieldParserEnum[] val)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var values = val.Select(x => FieldParser.CreateFromEnum(x).InstanceName());
                var v = string.Join(", ", values);

                ViewModel.Values.Add(new ItemView() 
                { 
                    Idx= ViewModel.Values.Count() + 1,
                    Value = v,
                    Enums = val.ToList(),
                    Columns = val.Count(),
                });

                ViewModel.PossibleCombinationsFound = ViewModel.Values.Count().ToString();
            });
        }

        class TempDisplayValues
        {
            public BigInteger EvaluatedCombinations { get; set; } = 0;
            public BigInteger SkippedEarlyCombinations { get; set; } = 0;
            public BigInteger ComputedPerSec { get; set; } = 0;
            public BigInteger PossibleFirstRows { get; set; } = 0;
        }

        Dictionary<BruteForceParser, TempDisplayValues> _viewHolder = new Dictionary<BruteForceParser, TempDisplayValues>();
        BigInteger _possibleCombinations = 0;
        void Update(TimedThreadEvent<BruteForceParser> arg)
        {
            var parser = arg.TaskHandler;
            var current = _viewHolder[parser];
            current.EvaluatedCombinations = parser.EvaluatedCombinations;
            current.SkippedEarlyCombinations = parser.SkippedEarlyCombinations;
            current.PossibleFirstRows = parser.PossibleFirstRows;

            BigInteger EvaluatedCombinations = 0;
            BigInteger SkippedEarlyCombinations = 0;
            BigInteger PossibleFirstRows = 0;
            foreach (var item in _viewHolder.Values)
            {
                EvaluatedCombinations += item.EvaluatedCombinations;
                SkippedEarlyCombinations += item.SkippedEarlyCombinations;
                PossibleFirstRows += item.PossibleFirstRows;
            }

            ViewModel.RunTime = (int)arg.Process.RunTimeInSec() + "s";

            ViewModel.EvaluatedCombinations = EvaluatedCombinations.ToString("N0");
            ViewModel.SkippedEarlyCombinations = SkippedEarlyCombinations.ToString("N0");
            ViewModel.PossibleFirstRows = PossibleFirstRows.ToString("N0");
   
            BigFloat bigFloatEvaluatedCombinations = new BigFloat(EvaluatedCombinations);
            BigFloat bigFloatTotal = new BigFloat(_possibleCombinations);

            var percentageDone = ((bigFloatEvaluatedCombinations / bigFloatTotal) * 100).ToString().Take(10);
            ViewModel.EstimatedRunTime = new string(percentageDone.ToArray()) + "%";
        }

        public bool Cancel()
        {
            if (_timedProcess == null)
                return false;

            bool foundRunninProcess = false;
            for(int i = 0; i < _timedProcess.Length; i++)
            {
                if (_timedProcess[i].IsRunning)
                {
                    foundRunninProcess = true;
                    _timedProcess[i].Stop(true);
                }
            }
            ViewModel.CalculateButtonText = "Calculate";
            ViewModel.RunningStatus = "Stopped";
            return foundRunninProcess;
        }
    }*/
}