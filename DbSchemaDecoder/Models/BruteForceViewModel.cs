using DbSchemaDecoder.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Models
{
    class BruteForceViewModel : NotifyPropertyChangedImpl
    {
        public class ItemView
        {
            public List<FieldParserEnum> Enums { get; set; }
            public int Idx { get; set; }
            public string Value { get; set; }
            public int Columns { get; set; }
        }
        public ObservableCollection<ItemView> Values { get; set; } = new ObservableCollection<ItemView>();

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

        int _computeType = 1;
        public int ComputeType
        {
            get { return _computeType; }
            set
            {
                _computeType = value;
                NotifyPropertyChanged();
            }
        }


    }
}
