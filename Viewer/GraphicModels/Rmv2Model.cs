using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        Dictionary<TexureType, (Texture2D, Material )> _textures = new Dictionary<TexureType, (Texture2D, Material)>();

        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device, RigidModel rigidModelData, int lodLevel, int model, Animation.AnimationClip animationData, int frame = 30)
        {
            _animationPlayer = animationPlayer;
            _model = rigidModelData.LodInformations[lodLevel].LodModels[model];
            _bufferArray = new VertexPositionNormalTexture[_model.VertexArray.Length];
            Create(animationPlayer, device, _bufferArray, _model.IndicesBuffer);

            foreach (var material in _model.Materials)
            {
                _textures[material.Type] = (LoadTexture(material, device), material);
            }
        }

        Texture2D LoadTexture(Material material, GraphicsDevice device)
        {
            if (material.File != null)
            {
                var tex =  LoadTextureAsTexture2d(material, device);

                if(tex != null)
                {
                    var filename = Path.GetFileNameWithoutExtension(material.Name);
                    SaveTexture2d($@"c:\temp\TextureLoading\{filename}_tex2d.png", tex);
                }
                return tex;
            }
            return null;
        }

        Texture2D CreateTexture2dFromBitmap(Bitmap bitmap, GraphicsDevice device)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                Texture2D tex = Texture2D.FromStream(device, memoryStream);
                return tex;
            }
        }

        void SaveTexture2d(string path, Texture2D texture)
        {
            return;
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }


        Texture2D LoadTextureAsTexture2d(Material material, GraphicsDevice device)
        {
            var content = material.File.Data;
            using (MemoryStream stream = new MemoryStream(content))
            {
                var image = Pfim.Dds.Create(stream, new Pfim.PfimConfig(32768, Pfim.TargetFormat.Native, false ));
         
                if (image as Pfim.Dxt1Dds != null)
                {
                    var t = image as Pfim.Dxt1Dds;
                    /*var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    try
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        var bitmap = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format24bppRgb, data);
                        return CreateTexture2dFromBitmap(bitmap, device);
                    }
                    finally
                    {
                        handle.Free();
                    }*/

                    var texture = new Texture2D(device, image.Width, image.Height, false, SurfaceFormat.Dxt1);
                    texture.SetData(image.Data, 0, (int)image.Header.PitchOrLinearSize);
                    return texture;
                }
                else if (image.Format == Pfim.ImageFormat.Rgba32)
                {
                    /*var texture = new Texture2D(device, image.Width, image.Height, false, SurfaceFormat.Dxt1);
                    texture.SetData(image.Data, 0, image.DataLen);
                    return texture;*/
                }
            }
            return null;
        }

        public override void Render(GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            UpdateVertexBuffer();

            var item = _textures[TexureType.Diffuse];
            effect.Parameters["ModelTexture"].SetValue(item.Item1);

            base.Render(device, effect, effectPass);
        }

        private void UpdateVertexBuffer()
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

                Vector3 animatedVertexPos = new Vector3
                {
                    X = vertex.X * transformSum.M11 + vertex.Y * transformSum.M21 + vertex.Z * transformSum.M31 + transformSum.M41,
                    Y = vertex.X * transformSum.M12 + vertex.Y * transformSum.M22 + vertex.Z * transformSum.M32 + transformSum.M42,
                    Z = vertex.X * transformSum.M13 + vertex.Y * transformSum.M23 + vertex.Z * transformSum.M33 + transformSum.M43
                };

                Vector3 animatedNormal = new Vector3
                {
                    X = vertex.Normal_X * transformSum.M11 + vertex.Normal_Y * transformSum.M21 + vertex.Normal_Z * transformSum.M31 + transformSum.M41,
                    Y = vertex.Normal_X * transformSum.M12 + vertex.Normal_Y * transformSum.M22 + vertex.Normal_Z * transformSum.M32 + transformSum.M42,
                    Z = vertex.Normal_X * transformSum.M13 + vertex.Normal_Y * transformSum.M23 + vertex.Normal_Z * transformSum.M33 + transformSum.M43
                };
                animatedNormal.Normalize();
                _bufferArray[index] = new VertexPositionNormalTexture(animatedVertexPos, animatedNormal, new Vector2(vertex.Uv0, vertex.Uv1));
            }


            _vertexBuffer.SetData(_bufferArray);
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

        public override void Render(GraphicsDevice device, Effect effect, EffectPass effectPass)
        {
            foreach (var model in _models)
                model.Render(device, effect, effectPass);
        }
    }
}
