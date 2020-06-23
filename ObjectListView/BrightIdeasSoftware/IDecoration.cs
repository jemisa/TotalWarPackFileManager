namespace BrightIdeasSoftware
{
    using System;

    public interface IDecoration : IOverlay
    {
        OLVListItem ListItem { get; set; }

        OLVListSubItem SubItem { get; set; }
    }
}

