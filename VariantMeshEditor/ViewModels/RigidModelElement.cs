using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using Viewer.Animation;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
    public class RigidModelElement : RenderableFileSceneElement
    {
        public RigidModel Model { get; set; }
        public RigidModelController Controller { get; set; }

        public List<List<MeshInstance>> MeshInstances { get; set; } = new List<List<MeshInstance>>();

        public RigidModelElement(FileSceneElement parent, RigidModel model, string fullPath) : base(parent, fullPath, "")
        {
            Model = model;
            DisplayName = $"RigidModel - {FileName}";
        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;
    }
}
