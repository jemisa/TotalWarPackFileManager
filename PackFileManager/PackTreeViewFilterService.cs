using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PackFileManager
{
    class PackTreeViewFilterService
    {
        TreeNode _copyOfOriginal = null;
        int _numOriginalNodes = -1;
        TreeView _packTreeView;
        Timer _deplayedSearchTimer;
        string _deplayedStrStr;
        string _lastSearchStr;

        public PackTreeViewFilterService(TreeView packTreeView)
        {
            _packTreeView = packTreeView;
            _deplayedSearchTimer = new Timer();
            _deplayedSearchTimer.Interval = 1000;
            _deplayedSearchTimer.Tick += SeachTimer_Tick;
        }

        public void DeplayedSearch(string searchStr)
        {
            _deplayedStrStr = searchStr;
            _deplayedSearchTimer.Stop();
            _deplayedSearchTimer.Start();
        }

        void CancelPendingSearches()
        {
            _deplayedSearchTimer.Stop();
        }

        void SeachTimer_Tick(object sender, EventArgs e)
        {
            CancelPendingSearches();
            Search(_deplayedStrStr);
        }

        public void ClearSearch()
        {
            CancelPendingSearches();

            if (_copyOfOriginal != null)
            {
                _packTreeView.BeginUpdate();
                _packTreeView.Nodes.Clear();
                _packTreeView.Nodes.Add(_copyOfOriginal);
                _packTreeView.EndUpdate();
                _lastSearchStr = "";
            }
        }

        public void Search(string searchStr)
        {
            if (_packTreeView.Nodes.Count == 0)
                return;
            if (_lastSearchStr == searchStr)
                return;

            var currentNodeCount = _packTreeView.Nodes[0].GetNodeCount(true);
            if (_copyOfOriginal == null || currentNodeCount != _numOriginalNodes)
            {
                _copyOfOriginal = CopyTreeNodes(_packTreeView.Nodes[0]);
                _numOriginalNodes = currentNodeCount;
            }

            var startNode = CopyTreeNodes(_copyOfOriginal); ;

            _packTreeView.BeginUpdate();
            RemoveUnwantedNodes(startNode, searchStr);
            RemoveEmptyNodes(startNode);
            _packTreeView.Nodes.Clear();
            _packTreeView.Nodes.Add(startNode);
            _packTreeView.ExpandAll();
            _packTreeView.EndUpdate();

            _lastSearchStr = searchStr;
        }

        bool RemoveEmptyNodes(TreeNode parent)
        {
            if (parent == null)
                return false;

            for (int i = 0; i < parent.Nodes.Count; i++)
            {
                var res = RemoveEmptyNodes(parent.Nodes[i]);
                if (res)
                    i--;
            }

            if (parent.Tag as VirtualDirectory != null)
            {
                if (parent.Nodes.Count == 0)
                {
                    parent.Remove();
                    return true;
                }
            }

            return false;
        }

        void RemoveUnwantedNodes(TreeNode parent, string searchStr)
        {
            for (int i = 0; i < parent.Nodes.Count; i++)
            {
                var node = parent.Nodes[i];
                if (node != null)
                {
                    if (node.Nodes.Count == 0)
                    {
                        if (!node.Text.Contains(searchStr))
                        {
                            node.Remove();
                            i--;
                        }
                    }
                    else
                    {
                        RemoveUnwantedNodes(node, searchStr);
                    }
                }
            }
        }

        TreeNode CopyTreeNodes(TreeNode node)
        {
            TreeNode copyNode = new TreeNode(node.Text, node.ImageIndex, node.SelectedImageIndex);
            copyNode.Tag = node.Tag;
            CopyChildren(copyNode, node);
            return copyNode;
        }

        void CopyChildren(TreeNode parent, TreeNode original)
        {
            TreeNode copyNode;
            foreach (TreeNode tn in original.Nodes)
            {
                copyNode = new TreeNode(tn.Text, tn.ImageIndex, tn.SelectedImageIndex);
                copyNode.Tag = tn.Tag;
                parent.Nodes.Add(copyNode);
                CopyChildren(copyNode, tn);
            }
        }
    }
}
