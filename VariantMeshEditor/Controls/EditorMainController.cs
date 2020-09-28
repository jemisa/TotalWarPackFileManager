using Common;
using Filetypes.ByteParsing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VariantMeshEditor.Controls.EditorControllers;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using Viewer.GraphicModels;
using Viewer.Scene;
using WpfTest.Scenes;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class EditorMainController
    {
        FileSceneElement _rootElement;
        SceneTreeViewController _treeViewController;
        Scene3d _scene3d;
        Panel _editorPanel;
        ResourceLibary _resourceLibary;
        List<PackFile> _temp_loadedContent;

        public EditorMainController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel editorPanel)
        {


            byte[] data = File.ReadAllBytes(@"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\Skeleton\hu1_sws_stand_01.anm.meta");
            var chunk = new ByteChunk(data);

            var header = chunk.ReadBytes(10);
            var label = chunk.ReadFixedLength(10);
            //var dataContent = chunk.ReadBytes(28);
            var removed = chunk.ReadBytes(17);
            var f0 = chunk.ReadSingle();
            var f1 = chunk.ReadSingle();
            var f2 = chunk.ReadSingle();
            var f3 = chunk.ReadSingle();

            var label2 = chunk.ReadFixedLength(10);








            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _editorPanel = editorPanel;

            _temp_loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
            _resourceLibary = new ResourceLibary(_temp_loadedContent);


            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvent;
            _scene3d.LoadScene += Create3dWorld;
        }

        public void LoadModel(string path)
        {

        }

        private void _treeViewController_VisabilityChangedEvent(FileSceneElement element, bool isVisible)
        {
            if (element is RigidModelElement rigidModelElement)
            {
                rigidModelElement.Controller.SetVisible(0, isVisible);
            }

            if (element is SkeletonElement skeletonElement)
            {
                //skeletonElement.Controller.SetVisible(0, isVisible);
            }
        }

        private void _treeViewController_SceneElementSelectedEvent(FileSceneElement element)
        {
            _editorPanel.Children.Clear();
            if(element.Editor != null)
                _editorPanel.Children.Add(element.Editor);
        }



        public void CreateEditors(FileSceneElement root)
        {
            EditorBuilder builder = new EditorBuilder(_temp_loadedContent, _treeViewController);
            builder.Build(root);
        }


        void Create3dWorld(GraphicsDevice device)
        {
            _scene3d.SetResourceLibary(_resourceLibary);

            SceneLoader sceneLoader = new SceneLoader(_temp_loadedContent, _resourceLibary);
            _rootElement = sceneLoader.Load(null, null, "variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");

            
            _treeViewController.Populate(_rootElement);

            CreateEditors(_rootElement);

            Create3dModels(device, _rootElement);
            ResovleAttachment(_rootElement);
            RegisterAnimations(_rootElement);
            _treeViewController.SetInitialVisability(_rootElement, true);
        }

        public void ResovleAttachment(FileSceneElement element)
        {

            if (element is SlotElement rigidModelElement)
            {

                var skeleton = _treeViewController.GetAllOfTypeInSameVariantMesh<SkeletonElement>(element);

                if (!string.IsNullOrWhiteSpace(rigidModelElement.AttachmentPoint))
                {
                    var models = new List<RigidModelElement>();
                    GetAllChildrenOfType<RigidModelElement>(element, models);

                    foreach (var model in models)
                    {
                        foreach (var mesh in model.MeshInstances)
                        {
                            foreach (var childmesh in mesh)
                            {
                                childmesh.AttachmentResolver = new AttachmentResolver(rigidModelElement.AttachmentPoint, skeleton.First().SkeletonModel);
                            }
                        }

                    }

                }
            }

            foreach (var child in element.Children)
            {
                ResovleAttachment(child);
            }
        }

        void GetAllChildrenOfType<T>(FileSceneElement element, List<T> output) where T : FileSceneElement
        {
            if (element as T != null)
            {
                output.Add(element as T);
            }

            foreach (var child in element.Children)
            {
                GetAllChildrenOfType(child, output);
            }
        }

        void Create3dModels(GraphicsDevice device, FileSceneElement element)
        {
            if (element is RigidModelElement rigidModelElement)
            {
                var rigidModelData = rigidModelElement.Model;
                var controller = rigidModelElement.Controller;

                for (int lodIndex = 0; lodIndex < rigidModelData.LodInformations.Count; lodIndex++)
                {
                    rigidModelElement.MeshInstances.Add(new List<MeshRenderItem>());

                    for (int modelIndex = 0; modelIndex < rigidModelData.LodInformations[lodIndex].LodModels.Count(); modelIndex++)
                    {
                        var animation = _treeViewController.GetAllOfTypeInSameVariantMesh<AnimationElement>(element);

                        Rmv2Model meshModel = new Rmv2Model();
                        meshModel.Create(animation.First().AnimationPlayer, device, rigidModelData, lodIndex, modelIndex);

                        MeshRenderItem meshRenderItem = new MeshRenderItem(meshModel, _resourceLibary.GetEffect(ShaderTypes.Mesh));
                        meshRenderItem.Visible = lodIndex == 0;

                        rigidModelElement.MeshInstances[lodIndex].Add(meshRenderItem);
                        controller.AssignModel(meshRenderItem, lodIndex, modelIndex);
                        _scene3d.DrawBuffer.Add(meshRenderItem);

                        // Resolve the textures
                        //controller.resolveTexture();
                        meshModel.ResolveTextures(_resourceLibary, device);
                    }
                }
            }

            if (element is SkeletonElement skeletonElement)
            {
                _scene3d.DrawBuffer.Add(skeletonElement.SkeletonModel);
            }

            foreach (var child in element.Children)
            {
                Create3dModels(device, child);
            }
        }

        void RegisterAnimations(FileSceneElement scene)
        {
            if (scene as AnimationElement != null)
                _scene3d.AnimationPlayers.Add((scene as AnimationElement).AnimationPlayer);

            foreach (var item in scene.Children)
                RegisterAnimations(item);
        }

    }

    class EditorBuilder
    {
        List<PackFile> _loadedContent;
        SceneTreeViewController _treeViewController;
        public EditorBuilder(List<PackFile> loadedContent, SceneTreeViewController treeViewController)
        {
            _loadedContent = loadedContent;
            _treeViewController = treeViewController;
        }

        public void Build(FileSceneElement rootItem)
        {
            CreateEditor(rootItem);
            foreach (var child in rootItem.Children)
                Build(child);
        }


        void CreateEditor(FileSceneElement element)
        {
            switch (element.Type)
            {
                case FileSceneElementEnum.Skeleton:
                    CreateSkeletonEditor(element as SkeletonElement);
                    break;

                case FileSceneElementEnum.Animation:
                    CreateAnimationEditor(element as AnimationElement);
                    break;

                case FileSceneElementEnum.Slot:
                    CreateSlotEditor(element as SlotElement);
                    break;

                case FileSceneElementEnum.RigidModel:
                    CreateRigidModelEditor(element as RigidModelElement);
                    break;

            }
        }

        void CreateSkeletonEditor(SkeletonElement element)
        {
            SkeletonEditorView view = new SkeletonEditorView();
            SkeletonController controller = new SkeletonController(view, element);
            element.Editor = view;
        }

        void CreateAnimationEditor(AnimationElement element)
        {
            var skeleton = _treeViewController.GetAllOfTypeInSameVariantMesh<SkeletonElement>(element);
            if (skeleton.Count == 1)
            {
                AnimationEditorView view = new AnimationEditorView();
                AnimationController controller = new AnimationController(view, _loadedContent, element, skeleton.First());
                element.Editor = view;
            }
        }

        void CreateSlotEditor(SlotElement element)
        {
            SlotEditorView view = new SlotEditorView();
            SlotController controller = new SlotController(view, element);
            element.Editor = view;
        }

        void CreateRigidModelEditor(RigidModelElement element)
        {
            RigidModelEditorView view = new RigidModelEditorView();
            
            var controller = new RigidModelController(view, element, _loadedContent);
            element.Editor = view;
            element.Controller = controller;
        }
    }
}
