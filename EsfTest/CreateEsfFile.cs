using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EsfLibrary;

namespace EsfTest
{
    class CreateEsfFile
    {
        public static void Main(string[] args)
        {
            AbcaFileCodec codec = new AbcaFileCodec
            {
                Header = new EsfHeader
                {
                    ID = 0xABCA
                }
            };
            RecordNode root = new RecordNode(codec)
            {
                Name = "root"
            };
            RecordNode child = new RecordNode(codec)
            {
                Name = "copySource"
            };
            EsfValueNode<string> utf16String = (EsfValueNode<string>)codec.CreateValueNode(EsfType.UTF16);
            utf16String.Value = "utf16";
            child.Value.Add(utf16String);
            root.Value.Add(child);
            child = new RecordNode(codec)
            {
                Name = "copyTarget"
            };
            EsfValueNode<string> copyString = (EsfValueNode<string>) utf16String.CreateCopy();
            // copyString.Value = "copy";
            copyString.FromString("copy");
            child.Value.Add(copyString);
            root.Value.Add(child);

            // this is needed for the file to create the record nodes properly
            SortedList<int, string> nodeNames = new SortedList<int, string>();
            nodeNames.Add(0, "root");
            nodeNames.Add(1, "copySource");
            nodeNames.Add(2, "copyTarget");
            // this property needs to be added to the EsfFileCodec, getting and setting EsfFileCodec#nodeNames
            codec.NodeNames = nodeNames;

            EsfFile file = new EsfFile(root, codec);
            EsfCodecUtil.WriteEsfFile("string_container.esf", file);
        }
    }
}
