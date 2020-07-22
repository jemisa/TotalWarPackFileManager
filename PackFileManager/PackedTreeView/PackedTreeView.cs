using System;
using System.Linq;
using System.Windows.Forms;
using Common;
using System.Text.RegularExpressions;
using System.IO;
using Aga.Controls.Tree;
using CommonDialogs;
using Aga.Controls.Tree.NodeControls;

namespace PackFileManager.ExtentedTreeView
{
    public partial class ExtendedTreeView : UserControl
    {
        private class ToolTipProvider : IToolTipProvider
        {
            public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
            {
                var valueNode = node.Tag as PackEntryNode;
                if(valueNode != null)
                    return valueNode.ToolTipText;
                return "";
            }
        }

        public PackFileManagerForm _parentRef;
        TreeModel _treeModel;

        public ExtendedTreeView()
        {
            InitializeComponent();
            _treeModel = new TreeModel();

            treeViewAdv1.ItemDrag += new ItemDragEventHandler(this.packTreeView_ItemDrag);
            treeViewAdv1.NodeMouseClick += (sender, args) => treeViewAdv1.SelectedNode = args.Node;
            treeViewAdv1.Expanded += TreeViewAdv1_Expanded;
            
            treeViewAdv1.Model = _treeModel;
            treeViewAdv1.NodeFilter = filter;

            nodeTextBox1.ToolTipProvider = new ToolTipProvider();
            nodeTextBox1.DrawText += new EventHandler<DrawTextEventArgs>(_nodeTextBox_DrawText);
        }

        void _nodeTextBox_DrawText(object sender, DrawTextEventArgs e)
        {
            var node = e.Node.Tag as PackEntryNode;
            if (node != null)
            {
                e.TextColor = node.Color;
            }
        }

        private void TreeViewAdv1_Expanded(object sender, TreeViewAdvEventArgs e)
        {
            var content = e.Node.Tag as DirEntryNode;
            if (content != null)
            {
                var children = e.Node.Children;
                foreach (var child in children)
                {
                    var file = child.Tag as PackedFileNode;
                    if (file != null)
                        child.Expand(true);
                }
            }
        }

        public void LabelEdit(bool state)
        {
        }


        public TreeViewAdv GetTreeView()
        {
            return treeViewAdv1;
        }

        public object GetNodeContent(TreeNodeAdv node)
        {
            var outerNode = node.Tag as Node;
            return outerNode.Tag;
        }

        public object GetSelectedNodeContent()
        {
            if (treeViewAdv1.SelectedNode == null)
                return null;

            var outerNode = treeViewAdv1.SelectedNode.Tag as Node;
            return outerNode.Tag;
        }

        public bool IsSelectedNodeRoot()
        {
            return treeViewAdv1.SelectedNode == treeViewAdv1.Root;
        }

        public bool IsSelectedNoodLeaf()
        {
            if (treeViewAdv1.SelectedNode == null)
                return false;
            return treeViewAdv1.SelectedNode.IsLeaf;
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
            var nodes = new DirEntryNode(currentPackFile.Root);
            _treeModel.Nodes.Clear();
            _treeModel.Nodes.Add(nodes);
            treeViewAdv1.EndUpdate();
        }


        public void ShowRenameSelectedNodeDialog()
        {
            if (!_parentRef.CanWriteCurrentPack)
            {
                MessageBox.Show("Unable to edit current pack as it is read only");
                return;
            }

            PackEntry entry = GetSelectedNodeContent() as PackEntry;
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
            if (e.Button == MouseButtons.Left && e.Item is TreeNode && e.Item != null &&
                ((TreeNode)e.Item).Tag is PackedFile && ((TreeNode)e.Item).Tag != null)
            {
                var file = ((TreeNode)e.Item).Tag as PackedFile;
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
