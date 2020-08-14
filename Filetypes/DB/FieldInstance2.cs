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
        string _enumvalue;
        IFieldParser _parser;
        FieldMetaData _metaData;
        
    }

    class FieldInstanceValue
    {
        FieldInstance2 _stuff;
        string _currentValue;

    }
}
