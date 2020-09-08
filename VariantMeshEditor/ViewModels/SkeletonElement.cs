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
using Viewer.Animation;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
    public class SkeletonElement : RenderableFileSceneElement
    {
        public SkeletonFile SkeletonFile { get; set; }
        public SkeletonModel SkeletonModel { get; set; }
        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, fullPath, "Skeleton")
        {
            Create3dModel();
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;


        public void Create(AnimationPlayer animationPlayer, List<PackFile> loadedContent, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(loadedContent, skeletonFilePath);
            if (file != null)
            {
                SkeletonFile = SkeletonFile.Create(new ByteChunk(file.Data), out string errorMessage);
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
            }

            Refresh3dModel(animationPlayer);
        }

        void Refresh3dModel(AnimationPlayer animationPlayer)
        {
            SkeletonModel = new SkeletonModel();
            SkeletonModel.Create(animationPlayer, SkeletonFile);
            MeshInstance.Model = SkeletonModel;
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
