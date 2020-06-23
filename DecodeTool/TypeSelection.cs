using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Filetypes;

namespace DecodeTool {
    public partial class TypeSelection : UserControl {
        public delegate FieldInfo TypeFactory();

        TypeFactory factory;

        public FieldInfo Type {
            get {
                return factory != null ? factory() : null;
            }
            set {
                // set via factory method
            }
        }

        public TypeFactory Factory {
            set {
                factory = value;
            }
        }

        public delegate void selection(FieldInfo type);
        public event selection Selected;

        public void ShowPreview(BinaryReader bytes) {
            if (Type != null) {
                preview.Text = Util.decodeSafe(Type, bytes);
            }
        }

        public TypeSelection() {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e) {
            if (Selected != null) {
                Selected(Type);
            }
        }
    }
}
