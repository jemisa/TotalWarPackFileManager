using Common;
using Filetypes.ByteParsing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        string _modelToLoad;


        Stopwatch sw;
        long finishedLoadingTimer;
        public EditorMainController(SceneTreeViewController treeViewController, Scene3d scene3d, Panel editorPanel)
        {
            sw = Stopwatch.StartNew();
            _treeViewController = treeViewController;
            _scene3d = scene3d;
            _editorPanel = editorPanel;

            List<PackFile> loadedContent = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);
            finishedLoadingTimer = sw.ElapsedMilliseconds;

            _resourceLibary = new ResourceLibary(loadedContent);

            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvent;
            _scene3d.LoadScene += Create3dWorld;
        }

        public void LoadModel(string path)
        {
            _modelToLoad = path;
        }

        private void _treeViewController_VisabilityChangedEvent(FileSceneElement element, bool isVisible)
        {
            element.Visible = isVisible;
        }

        private void _treeViewController_SceneElementSelectedEvent(FileSceneElement element)
        {
            _editorPanel.Children.Clear();
            if(element.EditorViewModel != null)
                _editorPanel.Children.Add(element.EditorViewModel);
        }

        void Create3dWorld(GraphicsDevice device)
        {
            long end0 = sw.ElapsedMilliseconds;

            _scene3d.SetResourceLibary(_resourceLibary);

            long end_0 = sw.ElapsedMilliseconds;
            SceneLoader sceneLoader = new SceneLoader(_resourceLibary);
            _rootElement = sceneLoader.Load(_modelToLoad, new RootElement());
            var contentDur = sw.ElapsedMilliseconds - end_0;

            _rootElement.CreateContent(_scene3d, _resourceLibary);


            _scene3d.SceneGraphRootNode = _rootElement;
            _treeViewController.SetRootItem(_rootElement);
            SceneElementHelper.SetInitialVisability(_rootElement, true);

            var end = sw.ElapsedMilliseconds;
            var dur = end - end0;
        }
    }

}
