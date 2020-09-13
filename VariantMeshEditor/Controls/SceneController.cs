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
using WpfTest.Scenes;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class SceneController
    {
        SceneTreeViewController _treeViewController;
        List<PackFile> _loadedContent;
        Panel _toolPanel;
        Scene3d _scene3d;
        Dictionary<FileSceneElement, MeshInstance> _models = new Dictionary<FileSceneElement, MeshInstance>();

        public SceneController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel toolPanel)
        {
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _toolPanel = toolPanel;

            _loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
           
            _treeViewController.SceneElementSelectedEvent += CreateEditor;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvnt;
            _scene3d.LoadScene += LoadScene;
        }


        private void _treeViewController_VisabilityChangedEvnt(FileSceneElement element, bool isVisible)
        {
            if (element.Type == FileSceneElementEnum.RigidModel ||
                element.Type == FileSceneElementEnum.WsModel ||
                element.Type == FileSceneElementEnum.Skeleton)
            {
                if(_models.ContainsKey(element))
                    _models[element].Visible = isVisible;
            }
        }

        public void LoadScene(GraphicsDevice device)
        {
            SceneLoader sceneLoader = new SceneLoader(_loadedContent);
            var scene = sceneLoader.Load(null, device, "variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");
            
            _treeViewController.Populate(scene);
            RegisterAnimations(scene);
            CreateMeshDictionary(scene, device, ref _models);
            _scene3d.DrawBuffer = _models.Select(x=>x.Value).ToList();
        }

        void CreateMeshDictionary(FileSceneElement scene, GraphicsDevice device, ref Dictionary<FileSceneElement, MeshInstance> out_created_models)
        {
             foreach (var item in scene.Children)
             {
                var typedItem = item as RenderableFileSceneElement;
                if (typedItem != null)
                {
                    typedItem.MeshInstance.Visible = item.Vis == System.Windows.Visibility.Visible && item.IsChecked == true;
                    out_created_models.Add(item, typedItem.MeshInstance);
                }

                CreateMeshDictionary(item, device, ref out_created_models);
             }
        }

        void RegisterAnimations(FileSceneElement scene)
        {
            if (scene as AnimationElement != null)
                _scene3d.AnimationPlayers.Add((scene as AnimationElement).AnimationPlayer);

            foreach (var item in scene.Children)
                RegisterAnimations(item);
        }

        void CreateEditor(FileSceneElement element)
        {
            _toolPanel.Children.Clear();
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
            _toolPanel.Children.Add(view);
            SkeletonController controller = new SkeletonController(view, element);
        }

        void CreateAnimationEditor(AnimationElement element)
        {
            var skeleton = _treeViewController.GetAllOfTypeInSameVariantMesh<SkeletonElement>(element);
            if (skeleton.Count == 1)
            {
                AnimationEditorView view = new AnimationEditorView();
                _toolPanel.Children.Add(view);
                AnimationController controller = new AnimationController(view, _loadedContent, element, skeleton.First());
            }
        }

        void CreateSlotEditor(SlotElement element)
        {
            SlotEditorView view = new SlotEditorView();
            _toolPanel.Children.Add(view);
            SlotController controller = new SlotController(view, element);
        }

        void CreateRigidModelEditor(RigidModelElement element)
        {
            RigidModelEditorView view = new RigidModelEditorView();
            _toolPanel.Children.Add(view);
            var controller = new RigidModelController(view, element, _loadedContent);
        }
    }
}
