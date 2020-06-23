namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class OLVGroup
    {
        private string bottomDescription;
        private IList contents;
        private object extendedImage;
        private string footer;
        private static PropertyInfo groupIdPropInfo;
        private string header;
        private HorizontalAlignment headerAlignment;
        private int id;
        private IList<OLVListItem> items;
        private object key;
        private ObjectListView listView;
        private System.Windows.Forms.ListViewGroup listViewGroup;
        private string name;
        private static int nextId;
        private IComparable sortValue;
        private GroupState state;
        private GroupState stateMask;
        private string subsetTitle;
        private string subtitle;
        private object tag;
        private string task;
        private object titleImage;
        private string topDescription;
        private int virtualItemCount;

        public OLVGroup() : this("Default group header")
        {
        }

        public OLVGroup(string header)
        {
            this.items = new List<OLVListItem>();
            this.Header = header;
            this.Id = nextId++;
            this.TitleImage = -1;
            this.ExtendedImage = -1;
        }

        internal BrightIdeasSoftware.NativeMethods.LVGROUP2 AsNativeGroup(bool withId)
        {
            BrightIdeasSoftware.NativeMethods.LVGROUP2 lvgroup = new BrightIdeasSoftware.NativeMethods.LVGROUP2 {
                cbSize = (uint) Marshal.SizeOf(typeof(BrightIdeasSoftware.NativeMethods.LVGROUP2)),
                mask = 13,
                pszHeader = this.Header,
                uAlign = (uint) this.HeaderAlignment,
                stateMask = (uint) this.StateMask,
                state = (uint) this.State
            };
            if (withId)
            {
                lvgroup.iGroupId = this.GroupId;
                lvgroup.mask ^= 0x10;
            }
            if (!string.IsNullOrEmpty(this.Footer))
            {
                lvgroup.pszFooter = this.Footer;
                lvgroup.mask ^= 2;
            }
            if (!string.IsNullOrEmpty(this.Subtitle))
            {
                lvgroup.pszSubtitle = this.Subtitle;
                lvgroup.mask ^= 0x100;
            }
            if (!string.IsNullOrEmpty(this.Task))
            {
                lvgroup.pszTask = this.Task;
                lvgroup.mask ^= 0x200;
            }
            if (!string.IsNullOrEmpty(this.TopDescription))
            {
                lvgroup.pszDescriptionTop = this.TopDescription;
                lvgroup.mask ^= 0x400;
            }
            if (!string.IsNullOrEmpty(this.BottomDescription))
            {
                lvgroup.pszDescriptionBottom = this.BottomDescription;
                lvgroup.mask ^= 0x800;
            }
            int imageIndex = this.GetImageIndex(this.TitleImage);
            if (imageIndex >= 0)
            {
                lvgroup.iTitleImage = imageIndex;
                lvgroup.mask ^= 0x1000;
            }
            imageIndex = this.GetImageIndex(this.ExtendedImage);
            if (imageIndex >= 0)
            {
                lvgroup.iExtendedImage = imageIndex;
                lvgroup.mask ^= 0x2000;
            }
            if (!string.IsNullOrEmpty(this.SubsetTitle))
            {
                lvgroup.pszSubsetTitle = this.SubsetTitle;
                lvgroup.mask ^= 0x8000;
            }
            if (this.VirtualItemCount > 0)
            {
                lvgroup.cItems = this.VirtualItemCount;
                lvgroup.mask ^= 0x4000;
            }
            return lvgroup;
        }

        public int GetImageIndex(object imageSelector)
        {
            if (((imageSelector != null) && (this.ListView != null)) && (this.ListView.GroupImageList != null))
            {
                if (imageSelector is int)
                {
                    return (int) imageSelector;
                }
                string key = imageSelector as string;
                if (key != null)
                {
                    return this.ListView.GroupImageList.Images.IndexOfKey(key);
                }
            }
            return -1;
        }

        private bool GetOneState(GroupState stateMask)
        {
            if (this.Created)
            {
                this.State = this.GetState();
            }
            return ((this.State & stateMask) == stateMask);
        }

        protected GroupState GetState()
        {
            return BrightIdeasSoftware.NativeMethods.GetGroupState(this.ListView, this.GroupId, GroupState.LVGS_ALL);
        }

        public void InsertGroupNewStyle(ObjectListView olv)
        {
            this.ListView = olv;
            BrightIdeasSoftware.NativeMethods.InsertGroup(olv, this.AsNativeGroup(true));
            this.SetGroupSpacing();
        }

        public void InsertGroupOldStyle(ObjectListView olv)
        {
            this.ListView = olv;
            if (this.ListViewGroup == null)
            {
                this.ListViewGroup = new System.Windows.Forms.ListViewGroup();
            }
            this.ListViewGroup.Header = this.Header;
            this.ListViewGroup.HeaderAlignment = this.HeaderAlignment;
            this.ListViewGroup.Name = this.Name;
            this.ListViewGroup.Tag = this.Tag;
            olv.Groups.Add(this.ListViewGroup);
            BrightIdeasSoftware.NativeMethods.SetGroupInfo(olv, this.GroupId, this.AsNativeGroup(false));
            this.SetGroupSpacing();
        }

        protected int SetGroupSpacing()
        {
            if (this.ListView.SpaceBetweenGroups <= 0)
            {
                return 0;
            }
            BrightIdeasSoftware.NativeMethods.LVGROUPMETRICS metrics = new BrightIdeasSoftware.NativeMethods.LVGROUPMETRICS {
                cbSize = (uint) Marshal.SizeOf(typeof(BrightIdeasSoftware.NativeMethods.LVGROUPMETRICS)),
                mask = 1,
                Bottom = (uint) this.ListView.SpaceBetweenGroups
            };
            return BrightIdeasSoftware.NativeMethods.SetGroupMetrics(this.ListView, this.GroupId, metrics);
        }

        public void SetItemsOldStyle()
        {
            List<OLVListItem> items = this.Items as List<OLVListItem>;
            if (items == null)
            {
                foreach (OLVListItem item in this.Items)
                {
                    this.ListViewGroup.Items.Add(item);
                }
            }
            else
            {
                this.ListViewGroup.Items.AddRange(items.ToArray());
            }
        }

        private void SetOneState(bool value, GroupState stateMask)
        {
            this.StateMask ^= stateMask;
            if (value)
            {
                this.State ^= stateMask;
            }
            else
            {
                this.State &= ~stateMask;
            }
            if (this.Created)
            {
                this.SetState(this.State, stateMask);
            }
        }

        protected int SetState(GroupState state, GroupState stateMask)
        {
            BrightIdeasSoftware.NativeMethods.LVGROUP2 group = new BrightIdeasSoftware.NativeMethods.LVGROUP2 {
                cbSize = (uint) Marshal.SizeOf(typeof(BrightIdeasSoftware.NativeMethods.LVGROUP2)),
                mask = 4,
                state = (uint) state,
                stateMask = (uint) stateMask
            };
            return BrightIdeasSoftware.NativeMethods.SetGroupInfo(this.ListView, this.GroupId, group);
        }

        public override string ToString()
        {
            return this.Header;
        }

        public string BottomDescription
        {
            get
            {
                return this.bottomDescription;
            }
            set
            {
                this.bottomDescription = value;
            }
        }

        public bool Collapsed
        {
            get
            {
                return this.GetOneState(GroupState.LVGS_COLLAPSED);
            }
            set
            {
                this.SetOneState(value, GroupState.LVGS_COLLAPSED);
            }
        }

        public bool Collapsible
        {
            get
            {
                return this.GetOneState(GroupState.LVGS_COLLAPSIBLE);
            }
            set
            {
                this.SetOneState(value, GroupState.LVGS_COLLAPSIBLE);
            }
        }

        public IList Contents
        {
            get
            {
                return this.contents;
            }
            set
            {
                this.contents = value;
            }
        }

        public bool Created
        {
            get
            {
                return (this.ListView != null);
            }
        }

        public object ExtendedImage
        {
            get
            {
                return this.extendedImage;
            }
            set
            {
                this.extendedImage = value;
            }
        }

        public string Footer
        {
            get
            {
                return this.footer;
            }
            set
            {
                this.footer = value;
            }
        }

        public int GroupId
        {
            get
            {
                if (this.ListViewGroup == null)
                {
                    return this.Id;
                }
                if (groupIdPropInfo == null)
                {
                    groupIdPropInfo = typeof(System.Windows.Forms.ListViewGroup).GetProperty("ID", BindingFlags.NonPublic | BindingFlags.Instance);
                    Debug.Assert(groupIdPropInfo != null);
                }
                int? nullable = groupIdPropInfo.GetValue(this.ListViewGroup, null) as int?;
                if (nullable.HasValue)
                {
                    return nullable.Value;
                }
                return -1;
            }
        }

        public string Header
        {
            get
            {
                return this.header;
            }
            set
            {
                this.header = value;
            }
        }

        public HorizontalAlignment HeaderAlignment
        {
            get
            {
                return this.headerAlignment;
            }
            set
            {
                this.headerAlignment = value;
            }
        }

        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public IList<OLVListItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
            }
        }

        public object Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        public ObjectListView ListView
        {
            get
            {
                return this.listView;
            }
            protected set
            {
                this.listView = value;
            }
        }

        protected System.Windows.Forms.ListViewGroup ListViewGroup
        {
            get
            {
                return this.listViewGroup;
            }
            set
            {
                this.listViewGroup = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public IComparable SortValue
        {
            get
            {
                return this.sortValue;
            }
            set
            {
                this.sortValue = value;
            }
        }

        public GroupState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public GroupState StateMask
        {
            get
            {
                return this.stateMask;
            }
            set
            {
                this.stateMask = value;
            }
        }

        public bool Subseted
        {
            get
            {
                return this.GetOneState(GroupState.LVGS_SUBSETED);
            }
            set
            {
                this.SetOneState(value, GroupState.LVGS_SUBSETED);
            }
        }

        public string SubsetTitle
        {
            get
            {
                return this.subsetTitle;
            }
            set
            {
                this.subsetTitle = value;
            }
        }

        public string Subtitle
        {
            get
            {
                return this.subtitle;
            }
            set
            {
                this.subtitle = value;
            }
        }

        public object Tag
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

        public string Task
        {
            get
            {
                return this.task;
            }
            set
            {
                this.task = value;
            }
        }

        public object TitleImage
        {
            get
            {
                return this.titleImage;
            }
            set
            {
                this.titleImage = value;
            }
        }

        public string TopDescription
        {
            get
            {
                return this.topDescription;
            }
            set
            {
                this.topDescription = value;
            }
        }

        public int VirtualItemCount
        {
            get
            {
                return this.virtualItemCount;
            }
            set
            {
                this.virtualItemCount = value;
            }
        }
    }
}

