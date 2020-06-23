namespace BrightIdeasSoftware
{
    using System;

    public interface ITransparentOverlay : IOverlay
    {
        int Transparency { get; set; }
    }
}

