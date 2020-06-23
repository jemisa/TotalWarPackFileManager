﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace MMS {
    static class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try {
                Application.Run(new MainForm());
            } catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
