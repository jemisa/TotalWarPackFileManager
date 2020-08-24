using Filetypes.ByteParsing;
using System;
using System.Linq;
using System.Text;

namespace DbSchemaDecoder.Util
{
    abstract class FieldParser
    {
        public class ParseResult
        {
            public int OffsetAfter { get; set; }
            public bool Completed { get; set; }
        }

        abstract public ParseResult CanParse(byte[] buffer, int index);

        public static FieldParser CreateFromEnum(DbTypesEnum parserEnum) 
        {
            switch (parserEnum)
            {
                case DbTypesEnum.Optstring_ascii:
                    return new OptStringTypeAscii();
                case DbTypesEnum.String_ascii:
                    return new StringTypeAscii();
                case DbTypesEnum.Optstring:
                    return new OptStringType();
                case DbTypesEnum.String:
                    return new StringType();
                case DbTypesEnum.Boolean:
                    return new BoolType();
                case DbTypesEnum.Integer:
                    return new IntType();
                case DbTypesEnum.Single:
                    return new SingleType();
            }

            throw new NotImplementedException();
        }

    class OptStringTypeAscii : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.OptStringAscii;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (isAscii)
                {
                    return new ParseResult()
                    {
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }
            }
                
            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class StringTypeAscii : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.StringAscii;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new ParseResult()
                    {
                        Completed = false
                    };

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (isAscii)
                {
                    return new ParseResult()
                    {
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }
            }

            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class OptStringType : FieldParser
    {

        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.OptString;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                var isCorrectType = bytes.All(b => b >= 32 && b <= 127);
                if (isCorrectType)
                {
                    return new ParseResult()
                    {
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }
            }

            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class StringType : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.String;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                if (string.IsNullOrWhiteSpace(value))
                    return new ParseResult()
                    {
                        Completed = false
                    };

                byte[] bytes = Encoding.UTF8.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (isAscii)
                {
                    return new ParseResult()
                    {
                        //FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }
            }

            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class BoolType : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.Bool;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                return new ParseResult()
                {
                    OffsetAfter = index + bytesRead,
                    Completed = true
                };
            }

            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class IntType : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.Int32;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                return new ParseResult()
                {
                    OffsetAfter = index + bytesRead,
                    Completed = true
                };
            }

            return new ParseResult()
            {
                Completed = false
            };
        }
    }

    class SingleType : FieldParser
    {
        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.Single;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                return new ParseResult()
                {
                    OffsetAfter = index + bytesRead,
                    Completed = true
                };
            }

            return new ParseResult()
            {
                Completed = false
            };
        }

    }
        
    }
}