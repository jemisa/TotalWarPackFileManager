using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeViewWithCheckBoxes;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
    public enum FileSceneElementEnum
    {
        Root,
        Transform,
        Animation,
        Skeleton,
        VariantMesh,
        Slots,
        Slot,
        RigidModel,
        WsModel
    }

    public abstract class FileSceneElement : TreeViewDataModel
    {
        public FileSceneElement Parent { get; protected set; }
        public ObservableCollection<FileSceneElement> Children { get; private set; } = new ObservableCollection<FileSceneElement>();

        public abstract FileSceneElementEnum Type { get; }
        public string FileName { get; set; }
        public string FullPath { get; set; }

        public FileSceneElement(FileSceneElement parent, string fileName, string fullPath, string displayName)
            : base(displayName)
        {
            FileName = fileName;
            FullPath = fullPath;
            DisplayName = displayName;
            Parent = parent;
        }

        public override string ToString() => DisplayName;
    }

    public abstract class RenderableFileSceneElement : FileSceneElement
    {
        public RenderableFileSceneElement(FileSceneElement parent, string fullPath, string displayName) :
            base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, displayName)
        { }
        public MeshInstance MeshInstance { get; set; }
    }

    public class RootElement : FileSceneElement
    {
        public RootElement() : base(null, "", "", "Root") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Root;
    }

    public class TransformElement : FileSceneElement
    {
        public TransformElement(FileSceneElement parent) : base(parent, "", "", "Transform") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Transform;
    }

    public class AnimationElement : FileSceneElement
    {
        public AnimationElement(FileSceneElement parent) : base(parent, "", "", "Animation") { }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Animation;
    }

    

    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;
    }

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
    }

    

    public class WsModelElement : FileSceneElement
    {
        public WsModelElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            DisplayName = $"WsModel - {FileName}";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.WsModel;
    }

}
