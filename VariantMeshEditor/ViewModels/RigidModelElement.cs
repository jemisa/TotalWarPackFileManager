using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        RigidModelController Controller { get; set; }

        public override UserControl EditorViewModel { get => Controller.GetView(); protected set => throw new System.Exception(); }

        public RigidModelElement(FileSceneElement parent, RigidModel model, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            Model = model;
            DisplayName = $"RigidModel - {FileName}";
        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {

            Controller = new RigidModelController(this, resourceLibary, virtualWorld);
        }


        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            Controller.DrawNode(device, parentTransform, commonShaderParameters);
        }
    }
}
