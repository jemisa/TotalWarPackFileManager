namespace BrightIdeasSoftware
{
    using System;

    public class IsHyperlinkEventArgs : EventArgs
    {
        private OLVColumn column;
        private ObjectListView listView;
        private object model;
        private string text;
        public string Url;

        public OLVColumn Column
        {
            get
            {
                return this.column;
            }
            internal set
            {
                this.column = value;
            }
        }

        public ObjectListView ListView
        {
            get
            {
                return this.listView;
            }
            internal set
            {
                this.listView = value;
            }
        }

        public object Model
        {
            get
            {
                return this.model;
            }
            internal set
            {
                this.model = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            internal set
            {
                this.text = value;
            }
        }
    }
}

