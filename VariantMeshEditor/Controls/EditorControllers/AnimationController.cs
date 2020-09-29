using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel.Animation;
using Serilog;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using VariantMeshEditor.Util;
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews;
using Viewer.Animation;
using Viewer.Scene;

namespace VariantMeshEditor.Controls.EditorControllers
{


    class AnimationController
    {
        AnimationEditorView _viewModel;
        ResourceLibary _resourceLibary;
        SkeletonElement _skeletonElement;
        AnimationElement _animationElement;
        ILogger _logger = Logging.Create<AnimationController>();

        List<AnimationListItem> _animationFiles = new List<AnimationListItem>();
        public AnimationController(AnimationEditorView viewModel, ResourceLibary resourceLibary, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _viewModel = viewModel;
            _resourceLibary = resourceLibary;
            _skeletonElement = skeletonElement;
            _animationElement = animationElement;

            _viewModel.CurrentAnimation.Text = "";
            _viewModel.CurrentSkeletonName.Text = _skeletonElement.SkeletonFile.Name;
            _viewModel.AnimationList.SelectionChanged += OnAnimationChange;
            _viewModel.PlayPauseButton.Click += (sender, e) => OnPlayButtonPressed();
            _viewModel.NextFrameButton.Click += (sender, e) => NextFrame();
            _viewModel.PrivFrameButton.Click += (sender, e) => PrivFrame();

            _viewModel.ClearFilterButton.Click += (sender, e) => FindAllAnimations();
            _viewModel.FilterText.TextChanged += (sender, e) => FilterConditionChanged();
            _viewModel.FindAllValidAnimations.Click += (sender, e) => FindAllValidAnimations();

            FindAllAnimations();
            CreateAnimationSpeed();
        }

        private void FilterConditionChanged()
        {
            _viewModel.AnimationList.Items.Clear();
            foreach (var item in _animationFiles)
            {
                var filterText = _viewModel.FilterText.Text.ToLower();
                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    var contains = item.File.FullPath.ToLower().Contains(filterText);
                    if (contains)
                        _viewModel.AnimationList.Items.Add(item);
                }
                else
                {
                    _viewModel.AnimationList.Items.Add(item);
                }
            }
        }

        private void OnAnimationChange(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {

                foreach (AnimationListItem item in e.AddedItems)
                {
                    try
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
                    catch (Exception exception)
                    {
                        var error = $"Error loading skeleton {item.File.FullPath}:{exception.Message}";
                        _viewModel.ErrorText.Text = error;
                        _logger.Error(error);
                    }
                }
            }
        }

        void OnPlayButtonPressed()
        {
            var player = _animationElement.AnimationPlayer;
            if(player.IsPlaying)
                player.Pause();
            else
                player.Play();
        }

        void NextFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame++;
        }

        void PrivFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame--;
        }

        void CreateAnimationSpeed()
        {
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = 20.0 / 1000.0, DisplayName = "1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.5) / 1000.0, DisplayName = "0.5x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.1) / 1000.0, DisplayName = "0.1x" });
            _viewModel.AnimationSpeedComboBox.Items.Add(new AnimationSpeedItem() { FrameRate = (20.0 / 0.01) / 1000.0, DisplayName = "0.01x" });
            _viewModel.AnimationSpeedComboBox.SelectedIndex = 0;

            _viewModel.AnimationSpeedComboBox.SelectionChanged += OnAnimationSpeedChanged;
        }

        private void OnAnimationSpeedChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                foreach (AnimationSpeedItem item in e.AddedItems)
                    _animationElement.AnimationPlayer.FrameRate = item.FrameRate;
            }
        }

        void FindAllAnimations()
        {
            _viewModel.FilterText.Text = "";
            _animationFiles.Clear();
            var allAnimationFiles = PackFileLoadHelper.GetAllWithExtention(_resourceLibary.PackfileContent, "anim");
            foreach (var file in allAnimationFiles)
            {
                var animationItem = new AnimationListItem() { File = file };
                _animationFiles.Add(animationItem);
            }

            FilterConditionChanged();
        }

        void FindAllValidAnimations()
        {
            var filteredList = new List<AnimationListItem>();
            foreach (var item in _animationFiles)
            {
                try
                {
                    var animationSkeletonName = AnimationFile.GetAnimationSkeletonName(new ByteChunk(item.File.Data));
                    if (animationSkeletonName == _skeletonElement.SkeletonFile.Name)
                        filteredList.Add(new AnimationListItem() { File = item.File });
                }
                catch (Exception exception)
                {
                    var error = $"Error loading skeleton {item.File.FullPath}:{exception.Message}";
                    _viewModel.ErrorText.Text = error;
                    _logger.Error(error);
                }
            }

            _animationFiles = filteredList;
            _viewModel.FilterText.Text = "";
            FilterConditionChanged();
        }

        public string GetCurrentAnimationName()
        {
            return _viewModel.CurrentAnimation.Text;
        }

        public void Update()
        {
            _viewModel.CurretFrameText.Text = _animationElement.AnimationPlayer.CurrentFrame.ToString();
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
