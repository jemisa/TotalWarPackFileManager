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

        public EditorMainController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel editorPanel)
        {
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _editorPanel = editorPanel;

            List<PackFile> loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
            _resourceLibary = new ResourceLibary(loadedContent);

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



        void Create3dWorld(GraphicsDevice device)
        {
            _scene3d.SetResourceLibary(_resourceLibary);

            SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            _rootElement = sceneLoader.Load(null, null, "variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");

            
            

            _rootElement.CreateContent(_scene3d, _resourceLibary);

            //Create3dModels(device, _rootElement);
            //ResovleAttachment(_rootElement);
            //RegisterAnimations(_rootElement);
            _scene3d._rootNode = _rootElement;
            _treeViewController.Populate(_rootElement);
            _treeViewController.SetInitialVisability(_rootElement, true);
        }

        /*public void ResovleAttachment(FileSceneElement element)
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
        }*/

       



    }

}
