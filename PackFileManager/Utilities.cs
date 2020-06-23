using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Common;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PackFileManager {
    public static class Utilities {
        public static void DisposeHandlers(Control targetControl) {
			EventInfo[] events = targetControl.GetType ().GetEvents ();
			if (events.Length > 0) {
				foreach (EventInfo info in events) {
					MethodInfo raiseMethod = info.GetRaiseMethod ();
					if (raiseMethod != null) {
						Delegate handler = Delegate.CreateDelegate (typeof(EventHandler), targetControl, raiseMethod.Name, false);
						info.RemoveEventHandler (targetControl, handler);
					}
				}
			}
			Control.ControlCollection controls = targetControl.Controls;
			if (controls.Count > 0) {
				foreach (Control control in controls) {
					DisposeHandlers (control);
				}
			}
		}
	}
    
    class NumberedFileComparator : IComparer<string> {
        static readonly Regex NumberedFileNameRE = new Regex("([^0-9]*)([0-9]+).*");
        public static readonly NumberedFileComparator Instance = new NumberedFileComparator();

        public int Compare(string name1, string name2) {
            name1 = Path.GetFileName(name1);
            name2 = Path.GetFileName(name2);
            int result = name1.CompareTo(name2);
            try {
                if (NumberedFileNameRE.IsMatch(name1) && NumberedFileNameRE.IsMatch(name2)) {
                    Match m1 = NumberedFileNameRE.Match(name1);
                    Match m2 = NumberedFileNameRE.Match(name2);
                    if (m1.Groups[1].Value.Equals(m2.Groups[1].Value)) {
                        int number1 = int.Parse(m1.Groups[2].Value);
                        int number2 = int.Parse(m2.Groups[2].Value);
                        result = number2 - number1;
                    }
                }
            } catch {} // we don't really care; if we can't parse we'll just use the alphanum comparison
            return result;
        }
    }

    class LoadUpdater 
    {
        private readonly string file;
        private int currentCount;
        private uint count;
        private readonly ToolStripLabel label;
        private readonly ToolStripProgressBar progress;
        PackFileCodec currentCodec;
        public LoadUpdater(PackFileCodec codec, string f, ToolStripLabel l, ToolStripProgressBar bar) 
        {
            file = Path.GetFileName(f);
            label = l;
            progress = bar;
            bar.Minimum = 0;
            bar.Value = 0;
            bar.Step = 10;
            Connect(codec);
        }

        public void Connect(PackFileCodec codec) 
        {
            codec.HeaderLoaded += HeaderLoaded;
            codec.PackedFileLoaded += PackedFileLoaded;
            codec.PackFileLoaded += PackFileLoaded;
            currentCodec = codec;
        }

        public void Disconnect() 
        {
            if (currentCodec != null) 
            {
                currentCodec.HeaderLoaded -= HeaderLoaded;
                currentCodec.PackedFileLoaded -= PackedFileLoaded;
                currentCodec.PackFileLoaded -= PackFileLoaded;
                currentCodec = null;
            }
        }

        public void HeaderLoaded(PFHeader header) 
        {
            count = header.FileCount;
            progress.Maximum = (int) header.FileCount;
            label.Text = string.Format("Loading {0}: 0 of {1} files loaded", file, header.FileCount);
            Application.DoEvents();
        }

        public void PackedFileLoaded(PackedFile packedFile) 
        {
            currentCount++;
            if (currentCount % 10 <= 0) 
            {
                label.Text = string.Format("Opening {0} ({1} of {2} files loaded)", file, currentCount, count);
                progress.PerformStep();
                Application.DoEvents();
            }
        }

        public void PackFileLoaded(PackFile packedFile)
        {
            label.Text = string.Format("Finished opening {0} - {1} files loaded", packedFile, count);
            progress.Maximum = 0;
            Disconnect();
        }
    }
}

