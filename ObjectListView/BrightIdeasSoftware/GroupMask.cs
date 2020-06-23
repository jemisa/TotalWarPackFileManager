namespace BrightIdeasSoftware
{
    using System;

    [Flags]
    public enum GroupMask
    {
        LVGF_ALIGN = 8,
        LVGF_DESCRIPTIONBOTTOM = 0x800,
        LVGF_DESCRIPTIONTOP = 0x400,
        LVGF_EXTENDEDIMAGE = 0x2000,
        LVGF_FOOTER = 2,
        LVGF_GROUPID = 0x10,
        LVGF_HEADER = 1,
        LVGF_ITEMS = 0x4000,
        LVGF_NONE = 0,
        LVGF_STATE = 4,
        LVGF_SUBSET = 0x8000,
        LVGF_SUBSETITEMS = 0x10000,
        LVGF_SUBTITLE = 0x100,
        LVGF_TASK = 0x200,
        LVGF_TITLEIMAGE = 0x1000
    }
}

