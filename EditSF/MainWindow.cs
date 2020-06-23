using CommonDialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EsfControl;
using EsfLibrary;

namespace EditSF {
    public partial class EditSF : Form {
        ProgressUpdater updater;
        public static string FILENAME = "testfiles.txt";

        #region Properties
        string filename = null;
        public string FileName {
            get {
                return filename;
            }
            set {
                Text = string.Format("{0} - EditSF {1}", Path.GetFileName(value), Application.ProductVersion);
                statusLabel.Text = value;
                filename = value;
            }
        }
        EsfFile file;
        EsfFile EditedFile {
            get {
                return file;
            }
            set {
                file = value;
                editEsfComponent.RootNode = value.RootNode;
                editEsfComponent.RootNode.Modified = false;
                saveAsToolStripMenuItem.Enabled = file != null;
                saveToolStripMenuItem.Enabled = file != null;
                showNodeTypeToolStripMenuItem.Enabled = file != null;
            }
        }
        #endregion

        public EditSF() {
            InitializeComponent();

            updater = new ProgressUpdater(progressBar);

            Text = string.Format("EditSF {0}", Application.ProductVersion);
            
            editEsfComponent.NodeSelected += NodeSelected;
            
            if (File.Exists(BookmarkPath)) {
                foreach (string line in File.ReadAllLines(BookmarkPath)) {
                    string[] bm = line.Split(Path.PathSeparator);
                    AddBookmark(bm[0], bm[1], false);
                }
                editBookmarkToolStripMenuItem.Enabled = bookmarks.Count > 0;
            }
        }
        
        private void promptOpenFile() {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    OpenFile(dialog.FileName);
                } catch (Exception e) {
                    MessageBox.Show(string.Format("Could not open {0}: {1}", dialog.FileName, e));
                    updater.LoadingFinished();
                }
            }
        }
        private void OpenFile(string openFilename) {
            string oldStatus = statusLabel.Text;
            try {
                fileToolStripMenuItem.Enabled = false;
                optionsToolStripMenuItem.Enabled = false;
                // EsfCodec codec = EsfCodecUtil.GetCodec(stream);
                // updater.StartLoading(openFilename, codec);
                statusLabel.Text = string.Format("Loading {0}", openFilename);
                LogFileWriter logger = null;
                if (writeLogFileToolStripMenuItem.Checked) {
                    logger = new LogFileWriter(openFilename + ".xml");
                    //codec.NodeReadFinished += logger.WriteEntry;
                    //codec.Log += logger.WriteLogEntry;
                }
                EditedFile = EsfCodecUtil.LoadEsfFile(openFilename);
                //updater.LoadingFinished();
                FileName = openFilename;
                if (logger != null) {
                    logger.Close();
                    //codec.NodeReadFinished -= logger.WriteEntry;
                    //codec.Log -= logger.WriteLogEntry;
                }
                Text = string.Format("{0} - EditSF {1}", Path.GetFileName(openFilename), Application.ProductVersion);
                
                foreach(ToolStripItem item in bookmarksToolStripMenuItem.DropDownItems) {
                    if (item is BookmarkItem) {
                        item.Enabled = true;
                    }
                }
            } catch (Exception exception) {
                statusLabel.Text = oldStatus;
                Console.WriteLine(exception);
            } finally {
                fileToolStripMenuItem.Enabled = true;
                optionsToolStripMenuItem.Enabled = true;
            }
        }

        private void promptSaveFile() {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Save(dialog.FileName);
                FileName = dialog.FileName;
            }
        }
        
        #region Bookmarks
        private void NodeSelected(EsfNode node) {
            bookmarksToolStripMenuItem.Enabled = (node != null);
            addBookmarkToolStripMenuItem.Enabled = (node != null);
        }
        List<string> bookmarks = new List<string>();
        Dictionary<string, string> bookmarkToPath = new Dictionary<string, string>();
        private void AddBookmark(object sender, EventArgs args) {
#if DEBUG            
            Console.WriteLine("adding bookmark for node {0}", editEsfComponent.SelectedPath);
#endif
            InputBox box = new InputBox {
                Text = "Enter bookmark name",
                Input = editEsfComponent.SelectedPath
            };
            if (box.ShowDialog() == DialogResult.OK && !bookmarks.Contains(box.Input)) {
                AddBookmark(box.Input, editEsfComponent.SelectedPath);
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
                return Path.Combine(Application.UserAppDataPath, BOOKMARKS_FILE_NAME);
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
            bookmarksToolStripMenuItem.DropDownItems.Add (new BookmarkItem(label, path, editEsfComponent) {
                Enabled = enable
            });
        }
        #endregion

        #region Menu handlers
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            promptOpenFile();
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            promptSaveFile();
        }
        private void saveToolStripMenuItem1_Click(object sender, EventArgs e) {
            if (filename != null) {
                Save(filename);
            }
        }
        private void runTestsToolStripMenuItem_Click(object sender, EventArgs eventArgs) {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                dialog.Dispose();
                string logFileName = Path.Combine(dialog.SelectedPath, "EditSF_test.txt");
                FileTester tester = new FileTester();
                using (TextWriter logWriter = File.CreateText(logFileName)) {
                    foreach (string file in Directory.EnumerateFiles(dialog.SelectedPath)) {
                        if (file.EndsWith("EditSF_test.txt")) {
                            continue;
                        }
                        string testResult = tester.RunTest(file, progressBar, statusLabel);
                        logWriter.WriteLine(testResult);
                        logWriter.Flush();
                    }
                }
                MessageBox.Show(string.Format("Test successes {0}/{1}", tester.TestSuccesses, tester.TestsRun),
                                "Tests finished");
            }
        }
        private void runSingleTestToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog openDialog = new OpenFileDialog {
                RestoreDirectory = true
            };
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string output = new FileTester().RunTest(openDialog.FileName, progressBar, statusLabel);
                MessageBox.Show(output, "Test Finished");
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show(String.Format("EditSF {0}\nCreated by daniu", Application.ProductVersion), "About EditSF");
        }
        #endregion

        private void Save(string filename) {
            try {
                EsfCodecUtil.WriteEsfFile(filename, EditedFile);
                editEsfComponent.RootNode.Modified = false;
            } catch (Exception e) {
                MessageBox.Show(string.Format("Could not save {0}: {1}", filename, e));
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void showNodeTypeToolStripMenuItem_Click(object sender, EventArgs e) {
            editEsfComponent.ShowCode = true;
        }
    }
    
    public class LogFileWriter {
        private TextWriter writer;
        public LogFileWriter(string logFileName) {
            writer = File.CreateText(logFileName);
        }
        public void WriteEntry(EsfNode node, long position) {
            //ParentNode
            if (node is RecordNode) {
            }
            //writer.WriteLine("Entry {0} / {1:x} read at {2:x}", node, node.TypeCode, position);
        }
        public void WriteLogEntry(string entry) {
            writer.WriteLine(entry);
        }
        public void Close() {
            writer.Close();
        }
    }

    public class ProgressUpdater {
        private ToolStripProgressBar progress;
        private EsfCodec currentCodec;
        public ProgressUpdater(ToolStripProgressBar bar) {
            progress = bar;
        }
        public void StartLoading(string file, EsfCodec codec) {
            progress.Maximum = (int)new FileInfo(file).Length;
            currentCodec = codec;
            currentCodec.NodeReadFinished += Update;
        }
        public void LoadingFinished() {
            try {
                progress.Value = 0;
                currentCodec.NodeReadFinished -= Update;
            } catch { }
        }
        void Update(EsfNode ignored, long position) {
            if (ignored is ParentNode) {
                try {
                    if ((int)position <= progress.Maximum) {
                        progress.Value = (int)position;
                    }
                    Application.DoEvents();
                } catch {
                    progress.Value = 0;
                    currentCodec.NodeReadFinished -= Update;
                }
            }
        }
    }
}