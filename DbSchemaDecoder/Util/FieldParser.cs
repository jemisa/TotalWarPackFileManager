using Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbSchemaDecoder.Util
{

    enum FieldParserEnum
    {
        OptStringTypeAscii,
        StringTypeAscii,
        OptStringType,
        StringType,
        BoolType,
        IntType,
        SingleType
    }


    abstract class FieldParser
    {
        public class ParseResult
        {
            public FieldInfo FieldInfo { get; set; }
            public long OffsetAfter { get; set; }
            public bool Completed { get; set; }
        }

        abstract public ParseResult CanParse(BinaryReader reader, long startStreamPos);

        abstract public FieldInfo Instance();

        abstract public string InstanceName();

        public static FieldParser CreateFromEnum(FieldParserEnum parserEnum) 
        {
            switch (parserEnum)
            {
                case FieldParserEnum.OptStringTypeAscii:
                    return new OptStringTypeAscii();
                case FieldParserEnum.StringTypeAscii:
                    return new StringTypeAscii();
                case FieldParserEnum.OptStringType:
                    return new OptStringType();
                case FieldParserEnum.StringType:
                    return new StringType();
                case FieldParserEnum.BoolType:
                    return new BoolType();
                case FieldParserEnum.IntType:
                    return new IntType();
                case FieldParserEnum.SingleType:
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

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.OptStringTypeAscii().CreateInstance();
            if (instance.TryDecode(reader))
            {
                var value = instance.Value;

                byte[] bytes = Encoding.ASCII.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (isAscii)
                {
                    return new ParseResult()
                    {
                        FieldInfo = Types.OptStringTypeAscii(),
                        OffsetAfter = reader.BaseStream.Position,
                        Completed = true
                    };
                }
            }
                
            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.OptStringTypeAscii();
        }


    }

    class StringTypeAscii : FieldParser
    {
        public override string InstanceName()
        {
            return "StringTypeAscii";
        }


        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
           
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.StringTypeAscii().CreateInstance();
            if (instance.TryDecode(reader))
            {
                var value = instance.Value;

                byte[] bytes = Encoding.ASCII.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (isAscii)
                {
                    return new ParseResult()
                    {
                        FieldInfo = Types.StringTypeAscii(),
                        OffsetAfter = reader.BaseStream.Position,
                        Completed = true
                    };
                }
            }

            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.StringTypeAscii();
        }
    }

    class OptStringType : FieldParser
    {
        public override string InstanceName()
        {
            return "OptStringType";
        }

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.OptStringType().CreateInstance();
            if (instance.TryDecode(reader))
            {
                var value = instance.Value;

                byte[] bytes = Encoding.ASCII.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (!isAscii)
                {
                    return new ParseResult()
                    {
                        FieldInfo = Types.OptStringType(),
                        OffsetAfter = reader.BaseStream.Position,
                        Completed = true
                    };
                }
            }
 
            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.OptStringType();
        }
    }

    class StringType : FieldParser
    {
        public override string InstanceName()
        {
            return "StringType";
        }

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.StringType().CreateInstance();
            if (instance.TryDecode(reader))
            {
                var value = instance.Value;

                byte[] bytes = Encoding.ASCII.GetBytes(value);
                var isAscii = bytes.All(b => b >= 32 && b <= 127);
                if (!isAscii)
                { 
                    return new ParseResult()
                    {
                        FieldInfo = Types.StringType(),
                        OffsetAfter = reader.BaseStream.Position,
                        Completed = true
                    };
                }
            }

            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.StringType();
        }
    }

    class BoolType : FieldParser
    {
        public override string InstanceName()
        {
            return "BoolType";
        }

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {

            reader.BaseStream.Position = startStreamPos;
            var instance = Types.BoolType().CreateInstance();
            if (instance.TryDecode(reader))
            {

                return new ParseResult()
                {
                    FieldInfo = Types.BoolType(),
                    OffsetAfter = reader.BaseStream.Position,
                    Completed = true
                };
            }

            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.BoolType();
        }
    }

    class IntType : FieldParser
    {
        public override string InstanceName()
        {
            return "IntType";
        }

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
          
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.IntType().CreateInstance();
            if (instance.TryDecode(reader))
            {

                return new ParseResult()
                {
                    FieldInfo = Types.IntType(),
                    OffsetAfter = reader.BaseStream.Position,
                    Completed = true
                };
            }
    

            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.IntType();
        }
    }

    class SingleType : FieldParser
    {
        public override string InstanceName()
        {
            return "SingleType";
        }

        public override ParseResult CanParse(BinaryReader reader, long startStreamPos)
        {
            reader.BaseStream.Position = startStreamPos;
            var instance = Types.SingleType().CreateInstance();
            if (instance.TryDecode(reader))
            {

                return new ParseResult()
                {
                    FieldInfo = Types.SingleType(),
                    OffsetAfter = reader.BaseStream.Position,
                    Completed = true,
                };
            }

            return new ParseResult()
            {
                Completed = false
            };
        }

        public override FieldInfo Instance()
        {
            return Types.SingleType();
        }
    }
    }
}