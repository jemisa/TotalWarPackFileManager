using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class RootController
    {
        RootElement _rootElement;
        RootEditorView _viewModel;

        List<VariantMeshElement> _referenceElements = new List<VariantMeshElement>();

        public RootController(RootEditorView viewModel, RootElement rootElement)
        {
            _viewModel = viewModel;
            _rootElement = rootElement;
            CreateMeshList();
            _viewModel.AddButton.Click += AddButton_Click;
        }


        void CreateMeshList()
        {
            _viewModel.MeshStackPanel.Children.Clear();
            foreach (var child in _referenceElements)
            {
                if (child.Type == FileSceneElementEnum.VariantMesh ||
                    child.Type == FileSceneElementEnum.WsModel ||
                    child.Type == FileSceneElementEnum.RigidModel)
                {
                    var meshReference = new BrowsableItemView();
                    meshReference.LabelName.Content = "Mesh:";
                    meshReference.PathTextBox.Text = child.FullPath;
                    meshReference.Tag = child;

                    //meshReference.RemoveButton.Click += RemoveButton_Click;
                    meshReference.RemoveButton.Tag = child;

                    _viewModel.MeshStackPanel.Children.Add(meshReference);
                }
            }
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VariantMeshElement newSlot = new VariantMeshElement(_rootElement, "New");
            _referenceElements.Add(newSlot);
            _rootElement.Children.Add(newSlot);
            //_sceneTreeView.CreateNode(newSlot, false, _slotElement);
            CreateMeshList();
        }

    }
}
