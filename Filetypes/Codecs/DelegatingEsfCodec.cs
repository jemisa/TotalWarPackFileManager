using Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.Codecs
{
    public class DelegatingEsfCodec : ICodec<EsfNode>
    {
        EsfCodec codecDelegate;
        public EsfNode Decode(Stream stream)
        {
            codecDelegate = EsfCodecUtil.GetCodec(stream);
            return codecDelegate.Parse(stream);
        }
        public void Encode(Stream encodeTo, EsfNode node)
        {
            using (var writer = new BinaryWriter(encodeTo))
            {
                codecDelegate.EncodeRootNode(writer, node);
            }
        }
    }
}
