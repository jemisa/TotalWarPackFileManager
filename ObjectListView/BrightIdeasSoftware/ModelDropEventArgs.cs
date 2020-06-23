namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;

    public class ModelDropEventArgs : OlvDropEventArgs
    {
        private IList dragModels;
        private ObjectListView sourceListView;
        private object targetModel;
        private ArrayList toBeRefreshed = new ArrayList();

        public void RefreshObjects()
        {
            TreeListView sourceListView = this.SourceListView as TreeListView;
            if (sourceListView != null)
            {
                foreach (object obj2 in this.SourceModels)
                {
                    object parent = sourceListView.GetParent(obj2);
                    if (!this.toBeRefreshed.Contains(parent))
                    {
                        this.toBeRefreshed.Add(parent);
                    }
                }
            }
            this.toBeRefreshed.AddRange(this.SourceModels);
            if (base.ListView == this.SourceListView)
            {
                this.toBeRefreshed.Add(this.TargetModel);
                base.ListView.RefreshObjects(this.toBeRefreshed);
            }
            else
            {
                this.SourceListView.RefreshObjects(this.toBeRefreshed);
                base.ListView.RefreshObject(this.TargetModel);
            }
        }

        public ObjectListView SourceListView
        {
            get
            {
                return this.sourceListView;
            }
            internal set
            {
                this.sourceListView = value;
            }
        }

        public IList SourceModels
        {
            get
            {
                return this.dragModels;
            }
            internal set
            {
                this.dragModels = value;
                TreeListView sourceListView = this.SourceListView as TreeListView;
                if (sourceListView != null)
                {
                    foreach (object obj2 in this.SourceModels)
                    {
                        object parent = sourceListView.GetParent(obj2);
                        if (!this.toBeRefreshed.Contains(parent))
                        {
                            this.toBeRefreshed.Add(parent);
                        }
                    }
                }
            }
        }

        public object TargetModel
        {
            get
            {
                return this.targetModel;
            }
            internal set
            {
                this.targetModel = value;
            }
        }
    }
}

