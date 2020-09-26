using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;

namespace Viewer.GraphicModels
{
    public interface IRenderableContent : IDisposable
    {
        void Render(Matrix world, GraphicsDevice device, Effect effect, EffectPass effectPass);
    }


    public class MeshModel : IRenderableContent
    {
        public VertexDeclaration _vertexDeclaration;
        public VertexBuffer _vertexBuffer;
        public AnimationPlayer _animationPlayer;
        public IndexBuffer _indexBuffer;

        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, VertexPositionNormalTexture[] vertexMesh, ushort[] indices)
        {
            _animationPlayer = animationPlayer;

            _indexBuffer = new IndexBuffer(device, typeof(short), indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);

            _vertexDeclaration = VertexPositionNormalTexture.VertexDeclaration;


            _vertexBuffer = new VertexBuffer(device, _vertexDeclaration, vertexMesh.Length, BufferUsage.None);
            _vertexBuffer.SetData(vertexMesh);
        }

        public virtual void Render(Matrix world, GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            device.Indices = _indexBuffer;
            device.SetVertexBuffer(_vertexBuffer);
            //foreach (var pass in effect.CurrentTechnique.Passes)
            //{

            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
            //  pass.Apply();

            effectPass.Apply();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indexBuffer.IndexCount);
            //}
        }

        public void Dispose()
        {
            if(_vertexDeclaration != null)
                _vertexDeclaration.Dispose();
        }

    }
}
