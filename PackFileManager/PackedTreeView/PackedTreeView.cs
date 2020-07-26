using System;
using System.Linq;
using System.Windows.Forms;
using Common;
using System.Text.RegularExpressions;
using System.IO;
using Aga.Controls.Tree;
using CommonDialogs;
using Aga.Controls.Tree.NodeControls;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Collections.Generic;

namespace PackFileManager.PackedTreeView
{

    public partial class PackedTreeView : UserControl, IPackEntryEventHandler
    {
        private class ToolTipProvider : IToolTipProvider
        {
            public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
            {
                var valueNode = node.Tag as TreeNode;
                if(valueNode != null)
                    return valueNode.ToolTipText;
                return "";
            }
        }

        public PackFileManagerForm _parentRef;
        TreeModel _treeModel;
        TreeViewModelCreator _treeViewModelCreator;

        public PackedTreeView()
        {
            _treeViewModelCreator = new TreeViewModelCreator();

            InitializeComponent();
            _treeModel = new TreeModel();

            treeViewAdv1.ItemDrag += new ItemDragEventHandler(this.packTreeView_ItemDrag);
            treeViewAdv1.NodeMouseClick += (sender, args) => treeViewAdv1.SelectedNode = args.Node;
            treeViewAdv1.Expanded += TreeViewAdv1_Expanded;
            treeViewAdv1.Expanding += TreeViewAdv1_Expanding;


            treeViewAdv1.Model = _treeModel;
            treeViewAdv1.NodeFilter = filter;

            nodeTextBox1.ToolTipProvider = new ToolTipProvider();
            nodeTextBox1.DrawText += new EventHandler<DrawTextEventArgs>(_nodeTextBox_DrawText);
        }



        void _nodeTextBox_DrawText(object sender, DrawTextEventArgs e)
        {
            var colourNode = (e.Node.Tag as TreeNode);
            var node = colourNode.Tag as PackEntry;
            if (node != null)
            {
                // If no value is set, get colour from the pack file.
                if (colourNode.Colour.HasValue == false)
                {
                    var colour = Color.Black;
                    if (node.IsAdded)
                        colour = Color.Green;
                    else if (node.IsRenamed)
                        colour = Color.LimeGreen;
                    else if (node.Deleted)
                        colour = Color.LightGray;
                    else if (node.Modified)
                        colour = Color.Red;

                    e.TextColor = colour;
                }
                else
                {
                    e.TextColor = colourNode.Colour.Value;
                }
            }
        }

        private void TreeViewAdv1_Expanding(object sender, TreeViewAdvEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
        }

        private void TreeViewAdv1_Expanded(object sender, TreeViewAdvEventArgs e)
        {
            treeViewAdv1.Expanded -= TreeViewAdv1_Expanded;

            var content = GetNodeContent(e.Node);
            if (content != null)
            {
                bool isAllFiles = false;
                var nodeAsDirectory = content as VirtualDirectory;
                if (nodeAsDirectory != null)
                {
                    if(nodeAsDirectory.Subdirectories.Values.Count() == 0)
                        isAllFiles = true;
                }

                if (isAllFiles)
                {
                    e.Node.ExpandAll();
                }
                else
                {
                    var children = e.Node.Children;
                    foreach (var child in children)
                    {
                        var childContent = GetNodeContent(child);
                        var file = childContent as PackedFile;
                        if (file != null)
                            child.Expand(true);
                    }
                }
            }

            Cursor.Current = Cursors.Default;
            treeViewAdv1.Expanded += TreeViewAdv1_Expanded;
        }

        public TreeViewAdv GetTreeView()
        {
            return treeViewAdv1;
        }

        public PackEntry GetNodeContent(TreeNodeAdv node)
        {
            if (node == null)
                return null;
            if ((node.Tag as Node) == null)
                return null;
            return (node.Tag as Node).Tag as PackEntry;
        }

        public PackEntry GetSelectedNodeContent()
        {
            return GetNodeContent(treeViewAdv1.SelectedNode);
        }

        public List<PackEntry> GetPackedFilesInSelection() 
        {
            var output = new List<PackEntry>();
            var selectedNode = treeViewAdv1.SelectedNode.Tag as Node;
            if (selectedNode == null)
                return output;

            GetPackedFilesInSelectionInternal(output, selectedNode);
            return output;
        }

        void GetPackedFilesInSelectionInternal(List<PackEntry> out_packedFiles, Node node)
        {
            if (node.Tag as PackedFile != null)
                out_packedFiles.Add(node.Tag as PackedFile);

            foreach (var childNode in node.Nodes)
                GetPackedFilesInSelectionInternal(out_packedFiles, childNode);
        }


        public bool IsSelectedNodeRoot()
        {
            return treeViewAdv1.SelectedNode == treeViewAdv1.Root;
        }

        public bool IsSelectedNoodLeaf()
        {
            if (treeViewAdv1.SelectedNode == null)
                return false;
            return treeViewAdv1.SelectedNode.Children.Count() == 0;
        }

        public void ExpandSelectedNode()
        {
            if (treeViewAdv1.SelectedNode == null)
                return;

            treeViewAdv1.BeginUpdate();
            if (!treeViewAdv1.SelectedNode.IsExpanded)
                treeViewAdv1.SelectedNode.ExpandAll();
            else
                treeViewAdv1.SelectedNode.Collapse(false);
            treeViewAdv1.EndUpdate();
        }

        public void BuildTreeFromPackFile(PackFile currentPackFile)
        {
            if (currentPackFile == null)
                return;

            treeViewAdv1.BeginUpdate();
            var nodes = _treeViewModelCreator.Create(currentPackFile.Root, this);
            _treeModel.Nodes.Clear();
            _treeModel.Nodes.Add(nodes);
            TreeViewColourHelper.SetColourBasedOnValidation(_treeModel.Nodes);
            treeViewAdv1.EndUpdate();
        }

        

        public void Dir_FileRemoved(PackEntry entry)
        {
            foreach (var node in _treeModel.Nodes)
            {
                if (node.Tag == entry)
                {
                    treeViewAdv1.BeginUpdate();
                    _treeModel.Nodes.Remove(node);
                    treeViewAdv1.EndUpdate();
                    return;
                }
            }
        }

        Node FindTreeNodeByPackEntry(Collection<Node> searchSpace, PackEntry target)
        {
            foreach (var node in searchSpace)
            {
                var data = node.Tag as PackEntry;
                if (data == target)
                    return node;

                var res = FindTreeNodeByPackEntry(node.Nodes, target);
                if (res != null)
                    return res;
            }

            return null;
        }

        public void Dir_FileAdded(PackEntry entry)
        {
            var parentNode = FindTreeNodeByPackEntry(_treeModel.Nodes, entry.Parent);
            if (parentNode == null)
                return;
            var newNode = _treeViewModelCreator.CreateNode(entry, this);
            parentNode.Nodes.Add(newNode);
        }

        public void ShowRenameSelectedNodeDialog()
        {
            if (!_parentRef.CanWriteCurrentPack)
            {
                MessageBox.Show("Unable to edit current pack as it is read only");
                return;
            }

            PackEntry entry = GetSelectedNodeContent();
            if (entry == null)
            {
                MessageBox.Show("Can only rename Pack Entries");
                return;
            }

            var inputDialog = new InputBox() { Text = "Rename file:" };
            inputDialog.StartPosition = FormStartPosition.CenterParent;
            inputDialog.ShowDialog(this);

            if (inputDialog.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string newName = inputDialog.Input;
                if (versionedRegex.IsMatch(newName))
                {
                    newName = versionedRegex.Match(newName).Groups[1].Value;
                }
                    
                entry.Name = newName;
            }
        }

        static readonly Regex versionedRegex = new Regex("(.*) - version.*");

        private bool filter(object obj)
        {
            TreeNodeAdv viewNode = obj as TreeNodeAdv;
            Node n = viewNode != null ? viewNode.Tag as Node : obj as Node;
            return n == null || n.Text.ToUpper().Contains(this._treeViewSearchBox.Text.ToUpper()) || n.Nodes.Any(filter);
        }


        private void packTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Proceed with the drag-and-drop, passing the selected items for 
            if (e.Button == MouseButtons.Left && e.Item is System.Windows.Forms.TreeNode && e.Item != null &&
                ((System.Windows.Forms.TreeNode)e.Item).Tag is PackedFile && ((System.Windows.Forms.TreeNode)e.Item).Tag != null)
            {
                var file = ((System.Windows.Forms.TreeNode)e.Item).Tag as PackedFile;
                if (file != null)
                {
                    var dataObject = new DataObject();
                    var filesInfo = new DragFileInfo(file.FullPath, file.Size);

                    using (MemoryStream infoStream = DragDropHelper.GetFileDescriptor(filesInfo),
                                        contentStream = DragDropHelper.GetFileContents(file.Data))
                    {
                        dataObject.SetData(DragDropHelper.CFSTR_FILEDESCRIPTORW, infoStream);
                        dataObject.SetData(DragDropHelper.CFSTR_FILECONTENTS, contentStream);
                        dataObject.SetData(DragDropHelper.CFSTR_PERFORMEDDROPEFFECT, null);

                        DoDragDrop(dataObject, DragDropEffects.All);
                    }
                }
            }
        }

        private void _treeViewSearchBox_TextChanged(object sender, EventArgs e)
        {
            treeViewAdv1.UpdateNodeFilter();
        }
    }


}
