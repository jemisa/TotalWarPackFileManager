namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;

    public class ImageRenderer : BaseRenderer
    {
        private bool isPaused;
        private Stopwatch stopwatch;
        private System.Threading.Timer tickler;

        public ImageRenderer()
        {
            this.isPaused = true;
            this.tickler = new System.Threading.Timer(new TimerCallback(this.OnTimer), null, -1, -1);
            this.stopwatch = new Stopwatch();
        }

        public ImageRenderer(bool startAnimations) : this()
        {
            this.Paused = !startAnimations;
        }

        protected Image GetImageFromAspect()
        {
            if ((base.OLVSubItem != null) && (base.OLVSubItem.ImageSelector is Image))
            {
                if (base.OLVSubItem.AnimationState == null)
                {
                    return (Image) base.OLVSubItem.ImageSelector;
                }
                return base.OLVSubItem.AnimationState.image;
            }
            Image image = null;
            if (base.Aspect is byte[])
            {
                using (MemoryStream stream = new MemoryStream((byte[]) base.Aspect))
                {
                    try
                    {
                        image = Image.FromStream(stream);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            else if (base.Aspect is int)
            {
                image = this.GetImage(base.Aspect);
            }
            else
            {
                string aspect = base.Aspect as string;
                if (!string.IsNullOrEmpty(aspect))
                {
                    try
                    {
                        image = Image.FromFile(aspect);
                    }
                    catch (FileNotFoundException)
                    {
                        image = this.GetImage(base.Aspect);
                    }
                    catch (OutOfMemoryException)
                    {
                        image = this.GetImage(base.Aspect);
                    }
                }
            }
            if ((base.OLVSubItem != null) && AnimationState.IsAnimation(image))
            {
                base.OLVSubItem.AnimationState = new AnimationState(image);
            }
            if (base.OLVSubItem != null)
            {
                base.OLVSubItem.ImageSelector = image;
            }
            return image;
        }

        public void OnTimer(object state)
        {
            MethodInvoker method = null;
            if ((base.ListView == null) || this.Paused)
            {
                this.tickler.Change(0x3e8, -1);
            }
            else if (base.ListView.InvokeRequired)
            {
                if (method == null)
                {
                    method = delegate {
                        this.OnTimer(state);
                    };
                }
                base.ListView.Invoke(method);
            }
            else
            {
                this.OnTimerInThread();
            }
        }

        protected void OnTimerInThread()
        {
            if (!base.ListView.IsDisposed)
            {
                if ((base.ListView.View != View.Details) || (base.Column.Index < 0))
                {
                    this.tickler.Change(0x3e8, -1);
                }
                else
                {
                    long elapsedMilliseconds = this.stopwatch.ElapsedMilliseconds;
                    int index = base.Column.Index;
                    long num3 = elapsedMilliseconds + 0x3e8L;
                    Rectangle a = new Rectangle();
                    foreach (OLVListItem item in base.ListView.Items)
                    {
                        OLVListSubItem subItem = item.GetSubItem(index);
                        AnimationState animationState = subItem.AnimationState;
                        if ((animationState != null) && animationState.IsValid)
                        {
                            if (elapsedMilliseconds >= animationState.currentFrameExpiresAt)
                            {
                                animationState.AdvanceFrame(elapsedMilliseconds);
                                if (a.IsEmpty)
                                {
                                    a = subItem.Bounds;
                                }
                                else
                                {
                                    a = Rectangle.Union(a, subItem.Bounds);
                                }
                            }
                            num3 = Math.Min(num3, animationState.currentFrameExpiresAt);
                        }
                    }
                    if (!a.IsEmpty)
                    {
                        base.ListView.Invalidate(a);
                    }
                    this.tickler.Change((long) (num3 - elapsedMilliseconds), (long) (-1L));
                }
            }
        }

        public void Pause()
        {
            this.Paused = true;
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            if ((base.Aspect != null) && (base.Aspect != DBNull.Value))
            {
                if (base.Aspect is byte[])
                {
                    this.DrawAlignedImage(g, r, this.GetImageFromAspect());
                }
                else
                {
                    ICollection aspect = base.Aspect as ICollection;
                    if (aspect == null)
                    {
                        this.DrawAlignedImage(g, r, this.GetImageFromAspect());
                    }
                    else
                    {
                        this.DrawImages(g, r, aspect);
                    }
                }
            }
        }

        public void Unpause()
        {
            this.Paused = false;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Paused
        {
            get
            {
                return this.isPaused;
            }
            set
            {
                if (this.isPaused != value)
                {
                    this.isPaused = value;
                    if (this.isPaused)
                    {
                        this.tickler.Change(-1, -1);
                        this.stopwatch.Stop();
                    }
                    else
                    {
                        this.tickler.Change(1, -1);
                        this.stopwatch.Start();
                    }
                }
            }
        }

        internal class AnimationState
        {
            internal int currentFrame;
            internal long currentFrameExpiresAt;
            internal int frameCount;
            internal Image image;
            internal List<int> imageDuration;
            private const int PropertyTagFrameDelay = 0x5100;
            private const int PropertyTagLoopCount = 0x5101;
            private const int PropertyTagTypeLong = 4;
            private const int PropertyTagTypeShort = 3;

            public AnimationState()
            {
                this.imageDuration = new List<int>();
            }

            public AnimationState(Image image) : this()
            {
                if (IsAnimation(image))
                {
                    this.image = image;
                    this.frameCount = this.image.GetFrameCount(FrameDimension.Time);
                    foreach (PropertyItem item in this.image.PropertyItems)
                    {
                        if (item.Id == 0x5100)
                        {
                            for (int i = 0; i < item.Len; i += 4)
                            {
                                int num2 = (((item.Value[i + 3] << 0x18) + (item.Value[i + 2] << 0x10)) + (item.Value[i + 1] << 8)) + item.Value[i];
                                this.imageDuration.Add(num2 * 10);
                            }
                            break;
                        }
                    }
                    Debug.Assert(this.imageDuration.Count == this.frameCount, "There should be as many frame durations as there are frames.");
                }
            }

            public void AdvanceFrame(long millisecondsNow)
            {
                this.currentFrame = (this.currentFrame + 1) % this.frameCount;
                this.currentFrameExpiresAt = millisecondsNow + ((long) this.imageDuration[this.currentFrame]);
                this.image.SelectActiveFrame(FrameDimension.Time, this.currentFrame);
            }

            public static bool IsAnimation(Image image)
            {
                if (image == null)
                {
                    return false;
                }
                return new List<Guid>(image.FrameDimensionsList).Contains(FrameDimension.Time.Guid);
            }

            public bool IsValid
            {
                get
                {
                    return ((this.image != null) && (this.frameCount > 0));
                }
            }
        }
    }
}

