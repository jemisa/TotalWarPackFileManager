namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    public class OLVColumnAttribute : Attribute
    {
        private string aspectToStringFormat;
        private bool checkBoxes;
        private int displayIndex;
        private bool fillsFreeSpace;
        private int? freeSpaceProportion;
        private object[] groupCutoffs;
        private string[] groupDescriptions;
        private string groupWithItemCountFormat;
        private string groupWithItemCountSingularFormat;
        private bool hyperlink;
        private string imageAspectName;
        private bool isEditable;
        internal bool IsEditableSet;
        private bool isTileViewColumn;
        private bool isVisible;
        private int maximumWidth;
        private int minimumWidth;
        private string tag;
        private HorizontalAlignment textAlign;
        private string title;
        private string toolTipText;
        private bool triStateCheckBoxes;
        private bool useInitialLetterForGroup;
        private int width;

        public OLVColumnAttribute()
        {
            this.isEditable = true;
            this.IsEditableSet = false;
            this.isVisible = true;
            this.maximumWidth = -1;
            this.minimumWidth = -1;
            this.textAlign = HorizontalAlignment.Left;
            this.width = 150;
        }

        public OLVColumnAttribute(string title)
        {
            this.isEditable = true;
            this.IsEditableSet = false;
            this.isVisible = true;
            this.maximumWidth = -1;
            this.minimumWidth = -1;
            this.textAlign = HorizontalAlignment.Left;
            this.width = 150;
            this.Title = title;
        }

        public string AspectToStringFormat
        {
            get
            {
                return this.aspectToStringFormat;
            }
            set
            {
                this.aspectToStringFormat = value;
            }
        }

        public bool CheckBoxes
        {
            get
            {
                return this.checkBoxes;
            }
            set
            {
                this.checkBoxes = value;
            }
        }

        public int DisplayIndex
        {
            get
            {
                return this.displayIndex;
            }
            set
            {
                this.displayIndex = value;
            }
        }

        public bool FillsFreeSpace
        {
            get
            {
                return this.fillsFreeSpace;
            }
            set
            {
                this.fillsFreeSpace = value;
            }
        }

        public int? FreeSpaceProportion
        {
            get
            {
                return this.freeSpaceProportion;
            }
            set
            {
                this.freeSpaceProportion = value;
            }
        }

        public object[] GroupCutoffs
        {
            get
            {
                return this.groupCutoffs;
            }
            set
            {
                this.groupCutoffs = value;
            }
        }

        public string[] GroupDescriptions
        {
            get
            {
                return this.groupDescriptions;
            }
            set
            {
                this.groupDescriptions = value;
            }
        }

        public string GroupWithItemCountFormat
        {
            get
            {
                return this.groupWithItemCountFormat;
            }
            set
            {
                this.groupWithItemCountFormat = value;
            }
        }

        public string GroupWithItemCountSingularFormat
        {
            get
            {
                return this.groupWithItemCountSingularFormat;
            }
            set
            {
                this.groupWithItemCountSingularFormat = value;
            }
        }

        public bool Hyperlink
        {
            get
            {
                return this.hyperlink;
            }
            set
            {
                this.hyperlink = value;
            }
        }

        public string ImageAspectName
        {
            get
            {
                return this.imageAspectName;
            }
            set
            {
                this.imageAspectName = value;
            }
        }

        public bool IsEditable
        {
            get
            {
                return this.isEditable;
            }
            set
            {
                this.isEditable = value;
                this.IsEditableSet = true;
            }
        }

        public bool IsTileViewColumn
        {
            get
            {
                return this.isTileViewColumn;
            }
            set
            {
                this.isTileViewColumn = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }
            set
            {
                this.isVisible = value;
            }
        }

        public int MaximumWidth
        {
            get
            {
                return this.maximumWidth;
            }
            set
            {
                this.maximumWidth = value;
            }
        }

        public int MinimumWidth
        {
            get
            {
                return this.minimumWidth;
            }
            set
            {
                this.minimumWidth = value;
            }
        }

        public string Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        public HorizontalAlignment TextAlign
        {
            get
            {
                return this.textAlign;
            }
            set
            {
                this.textAlign = value;
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        public string ToolTipText
        {
            get
            {
                return this.toolTipText;
            }
            set
            {
                this.toolTipText = value;
            }
        }

        public bool TriStateCheckBoxes
        {
            get
            {
                return this.triStateCheckBoxes;
            }
            set
            {
                this.triStateCheckBoxes = value;
            }
        }

        public bool UseInitialLetterForGroup
        {
            get
            {
                return this.useInitialLetterForGroup;
            }
            set
            {
                this.useInitialLetterForGroup = value;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
    }
}

