using DbSchemaDecoder.Controllers;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Filetypes.ByteParsing;

namespace DbSchemaDecoder.Util
{

    class PreCalc
    {
        public List<Dictionary<DbTypesEnum, int>> _preComputeTable = new List<Dictionary<DbTypesEnum, int>>();

        AllCombinations _allCombinations = new AllCombinations();

        public void PreCompute(byte[] buffer, int dataIndex)
        {
            _preComputeTable = new List<Dictionary<DbTypesEnum, int>>(dataIndex);
            for (int i = 0; i < dataIndex; i++)
            {
                var possibleList = new Dictionary<DbTypesEnum, int>();
                _preComputeTable.Add(possibleList);
            }

            var allCombinations = _allCombinations.GetPossibleCombinations(0);
            for (int i = dataIndex; i < buffer.Length; i++)
            {
                var possibleList = new Dictionary<DbTypesEnum, int>();
                foreach (var parserEnum in allCombinations)
                {
                    var parser = FieldParser.CreateFromEnum(parserEnum);
                    var result =parser.CanParse(buffer, i);
                    if (result.Completed == true)
                    {
                        possibleList.Add(parserEnum, result.OffsetAfter);
                    }
                }

                _preComputeTable.Add(possibleList);
            }
        }
    }

    class BruteForceParser : IThreadableTask
    {
        public event EventHandler<DbTypesEnum[]> OnCombintionFoundEvent;
        public event EventHandler OnThreadCompleted;

        public BigInteger EvaluatedCombinations { get { return _permutationHelper != null ? _permutationHelper.ComputedPermutations : 0; } }
        public BigInteger SkippedEarlyCombinations { get { return _permutationHelper != null? _permutationHelper.SkippedEarlyPermutations : 0; } }
        public BigInteger PossibleFirstRows { get { return _permutationHelper != null? _permutationHelper.PossibleFirstRows : 0; } }

        static DbTypesEnum[] GetPossibleFields()
        {
            return new DbTypesEnum[]
            {
                DbTypesEnum.Optstring_ascii,
                DbTypesEnum.String_ascii,
                DbTypesEnum.Optstring,
                DbTypesEnum.String,
                DbTypesEnum.Integer,
                DbTypesEnum.Boolean,
            };
        }

        DataBaseFile _file;
        int _maxNumberOfFields;

        int _headerLength;
        int _HeaderEntryCount;

        byte[] _tableData;

        public static BigInteger PossibleCombinations(int numFields)
        {
            return BigInteger.Pow(GetPossibleFields().Count(), numFields);
        }

        public List<DbTypesEnum[]> PossiblePermutations { get; set; } = new List<DbTypesEnum[]>();
        PermutationHelper _permutationHelper;
        IBruteForceCombinationProvider _combinationProvider;


        PreCalc _preCalc;

        public BruteForceParser(DataBaseFile dataBaseFile, IBruteForceCombinationProvider combinationProvider, int maxNumberOfFields)
        {
            _file = dataBaseFile;
            _maxNumberOfFields = maxNumberOfFields;
            _combinationProvider = combinationProvider;

            _tableData = dataBaseFile.DbFile.Data;
            DBFileHeader header = PackedFileDbCodec.readHeader(dataBaseFile.DbFile);

            _headerLength = header.Length;
            _HeaderEntryCount = (int)header.EntryCount;
        }
        public void Compute()
        {
            _preCalc = new PreCalc();
            _preCalc.PreCompute(_tableData, _headerLength);

            var max = new AllCombinations().GetPossibleCombinations(0).Length;
            _permutationHelper = new PermutationHelper(OnEvaluatePermutation, _maxNumberOfFields, max);
            _permutationHelper.ComputePermutations(
                _preCalc,
                _tableData,
                new DbTypesEnum[_maxNumberOfFields],
                _combinationProvider,
                0,
                _headerLength,
                _maxNumberOfFields);


            OnThreadCompleted?.Invoke(this, null);
        }

        void OnEvaluatePermutation(DbTypesEnum[] combination)
        { 
            TableEntriesParser p = new TableEntriesParser(_tableData, _headerLength);
            var updateRes = p.CanParseTableFaster(
                _preCalc,
                combination,
                _HeaderEntryCount);

            if (updateRes)
            {
                UpdatePossiblePermutation(combination.Select(x => x).ToArray());
            }
        }
        void UpdatePossiblePermutation(DbTypesEnum[] combination)
        {
            PossiblePermutations.Add(combination);
            OnCombintionFoundEvent?.Invoke(this, combination);
        }

    }

    class PermutationHelper
    {
        public delegate void OnCombintionFoundDelegate(DbTypesEnum[] combination);
        OnCombintionFoundDelegate _evaluateCallback;
        public BigInteger ComputedPermutations { get; set; } = 0;
        public BigInteger SkippedEarlyPermutations { get; set; } = 0;
        public BigInteger PossibleFirstRows { get; set; } = 0;

        List<BigInteger> _powTable = new List<BigInteger>();
        public PermutationHelper(OnCombintionFoundDelegate callback, int maxNumberOfFields, int nPossibleItems)
        {
            _evaluateCallback = callback;
            for (int i = 0; i < maxNumberOfFields+1; i++)
            {
                var realCount = maxNumberOfFields - i;
                _powTable.Add(BigInteger.Pow(nPossibleItems, realCount));
            }
        }

        public void ComputePermutations(PreCalc precalc, byte[] buffer, DbTypesEnum[] n, IBruteForceCombinationProvider combinationProvider, int idx, int bufferIndex, int maxNumberOfFields)
        {
            if (idx == n.Length)
            {
                _evaluateCallback(n);
                ComputedPermutations++;
                PossibleFirstRows++;
                return;
            }
            var states = combinationProvider.GetPossibleCombinations(idx);
            for (int i = 0; i < states.Length; i++)
            {
                var currentState = states[i];
                var possible = precalc._preComputeTable[bufferIndex];
                if (possible.ContainsKey(currentState))
                {
                    var offset = possible[currentState];
                    n[idx] = currentState;
                    ComputePermutations(precalc, buffer, n, combinationProvider, idx + 1, offset, maxNumberOfFields);
                }
                else
                {
                    var realCount = n.Count() - idx -1;
                    var skippedCount = _powTable[idx+1];


                    SkippedEarlyPermutations += skippedCount;
                    ComputedPermutations += skippedCount;
                }
            }
        }
    }

    interface IBruteForceCombinationProvider
    {
        DbTypesEnum[] GetPossibleCombinations(int index);
    }
    class AllCombinations : IBruteForceCombinationProvider
    {
        DbTypesEnum[] _possibleCombinations;
        public AllCombinations()
        {
            _possibleCombinations = new DbTypesEnum[]
            {
                 DbTypesEnum.Boolean,
                DbTypesEnum.String_ascii,
                DbTypesEnum.String,
                DbTypesEnum.Integer,
                
                DbTypesEnum.Optstring_ascii,
                DbTypesEnum.Optstring,
               
            };
        }
        public DbTypesEnum[] GetPossibleCombinations(int index)
        {
            return _possibleCombinations;
        }
    }

    class CaTableCombinations : IBruteForceCombinationProvider
    {
        DbTypesEnum[][] _possibleCombinations;
        public CaTableCombinations(IEnumerable<CaSchemaEntry> caSchemaEntries)
        {
            var schemasAsList = caSchemaEntries.ToList();
            _possibleCombinations = new DbTypesEnum[schemasAsList.Count][];
            
            for(int i = 0; i < schemasAsList.Count; i++)
            {
                var possibleCombinations = GetCombinationsForType(schemasAsList[i]);
                _possibleCombinations[i] = possibleCombinations;
            }
        }

        DbTypesEnum[] GetCombinationsForType(CaSchemaEntry entry)
        {
            switch (entry.field_type)
            {
                case "yesno":
                    return new DbTypesEnum[] { DbTypesEnum.Boolean };
                case "single":
                case "decimal":
                case "double":
                    return new DbTypesEnum[] { DbTypesEnum.Single };
                case "autonumber":
                case "integer":
                    return new DbTypesEnum[] { DbTypesEnum.Integer};
                case "text":
                    return new DbTypesEnum[] 
                    {
                        DbTypesEnum.Optstring_ascii, DbTypesEnum.String_ascii,
                        DbTypesEnum.Optstring, DbTypesEnum.String 
                    };
            }

            // case "autonumber": -> // Should be long, but long is not supported?
            throw new Exception($"Field '{entry.name}' contains unkown type '{entry.field_type}'");
        }

        public DbTypesEnum[] GetPossibleCombinations(int index)
        {
            return _possibleCombinations[index];
        }
    }

    class AppendTableCombinations : IBruteForceCombinationProvider
    {
        DbTypesEnum[][] _existingFields;
        AllCombinations _allCombinations = new AllCombinations();

        public AppendTableCombinations(DbTypesEnum[] existingFields)
        {
            _existingFields = new DbTypesEnum[existingFields.Count()][];
            for(int i = 0; i < existingFields.Count(); i++)
                _existingFields[i] = GetCombinationsForType(existingFields[i]);
        }

        DbTypesEnum[] GetCombinationsForType(DbTypesEnum entry)
        {
            return new DbTypesEnum[] { entry };
        }

        public DbTypesEnum[] GetPossibleCombinations(int index)
        {
            if (_existingFields.Length > index)
                return _existingFields[index];
            else
                return _allCombinations.GetPossibleCombinations(index);
        }
    }
}
