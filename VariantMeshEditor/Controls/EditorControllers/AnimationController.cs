using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Serilog;
using SharpDX.Direct2D1;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
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
            _viewModel.CurrentSkeletonName.Text = _skeletonElement.SkeletonFile.SkeletonName;
            _viewModel.AnimationList.SelectionChanged += OnAnimationChange;
            _viewModel.PlayPauseButton.Click += (sender, e) => OnPlayButtonPressed();
            _viewModel.NextFrameButton.Click += (sender, e) => NextFrame();
            _viewModel.PrivFrameButton.Click += (sender, e) => PrivFrame();
            _viewModel.AnimateInPlaceCheckBox.Click += (sender, e) => OnAnimateInPlaceChanged();

            _viewModel.ClearFilterButton.Click += (sender, e) => FindAllAnimations();
            _viewModel.FilterText.TextChanged += (sender, e) => FilterConditionChanged();
            _viewModel.FindAllValidAnimations.Click += (sender, e) => FindAllValidAnimations();

            FindAllAnimations();
            CreateAnimationSpeed();
        }

        private void FilterConditionChanged()
        {
            _viewModel.FilterText.Background = new System.Windows.Media.SolidColorBrush(Colors.White);
            _viewModel.AnimationList.Items.Clear();


            var filterText = _viewModel.FilterText.Text.ToLower();
            if (string.IsNullOrWhiteSpace(filterText))
            {
                var toolTip = _viewModel.FilterText.ToolTip as ToolTip;
                if (toolTip != null)
                    toolTip.IsOpen = false;

                foreach (var item in _animationFiles)
                    _viewModel.AnimationList.Items.Add(item);
                return;
            }

            Regex rx = null;
            try
            {
                rx = new Regex(filterText, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var toolTip = _viewModel.FilterText.ToolTip as ToolTip;
                if (toolTip != null)
                    toolTip.IsOpen = false;
            }
            catch (Exception e)
            {
                _viewModel.FilterText.Background = new System.Windows.Media.SolidColorBrush(Colors.Red);
                var toolTip = _viewModel.FilterText.ToolTip as ToolTip;
                if (toolTip == null)
                {
                    toolTip = new ToolTip();
                    _viewModel.FilterText.ToolTip = toolTip;
                }

                toolTip.IsOpen = true;
                toolTip.Content = e.Message;
                toolTip.Content += "\n\nCommon usage:";
                toolTip.Content += "Value0.*Value1.*Value2 -> for searching for multiple substrings";
            }

            if (rx == null)
                return;

            foreach (var item in _animationFiles)
            {
                var match = rx.Match(item.File.FullPath);
                if (match.Success)
                    _viewModel.AnimationList.Items.Add(item);
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
                        AnimationClip clip = AnimationClip.Create(file, _skeletonElement.SkeletonFile, _skeletonElement.Skeleton);
                        _animationElement.AnimationPlayer.SetAnimation(clip);

                        _viewModel.CurrentAnimation.Text = item.File.Name;
                        _viewModel.AnimationSkeleton.Text = file.SkeletonName;
                        _viewModel.AnimationType.Text = file.AnimationType.ToString();
                        _viewModel.NoFramesLabel.Content = "/" + clip.KeyFrameCollection.Count();
                        _viewModel.DebugData.Text = $"[{file.Unknown0_alwaysOne}] [{file.Unknown1_alwaysZero}]";
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

        void OnAnimateInPlaceChanged()
        {
            _animationElement.AnimationPlayer.AnimateInPlace(_viewModel.AnimateInPlaceCheckBox.IsChecked.Value);
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
                    if (animationSkeletonName == _skeletonElement.SkeletonFile.SkeletonName)
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
