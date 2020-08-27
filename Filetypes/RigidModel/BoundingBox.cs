using Filetypes.ByteParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filetypes.RigidModel
{
    public class BoundingBox
    {
        public float MinimumX { get; set; }
        public float MinimumY { get; set; }
        public float MinimumZ { get; set; }
        public float MaximumX { get; set; }
        public float MaximumY { get; set; }
        public float MaximumZ { get; set; }
        public static BoundingBox Create(ByteChunk chunk)
        {
            return new BoundingBox()
            {
                MinimumX = chunk.ReadSingle(),
                MinimumY = chunk.ReadSingle(),
                MinimumZ = chunk.ReadSingle(),
                MaximumX = chunk.ReadSingle(),
                MaximumY = chunk.ReadSingle(),
                MaximumZ = chunk.ReadSingle(),
            };
        }

        public override string ToString()
        {
            return $"x(min:{MinimumX} max:{MaximumX}) y(min:{MinimumY} max:{MaximumY}) z(min:{MinimumZ} max:{MaximumZ})";
        }
    }
}
