using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;
using VariantMeshEditor.Views.EditorViews.Util;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class SlotController
    {
        SlotElement _slotElement;
        SlotEditorView _viewModel;
        SceneTreeViewController _sceneTreeView = null;
        public SlotController(SlotEditorView viewModel, SceneTreeViewController sceneTree, SlotElement slot)
        {
            _viewModel = viewModel;
            _slotElement = slot;
            _sceneTreeView = sceneTree;
            CreateUi();
        }

        void CreateUi()
        {
            _viewModel.SlotName.Text = "Name:" + _slotElement.SlotName;
            _viewModel.AttachmentPointComboBox.Items.Add(_slotElement.AttachmentPoint);
            _viewModel.AddButton.Click += AddButton_Click;

            CreateMeshList();
        }

        void CreateMeshList()
        {
            _viewModel.MeshStackPanel.Children.Clear();
            foreach (var child in _slotElement.Children)
            {
                if (child.Type == FileSceneElementEnum.VariantMesh ||
                    child.Type == FileSceneElementEnum.WsModel ||
                    child.Type == FileSceneElementEnum.RigidModel)
                {
                    var meshReference = new BrowsableItemView();
                    meshReference.LabelName.Content = "Mesh:";
                    meshReference.PathTextBox.Text = child.FullPath;
                    meshReference.Tag = child;

                    meshReference.RemoveButton.Click += RemoveButton_Click;
                    meshReference.RemoveButton.Tag = child;

                    _viewModel.MeshStackPanel.Children.Add(meshReference);
                }
            }
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            VariantMeshElement newSlot = new VariantMeshElement(_slotElement, "New");
            _slotElement.Children.Add(newSlot);
            _sceneTreeView.CreateNode(newSlot, false, _slotElement.TreeNode);
            CreateMeshList();
        }

        private void RemoveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var parent = button.Tag as FileSceneElement;


            //_sceneTreeView.RemoveNode();
        }
    }
}
