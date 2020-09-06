using Common;
using Filetypes.RigidModel.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VariantMeshEditor.Util;
using VariantMeshEditor.Views.EditorViews;

namespace VariantMeshEditor.Controls.EditorControllers
{
    class SkeletonController
    {
        SkeletonEditorView _viewModel;
        List<PackFile>  _caPackFiles;

        SkeletonElement _skeletonElement;
        public SkeletonController(SkeletonEditorView viewModel, List<PackFile> caPackFiles, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
            _skeletonElement = skeletonElement;
            _caPackFiles = caPackFiles;
            var allSkeletons = GetAllSkeltons();
            foreach(var skeleton in allSkeletons)
                _viewModel.SkeltonTypeComboBox.Items.Add(skeleton);

            var i = _viewModel.SkeltonTypeComboBox.Items.IndexOf(skeletonElement.Skeleton.Name + ".anim");
            _viewModel.SkeltonTypeComboBox.SelectedIndex = i;

            _viewModel.SkeltonTypeComboBox.SelectionChanged += SkeltonTypeComboBox_SelectionChanged;
            CreateBoneOverview();
        }

        void CreateBoneOverview()
        {
            _viewModel.SkeletonBonesView.Items.Clear();
            _viewModel.BoneCount.Content = "Bone Count : " + _skeletonElement.Skeleton.Bones.Count;
            var index = 0;
            foreach (var bone in _skeletonElement.Skeleton.Bones)
            {
                index++;
                if (bone.ParentId == -1)
                {
                    _viewModel.SkeletonBonesView.Items.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = _skeletonElement.Skeleton.Bones[bone.ParentId];
                    var treeParent = GetParent(_viewModel.SkeletonBonesView.Items, parentBone);

                    if (treeParent != null)
                        treeParent.Items.Add(CreateNode(bone));
                }
            }
        }

        private void SkeltonTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string skeletonName = _viewModel.SkeltonTypeComboBox.SelectedItem as string;

            _skeletonElement.Create(_caPackFiles, skeletonName);
            CreateBoneOverview();
        }

        TreeViewItem CreateNode(BoneInfo bone)
        {
            TreeViewItem item = new TreeViewItem
            {
                Header = bone.Name + " [" + bone.Id + "]",
                Tag = bone,
                IsExpanded = true
            };
            return item;
        }

        TreeViewItem GetParent(ItemCollection root, BoneInfo parentBone)
        {
            foreach (TreeViewItem item in root)
            {
                if (item.Tag == parentBone)
                    return item;

                var result =  GetParent(item.Items, parentBone);
                if (result != null)
                    return result;
            }
            return null;
        }

        List<string> GetAllSkeltons()
        {

            var possibleSkeletons = PackFileLoadHelper.GetAllFilesWithExtentionInDirectory(_caPackFiles, @"animations\skeletons");
            return possibleSkeletons.Where(x => x.FileExtention == "anim").Select(x => x.Name).ToList();
        }


    }
}
