using Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.Codecs
{
    public class DelegatingDbFileCodec : ICodec<DBFile>
    {
        public ICodec<DBFile> Codec { get; set; }
        public void Encode(Stream stream, DBFile file)
        {
            Codec.Encode(stream, file);
        }
        public DBFile Decode(Stream stream)
        {
            return Codec.Decode(stream);
        }
    }
}
