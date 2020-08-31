using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.GraphicModels
{
    class MeshInstance
    {
        public Matrix World { get; set; } = Matrix.Identity;
        public MeshModel Model { get; set; }

        public void Render(GraphicsDevice device, BasicEffect effect)
        {
            effect.World = World;
            Model.Render(device, effect);
        }
    }
}
