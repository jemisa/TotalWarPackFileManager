using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WpfTest.Scenes.CubeDemoScene;

namespace Viewer.GraphicModels
{
    class Rmv2Model : MeshModel
    {
        public void Create(GraphicsDevice device, RigidModel rigidModelData, int lodLevel, int model, AnimationModel animationModel = null, int frame = 20)
        {
            var lodModel = rigidModelData.LodInformations[lodLevel].LodModels[model];
            var vertices = new VertexPositionNormalTexture[lodModel.IndicesBuffer.Length];

            for (int indecie = 0; indecie < lodModel.IndicesBuffer.Length; indecie++)
            {
                var index = lodModel.IndicesBuffer[indecie];
                var vertex = lodModel.VertexArray[index];

                Vector3 vertexPos = new Vector3(vertex.X, vertex.Y, vertex.Z); ;// Vector3(0,0, 0);
                //Vector3 inputVertexPos = new Vector3(vertex.X, vertex.Y, vertex.Z);

               /* var currentFrame = animationModel.Animation[frame];
                



                for (int i = 0; i < currentFrame.KeyFrames.Count; i++)
                {
                    float weight = 2;
                    var transform = currentFrame.KeyFrames[i];

                    var weightedMatrix = transform.Transform * weight;
                    vertexPos += Vector3.Transform(inputVertexPos, weightedMatrix);
                }*/



                Vector3 normal = new Vector3(vertex.Normal_X, vertex.Normal_Y, vertex.Normal_Z);
                Vector2 textureTopLeft = new Vector2(0.0f, 0.0f);
                vertices[indecie] = new VertexPositionNormalTexture(vertexPos, normal, textureTopLeft);
            }

            Create(device, vertices);
        }
    }

    class Rmv2CompoundModel : MeshModel
    {
        List<MeshModel> _models = new List<MeshModel>();

        public void Create(GraphicsDevice device, RigidModel rigidModelData, int lodLevel)
        {
            for (int i = 0; i < rigidModelData.LodInformations[lodLevel].LodModels.Count(); i++)
            {
                Rmv2Model meshModel = new Rmv2Model();
                meshModel.Create(device, rigidModelData, lodLevel, i);
                _models.Add(meshModel);
            }
        }

        public override void Render(GraphicsDevice device, Effect effect)
        {
            foreach (var model in _models)
                model.Render(device, effect);
        }
    }
}
