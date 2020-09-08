using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;
using static WpfTest.Scenes.Scene3d;

namespace Viewer.GraphicModels
{
    public class Rmv2Model : MeshModel
    {
        LodModel _model;
        VertexPositionNormalTexture[] _bufferArray;

        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, RigidModel rigidModelData, int lodLevel, int model, Animation.AnimationClip animationData, int frame = 30)
        {
            _animationPlayer = animationPlayer;
            var lodModel = rigidModelData.LodInformations[lodLevel].LodModels[model];
            
            _model = lodModel;

            _bufferArray = new VertexPositionNormalTexture[_model.VertexArray.Length];
            Create(animationPlayer, device, _bufferArray, lodModel.IndicesBuffer);
        }
       
        public override void Render(GraphicsDevice device, Effect effect)
        {
            for (int index = 0; index < _model.VertexArray.Length; index++)
            {
                var vertex = _model.VertexArray[index];

                var transformSum = Matrix.Identity;
                var animationData = _animationPlayer.GetCurrentFrame();
                if (animationData != null && vertex.BoneInfos.Count != 0)
                {
                    int b0 = vertex.BoneInfos[0].BoneIndex;
                    int b1 = vertex.BoneInfos[1].BoneIndex;
                    int b2 = vertex.BoneInfos[2].BoneIndex;
                    int b3 = vertex.BoneInfos[3].BoneIndex;

                    float w1 = vertex.BoneInfos[0].BoneWeight;
                    float w2 = vertex.BoneInfos[1].BoneWeight;
                    float w3 = vertex.BoneInfos[2].BoneWeight;
                    float w4 = vertex.BoneInfos[3].BoneWeight;

                    Matrix m1 = animationData.BoneTransforms[b0].Transform;
                    Matrix m2 = animationData.BoneTransforms[b1].Transform;
                    Matrix m3 = animationData.BoneTransforms[b2].Transform;
                    Matrix m4 = animationData.BoneTransforms[b3].Transform;
                    transformSum.M11 = (m1.M11 * w1) + (m2.M11 * w2) + (m3.M11 * w3) + (m4.M11 * w4);
                    transformSum.M12 = (m1.M12 * w1) + (m2.M12 * w2) + (m3.M12 * w3) + (m4.M12 * w4);
                    transformSum.M13 = (m1.M13 * w1) + (m2.M13 * w2) + (m3.M13 * w3) + (m4.M13 * w4);
                    transformSum.M21 = (m1.M21 * w1) + (m2.M21 * w2) + (m3.M21 * w3) + (m4.M21 * w4);
                    transformSum.M22 = (m1.M22 * w1) + (m2.M22 * w2) + (m3.M22 * w3) + (m4.M22 * w4);
                    transformSum.M23 = (m1.M23 * w1) + (m2.M23 * w2) + (m3.M23 * w3) + (m4.M23 * w4);
                    transformSum.M31 = (m1.M31 * w1) + (m2.M31 * w2) + (m3.M31 * w3) + (m4.M31 * w4);
                    transformSum.M32 = (m1.M32 * w1) + (m2.M32 * w2) + (m3.M32 * w3) + (m4.M32 * w4);
                    transformSum.M33 = (m1.M33 * w1) + (m2.M33 * w2) + (m3.M33 * w3) + (m4.M33 * w4);
                    transformSum.M41 = (m1.M41 * w1) + (m2.M41 * w2) + (m3.M41 * w3) + (m4.M41 * w4);
                    transformSum.M42 = (m1.M42 * w1) + (m2.M42 * w2) + (m3.M42 * w3) + (m4.M42 * w4);
                    transformSum.M43 = (m1.M43 * w1) + (m2.M43 * w2) + (m3.M43 * w3) + (m4.M43 * w4);
                }

                Vector3 animatedVertexPos = new Vector3();
                animatedVertexPos.X = vertex.X * transformSum.M11 + vertex.Y * transformSum.M21 + vertex.Z * transformSum.M31 + transformSum.M41;
                animatedVertexPos.Y = vertex.X * transformSum.M12 + vertex.Y * transformSum.M22 + vertex.Z * transformSum.M32 + transformSum.M42;
                animatedVertexPos.Z = vertex.X * transformSum.M13 + vertex.Y * transformSum.M23 + vertex.Z * transformSum.M33 + transformSum.M43;

                Vector3 animatedNormal = new Vector3();
                animatedNormal.X = vertex.Normal_X * transformSum.M11 + vertex.Normal_Y * transformSum.M21 + vertex.Normal_Z * transformSum.M31 + transformSum.M41;
                animatedNormal.Y = vertex.Normal_X * transformSum.M12 + vertex.Normal_Y * transformSum.M22 + vertex.Normal_Z * transformSum.M32 + transformSum.M42;
                animatedNormal.Z = vertex.Normal_X * transformSum.M13 + vertex.Normal_Y * transformSum.M23 + vertex.Normal_Z * transformSum.M33 + transformSum.M43;
                animatedNormal.Normalize();
                _bufferArray[index] = new VertexPositionNormalTexture(animatedVertexPos, animatedNormal, new Vector2(0.0f, 0.0f));
            }


            _vertexBuffer.SetData(_bufferArray);
            base.Render(device, effect);
        }
    }

    public class Rmv2CompoundModel : MeshModel
    {
        List<MeshModel> _models = new List<MeshModel>();

        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, RigidModel rigidModelData, Animation.AnimationClip animationModel, int lodLevel, int frame)
        {
            for (int i = 0 ; i < rigidModelData.LodInformations[lodLevel].LodModels.Count(); i++)
            {
                Rmv2Model meshModel = new Rmv2Model();
                meshModel.Create(animationPlayer, device, rigidModelData, lodLevel, i, animationModel, frame);
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
