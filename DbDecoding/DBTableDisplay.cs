using Filetypes;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DbDecoding
{
    public partial class DBTableDisplay : Form {
        public DBTableDisplay() {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog fileDlg = new OpenFileDialog();
            if (fileDlg.ShowDialog() == DialogResult.OK) {
                DBTypeMap.Instance.initializeFromFile(fileDlg.FileName);
                SetTypeInfos();
            }
        }
        private void SetTypeInfos() {
            List<String> added = new List<string>();
            List<TypeInfo> infos = new List<TypeInfo>(DBTypeMap.Instance.AllInfos);
            infos.Sort();
            foreach (TypeInfo type in infos) {
                if (!added.Contains(type.Name)) {
                    dbTypeComboBox.Items.Add(type.Name);
                    added.Add(type.Name);
                }
            } 
            CurrentInfos = null;
        }

        List<TypeInfo> currentInfos = new List<TypeInfo>();
        List<TypeInfo> CurrentInfos {
            get {
                return currentInfos;
            }
            set {
                currentInfos.Clear();
                if (value != null) {
                    currentInfos.AddRange(value);
                }
                versionsListBox.Items.Clear();
                foreach (TypeInfo info in currentInfos) {
                    int addedIndex = versionsListBox.Items.Add(info.Version);
                }
                fieldsListBox.Items.Clear();
            }
        }
        private void dbTypeComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            versionsListBox.Items.Clear();
            String selected = dbTypeComboBox.SelectedItem as String;
            CurrentInfos = DBTypeMap.Instance.GetAllInfos(selected);
        }

        private void versionsListBox_SelectedIndexChanged(object sender, EventArgs e) {
            fieldsListBox.Items.Clear();
            int index = versionsListBox.SelectedIndex;
            foreach (FieldInfo field in CurrentInfos[index].Fields) {
                fieldsListBox.Items.Add(String.Format("{0} : {1}", field.Name, field.TypeName));
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e) {
            Add(true);
        }
        private void integrateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Add(false);
        }
        private void Add(bool keepExistingNames)
        {
            FileDialog fileDlg = new OpenFileDialog();
            if (fileDlg.ShowDialog() == DialogResult.OK) {
                using (var stream = File.OpenRead(fileDlg.FileName)) {
                    XmlImporter importer = new XmlImporter(stream);
                    importer.Import(true);
                    foreach (TypeInfo info in importer.Imported) {
                        bool exists = false;
                        foreach (TypeInfo existing in DBTypeMap.Instance.GetAllInfos(info.Name)) {
                            if (existing.Version == info.Version && existing.SameTypes(info)) {
                                Console.WriteLine("imported type info {0}/{1} already exists with those fields",
                                    info.Name, info.Version);
                                exists = true;
                                if (!keepExistingNames) {
                                    for (int j = 0; j < existing.Fields.Count; j++) {
                                        if (!existing.Fields[j].Name.Equals(info.Fields[j].Name)) {
                                            SchemaFieldNameChooser f = new SchemaFieldNameChooser();
                                            f.LeftInfo = existing;
                                            f.RightInfo = info;
                                            if (f.ShowDialog() == DialogResult.OK) {
                                                TypeInfo result = f.MergedInfo;
                                                for (int i = 0; i < result.Fields.Count; i++) {
                                                    existing.Fields[i].Name = result.Fields[i].Name;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        if (!exists) {
                            Console.WriteLine("imported type info {0}/{1} into schema", info.Name, info.Version);
                            DBTypeMap.Instance.AllInfos.Add(info);
                        }
                    }
                }
                SetTypeInfos();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            FileDialog fileDlg = new SaveFileDialog();
            if (fileDlg.ShowDialog() == DialogResult.OK) {
                DBTypeMap.Instance.SaveToFile(fileDlg.FileName);
            }
        }

    }
}
