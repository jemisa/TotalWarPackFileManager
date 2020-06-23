namespace BrightIdeasSoftware
{
    using System.Drawing;

    public class CellBorderDecoration : BorderDecoration
    {
        protected override Rectangle CalculateBounds()
        {
            return base.CellBounds;
        }
    }
}

