﻿using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

namespace VariantMeshEditor.ViewModels
{
    public class SkeletonElement : FileSceneElement
    {

        public SkeletonController Controller { get; set; }
        public AnimationFile SkeletonFile { get; set; }
        public SkeletonModel SkeletonModel { get; set; }
        public SkeletonElement(FileSceneElement parent, string fullPath) : base(parent, "", fullPath, "Skeleton")
        {
           
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.Skeleton;


        public void Create(AnimationPlayer animationPlayer, List<PackFile> loadedContent, ResourceLibary resourceLibary, string skeletonName)
        {
            string animationFolder = "animations\\skeletons\\";
            var skeletonFilePath = animationFolder + skeletonName;
            var file = PackFileLoadHelper.FindFile(loadedContent, skeletonFilePath);
            if (file != null)
            {
                SkeletonFile = AnimationFile.Create(new ByteChunk(file.Data));
                FullPath = skeletonFilePath;
                FileName = Path.GetFileNameWithoutExtension(skeletonFilePath);
            }

            SkeletonModel = new SkeletonModel(resourceLibary.GetEffect(ShaderTypes.Line));
            SkeletonModel.Create(animationPlayer, SkeletonFile);
        }

        protected override void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            SkeletonEditorView view = new SkeletonEditorView();
            SkeletonController controller = new SkeletonController(view, this);
            Editor = view;
        }

        protected override void UpdateNode(GameTime time)
        {
            SkeletonModel.Update(time);
        }

        protected override void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            SkeletonModel.Draw(device, parentTransform, commonShaderParameters);
        }
    }
}
