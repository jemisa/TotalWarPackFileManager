using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class SlotsElement : FileSceneElement
    {
        public SlotsElement(FileSceneElement parent) : base(parent, "", "", "Slots") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slots;
    }

    public class SlotElement : FileSceneElement
    {
        public string SlotName { get; set; }
        public string AttachmentPoint { get; set; }

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "", "", "")
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            DisplayName = $"Slot -{SlotName} - {AttachmentPoint}";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        AttachmentResolver _attachmentResolver;

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            SlotEditorView view = new SlotEditorView();
            SlotController controller = new SlotController(view, this);
            Editor = view;

            var skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this);
            if (!string.IsNullOrWhiteSpace(AttachmentPoint))
            {
                _attachmentResolver = new AttachmentResolver(AttachmentPoint, skeleton.First().SkeletonModel);
            }
        }

        protected override void UpdateNode(GameTime time)
        {
            if (_attachmentResolver != null)
                WorldTransform = _attachmentResolver.GetWorldMatrix();
            else
                WorldTransform = Matrix.Identity;
        }
    }
}
