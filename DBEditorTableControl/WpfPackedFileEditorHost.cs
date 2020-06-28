using System;
using System.Windows;
using System.Windows.Forms;
using Filetypes;
using Common;
using System.Windows.Controls;

namespace DBTableControl {
    
    /*
     * ElementHost that implements IPackedFileEditor by delegating its methods and properties
     * to the hosted DBEditorTableControl.
     */
    public class WpfPackedFileEditorHost : System.Windows.Forms.Integration.ElementHost, IPackedFileEditor {
        public IPackedFileEditor TypedChild {
            get {
                return Child as IPackedFileEditor;
            }
        }
        public PackedFile CurrentPackedFile {
            get {
                return TypedChild.CurrentPackedFile;
            }
            set {
                TypedChild.CurrentPackedFile = value;
            }
        }
        public bool CanEdit(PackedFile file) {
            return TypedChild.CanEdit(file);
        }
        public void Commit() {
            TypedChild.Commit();
        }

        public bool ReadOnly {
            get {
                return TypedChild.ReadOnly;
            }
            set {
                TypedChild.ReadOnly = value;
            }
        }

        public WpfPackedFileEditorHost() : base()
        {

        }

        public static WpfPackedFileEditorHost Create<T>() where T: System.Windows.Controls.UserControl, new()
        {
            var host = new WpfPackedFileEditorHost
            {
                Child = new T(),
                Dock = DockStyle.Fill
            };
            return host;
        }
    }
}

