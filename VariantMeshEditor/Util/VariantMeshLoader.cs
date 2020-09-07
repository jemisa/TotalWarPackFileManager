﻿using Common;
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
using System.Windows.Navigation;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.ViewModels;
using Viewer.GraphicModels;
using static Filetypes.RigidModel.VariantMeshDefinition;

namespace VariantMeshEditor.Util
{
    

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
                skeletonElement.Create(_loadedContent, skeletons.First() + ".anim");
            else
                variantMeshElement.Children.Remove(skeletonElement);
        }

        void LoadRigidMesh(PackedFile file, FileSceneElement parent)
        {
            ByteChunk chunk = new ByteChunk(file.Data);
            var model3d = RigidModel.Create(chunk, out string errorMessage);
            var model = new RigidModelElement(parent, model3d, file.FullPath);
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
