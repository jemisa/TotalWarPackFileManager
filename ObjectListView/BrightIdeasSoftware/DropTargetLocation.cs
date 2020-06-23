namespace BrightIdeasSoftware
{
    using System;

    [Flags]
    public enum DropTargetLocation
    {
        AboveItem = 8,
        Background = 1,
        BelowItem = 0x10,
        BetweenItems = 4,
        Item = 2,
        LeftOfItem = 0x80,
        None = 0,
        RightOfItem = 0x40,
        SubItem = 0x20
    }
}

