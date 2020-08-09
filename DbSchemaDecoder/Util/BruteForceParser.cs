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

namespace DbSchemaDecoder.Util
{
    class BruteForceParser
    {
        public class StatusUpdate
        {
            public int computedPermutations { get; set; }
            public int skippedEarlyPermutations { get; set; }
        }

        public event EventHandler<StatusUpdate> OnParsingCompleteEvent;
        public event EventHandler<FieldParserEnum[]> OnCombintionFoundEvent;
        public event EventHandler<StatusUpdate> OnStatusUpdateEvent;

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


        public int PossibleCombinations { get { return (int)Math.Pow(_maxNumberOfFields, GetPossibleFields().Count()); } }
        public List<FieldParserEnum[]> PossiblePermutations { get; set; } = new List<FieldParserEnum[]>();

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
            PermutationHelper permutationHelper = new PermutationHelper(OnEvaluatePermutation, OnUpdateStatus);
            permutationHelper.ComputePermutations(
                _permutationReader, 
                new FieldParserEnum[_maxNumberOfFields], 
                GetPossibleFields(), 
                0,
                _headerLength,
                _maxNumberOfFields);

            _permutationReader.Dispose();
            _permutationStream.Dispose();

            
            StatusUpdate update = new StatusUpdate()
            { 
                computedPermutations = permutationHelper.computedPermutations,
                skippedEarlyPermutations = permutationHelper.skippedEarlyPermutations 
            };
            OnParsingCompleteEvent?.Invoke(this, update);
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

        void OnUpdateStatus(int computedPermutations, int skippedEarlyPermutations)
        {
            StatusUpdate update = new StatusUpdate() 
            { computedPermutations = computedPermutations, skippedEarlyPermutations = skippedEarlyPermutations };
            OnStatusUpdateEvent?.Invoke(this, update);
        }

        void UpdatePossiblePermutation(FieldParserEnum[] combination)
        {
            PossiblePermutations.Add(combination);
            OnCombintionFoundEvent?.Invoke(this, combination);
        }


        class PermutationHelper
        {
            public List<FieldParserEnum[]> Results { get; set; } = new List<FieldParserEnum[]>();

            public delegate void OnCombintionFoundDelegate(FieldParserEnum[] combination);
            public delegate void OnUpdateStatusDelegate(int computedPermutations, int skippedEarlyPermutations);

            OnCombintionFoundDelegate _evaluateCallback;
            OnUpdateStatusDelegate _statusCallback;
            public PermutationHelper(OnCombintionFoundDelegate callback, OnUpdateStatusDelegate statusCallback)
            {
                _evaluateCallback = callback;
                _statusCallback = statusCallback;
            }

            public int computedPermutations { get; set; } = 0;
            public  int skippedEarlyPermutations { get; set; } = 0;           
            void UpdateStatus(int updateVariable)
            {
                if (updateVariable % 10000 == 0)
                    _statusCallback(computedPermutations, skippedEarlyPermutations);
            }
            public void ComputePermutations(BinaryReader reader, FieldParserEnum[] n, FieldParserEnum[] states, int idx, long streamOffset, int maxNumberOfFields)
            {
                if (idx == n.Length)
                {
                    var newItem = n.Select(x => x).ToArray();
                    Results.Add(newItem);
                    _evaluateCallback(newItem);
                    computedPermutations++;
                    UpdateStatus(computedPermutations);
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
                        skippedEarlyPermutations++;
                        UpdateStatus(skippedEarlyPermutations);
                    }

                }
            }
        }
    }
}
