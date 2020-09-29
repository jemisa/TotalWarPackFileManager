using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Viewer.Animation.AnimationClip;

namespace Viewer.Animation
{
    public class AnimationPlayer
    {
        AnimationClip _currentAnimation;
        int _currentFrame;


        public int CurrentFrame
        {
            get { return _currentFrame; }
            set 
            {
                if (value < 0)
                    value = 0;

                if(_currentAnimation != null)
                {
                    if (value > FrameCount() - 1)
                        value = FrameCount() - 1;
                }
                
                _currentFrame = value; 
            }
        }

        TimeSpan _timeAtCurrentFrame;
        public double FrameRate { get; set; } = 20.0 / 1000.0;
        public bool IsPlaying { get; private set; } = false;
        
        public void Update(GameTime gameTime)
        {
            if (_currentAnimation != null && IsPlaying)
            {
                _timeAtCurrentFrame += gameTime.ElapsedGameTime;
                if (_timeAtCurrentFrame.TotalMilliseconds >= FrameRate * 1000)
                {
                    _timeAtCurrentFrame = TimeSpan.FromSeconds(0);
                    _currentFrame++;

                    if (_currentFrame >= _currentAnimation.KeyFrameCollection.Count)
                        _currentFrame = 0;
                }
            }
        }

        public void SetAnimation(AnimationClip animation)
        {
            IsPlaying = true;
            _currentFrame = 0;
            _currentAnimation = animation;
            _timeAtCurrentFrame = TimeSpan.FromSeconds(0);
        }

        public void Play() { IsPlaying = true; }

        public void Pause() { IsPlaying = false; }

        public AnimationFrame GetCurrentFrame()
        {
            if (_currentAnimation != null)
                return _currentAnimation.KeyFrameCollection[_currentFrame];
            return null;
        }

        public int FrameCount()
        {
            if (_currentAnimation != null)
                return _currentAnimation.KeyFrameCollection.Count();
            return 0;
        }
    }
}
