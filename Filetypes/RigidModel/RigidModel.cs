﻿using Common;
using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Filetypes.RigidModel
{
    public class RigidModel
    {
        public string FileType { get; set; }
        public uint Version { get; set; }
        public uint LodCount { get; set; }
        public string BaseSkeleton { get; set; }
        public List<LodHeader> LodHeaders = new List<LodHeader>();
       

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
                Version = chunk.ReadUInt32(),
                LodCount = chunk.ReadUInt32(),
                BaseSkeleton = Util.SanatizeFixedString(chunk.ReadFixedLength(128))
            };

            for (int i = 0; i < model.LodCount; i++)
                model.LodHeaders.Add(LodHeader.Create(chunk));

            for (int i = 0; i < model.LodCount; i++)
                for(int j = 0; j < model.LodHeaders[i].GroupsCount; j++)
                    model.LodHeaders[i].LodModels.Add(LodModel.Create(chunk));
           
            Validate(chunk, out errorMessage);
           
            return model;
        }

    }
}
