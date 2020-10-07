using Filetypes.ByteParsing;
using System.Collections.Generic;

namespace Filetypes.RigidModel
{
    public class LodHeader
    {
        public uint GroupsCount { get; set; }
        public uint VerticesDataLength  { get; set; }
        public uint IndicesDataLength  { get; set; }
        public uint StartOffset { get; set; }
        public float LodZoomFactor { get; set; }
        public uint LodLevel { get; set; }
        public uint Unknown { get; set; }


        public List<LodModel> LodModels = new List<LodModel>();

        public static LodHeader Create(ByteChunk chunk)
        {
            var data = new LodHeader()
            {
                GroupsCount = chunk.ReadUInt32(),
                VerticesDataLength  = chunk.ReadUInt32(),
                IndicesDataLength  = chunk.ReadUInt32(),
                StartOffset = chunk.ReadUInt32(),
                LodZoomFactor = chunk.ReadSingle(),
                LodLevel = chunk.ReadUInt32(),
                Unknown = chunk.ReadUInt32()
            };
            return data;
        }
    }
}
