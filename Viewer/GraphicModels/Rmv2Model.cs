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
        public void Create(GraphicsDevice device, RigidModel rigidModelData, int lodLevel, int model, AnimationInformation animationData, int frame = 30)
        {
            var lodModel = rigidModelData.LodInformations[lodLevel].LodModels[model];
            var vertices = new VertexPositionNormalTexture[lodModel.IndicesBuffer.Length];
            var currentFrame = animationData.Animation[frame];

            for (int index = 0; index < lodModel.IndicesBuffer.Length; index++)
            {
                var vertIndex = lodModel.IndicesBuffer[index];
                var vertex = lodModel.VertexArray[vertIndex];
                
                var combinesTransformationMatrix = Matrix.Identity;

               foreach (var boneActingOnVertex in vertex.BoneInfos)
               {
                   var boneTransform = currentFrame.BoneTransforms[boneActingOnVertex.BoneIndex];
                   var weightedMatrix = boneTransform.Transform * boneActingOnVertex.BoneWeight;
                   combinesTransformationMatrix += weightedMatrix;
               }
               //combinesTransformationMatrix.Translation = new Vector3(0,0,0);
               //combinesTransformationMatrix.M44 = 1;

                Vector3 inputVertexPos = new Vector3(vertex.X, vertex.Y, vertex.Z);
                Vector3 animatedVertexPos = Vector3.Transform(inputVertexPos, combinesTransformationMatrix);

                Vector3 normal = new Vector3(vertex.Normal_X, vertex.Normal_Y, vertex.Normal_Z);
                vertices[index] = new VertexPositionNormalTexture(animatedVertexPos, normal, new Vector2(0.0f, 0.0f));
            }

            Create(device, vertices);
        }
    }

    class Rmv2CompoundModel : MeshModel
    {
        List<MeshModel> _models = new List<MeshModel>();

        public void Create(GraphicsDevice device, RigidModel rigidModelData, AnimationInformation animationModel, int lodLevel, int frame)
        {
            for (int i = 1; i < rigidModelData.LodInformations[lodLevel].LodModels.Count(); i++)
            {
                Rmv2Model meshModel = new Rmv2Model();
                meshModel.Create(device, rigidModelData, lodLevel, i, animationModel, frame);
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
