using CommonDialogs;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Filetypes;
using Common;
using EsfLibrary;
using EsfControl;

namespace PackFileManager {
    public partial class PackedEsfEditor : PackedFileEditor<EsfNode> {
        public PackedEsfEditor() : base(new DelegatingEsfCodec()) {
            InitializeComponent();
            
#if DEBUG
            Console.WriteLine("storing bookmarks in {0}", BookmarkPath);
#endif
            esfComponent.NodeSelected += HandleEsfComponentNodeSelected;
            if (File.Exists(BookmarkPath)) {
                foreach(string line in File.ReadAllLines(BookmarkPath)) {
                    try {
                        string[] bm = line.Split(Path.PathSeparator);
                        AddBookmark(bm[0], bm[1], true);
                    } catch {}
                }
            }
        }
        
        EsfNode selectedNode = null;
        void HandleEsfComponentNodeSelected (EsfNode node)
        {
            selectedNode = node;
            addToolStripMenuItem.Enabled = selectedNode != null;
        }

        string[] EXTENSIONS = { ".esf" };
        public override bool CanEdit(PackedFile file) {
            return HasExtension(file, EXTENSIONS);
        }

        public override EsfNode EditedFile {
            get {
                return base.EditedFile;
            }
            set {
                if (EditedFile != null) {
                    EditedFile.ModifiedEvent -= SetModified;
                }
                esfComponent.RootNode = value;
                base.EditedFile = value;
                EditedFile.ModifiedEvent += SetModified;
                DataChanged = false;
            }
        }

        private void SetModified(EsfNode n) {
            DataChanged = true;
        }

        #region Bookmarks
        List<string> bookmarks = new List<string>();
        Dictionary<string, string> bookmarkToPath = new Dictionary<string, string>();
        void HandleAddToolStripMenuItemhandleClick (object sender, System.EventArgs e)
        {
            InputBox inputBox = new InputBox {
                Text = "Enter bookmark name",
                Input = esfComponent.SelectedPath
            };
            if (inputBox.ShowDialog() == DialogResult.OK) {
                AddBookmark(inputBox.Input, esfComponent.SelectedPath, true);
                SaveBookmarks();
            }
        }
        
        public void EditBookmarks(object sender, EventArgs args) {
            List<string> bm = new List<string>(bookmarks);
            List<string> delete = new List<string>();
            ListEditor editor = new ListEditor {
                LeftLabel = "Current Bookmarks",
                LeftList = bm,
                RightLabel = "Delete Bookmarks",
                RightList = delete
            };
            if (editor.ShowDialog() == DialogResult.OK) {
                foreach(string toDelete in editor.RightList) {
                    bookmarks.Remove(toDelete);
                    bookmarkToPath.Remove(toDelete);
                    foreach(ToolStripItem item in bookmarksToolStripMenuItem.DropDownItems) {
                        if (item is BookmarkItem && item.Text.Equals (toDelete)) {
                            bookmarksToolStripMenuItem.DropDownItems.Remove(item);
                            break;
                        }
                    }
                }
                SaveBookmarks();
            }
        }
        string BookmarkPath {
            get {
                return Path.Combine(Program.ApplicationFolder, BOOKMARKS_FILE_NAME);
            }
        }
        private void SaveBookmarks() {
            using (var stream = File.CreateText(BookmarkPath)) {
                foreach(string bookmark in bookmarks) {
                    stream.WriteLine("{0}{1}{2}", bookmark, Path.PathSeparator, bookmarkToPath[bookmark]);
                }
            }
        }
        static string BOOKMARKS_FILE_NAME = "bookmarks.txt";
        void AddBookmark(string label, string path, bool enable = true) {
            bookmarks.Add(label);
            bookmarkToPath[label] = path;
            bookmarksToolStripMenuItem.DropDownItems.Add (new BookmarkItem(label, path, esfComponent) {
                Enabled = enable
            });
        }
        #endregion
    }
    
    class DelegatingEsfCodec : Codec<EsfNode> {
        EsfCodec codecDelegate;
        public EsfNode Decode(Stream stream) {
            codecDelegate = EsfCodecUtil.GetCodec(stream);
            return codecDelegate.Parse(stream);
        }
        public void Encode(Stream encodeTo, EsfNode node) {
            using (var writer = new BinaryWriter(encodeTo)) {
                codecDelegate.EncodeRootNode(writer, node);
            }
        }
    }
}
