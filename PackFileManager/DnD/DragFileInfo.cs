using System;
using System.IO;

namespace PackFileManager
{
    public class DragFileInfo
    {
        public string FileName;
        public string SourceFileName;
        public DateTime WriteTime;
        public Int64 FileSize;

        public DragFileInfo(string fileName, long fileSize)
        {
            FileName = Path.GetFileName(fileName);
            SourceFileName = fileName;
            WriteTime = DateTime.Now;
            FileSize = fileSize;
        }
    }
}