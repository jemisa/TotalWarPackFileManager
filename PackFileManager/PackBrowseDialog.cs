using Aga.Controls.Tree;
using Common;
using PackFileManager.PackedTreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PackFileManager {
    public partial class PackBrowseDialog : Form 
    {
        TreeModel _treeModel = new TreeModel();
        public PackBrowseDialog() 
        {
            InitializeComponent();
            AcceptButton = okButton;
            CancelButton = cancelButton;
            packFileTree.Model = _treeModel;
            /*
             * Add/Remove to/from selected file list upon double click.
             */
            packFileTree.NodeMouseClick += delegate(object sender, TreeNodeAdvMouseEventArgs e) 
            {
                var node = e.Node.Tag as Node;
                PackedFile selected = node.Tag as PackedFile;
                if (selected != null) {
                    if (selectedFiles.Contains(selected)) 
                    {
                        selectedFiles.Remove(selected);
                        statusLabel.Text = string.Format("{0} removed", selected.FullPath);
                    } 
                    else 
                    {
                        selectedFiles.Add(selected);
                        statusLabel.Text = string.Format("{0} added", selected.FullPath);
                    }
                }
            };
            /*
             * Add all files below a directory upon right click.
             */
            packFileTree.NodeMouseDoubleClick += delegate(object sender, TreeNodeAdvMouseEventArgs e) 
            {
                var node = e.Node.Tag as Node;
                VirtualDirectory directory = node.Tag as VirtualDirectory;
                if (e.Button == MouseButtons.Right && directory != null) 
                {
                    directory.AllFiles.ForEach(f => 
                    {
                        if (!selectedFiles.Contains(f)) 
                        {
                            selectedFiles.Add(f);
                            statusLabel.Text = string.Format("{0} added", f.FullPath);
                        }
                    });
                }
            };
        }

        /*
         * The pack file to browse.
         */
        private PackFile pack;
        public PackFile PackFile 
        {
            private get { return pack; }
            set 
            {
                statusLabel.Text = "Double click to select file; right-click directory to add all files below";
                pack = value;
                _treeModel.Nodes.Clear();
                selectedFiles.Clear();
                if (value != null) 
                {
                    TreeViewModelCreator creator = new TreeViewModelCreator();
                    _treeModel.Nodes.Add(creator.Create(pack.Root, null));
                }
            }
        }
  
        /*
         * The files that were selected while browsing.
         * Selection happens by double-clicking on a file node.
         */
        private List<PackedFile> selectedFiles = new List<PackedFile>();
        public List<PackedFile> SelectedFiles {
            get {
                if (selectedFiles.Count == 0) {
                    TreeNodeAdv node = packFileTree.SelectedNode;
                    if (node != null) 
                    {
                        var nodeTag = node.Tag as Node;
                        PackedFile selected = nodeTag.Tag as PackedFile;
                        if (selected != null) 
                            selectedFiles.Add(selected);
                    }
                }
                return selectedFiles;
            }
        }
    }
}
