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
        Skeleton _skeleton;
        public SkeletonController(SkeletonEditorView viewModel, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
            _skeleton = skeletonElement.Skeleton;

            _viewModel.SkeletonName.Content = "Name : " + _skeleton.Name;
            _viewModel.BoneCount.Content = "Bone Count : " + _skeleton.Bones.Count;
            var index = 0;
            foreach (var bone in _skeleton.Bones)
            {
                index++;
                if (bone.ParentId == -1)
                {
                    _viewModel.SkeletonBonesView.Items.Add(CreateNode(bone));
                }
                else
                {
                    var parentBone = _skeleton.Bones[bone.ParentId];
                    var treeParent = GetParent(_viewModel.SkeletonBonesView.Items, parentBone);

                    if (treeParent != null)
                        treeParent.Items.Add(CreateNode(bone));
                }
            }
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

      


    }
}
