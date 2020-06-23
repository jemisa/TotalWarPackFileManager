using EsfLibrary;
using System;
using System.IO;
using System.Windows.Forms;


namespace CameraFix {
    class MainClass {
        static string[] pathToCamera = {
            "COMPRESSED_DATA", "CAMPAIGN_ENV", "CAMPAIGN_CAMERA_MANAGER"
        };
        
        public static void Main(string[] args) {
            OpenFileDialog openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == DialogResult.OK) {
                string saveFile = openDialog.FileName;
                EsfFile file = EsfCodecUtil.LoadEsfFile(saveFile);
                ParentNode parent = file.RootNode as ParentNode;
                for (int i = 0; i < pathToCamera.Length; i++) {
                    if (parent == null) {
                        break;
                    }
                    parent = parent[pathToCamera[i]];
                }
                if (parent == null || parent.Values.Count == 0) {
                    MessageBox.Show("Could not find path to camera in save file :(");
                    return;
                }
                EsfValueNode<uint> node = parent.Values[0] as EsfValueNode<uint>;
                node.FromString("1");
                
                SaveFileDialog saveDialog = new SaveFileDialog {
                    InitialDirectory = Path.GetDirectoryName(openDialog.FileName)
                };
                if (saveDialog.ShowDialog() == DialogResult.OK) {
                    EsfCodecUtil.WriteEsfFile(saveDialog.FileName, file);
                }
            }
        }
    }
}
