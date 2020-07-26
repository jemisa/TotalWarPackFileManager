using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Common;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PackFileManager 
{
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

}

