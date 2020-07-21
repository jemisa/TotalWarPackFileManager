namespace PackFileManager.Properties
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
    public sealed class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings) SettingsBase.Synchronized(new Settings()));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [DefaultSettingValue("False"), UserScopedSetting]
        public bool UseFirstColumnAsRowHeader
        {
            get
            {
                return (bool) this["UseFirstColumnAsRowHeader"];
            }
            set
            {
                this["UseFirstColumnAsRowHeader"] = value;
                Save();
            }
        }

        [DefaultSettingValue("True"), UserScopedSetting]
        public bool FirstStart
        {
            get
            {
                return (bool) this["FirstStart"];
            }
            set
            {
                this["FirstStart"] = value;
                Save();
            }
        }

        [DefaultSettingValue(""), UserScopedSetting]
        public string IgnoreColumns
        {
            get
            {
                return (string) this["IgnoreColumns"];
            }
            set
            {
                this["IgnoreColumns"] = value;
                Save();
            }
        }
        [DefaultSettingValue(""), UserScopedSetting]
        public string ImportExportDirectory {
            get {
                string result = (string)this["ImportExportDirectory"];
                if (ModManager.Instance.CurrentModSet) {
                    try {
                        result = ModManager.Instance.CurrentModDirectory;
                    } catch { }
                }
                return result;
            }
            set {
                this["ImportExportDirectory"] = value;
                Save();
            }
        }
        [DefaultSettingValue(""), UserScopedSetting]
        public string LastPackDirectory {
            get {
                string result = (string)this["LastPackDirectory"];
                if (ModManager.Instance.CurrentModSet) {
                    try {
                        result = ModManager.Instance.CurrentModDirectory;
                    } catch { }
                }
                return result;
            }
            set {
                this["LastPackDirectory"] = value;
                Save();
            }
        }
        [DefaultSettingValue("csv"), UserScopedSetting]
        public string TsvExtension {
            get {
                string result = (string)this["TsvExtension"];
                return result;
            }
            set {
                this["TsvExtension"] = value;
                Save();
            }
        }
        public string TsvFile(string baseFile) {
            return string.Format("{0}.{1}", baseFile, TsvExtension);
        }

        public bool IsColumnIgnored(string key, string columnName)
        {
            SortedSet<string> list;
            bool result = false;
            if (decode(IgnoreColumns).TryGetValue(key, out list))
            {
                result = list.Contains(columnName);
            }
            return result;
        }
        public void IgnoreColumn(string key, string column)
        {
            SortedSet<string> addTo = null;
            Dictionary<string, SortedSet<string>> dict = decode(IgnoreColumns);
            if (!dict.TryGetValue(key, out addTo))
            {
                addTo = new SortedSet<string>();
                dict.Add(key, addTo);
            }
            addTo.Add(column);
            IgnoreColumns = encode(dict);
        }
        public void UnignoreColumn(string key, string column)
        {
            SortedSet<string> removeFrom;
            Dictionary<string, SortedSet<string>> dict = decode(IgnoreColumns);
            if (dict.TryGetValue(key, out removeFrom)) {
                removeFrom.Remove(column);
                if (removeFrom.Count == 0)
                {
                    dict.Remove(key);
                }
            }
            IgnoreColumns = encode(dict);
        }
        public ICollection<string> IgnoredColumns(string key) {
            SortedSet<string> result;
            Dictionary<string, SortedSet<string>> dict = decode(IgnoreColumns);
            dict.TryGetValue(key, out result);
            return result != null ? result : new SortedSet<string>();
        }
        public void ResetIgnores(string key)
        {
            Dictionary<string, SortedSet<string>> dict = decode(IgnoreColumns);
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }
            IgnoreColumns = encode(dict);
        }
        private string encode(Dictionary<string, SortedSet<string>> encode)
        {
            string encoded = "";
            foreach(string key in encode.Keys) {
                SortedSet<string> value = encode[key];
                string entry = "";
                foreach (string e in value)
                {
                    entry += (entry == "") ? e : ";" + e;
                }
                encoded += string.Format("{0}:{1}\n", key, entry);
            }
            return encoded;
        }
        private Dictionary<string, SortedSet<string>> decode(string encoded)
        {
            Dictionary<string, SortedSet<string>> result = new Dictionary<string, SortedSet<string>>();
            string[] entries = encoded.Split('\n');
            foreach(string entry in entries) {
                try
                {
                    if (entry != "")
                    {
                        string[] split = entry.Split(':');
                        string[] items = split[1].Split(';');
                        result.Add(split[0], new SortedSet<string>(items));
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x);
                }
            }
            return result;
        }

        [DefaultSettingValue("False"), UserScopedSetting]
        public bool ShowAllColumns
        {
            get
            {
                return (bool)this["ShowAllColumns"];
            }
            set
            {
                this["ShowAllColumns"] = value;
                Save();
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue("True"), UserScopedSetting]
        public bool UseComboboxCells
        {
            get
            {
                return (bool)this["UseComboboxCells"];
            }
            set
            {
                this["UseComboboxCells"] = value;
                Save();
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue("True"), UserScopedSetting]
        public bool UpdateOnStartup
        {
            get
            {
                return (bool)this["UpdateOnStartup"];
            }
            set
            {
                this["UpdateOnStartup"] = value;
                Save();
            }
        }
        [DebuggerNonUserCode, DefaultSettingValue("False"), UserScopedSetting]
        public bool SubscribeToBetaSchema {
            get {
                return (bool)this["SubscribeToBetaSchema"];
            }
            set {
                this["SubscribeToBetaSchema"] = value;
                Save();
            }
        }

        [DebuggerNonUserCode, DefaultSettingValue("False"), UserScopedSetting]
        public bool ShowDecodeToolOnError {
            get {
                return (bool)this["ShowDecodeToolOnError"];
            }
            set {
                this["ShowDecodeToolOnError"] = value;
                Save();
            }
        }
    }
}

