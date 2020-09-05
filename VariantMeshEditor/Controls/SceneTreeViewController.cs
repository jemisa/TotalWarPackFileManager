using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.Util;

namespace VariantMeshEditor.Controls
{
    class SceneTreeViewController
    {
        public delegate void SceneElementSelectedEventHandler(FileSceneElement element);
        public event SceneElementSelectedEventHandler SceneElementSelectedEvent;
        public delegate void VisabilityChangedEvntHandler(FileSceneElement element, bool isVisible);
        public event VisabilityChangedEvntHandler VisabilityChangedEvent;

        TreeView _viewModel;
        public SceneTreeViewController(TreeView viewModel)
        {
            _viewModel = viewModel;
            _viewModel.SelectedItemChanged += _viewModel_SelectedItemChanged;
        }

        public void Populate(FileSceneElement rootItem)
        {
            var node = Create(rootItem, true);
            node.Initialize();
            _viewModel.DataContext = new List<TreeViewDataModel>() { node }; ;
        }

        TreeViewDataModel Create(FileSceneElement scene, bool shouldBeSelected, TreeViewDataModel parent = null)
        {
            TreeViewDataModel node = new TreeViewDataModel(scene.ToString())
            {
                IsChecked = shouldBeSelected,
                Tag = scene,
            };

            node.PropertyChanged += Node_PropertyChanged;

            if (scene as TransformElement != null)
                node.Vis = Visibility.Hidden;
            if (scene as AnimationElement != null)
                node.Vis = Visibility.Hidden;

            bool areAllChildrenModels = scene.Children.Where(x => (x as RigidModelElement) != null).Count() == scene.Children.Count();
            bool firstItem = true;
            foreach (var item in scene.Children)
            {
                if (areAllChildrenModels && !firstItem)
                    shouldBeSelected = false;

                firstItem = false;
                Create(item, shouldBeSelected, node);
            }

            if (parent != null)
                parent.Children.Add(node);

            return node;
        }

        private void _viewModel_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            //SceneElementSelectedEvent?.Invoke((sender as TreeViewDataModel).Tag as FileSceneElement);
        }

        bool _updatingCheckedStatus = false;
        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updatingCheckedStatus == true)
                return;
            _updatingCheckedStatus = true;
            if (e.PropertyName == "IsChecked")
            {
                var treeItem = sender as TreeViewDataModel;
                var fileSceneElement = treeItem.Tag as FileSceneElement;
                if (treeItem.IsChecked.Value)
                {
                   
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        var parentChildren = treeItem.Parent.Children;
                        foreach (var child in parentChildren)
                        {
                            child.IsChecked = false;
                            VisabilityChangedEvent?.Invoke(child.Tag as FileSceneElement, child.IsChecked.Value);
                        }
                        treeItem.IsChecked = true;

                        VisabilityChangedEvent?.Invoke(treeItem.Tag as FileSceneElement, treeItem.IsChecked.Value);

                        var parent = treeItem.Parent;
                        while (parent != null)
                        {
                            parent.IsChecked = true;
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        SetChildrenVisability(treeItem, true);
                    }
                }
                else
                {
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        VisabilityChangedEvent?.Invoke(treeItem.Tag as FileSceneElement, treeItem.IsChecked.Value);
                    }
                    else
                    {
                        SetChildrenVisability(treeItem, false);
                    }
                }
            }

            _updatingCheckedStatus = false;
        }

        void SetChildrenVisability(TreeViewDataModel root, bool isVisible)
        {
            foreach (var treeItem in root.Children)
            {
                var fileSceneElement = treeItem.Tag as FileSceneElement;
                if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                {
                    if (isVisible)
                        VisabilityChangedEvent?.Invoke(treeItem.Tag as FileSceneElement, treeItem.IsChecked.Value);
                    else
                        VisabilityChangedEvent?.Invoke(treeItem.Tag as FileSceneElement, false);
                }

                if (fileSceneElement.Type == FileSceneElementEnum.Slot && treeItem.IsChecked == false)
                {
                }
                else if (fileSceneElement.Type == FileSceneElementEnum.VariantMesh && treeItem.IsChecked == false)
                {
                }
                else
                {
                    SetChildrenVisability(treeItem, isVisible);
                }
                
            }
        }
    }
}
