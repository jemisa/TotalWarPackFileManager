using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Viewer.Animation.AnimationClip;

namespace Viewer.Animation
{
    public class AnimationPlayer
    {
        AnimationClip _currentAnimation;
        int _currentFrame;
        TimeSpan _timeAtCurrentFrame;
        public double FrameRate { get; set; } = 20.0 / 1000.0;
        
        public void Update(GameTime gameTime)
        {
            if (_currentAnimation != null)
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
            _currentAnimation = animation;
            _timeAtCurrentFrame = TimeSpan.FromSeconds(0);
            _currentFrame = 0;
        }

        public void Play() { }

        public void Pause() { }

        public AnimationFrame GetCurrentFrame()
        {
            if (_currentAnimation != null)
                return _currentAnimation.KeyFrameCollection[_currentFrame];
            return null;
        }
    }
}
