namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Forms;

    [Browsable(false)]
    public class OLVListSubItem : ListViewItem.ListViewSubItem
    {
        internal BrightIdeasSoftware.ImageRenderer.AnimationState AnimationState;
        private IList<IDecoration> decorations;
        private object imageSelector;
        private string url;

        public OLVListSubItem()
        {
        }

        public OLVListSubItem(string text, object image)
        {
            base.Text = text;
            this.ImageSelector = image;
        }

        public IDecoration Decoration
        {
            get
            {
                if (this.HasDecoration)
                {
                    return this.Decorations[0];
                }
                return null;
            }
            set
            {
                this.Decorations.Clear();
                if (value != null)
                {
                    this.Decorations.Add(value);
                }
            }
        }

        public IList<IDecoration> Decorations
        {
            get
            {
                if (this.decorations == null)
                {
                    this.decorations = new List<IDecoration>();
                }
                return this.decorations;
            }
        }

        public bool HasDecoration
        {
            get
            {
                return ((this.decorations != null) && (this.decorations.Count > 0));
            }
        }

        public object ImageSelector
        {
            get
            {
                return this.imageSelector;
            }
            set
            {
                this.imageSelector = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }
    }
}

