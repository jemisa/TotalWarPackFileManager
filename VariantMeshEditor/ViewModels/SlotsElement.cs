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

            SetDisplayName(AttachmentPoint);
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;

        SlotController _controller;
        SkeletonElement _skeleton;
        void SetDisplayName(string attachmentPointName)
        {
            DisplayName = $"Slot -{SlotName} - {attachmentPointName}";
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            _skeleton = SceneElementHelper.GetAllOfTypeInSameVariantMesh<SkeletonElement>(this).FirstOrDefault();
            SlotEditorView view = new SlotEditorView();
            _controller = new SlotController(view, this, _skeleton);
            Editor = view;
        }

        protected override void UpdateNode(GameTime time)
        {
            if (_controller.AttachmentBoneIndex != -1)
            {
                WorldTransform = _skeleton.SkeletonModel.GetAnimatedBone(_controller.AttachmentBoneIndex);
                SetDisplayName(_skeleton.SkeletonModel.Bones[_controller.AttachmentBoneIndex].Name);
            }
            else
            {
                WorldTransform = Matrix.Identity;
                SetDisplayName("");
            }
        }
    }
}
