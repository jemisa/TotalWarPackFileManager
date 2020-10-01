using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TreeViewWithCheckBoxes;
using VariantMeshEditor.ViewModels;

namespace VariantMeshEditor.Controls
{
    class SceneTreeViewController
    {
        public delegate void SceneElementSelectedEventHandler(FileSceneElement element);
        public event SceneElementSelectedEventHandler SceneElementSelectedEvent;
        public delegate void VisabilityChangedEvntHandler(FileSceneElement element, bool isVisible);
        public event VisabilityChangedEvntHandler VisabilityChangedEvent;

        TreeView _viewModel;
        ObservableCollection<TreeViewDataModel> _dataContext = new ObservableCollection<TreeViewDataModel>();


        public SceneTreeViewController(TreeView viewModel)
        {
            _viewModel = viewModel;
            _viewModel.SelectedItemChanged += _viewModel_SelectedItemChanged;
            _viewModel.DataContext = _dataContext;
            _viewModel.MouseDoubleClick += MyTreeView_PreviewMouseDoubleClick;

            _dataContext.CollectionChanged += _dataContext_CollectionChanged;
        }

        void MyTreeView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var clickedItem = TryGetClickedItem(_viewModel, e);
            if (clickedItem == null)
                return;

            var item = clickedItem.DataContext as FileSceneElement;
            if (item != null)
            {
                item.IsChecked = !item.IsChecked;
                e.Handled = true;
            }
        }

        TreeViewItem TryGetClickedItem(TreeView treeView, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
                hit = VisualTreeHelper.GetParent(hit);

            return hit as TreeViewItem;
        }

        private void _dataContext_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (FileSceneElement item in e.NewItems)
                {
                    AddCallbacks(item);
                }
            }
        }

        void AddCallbacks(FileSceneElement item)
        {
            item.Children.CollectionChanged += _dataContext_CollectionChanged;
            item.PropertyChanged += Node_PropertyChanged;
            foreach (var child in item.Children)
                AddCallbacks(child);
        }

        public List<T> GetAllOfTypeInSameVariantMesh<T>(FileSceneElement knownNode) where T : FileSceneElement
        {
            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
            {
                knownNode = knownNode.Parent;
                while (knownNode != null )
                {
                    if (knownNode.Type == FileSceneElementEnum.VariantMesh)
                        break;

                    knownNode = knownNode.Parent;
                }
            }

            if (knownNode.Type != FileSceneElementEnum.VariantMesh)
                return new List<T>();

            var output = new List<T>();
            GetAllOfType(knownNode, ref output);
            return output;
        }


        void GetAllOfType<T>(FileSceneElement root, ref List<T> outputList) where T : FileSceneElement
        {
            if (root as T != null)
                outputList.Add(root as T);

            foreach (var child in root.Children)
                GetAllOfType<T>(child, ref outputList);
        }


        public void SetRootItem(FileSceneElement rootItem)
        {
            _dataContext.Add(rootItem);
        }

        private void _viewModel_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewDataModel selectedItem = e.NewValue as TreeViewDataModel;
            SceneElementSelectedEvent?.Invoke(selectedItem as FileSceneElement);
        }

        bool _updatingCheckedStatus = false;
        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_updatingCheckedStatus == true)
                return;
            _updatingCheckedStatus = true;
            if (e.PropertyName == "IsChecked")
            {
                var fileSceneElement = sender as FileSceneElement;

                if (fileSceneElement.Type == FileSceneElementEnum.Skeleton)
                {
                    VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);
                }

                if (fileSceneElement.IsChecked)
                {
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        var parentChildren = fileSceneElement.Parent.Children;
                        foreach (var child in parentChildren)
                        {
                            child.IsChecked = false;
                            VisabilityChangedEvent?.Invoke(child as FileSceneElement, child.IsChecked);
                        }
                        fileSceneElement.IsChecked = true;

                        VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);

                        var parent = fileSceneElement.Parent;
                        while (parent != null)
                        {
                            parent.IsChecked = true;
                            parent = parent.Parent;
                        }
                    }
                    else
                    {
                        SetChildrenVisability(fileSceneElement, true);
                    }
                }
                else
                {
                    if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                    {
                        VisabilityChangedEvent?.Invoke(fileSceneElement as FileSceneElement, fileSceneElement.IsChecked);
                    }
                    else
                    {
                        SetChildrenVisability(fileSceneElement, false);
                    }
                }
            }

            _updatingCheckedStatus = false;
        }

        void SetChildrenVisability(FileSceneElement root, bool isVisible)
        {
            foreach (var treeItem in root.Children)
            {
                var fileSceneElement = treeItem as FileSceneElement;
                if (fileSceneElement.Type == FileSceneElementEnum.RigidModel ||
                        fileSceneElement.Type == FileSceneElementEnum.WsModel)
                {
                    if (isVisible)
                        VisabilityChangedEvent?.Invoke(treeItem as FileSceneElement, treeItem.IsChecked);
                    else
                        VisabilityChangedEvent?.Invoke(treeItem as FileSceneElement, false);
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
