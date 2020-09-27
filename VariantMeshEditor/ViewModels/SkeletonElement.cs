﻿using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using Viewer.Animation;
using Viewer.GraphicModels;

namespace VariantMeshEditor.ViewModels
{
    public class SkeletonElement : RenderableFileSceneElement
    {

        public SkeletonController Controller { get; set; }
        public SkeletonFile SkeletonFile { get; set; }
        public SkeletonModel SkeletonModel { get; set; }
        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, fullPath, "Skeleton")
        {
           
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;


        public MeshInstance Create(AnimationPlayer animationPlayer, List<PackFile> loadedContent, string skeletonName)
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

            SkeletonModel = new SkeletonModel();
            SkeletonModel.Create(animationPlayer, SkeletonFile);
            MeshInstance.Model = SkeletonModel;
            return MeshInstance;
        }


        
    }
}
