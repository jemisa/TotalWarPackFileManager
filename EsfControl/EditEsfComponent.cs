using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using CommonDialogs;
using Filetypes;

namespace EsfControl {
    public partial class EditEsfComponent : UserControl {
        public delegate void Selected(EsfNode node);
        public event Selected NodeSelected;

        TreeEventHandler treeEventHandler;

        EsfTreeNode rootNode;
        public EsfNode RootNode {
            get {
                return rootNode != null ? rootNode.Tag as EsfNode : null;
            }
            set {
                esfNodeTree.Nodes.Clear();
                if (value != null) {
                    rootNode = new EsfTreeNode(value as ParentNode);
                    rootNode.ShowCode = ShowCode;
                    esfNodeTree.Nodes.Add(rootNode);
                    rootNode.Fill();
                    nodeValueGridView.Rows.Clear();
                    value.Modified = false;
                }
            }
        }

        bool showCode;
        public bool ShowCode {
            get { return showCode; }
            set {
                showCode = value;
                if (esfNodeTree.Nodes.Count > 0) {
                    (esfNodeTree.Nodes[0] as EsfTreeNode).ShowCode = value;
                    nodeValueGridView.Columns["Code"].Visible = value;
                }
            }
        }

        public EditEsfComponent() {
            InitializeComponent();
            nodeValueGridView.Rows.Clear();

            treeEventHandler = new TreeEventHandler(nodeValueGridView, this);
            esfNodeTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(treeEventHandler.FillNode);
            esfNodeTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeEventHandler.TreeNodeSelected);

            nodeValueGridView.CellValidating += new DataGridViewCellValidatingEventHandler(validateCell);
            nodeValueGridView.CellEndEdit += new DataGridViewCellEventHandler(cellEdited);

            MouseHandler mouseHandler = new MouseHandler();
            esfNodeTree.MouseUp += new MouseEventHandler(mouseHandler.ShowContextMenu);
            
            nodeValueGridView.CellClick += CellClicked;
        }
        
        private void CellClicked(object sender, DataGridViewCellEventArgs args) {
            if (args.ColumnIndex == 1) {
                Console.WriteLine("editing {0}", nodeValueGridView.Rows[args.RowIndex].Cells[0].Value);
            }
        }

        private void validateCell(object sender, DataGridViewCellValidatingEventArgs args) {
            EsfNode valueNode = nodeValueGridView.Rows[args.RowIndex].Tag as EsfNode;
            if (valueNode != null) {
                string newValue = args.FormattedValue.ToString();
                try {
                    if (args.ColumnIndex == 0 && newValue != valueNode.ToString()) {
                        valueNode.FromString(newValue);
                    }
                } catch {
                    Console.WriteLine("Invalid value {0}", newValue);
                    args.Cancel = true;
                }
            } else {
                nodeValueGridView.Rows[args.RowIndex].ErrorText = "Cannot edit this value";
                // args.Cancel = true;
            }
        }
        private void cellEdited(object sender, DataGridViewCellEventArgs args) {
            nodeValueGridView.Rows[args.RowIndex].ErrorText = String.Empty;
        }
        
        public void NotifySelection(EsfNode node) {
            if (NodeSelected != null) {
                NodeSelected(node);
            }
        }
        
        public string SelectedPath {
            get {
                EsfNode node = esfNodeTree.SelectedNode.Tag as EsfNode;
                String result = NodePathCreator.CreatePath(node);
                return result;
            }
            set {
                string[] nodes = value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                TreeNode currentNode = rootNode;
                rootNode.Expand();
                for (int i = 1; i < nodes.Length; i++) {
                    currentNode = FindNode(currentNode.Nodes, nodes[i]);
                    if (currentNode != null) {
                        currentNode.Expand();
                    } else {
                        Console.WriteLine("Cannot find {0} in {1}", nodes[i], nodes[i-1]);
                        break;
                    }
                };
                if (currentNode != null) {
                    esfNodeTree.SelectedNode = currentNode;
                }
            }
        }
        private TreeNode FindNode(TreeNodeCollection collection, string pathSegment) {
            foreach(TreeNode node in collection) {
                if (node.Text.Equals(pathSegment)) {
                    return node;
                }
            }
            return null;
        }
    }

    public class MouseHandler {
        public delegate void NodeAction(EsfNode node);

        public void ShowContextMenu(object sender, System.Windows.Forms.MouseEventArgs e) {
            TreeView treeView = sender as TreeView;
            if (e.Button == MouseButtons.Right && treeView != null) {
                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);
                ContextMenuStrip contextMenu = new ContextMenuStrip();

                // Get the node that the user has clicked.
                TreeNode node = treeView.GetNodeAt(p);
                ParentNode selectedNode = (node != null) ? node.Tag as ParentNode : null;
                if (selectedNode != null && (node.Tag as EsfNode).Parent is RecordArrayNode) {
                    treeView.SelectedNode = node;

                    ToolStripItem toolItem = CreateMenuItem("Duplicate", selectedNode, CopyNode);
                    contextMenu.Items.Add(toolItem);
                    toolItem = CreateMenuItem("Delete", selectedNode, DeleteNode);
                    contextMenu.Items.Add(toolItem);
                    toolItem = CreateMenuItem("Move", selectedNode, MoveNode);
                    contextMenu.Items.Add(toolItem);
                }
                
                if (contextMenu.Items.Count != 0) {
                    contextMenu.Show(treeView, p);
                }
            }
        }
        
        private ToolStripMenuItem CreateMenuItem(String label, EsfNode node, NodeAction action) {
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Click += new EventHandler(delegate(object s, EventArgs args) { action(node); });
            return item;
        }
        
        private void CopyNode(EsfNode node) {
            ParentNode toCopy = node as ParentNode;
            ParentNode copy = toCopy.CreateCopy() as ParentNode;
            if (copy != null) {
                ParentNode parent = toCopy.Parent as ParentNode;
                if (parent != null) {
                    List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                    SetAllModified(copy);
                    int insertAt = parent.Children.IndexOf(toCopy) + 1;
                    nodes.Insert(insertAt, copy);
#if DEBUG
                    Console.Out.WriteLine("new list now {0}", string.Join(",", nodes));
#endif
                    // copy.Modified = true;
                    // copy.AllNodes.ForEach(n => n.Modified = true);
                    parent.Value = nodes;
#if DEBUG
                } else {
                    Console.WriteLine("no parent to add to");
#endif
                }
#if DEBUG
            } else {
                Console.WriteLine("couldn't create copy");
#endif
            }
        }
        
        private void SetAllModified(ParentNode node) {
            node.Modified = true;
            node.Children.ForEach(n => SetAllModified(n));
        }

        private void DeleteNode(EsfNode node) {
            RecordArrayNode parent = node.Parent as RecordArrayNode;
            if (parent != null) {
                List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                nodes.Remove(node);
                parent.Value = nodes;
            }
        }
        
        private void MoveNode(EsfNode node) {
            RecordArrayNode parent = node.Parent as RecordArrayNode;
            if (parent != null) {
                InputBox input = new InputBox{
                    Input = "Move to index"
                };
                if (input.ShowDialog() == DialogResult.OK) {
                    int moveToIndex = -1;
                    List<EsfNode> nodes = new List<EsfNode>(parent.Value);
                    if (int.TryParse(input.Input, out moveToIndex)) {
                        if (moveToIndex >= 0 && moveToIndex < nodes.Count) {
                            nodes.Remove(node);
                            nodes.Insert(moveToIndex, node);
#if DEBUG
                            Console.Out.WriteLine("new list now {0}", string.Join(",", nodes));
#endif
                            parent.Value = nodes;
                        } else {
                            MessageBox.Show(string.Format("Entry only valid between 0 and {0}", nodes.Count - 1),
                                       "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } else {
                        MessageBox.Show(string.Format("Enter index (between 0 and {0})", nodes.Count - 1),
                                        "Invalid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
    
    public class EsfTreeNode : TreeNode {
        private bool showCode;
        public bool ShowCode {
            get { return showCode; }
            set {
                ParentNode node = Tag as ParentNode;
                if (node != null) {
                    string baseName = (node as INamedNode).GetName();
                    Text = value ? string.Format("{0} - {1}", baseName, node.TypeCode) : baseName;
                    showCode = value;
                    foreach (TreeNode child in Nodes) {
                        (child as EsfTreeNode).ShowCode = value;
                    }
                }
            }
        }
        public EsfTreeNode(ParentNode node, bool showC = false) {
            Tag = node;
            Text = (node as INamedNode).GetName();
            node.ModifiedEvent += NodeChange;
            ForeColor = node.Modified ? Color.Red : Color.Black;
            ShowCode = showC;
            
            node.RenameEvent += delegate(EsfNode n) {
                Text = node.Name;
            };
        }
        public void Fill() {
            if (Nodes.Count == 0) {
#if DEBUG
                Console.WriteLine("filling list for {0}", (Tag as ParentNode).Name);
#endif
                ParentNode parentNode = (Tag as ParentNode);
                foreach (ParentNode child in parentNode.Children) {
                    EsfTreeNode childNode = new EsfTreeNode(child, ShowCode);
                    Nodes.Add(childNode);
                }
            }
        }
        public void NodeChange(EsfNode n) {
            ForeColor = n.Modified ? Color.Red : Color.Black;
            if (!n.Modified) {
                return;
            }
            ParentNode node = (Tag as ParentNode);
            bool sameChildren = node.Children.Count == this.Nodes.Count;
            for (int i = 0; sameChildren && i < node.Children.Count; i++) {
                sameChildren &= node.Children[i].Name.Equals(Nodes[i].Text);
            }
            if (node != null) {
                if (!sameChildren) {
                    Nodes.Clear();
                    Fill();
                    if (IsExpanded) {
                        foreach (TreeNode child in Nodes) {
                            (child as EsfTreeNode).Fill();
                        }
                    }
                } else {
                    for(int i = 0; i < node.Children.Count; i++) {
                        Nodes[i].Text = node.Children[i].Name;
                    }
                }
            }
        }
    }

    public class TreeEventHandler {
        private List<ModificationColorizer> registeredEvents = new List<ModificationColorizer>();
        private EditEsfComponent component;
        
        DataGridView nodeValueGridView;
        public TreeEventHandler(DataGridView view, EditEsfComponent c) {
            nodeValueGridView = view;
            component = c;
        }
        /*
         * Fill the event's target tree node's children with their children
         * (to show the [+] if they contain child nodes).
         */
        public void FillNode(object sender, TreeViewCancelEventArgs args) {
            foreach (TreeNode child in args.Node.Nodes) {
                EsfTreeNode esfNode = child as EsfTreeNode;
                if (esfNode != null) {
                    esfNode.Fill();
                }
            }
        }

        /*
         * Render the data cell view, preparing the red color for modified entries.
         */
        public void TreeNodeSelected(object sender, TreeViewEventArgs args) {
            ParentNode node = args.Node.Tag as ParentNode;
            try {
                nodeValueGridView.Rows.Clear();
                registeredEvents.ForEach(handler => { (handler.row.Tag as EsfNode).ModifiedEvent -= handler.ChangeColor; });
                registeredEvents.Clear();
                foreach (EsfNode value in node.Values) {
                    int index = nodeValueGridView.Rows.Add(value.ToString(), value.SystemType.ToString(), value.TypeCode.ToString());
                    DataGridViewRow newRow = nodeValueGridView.Rows [index];
                    ModificationColorizer colorizer = new ModificationColorizer(newRow);
                    registeredEvents.Add(colorizer);
                    foreach (DataGridViewCell cell in newRow.Cells) {
                        cell.Style.ForeColor = value.Modified ? Color.Red : Color.Black;
                    }
                    value.ModifiedEvent += colorizer.ChangeColor;
                    
                    newRow.Tag = value;
                }
                component.NotifySelection(node);
            } catch {
            }
        }
    }    
    
    public class ModificationColorizer {
        public DataGridViewRow row;
        public ModificationColorizer(DataGridViewRow r) {
            row = r;
        }
        public void ChangeColor(EsfNode node) {
            foreach (DataGridViewCell cell in row.Cells) {
                cell.Style.ForeColor = node.Modified ? Color.Red : Color.Black;
            }
        }
    }

    public class BookmarkItem : ToolStripMenuItem {
        string openPath;
        EditEsfComponent component;
        public BookmarkItem(string label, string path, EditEsfComponent c) : base(label) {
            openPath = path;
            component = c;
            Click += OpenPath;
        }
        private void OpenPath(object sender, EventArgs args) {
            component.SelectedPath = openPath;
        }
    }
}
