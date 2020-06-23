namespace Filetypes
{
    using System;

    public class AtlasObject
    {
        private string container1;
        private string container2;
        private float px1;
        private float px2;
        private float py1;
        private float py2;
        private float x1;
        private float x2;
        private float x3;
        private float y1;
        private float y2;
        private float y3;

        public AtlasObject()
        {
            this.container1 = string.Empty;
            this.container2 = string.Empty;
            this.x1 = 0f;
            this.y1 = 0f;
            this.x2 = 0f;
            this.y2 = 0f;
            this.x3 = 0f;
            this.y3 = 0f;
            this.px1 = 0f;
            this.py1 = 0f;
            this.px2 = 0f;
            this.py2 = 0f;
        }

        public AtlasObject(string container1, string container2, float[] coordinates, float[] pCoordinates)
        {
            this.container1 = container1;
            this.container2 = container2;
            this.x1 = coordinates[0];
            this.y1 = coordinates[1];
            this.x2 = coordinates[2];
            this.y2 = coordinates[3];
            this.x3 = coordinates[4];
            this.y3 = coordinates[5];
            this.px1 = pCoordinates[0];
            this.py1 = pCoordinates[1];
            this.px2 = pCoordinates[2];
            this.py2 = pCoordinates[3];
        }

        public string Container1
        {
            get
            {
                return this.container1;
            }
            set
            {
                this.container1 = value;
            }
        }

        public string Container2
        {
            get
            {
                return this.container2;
            }
            set
            {
                this.container2 = value;
            }
        }

        public float PX1
        {
            get
            {
                return this.px1;
            }
            set
            {
                this.px1 = value;
            }
        }

        public float PX2
        {
            get
            {
                return this.px2;
            }
            set
            {
                this.px2 = value;
            }
        }

        public float PY1
        {
            get
            {
                return this.py1;
            }
            set
            {
                this.py1 = value;
            }
        }

        public float PY2
        {
            get
            {
                return this.py2;
            }
            set
            {
                this.py2 = value;
            }
        }

        public float X1
        {
            get
            {
                return this.x1;
            }
            set
            {
                this.x1 = value;
            }
        }

        public float X2
        {
            get
            {
                return this.x2;
            }
            set
            {
                this.x2 = value;
            }
        }

        public float X3
        {
            get
            {
                return this.x3;
            }
            set
            {
                this.x3 = value;
            }
        }

        public float Y1
        {
            get
            {
                return this.y1;
            }
            set
            {
                this.y1 = value;
            }
        }

        public float Y2
        {
            get
            {
                return this.y2;
            }
            set
            {
                this.y2 = value;
            }
        }

        public float Y3
        {
            get
            {
                return this.y3;
            }
            set
            {
                this.y3 = value;
            }
        }
    }
}

