﻿using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonDialogs
{
    public partial class LoadedPackFileBrowser : Form
    {
        public LoadedPackFileBrowser(PackFile currentPackFile)
        {
            InitializeComponent();
            packedTreeView.BuildTreeFromPackFile(currentPackFile);
        }
    }
}
