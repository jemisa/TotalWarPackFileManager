using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using System.Text.RegularExpressions;
using System.IO;

namespace PackFileManager.ExtentedTreeView
{
    public partial class ExtendedTreeView : UserControl
    {
        PackTreeViewFilterService _packTreeFilterService;

        public PackFileManagerForm _parentRef;

        public ExtendedTreeView()
        {
            InitializeComponent();

            _treeViewSearchBox.KeyUp += OnSearchKeyUp;
            _packTreeFilterService = new PackTreeViewFilterService(_treeView);
            _treeView.NodeMouseHover += new System.Windows.Forms.TreeNodeMouseHoverEventHandler(this.PackTreeView_NodeMouseHover);
            _treeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.packTreeView_AfterLabelEdit);
            _treeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.packTreeView_ItemDrag);
            _treeView.NodeMouseClick += (sender, args) => _treeView.SelectedNode = args.Node;
        }

        public TreeView GetTreeView()
        {
            return _treeView;
        }

        public List<TreeNode> GetAllContainedNodes()
        {
            List<TreeNode> nodes = new List<TreeNode>();
            GetAllContainedNodes(ref nodes, GetTreeView().Nodes);
            return nodes;
        }

        private void GetAllContainedNodes(ref List<TreeNode> nodes, TreeNodeCollection trunk)
        {
            foreach (TreeNode node in trunk)
            {
                nodes.Add(node);
                GetAllContainedNodes(ref nodes, node.Nodes);
            }
        }

        public void ExpandSelectedNode()
        {
            if (GetTreeView().SelectedNode == null)
                return;

            GetTreeView().BeginUpdate();
            if (!GetTreeView().SelectedNode.IsExpanded)
                GetTreeView().SelectedNode.ExpandAll();
            else
                GetTreeView().SelectedNode.Collapse(false);
            GetTreeView().EndUpdate();
        }

        public void Refresh(PackFile currentPackFile)
        {
            // save currently opened nodes
            var expandedNodes = new List<string>();
            foreach (TreeNode node in GetAllContainedNodes())
            {
                if (node.IsExpanded && node is PackEntryNode)
                {
                    expandedNodes.Add((node.Tag as PackEntry).FullPath);
                }
            }
            string selectedNode = (GetTreeView().SelectedNode != null)
                ? (GetTreeView().SelectedNode.Tag as PackEntry).FullPath : "";

            // rebuild tree
            GetTreeView().Nodes.Clear();
            if (currentPackFile == null)
            {
                return;
            }
            TreeNode node2 = new DirEntryNode(currentPackFile.Root);
            GetTreeView().BeginUpdate();
            GetTreeView().Nodes.Add(node2);
            GetTreeView().EndUpdate();

            // recover opened nodes and selection
            foreach (TreeNode node in GetAllContainedNodes())
            {
                string path = (node.Tag as PackEntry).FullPath;
                if (expandedNodes.Contains(path))
                {
                    node.Expand();
                }
            }
        }


        static readonly Regex versionedRegex = new Regex("(.*) - version.*");
        private void packTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (_parentRef.CanWriteCurrentPack)
            {
                PackEntry entry = e.Node.Tag as PackEntry;
                if ((e.Label != null) && (e.Label != e.Node.Text) && (entry != null))
                {
                    string newName = e.Label;
                    if (versionedRegex.IsMatch(newName))
                    {
                        newName = versionedRegex.Match(newName).Groups[1].Value;
                    }
                    entry.Name = newName;
                }
            }
            e.CancelEdit = true;
        }

        private void PackTreeView_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            packTreeViewToolTip.RemoveAll();
            var pos = GetTreeView().PointToClient(Cursor.Position);
            TreeNode selNode = (TreeNode)GetTreeView().GetNodeAt(pos);

            if (selNode != null)
            {
                var toolTip = selNode.ToolTipText;
                if (selNode.Tag != null)
                {
                    var directory = selNode.Tag as VirtualDirectory;
                    if (directory != null)
                    {
                        var fileCount = directory.AllFiles.Count;
                        toolTip += "File count = " + fileCount;
                    }
                }

                if (!string.IsNullOrWhiteSpace(selNode.ToolTipText))
                    toolTip += "\n" + selNode.ToolTipText;
                packTreeViewToolTip.SetToolTip(GetTreeView(), toolTip);
            }
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


        // Search
        private void OnSearchKeyUp(object sender, KeyEventArgs e)
        {
            var searchText = _treeViewSearchBox.Text;
            if (searchText.Length >= 3)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _packTreeFilterService.Search(searchText);
                }
                else
                {
                    _packTreeFilterService.DeplayedSearch(_treeView.Text);
                }
            }

            if (searchText.Length == 0)
            {
                _packTreeFilterService.ClearSearch();
            }

        }
    }
}
