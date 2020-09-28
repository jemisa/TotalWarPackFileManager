using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;

namespace VariantMeshEditor.Controls.EditorControllers
{


    class AnimationController
    {
        AnimationEditorView _viewModel;
        List<PackFile> _loadedContent;
        SkeletonElement _skeletonElement;
        AnimationElement _animationElement;
        public AnimationController(AnimationEditorView viewModel, List<PackFile> loadedContent, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
            _loadedContent = loadedContent;
            _skeletonElement = skeletonElement;
            _animationElement = animationElement;

            _viewModel.CurrentAnimation.Text = "";
            _viewModel.CurrentSkeletonName.Text = _skeletonElement.SkeletonFile.Name;
            _viewModel.AnimationList.SelectionChanged += AnimationList_SelectionChanged;
            FindAllAnimationsForSkeleton();
            CreateAnimationSpeed();
        }

        private void AnimationList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                foreach (AnimationListItem item in e.AddedItems)
                {
                    AnimationFile file = AnimationFile.Create(new ByteChunk(item.File.Data));
                    AnimationClip clip = AnimationClip.Create(file, _skeletonElement.SkeletonModel);
                   _animationElement.AnimationPlayer.SetAnimation(clip);

                    _viewModel.CurrentAnimation.Text = item.File.Name;
                    _viewModel.AnimationSkeleton.Text = file.SkeletonName;
                    _viewModel.AnimationType.Text = file.AnimationType.ToString();
                    _viewModel.NoFramesLabel.Content = "/" + clip.KeyFrameCollection.Count();
                    _viewModel.DebugData.Text = $"[{file.Unknown0}] [{file.Unknown1}] [{file.Unknown2}] [{file.Unknown3}] [{file.Unknown4}]";
                }
            }
        }

        void CreateAnimationSpeed()
        {
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = 20.0 / 1000.0, DisplayName = "1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.5) / 1000.0, DisplayName = "0.5x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.1) / 1000.0, DisplayName = "0.1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.01) / 1000.0, DisplayName = "0.01x" });
            _viewModel.AnimationSpeedComboBox.SelectedIndex = 0;

            _viewModel.AnimationSpeedComboBox.SelectionChanged += AnimationSpeedComboBox_SelectionChanged;
        }

        private void AnimationSpeedComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                foreach (AnimationSpeedItem item in e.AddedItems)
                    _animationElement.AnimationPlayer.FrameRate = item.FrameRate;
            }
        }

        void FindAllAnimationsForSkeleton()
        {
            var files = PackFileLoadHelper.GetAllFilesInDirectory(_loadedContent, "animations\\battle\\" + _skeletonElement.SkeletonFile.Name);
            var animations = files.Where(x => x.FileExtention == "anim").ToList();

            foreach (var file in animations)
            {
                if(file.Name.Contains("hu1_sws_stand_01"))
                _viewModel.AnimationList.Items.Add(new AnimationListItem() { File = file });
            }
        }

        class AnimationListItem
        { 
            public PackedFile File { get; set; }
            public override string ToString()
            {
                return File.FullPath;
            }
        }

        class AnimationSpeedItem
        {
            public double FrameRate { get; set; }
            public string DisplayName { get; set; }
            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
