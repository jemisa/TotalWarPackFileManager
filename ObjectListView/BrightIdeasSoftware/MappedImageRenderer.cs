namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Drawing;

    public class MappedImageRenderer : BaseRenderer
    {
        private Hashtable map;
        private object nullImage;

        public MappedImageRenderer()
        {
            this.map = new Hashtable();
        }

        public MappedImageRenderer(object[] keysAndImages) : this()
        {
            if ((keysAndImages.GetLength(0) % 2) != 0)
            {
                throw new ArgumentException("Array must have key/image pairs");
            }
            for (int i = 0; i < keysAndImages.GetLength(0); i += 2)
            {
                this.Add(keysAndImages[i], keysAndImages[i + 1]);
            }
        }

        public MappedImageRenderer(object key, object image) : this()
        {
            this.Add(key, image);
        }

        public MappedImageRenderer(object key1, object image1, object key2, object image2) : this()
        {
            this.Add(key1, image1);
            this.Add(key2, image2);
        }

        public void Add(object value, object image)
        {
            if (value == null)
            {
                this.nullImage = image;
            }
            else
            {
                this.map[value] = image;
            }
        }

        public static MappedImageRenderer Boolean(object trueImage, object falseImage)
        {
            return new MappedImageRenderer(true, trueImage, false, falseImage);
        }

        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            ICollection aspect = base.Aspect as ICollection;
            if (aspect == null)
            {
                this.RenderOne(g, r, base.Aspect);
            }
            else
            {
                this.RenderCollection(g, r, aspect);
            }
        }

        protected void RenderCollection(Graphics g, Rectangle r, ICollection imageSelectors)
        {
            ArrayList list = new ArrayList();
            Image image = null;
            foreach (object obj2 in imageSelectors)
            {
                if (obj2 == null)
                {
                    image = this.GetImage(this.nullImage);
                }
                else if (this.map.ContainsKey(obj2))
                {
                    image = this.GetImage(this.map[obj2]);
                }
                else
                {
                    image = null;
                }
                if (image != null)
                {
                    list.Add(image);
                }
            }
            this.DrawImages(g, r, list);
        }

        protected void RenderOne(Graphics g, Rectangle r, object selector)
        {
            Image image = null;
            if (selector == null)
            {
                image = this.GetImage(this.nullImage);
            }
            else if (this.map.ContainsKey(selector))
            {
                image = this.GetImage(this.map[selector]);
            }
            if (image != null)
            {
                this.DrawAlignedImage(g, r, image);
            }
        }

        public static MappedImageRenderer TriState(object trueImage, object falseImage, object nullImage)
        {
            object[] keysAndImages = new object[6];
            keysAndImages[0] = true;
            keysAndImages[1] = trueImage;
            keysAndImages[2] = false;
            keysAndImages[3] = falseImage;
            keysAndImages[5] = nullImage;
            return new MappedImageRenderer(keysAndImages);
        }
    }
}

