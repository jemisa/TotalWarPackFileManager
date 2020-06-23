namespace BrightIdeasSoftware
{
    using System;
    using System.Drawing;

    public interface IItemStyle
    {
        Color BackColor { get; set; }

        System.Drawing.Font Font { get; set; }

        System.Drawing.FontStyle FontStyle { get; set; }

        Color ForeColor { get; set; }
    }
}

