using System;
using System.Windows;
using System.Windows.Forms;
using Filetypes;
using Common;

namespace DBTableControl {
    
    /*
     * ElementHost that implements IPackedFileEditor by delegating its methods and properties
     * to the hosted DBEditorTableControl.
     */
    public class DBEditorTableHost : System.Windows.Forms.Integration.ElementHost, IPackedFileEditor {
        DBEditorTableControl DbeChild {
            get {
                return Child as DBEditorTableControl;
            }
        }
        public PackedFile CurrentPackedFile {
            get {
                return DbeChild.CurrentPackedFile;
            }
            set {
                DbeChild.CurrentPackedFile = value;
            }
        }
        public bool CanEdit(PackedFile file) {
            return DbeChild.CanEdit(file);
        }
        public void Commit() {
            DbeChild.Commit();
        }
        
        // one... more... property...
        public bool ReadOnly {
            get {
                return DbeChild.ReadOnly;
            }
            set {
                DbeChild.ReadOnly = value;
            }
        }

        public DBEditorTableHost() : base()
        {

        }
    }
}

