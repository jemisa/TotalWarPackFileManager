using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PackFileManager
{
    public class DragDropHelper
    {
        public const string CFSTR_PREFERREDDROPEFFECT = "Preferred DropEffect";
        public const string CFSTR_PERFORMEDDROPEFFECT = "Performed DropEffect";
        public const string CFSTR_FILEDESCRIPTORW = "FileGroupDescriptorW";
        public const string CFSTR_FILECONTENTS = "FileContents";

        public const Int32 FD_WRITESTIME = 0x00000020;
        public const Int32 FD_FILESIZE = 0x00000040;
        public const Int32 FD_PROGRESSUI = 0x00004000;

        public static MemoryStream GetFileDescriptor(DragFileInfo fileInfo)
        {
            var stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(1), 0, sizeof(UInt32));

            var fileDescriptor = new FILEDESCRIPTOR();

            fileDescriptor.cFileName = fileInfo.FileName;
            var fileWriteTimeUtc = fileInfo.WriteTime.ToFileTimeUtc();
            fileDescriptor.ftLastWriteTime.dwHighDateTime = (Int32)(fileWriteTimeUtc >> 32);
            fileDescriptor.ftLastWriteTime.dwLowDateTime = (Int32)(fileWriteTimeUtc & 0xFFFFFFFF);
            fileDescriptor.nFileSizeHigh = (UInt32)(fileInfo.FileSize >> 32);
            fileDescriptor.nFileSizeLow = (UInt32)(fileInfo.FileSize & 0xFFFFFFFF);
            fileDescriptor.dwFlags = FD_WRITESTIME | FD_FILESIZE | FD_PROGRESSUI;

            var fileDescriptorSize = Marshal.SizeOf(fileDescriptor);
            var fileDescriptorPointer = Marshal.AllocHGlobal(fileDescriptorSize);
            var fileDescriptorByteArray = new Byte[fileDescriptorSize];

            try
            {
                Marshal.StructureToPtr(fileDescriptor, fileDescriptorPointer, true);
                Marshal.Copy(fileDescriptorPointer, fileDescriptorByteArray, 0, fileDescriptorSize);
            }
            finally
            {
                Marshal.FreeHGlobal(fileDescriptorPointer);
            }
            stream.Write(fileDescriptorByteArray, 0, fileDescriptorByteArray.Length);
            return stream;
        }

        public static MemoryStream GetFileContents(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            return stream;
        }
    }
}