﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text;

namespace Filetypes.ByteParsing
{
    public interface ByteParser
    {
        DbTypesEnum Type { get; }
        bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string error);
        bool CanDecode(byte[] buffer, int index, out int bytesRead, out string error);
        //public abstract bool Encode();
    }

    public interface SpesificByteParser<T> : ByteParser
    {
        bool TryDecodeValue(byte[] buffer, int index, out T value, out int bytesRead, out string error);
    }

    public abstract class NumberParser<T> : SpesificByteParser<T>
    {
        protected abstract int FieldSize { get; }
        public abstract DbTypesEnum Type { get; }

        protected abstract T Decode(byte[] buffer, int index);

        public bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error)
        {
            if (buffer.Length - index < FieldSize)
            {
                bytesRead = 0;
                _error = "Not enough space in stream";
                return false;
            }
            bytesRead = FieldSize;
            _error = null;
            return true;
        }

        public bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error)
        {
            var result = TryDecodeValue(buffer, index, out T temp, out bytesRead, out _error);
            value = temp.ToString();
            return result;
        }

        public bool TryDecodeValue(byte[] buffer, int index, out T value, out int bytesRead, out string _error)
        {
            value = default;
            bool canDecode = CanDecode(buffer, index, out bytesRead, out _error);
            if (canDecode)
                value = Decode(buffer, index);
            return canDecode;
        }
    }

    public class IntParser : NumberParser<int>
    {
        public override DbTypesEnum Type => DbTypesEnum.Integer;

        protected override int FieldSize => 4;

        protected override int Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(buffer, index);
        }
    }

    public class SingleParser : NumberParser<float>
    {
        public override DbTypesEnum Type => DbTypesEnum.Single;
        protected override int FieldSize => 4;

        protected override float Decode(byte[] buffer, int index)
        {
            return BitConverter.ToSingle(buffer, index);
        }
    }

    public class ShortParser : NumberParser<short>
    {
        public override DbTypesEnum Type => DbTypesEnum.Short;
        protected override int FieldSize => 2;

        protected override short Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt16(buffer, index);
        }
    }

    public class BoolParser : SpesificByteParser<bool>
    {
        public DbTypesEnum Type => DbTypesEnum.Boolean;
        protected int FieldSize => 1;

        public bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error)
        {
            if (buffer.Length - index < FieldSize)
            {
                bytesRead = 0;
                _error = "Not enough space in stream";
                return false;
            }
            var value = buffer[index];
            if (!(value == 1 || value == 0))
            {
                bytesRead = 0;
                _error = value + " is not a valid bool";
                return false;
            }

            bytesRead = FieldSize;
            _error = null;
            return true;
        }

        public bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error)
        {
            var result = TryDecodeValue(buffer, index, out var temp, out bytesRead, out _error);
            value = temp.ToString();
            return result;
        }

        public bool TryDecodeValue(byte[] buffer, int index, out bool value, out int bytesRead, out string _error)
        {
            value = false;
            bool canDecode = CanDecode(buffer, index, out bytesRead, out _error);
            if (canDecode)
                value = (buffer[index] == 1);
            return canDecode;
        }
    }

    public class StringParser : SpesificByteParser<string>
    {
        virtual public DbTypesEnum Type => DbTypesEnum.String;

        virtual protected Encoding StringEncoding => Encoding.UTF8;
        virtual protected bool IsOptStr => false;

        bool TryReadReadCAStringAsArray(byte[] buffer, int index, Encoding encoding, bool isOptString,
             out string errorMessage, out int stringStart, out int stringLength, out int bytesInString)
        {
            stringStart = 0;
            stringLength = 0;
            bytesInString = 0;
            var bytesLeft = buffer.Length - index;

            int offset = 0;
            bool readTheString = true;
            if (isOptString)
            {
                if (bytesLeft < 4)
                {
                    errorMessage = $"Cannot read optString flag {bytesLeft} bytes left";
                    return false;
                }

                var flag = BitConverter.ToInt32(buffer, index);
                if (flag == 0)
                {
                    readTheString = false;
                }
                else if (flag != 1)
                {
                    errorMessage = $"Invalid flag {flag} at beginnning of optStr";
                    return false;
                }
                offset += 4;
                bytesLeft -= 4;
            }

            if (readTheString)
            {
                if (bytesLeft < 2)
                {
                    errorMessage = $"Cannot read length of string {bytesLeft} bytes left";
                    return false;
                }

                int num = BitConverter.ToInt16(buffer, index + offset);
                bytesLeft -= 2;
                if (0 > num)
                {
                    errorMessage = "Negative file length";
                    return false;
                }

                // Unicode is 2 bytes per character; UTF8 is variable, but the number stored is the number of bytes, so use that
                int bytes = (encoding == Encoding.Unicode ? 2 : 1) * num;
                // enough data left?
                if (bytesLeft < bytes)
                {
                    errorMessage = string.Format("Cannot read string of length {0}: only {1} bytes left", bytes, bytesLeft);
                    return false;
                }

                stringStart = (index + 2 + offset);
                stringLength = num;
                bytesInString = bytes + 2;
            }

            bytesInString += offset;
            errorMessage = null;
            return true;
        }

        public bool CanDecode(byte[] buffer, int index, out int bytesRead, out string error)
        {
            return TryReadReadCAStringAsArray(buffer, index, StringEncoding, IsOptStr, out error, out _, out _, out bytesRead);
        }

        public bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string error)
        {
            return TryDecodeValue(buffer, index, out value, out bytesRead, out error);
        }

        public bool TryDecodeValue(byte[] buffer, int index, out string value, out int bytesRead, out string error)
        {
            value = null;
            var result = TryReadReadCAStringAsArray(buffer, index, StringEncoding, IsOptStr, out error, out int stringStrt, out int stringLength, out bytesRead);
            if (result)
                value = StringEncoding.GetString(buffer, stringStrt, stringLength);
            return result;
        }
    }

    public class StringAsciiParser : StringParser
    {
        public override DbTypesEnum Type => DbTypesEnum.String_ascii;
        protected override Encoding StringEncoding => Encoding.ASCII;
        protected override bool IsOptStr => false;
    }

    public class OptionalStringParser : StringParser
    {
        public override DbTypesEnum Type => DbTypesEnum.Optstring;
        protected override Encoding StringEncoding => Encoding.UTF8;
        protected override bool IsOptStr => true;
    }

    public class OptionalStringAsciiParser : StringParser
    {
        public override DbTypesEnum Type => DbTypesEnum.Optstring_ascii;
        protected override Encoding StringEncoding => Encoding.ASCII;
        protected override bool IsOptStr => true;
    }

    public static class ByteParsers
    {
        public static IntParser Int32 { get; set; } = new IntParser();
        public static SingleParser Single { get; set; } = new SingleParser();
        public static ShortParser Short { get; set; } = new ShortParser();
        public static BoolParser Bool { get; set; } = new BoolParser();
        public static OptionalStringParser OptString { get; set; } = new OptionalStringParser();
        public static StringParser String { get; set; } = new StringParser();
        public static OptionalStringAsciiParser OptStringAscii { get; set; } = new OptionalStringAsciiParser();
        public static StringAsciiParser StringAscii { get; set; } = new StringAsciiParser();
    }

    public static class ParserFactory
    {
        public static ByteParser Create(DbTypesEnum typeEnum)
        {
            switch (typeEnum)
            {
                case DbTypesEnum.String:
                    return ByteParsers.String;

                case DbTypesEnum.String_ascii:
                    return ByteParsers.StringAscii;

                case DbTypesEnum.Optstring:
                    return ByteParsers.OptString;

                case DbTypesEnum.Optstring_ascii:
                    return ByteParsers.OptStringAscii;

                case DbTypesEnum.Integer:
                case DbTypesEnum.Autonumber:
                    return ByteParsers.Int32;

                case DbTypesEnum.Short:
                    return ByteParsers.Short;

                case DbTypesEnum.Float:
                case DbTypesEnum.Single:
                    return ByteParsers.Single;

                case DbTypesEnum.Boolean:
                    return ByteParsers.Bool;

                case DbTypesEnum.Decimal:
                case DbTypesEnum.List:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        // From string?
        // FromEnum
        // From Ca type
        // ??
    }

    public class ByteChunk
    {
        byte[] _buffer;
        int _currentIndex;
        public ByteChunk(byte[] buffer, int index = 0)
        {
            _buffer = buffer;
            _currentIndex = index;
        }

        public int BytesLeft => _buffer.Length - _currentIndex;

        T Read<T>(SpesificByteParser<T> parser)
        {
            if (!parser.TryDecodeValue(_buffer, _currentIndex, out T value, out int bytesRead, out string error))
                throw new Exception("Unable to parse :" + error);

            _currentIndex += bytesRead;
            return value;
        }

        public int ReadInt32() => Read(ByteParsers.Int32);
        public float ReadSingle() => Read(ByteParsers.Single);
        public short ReadShort() => Read(ByteParsers.Short);
        public bool ReadBool() => Read(ByteParsers.Bool);
    }


    public class DbFieldMetaData : ICloneable
    { 
        public string Name { get; set; }
        public string FieldReference { get; set; }
        public string TableReference { get; set; }
        public bool IsKey { get; set; }
        public bool IsOptional { get; set; }
        public int MaxLength { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class DbColumnDefinition 
    {
        public DbFieldMetaData MetaData { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DbTypesEnum Type { get; set; }
    }

    public class DbField : ICloneable
    {
        ByteParser _parser;
       
        public ByteParser Parser { get { return _parser; } }
        public string Value { get; set; }
        public string Error { get; set; }
        public bool HasError { get; set; }
        public DbTypesEnum Type { get { return _parser.Type; } set { _parser = ParserFactory.Create(value); } }
        public void Decode()
        { }

        public void Encode()
        { }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

}
