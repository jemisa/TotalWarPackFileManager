namespace Filetypes
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class LocFile
    {
		public string Name { get; set; }

		public List<LocEntry> Entries {
			get;
			private set;
		}
        public int NumEntries {
			get {
				return Entries.Count;
			}
		}
		
		public LocFile () {
			Entries = new List<LocEntry> ();
		}

		#region CSV export
        static char[] TABS = { '\t' };
        
        public void Export(StreamWriter writer) {
			for (int j = 0; j < NumEntries; j++) {
                LocEntry entry = Entries[j];
                string str = CsvUtil.Format (entry[0]);
                for (int i = 1; i < 3; i++) {
                    string current = entry[i];
                    str += "\t" + CsvUtil.Format (current);
                }
                writer.WriteLine (str);
			}
		}

        public void Import(StreamReader reader) {			
			Entries.Clear ();
			while (!reader.EndOfStream) {
                try {
    				string str = reader.ReadLine ();
    				if (str.Trim () != "") {
                        string[] strArray = str.Split (TABS, StringSplitOptions.None);
                        if (strArray.Length != 3) {
                            continue;
                        }
                        List<string> imported = new List<string>(3);
    
                        for (int i = 0; i < 3; i++) {
                            string str3 = CsvUtil.Unformat (strArray [i]);
                            imported.Add(str3);
                        }
    					Entries.Add (new LocEntry(imported[0], imported[1], Boolean.Parse(imported[2])));
    				}
                } catch {}
			}
		}
		#endregion
    }

    public class LocEntry {
		public string Localised { get; set; }
		public string Tag { get; set; }
		public bool Tooltip { get; set; }
        
        public string this[int index] {
            get {
                switch(index) {
                case 0:
                    return Tag;
                case 1:
                    return Localised;
                case 2:
                    return Tooltip.ToString();
                default:
                    break;
                }
                throw new IndexOutOfRangeException();
            }
        }

		public LocEntry (string tag, string localised, bool tooltip) {
			this.Tag = tag;
			this.Localised = localised;
			this.Tooltip = tooltip;
		}
	}
}

