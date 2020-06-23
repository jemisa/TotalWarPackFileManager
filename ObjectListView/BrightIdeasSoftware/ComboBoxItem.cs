namespace BrightIdeasSoftware
{
    using System;

    internal class ComboBoxItem
    {
        private string description;
        private object key;

        public ComboBoxItem(object key, string description)
        {
            this.key = key;
            this.description = description;
        }

        public override string ToString()
        {
            return this.description;
        }

        public object Key
        {
            get
            {
                return this.key;
            }
        }
    }
}

