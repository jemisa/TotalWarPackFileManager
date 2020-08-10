using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.Util;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DbSchemaDecoder.Util
{
    class BruteForceParser
    {
        public event EventHandler OnParsingCompleteEvent;
        public event EventHandler<FieldParserEnum[]> OnCombintionFoundEvent;
        public BigInteger EvaluatedCombinations { get { return _permutationHelper.ComputedPermutations; } }
        public BigInteger SkippedEarlyCombinations { get { return _permutationHelper.SkippedEarlyPermutations; } }
        public BigInteger PossibleFirstRows { get { return _permutationHelper.PossibleFirstRows; } }
        
        FieldParserEnum[] GetPossibleFields()
        {
            return new FieldParserEnum[]
            {
                FieldParserEnum.OptStringTypeAscii,
                FieldParserEnum.StringTypeAscii,
                FieldParserEnum.OptStringType,
                FieldParserEnum.StringType,
                FieldParserEnum.IntType,
                FieldParserEnum.BoolType,
            };
        }

        DataBaseFile _file;
        int _maxNumberOfFields;

        MemoryStream _permutationStream;
        BinaryReader _permutationReader;
        int _headerLength;
        int _HeaderEntryCount;


        public BigInteger PossibleCombinations { get { return BigInteger.Pow(GetPossibleFields().Count(), _maxNumberOfFields); } }
        public List<FieldParserEnum[]> PossiblePermutations { get; set; } = new List<FieldParserEnum[]>();
        PermutationHelper _permutationHelper;
        public BruteForceParser(DataBaseFile dataBaseFile, int maxNumberOfFields)
        {
            _file = dataBaseFile;
            _maxNumberOfFields = maxNumberOfFields;

            _permutationStream = new MemoryStream(_file.DbFile.Data);
            _permutationReader = new BinaryReader(_permutationStream);

            DBFileHeader header = PackedFileDbCodec.readHeader(_permutationReader);
            _headerLength = header.Length;
            _HeaderEntryCount = (int)header.EntryCount;
        }



        public void Compute()
        {
            _permutationHelper = new PermutationHelper(OnEvaluatePermutation);
            _permutationHelper.ComputePermutations(
                _permutationReader, 
                new FieldParserEnum[_maxNumberOfFields], 
                GetPossibleFields(), 
                0,
                _headerLength,
                _maxNumberOfFields);

            _permutationReader.Dispose();
            _permutationStream.Dispose();

            
            OnParsingCompleteEvent?.Invoke(this, null);
        }

        void OnEvaluatePermutation(FieldParserEnum[] combination)
        {
            var possibleSchema = combination.Select(x => FieldParser.CreateFromEnum(x).Instance()).ToList();
            _permutationReader.BaseStream.Position = _headerLength;

            TableEntriesParser p = new TableEntriesParser();
            var updateRes = p.CanParseTable(
                _permutationReader,
                _permutationStream.Capacity,
                possibleSchema,
                _HeaderEntryCount);

            if (!updateRes.HasError)
            {
                UpdatePossiblePermutation(combination);
            }
        }
        void UpdatePossiblePermutation(FieldParserEnum[] combination)
        {
            PossiblePermutations.Add(combination);
            OnCombintionFoundEvent?.Invoke(this, combination);
        }


        class PermutationHelper
        {
            public delegate void OnCombintionFoundDelegate(FieldParserEnum[] combination);
            OnCombintionFoundDelegate _evaluateCallback;

            public BigInteger ComputedPermutations { get; set; } = 0;
            public BigInteger SkippedEarlyPermutations { get; set; } = 0;
            public BigInteger PossibleFirstRows { get; set; } = 0;
            public PermutationHelper(OnCombintionFoundDelegate callback)
            {
                _evaluateCallback = callback;
            }


            public void ComputePermutations(BinaryReader reader, FieldParserEnum[] n, FieldParserEnum[] states, int idx, long streamOffset, int maxNumberOfFields)
            {
                if (idx == n.Length)
                {
                    var newItem = n.Select(x => x).ToArray();
                    _evaluateCallback(newItem);
                    ComputedPermutations++;
                    PossibleFirstRows++;
                    return;
                }
                for (int i = 0; i < states.Length; i++)
                {
                    var currentState = states[i];
                    var parser = FieldParser.CreateFromEnum(currentState);
                    var parseResult = parser.CanParse(reader, streamOffset);
                    if (parseResult.Completed)
                    {
                        n[idx] = currentState;
                        ComputePermutations(reader, n, states, idx + 1, parseResult.OffsetAfter, maxNumberOfFields);
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
    }
}
