using Common;
using Filetypes;
using PackFileManager.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CommonDialogs;
using CommonUtilities;
using System.Linq;
using Filetypes.Codecs;

namespace PackFileManager
{
    public partial class PackFileManagerForm : Form {

        public PackFileManagerForm() : this(new string[0]) {}

        private PackFile currentPackFile;
        public PackFile CurrentPackFile {
            get { return currentPackFile; }
            set {
                CloseEditors();

                // register previous and build tree
                currentPackFile = value;
                RefreshTitle();
                EnableMenuItems();
                currentPackFile.Modified += RefreshTitle;
                currentPackFile.Modified += EnableMenuItems;

                DBReferenceMap.Instance.CurrentPack = value;
                
                Refresh();
            }
        }

        private CustomMessageBox search;
        DBFileUpdate dbUpdater = new DBFileUpdate();
        PackTreeViewFilterService _packTreeFilterService;

        #region Editors

        private readonly DBFileEditorControl dbFileEditorControl = new DBFileEditorControl {
            Dock = DockStyle.Fill
        };
        private TextFileEditorControl textFileEditorControl = new TextFileEditorControl { 
            Dock = DockStyle.Fill };
        ExternalEditor externalEditor = new ExternalEditor();

        private void CreateEditors()
        {  
#if __MonoCS__
#else
            // relies on win32 dll, so can't use it on Linux
            PackedFileEditorRegistry.Editors.Add(new AtlasFileEditorControl { Dock = DockStyle.Fill });
            try
            {
                string dbeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                dbeLocation = Path.Combine(dbeLocation, "DBEditorTableControl.dll");
                Assembly dbeAssembly = Assembly.LoadFile(dbeLocation);
                Type dbeType = dbeAssembly.GetType("DBTableControl.DBEditorTableControl");
                MethodInfo registerMethodInfo = dbeType.GetMethod("RegisterDbEditor", BindingFlags.Public | BindingFlags.Static);
                object registered = registerMethodInfo.Invoke(null, null);
                string newDir = ModManager.Instance.CurrentModDirectory;
                PropertyInfo modDirProperty = dbeType.GetProperty("ModDirectory");
                modDirProperty.SetValue(registered, newDir, null);
                ModManager.Instance.CurrentModChanged += delegate() {
                    newDir = ModManager.Instance.CurrentModDirectory;
                    modDirProperty.SetValue(registered, newDir, null);
                };
            }
            catch (Exception e)
            {
                string message = string.Format("Failed to load DBE: {0}", e.Message);
                if (e.Message.Contains("80131515")) {
                    message += "For a workaround:\n" +
                        "- right-click the DBEditorTableControl.dll\n" +
                            "- select Properties\n" +
                            "- click the \"Unblock\" button.\n\n" +
                            "- Do you want to go to the install directory now?";
                    string dllPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "DBEditorTableControl.dll");
                    if (MessageBox.Show(message, "Failed to load dll", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                        string arguments = string.Format("/select {0}", dllPath);
                        Process.Start("explorer.exe", arguments);
                    }
                } else {
                    MessageBox.Show(string.Format("failed to load DBE: {0}", e.Message));
                }
            }
#endif
            PackedFileEditorRegistry.Editors.Add(dbFileEditorControl);
            PackedFileEditorRegistry.Editors.Add(new ImageViewerControl { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(new LocFileEditorControl { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(new GroupformationEditor { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(new UnitVariantFileEditorControl { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(new PackedEsfEditor { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(new DBFileEditorTree { Dock = DockStyle.Fill });
            // new ReadmeEditorControl { Dock = DockStyle.Fill });
            PackedFileEditorRegistry.Editors.Add(textFileEditorControl);
        }
        #endregion

        public PackFileManagerForm (string[] args) {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));
            InitializeComponent();

            ReadInitialSize();

            CreateEditors();
            dbUpdater.DetermineGuid = QueryGuid;

            try
            {
                if (Settings.Default.FirstStart)
                {
                    Settings.Default.UpdateOnStartup = (MessageBox.Show(
                        "Looking for updated schema files for decoding DB files.\n" +
                        "Do you want to do this every time the PFM is started (recommended)?", 
                        "First start", MessageBoxButtons.YesNo) == DialogResult.Yes);
                    Settings.Default.FirstStart = false;
                }
                if (Settings.Default.UpdateOnStartup)
                    TryUpdate (false);
            }
            catch {}

            try {
                if (!DBTypeMap.Instance.Initialized) {
                    DBTypeMap.Instance.InitializeTypeMap(Path.GetDirectoryName(Application.ExecutablePath));
                }
            } catch (Exception e) {
                if (MessageBox.Show(string.Format("Could not initialize type map: {0}.\nTry autoupdate?", e.Message),
                    "Initialize failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                    TryUpdate();
                }
            }

            // reflect settings in check box menu items
            updateOnStartupToolStripMenuItem.Checked = Settings.Default.UpdateOnStartup;
            subscribeToBetaToolStripMenuItem.Checked = Settings.Default.SubscribeToBetaSchema;
            showDecodeToolOnErrorToolStripMenuItem.Checked = Settings.Default.ShowDecodeToolOnError;

            tsvToolStripMenuItem.Checked = "tsv".Equals(Settings.Default.TsvExtension);
            csvToolStripMenuItem.Checked = !tsvToolStripMenuItem.Checked;
            if (!"tsv".Equals(Settings.Default.TsvExtension) && !"csv".Equals(Settings.Default.TsvExtension)) {
                Settings.Default.TsvExtension = "csv";
            }

            foreach(PackType type in Enum.GetValues(typeof(PackType))) {
                ToolStripMenuItem item = new ToolStripMenuItem(type.ToString()) {
                    Tag = type,
                    CheckOnClick = true,
                    Name = string.Format("{0}ToolStripMenuItem", type.ToString().ToLower())
                };
                item.Click += PackTypeItemSelected;
                changePackTypeToolStripMenuItem.DropDownItems.Insert(0, item);
            }
            setShaderToolStripMenuItem.Click += (sender, e) => {
                currentPackFile.IsShader = !currentPackFile.IsShader;
                setShaderToolStripMenuItem.Checked = currentPackFile.IsShader;
            };

            InitializeBrowseDialogs (args);

            // fill CA file list and refill for new game when changed
            FillCaPackMenu();
            GameManager.Instance.GameChanged += FillCaPackMenu;
            // allow/disallow mod installation depending on if game is installed
            GameManager.Instance.GameChanged += UpdateModMenuItems;
            // reload when game has changed (rebuild tree etc)
            GameManager.Instance.GameChanged += OpenCurrentModPack;
            // ask if the user also wants to change the current mod's game
            GameManager.Instance.GameChanged += QueryModGameChange;
            // update window title to show new game setting
            GameManager.Instance.GameChanged += RefreshTitle;
            // change icon upon game change
            ChangeGameIcon();
            GameManager.Instance.GameChanged += ChangeGameIcon;
            UpdateGameDirectoryItems();
            GameManager.Instance.GameChanged += UpdateGameDirectoryItems;

            // fill game list
            for (int i = Game.Games.Count - 1; i >= 0; i--) {
                gameToolStripMenuItem.DropDownItems.Insert(0, CreateGameItem(Game.Games[i]));
            }

            // allow/disallow mod installation, depending on if mod is set
            UpdateModMenuItems();
            ModManager.Instance.CurrentModChanged += UpdateModMenuItems;
            // when user selects a mod, open the corresponding pack file if it exists
            ModManager.Instance.CurrentModChanged += OpenCurrentModPack;

            // initialize MyMods menu
            modsToolStripMenuItem.DropDownItems.Insert(0, new ModMenuItem("None", ""));
            ModManager.Instance.ModNames.ForEach(name => 
                                                 modsToolStripMenuItem.DropDownItems.Insert(1, new ModMenuItem(name, name)));
            if (args.Length == 0) {
                OpenCurrentModPack();
            } else if (args.Length == 1) {
                // open pack file from command line if applicable
                if (!File.Exists(args[0])) {
                    throw new ArgumentException("path is not a file or path does not exist");
                }
                OpenExistingPackFile(args[0]);
            }

            EnableMenuItems();

            RefreshTitle();

            _packTreeFilterService = new PackTreeViewFilterService(packTreeView);
        }
        
        #region Form Management
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            base.Close();
        }

        private void PackFileManagerForm_Activated(object sender, EventArgs e) {
            if ((base.OwnedForms.Length > 0) && (search != null)) {
                packTreeView.SelectedNode = search.nextNode;
            }
        }

        private void PackFileManagerForm_FormClosing(object sender, FormClosingEventArgs e) {
            switch(e.CloseReason) {
            case CloseReason.WindowsShutDown:
            case CloseReason.TaskManagerClosing:
            case CloseReason.ApplicationExitCall:
                break;
            default:
                e.Cancel = QuerySaveModifiedFile() == DialogResult.Cancel;
                break;
            }
            if (!e.Cancel) {
                try {
#if DEBUG
                    Console.WriteLine("Writing window state to {0}", SizeLocationFile);
#endif
                    using (var writer = File.CreateText(SizeLocationFile)) {
                        writer.WriteLine((int)this.WindowState);
                        writer.WriteLine("{0}:{1}", this.Location.X, this.Location.Y);
                        writer.WriteLine("{0}:{1}", this.Width, this.Height);
                    }
                } catch { }
            }
        }
        
        private void ReadInitialSize() {
            if (!File.Exists(SizeLocationFile)) {
                return;
            }
            try {
                using (var reader = File.OpenText(SizeLocationFile)) {
                    string state = reader.ReadLine();
                    int winState = (int) Convert.ChangeType(state, typeof(int));
                    FormWindowState initialState = (FormWindowState) winState;
                    if (initialState == FormWindowState.Minimized) {
                        return;
                    }
                    this.WindowState = initialState;
                    if (initialState != FormWindowState.Maximized) {
                        this.StartPosition = FormStartPosition.Manual;
                        string line = reader.ReadLine();
                        this.Location = ParsePoint(line);
                        line = reader.ReadLine();
                        this.Size = new Size(ParsePoint(line));
                    }
                }
#if DEBUG
            } catch (Exception e) {
                Console.WriteLine("Failed to restore previous window location/size: {0}", e.Message);
#else
            } catch {
#endif
            }
        }
        private static Point ParsePoint(string pointString) {
            Point p = new Point();
            string[] coords = pointString.Split(':');
            p.X = int.Parse(coords[0]);
            p.Y = int.Parse(coords[1]);
            return p;
        }
        
        private string SizeLocationFile {
            get {
                return Path.Combine(Program.ApplicationFolder, "window_state.txt");
            }
        }

        private void PackFileManagerForm_GotFocus(object sender, EventArgs e) {
            base.Activated -= new EventHandler (PackFileManagerForm_GotFocus);
            if (externalEditor.Modified) {
                if (MessageBox.Show ("Changes were made to the extracted file. "+
                                     "Do you want to replace the packed file with the extracted file?", "Save changes?", 
                                     MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    externalEditor.Commit();
                }
            }
        }

        /*
         * Determine a reasonable initial directory for the open/save/extract
         * browse dialogs.
         */
        void InitializeBrowseDialogs(string[] args) {

            // do we have a last directory? use as default
            // default: current directory
            string initialDialog = Directory.GetCurrentDirectory();
            try {

                // use the last load/save location if we have one
                if (!string.IsNullOrEmpty(Settings.Default.LastPackDirectory)) {
                    initialDialog = Settings.Default.LastPackDirectory;
                } else {
                    // otherwise, try to determine the shogun install path and use the data directory
                    initialDialog = GameManager.Instance.CurrentGame.GameDirectory;
                    if (!string.IsNullOrEmpty(initialDialog)) {
                        initialDialog = Path.Combine(initialDialog, "data");
                    } else {

                        // go through the arguments (interpreted as file names)
                        // and use the first for which the directory exists
                        foreach (string file in args) {
                            string dir = Path.GetDirectoryName(file);
                            if (File.Exists(dir)) {
                                initialDialog = dir;
                                break;
                            }
                        }
                    }
                }
            } catch {
                // we have not set an invalid path along the way; should still be current dir here
            }
            // set to the dialogs
            Settings.Default.LastPackDirectory = initialDialog;
        }
        #endregion

        private void ExportFileList(object sender, EventArgs e) {
            SaveFileDialog fileListDialog = new SaveFileDialog {
                InitialDirectory = ImportExportDirectory,
                FileName = Path.GetFileNameWithoutExtension(currentPackFile.Filepath) + ".pack-file-list.txt"
            };
            if (fileListDialog.ShowDialog() == DialogResult.OK) {
                Settings.Default.ImportExportDirectory = Path.GetDirectoryName(fileListDialog.FileName);
                using (StreamWriter writer = new StreamWriter(fileListDialog.FileName)) {
                    foreach (PackedFile file in currentPackFile.Files) {
                        writer.WriteLine(file.FullPath);
                    }
                }
            }
        }

        private void MinimizeDbFiles(object sender, EventArgs e) {
            if (QuerySaveModifiedFile() == DialogResult.Cancel) {
                return;
            }
            DbFileOptimizer optimizer = new DbFileOptimizer(GameManager.Instance.CurrentGame);
            PackFile optimizedFile = optimizer.CreateOptimizedFile(CurrentPackFile);
            //optimizedFile.IsModified = false;
            CurrentPackFile = optimizedFile;
        }
  
        #region Menu Items
        private void UpdateModMenuItems() {
            bool enabled = ModManager.Instance.CurrentModSet;
            enabled &= GameManager.Instance.CurrentGame.IsInstalled;
            installModMenuItem.Enabled = uninstallModMenuItem.Enabled = 
                openModPathToolStripMenuItem.Enabled = enabled;
            if (enabled) {
                installModMenuItem.Text = string.Format("Install {0}", ModManager.Instance.CurrentMod.Name);
                uninstallModMenuItem.Text = string.Format("Uninstall {0}", ModManager.Instance.CurrentMod.Name);
                openModPathToolStripMenuItem.Tag = ModManager.Instance.CurrentModDirectory;
            }
        }
        
        ToolStripMenuItem CreateGameItem(Game g) {
            ToolStripMenuItem item = new ToolStripMenuItem(g.Id);
            item.Enabled = g.IsInstalled;
            item.Checked = GameManager.Instance.CurrentGame == g;
            item.Click += new EventHandler(delegate(object o, EventArgs unused) { 
                GameManager.Instance.CurrentGame = Game.ById(item.Text); 
            });
            Icon icon = Resources.GetGameIcon(g);
            if (icon != null) {
                item.Image = icon.ToBitmap();
            }
            GameManager.Instance.GameChanged += delegate() {
                item.Checked = GameManager.Instance.CurrentGame.Id.Equals(item.Text);
            };
            return item;
        }

        void UpdateGameDirectoryItems() {
            if (GameManager.Instance.CurrentGame.IsInstalled) {
                UpdateDirectoryItem(openGameDirToolStripMenuItem, GameManager.Instance.CurrentGame.GameDirectory);
                UpdateDirectoryItem(openDataDirToolStripMenuItem, GameManager.Instance.CurrentGame.DataDirectory);
                UpdateDirectoryItem(openEncyclopediaDirToolStripMenuItem, 
                                    Path.Combine(GameManager.Instance.CurrentGame.DataDirectory, "encyclopedia"));
                UpdateDirectoryItem(openUserDirToolStripMenuItem, GameManager.Instance.CurrentGame.UserDir);
                UpdateDirectoryItem(openReplaysDirToolStripMenuItem, 
                                    Path.Combine(GameManager.Instance.CurrentGame.UserDir, "replays"));
                UpdateDirectoryItem(openScriptsDirToolStripMenuItem, GameManager.Instance.CurrentGame.ScriptDirectory);
            } else {
                String message = String.Format("Currently edited game is {0}, but it does not seem to be installed.\n" +
                                               "Make sure to select the proper game in the Games menu.", 
                                               GameManager.Instance.CurrentGame.Id);
                MessageBox.Show(message, "Game not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        void UpdateDirectoryItem(ToolStripMenuItem item, string tag) {
            item.Tag = tag;
            item.Enabled = Directory.Exists(tag);
        }
        
        private void FillCaPackMenu() {
            string shogunPath = GameManager.Instance.CurrentGame.GameDirectory;
            openCAToolStripMenuItem.DropDownItems.Clear();
            openCAToolStripMenuItem.Enabled = shogunPath != null;
            if (shogunPath != null) {
                shogunPath = Path.Combine(shogunPath, "data");
                if (Directory.Exists(shogunPath)) {
                    List<string> packFiles = new List<string> (Directory.GetFiles(shogunPath, "*.pack"));
                    packFiles.Sort(NumberedFileComparator.Instance);
                    packFiles.ForEach(file => openCAToolStripMenuItem.DropDownItems.Add(
                        new ToolStripMenuItem(Path.GetFileName(file), null, 
                                         delegate(object s, EventArgs a) { 
                        OpenExistingPackFile(file, true); 
                    })));
                }
            }
        }

        void FillRecentFilesList()
        {
            recentFilesMenuItem.DropDownItems.Clear();
            for (int i = 0; i < PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.Count; i++)
            {
                var file = PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles[i];
                var item = new ToolStripMenuItem($"{i+1} {file}", null,
                            delegate (object s, EventArgs a)
                            {
                                OpenExistingPackFile(file, true);
                            });

                recentFilesMenuItem.DropDownItems.Add(item);
            }
        }

        void AddNewRecentFile(string filePath)
        {
            int maxRecentFiles = 5;

            // Remove the file if it is add already
            var index = PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.IndexOf(filePath);
            if (index != -1)
                PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.RemoveAt(index);

            // Add the file
            PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.Insert(0, filePath);

            // Ensure we only have maxRecentFiles in the list
            var currentFileCount = PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.Count;
            if (currentFileCount > maxRecentFiles)
            {
                PackFileManagerSettingManager.CurrentSettings.RecentUsedFiles.RemoveRange(maxRecentFiles, currentFileCount - maxRecentFiles);
            }
            PackFileManagerSettingManager.Save();
        }

        //class PackFileManagerSettings

        private void ChangeGameIcon() {
            if(GameManager.Instance.CurrentGame == Game.STW)
                this.Icon = Resources.Shogun;
            else if(GameManager.Instance.CurrentGame == Game.NTW)
                this.Icon = Resources.Napoleon;
            else if(GameManager.Instance.CurrentGame == Game.ETW)
                this.Icon = Resources.Empire;
            else if(GameManager.Instance.CurrentGame == Game.R2TW)
                this.Icon = Resources.Rome2;
            else if(GameManager.Instance.CurrentGame == Game.ATW)
                this.Icon = Resources.Attila;
            else if(GameManager.Instance.CurrentGame == Game.TWH)
                this.Icon = Resources.Warhammer;
            else if(GameManager.Instance.CurrentGame == Game.TWH2)
                this.Icon = Resources.Warhammer2;
            else if(GameManager.Instance.CurrentGame == Game.TOB)
                this.Icon = Resources.Britannia;
            else if(GameManager.Instance.CurrentGame == Game.TW3K)
                this.Icon = Resources.ThreeKingdoms;
        }

        protected void EnableMenuItems() {
            createReadMeToolStripMenuItem.Enabled = !CanWriteCurrentPack;
            changePackTypeToolStripMenuItem.Enabled = currentPackFile != null;
            exportFileListToolStripMenuItem.Enabled = currentPackFile != null;
            minimizeToolStripMenuItem.Enabled = currentPackFile != null;
            renameMultiToolStripMenuItem.Enabled = currentPackFile != null;

            filesMenu.Enabled = saveAsToolStripMenuItem.Enabled = 
                createReadMeToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = currentPackFile != null &&
                currentPackFile.IsModified;
            
            // enable and check correct type selection item
            if (currentPackFile == null) {
                changePackTypeToolStripMenuItem.Enabled = false;
            } else {
                changePackTypeToolStripMenuItem.Enabled = true;
                foreach(ToolStripItem i in changePackTypeToolStripMenuItem.DropDownItems) {
                    ToolStripMenuItem item = i as ToolStripMenuItem;
                    if (item == null) {
                        break;
                    }
                    item.Checked = (item.Tag.Equals(currentPackFile.Type));
                    switch ((PackType) item.Tag) {
                    case PackType.Mod:
                    case PackType.Movie:
                        item.Enabled = true;
                        break;
                    case PackType.Other:
                        item.Enabled = false;
                        break;
                    default:
                        item.Enabled = CanWriteCurrentPack;
                        break;
                    }
                }
                setShaderToolStripMenuItem.Checked = currentPackFile.Header.IsShader;
                setShaderToolStripMenuItem.Enabled = CanWriteCurrentPack;
            }
            
            addToolStripMenuItem.Enabled = CanWriteCurrentPack;
            createReadMeToolStripMenuItem.Enabled = CanWriteCurrentPack;

            // selection-depending items
            bool nodeSelected = packTreeView.SelectedNode != null;
            extractSelectedToolStripMenuItem.Enabled = nodeSelected;
            renameSelectedToolStripMenuItem.Enabled = nodeSelected;
            openMenuItem.Enabled = nodeSelected;

            bool isLeafNode = nodeSelected && (packTreeView.SelectedNode.Nodes.Count == 0);
            replaceFileToolStripMenuItem.Enabled = CanWriteCurrentPack && isLeafNode;

            bool isRootNode = nodeSelected && packTreeView.SelectedNode == packTreeView.Nodes[0];
            renameToolStripMenuItem.Enabled = CanWriteCurrentPack && nodeSelected && !isRootNode;
            deleteFileToolStripMenuItem.Enabled = CanWriteCurrentPack && nodeSelected && !isRootNode;
        }

        private void OpenDirectory(object sender, EventArgs args) {
            string pathToOpen = ((ToolStripMenuItem)sender).Tag as string;
            if (pathToOpen != null && Directory.Exists(pathToOpen)) {
                Process.Start("explorer", pathToOpen);
            }
        }
        #endregion

        #region Open Pack
        private void newToolStripMenuItem_Click(object sender, EventArgs e) {
            if (QuerySaveModifiedFile() == System.Windows.Forms.DialogResult.No) {
                NewMod("Untitled.pack");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog packOpenFileDialog = new OpenFileDialog {
                InitialDirectory = Settings.Default.LastPackDirectory,
                Filter = IOFunctions.PACKAGE_FILTER
            };
            if ((QuerySaveModifiedFile() != DialogResult.Cancel) && 
                (packOpenFileDialog.ShowDialog() == DialogResult.OK)) {
                OpenExistingPackFile(packOpenFileDialog.FileName);
            }
        }

        private void OpenExistingPackFile(string filepath, bool querySaveCurrent = false) {
            if (querySaveCurrent && QuerySaveModifiedFile() == DialogResult.Cancel) {
                return;
            }

            Settings.Default.LastPackDirectory = Path.GetDirectoryName(filepath);
            try {
                var codec = new PackFileCodec();
                new LoadUpdater(codec, filepath, packStatusLabel, packActionProgressBar);
                CurrentPackFile = codec.Open(filepath);
            } catch (Exception exception) {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                packTreeView.Enabled = true;
            }
#if DEBUG
            Console.WriteLine("{0}allowing label edit", (CanWriteCurrentPack ? "" : "dis"));
#endif

            AddNewRecentFile(filepath);
            packTreeView.LabelEdit = CanWriteCurrentPack;
        }
        #endregion

        #region Save Pack
        private bool CanWriteCurrentPack {
            get {
                bool result = currentPackFile != null;
                if (result && cAPacksAreReadOnlyToolStripMenuItem.Checked) {
                    switch (currentPackFile.Type) {
                        case PackType.Mod:
                            // mod files can always be saved
                            result = true;
                            break;
                        case PackType.Movie:
                            // exclude files named patch_moviesX.pack
                            var caMovieRe = new Regex("(patch_)?movies([0-9]*).pack");
                            result = !caMovieRe.IsMatch(Path.GetFileName(currentPackFile.Filepath));
                            break;
                        default:
                            result = false;
                            break;
                    }
                }
                return result;
            }
        }

        static string CA_FILE_WARNING = "Will only save MOD and non-CA MOVIE files with current Setting.";
        private void QueryNameAndSave(object sender, EventArgs e) {
            if (!CanWriteCurrentPack) {
                MessageBox.Show(CA_FILE_WARNING);
            } else {
                var dialog = new SaveFileDialog {
                    InitialDirectory = Settings.Default.LastPackDirectory,
                    AddExtension = true,
                    Filter = IOFunctions.PACKAGE_FILTER
                };
                if (dialog.ShowDialog() == DialogResult.OK) {
                    Settings.Default.LastPackDirectory = Path.GetDirectoryName(dialog.FileName);
                    SaveAsFile(dialog.FileName);
                }
            }
        }

        private void SaveCurrentPack(object sender, EventArgs e) {
            if (!CanWriteCurrentPack) {
                MessageBox.Show(CA_FILE_WARNING);
            } else if (currentPackFile.Filepath.EndsWith("Untitled.pack")) {
                // ask for a name first
                QueryNameAndSave(null, null);
            } else {
                CloseEditors();

                new PackFileCodec().Save(currentPackFile);
                OpenExistingPackFile(currentPackFile.Filepath);
            }
        }

        void SaveAsFile(string filename) {
            if (!CanWriteCurrentPack) {
                MessageBox.Show(CA_FILE_WARNING);
            } else {
                CloseEditors ();

                string tempFile = Path.GetTempFileName ();
                new PackFileCodec ().WriteToFile (tempFile, currentPackFile);
                if (File.Exists (filename)) {
                    File.Delete (filename);
                }
                File.Move (tempFile, filename);
                OpenExistingPackFile (filename);
            }
        }
        #endregion

        private void PackTypeItemSelected(object sender, EventArgs e) {
            foreach (ToolStripItem inMenu in changePackTypeToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem item = inMenu as ToolStripMenuItem;
                if (item == null) {
                    continue;
                }
                item.Checked = (sender == item);
                if (item.Checked) {
                    currentPackFile.Type = (PackType) item.Tag;
                }
            }
        }

        #region Game Menu
        private void loadGamePacksToolStripMenuItem_Click(object sender, EventArgs args) {
            if (MessageBox.Show("Are you sure you want to do this?\nIt takes quite long.", "Can I Has Load Time?",
                                MessageBoxButtons.YesNo) != DialogResult.Yes) {
                return;
            }
            if (QuerySaveModifiedFile() == DialogResult.Cancel) {
                return;
            }
            packStatusLabel.Text = "Collecting Game files";
            PackLoadSequence allFiles = new PackLoadSequence {
                IncludePacksContaining = delegate(string s) { return true; }
            };
            List<string> packPaths = allFiles.GetPacksLoadedFrom(GameManager.Instance.CurrentGame.GameDirectory);
            packPaths.Reverse();
            PackFile file = new PackFile("All Packs");
            PackFileCodec codec = new PackFileCodec();
            foreach(string path in packPaths) {
                new LoadUpdater(codec, path, packStatusLabel, packActionProgressBar);
                PackFile pack = codec.Open(path);
                pack.Files.ForEach(f => file.Add(f, true));
            }
            CurrentPackFile = file;
            CurrentPackFile.IsModified = false;
            DBReferenceMap.Instance.CurrentPack = CurrentPackFile;
        }
        #endregion
        
        #region MyMod
        private void newModMenuItem_Click(object sender, EventArgs e) {
            List<string> oldMods = ModManager.Instance.ModNames;
            string packFileName = ModManager.Instance.AddMod();
            if (packFileName != null) {
                // add mod entry to menu
                if (ModManager.Instance.CurrentModSet) {
                    if (!oldMods.Contains(ModManager.Instance.CurrentMod.Name)) {
                        modsToolStripMenuItem.DropDownItems.Insert(1, new ModMenuItem(ModManager.Instance.CurrentMod.Name, 
                                                                                      ModManager.Instance.CurrentMod.Name));
                    }
                }
                if (File.Exists(packFileName)) {
                    OpenExistingPackFile(packFileName, true);
                } else {
                    NewMod(Path.Combine(ModManager.Instance.CurrentModDirectory, packFileName));
                    OpenExistingPackFile(Path.Combine(ModManager.Instance.CurrentModDirectory, packFileName), true);
                }
            }
        }

        private void NewMod(string name) {
            string packType = GameManager.Instance.CurrentGame.DefaultPfhType;
            CurrentPackFile = new PackFile(name, new PFHeader(packType));
        }
        
        private void installModMenuItem_Click(object sender, EventArgs e) {
            if (QuerySaveModifiedFile() == DialogResult.Cancel) {
                return;
            }
            try {
                ModManager.Instance.InstallCurrentMod();
            } catch (Exception ex) {
                MessageBox.Show(string.Format("Install failed: {0}", ex), "Install Failed", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void uninstallModMenuItem_Click(object sender, EventArgs e) {
            try {
                ModManager.Instance.UninstallCurrentMod();
            } catch (Exception ex) {
                MessageBox.Show(string.Format("Uninstall failed: {0}", ex), "Uninstall Failed", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteCurrentToolStripMenuItem_Click(object sender, EventArgs e) {
            if (ModManager.Instance.CurrentModSet) {
                string current = ModManager.Instance.CurrentMod.Name;
                ModManager.Instance.DeleteCurrentMod();
                foreach (ToolStripItem item in modsToolStripMenuItem.DropDownItems) {
                    if (item.Text == current) {
                        modsToolStripMenuItem.DropDownItems.Remove(item);
                        break;
                    }
                }
            }
        }
        
        private void OpenCurrentModPack() {
            try {
                if (ModManager.Instance.CurrentModSet) {
                    string modPath = ModManager.Instance.CurrentMod.FullModPath;
                    if (File.Exists(modPath)) {
                        OpenExistingPackFile(modPath, true);
                    }
                }
            } catch { }
        }
        #endregion

        #region Entry Add/Delete
        VirtualDirectory AddTo {
            get {
                VirtualDirectory addTo;
                if (packTreeView.SelectedNode == null) {
                    addTo = CurrentPackFile.Root;
                } else {
                    addTo = packTreeView.SelectedNode.Tag as VirtualDirectory 
                        ?? packTreeView.SelectedNode.Parent.Tag as VirtualDirectory;
                }
                return addTo;
            }
        }

        // removes the part of the given path up to the current mod directory
        string GetPathRelativeToMod(string file) {
            string addBase = "" + Path.DirectorySeparatorChar;
            string modDir = ModManager.Instance.CurrentModDirectory;
            if (!string.IsNullOrEmpty(modDir) && file.StartsWith(modDir)) {
                Uri baseUri = new Uri(ModManager.Instance.CurrentModDirectory);
                Uri createPath = baseUri.MakeRelativeUri(new Uri(file));
                addBase = createPath.ToString().Replace('/', Path.DirectorySeparatorChar);
                addBase = addBase.Remove(0, addBase.IndexOf(Path.DirectorySeparatorChar) + 1);
                addBase = Uri.UnescapeDataString(addBase);
            }
            return addBase;
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e) {
            VirtualDirectory addToBase = (ModManager.Instance.CurrentModSet)
                ? currentPackFile.Root : AddTo;
            if (addToBase == null) {
                return;
            }
            var addReplaceOpenFileDialog = new OpenFileDialog {
                InitialDirectory = ImportExportDirectory,
                Multiselect = true
            };
            if (addReplaceOpenFileDialog.ShowDialog() == DialogResult.OK) {
                Settings.Default.ImportExportDirectory = Path.GetDirectoryName(addReplaceOpenFileDialog.FileName);
                try {
                    foreach (string file in addReplaceOpenFileDialog.FileNames) {
                        string addBase = (ModManager.Instance.CurrentModSet) 
                            ? GetPathRelativeToMod(file) : Path.GetFileName(file);
                        addToBase.Add(addBase, new PackedFile(file));
                    }
                } catch (Exception x) {
                    MessageBox.Show(x.Message, "Problem, Sir!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void AddFromPack(object sender, EventArgs args) {
            OpenFileDialog openDialog = new OpenFileDialog {
                InitialDirectory = GameManager.Instance.CurrentGame.DataDirectory,
                Filter = IOFunctions.PACKAGE_FILTER
            };
            if (openDialog.ShowDialog() == DialogResult.OK) {
                try {
                    PackFile importFrom = new PackFileCodec().Open(openDialog.FileName);
                    PackBrowseDialog packBrowser = new PackBrowseDialog {
                        PackFile = importFrom
                    };
                    if (packBrowser.ShowDialog() == DialogResult.OK) {
                        foreach(PackedFile file in packBrowser.SelectedFiles) {
                            currentPackFile.Add(file, true);
                        }
                    }
                } catch (Exception e) {
                    MessageBox.Show(string.Format ("Failed to open pack: {0}", e));
                }
            }
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e) {
            if (packTreeView.SelectedNode == null) {
                return;
            }
            var packedFiles = new List<PackedFile>();
            if ((packTreeView.SelectedNode == packTreeView.Nodes[0]) || (packTreeView.SelectedNode.Nodes.Count > 0)) {
                getPackedFilesFromBranch(packedFiles, packTreeView.SelectedNode.Nodes);
            } else {
                packedFiles.Add(packTreeView.SelectedNode.Tag as PackedFile);
            }
            foreach (PackedFile file in packedFiles) {
                file.Deleted = true;
            }
        }

        private void getPackedFilesFromBranch(List<PackedFile> packedFiles, TreeNodeCollection trunk, Predicate<PackedFile> filter = null) {
            foreach (TreeNode node in trunk)
            {
                if (node.Nodes.Count > 0) {
                    getPackedFilesFromBranch(packedFiles, node.Nodes, filter);
                }
                else if (filter == null || filter(node.Tag as PackedFile)) {
                    packedFiles.Add(node.Tag as PackedFile);
                }
            }
        }
        
        private void addDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
            DirectoryDialog addDirectoryDialog = new DirectoryDialog() {
                Description = "Add which directory?",
                SelectedPath = ImportExportDirectory
            };
            if (AddTo != null && addDirectoryDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    string basePath = addDirectoryDialog.SelectedPath;
                    VirtualDirectory addToBase = AddTo;
                    if (ModManager.Instance.CurrentModSet && basePath.StartsWith(ModManager.Instance.CurrentModDirectory)) {
                        string relativePath = GetPathRelativeToMod(basePath);
                        addToBase = CurrentPackFile.Root;
                        foreach (string pathElement in relativePath.Split(Path.DirectorySeparatorChar)) {
                            addToBase = addToBase.GetSubdirectory(pathElement);
                        }
                        addToBase = addToBase.Parent as VirtualDirectory;
                    }
                    addToBase.Add(addDirectoryDialog.SelectedPath);
                } catch (Exception x) {
                    MessageBox.Show(string.Format("Failed to add {0}: {1}", 
                                                  addDirectoryDialog.SelectedPath, x.Message), 
                                    "Failed to add directory");
                }
            }
        }

        private void createReadMeToolStripMenuItem_Click(object sender, EventArgs e) {
            var readme = new PackedFile { Name = "readme.xml", Data = new byte[0] };
            currentPackFile.Add(readme);
            OpenPackedFile(readme);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e) {
            packTreeView.SelectedNode.BeginEdit();
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog replaceFileDialog = new OpenFileDialog() {
                InitialDirectory = ImportExportDirectory,
                Multiselect = false
            };
            if (replaceFileDialog.ShowDialog() == DialogResult.OK) {
                Settings.Default.ImportExportDirectory = Path.GetDirectoryName(replaceFileDialog.FileName);
                PackedFile tag = packTreeView.SelectedNode.Tag as PackedFile;
                tag.Source = new FileSystemSource(replaceFileDialog.FileName);
            }
        }
        
        private void emptyDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
            VirtualDirectory dir = 
                (packTreeView.SelectedNode != null) ? packTreeView.SelectedNode.Tag as VirtualDirectory : CurrentPackFile.Root;
            if (dir != null) {
                try {
                    VirtualDirectory newDir = new VirtualDirectory() { Name = "empty" };
                    dir.Add(newDir);
                    foreach (TreeNode node in packTreeView.SelectedNode.Nodes) {
                        if (node.Tag == newDir) {
                            node.EnsureVisible();
                            node.BeginEdit();
                            break;
                        }
                    }
                } catch { }
            }
        }

        string ImportExportDirectory {
            get {
                return (ModManager.Instance.CurrentModSet) 
                    ? ModManager.Instance.CurrentMod.BaseDirectory 
                    : Settings.Default.ImportExportDirectory;
            }
        }

        private void dBFileFromTSVToolStripMenuItem_Click(object sender, EventArgs e) {
            VirtualDirectory addToBase = (ModManager.Instance.CurrentModSet)
                ? currentPackFile.Root : AddTo;
            if (addToBase != null) {
                OpenFileDialog openDBFileDialog = new OpenFileDialog {
                    InitialDirectory = ImportExportDirectory,
                    Filter = IOFunctions.TSV_FILTER
                };
                if (openDBFileDialog.ShowDialog() == DialogResult.OK) {
                    Settings.Default.ImportExportDirectory = Path.GetDirectoryName(openDBFileDialog.FileName);
                    try {
                        string addBase = (ModManager.Instance.CurrentModSet)
                            ? GetPathRelativeToMod(openDBFileDialog.FileName) : Path.GetFileName(openDBFileDialog.FileName);
                        using (FileStream filestream = File.OpenRead(openDBFileDialog.FileName)) {
                            string filename = Path.GetFileNameWithoutExtension(openDBFileDialog.FileName);
                            byte[] data;
                            if (openDBFileDialog.FileName.Contains(".loc.")) {
                                LocFile file = new LocFile();
                                using (StreamReader reader = new StreamReader(filestream)) {
                                    file.Import(reader);
                                    using (MemoryStream stream = new MemoryStream()) {
                                        new LocCodec().Encode(stream, file);
                                        data = stream.ToArray();
                                    }
                                }
                            } else {
                                DBFile file = new TextDbCodec().Decode(filestream);
                                data = PackedFileDbCodec.FromFilename(openDBFileDialog.FileName).Encode(file);
                                addBase = String.Format("/db/{0}/", file.CurrentType.Name);
                            }

                            PackedFile packedFile = new PackedFile { Data = data, Name = filename };
                            addToBase.Add(addBase, packedFile);
                            foreach(TreeNode node in GetAllContainedNodes(packTreeView.Nodes)) {
                                if (node.Tag == packedFile) {
                                    node.Expand();
                                    packTreeView.SelectedNode = node;
                                    OpenPackedFile(packedFile);
                                    break;
                                }
                            }
                        }
                    } catch (Exception x) {
                        MessageBox.Show(x.Message);
                    }
                }
            } else {
                MessageBox.Show("Select a directory to add to");
            }
        }
        #endregion

        #region Open Packed File
        private void openAsTextMenuItem_Click(object sender, EventArgs e) {
            OpenPackedFile(textFileEditorControl, packTreeView.SelectedNode.Tag as PackedFile);
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenExternal();
        }

        private void openDecodeToolMenuItem_Click(object sender, EventArgs e) {
            PackedFile packedFile = packTreeView.SelectedNode.Tag as PackedFile;
            if (packedFile != null) {
                DecodeTool.DecodeTool decoder = null;
                // best used if a db file...
                try {
                    string key = DBFile.Typename(packedFile.FullPath);
                    bool unicode = false;

                    if (GameManager.Instance.CurrentGame == Game.ETW ||
                        GameManager.Instance.CurrentGame == Game.NTW ||
                        GameManager.Instance.CurrentGame == Game.STW) {
                        unicode = true;
                    }
                    decoder = new DecodeTool.DecodeTool(unicode) { TypeName = key, Bytes = packedFile.Data };
                    decoder.ShowDialog();
                } catch (Exception ex) {
                    MessageBox.Show (string.Format("DecodeTool could not be opened:\n{0}", ex.Message), 
                                     "DecodeTool problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
#if DEBUG
                    Console.WriteLine(ex);
#endif
                } finally {
                    if (decoder != null) {
                        decoder.Dispose();
                    }
                }
            }
        }

        private void OpenPackedFile(object tag) {
            PackedFile packedFile = tag as PackedFile;
            if (packedFile == null) {
                return;
            }
            PackedFileSource source = packedFile.Source as PackedFileSource;
            string sourceInfo = (source != null) ? string.Format("offset {0}", source.Offset.ToString("x2")) : "from memory";
            packStatusLabel.Text = String.Format("Viewing {0} ({1})", packedFile.Name, sourceInfo);

            IPackedFileEditor editor = null;
            foreach(IPackedFileEditor e in PackedFileEditorRegistry.Editors) {
                if (e.CanEdit(packedFile)) {
                    editor = e;
                    break;
                }
            }
            
            OpenPackedFile(editor, packedFile);
        }
        
        private void OpenPackedFile(IPackedFileEditor editor, PackedFile packedFile) {
            if (editor != null) {
                try {
                    editor.ReadOnly = !CanWriteCurrentPack;
                    editor.CurrentPackedFile = packedFile;
                    if (!splitContainer1.Panel2.Controls.Contains(editor as Control)) {
                        splitContainer1.Panel2.Controls.Add(editor as Control);
                    }
                } catch (Exception ex) {
                    MessageBox.Show(string.Format("Failed to open {0}: {1}", Path.GetFileName(packedFile.FullPath), ex));
                }
            }
        }

        private void CloseEditors() {
            foreach(IPackedFileEditor editor in PackedFileEditorRegistry.Editors) {
                editor.Commit();
            }

            splitContainer1.Panel2.Controls.Clear();
        }

        private void OpenExternal() {
            PackedFile packed = packTreeView.SelectedNode.Tag as PackedFile;
            if (externalEditor.CanEdit(packed)) {
                Activated += PackFileManagerForm_GotFocus;
                externalEditor.CurrentPackedFile = packed;
            }
        }
        #endregion

        #region Mass-operate on Packed Files (extract, rename)
        /*
         * Safely retrieves all files in the currently open pack.
         */
        private ICollection<PackedFile> AllFiles {
            get {
                ICollection<PackedFile> result = (currentPackFile != null) 
                    ? currentPackFile.Files : new List<PackedFile>();
                return result;
            }
        }
        /*
         * Safely retrieves all files below the currently selected node.
         * Recursively into subdirectories if a directory is selected,
         * only the single node if a file is selected.
         */
        private ICollection<PackedFile> FilesInSelection {
            get {
                List<PackedFile> result = new List<PackedFile>();
                VirtualDirectory collectFrom = packTreeView.SelectedNode.Tag as VirtualDirectory;
                if (collectFrom != null) {
                    result.AddRange(collectFrom.AllFiles);
                } else if (packTreeView.SelectedNode.Tag is PackedFile) {
                    result.Add(packTreeView.SelectedNode.Tag as PackedFile);
                }
                return result;
            }
        }

        public string ExportDirectory {
            get {
                if (ModManager.Instance.CurrentModSet) {
                    return ModManager.Instance.CurrentModDirectory;
                }
                string exportDirectory = currentPackFile != null ? Path.GetDirectoryName(currentPackFile.Filepath) : null;
                exportDirectory = (exportDirectory == null) ? Settings.Default.LastPackDirectory : exportDirectory;
                DirectoryDialog directoryDialog = new DirectoryDialog {
                    Description = "Extract to what folder?",
                    SelectedPath = exportDirectory
                };
                directoryDialog.ShowDialog();
                exportDirectory = directoryDialog.SelectedPath;
                if (!string.IsNullOrEmpty(exportDirectory)) {
                    Settings.Default.LastPackDirectory = exportDirectory;
                }
                return exportDirectory;
            }
        }

        private void extractAllTsv_Click(object sender, EventArgs e) {
            string extractTo = ExportDirectory;
            if (!string.IsNullOrEmpty(extractTo)) {
                List<PackedFile> files = currentPackFile.Files;
                IExtractionPreprocessor tsvExport = new TsvExtractionPreprocessor();
                files.RemoveAll(f => !tsvExport.CanExtract(f));
                FileExtractor extractor = new FileExtractor(packStatusLabel, packActionProgressBar, extractTo) {
                    Preprocessor = tsvExport
                };
                extractor.ExtractFiles(files);
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e) {
            string extractTo = ExportDirectory;
            if (!string.IsNullOrEmpty(extractTo)) {
                new FileExtractor(packStatusLabel, packActionProgressBar, extractTo).ExtractFiles(AllFiles);
            }
        }        
        private void extractSelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            string extractTo = ExportDirectory;
            if (!string.IsNullOrEmpty(extractTo)) {
                new FileExtractor(packStatusLabel, packActionProgressBar, extractTo).ExtractFiles(FilesInSelection);
            }
        }

        private void renameAllToolStripMenuItem_Click(object sender, EventArgs e) {
            RenameFiles(AllFiles);
        }
        
        private void renameSelectedToolStripMenuItem_Click(object sender, EventArgs e) {
            RenameFiles(FilesInSelection);
        }
        
        private void RenameFiles(IEnumerable<PackedFile> files) {
            InputBox box = new InputBox {
                Text = "Enter prefix to prepend to files"
            };
            if (box.ShowDialog() == DialogResult.OK) {
                PackedFileRenamer renamer = new PackedFileRenamer(box.Input);
                renamer.Rename(files);
            }
        }

        private void exportUnknownToolStripMenuItem_Click(object sender, EventArgs e) {
            string extractTo = ExportDirectory;
            if (!string.IsNullOrEmpty(extractTo)) {
                var packedFiles = new List<PackedFile>();
                CurrentPackFile.Files.ForEach(f => { if (unknownDbFormat(f)) { packedFiles.Add(f); } });
                new FileExtractor(packStatusLabel, packActionProgressBar, extractTo).ExtractFiles(packedFiles);
            }
        }
        private bool unknownDbFormat(PackedFile file) {
            bool result = file.FullPath.StartsWith ("db");
            string buffer;
            result &= !PackedFileDbCodec.CanDecode (file, out buffer);
            return result;
        }
        #endregion
        
        #region DB Descriptions Menu
        private void saveToDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
            try {
                DBTypeMap.Instance.SaveToFile(Path.GetDirectoryName(Application.ExecutablePath), 
                                              "user");
                string message = "You just saved your own DB definitions in a new file.\n" +
                    "This means that these will be used instead of the ones received in updates from TWC.\n" +
                    "Once you have uploaded your changes and they have been integrated,\n" +
                    "please delete the file schema_user.xml.";
                MessageBox.Show(message, "New User DB Definitions created", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

            } catch (Exception x) {
                MessageBox.Show(string.Format("Could not save user db descriptions: {0}\n" + 
                                              "User file won't be used anymore. A backup has been made.", x.Message));
            }
        }

        private void updateAllToolStripMenuItem_Click(object sender, EventArgs e) {
            if (currentPackFile != null) {
                foreach (PackedFile packedFile in currentPackFile.Files) {
                    UpdatePackedFile(packedFile);
                }
            }
        }

        private void updateCurrentToolStripMenuItem_Click(object sender, EventArgs e) {
#if __MonoCS__
            if (dbFileEditorControl.CurrentPackedFile != null) {
                UpdatePackedFile(dbFileEditorControl.CurrentPackedFile);
            }
#else
#endif
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK) {
                DBTypeMap.Instance.initializeFromFile(dlg.FileName);
            }
            if (CurrentPackFile != null) {
                Refresh();
            }
        }
  
        private void updateToolStripMenuItem_Click(object sender, EventArgs ev) {
            TryUpdate(true, currentPackFile == null ? null : currentPackFile.Filepath);
        }

        static void TryUpdate(bool showSuccess = true, string currentPackFile = null) {
            try {
                DBFileTypesUpdater updater = new DBFileTypesUpdater(Settings.Default.SubscribeToBetaSchema);
                if (updater.NeedsSchemaUpdate) {
                    updater.UpdateSchema();
                    DBTypeMap.Instance.InitializeTypeMap(Path.GetDirectoryName(Application.ExecutablePath));
                    if (showSuccess) {
                        MessageBox.Show("DB File description updated.", "Update result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                } else if (showSuccess) {
                    MessageBox.Show("No update performed.", "Update result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                if (updater.NeedsPfmUpdate) {
                    if (MessageBox.Show(string.Format("A new version of PFM is available ({0})\nAutoinstall?", updater.LatestPfmVersion),
                                        "New Software version available",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) {
                        updater.UpdatePfm(currentPackFile);
                    }
                }
            } catch (Exception e) {
                MessageBox.Show(
                    string.Format("Update failed: \n{0}\n{1}", e.Message, e.StackTrace),
                    "Problem, sir!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePackedFile(PackedFile packedFile) {
            try {
                dbUpdater.UpdatePackedFile(packedFile);
#if __MonoCS__
                if (dbFileEditorControl.CurrentPackedFile == packedFile) {
                    dbFileEditorControl.Open();
                }
#else
                // Any need to update new editors packed file?
#endif
            } catch (Exception x) {
                MessageBox.Show(string.Format("Could not update {0}: {1}", Path.GetFileName(packedFile.FullPath), x.Message));
            }
        }
        #endregion

        #region Options Menu
        private void cAPacksAreReadOnlyToolStripMenuItem_CheckStateChanged(object sender, EventArgs e) {
            if (cAPacksAreReadOnlyToolStripMenuItem.CheckState == CheckState.Unchecked) {
                var advisory = new CaFileEditAdvisory();
                cAPacksAreReadOnlyToolStripMenuItem.CheckState = 
                    (advisory.DialogResult == DialogResult.Yes) ? CheckState.Unchecked : CheckState.Checked;
            }
            EnableMenuItems();
            
            if (packTreeView.SelectedNode != null) {
                OpenPackedFile(packTreeView.SelectedNode.Tag);
            }
            packTreeView.LabelEdit = CanWriteCurrentPack;
        }

        private void updateOnStartupToolStripMenuItem_Click(object sender, EventArgs e) {
            Settings.Default.UpdateOnStartup = updateOnStartupToolStripMenuItem.Checked;
        }

        private void subscribeToBetaToolStripMenuItem_Click(object sender, EventArgs e) {
            Settings.Default.SubscribeToBetaSchema = subscribeToBetaToolStripMenuItem.Checked;
        }

        private void showDecodeToolOnErrorToolStripMenuItem_Click(object sender, EventArgs e) {
            Settings.Default.ShowDecodeToolOnError = showDecodeToolOnErrorToolStripMenuItem.Checked;
        }

        private void extensionSelectionChanged(object sender, EventArgs e) {
            Settings.Default.TsvExtension = (sender as ToolStripMenuItem).Text;
            csvToolStripMenuItem.Checked = "csv".Equals(Settings.Default.TsvExtension);
            tsvToolStripMenuItem.Checked = "tsv".Equals(Settings.Default.TsvExtension);
        }
        #endregion
  
        #region Help Menu
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            var form = new Form {
                Text = string.Format("About Pack File Manager {0}", Application.ProductVersion),
                Size = new Size (0x177, 0x130),
                WindowState = FormWindowState.Normal,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent
            };
            var label = new Label {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = string.Format (
                    "\r\nPack File Manager {0} by daniu\r\n" +
                    "\r\nBased on original work by Matt Chambers\r\n" +
                    "\r\nPack File Manager Update for NTW by erasmus777\r\n" +
                    "\r\nPack File Manager Update for TWS2 by Lord Maximus and Porphyr\r\n" +
                    "Copyright 2009-2013 Distributed under the Simple Public License 2.0\r\n" +
                    "\r\nThanks to the hard work of the people at twcenter.net.\r\n" +
                    "\r\nIcons created by Lomp, Rome 2 icon by Sarsay\r\n" +
                    "\r\nSpecial thanks to alpaca, just, ancientxx, Delphy, Scanian, iznagi11, barvaz, " +
                    "Mechanic, mac89, badger1815, husserlTW, The Vicar, AveiMil, Jinarik, PietroMicca "+
                    "and many others!", Application.ProductVersion)
            };
            form.Controls.Add (label);
            form.ShowDialog (this);
        }
        #endregion

        #region Tree Handler
        static readonly Regex versionedRegex = new Regex("(.*) - version.*");
        private void packTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) {
            if (CanWriteCurrentPack) {
                PackEntry entry = e.Node.Tag as PackEntry;
                if ((e.Label != null) && (e.Label != e.Node.Text) && (entry != null)) {
                    string newName = e.Label;
                    if (versionedRegex.IsMatch(newName)) {
                        newName = versionedRegex.Match(newName).Groups[1].Value;
                    }
                    entry.Name = newName;
                }
            }
            e.CancelEdit = true;
        }

        private void packTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
            if (e.Action != TreeViewAction.ByKeyboard && e.Action != TreeViewAction.ByMouse) {
#if DEBUG
                Console.WriteLine("Ignoring tree selection");
#endif
                return;
            }
#if DEBUG
            Console.WriteLine("handling tree selection");
#endif
            CloseEditors();
            splitContainer1.Panel2.Controls.Clear();
         
            if (packTreeView.SelectedNode != null) {
                packTreeView.LabelEdit = packTreeView.Nodes.Count > 0 && packTreeView.SelectedNode != packTreeView.Nodes[0];
                if (packTreeView.SelectedNode.Tag is PackedFile) {
                    OpenPackedFile(packTreeView.SelectedNode.Tag);
                }
            }
            // packStatusLabel.Text = PackInfo;
            EnableMenuItems();
        }
        
        private string PackInfo {
            get {
                string result = "";
                if (currentPackFile != null) {
                    result = String.Format("Data start at {0}", currentPackFile.Header.DataStart);
                }
                return result;
            }
        }

        private void packTreeView_MouseDoubleClick(object sender, MouseEventArgs e) {
            TreeNode nodeAt = packTreeView.GetNodeAt(e.Location);
            if ((nodeAt != null) && (e.Button == MouseButtons.Right)) {
                packTreeView.SelectedNode = nodeAt;
            }
            if ((packTreeView.SelectedNode != null) && (packTreeView.SelectedNode.Tag != null)) {
                OpenExternal();
            }
        }

        private void packTreeView_MouseDown(object sender, MouseEventArgs e) {
            TreeNode nodeAt = packTreeView.GetNodeAt(e.Location);
            if ((nodeAt != null) && (e.Button == MouseButtons.Right)) {
                packTreeView.SelectedNode = nodeAt;
            }
        }

        private void packTreeView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Insert:
                    if (!e.Shift) {
                        addFileToolStripMenuItem_Click(this, EventArgs.Empty);
                        break;
                    }
                    addDirectoryToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;

                case Keys.Delete:
                    if (packTreeView.SelectedNode != null) {
                        deleteFileToolStripMenuItem_Click(this, EventArgs.Empty);
                    }
                    break;

                case Keys.X:
                    if (e.Control) {
                        extractSelectedToolStripMenuItem_Click(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        private void packTreeView_ItemDrag(object sender, ItemDragEventArgs e) {
            // Proceed with the drag-and-drop, passing the selected items for 
            if (e.Button == MouseButtons.Left && e.Item is TreeNode && e.Item != null &&
                ((TreeNode)e.Item).Tag is PackedFile && ((TreeNode)e.Item).Tag != null) {
                var file = ((TreeNode)e.Item).Tag as PackedFile;
                if (file != null) {
                    var dataObject = new DataObject();
                    var filesInfo = new DragFileInfo(file.FullPath, file.Size);

                    using (MemoryStream infoStream = DragDropHelper.GetFileDescriptor(filesInfo),
                                        contentStream = DragDropHelper.GetFileContents(file.Data)) {
                        dataObject.SetData(DragDropHelper.CFSTR_FILEDESCRIPTORW, infoStream);
                        dataObject.SetData(DragDropHelper.CFSTR_FILECONTENTS, contentStream);
                        dataObject.SetData(DragDropHelper.CFSTR_PERFORMEDDROPEFFECT, null);

                        DoDragDrop(dataObject, DragDropEffects.All);
                    }
                }
            }
        }
        #endregion

        #region User Query Dialogs
        private void QueryModGameChange() {
            string currentGameId = GameManager.Instance.CurrentGame.Id;
            string modGameId = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentMod.Game.Id : "";
            if (CurrentPackFile != null &&  ModManager.Instance.CurrentModSet && !currentGameId.Equals(modGameId)) {
                string message = string.Format("Note that {0}'s database structure may not be compatible " +
                                               "with the currently opened pack's.", 
                                               currentGameId);
                MessageBox.Show(message);
            }
        }

        private DialogResult QuerySaveModifiedFile() {
            if ((currentPackFile != null) && currentPackFile.IsModified) {
                switch (MessageBox.Show("You modified the pack file. Do you want to save your changes?", 
                                        "Save Changes?", 
                                        MessageBoxButtons.YesNoCancel, 
                                        MessageBoxIcon.Exclamation, 
                                        MessageBoxDefaultButton.Button3)) {
                    case DialogResult.Yes:
                        SaveCurrentPack(this, EventArgs.Empty);
                        if (!currentPackFile.IsModified) {
                            break;
                        }
                        return DialogResult.Cancel;

                    case DialogResult.No:
                        return DialogResult.No;

                    case DialogResult.Cancel:
                        return DialogResult.Cancel;
                }
            }
            return DialogResult.No;
        }

        private string QueryGuid(List<string> newGuid) {
            string guid = null;
            for (int index = 0; newGuid.Count > index && guid == null; index++) {
                string message = string.Format("There are more than one definitions for the maximum version.\nUse GUID {0}?",
                                               newGuid[index]);
                if (MessageBox.Show(message, "Choose GUID", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                    guid = newGuid[index];
                }
            }
            return guid;
        }
        #endregion
  
        #region Nodes from Tree
        private List<TreeNode> GetAllContainedNodes(TreeNodeCollection trunk) {
            List<TreeNode> nodes = new List<TreeNode>();
            GetAllContainedNodes(nodes, trunk);
            return nodes;
        }

        private void GetAllContainedNodes(List<TreeNode> nodes, TreeNodeCollection trunk) {
            foreach (TreeNode node in trunk) {
                nodes.Add(node);
                GetAllContainedNodes(nodes, node.Nodes);
            }
        }
        #endregion

        public override void Refresh() {
            // save currently opened nodes
            var expandedNodes = new List<string>();
            foreach (TreeNode node in GetAllContainedNodes(packTreeView.Nodes)) {
                if (node.IsExpanded && node is PackEntryNode) {
                    expandedNodes.Add((node.Tag as PackEntry).FullPath);
                }
            }
            string selectedNode = (packTreeView.SelectedNode != null) 
                ? (packTreeView.SelectedNode.Tag as PackEntry).FullPath : "";
            
            // rebuild tree
            packTreeView.Nodes.Clear();
            if (currentPackFile == null) {
                return;
            }
            TreeNode node2 = new DirEntryNode(CurrentPackFile.Root);
            packTreeView.BeginUpdate();
            packTreeView.Nodes.Add(node2);
            packTreeView.EndUpdate();

            // recover opened nodes and selection
            foreach (TreeNode node in GetAllContainedNodes(packTreeView.Nodes)) {
                string path = (node.Tag as PackEntry).FullPath;
                if (expandedNodes.Contains(path)) {
                    node.Expand();
                }
				if (path == selectedNode) {
                    packTreeView.SelectedNode = node;
                    OpenPackedFile(node.Tag as PackedFile);
                }
            }
            RefreshTitle();
            packStatusLabel.Text = PackInfo;          
            base.Refresh();
        }

        private void RefreshTitle() {
            string file = "";
            if (currentPackFile != null) {
                file = Path.GetFileName(currentPackFile.Filepath);
                if (currentPackFile.IsModified) {
                    file = file + " (modified)";
                }
                file += " - ";
            }
            Text = string.Format("{0}Pack File Manager {1} ({2})", file, Application.ProductVersion, GameManager.Instance.CurrentGame.Id);
        }
  
        #region Search
        private void searchFileToolStripMenuItem_Click(object sender, EventArgs e) {
            search = new CustomMessageBox();
            AddOwnedForm(search);
            search.lblMessage.Text = "Query:";
            search.Text = @"Search files\directories";
            findChild(packTreeView.Nodes[0]);
            search.Show();
        }

        private void findChild(TreeNode tnChild) {
            foreach (TreeNode node in tnChild.Nodes) {
                search.tnList.Add(node);
                findChild(node);
            }
        }
        #endregion

        private void OnFileDropDownOpening(object sender, EventArgs e)
        {
            FillRecentFilesList();
        }

        private void expandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (packTreeView.SelectedNode == null)
                return;

            packTreeView.BeginUpdate();
            if (!packTreeView.SelectedNode.IsExpanded)
                packTreeView.SelectedNode.ExpandAll();
            else
                packTreeView.SelectedNode.Collapse(false);
            packTreeView.EndUpdate();
        }


        private void PackTreeView_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            packTreeViewToolTip.RemoveAll();
            var pos = packTreeView.PointToClient(Cursor.Position);
            TreeNode selNode = (TreeNode)packTreeView.GetNodeAt(pos);

            if (selNode != null)
            {
                var toolTip = selNode.ToolTipText;
                if (selNode.Tag != null)
                {
                    var directory = selNode.Tag as VirtualDirectory;
                    if (directory != null)
                    {
                        var fileCount = directory.AllFiles.Count;
                        toolTip += "File count = " + fileCount;
                    }
                }

                if(!string.IsNullOrWhiteSpace(selNode.ToolTipText))
                    toolTip += "\n" + selNode.ToolTipText;
                packTreeViewToolTip.SetToolTip(packTreeView, toolTip);
            }
        }

        private void OnSearchKeyUp(object sender, KeyEventArgs e)
        {
            if (packTreeSearchTextBox.Text.Length >= 3)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _packTreeFilterService.Search(packTreeSearchTextBox.Text);
                }
                else
                {
                    _packTreeFilterService.DeplayedSearch(packTreeSearchTextBox.Text);
                } 
            }

            if (packTreeSearchTextBox.Text.Length == 0)
            {
                _packTreeFilterService.ClearSearch();
            }
          
        }    
    }
}

