using Filetypes.ByteParsing;

namespace Filetypes.RigidModel
{
    public class Bone
    {
        public string Name { get; set; }
        public static Bone Create(ByteChunk chunk)
        {
            return new Bone()
            {
                Name = Util.SanatizeFixedString(chunk.ReadFixedLength(84)),
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
