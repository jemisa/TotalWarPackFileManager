using Common;
using Filetypes;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonDialogs;

namespace CommonUtilities
{
    /*
     * Class extracting a number of packed files, possibly processing them on the way.
     */
    public class FileExtractor {
        private ToolStripStatusLabel packStatusLabel;
        private ToolStripProgressBar packActionProgressBar;

        public IExtractionPreprocessor Preprocessor {
            get;
            set;
        }

        private string exportDirectory;

        public FileExtractor(ToolStripStatusLabel l, ToolStripProgressBar b, string exportTo) {
            packStatusLabel = l;
            packActionProgressBar = b;
            exportDirectory = exportTo;
            Preprocessor = new IdentityPreprocessor();
        }

        // Constructor for new DBEditorTableControl
        public FileExtractor(string exportTo)
        {
            exportDirectory = exportTo;
            Preprocessor = new IdentityPreprocessor();
        }

        /*
         * Extract the given files to the export directory, maintaing their relative path.
         * If a file exists already, the user is queried what to do, with the options
         * to not extract, overwrite, or rename; also to keep the answer for the remaining conflicts.
         */
        public void ExtractFiles(ICollection<PackedFile> packedFiles) {
            if (!string.IsNullOrEmpty(exportDirectory)) {
                FileAlreadyExistsDialog.Action action = FileAlreadyExistsDialog.Action.Ask;
                FileAlreadyExistsDialog.Action defaultAction = FileAlreadyExistsDialog.Action.Ask;
                SetStatusText(string.Format("Extracting file (0 of {0} files extracted, 0 skipped)", packedFiles.Count));
                if (packActionProgressBar != null) {
                    packActionProgressBar.Visible = true;
                    packActionProgressBar.Minimum = 0;
                    packActionProgressBar.Maximum = packedFiles.Count;
                    packActionProgressBar.Step = 1;
                    packActionProgressBar.Value = 0;
                }
                int extractedCount = 0;
                int skippedCount = 0;
                foreach (PackedFile file in packedFiles) {
                    if (!Preprocessor.CanExtract(file)) {
                        skippedCount++;
                        continue;
                    }
                    string path = Path.Combine(exportDirectory, Preprocessor.GetFileName(file));
                    if (File.Exists(path)) {
                        // file with that name already present: ask user what to do
                        string renamedFilename;
                        if (defaultAction == FileAlreadyExistsDialog.Action.Ask) {
                            FileAlreadyExistsDialog dialog = new FileAlreadyExistsDialog(path);
                            dialog.ShowDialog(null);
                            action = dialog.ChosenAction;
                            defaultAction = dialog.NextAction;
                        } else {
                            // we already asked, and the user chose for all conflicts
                            action = defaultAction;
                        }
                        switch (action) {
                        case FileAlreadyExistsDialog.Action.Skip:

                            // don't extract
                            skippedCount++;
                            SetStatus(file.FullPath, extractedCount, packedFiles.Count, skippedCount);
                            if (packActionProgressBar != null) {
                                packActionProgressBar.PerformStep();
                            }
                            Application.DoEvents();
                            continue;

                        case FileAlreadyExistsDialog.Action.RenameExisting:
                            
                            // extract after copying the existing file
                            renamedFilename = path + ".bak";
                            while (File.Exists(renamedFilename)) {
                                renamedFilename = renamedFilename + ".bak";
                            }
                            File.Move(path, renamedFilename);
                            break;
                            
                        case FileAlreadyExistsDialog.Action.RenameNew:
                            
                            // extract to new name
                            do {
                                path = path + ".new";
                            }
                            while (File.Exists(path));
                            break;
                            
                        case FileAlreadyExistsDialog.Action.Cancel:
                            
                            // cancel extraction altogether
                            SetStatusText("Extraction cancelled.");
                            if (packActionProgressBar != null) {
                                packActionProgressBar.Visible = false;
                            }
                            return;
                        }
                    } else {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    SetStatus(file.FullPath, extractedCount, packedFiles.Count, skippedCount);
                    Application.DoEvents();

                    try {
                        // perform the extraction, through the preprocessor
                        File.WriteAllBytes(path, Preprocessor.Process(file));
                        extractedCount++;
                    } catch (Exception e) {
                        MessageBox.Show(string.Format("Failed to export {0}: {1}", file.FullPath, e.Message));
                        skippedCount++;
                    }

                    SetStatus(file.FullPath, extractedCount, packedFiles.Count, skippedCount);
                    if (packActionProgressBar != null) {
                        packActionProgressBar.PerformStep();
                    }
                    Application.DoEvents();
                }
            }
        }
        void SetStatus(string currentFile, int extractedCount, int totalCount, int skippedCount) {
            SetStatusText(string.Format("({1} of {2} files extracted, {3} skipped): extracting {0}", 
                new object[] { currentFile, extractedCount, totalCount, skippedCount }));
        }
        void SetStatusText(string text) {
            if (packStatusLabel != null) {
                packStatusLabel.Text = text;
            }
        }
    }
    
    /*
     * An interface for classes performing processing before a file is extracted.
     */
    public interface IExtractionPreprocessor {
        bool CanExtract(PackedFile file);
        string GetFileName(PackedFile file);
        byte[] Process(PackedFile file);
    }
 
    /*
     * Processor doing nothing.
     */
    public class IdentityPreprocessor : IExtractionPreprocessor {
        public bool CanExtract(PackedFile file) { return true; }
        public string GetFileName(PackedFile path) { return path.FullPath; }
        public byte[] Process(PackedFile file) { return file.Data; }
    }
    
    /*
     * Processor encoding extracted DB and LOC files into TSV.
     */
    public class TsvExtractionPreprocessor : IExtractionPreprocessor {
        List<IExtractionPreprocessor> processors = new List<IExtractionPreprocessor>();
        public TsvExtractionPreprocessor() {
            processors.Add(new DbTsvExtractor());
            processors.Add(new LocTsvPreprocessor());
        }
        private IExtractionPreprocessor GetExtractor(PackedFile file) {
            IExtractionPreprocessor result = null;
            foreach(IExtractionPreprocessor e in processors) {
                if (e.CanExtract(file)) {
                    result = e;
                    break;
                }
            }
            return result;
        }
        public bool CanExtract(PackedFile file) {
            return GetExtractor(file) != null;
        }
        public string GetFileName(PackedFile file) {
            return GetExtractor(file).GetFileName(file);
        }
        public byte[] Process(PackedFile file) {
            return GetExtractor(file).Process(file);
        }
    }
 
    /*
     * Processor encoding DB files to TSV.
     */
    public class DbTsvExtractor : IExtractionPreprocessor {
        public bool CanExtract(PackedFile file) {
            return file.FullPath.StartsWith("db");
        }
        public string GetFileName(PackedFile path) {
            return string.Format("{0}.tsv", path.FullPath);
        }
        public byte[] Process(PackedFile file) {
            byte[] result = file.Data;
            using (MemoryStream stream = new MemoryStream()) {
                DBFile dbFile = PackedFileDbCodec.Decode(file);
                TextDbCodec.Instance.Encode(stream, dbFile);
                result = stream.ToArray();
            }
            return result;
        }
    }
    
    /*
     * Processor encoding LOC files to TSV.
     */
    public class LocTsvPreprocessor : IExtractionPreprocessor {
        public bool CanExtract(PackedFile file) {
            return file.FullPath.EndsWith(".loc");
        }
        public string GetFileName(PackedFile path) {
            return string.Format("{0}.tsv", path.FullPath);
        }
        public byte[] Process(PackedFile file) {
            byte[] result;
            MemoryStream stream = new MemoryStream();
            using (var writer = new StreamWriter(stream)) {
                LocFile locFile = LocCodec.Instance.Decode(file.Data);
                locFile.Export(writer);
                result = stream.ToArray();
            }
            return result;
        }
    }
}
