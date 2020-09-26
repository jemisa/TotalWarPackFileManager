using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;

namespace Viewer.GraphicModels
{
    public class LineModel : IRenderableContent
    {
        List<VertexPositionNormalTexture[]> _originalVertecies { get; set; } = new List<VertexPositionNormalTexture[]>();

        public void CreateLineList(List<(Vector3, Vector3)> lines)
        {
            foreach (var line in lines)
            {
                var vertices = new[]
                {
                    new VertexPositionNormalTexture(line.Item1, new Vector3(0,0,0), new Vector2(0,0)),
                    new VertexPositionNormalTexture(line.Item2, new Vector3(0,0,0), new Vector2(0,0))
                };
                _originalVertecies.Add(vertices);
            }
        }


        public virtual void Render(GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                foreach (var line in _originalVertecies)
                    device.DrawUserPrimitives(PrimitiveType.LineList, line, 0, 1);
            }
        }

        public void Dispose()
        {

        }

    }
}
