namespace BrightIdeasSoftware
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;

    public class FlagRenderer : BaseRenderer
    {
        private Dictionary<int, object> imageMap = new Dictionary<int, object>();
        private List<int> keysInOrder = new List<int>();

        public void Add(object key, object imageSelector)
        {
            int item = ((IConvertible) key).ToInt32(NumberFormatInfo.InvariantInfo);
            this.imageMap[item] = imageSelector;
            this.keysInOrder.Remove(item);
            this.keysInOrder.Add(item);
        }

        protected override void HandleHitTest(Graphics g, OlvListViewHitTestInfo hti, int x, int y)
        {
            IConvertible aspect = base.Aspect as IConvertible;
            if (aspect != null)
            {
                int num = aspect.ToInt32(NumberFormatInfo.InvariantInfo);
                Point location = base.Bounds.Location;
                foreach (int num2 in this.keysInOrder)
                {
                    if ((num & num2) == num2)
                    {
                        Image image = this.GetImage(this.imageMap[num2]);
                        if (image != null)
                        {
                            Rectangle rectangle = new Rectangle(location, image.Size);
                            if (rectangle.Contains(x, y))
                            {
                                hti.UserData = num2;
                                break;
                            }
                            location.X += image.Width + base.Spacing;
                        }
                    }
                }
            }
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            IConvertible aspect = base.Aspect as IConvertible;
            if (aspect != null)
            {
                int num = aspect.ToInt32(NumberFormatInfo.InvariantInfo);
                Point location = r.Location;
                foreach (int num2 in this.keysInOrder)
                {
                    if ((num & num2) == num2)
                    {
                        Image image = this.GetImage(this.imageMap[num2]);
                        if (image != null)
                        {
                            g.DrawImage(image, location);
                            location.X += image.Width + base.Spacing;
                        }
                    }
                }
            }
        }
    }
}

