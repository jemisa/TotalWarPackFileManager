using Filetypes.RigidModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.Animation;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
    public class RigidModelElement : RenderableFileSceneElement
    {
        public RigidModel Model { get; set; }


        public RigidModelElement(FileSceneElement parent, RigidModel model, string fullPath) : base(parent, fullPath, "")
        {
            Model = model;
            DisplayName = $"RigidModel - {FileName}";
        }


        public void Create(AnimationPlayer animationPlayer, GraphicsDevice device)
        {
            Rmv2CompoundModel model3d = new Rmv2CompoundModel();
            model3d.Create(animationPlayer, device, Model, null, 0, 0);

            MeshInstance = new MeshInstance()
            {
                Model = model3d,
                World = Matrix.Identity,
            };
        }

        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;
    }
}
