namespace BrightIdeasSoftware
{
    using System;

    [Flags]
    public enum GroupState
    {
        LVGS_ALL = 0xffff,
        LVGS_COLLAPSED = 1,
        LVGS_COLLAPSIBLE = 8,
        LVGS_FOCUSED = 0x10,
        LVGS_HIDDEN = 2,
        LVGS_NOHEADER = 4,
        LVGS_NORMAL = 0,
        LVGS_SELECTED = 0x20,
        LVGS_SUBSETED = 0x40,
        LVGS_SUBSETLINKFOCUSED = 0x80
    }
}

