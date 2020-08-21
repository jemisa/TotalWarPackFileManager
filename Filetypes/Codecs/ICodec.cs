using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Filetypes.Codecs {
    public interface ICodec<T> 
    {
        T Decode(Stream file);
        void Encode(Stream stream, T toEncode);
    }
	
	
}
