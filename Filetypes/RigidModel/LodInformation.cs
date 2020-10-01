using Filetypes.ByteParsing;
using System.Collections.Generic;

namespace Filetypes.RigidModel
{
    public class LodInformation
    {
        public uint GroupsCount { get; set; }
        public byte[] Unknown0 { get; set; }
        public byte[] Unknown1 { get; set; }
        public byte[] Unknown2 { get; set; }
        public uint StartOffset { get; set; }
        public float Scale { get; set; }
        public uint LodLevel { get; set; }//??
       

        public List<LodModel> LodModels = new List<LodModel>();

        public static LodInformation Create(ByteChunk chunk)
        {
            var data = new LodInformation()
            {
                GroupsCount = chunk.ReadUInt32(),
                Unknown0 = chunk.ReadBytes(4),
                Unknown1 = chunk.ReadBytes(4),
                StartOffset = chunk.ReadUInt32(),
                Scale = chunk.ReadSingle(),
                LodLevel = chunk.ReadUInt32(),
                Unknown2 = chunk.ReadBytes(4)
            };
            return data;
        }
    }
}
