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
    class BruteForceParser : IThreadableTask
    {
        public event EventHandler<DbTypesEnum[]> OnCombintionFoundEvent;
        public event EventHandler OnThreadCompleted;

        public BigInteger EvaluatedCombinations { get { return _permutationHelper.ComputedPermutations; } }
        public BigInteger SkippedEarlyCombinations { get { return _permutationHelper.SkippedEarlyPermutations; } }
        public BigInteger PossibleFirstRows { get { return _permutationHelper.PossibleFirstRows; } }

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

        List<DbColumnDefinition> _dbColumnDefinitions;
        public List<DbTypesEnum[]> PossiblePermutations { get; set; } = new List<DbTypesEnum[]>();
        PermutationHelper _permutationHelper;
        IBruteForceCombinationProvider _combinationProvider;
        public BruteForceParser(DataBaseFile dataBaseFile, IBruteForceCombinationProvider combinationProvider, int maxNumberOfFields)
        {
            _file = dataBaseFile;
            _maxNumberOfFields = maxNumberOfFields;
            _combinationProvider = combinationProvider;

            _tableData = dataBaseFile.DbFile.Data;
            DBFileHeader header = PackedFileDbCodec.readHeader(dataBaseFile.DbFile);
            _headerLength = header.Length;
            _HeaderEntryCount = (int)header.EntryCount;

            _dbColumnDefinitions = new List<DbColumnDefinition>();
            for (int i = 0; i < maxNumberOfFields; i++)
            {
                _dbColumnDefinitions.Add(new DbColumnDefinition()
                {
                    Name = "Unkown" + i
                });
            }
        }
        public void Compute()
        {
            _permutationHelper = new PermutationHelper(OnEvaluatePermutation);
            _permutationHelper.ComputePermutations(
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
            for(int i = 0; i < combination.Count(); i++)
                _dbColumnDefinitions[i].Type = combination[i];

            TableEntriesParser p = new TableEntriesParser(_tableData, _headerLength);
            var updateRes = p.CanParseTable(
                _dbColumnDefinitions,
                _HeaderEntryCount);

            if (!updateRes.HasError)
            {
                UpdatePossiblePermutation(combination);
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
        public PermutationHelper(OnCombintionFoundDelegate callback)
        {
            _evaluateCallback = callback;
        }

        public void ComputePermutations(byte[] buffer, DbTypesEnum[] n, IBruteForceCombinationProvider combinationProvider, int idx, int bufferIndex, int maxNumberOfFields)
        {
            if (idx == n.Length)
            {
                var newItem = n.Select(x => x).ToArray();
                _evaluateCallback(newItem);
                ComputedPermutations++;
                PossibleFirstRows++;
                return;
            }
            var states = combinationProvider.GetPossibleCombinations(idx);
            for (int i = 0; i < states.Length; i++)
            {
                var currentState = states[i];

                var parser = FieldParser.CreateFromEnum(currentState);
                var parseResult = parser.CanParse(buffer, bufferIndex);
                if (parseResult.Completed)
                {
                    n[idx] = currentState;
                    ComputePermutations(buffer, n, combinationProvider, idx + 1, parseResult.OffsetAfter, maxNumberOfFields);
                }
                else
                {
                    var realCount = n.Count() - idx - 1;
                    var skippedCount = BigInteger.Pow(states.Length, realCount);

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
