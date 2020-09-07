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
        List<PackFile> _caPackFiles;
        Panel _toolPanel;
        Scene3d _scene3d;
        Dictionary<FileSceneElement, MeshInstance> _models = new Dictionary<FileSceneElement, MeshInstance>();

        public SceneController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel toolPanel)
        {
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _toolPanel = toolPanel;

            _caPackFiles = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
           

            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvnt;
            _scene3d.LoadScene += LoadScene;
        }

        private void _treeViewController_SceneElementSelectedEvent(FileSceneElement element)
        {
            _toolPanel.Children.Clear();
            if (element.Type == FileSceneElementEnum.Skeleton)
            {
                SkeletonEditorView view = new SkeletonEditorView();
                _toolPanel.Children.Add(view);
                SkeletonController controller = new SkeletonController(view, (element as SkeletonElement));
            }
            else if (element.Type == FileSceneElementEnum.Animation)
            {
                var skeleton = _treeViewController.GetAllOfTypeInSameVariantMesh<SkeletonElement>(element);
                if (skeleton.Count == 1)
                { 
                    AnimationEditorView view = new AnimationEditorView();
                    _toolPanel.Children.Add(view);
                    AnimationController controller = new AnimationController(view, (element as AnimationElement), skeleton.First());
                }
            }
            else if (element.Type == FileSceneElementEnum.Slot)
            {
                SlotEditorView view = new SlotEditorView();
                _toolPanel.Children.Add(view);
                SlotController controller = new SlotController(view, (element as SlotElement));
            }
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
            SceneLoader sceneLoader = new SceneLoader(_caPackFiles);
            var scene = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");
            
            _treeViewController.Populate(scene);
            CreateMeshes(scene, device, ref _models);
            _scene3d.DrawBuffer = _models.Select(x=>x.Value).ToList();
        }

        void CreateMeshes(FileSceneElement scene, GraphicsDevice device, ref Dictionary<FileSceneElement, MeshInstance> out_created_models)
        {
             foreach (var item in scene.Children)
             {
                 if(item.Type == FileSceneElementEnum.RigidModel)
                 { 
                     Rmv2CompoundModel model3d = new Rmv2CompoundModel();
                     model3d.Create(device, (item as RigidModelElement).Model, null, 0, 0);

                     MeshInstance instance = new MeshInstance()
                     {
                         Model = model3d,
                         World = Matrix.Identity,
                         Visible = item.Vis == System.Windows.Visibility.Visible && item.IsChecked == true
                     };

                     out_created_models.Add(item, instance);
                 }

                 if (item .Type == FileSceneElementEnum.Skeleton)
                 {
                     var skeleteonElement = item as SkeletonElement;
                     out_created_models.Add(item, skeleteonElement.MeshInstance);
                 }

                 CreateMeshes(item, device, ref out_created_models);
             }
        }
    }
}
