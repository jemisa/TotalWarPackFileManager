using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common;
using Filetypes;
using CommonDialogs;
using Filetypes.Codecs;

namespace PackFileManager {
    public class GroupformationEditor : PackedFileEditor<GroupformationFile> {
        TabControl tabControl = new TabControl {
            Name = "tabControl",
            Multiline = true
        };
        List<GroupformationEditorControl> Editors = new List<GroupformationEditorControl>();
        
        public override bool CanEdit(PackedFile file) {
            return file != null && file.FullPath.EndsWith("groupformations.bin");
        }
        
        protected override bool DataChanged {
            get {
                bool result = false;
                Editors.ForEach(e => result |= e.Modified);
                return result;
            }
            set {
                // should probably not really do this... at least for setting to true
                Editors.ForEach(e => e.Modified = value);
            }
        }
        
        public override GroupformationFile EditedFile {
            get {
                return base.EditedFile;
            }
            set {
                base.EditedFile = value;
                Console.WriteLine("opening group formations");
                base.SuspendLayout();
                this.Dock = DockStyle.Fill;
                Editors.Clear();
                // Console.WriteLine("edited file now {0}", EditedFile);
                foreach (Groupformation formation in EditedFile.Formations) {
                    // Console.WriteLine("adding tab {0}", formation.Name);
                    TabPage tabPage = CreatePage(formation);
                    tabControl.TabPages.Add(tabPage);
                }
                tabControl.Dock = DockStyle.Fill;
                Controls.Add(tabControl);
                
                TabPage addPage = new TabPage("+") {
                    Name = "+"
                };
                tabControl.TabPages.Add(addPage);

                tabControl.Selecting += new TabControlCancelEventHandler(SelectedAddTab);
                tabControl.Click += new EventHandler(RightClickTab);
                
                base.ResumeLayout(true);
            }
        }
        
        TabPage CreatePage(Groupformation formation) {
            TabPage tabPage = new TabPage(formation.Name) {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            GroupformationEditorControl editor = new GroupformationEditorControl {
                Dock = DockStyle.Fill,
                EditedFormation = formation
            };
            tabPage.Controls.Add(editor);
            Editors.Add(editor);
            return tabPage;
        }

        void RightClickTab(object sender, EventArgs args) {
            MouseEventArgs mouseArgs = args as MouseEventArgs;
            if ((mouseArgs.Button & System.Windows.Forms.MouseButtons.Right) != 0) {
                int tabIndex = -1;
                for(int i = 0; i < tabControl.TabPages.Count; i++) {
                    if (tabControl.GetTabRect(i).Contains(mouseArgs.Location)) {
                        tabIndex = i;
                        break;
                    }
                }
                if (MessageBox.Show(string.Format("Do you want to delete formation {0}?", tabControl.TabPages[tabIndex].Text)) 
                    == DialogResult.OK) {
                        tabControl.TabPages.RemoveAt(tabIndex);
                }
            }
        }

        void SelectedAddTab(object sender, TabControlCancelEventArgs args) {
            if (args.TabPage.Name == "+") {
                Console.WriteLine("adding!");
                InputBox box = new InputBox {
                    Input = "New Formation"
                };
                if (box.ShowDialog() == DialogResult.OK) {
                    tabControl.TabPages.Remove(args.TabPage);
                    Groupformation newFormation = new Groupformation {
                        Name = box.Input
                    };
                    EditedFile.Formations.Add(newFormation);
                    TabPage page = CreatePage(newFormation);
                    tabControl.TabPages.Add(page);
                    tabControl.TabPages.Add(args.TabPage);
                    tabControl.SelectTab(page);
                }
                args.Cancel = true;
            } else {
                Console.WriteLine("selected {0}", args.TabPage.Name);
            }
        }
        
        public GroupformationEditor () : base(new GroupformationCodec()) {
        }
    }
}

