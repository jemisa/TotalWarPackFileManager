using System;
using System.Runtime.InteropServices;

namespace PackFileManager
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    struct FILEDESCRIPTOR
    {
        public UInt32 dwFlags;
        public Guid clsid;
        public System.Drawing.Size sizel;
        public System.Drawing.Point pointl;
        public UInt32 dwFileAttributes;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
        public UInt32 nFileSizeHigh;
        public UInt32 nFileSizeLow;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public String cFileName;
    }
}