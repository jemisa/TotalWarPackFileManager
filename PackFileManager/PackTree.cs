using Common;
using Filetypes;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
namespace PackFileManager {
    /*
     * Base class for any kind of entry (directory of file).
     */
    abstract class PackEntryNode : TreeNode {
        protected bool renamed;
        private bool added;
        public bool Added {
            get { return added; }
            set {
                added = value;
                ChangeColor();
            }
        }
        public PackEntryNode(PackEntry entry)
            : base(entry.Name) {
            Tag = entry;
            entry.ModifiedEvent += delegate(PackEntry p) { ChangeColor(); };
            entry.RenameEvent += (e, name) => { renamed = true; ChangeColor(); };
            ChangeColor();
        }
        public virtual void ChangeColor() {
            PackEntry e = Tag as PackEntry;
            ForeColor = Color.Black;
            if (added) {
                ForeColor = Color.Green;
            } else if (renamed) {
                ForeColor = Color.LimeGreen;
            } else if (e.Deleted) {
                ForeColor = Color.LightGray;
            } else if (e.Modified) {
                ForeColor = Color.Red;
            }
        }
        public void Reset() {
            renamed = added = false;
            foreach (TreeNode node in Nodes) {
                PackEntryNode packNode = node as PackEntryNode;
                packNode.Reset();
            }
        }
    }

    /*
     * A tree node representing an actual data file.
     */
    class PackedFileNode : PackEntryNode {
        public PackedFileNode(PackedFile file)
            : base(file) {
                if (file.FullPath.StartsWith("db")) {
                    file.RenameEvent += Renamed;
                }
        }
        void Renamed(PackEntry file, string val) {
            Text = val;
        }

        /*
         * Overridden to adjust to color depending on we have DB type information.
         */
        public override void ChangeColor() {
            base.ChangeColor();

            PackedFile packedFile = Tag as PackedFile;
            string text = Path.GetFileName(packedFile.Name);
            if (packedFile != null && packedFile.FullPath.StartsWith("db")) {
                if (packedFile.Data.Length == 0) {
                    text = string.Format("{0} (empty)", packedFile.Name);
                } else {
                    string mouseover;
                    
                    try {
                        DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
                        // text = string.Format("{0} - version {1}", text, header.Version);
                        if (header.EntryCount == 0) {
                            // empty db file
                            ForeColor = Color.Blue;
                            if (Parent != null) {
                                Parent.ForeColor = Color.Blue;
                            }
                        } else if (!PackedFileDbCodec.CanDecode(packedFile, out mouseover)) {
                            if (Parent != null) {
                                Parent.ToolTipText = mouseover;
                                Parent.ForeColor = Color.Red;
                            }
                            ForeColor = Color.Red;
                        } else if (HeaderVersionObsolete(packedFile)) {
                            if (Parent != null) {
                                Parent.BackColor = Color.Yellow;
                            }
                            BackColor = Color.Yellow;
                        }
                    } catch { }
                }
            }
            Text = text;
        }

        public static bool HeaderVersionObsolete(PackedFile packedFile) {
            DBFileHeader header = PackedFileDbCodec.readHeader(packedFile);
            string type = DBFile.Typename(packedFile.FullPath);
            int maxVersion = GameManager.Instance.GetMaxDbVersion(type);
            return DBTypeMap.Instance.IsSupported(type) && maxVersion != 0 && (header.Version < maxVersion);
        }
    }

    /*
     * A tree node representing a directory.
     */
    class DirEntryNode : PackEntryNode {
        public DirEntryNode(VirtualDirectory nodeDir)
            : base(nodeDir) {
            foreach (VirtualDirectory dir in nodeDir.Subdirectories) {
                Nodes.Add(new DirEntryNode(dir));
            }
            foreach (PackedFile file in nodeDir.Files) {
                PackEntryNode node = new PackedFileNode(file);
                Nodes.Add(node);
                node.ChangeColor();
            }
            nodeDir.DirectoryAdded += InsertNew;
            nodeDir.FileAdded += InsertNew;
            nodeDir.FileRemoved += RemoveEntry;
        }
        private void RemoveEntry(PackEntry entry) {
            TreeNode remove = null;
            foreach (TreeNode node in Nodes) {
                if (node.Tag == entry) {
                    remove = node;
                    break;
                }
            }
            if (remove != null) {
                Nodes.Remove(remove);
            }
        }
        private void InsertNew(PackEntry entry) {
            PackEntryNode node = null;
            if (entry is PackedFile) {
                node = new PackedFileNode(entry as PackedFile);
            } else {
                node = new DirEntryNode(entry as VirtualDirectory);
            }
            node.Added = true;
            int index = 0;
            List<PackEntry> entries = (Tag as VirtualDirectory).Entries;
            for (index = 0; index < entries.Count; index++) {
                if (entries[index] == entry) {
                    break;
                }
            }
            Nodes.Insert(index, node);
            node.ChangeColor();

            ChangeColor();
            PackEntryNode parent = Parent as PackEntryNode;
            while (parent != null) {
                parent.ChangeColor();
                parent = parent.Parent as PackEntryNode;
            }

            Expand();
        }
    }
}
