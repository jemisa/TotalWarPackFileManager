using DbSchemaDecoder.Util;
using Filetypes;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DbSchemaDecoder.Controllers
{
    class BruteForceController : NotifyPropertyChangedImpl
    {
        public class ItemView
        {
            public List<FieldParserEnum> Enums { get; set; }
            public int Idx { get; set; }
            public string Value{get;set;}
        }

        public ObservableCollection<ItemView> Values { get; set; } = new ObservableCollection<ItemView>();


        public event EventHandler<List<FieldInfo>> OnNewDefinitionApplied;

        Thread _threadHandle;
        BruteForceParser _bruteForceparser;
        DateTime _startTime;
        private readonly System.Timers.Timer _timer;
        public ICommand ComputeBruteForceCommand { get; private set; }
        public ICommand SaveResultCommand { get; private set; }
        public ICommand OnClickCommand { get; private set; }
        public BruteForceController()
        {
            DispatcherHelper.Initialize();
            ComputeBruteForceCommand = new RelayCommand(OnCompute);
            SaveResultCommand = new RelayCommand(OnSave);
            OnClickCommand = new RelayCommand<ItemView>(OnClickFunc);

            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            RunningStatus = "Not run";
            CalculateButtonText = "Calculate";
        }

        void OnClickFunc(ItemView clickedItem)
        {
            var items = clickedItem.Enums.Select(x => FieldParser.CreateFromEnum(x).Instance()).ToList();
            for (int i = 0; i < items.Count(); i++)
                items[i].Name = "Unknown" + i;
            OnNewDefinitionApplied(this, items);
        }
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var runTimeSec = (e.SignalTime - _startTime).TotalSeconds;
            Update(runTimeSec);
        }

        public DataBaseFile SelectedFile { get; set; }
        public int TabelCount { set { ColumnCount = value; } }

        void OnCompute()
        {
            BruteForce(SelectedFile, ColumnCount);
        }


        void OnSave()
        {
            File.WriteAllLines(@"C:\temp\output.text", Values.Select(x => x.Value));
        }
        void BruteForce(DataBaseFile file, int count)
        {
            if (_threadHandle == null)
            {
                Values.Clear();
                _startTime = DateTime.Now;
                _timer.Start();
                RunTime = "";
                RunningStatus = "Running";
                TotalPossibleCombinations = "";
                EvaluatedCombinations = "";
                SkippedEarlyCombinations = "";
                ComputedPerSec = "";
                PossibleFirstRpws = "";
                CalculateButtonText = "Cancel";

                _bruteForceparser = new BruteForceParser(file, count);
                TotalPossibleCombinations = _bruteForceparser.PossibleCombinations.ToString("N0");


                _bruteForceparser.OnParsingCompleteEvent += ComputationDoneEventHandler;
                _bruteForceparser.OnCombintionFoundEvent += CombinationFoundEventHandler;

                _threadHandle = new Thread(new ThreadStart(_bruteForceparser.Compute));
                _threadHandle.Start();
            }
            else
            {
                Cancel();
            
            }
        }
        void CombinationFoundEventHandler(object sender, FieldParserEnum[] val)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var values = val.Select(x => FieldParser.CreateFromEnum(x).InstanceName());
                var v = string.Join(", ", values);

                Values.Add(new ItemView() 
                { 
                    Idx= Values.Count() + 1,
                    Value = v,
                    Enums = val.ToList()
                });

                PossibleCombinationsFound = Values.Count().ToString();
            });
        }

        void ComputationDoneEventHandler(object sender, BruteForceParser.StatusUpdate update)
        {
            RunningStatus = "Done";
            CalculateButtonText = "Calculate";
            var runTimeSec = (DateTime.Now - _startTime).TotalSeconds;
            _timer.Stop();

            Update(runTimeSec);
        }

        void Update(double runTimeSec)
        {
            RunTime = (int)runTimeSec + "s";

            EvaluatedCombinations =  _bruteForceparser.EvaluatedCombinations.ToString("N0");
            SkippedEarlyCombinations =  _bruteForceparser.SkippedEarlyCombinations.ToString("N0");

            var bigIntTime = new BigInteger(runTimeSec);
            if (bigIntTime == 0)
                bigIntTime = 1;
            ComputedPerSec = (_bruteForceparser.EvaluatedCombinations / bigIntTime).ToString("N0");
            PossibleFirstRpws =  _bruteForceparser.PossibleFirstRows.ToString("N0");

   
            BigFloat bigFloatEvaluatedCombinations = new BigFloat(_bruteForceparser.EvaluatedCombinations);
            BigFloat bigFloatTotal = new BigFloat(_bruteForceparser.PossibleCombinations);

   


            var ar = ((bigFloatEvaluatedCombinations / bigFloatTotal) * 100).ToString().Take(5).ToArray();
            string firstFivChar = new string(ar);
            EstimatedRunTime = firstFivChar + "%";
        }

        public void Cancel()
        {
            if (_threadHandle != null)
            {
                _threadHandle.Abort();
                _threadHandle = null;
                CalculateButtonText = "Calculate";
                RunningStatus = "Stopped";
                _timer.Stop();
            }
        }



        string _totalPossibleCombinations;
        public string TotalPossibleCombinations
        {
            get { return _totalPossibleCombinations; }
            set
            {
                _totalPossibleCombinations = value;
                NotifyPropertyChanged();
            }
        }

        string _possibleCombinationsFound;
        public string PossibleCombinationsFound
        {
            get { return _possibleCombinationsFound; }
            set
            {
                _possibleCombinationsFound = value;
                NotifyPropertyChanged();
            }
        }

        string _runningStatus;
        public string RunningStatus
        {
            get { return _runningStatus; }
            set
            {
                _runningStatus = value;
                NotifyPropertyChanged();
            }
        }

        string _runTime;
        public string RunTime
        {
            get { return _runTime; }
            set
            {
                _runTime = value;
                NotifyPropertyChanged();
            }
        }

        int _columnCount;
        public int ColumnCount
        {
            get { return _columnCount; }
            set
            {
                _columnCount = value;
                NotifyPropertyChanged();
            }
        }


        string _evaluatedCombinations;
        public string EvaluatedCombinations
        {
            get { return _evaluatedCombinations; }
            set
            {
                _evaluatedCombinations = value;
                NotifyPropertyChanged();
            }
        }

        string _skippedEarlyCombinations;
        public string SkippedEarlyCombinations
        {
            get { return _skippedEarlyCombinations; }
            set
            {
                _skippedEarlyCombinations = value;
                NotifyPropertyChanged();
            }
        }

        string _computedPerSec;
        public string ComputedPerSec
        {
            get { return _computedPerSec; }
            set
            {
                _computedPerSec = value;
                NotifyPropertyChanged();
            }
        }

        string _possibleFirstRpws;
        public string PossibleFirstRpws
        {
            get { return _possibleFirstRpws; }
            set
            {
                _possibleFirstRpws = value;
                NotifyPropertyChanged();
            }
        }

        string _estimatedRunTime;
        public string EstimatedRunTime
        {
            get { return _estimatedRunTime; }
            set
            {
                _estimatedRunTime = value;
                NotifyPropertyChanged();
            }
        }

        string _calculateButtonText;
        public string CalculateButtonText
        {
            get { return _calculateButtonText; }
            set
            {
                _calculateButtonText = value;
                NotifyPropertyChanged();
            }
        }

    }


}

/*
 
     
You may use a delegate to solve this issue. Here is an example that is showing how to update a textBox using diffrent thread

public delegate void UpdateTextCallback(string message);

private void TestThread()
{
    for (int i = 0; i <= 1000000000; i++)
    {
        Thread.Sleep(1000);                
        richTextBox1.Dispatcher.Invoke(
            new UpdateTextCallback(this.UpdateText),
            new object[] { i.ToString() }
        );
    }
}
private void UpdateText(string message)
{
    richTextBox1.AppendText(message + "\n");
}

private void button1_Click(object sender, RoutedEventArgs e)
{
   Thread test = new Thread(new ThreadStart(TestThread));
   test.Start();
}
     */
