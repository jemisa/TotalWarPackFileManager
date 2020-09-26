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
        public void Render(Matrix world, GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            if (Visible)
            {

                Model.Render(world, device, effect, effectPass);
            }
        }
    }
}
