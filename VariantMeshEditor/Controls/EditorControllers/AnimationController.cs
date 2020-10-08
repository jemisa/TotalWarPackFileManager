using Common;
using Filetypes.ByteParsing;
using Filetypes.RigidModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;
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
        string _currentAnimationName;

        List<AnimationListItem> _animationFiles = new List<AnimationListItem>();
        public AnimationController( ResourceLibary resourceLibary, AnimationElement animationElement, SkeletonElement skeletonElement)
        {
            _resourceLibary = resourceLibary;
            _skeletonElement = skeletonElement;
            _animationElement = animationElement;
        }

        public AnimationEditorView GetView()
        {
            if (_viewModel == null)
            {
                _viewModel = new AnimationEditorView();
                _viewModel.CurrentSkeletonName.Text = _skeletonElement.SkeletonFile.Header.SkeletonName;
                _viewModel.AnimationList.SelectionChanged += OnAnimationChange;
                _viewModel.PlayPauseButton.Click += (sender, e) => OnPlayButtonPressed();
                _viewModel.NextFrameButton.Click += (sender, e) => NextFrame();
                _viewModel.PrivFrameButton.Click += (sender, e) => PrivFrame();
                _viewModel.AnimateInPlaceCheckBox.Click += (sender, e) => OnAnimationSettingsChanged();
                _viewModel.DynamicFrameCheckbox.Click += (sender, e) => OnAnimationSettingsChanged();
                _viewModel.StaticFramesCheckbox.Click += (sender, e) => OnAnimationSettingsChanged();

                _viewModel.ClearFilterButton.Click += (sender, e) => FindAllAnimations();
                _viewModel.FilterText.TextChanged += (sender, e) => FilterConditionChanged();
                _viewModel.FindAllValidAnimations.Click += (sender, e) => FindAllValidAnimations();

                FindAllAnimations();
                CreateAnimationSpeed();
            }
            return _viewModel;
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
                        AnimationFile animationFile = AnimationFile.Create(new ByteChunk(item.File.Data));
                        AnimationClip clip = AnimationClip.Create(animationFile, _skeletonElement.SkeletonFile, _skeletonElement.Skeleton);
                        _animationElement.AnimationPlayer.SetAnimation(clip);

                        _currentAnimationName = item.File.Name;
                        _viewModel.MainAnimationExpander.Header = "Main animation : " + _currentAnimationName;
                        _viewModel.AnimationType.Text = animationFile.Header.AnimationType.ToString();
                        _viewModel.NoFramesLabel.Content = "/" + clip.KeyFrameCollection.Count();

                        _viewModel.DynamicFrameCheckbox.IsEnabled = animationFile.DynamicFrames.Count != 0;
                        _viewModel.DynamicFrameCheckbox.IsChecked = _viewModel.DynamicFrameCheckbox.IsEnabled;

                        _viewModel.StaticFramesCheckbox.IsEnabled = animationFile.StaticFrame != null;
                        _viewModel.StaticFramesCheckbox.IsChecked = _viewModel.StaticFramesCheckbox.IsEnabled;

                        SyncAllAnimations();

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

            SyncAllAnimations();
        }

        void NextFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame++;

            SyncAllAnimations();
        }

        void PrivFrame()
        {
            _animationElement.AnimationPlayer.Pause();
            _animationElement.AnimationPlayer.CurrentFrame--;

            SyncAllAnimations();
        }

        void OnAnimationSettingsChanged()
        {
            _animationElement.AnimationPlayer.UpdatCurrentAnimationSettings(
                _viewModel.AnimateInPlaceCheckBox.IsChecked.Value, 
                _viewModel.DynamicFrameCheckbox.IsChecked.Value, 
                _viewModel.StaticFramesCheckbox.IsChecked.Value);
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

            SyncAllAnimations();
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
                    var animationSkeletonName = AnimationFile.GetAnimationHeader(new ByteChunk(item.File.Data)).SkeletonName;
                    if (animationSkeletonName == _skeletonElement.SkeletonFile.Header.SkeletonName)
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
            return _currentAnimationName;
        }

        public void Update()
        {
            if(_viewModel != null)
                _viewModel.CurretFrameText.Text = (_animationElement.AnimationPlayer.CurrentFrame + 1).ToString();
        }

        void SyncAllAnimations()
        {
            var root = SceneElementHelper.GetRoot(_animationElement);
            List<AnimationElement> animationItems = new List<AnimationElement>();
            SceneElementHelper.GetAllChildrenOfType<AnimationElement>(root, animationItems);
            animationItems.Remove(_animationElement);

            foreach (var animationItem in animationItems)
            {
                animationItem.AnimationPlayer.CurrentFrame = _animationElement.AnimationPlayer.CurrentFrame;
                if (_animationElement.AnimationPlayer.IsPlaying)
                    animationItem.AnimationPlayer.Play();
                else
                    animationItem.AnimationPlayer.Pause();
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
