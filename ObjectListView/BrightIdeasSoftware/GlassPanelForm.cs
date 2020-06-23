namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    public class GlassPanelForm : Form
    {
        private bool isDuringResizeSequence;
        private bool isGlassShown;
        private ObjectListView objectListView;
        internal IOverlay Overlay;
        private bool wasGlassShownBeforeResize;

        public void Bind(ObjectListView olv, IOverlay overlay)
        {
            if (this.objectListView != null)
            {
                this.Unbind();
            }
            this.objectListView = olv;
            this.Overlay = overlay;
            this.objectListView.LocationChanged += new EventHandler(this.objectListView_LocationChanged);
            this.objectListView.SizeChanged += new EventHandler(this.objectListView_SizeChanged);
            this.objectListView.VisibleChanged += new EventHandler(this.objectListView_VisibleChanged);
            this.objectListView.ParentChanged += new EventHandler(this.objectListView_ParentChanged);
            for (Control control = this.objectListView.Parent; control != null; control = control.Parent)
            {
                control.ParentChanged += new EventHandler(this.objectListView_ParentChanged);
                TabControl control2 = control as TabControl;
                if (control2 != null)
                {
                    control2.Selected += new TabControlEventHandler(this.tabControl_Selected);
                }
            }
            base.Owner = this.objectListView.TopLevelControl as Form;
            if (base.Owner != null)
            {
                base.Owner.LocationChanged += new EventHandler(this.Owner_LocationChanged);
                base.Owner.SizeChanged += new EventHandler(this.Owner_SizeChanged);
                base.Owner.ResizeBegin += new EventHandler(this.Owner_ResizeBegin);
                base.Owner.ResizeEnd += new EventHandler(this.Owner_ResizeEnd);
                if (base.Owner.TopMost)
                {
                    BrightIdeasSoftware.NativeMethods.MakeTopMost(this);
                }
            }
            this.UpdateTransparency();
        }

        public void HideGlass()
        {
            if (this.isGlassShown)
            {
                this.isGlassShown = false;
                base.Bounds = new Rectangle(-10000, -10000, 1, 1);
            }
        }

        private void objectListView_LocationChanged(object sender, EventArgs e)
        {
            if (this.isGlassShown)
            {
                this.RecalculateBounds();
            }
        }

        private void objectListView_ParentChanged(object sender, EventArgs e)
        {
            ObjectListView objectListView = this.objectListView;
            IOverlay overlay = this.Overlay;
            this.Unbind();
            this.Bind(objectListView, overlay);
        }

        private void objectListView_SizeChanged(object sender, EventArgs e)
        {
        }

        private void objectListView_VisibleChanged(object sender, EventArgs e)
        {
            if (this.objectListView.Visible)
            {
                this.ShowGlass();
            }
            else
            {
                this.HideGlass();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if ((this.objectListView != null) && (this.Overlay != null))
            {
                Graphics g = e.Graphics;
                g.TextRenderingHint = ObjectListView.TextRendereringHint;
                g.SmoothingMode = SmoothingMode.HighQuality;
                this.Overlay.Draw(this.objectListView, g, this.objectListView.ClientRectangle);
            }
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            this.RecalculateBounds();
        }

        private void Owner_ResizeBegin(object sender, EventArgs e)
        {
            this.isDuringResizeSequence = true;
            this.wasGlassShownBeforeResize = this.isGlassShown;
        }

        private void Owner_ResizeEnd(object sender, EventArgs e)
        {
            this.isDuringResizeSequence = false;
            if (this.wasGlassShownBeforeResize)
            {
                this.ShowGlass();
            }
        }

        private void Owner_SizeChanged(object sender, EventArgs e)
        {
            this.HideGlass();
        }

        protected void RecalculateBounds()
        {
            if (this.isGlassShown)
            {
                Rectangle clientRectangle = this.objectListView.ClientRectangle;
                clientRectangle.X = 0;
                clientRectangle.Y = 0;
                base.Bounds = this.objectListView.RectangleToScreen(clientRectangle);
            }
        }

        public void ShowGlass()
        {
            if (!this.isGlassShown && !this.isDuringResizeSequence)
            {
                this.isGlassShown = true;
                this.RecalculateBounds();
            }
        }

        private void tabControl_Selected(object sender, TabControlEventArgs e)
        {
            this.HideGlass();
        }

        public void Unbind()
        {
            if (this.objectListView != null)
            {
                this.objectListView.SizeChanged -= new EventHandler(this.objectListView_SizeChanged);
                this.objectListView.VisibleChanged -= new EventHandler(this.objectListView_VisibleChanged);
                this.objectListView.ParentChanged -= new EventHandler(this.objectListView_ParentChanged);
                for (Control control = this.objectListView.Parent; control != null; control = control.Parent)
                {
                    control.ParentChanged -= new EventHandler(this.objectListView_ParentChanged);
                    TabControl control2 = control as TabControl;
                    if (control2 != null)
                    {
                        control2.Selected -= new TabControlEventHandler(this.tabControl_Selected);
                    }
                }
                base.Owner = this.objectListView.TopLevelControl as Form;
                if (base.Owner != null)
                {
                    base.Owner.LocationChanged -= new EventHandler(this.Owner_LocationChanged);
                    base.Owner.SizeChanged -= new EventHandler(this.Owner_SizeChanged);
                    base.Owner.ResizeBegin -= new EventHandler(this.Owner_ResizeBegin);
                    base.Owner.ResizeEnd -= new EventHandler(this.Owner_ResizeEnd);
                }
                this.objectListView = null;
            }
        }

        internal void UpdateTransparency()
        {
            ITransparentOverlay overlay = this.Overlay as ITransparentOverlay;
            if (overlay == null)
            {
                base.Opacity = ((float) this.objectListView.OverlayTransparency) / 255f;
            }
            else
            {
                base.Opacity = ((float) overlay.Transparency) / 255f;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                m.Result = (IntPtr) (-1);
            }
            base.WndProc(ref m);
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x20;
                return createParams;
            }
        }
    }
}

