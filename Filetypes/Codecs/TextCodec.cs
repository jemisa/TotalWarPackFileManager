using Filetypes;
using System;
using System.IO;
using System.Text;

namespace Filetypes.Codecs
{
    /*
    * Lightweight codec to read data as ASCII string from the stream.
    */
    public class TextCodec : ICodec<string>
    {
        public static readonly TextCodec Instance = new TextCodec();
        public string Decode(Stream file)
        {
            string result = "";
            using (var reader = new StreamReader(file, Encoding.ASCII))
            {
                result = reader.ReadToEnd();
            }
#if DEBUG
            Console.WriteLine("read string\n{0}", result);
#endif
            return result;
        }
        public void Encode(Stream stream, string toEncode)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(toEncode);
            }
        }
    }
}
