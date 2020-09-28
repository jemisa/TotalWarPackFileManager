using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Filetypes.RigidModel.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TreeViewWithCheckBoxes;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;

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




    public abstract class FileSceneElement : TreeViewDataModel, ISceneGraphNode
    {
        public FileSceneElement Parent { get; protected set; }
        public ObservableCollection<FileSceneElement> Children { get; private set; } = new ObservableCollection<FileSceneElement>();

        public abstract FileSceneElementEnum Type { get; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public UserControl Editor { get; set; }

        public FileSceneElement(FileSceneElement parent, string fileName, string fullPath, string displayName)
            : base(displayName)
        {
            FileName = fileName;
            FullPath = fullPath;
            DisplayName = displayName;
            Parent = parent;
        }

        public override string ToString() => DisplayName;



        public void CreateContent(Scene3d virtualWorld, ResourceLibary resourceLibary)
        {
            CreateEditor(virtualWorld, resourceLibary);
            foreach (var child in Children)
            {
                child.CreateContent(virtualWorld, resourceLibary);
            }
        }

        protected virtual void CreateEditor(Scene3d virtualWorld, ResourceLibary resourceLibary)
        { }


        public Matrix WorldTransform { get; set; } = Matrix.Identity;

        public void Render(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {
            DrawNode(device, parentTransform, commonShaderParameters);
            var newWorld = parentTransform * WorldTransform;
            foreach (var child in Children)
            {
                child.Render(device, newWorld, commonShaderParameters);
            }
        }

        virtual public void Update(GameTime time)
        {
            UpdateNode(time);
            foreach (var child in Children)
                child.Update(time);
        }

        virtual protected void UpdateNode(GameTime time)
        { }

        virtual protected void DrawNode(GraphicsDevice device, Matrix parentTransform, CommonShaderParameters commonShaderParameters)
        {}
    }

    public abstract class RenderableFileSceneElement : FileSceneElement
    {
        public RenderableFileSceneElement(FileSceneElement parent, string fullPath, string displayName) :
            base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, displayName)
        { }
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


    public class VariantMeshElement : FileSceneElement
    {
        public VariantMeshElement(FileSceneElement parent, string fullPath) : base(parent, Path.GetFileNameWithoutExtension(fullPath), fullPath, "VariantMesh")
        {
            DisplayName = "VariantMesh - " + FileName;
        }
        public override FileSceneElementEnum Type => FileSceneElementEnum.VariantMesh;
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
