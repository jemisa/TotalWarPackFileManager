using Filetypes.ByteParsing;
using System.Collections.Generic;
using System.IO;

namespace Filetypes.RigidModel
{
    public class RigidModel
    {
        public string FileType { get; set; }
        public uint MeshCount { get; set; }
        public uint LodCount { get; set; }
        public string BaseSkeleton { get; set; }
        public List<LodInformation> LodInformations = new List<LodInformation>();
        public List<LodModel> LodModels = new List<LodModel>();

        static bool Validate(ByteChunk chunk, out string errorMessage)
        {
            errorMessage = "";
            return true;
        }

        public static RigidModel Create(ByteChunk chunk, out string errorMessage)
        {
            RigidModel model = new RigidModel
            {
                FileType = chunk.ReadFixedLength(4),
                MeshCount = chunk.ReadUInt32(),
                LodCount = chunk.ReadUInt32(),
                BaseSkeleton = chunk.ReadFixedLength(128)
            };

            for (int i = 0; i < model.LodCount; i++)
                model.LodInformations.Add(LodInformation.Create(chunk));

            for (int i = 0; i < model.LodCount; i++)
                model.LodModels.Add(LodModel.Create(chunk));

            Validate(chunk, out errorMessage);

            return model;
        }

    }
}
