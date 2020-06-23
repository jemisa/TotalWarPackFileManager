namespace Filetypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class AtlasFile
    {
        private List<AtlasObject> atlasObjects = new List<AtlasObject>();
        public int numEntries;

        public void add(AtlasObject newEntry)
        {
            this.atlasObjects.Add(newEntry);
            this.numEntries++;
        }

        public void removeAt(int index)
        {
            this.atlasObjects.RemoveAt(index);
            this.numEntries--;
        }

        public void replace(int index, AtlasObject newEntry)
        {
            this.atlasObjects[index] = newEntry;
        }

        public void resetEntries()
        {
            this.atlasObjects = new List<AtlasObject>();
            this.numEntries = 0;
        }

        public void setPixelUnits(float imageHeight)
        {
            foreach (AtlasObject obj2 in this.atlasObjects)
            {
                obj2.PX1 = obj2.X1 * 4096f;
                obj2.PY1 = obj2.Y1 * imageHeight;
                obj2.PX2 = obj2.X2 * 4096f;
                obj2.PY2 = obj2.Y2 * imageHeight;
            }
        }

        public List<AtlasObject> Entries
        {
            get
            {
                return this.atlasObjects;
            }
        }
    }
}

