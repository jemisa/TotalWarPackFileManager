using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class RigidModelElement : FileSceneElement
    {
        public RigidModel Model { get; set; }
        public RigidModelController Controller { get; set; }

        public List<List<MeshRenderItem>> MeshInstances { get; set; } = new List<List<MeshRenderItem>>();

        public RigidModelElement(FileSceneElement parent, RigidModel model, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            Model = model;
            DisplayName = $"RigidModel - {FileName}";
        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            RigidModelEditorView view = new RigidModelEditorView();

            var controller = new RigidModelController(view, this);
            Editor = view;
            Controller = controller;

            Create3dModels(virtualWorld, resourceLibary);
        }

        void Create3dModels(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            for (int lodIndex = 0; lodIndex < Model.LodInformations.Count; lodIndex++)
            {
                MeshInstances.Add(new List<MeshRenderItem>());

                for (int modelIndex = 0; modelIndex < Model.LodInformations[lodIndex].LodModels.Count(); modelIndex++)
                {
                    var animation = SceneElementHelper.GetAllOfTypeInSameVariantMesh<AnimationElement>(this).FirstOrDefault();

                    Rmv2Model meshModel = new Rmv2Model();
                    meshModel.Create(animation?.AnimationPlayer, virtualWorld.GraphicsDevice, Model, lodIndex, modelIndex);

                    MeshRenderItem meshRenderItem = new MeshRenderItem(meshModel, resourceLibary.GetEffect(ShaderTypes.Mesh));
                    meshRenderItem.Visible = lodIndex == 0;

                    MeshInstances[lodIndex].Add(meshRenderItem);
                    Controller.AssignModel(meshRenderItem, lodIndex, modelIndex);

                    // Resolve the textures
                    //controller.resolveTexture();
                    meshModel.ResolveTextures(resourceLibary, virtualWorld.GraphicsDevice);
                }
            }
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            foreach (var item in MeshInstances)
            {
                foreach (var item2 in item)
                    item2.Draw(device, parentTransform, commonShaderParameters);
            }
        }
    }
}
