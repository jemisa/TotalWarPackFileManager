using Filetypes.RigidModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override FileSceneElementEnum Type => FileSceneElementEnum.RigidModel;
    }
}
