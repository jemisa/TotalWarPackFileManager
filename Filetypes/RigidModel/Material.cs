using Filetypes.ByteParsing;

namespace Filetypes.RigidModel
{

    public enum TexureType
    {
        Diffuse = 0,
        Normal = 1,
        Alpha = 3,
        Specular = 11,
        Gloss = 12,
    }

    public class Material
    {
        public string Name { get; set; }
        public TexureType Type { get { return (TexureType)TypeRaw; } }
        public int TypeRaw { get; set; }
        public static Material Create(ByteChunk chunk)
        {
            return new Material()
            {
                TypeRaw = chunk.ReadInt32(),
                Name = Util.SanatizeFixedString(chunk.ReadFixedLength(256)),
            };
        }

        public override string ToString()
        {
            return Type + " " + Name;
        }
    }
}
