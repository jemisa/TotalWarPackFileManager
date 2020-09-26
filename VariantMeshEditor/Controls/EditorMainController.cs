using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
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
        TextureLibary _textureLibary;
        List<PackFile> _temp_loadedContent;

        public EditorMainController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel editorPanel)
        {
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _editorPanel = editorPanel;

            _temp_loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
            _textureLibary = new TextureLibary(_temp_loadedContent);


            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvent;
            _scene3d.LoadScene += Create3dWorld;
        }

        public void LoadModel(string path)
        {
            SceneLoader sceneLoader = new SceneLoader(_temp_loadedContent);
            _rootElement = sceneLoader.Load(null, null, "variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");

            _treeViewController.Populate(_rootElement);

            CreateEditors(_rootElement);
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
            Create3dModels(device, _rootElement);
            RegisterAnimations(_rootElement);
            _treeViewController.SetInitialVisability(_rootElement, true);
        }

        void Create3dModels(GraphicsDevice device, FileSceneElement element)
        {
            if (element is RigidModelElement rigidModelElement)
            {
                var rigidModelData = rigidModelElement.Model;
                var controller = rigidModelElement.Controller;

                for (int lodIndex = 0; lodIndex < rigidModelData.LodInformations.Count; lodIndex++)
                {
                    rigidModelElement.MeshInstances.Add(new List<MeshInstance>());

                    for (int modelIndex = 0; modelIndex < rigidModelData.LodInformations[lodIndex].LodModels.Count(); modelIndex++)
                    {
                        Rmv2Model meshModel = new Rmv2Model();
                        meshModel.Create(null, device, rigidModelData, lodIndex, modelIndex);
                        MeshInstance instance = new MeshInstance()
                        {
                            Model = meshModel,
                            Visible = lodIndex == 0
                        };

                        rigidModelElement.MeshInstances[lodIndex].Add(instance);
                        controller.AssignModel(instance, lodIndex, modelIndex);
                        _scene3d.DrawBuffer.Add(instance);

                        // Resolve the textures
                        meshModel.ResolveTextures(_textureLibary, device);

                    }
                }
            }

            if (element is SkeletonElement skeletonElement)
            {
                _scene3d.DrawBuffer.Add(skeletonElement.MeshInstance);
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
