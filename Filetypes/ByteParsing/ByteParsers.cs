﻿using Filetypes.DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Filetypes.ByteParsing
{
    public enum DbTypesEnum
    {
        Byte,
        String,
        String_ascii,
        Optstring,
        Optstring_ascii,
        Int64,
        Integer,
        uint32,
        Short,
        Single,
        Boolean,
        List,
    }

    public interface IByteParser
    {
        string TypeName { get; }
        DbTypesEnum Type { get; }
        bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string error);
        bool CanDecode(byte[] buffer, int index, out int bytesRead, out string error);
        //public abstract bool Encode();
    }

    public interface SpesificByteParser<T> : IByteParser
    {
        bool TryDecodeValue(byte[] buffer, int index, out T value, out int bytesRead, out string error);
    }

    public abstract class NumberParser<T> : SpesificByteParser<T>
    {
        protected abstract int FieldSize { get; }
        public abstract DbTypesEnum Type { get; }

        public abstract string TypeName { get; }

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
        public override string TypeName { get { return "Int32"; } }
        public override DbTypesEnum Type => DbTypesEnum.Integer;

        protected override int FieldSize => 4;

        protected override int Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(buffer, index);
        }
    }

    public class Int64Parser : NumberParser<long>
    {
        public override string TypeName { get { return "Int64"; } }
        public override DbTypesEnum Type => DbTypesEnum.Int64;

        protected override int FieldSize => 8;

        protected override long Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt64(buffer, index);
        }
    }

    public class UIntParser : NumberParser<uint>
    {
        public override string TypeName { get { return "UInt32"; } }
        public override DbTypesEnum Type => DbTypesEnum.Integer;

        protected override int FieldSize => 4;

        protected override uint Decode(byte[] buffer, int index)
        {
            return BitConverter.ToUInt32(buffer, index);
        }
    }

    public class ByteParser : NumberParser<byte>
    {
        public override string TypeName { get { return "Byte"; } }
        public override DbTypesEnum Type => DbTypesEnum.Byte;

        protected override int FieldSize => 1;

        protected override byte Decode(byte[] buffer, int index)
        {
            return buffer[index];
        }
    }

    public class SingleParser : NumberParser<float>
    {
        public override string TypeName { get { return "Float"; } }
        public override DbTypesEnum Type => DbTypesEnum.Single;
        protected override int FieldSize => 4;

        protected override float Decode(byte[] buffer, int index)
        {
            return BitConverter.ToSingle(buffer, index);
        }
    }

    public class ShortParser : NumberParser<short>
    {
        public override string TypeName { get { return "Short"; } }
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

        public string TypeName { get { return "Bool"; } }

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

        public virtual string TypeName { get { return "String"; } }

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
                if (bytesLeft < 1)
                {
                    errorMessage = $"Cannot read optString flag {bytesLeft} bytes left";
                    return false;
                }

                var flag = buffer[index];
                if (flag == 0)
                {
                    readTheString = false;
                }
                else if (flag != 1)
                {
                    errorMessage = $"Invalid flag {flag} at beginnning of optStr";
                    return false;
                }
                offset += 1;
                bytesLeft -= 1;
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
                stringLength = bytes;
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
            {
                if (stringLength != 0)
                    value = StringEncoding.GetString(buffer, stringStrt, stringLength);
                else
                    value = "";
            }
            return result;
        }
    }

    public class StringAsciiParser : StringParser
    {
        public override string TypeName { get { return "StringAscii"; } }
        public override DbTypesEnum Type => DbTypesEnum.String_ascii;
        protected override Encoding StringEncoding => Encoding.Unicode;
        protected override bool IsOptStr => false;
    }

    public class OptionalStringParser : StringParser
    {
        public override string TypeName { get { return "Optstring"; } }
        public override DbTypesEnum Type => DbTypesEnum.Optstring;
        protected override Encoding StringEncoding => Encoding.UTF8;
        protected override bool IsOptStr => true;
    }

    public class OptionalStringAsciiParser : StringParser
    {
        public override string TypeName { get { return "OptStringAscii"; } }
        public override DbTypesEnum Type => DbTypesEnum.Optstring_ascii;
        protected override Encoding StringEncoding => Encoding.ASCII;
        protected override bool IsOptStr => true;
    }

    public static class ByteParsers
    {
        public static ByteParser Byte { get; set; } = new ByteParser();
        public static IntParser Int32 { get; set; } = new IntParser();
        public static Int64Parser Int64 { get; set; } = new Int64Parser();
        public static UIntParser UInt32 { get; set; } = new UIntParser();
        public static SingleParser Single { get; set; } = new SingleParser();
        public static ShortParser Short { get; set; } = new ShortParser();
        public static BoolParser Bool { get; set; } = new BoolParser();
        public static OptionalStringParser OptString { get; set; } = new OptionalStringParser();
        public static StringParser String { get; set; } = new StringParser();
        public static OptionalStringAsciiParser OptStringAscii { get; set; } = new OptionalStringAsciiParser();
        public static StringAsciiParser StringAscii { get; set; } = new StringAsciiParser();
    }

    public static class ByteParserFactory
    {
        public static IByteParser Create(DbTypesEnum typeEnum)
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
                    return ByteParsers.Int32;

                case DbTypesEnum.Int64:
                    return ByteParsers.Int64;

                case DbTypesEnum.Short:
                    return ByteParsers.Short;

                case DbTypesEnum.Single:
                    return ByteParsers.Single;

                case DbTypesEnum.Boolean:
                    return ByteParsers.Bool;

                case DbTypesEnum.uint32:
                    return ByteParsers.UInt32;

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

        T Peak<T>(SpesificByteParser<T> parser)
        {
            if (!parser.TryDecodeValue(_buffer, _currentIndex, out T value, out int bytesRead, out string error))
                throw new Exception("Unable to parse :" + error);

            return value;
        }

        public void Read(IByteParser parser, out string value, out string error)
        {
            if (!parser.TryDecode(_buffer, _currentIndex, out  value, out int bytesRead, out error))
                throw new Exception("Unable to parse :" + error);

            _currentIndex += bytesRead;
        }

        public string ReadStringAscii() => Read(ByteParsers.StringAscii);
        public string ReadString() => Read(ByteParsers.String);
        public int ReadInt32() => Read(ByteParsers.Int32);
        public uint ReadUInt32() => Read(ByteParsers.UInt32);
        public float ReadSingle() => Read(ByteParsers.Single);
        public short ReadShort() => Read(ByteParsers.Short);
        public bool ReadBool() => Read(ByteParsers.Bool);
        public byte ReadByte() => Read(ByteParsers.Byte);


        public uint PeakUint32() => Peak(ByteParsers.UInt32);
    }




}
