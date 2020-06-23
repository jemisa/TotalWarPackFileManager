namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    internal class NullableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private bool hasNullKey;
        private TValue nullValue;

        public bool ContainsKey(TKey key)
        {
            if (key == null)
            {
                return this.hasNullKey;
            }
            return base.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    if (!this.hasNullKey)
                    {
                        throw new KeyNotFoundException();
                    }
                    return this.nullValue;
                }
                return base[key];
            }
            set
            {
                if (key == null)
                {
                    this.hasNullKey = true;
                    this.nullValue = value;
                }
                else
                {
                    base[key] = value;
                }
            }
        }

        public IList Keys
        {
            get
            {
                ArrayList list = new ArrayList(base.Keys);
                if (this.hasNullKey)
                {
                    list.Add(null);
                }
                return list;
            }
        }
    }
}

