using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using EsfLibrary;

namespace EditSF {
    public class FileTester {
        public int TestsRun { get; private set; }
        public int TestSuccesses { get; set; }
        public string RunTest(string file, ToolStripProgressBar progress, ToolStripStatusLabel statusLabel) {
            string originalTitle = statusLabel.Text;
            statusLabel.Text = string.Format("Loading file {0}", Path.GetFileName(file));
            Application.DoEvents();
            string result;
            // ProgressUpdater updater = new ProgressUpdater(progress);
            using (Stream s = File.OpenRead(file)) {
                try {
                    EsfCodec codec = EsfCodecUtil.GetCodec(File.OpenRead(file));
                    if (codec != null) {
                        //updater.StartLoading(file, codec);
                        TestsRun++;
                        EsfFile esfFile = new EsfFile(s, codec);
                        string testFileName = file + "_test";
                        EsfCodecUtil.WriteEsfFile(testFileName, esfFile);
//                        using (BinaryWriter writer = new BinaryWriter(File.Create(testFileName))) {
//                            codec.EncodeRootNode(writer, esfFile.RootNode);
//                        }
                        statusLabel.Text = string.Format("Saving file {0}", Path.GetFileName(file));
                        Application.DoEvents();
                        using (Stream reloadStream = File.OpenRead(testFileName)) {
                            EsfFile reloadedFile = new EsfFile(reloadStream, codec);
                            if (esfFile.Equals(reloadedFile)) {
                                TestSuccesses++;
                                result = string.Format("success Test {0}", file);
                            } else {
                                result = string.Format("FAIL Test {0}: Reload of save file different from original", file);
                            }
                        }
                        Application.DoEvents();
                        File.Delete(testFileName);
                    } else {
                        result = string.Format("not running test on {0}", file);
                    }
                } catch (Exception e) {
                    result = string.Format("FAIL Test of {0}: {1}", file, e);
                }
                s.Close();
                //updater.LoadingFinished();
            }
            statusLabel.Text = originalTitle;
            return result;
        }
    }
}
