﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewer.GraphicModels
{
    class MeshModel : IDisposable
    {
        VertexDeclaration _vertexDeclaration;
        VertexBuffer _vertexBuffer;

        public void Create(GraphicsDevice device, VertexPositionNormalTexture[] vertexMesh)
        {
            _vertexDeclaration = new VertexDeclaration(
               new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
               new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
               new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
           );

            _vertexBuffer = new VertexBuffer(device, _vertexDeclaration, vertexMesh.Length, BufferUsage.None);
            _vertexBuffer.SetData(vertexMesh);
        }

        public virtual void Render(GraphicsDevice device, Effect effect)
        {
            device.SetVertexBuffer(_vertexBuffer);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount);
            }
        }

        public void Dispose()
        {
            if(_vertexDeclaration != null)
                _vertexDeclaration.Dispose();
        }
    }
}
