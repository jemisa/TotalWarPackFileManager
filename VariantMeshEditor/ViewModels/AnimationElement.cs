using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Views.Animation;
using Viewer.Animation;

namespace VariantMeshEditor.ViewModels
{
    public class AnimationElement : FileSceneElement
    {
        public AnimationPlayer AnimationPlayer { get; set; } = new AnimationPlayer();

        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
    }
}
