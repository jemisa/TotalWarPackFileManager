using Common;
using CommonDialogs;
using CommonUtilities;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DBEditorTableControl
{
    class ImportExportHelper
    {
        public static void ExportTSVMenu(PackedFile CurrentPackedFile, string _exportDirectory)
        {
            string extractTo = null;
            // TODO: Add support for ModManager
            //extractTo = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentModDirectory : null;
            if (extractTo == null)
            {
                DirectoryDialog dialog = new DirectoryDialog
                {
                    Description = "Please point to folder to extract to",
                    SelectedPath = String.IsNullOrEmpty(_exportDirectory)
                                    ? System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                                    : _exportDirectory
                };
                extractTo = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
                _exportDirectory = dialog.SelectedPath;
            }
            if (!string.IsNullOrEmpty(extractTo))
            {
                List<PackedFile> files = new List<PackedFile>();
                files.Add(CurrentPackedFile);
                FileExtractor extractor = new FileExtractor(extractTo) { Preprocessor = new TsvExtractionPreprocessor() };
                extractor.ExtractFiles(files);
                MessageBox.Show(string.Format("File exported to TSV."));
            }
        }

        public static void ExportBinary(PackedFile CurrentPackedFile, string _exportDirectory)
        {
            string extractTo = null;
            // TODO: Add support for ModManager
            //extractTo = ModManager.Instance.CurrentModSet ? ModManager.Instance.CurrentModDirectory : null;
            if (extractTo == null)
            {
                DirectoryDialog dialog = new DirectoryDialog
                {
                    Description = "Please point to folder to extract to",
                    SelectedPath = String.IsNullOrEmpty(_exportDirectory)
                                    ? System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                                    : _exportDirectory
                };
                extractTo = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
                _exportDirectory = dialog.SelectedPath;
            }
            if (!string.IsNullOrEmpty(extractTo))
            {
                List<PackedFile> files = new List<PackedFile>();
                files.Add(CurrentPackedFile);
                FileExtractor extractor = new FileExtractor(extractTo);
                extractor.ExtractFiles(files);
                MessageBox.Show(string.Format("File exported as binary."));
            }
        }

        public static void ExportCAXml(PackedFile CurrentPackedFile, string _exportDirectory)
        {
            // TODO: Write PackedFile EncodeasCAXml()
            //Refresh();
        }


        static void Import(DBTableControl.DBEditorTableControl _parentDbEdtiorTable, string filename, ICodec<DBFile> codec)
        {
            System.Windows.Forms.OpenFileDialog openDBFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                InitialDirectory = _parentDbEdtiorTable._exportDirectory,
                FileName = filename
            };

            bool tryAgain = false;
            if (openDBFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _parentDbEdtiorTable._importDirectory = System.IO.Path.GetDirectoryName(openDBFileDialog.FileName);
                do
                {
                    try
                    {
                        try
                        {
                            using (var stream = new MemoryStream(File.ReadAllBytes(openDBFileDialog.FileName)))
                            {
                                var loadedfile = codec.Decode(stream);
                                // No need to import to editedFile directly, since it will be handled in the 
                                _parentDbEdtiorTable.Import(loadedfile);
                            }

                        }
                        catch (DBFileNotSupportedException exception)
                        {
                            _parentDbEdtiorTable.showDBFileNotSupportedMessage(exception.Message);
                        }

                        _parentDbEdtiorTable.CurrentPackedFile.Data = (_parentDbEdtiorTable._codec.Encode(_parentDbEdtiorTable.EditedFile));
                    }
                    catch (Exception ex)
                    {
                        tryAgain = (System.Windows.Forms.MessageBox.Show(string.Format("Import failed: {0}", ex.Message),
                            "Import failed",
                            System.Windows.Forms.MessageBoxButtons.RetryCancel)
                            == System.Windows.Forms.DialogResult.Retry);
                    }
                } while (tryAgain);
            }
        }

        static public void ImportTSV(DBTableControl.DBEditorTableControl _parentDbEdtiorTable)
        {
            Import(_parentDbEdtiorTable, String.Format("{0}.csv", _parentDbEdtiorTable.EditedFile.CurrentType.Name), new TextDbCodec());
        }

        static public void ImportCSV(DBTableControl.DBEditorTableControl _parentDbEdtiorTable)
        {
            Import(_parentDbEdtiorTable, String.Format("{0}.tsv", _parentDbEdtiorTable.EditedFile.CurrentType.Name), new TextDbCodec());
        }

        static public void ImportBinary(DBTableControl.DBEditorTableControl _parentDbEdtiorTable)
        {
            Import(_parentDbEdtiorTable, _parentDbEdtiorTable.EditedFile.CurrentType.Name, _parentDbEdtiorTable._codec);
        }

        static public void ImportCAXml(DBTableControl.DBEditorTableControl _parentDbEdtiorTable)
        {
            // TODO: Implement
        }
    }
}
