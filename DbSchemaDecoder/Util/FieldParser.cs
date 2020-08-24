using Filetypes;
using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{

    /*public enum FieldParserEnum
    {
        OptStringTypeAscii,
        StringTypeAscii,
        OptStringType,
        StringType,
        BoolType,
        IntType,
        SingleType
    }*/


    abstract class FieldParser
    {
        public class ParseResult
        {
            public object FieldInfo { get; set; }
            public int OffsetAfter { get; set; }
            public bool Completed { get; set; }
        }

        abstract public ParseResult CanParse(byte[] buffer, int index);

        //abstract public object Instance();

        abstract public string InstanceName();

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
        public override string InstanceName()
        {
            return "OptStringTypeAscii";
        }

        public override ParseResult CanParse(byte[] buffer, int index)
        {
            var parser = ByteParsers.OptStringAscii;
            if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
            {
                byte[] bytes = Encoding.ASCII.GetBytes(value);
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

        //public override FieldInfo Instance()
        //{
        //    return Types.OptStringTypeAscii();
        //}


    }

    class StringTypeAscii : FieldParser
    {
        public override string InstanceName()
        {
            return "StringTypeAscii";
        }


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

                    byte[] bytes = Encoding.ASCII.GetBytes(value);
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

        //public override FieldInfo Instance()
        //{
        //    return Types.StringTypeAscii();
        //}
    }

    class OptStringType : FieldParser
    {
        public override string InstanceName()
        {
            return "OptStringType";
        }

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


        //public override FieldInfo Instance()
        //{
        //    return Types.OptStringType();
        //}
    }

    class StringType : FieldParser
    {
        public override string InstanceName()
        {
            return "StringType";
        }

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

        //public override FieldInfo Instance()
        //{
        //    return Types.StringType();
        //}
    }

    class BoolType : FieldParser
    {
        public override string InstanceName()
        {
            return "BoolType";
        }

            public override ParseResult CanParse(byte[] buffer, int index)
            {
                var parser = ByteParsers.Bool;
                if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
                {
                    return new ParseResult()
                    {
                        //FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }

                return new ParseResult()
                {
                    Completed = false
                };
            }

        //public override FieldInfo Instance()
        //{
        //    return Types.BoolType();
        //}
    }

    class IntType : FieldParser
    {
        public override string InstanceName()
        {
            return "IntType";
        }

            public override ParseResult CanParse(byte[] buffer, int index)
            {
                var parser = ByteParsers.Int32;
                if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
                {
                    return new ParseResult()
                    {
                        //FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }

                return new ParseResult()
                {
                    Completed = false
                };
            }

        //    public override FieldInfo Instance()
        //{
        //    return Types.IntType();
        //}
    }

    class SingleType : FieldParser
    {
        public override string InstanceName()
        {
            return "SingleType";
        }

            public override ParseResult CanParse(byte[] buffer, int index)
            {
                var parser = ByteParsers.Single;
                if (parser.TryDecode(buffer, index, out string value, out var bytesRead, out string error))
                {
                    return new ParseResult()
                    {
                        //FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = index + bytesRead,
                        Completed = true
                    };
                }

                return new ParseResult()
                {
                    Completed = false
                };
            }

       //    public override FieldInfo Instance()
       //{
       //    return Types.SingleType();
       //}
    }
        
    }
}