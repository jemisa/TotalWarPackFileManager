using Filetypes.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace PackFileManagerUnitTests.FileTypes.ByteParsing
{
    [TestClass]
    public class DataParserTests
    {
        [TestMethod]
        public void IntParser()
        {
            ValidateParser_ValidInput(new IntParser(), (int)6001, BitConverter.GetBytes((int)6001), 4);
            ValidateParser_InvalidInput(new IntParser(), new byte[3]);
        }

        [TestMethod]
        public void SingleParser()
        {
            ValidateParser_ValidInput(new SingleParser(), (float)6001.066, BitConverter.GetBytes((float)6001.066), 4);
            ValidateParser_InvalidInput(new SingleParser(), new byte[2]);
        }

        [TestMethod]
        public void ShortParser()
        {
            ValidateParser_ValidInput(new ShortParser(), (short)240, BitConverter.GetBytes((short)240), 2);
            ValidateParser_InvalidInput(new ShortParser(), new byte[1]);
        }

        [TestMethod]
        public void BoolParser()
        {
            ValidateParser_ValidInput(new BoolParser(), true, BitConverter.GetBytes(true), 1);
            ValidateParser_ValidInput(new BoolParser(), false, BitConverter.GetBytes(false), 1);
            ValidateParser_InvalidInput(new BoolParser(), new byte[0]);
            ValidateParser_InvalidInput(new BoolParser(), BitConverter.GetBytes(250));
            ValidateParser_InvalidInput(new BoolParser(), BitConverter.GetBytes(5));
        }

        [TestMethod]
        public void StringParser()
        {
            bool isOptStr = false;
            var encoding = Encoding.UTF8;
            string value0 = "Jonas!";
            string value1 = "4 horses and 1 dog, who controlls what?!";
            string value2 = "";

            ValidateParser_ValidInput(new StringParser(), value0, CreateString(value0, encoding, isOptStr), GetExpectedByteCount(value0, encoding, isOptStr));
            ValidateParser_ValidInput(new StringParser(), value1, CreateString(value1, encoding, isOptStr), GetExpectedByteCount(value1, encoding, isOptStr));
            ValidateParser_ValidInput(new StringParser(), value2, CreateString(value2, encoding, isOptStr), GetExpectedByteCount(value2, encoding, isOptStr));

            ValidateParser_InvalidInput(new StringParser(), new byte[0]);
            ValidateParser_InvalidInput(new StringParser(), BitConverter.GetBytes(250));
            ValidateParser_InvalidInput(new StringParser(), BitConverter.GetBytes(5));
        }

        [TestMethod]
        public void OptionalStringParser()
        {
            bool isOptStr = true;
            var encoding = Encoding.UTF8;
            string value0 = "Jonas!";
            string value1 = "4 horses and 1 dog, who controlls what?!";
            string value2 = "";

            ValidateParser_ValidInput(new OptionalStringParser(), value0, CreateString(value0, encoding, isOptStr), GetExpectedByteCount(value0, encoding, isOptStr));
            ValidateParser_ValidInput(new OptionalStringParser(), value1, CreateString(value1, encoding, isOptStr), GetExpectedByteCount(value1, encoding, isOptStr));
            ValidateParser_ValidInput(new OptionalStringParser(), value2, CreateString(value2, encoding, isOptStr), GetExpectedByteCount(value2, encoding, isOptStr));

            ValidateParser_InvalidInput(new OptionalStringParser(), new byte[0]);
            ValidateParser_InvalidInput(new OptionalStringParser(), BitConverter.GetBytes(250));
            ValidateParser_InvalidInput(new OptionalStringParser(), BitConverter.GetBytes(5));
        }

        [TestMethod]
        public void StringAsciiParser()
        {
            bool isOptStr = false;
            var encoding = Encoding.ASCII;
            string value0 = "Jonas!";
            string value1 = "4 horses and 1 dog, who controlls what?!";
            string value2 = "";

            ValidateParser_ValidInput(new StringAsciiParser(), value0, CreateString(value0, encoding, isOptStr), GetExpectedByteCount(value0, encoding, isOptStr));
            ValidateParser_ValidInput(new StringAsciiParser(), value1, CreateString(value1, encoding, isOptStr), GetExpectedByteCount(value1, encoding, isOptStr));
            ValidateParser_ValidInput(new StringAsciiParser(), value2, CreateString(value2, encoding, isOptStr), GetExpectedByteCount(value2, encoding, isOptStr));

            ValidateParser_InvalidInput(new StringAsciiParser(), new byte[0]);
            ValidateParser_InvalidInput(new StringAsciiParser(), BitConverter.GetBytes(250));
            ValidateParser_InvalidInput(new StringAsciiParser(), BitConverter.GetBytes(5));
        }

        [TestMethod]
        public void OptionalStringAsciiParser()
        {
            bool isOptStr = true;
            var encoding = Encoding.ASCII;
            string value0 = "Jonas!";
            string value1 = "4 horses and 1 dog, who controlls what?!";
            string value2 = "";

            ValidateParser_ValidInput(new OptionalStringAsciiParser(), value0, CreateString(value0, encoding, isOptStr), GetExpectedByteCount(value0, encoding, isOptStr));
            ValidateParser_ValidInput(new OptionalStringAsciiParser(), value1, CreateString(value1, encoding, isOptStr), GetExpectedByteCount(value1, encoding, isOptStr));
            ValidateParser_ValidInput(new OptionalStringAsciiParser(), value2, CreateString(value2, encoding, isOptStr), GetExpectedByteCount(value2, encoding, isOptStr));

            ValidateParser_InvalidInput(new OptionalStringAsciiParser(), new byte[0]);
            ValidateParser_InvalidInput(new OptionalStringAsciiParser(), BitConverter.GetBytes(250));
            ValidateParser_InvalidInput(new OptionalStringAsciiParser(), BitConverter.GetBytes(5));
        }


        void ValidateParser_ValidInput<T>(SpesificByteParser<T> parser, T value, byte[] bytes, int byteSize)
        {
            var canDecodeResult = parser.CanDecode(bytes, 0, out var bytesRead0, out var error0);
            Assert.IsTrue(canDecodeResult);
            Assert.AreEqual(byteSize, bytesRead0);
            Assert.IsNull(error0);

            var tryParseResult = parser.TryDecode(bytes, 0, out var value0, out var bytesRead1, out var error1);
            Assert.IsTrue(tryParseResult);
            Assert.AreEqual(byteSize, bytesRead1);
            Assert.AreEqual(value.ToString(), value0);
            Assert.IsNull(error1);

            var tryDecodeValueResult = parser.TryDecodeValue(bytes, 0, out var value1, out var bytesRead2, out var error2);
            Assert.IsTrue(tryDecodeValueResult);
            Assert.AreEqual(byteSize, bytesRead2);
            Assert.AreEqual(value, value1);
            Assert.IsNull(error2);
        }

        void ValidateParser_InvalidInput<T>(SpesificByteParser<T> parser, byte[] bytes)
        {
            var canDecodeResult = parser.CanDecode(bytes, 0, out var bytesRead0, out var error0);
            Assert.IsFalse(canDecodeResult);
            Assert.AreEqual(0, bytesRead0);
            Assert.IsNotNull(error0);

            var tryParseResult = parser.TryDecode(bytes, 0, out var value0, out var bytesRead1, out var error1);
            Assert.IsFalse(tryParseResult);
            Assert.AreEqual(0, bytesRead1);
            Assert.AreEqual(default(T)?.ToString(), value0);
            Assert.IsNotNull(error1);

            var tryDecodeValueResult = parser.TryDecodeValue(bytes, 0, out var value1, out var bytesRead2, out var error2);
            Assert.IsFalse(tryDecodeValueResult);
            Assert.AreEqual(0, bytesRead2);
            Assert.AreEqual(default(T), value1);
            Assert.IsNotNull(error2);
        }

        byte[] CreateString(string value, Encoding encoding, bool isOptString)
        {
            var optStrSize = isOptString ? 4 : 0;
            int encodingSizeMult = (encoding == Encoding.Unicode ? 2 : 1);

            var buffer = new byte[(value.Length * encodingSizeMult) + 2 + optStrSize];
            if (isOptString)
            {
                var flag = value.Length == 0 ? 0 : 1;
                var optStrBuffer = BitConverter.GetBytes((int)flag);
                for (int i = 0; i < optStrBuffer.Length; i++)
                    buffer[i] = optStrBuffer[i];
            }


            var lengthBuffer = BitConverter.GetBytes((short)value.Length);
            buffer[0 + optStrSize] = lengthBuffer[0];
            buffer[1 + optStrSize] = lengthBuffer[1];


            var stringBuffer = encoding.GetBytes(value);
            for (int i = 0; i < stringBuffer.Length; i++)
                buffer[i + 2 + optStrSize] = stringBuffer[i];

            return buffer;
        }

        int GetExpectedByteCount(string value, Encoding encoding, bool isOptString)
        {
            int encodingSizeMult = (encoding == Encoding.Unicode ? 2 : 1);
            var optStrSize = isOptString ? 4 : 0;
            if (value.Length == 0 && isOptString)
                return 4;
            return optStrSize + 2 + value.Length * encodingSizeMult;
        }
    }
}
