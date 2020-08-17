using Common;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Filetypes.DB
{
    /*
    class ParseResult
    {
        public int BytesRead;
        public string errorMessage;
        public bool sucsess;
        public object Value;
    }

    interface IFieldParser
    {
        ParseResult TryParse(byte[] array, int index);
    }

    abstract class NumberParser<T> : IFieldParser 
    {
        protected abstract int FieldSize { get; }

        public ParseResult TryParse(byte[] array, int index)
        {
            T value = FromByte(array, index);
            var res = new ParseResult();
            res.Value = value;
            return res;
        }

        public void Write(byte[] array, int index)
        {
            object test = 123;
            var bytes = ToBytes(test);
        }

        protected abstract T FromByte(byte[] array, int index);
        protected abstract byte[] ToBytes(object item);
    }

    class ShortParser : NumberParser<short>
    {
        protected override int FieldSize => sizeof(short);

        protected override short FromByte(byte[] array, int index)
        {
            return BitConverter.ToInt16(array, index);
        }

        protected override byte[] ToBytes(object item)
        {
            return BitConverter.GetBytes((short)item);
        }
    }

    class IntParer : NumberParser<int>
    {
        protected override int FieldSize => sizeof(int);

        protected override int FromByte(byte[] array, int index)
        {
            return BitConverter.ToInt32(array, index);
        }
    }


    class Tester
    {

        public void Test()
        {
            List<IFieldParser> parsers = new List<IFieldParser>();
            parsers.Add(new ShortParser());
            parsers.Add(new IntParer());


            new IntParer().TryParse()

            foreach (var item in parsers)
            {
                var res = item.TryParse(null, 0);
            }
        }
    }

    class FieldMetaData
    { 
        // Name,
        // Relation
        // Bla bla bla
    }

    class FieldInstance2
    {
        IFieldParser _parser;   // static?
        FieldMetaData _metaData;
    }

    class FieldInstanceValue
    {
        FieldInstance2 _stuff;
        string _currentValue;
        bool _valid;

    }*/


    abstract class FieldParser
    {
        public abstract bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error);
        public abstract bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error);
        //public abstract bool Encode();
    }

    abstract class NumberParser : FieldParser
    {
        protected abstract int FieldSize { get; }

        protected abstract string Decode(byte[] buffer, int index);

        public override bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error)
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

        public override bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error)
        {
            value = null;
            bool canDecode = CanDecode(buffer, index, out bytesRead, out _error);
            if (canDecode)
                value = Decode(buffer, index);
            return canDecode;
        }
    }

    class IntFieldParser : NumberParser
    {
        protected override int FieldSize => 4;

        protected override string Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(buffer, index).ToString();
        }
    }

    class SingleFieldParser : NumberParser
    {
        protected override int FieldSize => 4;

        protected override string Decode(byte[] buffer, int index)
        {
            return BitConverter.ToSingle(buffer, index).ToString();
        }
    }

    class ShortFieldParser : NumberParser
    {
        protected override int FieldSize => 2;

        protected override string Decode(byte[] buffer, int index)
        {
            return BitConverter.ToInt16(buffer, index).ToString();
        }
    }

    class BoolFieldParser : FieldParser
    {
        protected int FieldSize => 1;

        public override bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error)
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

        public override bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error)
        {
            value = null;
            bool canDecode = CanDecode(buffer, index, out bytesRead, out _error);
            if (canDecode)
                value = (buffer[index] == 1).ToString();
            return canDecode;
        }
    }


    class OptStringFieldParser : FieldParser
    {
        public override bool CanDecode(byte[] buffer, int index, out int bytesRead, out string _error)
        {
            _error = null;
            bytesRead = 0;
            byte flag = buffer[index];
            if (flag == 1)
            {
                var result = IOFunctions.ReadCAString(reader, stringEncoding);
            }
            else if (flag != 0)
            {

                _error = "can never be";

            }

            if (buffer.Length < index)
                _error = "out of range";
            bytesRead = 4;
            return true;
        }

        public override bool TryDecode(byte[] buffer, int index, out string value, out int bytesRead, out string _error)
        {
            var intVal = BitConverter.ToInt32(buffer, index);
            value = "sdf";
            bytesRead = 4;
            _error = "";
            return true;
        }
    }
}
