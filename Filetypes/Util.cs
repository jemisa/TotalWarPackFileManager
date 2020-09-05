using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes
{
    public static class Util
    {
        public static string SanatizeFixedString(string str)
        {
            var idx = str.IndexOf('\0');
            if (idx != -1)
                return str.Substring(0, idx);
            return str;
        }
    }
}
