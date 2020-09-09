using Filetypes.ByteParsing;

namespace Filetypes.RigidModel
{
    public class Material
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public static Material Create(ByteChunk chunk)
        {
            return new Material()
            {
                Type = chunk.ReadInt32(),
                Name = Util.SanatizeFixedString(chunk.ReadFixedLength(256)),
            };
        }

        public override string ToString()
        {
            return Type + " " + Name;
        }
    }
}
