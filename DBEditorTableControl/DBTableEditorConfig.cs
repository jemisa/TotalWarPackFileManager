using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DBTableControl
{
    public class DBTableEditorConfig
    {
        public bool FreezeKeyColumns { get; set; }

        public bool UseComboBoxes { get; set; }

        public bool ShowAllColumns { get; set; }

        public FakeDictionary<string, List<string>> HiddenColumns { get; set; }

        public string ImportDirectory { get; set; }

        public string ExportDirectory { get; set; }

        public bool ShowFilters { get; set; }

        public FakeDictionary<string, List<DBFilter>> Filters { get; set; }

        public DBTableEditorConfig()
        {
            FreezeKeyColumns = true;
            UseComboBoxes = true;
            ShowAllColumns = false;
            HiddenColumns = new FakeDictionary<string, List<string>>();
            ImportDirectory = "";
            ExportDirectory = "";
            ShowFilters = false;
            Filters = new FakeDictionary<string, List<DBFilter>>();
        }

        public void Load(string file = "Config\\DBTableEditorConfig.xml")
        {
            if (!Directory.Exists("Config"))
            {
                Directory.CreateDirectory("Config");
            }
            if (!File.Exists(file))
            {
                XmlTextWriter xw = new XmlTextWriter(File.Create(file), Encoding.UTF8);

                xw.WriteStartDocument();
                xw.WriteWhitespace("\n");
                xw.WriteStartElement("DBTableEditorConfig");
                xw.WriteWhitespace("\n\n");
                xw.WriteFullEndElement();
                xw.WriteEndDocument();

                xw.Close();
                return;
            }

            XmlTextReader xr = new XmlTextReader(file);

            while (xr.Read())
            {
                if (xr.NodeType == XmlNodeType.Element)
                {
                    if (xr.Name.Equals("FreezeKeyColumns"))
                    {
                        // Simple boolean element, read the value that follows and set property.
                        xr.Read();
                        if (xr.Value.Equals("True"))
                        {
                            FreezeKeyColumns = true;
                        }
                        else
                        {
                            FreezeKeyColumns = false;
                        }
                    }
                    else if (xr.Name.Equals("UseComboBoxes"))
                    {
                        // Simple boolean element, read the value that follows and set property.
                        xr.Read();
                        if (xr.Value.Equals("True"))
                        {
                            UseComboBoxes = true;
                        }
                        else
                        {
                            UseComboBoxes = false;
                        }
                    }
                    else if (xr.Name.Equals("ShowAllColumns"))
                    {
                        // Simple boolean element, read the value that follows and set property.
                        xr.Read();
                        if (xr.Value.Equals("True"))
                        {
                            ShowAllColumns = true;
                        }
                        else
                        {
                            ShowAllColumns = false;
                        }
                    }
                    else if (xr.Name.Equals("HiddenColumns"))
                    {
                        // Read until we hit the ending element
                        while (xr.Read())
                        {
                            if (xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("HiddenColumns"))
                            {
                                break;
                            }

                            // Skip all white space and other excess reads.
                            if (!xr.Name.Equals("Entry"))
                            {
                                continue;
                            }

                            // Save whether the current element is empty or not.
                            bool elementempty = xr.IsEmptyElement;

                            // Move to and pick up the key name from the elements attributes.
                            xr.MoveToNextAttribute();
                            string key = xr.Value;

                            List<string> columns = new List<string>();

                            // We have a list of columns, add them to the list.
                            if (!elementempty)
                            {
                                while (xr.Read())
                                {
                                    if (xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("Entry"))
                                    {
                                        break;
                                    }

                                    if (xr.NodeType != XmlNodeType.Element || !xr.Name.Equals("Value"))
                                    {
                                        continue;
                                    }

                                    // Move to column name and add it to the list.
                                    xr.Read();
                                    columns.Add(xr.Value);
                                }
                            }

                            HiddenColumns.Add(new KeyValuePair<string, List<string>>(key, columns));
                        }
                    }
                    else if (xr.Name.Equals("ImportDirectory"))
                    {
                        // Unused string.
                    }
                    else if (xr.Name.Equals("ExportDirectory"))
                    {
                        // Unused string.
                    }
                    else if (xr.Name.Equals("ShowFilters"))
                    {
                        // Simple boolean element, read the value that follows and set property.
                        xr.Read();
                        if (xr.Value.Equals("True"))
                        {
                            ShowFilters = true;
                        }
                        else
                        {
                            ShowFilters = false;
                        }
                    }
                    else if (xr.Name.Equals("Filters"))
                    {
                        // Read until we hit the ending element
                        while (xr.Read())
                        {
                            if (xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("Filters"))
                            {
                                break;
                            }

                            // Skip all white space and other excess reads.
                            if (!xr.Name.Equals("Entry"))
                            {
                                continue;
                            }

                            // Save whether the current element is empty or not.
                            bool elementempty = xr.IsEmptyElement;

                            // Move to and pick up the key name from the elements attributes.
                            xr.MoveToNextAttribute();
                            string key = xr.Value;

                            List<DBFilter> loadedfilters = new List<DBFilter>();

                            // We have a list of columns, add them to the list.
                            if (!elementempty)
                            {
                                while (xr.Read())
                                {
                                    if (xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("Entry"))
                                    {
                                        break;
                                    }

                                    if (xr.NodeType != XmlNodeType.Element || !xr.Name.Equals("Value"))
                                    {
                                        continue;
                                    }

                                    DBFilter dbf = new DBFilter();
                                    // Read through and add all filters for this table.
                                    while (xr.Read())
                                    {
                                        if (xr.NodeType == XmlNodeType.EndElement && xr.Name.Equals("Value"))
                                        {
                                            break;
                                        }

                                        if (xr.NodeType != XmlNodeType.Element)
                                        {
                                            continue;
                                        }

                                        if (xr.Name.Equals("IsActive"))
                                        {
                                            xr.Read();
                                            if (xr.Value.Equals("True"))
                                            {
                                                dbf.IsActive = true;
                                            }
                                            else
                                            {
                                                dbf.IsActive = false;
                                            }
                                        }
                                        else if (xr.Name.Equals("Name"))
                                        {
                                            xr.Read();
                                            dbf.Name = xr.Value;
                                        }
                                        else if (xr.Name.Equals("ApplyToColumn"))
                                        {
                                            xr.Read();
                                            dbf.ApplyToColumn = xr.Value;
                                        }
                                        else if (xr.Name.Equals("FilterValue"))
                                        {
                                            xr.Read();
                                            dbf.FilterValue = xr.Value;
                                        }
                                        else if (xr.Name.Equals("MatchMode"))
                                        {
                                            xr.Read();
                                            switch (xr.Value)
                                            {
                                                case "Exact":
                                                    dbf.MatchMode = MatchType.Exact;
                                                    break;
                                                case "Partial":
                                                    dbf.MatchMode = MatchType.Partial;
                                                    break;
                                                case "Regex":
                                                    dbf.MatchMode = MatchType.Regex;
                                                    break;
                                            }
                                        }
                                    }

                                    loadedfilters.Add(dbf);
                                }
                            }

                            Filters.Add(new KeyValuePair<string, List<DBFilter>>(key, loadedfilters));
                        }
                    }
                }
            }

            xr.Close();
        }

        public void Save(string file = "Config\\DBTableEditorConfig.xml")
        {
            try
            {
                XmlTextWriter xw = new XmlTextWriter(file, Encoding.UTF8);
                xw.Formatting = Formatting.Indented;
                xw.WriteStartDocument();

                // Write the root tags
                xw.WriteStartElement("DBTableEditorConfig");

                // Write boolean elements.
                xw.WriteElementString("FreezeKeyColumns", FreezeKeyColumns.ToString());
                xw.WriteElementString("UseComboBoxes", UseComboBoxes.ToString());
                xw.WriteElementString("ShowAllColumns", ShowAllColumns.ToString());
                xw.WriteElementString("ShowFilters", ShowFilters.ToString());

                // Write directory elements.
                xw.WriteElementString("ImportDirectory", ImportDirectory);
                xw.WriteElementString("ExportDirectory", ExportDirectory);

                // Write out Hidden Columns list.
                xw.WriteStartElement("HiddenColumns");
                foreach (KeyValuePair<string, List<string>> entry in HiddenColumns.Entries)
                {
                    xw.WriteStartElement("Entry");
                    xw.WriteAttributeString("Key", entry.Key);

                    foreach (string column in entry.Value)
                    {
                        xw.WriteElementString("Value", column);
                    }

                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                // Write out the Filters List.
                xw.WriteStartElement("Filters");
                foreach (KeyValuePair<string, List<DBFilter>> entry in Filters.Entries)
                {
                    xw.WriteStartElement("Entry");
                    xw.WriteAttributeString("Key", entry.Key);

                    foreach (DBFilter filter in entry.Value)
                    {
                        xw.WriteStartElement("Value");

                        xw.WriteElementString("IsActive", filter.IsActive.ToString());
                        xw.WriteElementString("Name", filter.Name);
                        xw.WriteElementString("ApplyToColumn", filter.ApplyToColumn);
                        xw.WriteElementString("FilterValue", filter.FilterValue);
                        xw.WriteElementString("MatchMode", filter.MatchMode.ToString());

                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                // Write the closing root tag.
                xw.WriteEndElement();
                xw.WriteEndDocument();
                xw.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }

    [Serializable]
    public class FakeDictionary<TKey, TValue>
    {
        public List<KeyValuePair<TKey, TValue>> Entries { get; set; }

        public FakeDictionary()
        {
            Entries = new List<KeyValuePair<TKey, TValue>>();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    return Entries[Entries.IndexOf(Entries.Single(n => n.Key.Equals(key)))].Value;
                }
                else
                {
                    throw new ArgumentException(String.Format("Dictionary {1} does not contain key: {0}", key, this));
                }
            }

            set
            {
                Entries[Entries.IndexOf(Entries.Single(n => n.Key.Equals(key)))].Value = value;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> newentry)
        {
            if (Entries.Any(n => n.Key.Equals(newentry.Key)))
            {
                throw new ArgumentException(String.Format("Dictionary {1} already contains key: {0}", newentry.Key, this));
            }
            else
            {
                Entries.Add(newentry);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return Entries.Any(n => n.Key.Equals(key));
        }

        public void Clear()
        {
            Entries.Clear();
        }


        public void Sort()
        {
            Entries.Sort();
        }
    }

    [Serializable]
    public class KeyValuePair<TKey, TValue> :IComparable
    {
        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public KeyValuePair()
        {

        }

        public KeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public int CompareTo(object tocompare)
        {
            if (tocompare is KeyValuePair<TKey, TValue>)
            {
                return CompareTo((KeyValuePair<TKey, TValue>)tocompare);
            }
            else
            {
                throw new ArgumentException("Object is not of the same type.");
            }
        }

        public int CompareTo(KeyValuePair<TKey, TValue> tocompare)
        {
            return Key.ToString().CompareTo(tocompare.Key.ToString());
        }
    }
}
