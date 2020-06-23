using System;
using System.IO;
using System.Windows.Forms;

namespace PackFileManager
{

    public class Program
    {
        public static PackFileManagerForm MainForm = null;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Program.CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            try
            {
                MainForm = new PackFileManagerForm(args);
                Application.Run(MainForm);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }
        
        /*
         * Get path to PFM setting folder; creates it if neccessary.
         */
        public static string ApplicationFolder {
            get {
                string localAppDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                localAppDir = Path.Combine(localAppDir, "PackFileManager");
                if (!Directory.Exists(localAppDir)) {
                    Directory.CreateDirectory(localAppDir);
                }
                return localAppDir;
            }
        }
    }
}

