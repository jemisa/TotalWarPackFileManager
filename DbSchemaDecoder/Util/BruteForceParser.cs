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

        public List<string> BruteForce(DataBaseFile dataBaseFile, int maxNumberOfFields)
        {
            var possibleSchamaList = new List<string>();
            var permutationTimer = new Stopwatch();
            var tableValidationTimer = new Stopwatch();
            var possibleMutations = Math.Pow(maxNumberOfFields, 6);
            int actuallPossibleMutations = 0;

            using (var stream = new MemoryStream(dataBaseFile.DbFile.Data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    DBFileHeader header = PackedFileDbCodec.readHeader(reader);

                    permutationTimer.Start();

                    var states = GetPossibleFields();
                    PermutationHelper helper = new PermutationHelper();
                    helper.ComputePermutations(reader, new FieldParserEnum[maxNumberOfFields], states, 0, header.Length, maxNumberOfFields);
                    actuallPossibleMutations = helper.Result.Count();
                    
                    permutationTimer.Stop();

                  

                    tableValidationTimer.Start();
                    for (int i = 0; i < helper.Result.Count(); i++)
                    {
                        var possibleSchema = helper.Result[i].Select(x => FieldParser.CreateFromEnum(x).Instance()).ToList();
                        reader.BaseStream.Position = header.Length;

                        TableEntriesParser p = new TableEntriesParser();
                        var updateRes = p.CanParseTable(
                            reader,
                            stream.Capacity,
                            possibleSchema,
                            (int)header.EntryCount);

                        if (!updateRes.HasError)
                        {
                            var nameList = helper.Result[i].Select(x => FieldParser.CreateFromEnum(x).InstanceName()).ToList();
                            possibleSchamaList.Add(string.Join(", ", nameList));
                        }
                    }
                    tableValidationTimer.Stop();

                }
            }

            return possibleSchamaList;
        }

        class PermutationHelper
        {
            public List<FieldParserEnum[]> Result { get; set; } = new List<FieldParserEnum[]>();
            public void ComputePermutations(BinaryReader reader, FieldParserEnum[] n, FieldParserEnum[] states, int idx, long streamOffset, int maxNumberOfFields)
            {
                if (idx == n.Length)
                {
                    Result.Add(n.Select(x => x).ToArray());
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

                }
            }
        }
    }
}
