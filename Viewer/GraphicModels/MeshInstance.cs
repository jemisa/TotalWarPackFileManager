using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.GraphicModels
{
    public class MeshInstance
    {
        public Matrix World { get; set; } = Matrix.Identity;
        public IRenderableContent Model { get; set; }
        public bool Visible { get; set; } = true;
        public void Render(GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            if (Visible)
            {
                //effect.World = World;
                Model.Render(device, effect, effectPass);
            }
        }
    }
}
