using Common;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PackFileManager.Properties;
using CommonDialogs;

namespace PackFileManager {
    /*
     * Class representing a Warscape mod.
     */
    public class Mod {
        /* 
         * Name of the mod.
         * Also expected to be the name of the pack file containing it.
         */
        public string Name { get; set; }
        public delegate void Notification();
        public event Notification GameChanged;

        /*
         * Directory to save mod pack and extract/import data to and from.
         */
        private string dir;
        public string BaseDirectory { 
            get {
                return dir;
            }
            set {
                dir = value;
            }
        }
        
        /*
         * The game to which this mod belongs.
         */
        private Game game;
        public Game Game { 
            get {
                return game;
            }
            set {
                game = value;
                if (GameChanged != null) {
                    GameChanged();
                }
            }
        }

        /*
         * Name of the pack file.
         */
        public string PackName {
            get {
                return string.Format("{0}.pack", Name);
            }
        }
        /*
         * Line to put into the game script file to tell game to load this mod.
         */
        public string ModScriptFileEntry {
            get {
                return string.Format("mod \"{0}\";", PackName);
            }
        }
        /*
         * Full path of the working pack file of this mod.
         */
        public string FullModPath {
            get {
                return Path.Combine(BaseDirectory, PackName);
            }
        }

        #region Overrides
        public override bool Equals(object obj) {
            bool result = obj is Mod;
            if (result) {
                result = (obj as Mod).Name.Equals(Name);
            }
            return result;
        }
        public override int GetHashCode() {
            return Name.GetHashCode ();
        }
        #endregion
    }
 
    /*
     * Manager for all mods of a user.
     */
    public class ModManager {
        const string MOD_SPACE_MESSAGE = "Your current mod name contains spaces, which Rome 2's Mod Manager can't handle.\n" +
                        "Replace spaces with underline?"; 

        // singleton
        public static readonly ModManager Instance = new ModManager();
        
        /*
         * Allow other code to react to a user changing the current mod.
         */
        public delegate void ModChangeEvent();
        public event ModChangeEvent CurrentModChanged;
  
        /*
         * Read mod list from current settings.
         */
        private ModManager() {
            if (File.Exists(ModListSaveFile)) {
                foreach(string line in File.ReadAllLines(ModListSaveFile)) {
                    Mod decoded = DecodeMod(line);
                    if (decoded != null) {
                        mods.Add(decoded);
                    }
                }
            }
            GameManager.Instance.GameChanged += SetModGame;
            if (File.Exists(CurrentModSaveFile)) {
                string lastSetMod = File.ReadAllText(CurrentModSaveFile).Trim();
                SetCurrentMod(lastSetMod);
            }
            // before 3.1.1, the mod list was saved in the settings;
            // try to restore those.
            RestoreLegacyModList();
        }
        
        private void RestoreLegacyModList() {
            try {
                string encodedList = Settings.Default["ModList"] as string;
                if (!string.IsNullOrEmpty(encodedList)) {
                    foreach(string encodedMod in encodedList.Split(new string[] {"@@@"}, StringSplitOptions.RemoveEmptyEntries)) {
                        Mod toAdd = DecodeMod(encodedMod);
                        if (toAdd != null) {
                            mods.Add(toAdd);
                        }
                    }
                    Settings.Default["ModList"] = null;
                }
                string currentInSettings = Settings.Default["CurrentMod"] as string;
                if (!string.IsNullOrEmpty(currentInSettings)) {
                    SetCurrentMod(currentInSettings);
                    Settings.Default["CurrentMod"] = null;
                }
                Settings.Default.Save();
            } catch {}
        }
  
        /*
         * When the user selects a different game with a mod being worked on,
         * offer to set the mod's game to the newly selected game.
         */
        private void SetModGame() {
            Game currentGame = GameManager.Instance.CurrentGame;
            if (CurrentModSet && !CurrentMod.Game.Id.Equals(currentGame.Id)) {
                string message = string.Format("Game set to {0}.\nDo you want to change the game setting for the current mod {1} (currently {2})?",
                                               currentGame.Id, CurrentMod.Name, CurrentMod.Game.Id);
                DialogResult answer = MessageBox.Show(message, "Modded Game Changed", MessageBoxButtons.YesNo);
                if (answer == DialogResult.Yes) {
                    CurrentMod.Game = currentGame;
                    StoreToSettings();
                }
            }
        }
        
        /*
         * Query if any mod is active at all right now.
         */
        public bool CurrentModSet {
            get {
                return CurrentMod != null;
            }
        }

        private List<Mod> mods = new List<Mod>();
        public List<string> ModNames {
            get {
                List<string> result = new List<string>();
                mods.ForEach(m => result.Add(m.Name));
                return result;
            }
        }
  
        #region Add, Deletion, Change of Mods
        /*
         * Query user for new mod name and its directory; then opens existing mod pack there
         * or lets user import initial data from a game pack.
         */
        public string AddMod() {
            string result = null;
            InputBox box = new InputBox { Text = "Enter Mod Name:", Input = "my_mod" };
            if (box.ShowDialog() == System.Windows.Forms.DialogResult.OK && box.Input.Trim() != "") {
                string modName = box.Input;
                if (modName.Contains(" ")) {
                    if (MessageBox.Show(MOD_SPACE_MESSAGE, "Mod name warning", MessageBoxButtons.YesNo)
                        == DialogResult.Yes) {
                        modName = modName.Replace(" ", "_");
                    }
                }
                string newModDir = Settings.Default.LastPackDirectory;
                if (newModDir != null) {
                    newModDir = Path.GetDirectoryName(newModDir);
                    newModDir = Path.Combine(newModDir, modName);
                }
                DirectoryDialog dialog = new DirectoryDialog {
                    SelectedPath = newModDir
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Mod mod = new Mod {
                        Name = modName,
                        BaseDirectory = dialog.SelectedPath
                    };
                    // create directory if it doesn't already exist
                    Directory.CreateDirectory(mod.BaseDirectory);

                    // create new mod file to start off with
                    result = Path.Combine(mod.BaseDirectory, string.Format("{0}.pack", modName));
                    if (Directory.Exists(mod.BaseDirectory) && !File.Exists(result)) {
                        PackFile newFile = new PackFile(result, new PFHeader(GameManager.Instance.CurrentGame.DefaultPfhType));
                        ImportDataFromGame(newFile);
                        new PackFileCodec().WriteToFile(result, newFile);
                    }
     
                    mod.Game = GameManager.Instance.CurrentGame;
                    AddMod(mod);
                }
            }
            return result;
        }

        /*
         * Query user for a pack to open to import data from, then open pack browser to
         * let user select the packed files he wants to start off with.
         * Selected packed files will be added to given pack.
         */
        private void ImportDataFromGame(PackFile newFile) {
            // open existing CA pack or create new pack
            string gamePath = GameManager.Instance.CurrentGame.GameDirectory;
            if (gamePath != null && Directory.Exists(gamePath)) {
                OpenFileDialog packOpenFileDialog = new OpenFileDialog {
                    InitialDirectory = Path.Combine(gamePath, "data"),
                    Filter = IOFunctions.PACKAGE_FILTER,
                    Title = "Open pack to extract basic data from"
                };
                if (packOpenFileDialog.ShowDialog() == DialogResult.OK) {
                    try {
                        PackBrowseDialog browser = new PackBrowseDialog {
                            PackFile = new PackFileCodec().Open(packOpenFileDialog.FileName)
                        };
                        if (browser.ShowDialog() == DialogResult.OK) {
                            foreach(PackedFile packed in browser.SelectedFiles) {
                                newFile.Add(packed, false);
                            }
                        }
                    } catch (Exception e) {
                        MessageBox.Show(string.Format("Failed to import data: {0}", e));
                    }
                }
            }
        }
        public void AddMod(Mod mod) {
            mods.Add(mod);
            mod.GameChanged += delegate { StoreToSettings(); };
            StoreToSettings ();
            CurrentMod = mod;
        }

        void StoreToSettings() {
            // Settings.Default.ModList = EncodeMods();
            using(var writer = File.CreateText(ModListSaveFile)) {
                foreach(Mod mod in mods) {
                    writer.WriteLine(EncodeMod(mod));
                }
            }
        }
        string ModListSaveFile {
            get {
                return Path.Combine(DirectoryHelper.FpmDirectory, "modlist.txt");
            }
        }
        #endregion
  
        #region Current Mod
        /*
         * Retrieve the currently active mod.
         */
        string currentMod = "";
        public Mod CurrentMod {
            get {
                // the current mod is stored in the settings
                return FindByName(currentMod);
            }
            set {
                currentMod = (value != null) ? value.Name : "";
                if (!string.IsNullOrEmpty(currentMod)) {
                    GameManager.Instance.CurrentGame = CurrentMod.Game;
                }
                if (CurrentModChanged != null) {
                    CurrentModChanged();
                }
                using (var writer = File.CreateText(CurrentModSaveFile)) {
                    writer.WriteLine(currentMod);
                }
                Console.WriteLine("Current mod now {0}", currentMod);
            }
        }
        /*
         * Change currently selected mod by its mod name.
         */
        public void SetCurrentMod(string modname) {
            CurrentMod = FindByName(modname);
        }
        /*
         * Delete the currently active mod.
         */
        public void DeleteCurrentMod() {
            if (CurrentMod != null) {
                mods.Remove(CurrentMod);
                StoreToSettings ();
                SetCurrentMod("");
            }
        }
        public string CurrentModDirectory {
            get {
                string result = (CurrentMod != null) ? CurrentMod.BaseDirectory : null;
                return result;
            }
        }
        static string CurrentModSaveFile {
            get {
                return Path.Combine(DirectoryHelper.FpmDirectory, "currentMod.txt");
            }
        }
        #endregion

        private Mod FindByName(string name) {
            Mod result = null;
            foreach(Mod m in mods) {
                if (m.Name.Equals(name)) {
                    result = m;
                    break;
                }
            }
            return result;
        }
  
        #region Install/Uninstall
        public void InstallCurrentMod() {
            if (CurrentMod == null) {
                throw new InvalidOperationException("No mod set");
            }
            string targetDir = CurrentMod.Game.GameDirectory;
            if (targetDir == null) {
                throw new FileNotFoundException(string.Format("Game install directory not found"));
            }
            targetDir = Path.Combine(targetDir, "data");
            if (File.Exists(CurrentMod.FullModPath) && Directory.Exists(targetDir)) {

                string installPackName = CurrentMod.PackName;
                if (installPackName.Contains(' ') && GameManager.Instance.CurrentGame == Game.R2TW) {
                    if (MessageBox.Show(MOD_SPACE_MESSAGE, "Invalid pack file name", MessageBoxButtons.YesNo) 
                        == DialogResult.Yes) {
                            installPackName = installPackName.Replace(' ', '_');
                    }
                }
                string targetFile = Path.Combine(targetDir, installPackName);
                
                // copy to data directory
                File.Copy(CurrentMod.FullModPath, targetFile, true);
                
                // add entry to user.script.txt if it's a mod file
                PFHeader header = PackFileCodec.ReadHeader(targetFile);
                if (header.Type == PackType.Mod) {
                    string modEntry = CurrentMod.ModScriptFileEntry;
                    string scriptFile = GameManager.Instance.CurrentGame.ScriptFile;
                    List<string> linesToWrite = new List<string>();
                    if (File.Exists(scriptFile)) {
                        // retain all other mods in the script file; will add our mod afterwards
                        foreach(string line in File.ReadAllLines(scriptFile, Encoding.Unicode)) {
                            if (!line.Contains(modEntry)) {
                                linesToWrite.Add(line);
                            }
                        }
                    }
                    if (!linesToWrite.Contains(modEntry)) {
                        linesToWrite.Add(modEntry);
                    }
                    File.WriteAllLines(scriptFile, linesToWrite, Encoding.Unicode);
                }
            }
        }

        public void UninstallCurrentMod() {
            if (CurrentMod == null) {
                throw new InvalidOperationException("No mod set");
            }
            
            string targetDir = GameManager.Instance.CurrentGame.GameDirectory;
            if (targetDir == null) {
                throw new FileNotFoundException(string.Format("Install directory not found"));
            }

            string targetFile = Path.Combine(targetDir, "data", CurrentMod.Name);
            if (File.Exists(targetFile)) {
                File.Move(targetFile, string.Format("{0}.old", targetFile));
            }

            string modEntry = CurrentMod.ModScriptFileEntry;
            string scriptFile = GameManager.Instance.CurrentGame.ScriptFile;
            List<string> linesToWrite = new List<string>();
            if (File.Exists(scriptFile)) {
                // retain all other mods in the script file
                foreach(string line in File.ReadAllLines(scriptFile, Encoding.Unicode)) {
                    if (!line.Contains(modEntry)) {
                        linesToWrite.Add(line);
                    } else {
                        linesToWrite.Add(string.Format("#{0}", modEntry));
                    }
                }
                File.WriteAllLines(scriptFile, linesToWrite, Encoding.Unicode);
            }
        }
        #endregion
        
        #region Helpers to Encode/Decode to Settings string 
        static Mod DecodeMod(string encoded) {
            Mod result = null;
            string[] nameDirTuple = encoded.Split(Path.PathSeparator);
            try {
                Game game = (nameDirTuple.Length > 2)?
                    Game.ById(nameDirTuple[2]) :
                    Game.STW;
                result = new Mod {
                    Name = nameDirTuple[0],
                    BaseDirectory = nameDirTuple[1],
                    Game = game
                };
            } catch { }
            return result;
        }
        static string EncodeMod(Mod mod) {
            return string.Format("{0}{1}{2}{1}{3}", mod.Name, Path.PathSeparator, mod.BaseDirectory, mod.Game.Id);
        }
        #endregion
    }
 
    /*
     * A menu item representing a mod for the user to select.
     */
    public class ModMenuItem : ToolStripMenuItem {
        public ModMenuItem(string title, string modName)
            : base(title) {
            if (ModManager.Instance.CurrentModSet) {
            string currentMod = ModManager.Instance.CurrentMod.Name;
                Checked = currentMod == modName;
            }
            ModManager.Instance.CurrentModChanged += CheckSelection;
            Tag = modName;
        }
        // select this item's mod as current mod in manager when clicked
        protected override void OnClick(EventArgs e) {
            ModManager.Instance.SetCurrentMod(Tag as string);
        }
        // check if the new selected mod is the one referred to by this item
        private void CheckSelection() {
            string selected = ModManager.Instance.CurrentMod != null ? ModManager.Instance.CurrentMod.Name : "";
            Checked = (Tag as string).Equals(selected);
        }
    }
}
