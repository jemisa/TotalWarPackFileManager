using Filetypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DbDecoding {
    public partial class SchemaFieldNameChooser : Form {
        TypeInfo leftInfo;
        public TypeInfo LeftInfo {
            get {
                return leftInfo;
            }
            set {
                leftInfo = value;
                if (MergedInfo == null) {
                    MergedInfo = new TypeInfo(leftInfo);
                }

                FillFieldList(leftFieldListBox, LeftInfo);
            }
        }
        TypeInfo rightInfo;
        public TypeInfo RightInfo {
            get {
                return rightInfo;
            }
            set {
                rightInfo = value;
                if (MergedInfo == null) {
                    MergedInfo = new TypeInfo(rightInfo);
                }

                FillFieldList(rightFieldListBox, rightInfo);
            }
        }

        private TypeInfo mergedInfo;
        public TypeInfo MergedInfo {
            get {
                return mergedInfo;
            }
            set {
                mergedInfo = value;

                infoLabel.Text = String.Format("Type {0}, Version {1}", mergedInfo.Name, mergedInfo.Version);
                FillFieldList(resultFieldListBox, MergedInfo);
            }
        }

        private void FillFieldList(ListBox list, TypeInfo info) {
            list.Items.Clear();
            info.Fields.ForEach(f =>
            {
                list.Items.Add(String.Format("{0} : {1}", f.Name, f.TypeName));
            });
        }

        public SchemaFieldNameChooser() {
            InitializeComponent();

            AcceptButton = okButton;
        }

        private void okButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }

        private void onListDoubleClick(object sender, EventArgs e) {
            ListBox list = sender as ListBox;
            TypeInfo info = (sender == leftFieldListBox) ? LeftInfo : RightInfo;
            int index = list.SelectedIndex;
            String name = info.Fields[index].Name;
            MergedInfo.Fields[index].Name = name;
            FillFieldList(resultFieldListBox, MergedInfo);
        }
    }
}
