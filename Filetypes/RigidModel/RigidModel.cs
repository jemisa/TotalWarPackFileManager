using Common;
using Filetypes.ByteParsing;
using System;
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
       

        static bool Validate(ByteChunk chunk, out string errorMessage)
        {
            if (chunk.BytesLeft != 0)
                throw new Exception("Data left!");
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
                BaseSkeleton = Util.SanatizeFixedString(chunk.ReadFixedLength(128))
            };

            for (int i = 0; i < model.LodCount; i++)
                model.LodInformations.Add(LodInformation.Create(chunk));

            for (int i = 0; i < model.LodCount; i++)
                for(int j = 0; j < model.LodInformations[i].GroupsCount; j++)
                    model.LodInformations[i].LodModels.Add(LodModel.Create(chunk));

            Validate(chunk, out errorMessage);

            return model;
        }

        public void ResolveTextures(List<PackFile> loadedContent)
        {

            foreach (var lodLevel in LodInformations)
            {
                foreach (var model in lodLevel.LodModels)
                {
                    model.ResolveTextures(loadedContent);
                }
            }
        }

    }
}
