using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
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
}
