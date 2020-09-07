using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using TreeViewWithCheckBoxes;
using Viewer.GraphicModels;
using static Filetypes.RigidModel.VariantMeshDefinition;

namespace VariantMeshEditor.Util
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

    public abstract class FileSceneElement
    {
        public TreeViewDataModel TreeNode { get; set; }
        public FileSceneElement Parent { get; set; }
        public List<FileSceneElement> Children { get; set; } = new List<FileSceneElement>();
        public abstract FileSceneElementEnum Type { get; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string DisplayName { get; set; }
        public FileSceneElement(FileSceneElement parent, string fileName, string fullPath, string displayName)
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
            base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, displayName) { }
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

    public class SkeletonElement : RenderableFileSceneElement
    {
        public Skeleton Skeleton { get; set; }
        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, fullPath, "Skeleton") 
        {
            Create3dModel();
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;


        public void Create(List<PackFile> loadedContent, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(loadedContent, skeletonFilePath);
            if (file != null)
            {
                Skeleton = Skeleton.Create(new ByteChunk(file.Data), out string errorMessage);
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
            }

            Refresh3dModel();
        }

        void Refresh3dModel()
        {
            SkeletonModel skeletonModel = new SkeletonModel();
            skeletonModel.Create(Skeleton);
            MeshInstance.Model = skeletonModel;
        }

        void Create3dModel()
        {
            MeshInstance = new MeshInstance()
            {
                Model = null,
                World = Matrix.Identity,
                Visible = true
            };
        }
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

        public SlotElement(FileSceneElement parent, string slotName, string attachmentPoint) : base(parent, "","", "") 
        {
            SlotName = slotName;
            AttachmentPoint = attachmentPoint;
            DisplayName = $"Slot -{SlotName} - {AttachmentPoint}";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Slot;
    }

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

    public class WsModelElement : FileSceneElement
    {
        public WsModelElement(FileSceneElement parent, string fullPath) : base(parent,Path.GetFileNameWithoutExtension(fullPath), fullPath, "")
        {
            DisplayName = $"WsModel - {FileName}";
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.WsModel;
    }


    class SceneLoader
    {
        public SceneLoader(List<PackFile> loadedContent)
        {
            _loadedContent = loadedContent;
        }
        List<PackFile> _loadedContent;
        public FileSceneElement Load(string filePath, FileSceneElement parent = null)
        {
            if(parent == null)
                parent = new RootElement();

            var file = PackFileLoadHelper.FindFile(_loadedContent, filePath);
            switch (file.FileExtention)
            {
                case "variantmeshdefinition":
                    LoadVariantMesh(file, parent);
                    break;

                case "rigid_model_v2":
                    LoadRigidMesh(file, parent);
                    break;

                case "wsmodel":
                    LoadWsModel(file, parent);
                    break;
            }

            return parent;
        }

        void LoadVariantMesh(PackedFile file, FileSceneElement parent)
        {
            var variantMeshElement = new VariantMeshElement(parent,file.Name);
            parent.Children.Add(variantMeshElement);

            variantMeshElement.Children.Add(new TransformElement(variantMeshElement));
            variantMeshElement.Children.Add(new AnimationElement(variantMeshElement));
            var skeletonElement = new SkeletonElement(variantMeshElement, "");
            variantMeshElement.Children.Add(skeletonElement);

            var slotsElement = new SlotsElement(variantMeshElement);
            variantMeshElement.Children.Add(slotsElement);

            var content = file.Data;
            var fileContent = Encoding.Default.GetString(content);
            VariantMeshFile meshFile = VariantMeshDefinition.Create(fileContent);

            foreach (var slot in meshFile.VARIANT_MESH.SLOT)
            {
                var slotElement = new SlotElement(slotsElement, slot.Name, slot.AttachPoint);
                slotsElement.Children.Add(slotElement);

                foreach (var mesh in slot.VariantMeshes)
                    Load(mesh.Name, slotElement);

                foreach (var meshReference in slot.VariantMeshReferences)
                    Load(meshReference.definition, slotElement);
            }

            // Load the animation
            var rigidModels = new List<RigidModelElement>();
            GetAllOfType<RigidModelElement>(parent, ref rigidModels);
            var skeletons = rigidModels
                .Where(x=>!string.IsNullOrEmpty(x.Model.BaseSkeleton))
                .Select(x => x.Model.BaseSkeleton)
                .Distinct();

            if (skeletons.Count() > 1)
                throw new Exception("More the one skeleton for a veriant mesh");
            if (skeletons.Count() == 1)
            {
                skeletonElement.Create(_loadedContent, skeletons.First() + ".anim");
            }
            else
            {
                variantMeshElement.Children.Remove(skeletonElement);
            }
        }

        void LoadRigidMesh(PackedFile file, FileSceneElement parent)
        {
            ByteChunk chunk = new ByteChunk(file.Data);
            var model3d = RigidModel.Create(chunk, out string errorMessage);
            var model = new RigidModelElement(parent,model3d, file.FullPath);
            parent.Children.Add(model);
        }


        void LoadWsModel(PackedFile file, FileSceneElement parent)
        {
            var model = new WsModelElement(parent,file.FullPath);
            parent.Children.Add(model);
        }

        void GetAllOfType<T>(FileSceneElement variantMeshParent, ref List<T> out_items) where T : FileSceneElement
        {
            if (variantMeshParent as T != null)
                out_items.Add(variantMeshParent as T);

            foreach (var child in variantMeshParent.Children)
            {
                if (variantMeshParent as T != null)
                    out_items.Add(variantMeshParent as T);

                GetAllOfType<T>(child, ref out_items);
            }
        }
    }
}
