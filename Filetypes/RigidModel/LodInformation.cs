using Filetypes.ByteParsing;

namespace Filetypes.RigidModel
{
    public class LodInformation
    {
        public uint GroupsCount { get; set; }
        public uint Unknown0 { get; set; }
        public uint Unknown1 { get; set; }
        public uint StartOffset { get; set; }
        public float Scale { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }

        public static LodInformation Create(ByteChunk chunk)
        {
            var data = new LodInformation()
            {
                GroupsCount = chunk.ReadUInt32(),
                Unknown0 = chunk.ReadUInt32(),
                Unknown1 = chunk.ReadUInt32(),
                StartOffset = chunk.ReadUInt32(),
                Scale = chunk.ReadSingle(),
                Unknown2 = chunk.ReadUInt32(),
                Unknown3 = chunk.ReadUInt32()
            };
            return data;
        }
    }
}
