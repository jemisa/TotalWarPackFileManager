using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using Viewer.GraphicModels;
using Game = Common.Game;

namespace VariantMeshEditor.Controls
{
    class SceneController
    {
        SceneTreeViewController _treeViewController;
        List<PackFile> _caPackFiles;

        Dictionary<string, MeshInstance> _models = new Dictionary<string, MeshInstance>();

        public SceneController(SceneTreeViewController treeViewController)
        {
            _treeViewController = treeViewController;
            _caPackFiles = PackFileLoadHelper.LoadCaPackFilesForGame(Game.TWH2);

            _treeViewController.SceneElementSelectedEvent += _treeViewController_SceneElementSelectedEvent;
            _treeViewController.VisabilityChangedEvent += _treeViewController_VisabilityChangedEvnt;
        }

        private void _treeViewController_SceneElementSelectedEvent(FileSceneElement element)
        {
       
        }

        private void _treeViewController_VisabilityChangedEvnt(FileSceneElement element, bool isVisible)
        {
            if (element.Type == FileSceneElementEnum.RigidModel ||
                element.Type == FileSceneElementEnum.WsModel)
            {
                _models[element.FullPath].Visible = isVisible;
            }
        }

        public List<MeshInstance> LoadScene(GraphicsDevice device)
        {
            List<MeshInstance> graphiScene = new List<MeshInstance>();

            SceneLoader sceneLoader = new SceneLoader(_caPackFiles);
            var scene = sceneLoader.Load("variantmeshes\\variantmeshdefinitions\\brt_paladin.variantmeshdefinition");

            Create(scene, true, device, ref _models);
            foreach (var item in _models)
                graphiScene.Add(item.Value);

            _treeViewController.Populate(scene);

            return graphiScene;
        }

        void Create(FileSceneElement scene, bool shouldBeVisible, GraphicsDevice device, ref Dictionary<string, MeshInstance> out_created_models)
        {
            bool areAllChildrenModels = scene.Children.Where(x =>x.Type == FileSceneElementEnum.RigidModel).Count() == scene.Children.Count();
            bool firstItem = true;
            foreach (var item in scene.Children)
            {
                if (areAllChildrenModels && !firstItem)
                    shouldBeVisible = false;

                if(item.Type == FileSceneElementEnum.RigidModel)
                { 
                    Rmv2CompoundModel model3d = new Rmv2CompoundModel();
                    model3d.Create(device, (item as RigidModelElement).Model, null, 0, 0);

                    MeshInstance instance = new MeshInstance()
                    {
                        Model = model3d,
                        World = Matrix.Identity,
                        Visible = shouldBeVisible
                    };

                    out_created_models.Add(item.FullPath, instance);
                }


                firstItem = false;
                Create(item, shouldBeVisible, device, ref out_created_models);
            }
        }
    }
}
