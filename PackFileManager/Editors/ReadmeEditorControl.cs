namespace PackFileManager {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Common;
    using Filetypes;

    public class ReadmeEditorControl : UserControl, IPackedFileEditor {
        private ToolStripButton addEntryButton;
        private ComponentResourceManager manager;
        private BindingSource bindingSource;
        private ToolStripButton cloneEntryButton;
        private ToolStripButton closeButton;
        private IContainer components = new Container();
        private DataSet dataSet;
        private ToolStripButton deleteEntryButton;
        private TreeNode nodeInEdit;
        private Dictionary<TreeNode, int> nodeRowIndex;
        private Dictionary<TreeNode, DataRow> nodeRowLinks;
        private bool nodeSaved = true;
        private TreeView nodesTree;
        private Dictionary<TreeNode, DataTable> nodeTableLinks;
        private ToolStripButton revertNodeButton;
        private ToolStripButton saveButton;
        private bool saved = false;
        private SplitContainer splitContainer1;
        private Dictionary<string, uint> tagToChildMaxOccurs;
        private Dictionary<string, uint> tagToMaxOccurs = new Dictionary<string, uint>();
        private TextBox textEditorBox;
        private ToolStrip toolStrip1;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton updateNode;

        public ReadmeEditorControl()
        {
            this.tagToMaxOccurs.Add("member", uint.MaxValue);
            this.tagToMaxOccurs.Add("packDependency", uint.MaxValue);
            this.tagToMaxOccurs.Add("entry", uint.MaxValue);
            this.tagToChildMaxOccurs = new Dictionary<string, uint>();
            this.tagToChildMaxOccurs.Add("members", uint.MaxValue);
            this.tagToChildMaxOccurs.Add("dependencies", uint.MaxValue);
            this.tagToChildMaxOccurs.Add("changelog", uint.MaxValue);
            this.InitializeComponent();
            this.dataSet.ReadXmlSchema(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "readme.xsd"));
            this.nodeTableLinks = new Dictionary<TreeNode, DataTable>();
            this.nodeRowLinks = new Dictionary<TreeNode, DataRow>();
            this.nodeRowIndex = new Dictionary<TreeNode, int>();
        }
        
        bool readOnly = true;
        public bool ReadOnly {
            get {
                return readOnly;
            }
            set {
                readOnly = value;
                textEditorBox.ReadOnly = !value;
            }
        }


        public bool CanEdit(PackedFile file) {
            return file.Name.Equals("readme.xml");
        }

        PackedFile packedFile;
        public PackedFile CurrentPackedFile {
            get { return packedFile; }
            set {
                packedFile = value;
                this.dataSet.Clear();
                if (packedFile.Size == 0) {
                    this.dataSet.ReadXml(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "readme.xml"));
                } else {
                    using (StreamReader reader = new StreamReader(new MemoryStream(packedFile.Data, false), Encoding.ASCII)) {
                        this.dataSet.ReadXml(reader);
                    }
                    this.saved = true;
                    this.saveButton.Enabled = !this.saved;
                }
                this.buildNodeTreeRecursive(this.nodesTree.Nodes, this.dataSet.Tables[0]);
            }
        }
        public void Commit() {
            this.packedFile.Data = this.GetBytes();
        }

        private void addEntryButton_Click(object sender, EventArgs e)
        {
            DataTable childTable = this.nodeTableLinks[this.nodesTree.SelectedNode].ChildRelations[0].ChildTable;
            DataRow row = childTable.NewRow();
            object[] itemArray = row.ItemArray;
            for (int i = 0; i < itemArray.Length; i++)
            {
                if (childTable.Columns[i].DataType == System.Type.GetType("System.String"))
                {
                    itemArray[i] = childTable.Columns[i].Caption;
                }
                else if (childTable.Columns[i].DataType == System.Type.GetType("System.Int32"))
                {
                    itemArray[i] = 0;
                }
            }
            row.ItemArray = itemArray;
            childTable.Rows.Add(row);
            TreeNode key = this.nodesTree.SelectedNode.Nodes.Add(childTable.TableName);
            this.nodeTreeAddRow(key.Nodes, row);
            this.nodeTableLinks.Add(key, childTable);
            this.nodeRowLinks.Add(key, row);
            this.nodeRowIndex.Add(key, childTable.Rows.Count - 1);
        }

        private void buildNodeTreeRecursive(TreeNodeCollection treenodes, DataTable table)
        {
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    TreeNode key = treenodes.Add(table.TableName);
                    this.nodeTableLinks.Add(key, table);
                    this.nodeTreeAddRow(key.Nodes, row);
                }
            }
            else
            {
                treenodes.Add(table.TableName);
                this.nodeTableLinks.Add(treenodes[treenodes.Count - 1], table);
            }
            if (table.ChildRelations.Count > 0)
            {
                foreach (DataRelation relation in table.ChildRelations)
                {
                    this.buildNodeTreeRecursive(treenodes[treenodes.Count - 1].Nodes, relation.ChildTable);
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (!this.saved) {
                switch (MessageBox.Show("The ReadMe has been modified. Do you want to save your changes?", "Save changes?", 
                                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
                {
                    case DialogResult.Cancel:
                        return;

                    case DialogResult.Yes:
                        this.Commit();
                        break;
                }
            }
            this.nodeInEdit = null;
            this.nodeRowIndex = null;
            this.nodeTableLinks = null;
            this.nodeRowLinks = null;
            base.Dispose();
            this.DestroyHandle();
        }

        private void cloneEntryButton_Click(object sender, EventArgs e)
        {
            DataTable table = this.nodeTableLinks[this.nodesTree.SelectedNode];
            DataRow row = table.NewRow();
            object[] itemArray = row.ItemArray;
            for (int i = 0; i < itemArray.Length; i++)
            {
                if (this.nodeRowLinks[this.nodesTree.SelectedNode.Nodes[0]].ItemArray[i] is string)
                {
                    itemArray[i] = this.nodeRowLinks[this.nodesTree.SelectedNode.Nodes[0]].ItemArray[i].ToString();
                }
            }
            row.ItemArray = itemArray;
            table.Rows.Add(row);
            TreeNode key = this.nodesTree.SelectedNode.Parent.Nodes.Add(table.TableName);
            this.nodeTreeAddRow(key.Nodes, row);
            this.nodeTableLinks.Add(key, table);
            this.nodeRowLinks.Add(key, row);
            this.nodeRowIndex.Add(key, table.Rows.Count - 1);
        }

        private void deleteEntryButton_Click(object sender, EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        public byte[] GetBytes()
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    this.dataSet.WriteXml(writer);
                    buffer = stream.ToArray();
                }
            }
            return buffer;
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.manager = new ComponentResourceManager(typeof(ReadmeEditorControl));
            this.dataSet = new DataSet();
            this.bindingSource = new BindingSource(this.components);
            this.nodesTree = new TreeView();
            this.splitContainer1 = new SplitContainer();
            this.textEditorBox = new TextBox();
            this.toolStrip1 = new ToolStrip();
            this.saveButton = new ToolStripButton();
            this.closeButton = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.updateNode = new ToolStripButton();
            this.revertNodeButton = new ToolStripButton();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.addEntryButton = new ToolStripButton();
            this.cloneEntryButton = new ToolStripButton();
            this.deleteEntryButton = new ToolStripButton();
            this.dataSet.BeginInit();
            ((ISupportInitialize) this.bindingSource).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            base.SuspendLayout();
            this.dataSet.DataSetName = "readmeData";
            this.nodesTree.Dock = DockStyle.Fill;
            this.nodesTree.Location = new Point(0, 0);
            this.nodesTree.Name = "nodesTree";
            this.nodesTree.Size = new Size(0x308, 0x141);
            this.nodesTree.TabIndex = 1;
            this.nodesTree.AfterSelect += new TreeViewEventHandler(this.nodesTree_AfterSelect);
            this.splitContainer1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.splitContainer1.Location = new Point(10, 30);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = Orientation.Horizontal;
            this.splitContainer1.Panel1.Controls.Add(this.nodesTree);
            this.splitContainer1.Panel2.Controls.Add(this.textEditorBox);
            this.splitContainer1.Size = new Size(780, 560);
            this.splitContainer1.SplitterDistance = 0x145;
            this.splitContainer1.TabIndex = 3;
            this.textEditorBox.AcceptsReturn = true;
            this.textEditorBox.AcceptsTab = true;
            this.textEditorBox.AllowDrop = true;
            this.textEditorBox.Dock = DockStyle.Fill;
            this.textEditorBox.Location = new Point(0, 0);
            this.textEditorBox.Multiline = true;
            this.textEditorBox.Name = "textEditorBox";
            this.textEditorBox.ScrollBars = ScrollBars.Both;
            this.textEditorBox.Size = new Size(0x308, 0xe3);
            this.textEditorBox.TabIndex = 0;
            this.textEditorBox.KeyDown += new KeyEventHandler(this.textEditorBox_KeyDown);
            this.textEditorBox.KeyPress += new KeyPressEventHandler(this.textEditorBox_KeyPress);
            this.toolStrip1.Items.AddRange(new ToolStripItem[] { 
                this.saveButton, this.closeButton, this.toolStripSeparator1, this.updateNode, this.revertNodeButton, 
                this.toolStripSeparator2, this.addEntryButton, this.cloneEntryButton, this.deleteEntryButton });
            this.toolStrip1.Location = new Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new Size(800, 0x19);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            this.saveButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.saveButton.Enabled = false;
            this.saveButton.ImageTransparentColor = Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new Size(0x51, 0x16);
            this.saveButton.Text = "Save Changes";
            this.saveButton.Click += new EventHandler(this.saveButton_Click);
            this.closeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.closeButton.Image = (Image) manager.GetObject("closeButton.Image");
            this.closeButton.ImageTransparentColor = Color.Magenta;
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new Size(0x25, 0x16);
            this.closeButton.Text = "Close";
            this.closeButton.Click += new EventHandler(this.cancelButton_Click);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(6, 0x19);
            this.updateNode.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.updateNode.Enabled = false;
            this.updateNode.Image = (Image) manager.GetObject("updateNode.Image");
            this.updateNode.ImageTransparentColor = Color.Magenta;
            this.updateNode.Name = "updateNode";
            this.updateNode.Size = new Size(0x6c, 0x16);
            this.updateNode.Text = "Update Edited Node";
            this.updateNode.Click += new EventHandler(this.updateNode_Click);
            this.revertNodeButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.revertNodeButton.Enabled = false;
            this.revertNodeButton.Image = (Image) manager.GetObject("revertNodeButton.Image");
            this.revertNodeButton.ImageTransparentColor = Color.Magenta;
            this.revertNodeButton.Name = "revertNodeButton";
            this.revertNodeButton.Size = new Size(0x69, 0x16);
            this.revertNodeButton.Text = "Revert Edited Node";
            this.revertNodeButton.Click += new EventHandler(this.revertNodeButton_Click);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(6, 0x19);
            this.addEntryButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.addEntryButton.Enabled = false;
            this.addEntryButton.Image = (Image) manager.GetObject("addEntryButton.Image");
            this.addEntryButton.ImageTransparentColor = Color.Magenta;
            this.addEntryButton.Name = "addEntryButton";
            this.addEntryButton.Size = new Size(0x56, 0x16);
            this.addEntryButton.Text = "Add Node Entry";
            this.addEntryButton.Click += new EventHandler(this.addEntryButton_Click);
            this.cloneEntryButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.cloneEntryButton.Enabled = false;
            this.cloneEntryButton.Image = (Image) manager.GetObject("cloneEntryButton.Image");
            this.cloneEntryButton.ImageTransparentColor = Color.Magenta;
            this.cloneEntryButton.Name = "cloneEntryButton";
            this.cloneEntryButton.Size = new Size(0x5e, 0x16);
            this.cloneEntryButton.Text = "Clone Node Entry";
            this.cloneEntryButton.Click += new EventHandler(this.cloneEntryButton_Click);
            this.deleteEntryButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.deleteEntryButton.Enabled = false;
            this.deleteEntryButton.Image = (Image) manager.GetObject("deleteEntryButton.Image");
            this.deleteEntryButton.ImageTransparentColor = Color.Magenta;
            this.deleteEntryButton.Name = "deleteEntryButton";
            this.deleteEntryButton.Size = new Size(0x62, 0x16);
            this.deleteEntryButton.Text = "Delete Node Entry";
            this.deleteEntryButton.Click += new EventHandler(this.deleteEntryButton_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.Controls.Add(this.toolStrip1);
            base.Controls.Add(this.splitContainer1);
            base.Name = "ReadmeEditorControl";
            base.Size = new Size(800, 600);
            this.dataSet.EndInit();
            ((ISupportInitialize) this.bindingSource).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void nodesTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((this.nodeInEdit != null) && !this.nodeSaved)
            {
                switch (MessageBox.Show("The node text has changed. Do you want to save these changes?", "Save Changes?", 
                                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
                {
                    case DialogResult.Yes:
                        this.nodeUpdate();
                        break;

                    case DialogResult.No:
                        this.nodeInEdit = null;
                        this.nodeSaved = true;
                        break;
                }
            }
            if (this.nodeRowLinks.ContainsKey(e.Node))
            {
                this.textEditorBox.Text = this.nodeRowLinks[e.Node].ItemArray[this.nodeRowIndex[e.Node]].ToString();
                this.nodeInEdit = e.Node;
            }
            if (this.tagToChildMaxOccurs.ContainsKey(e.Node.Text.Split(new char[] { ':' })[0]) && 
               (this.tagToChildMaxOccurs[e.Node.Text.Split(new char[] { ':' })[0]] > Convert.ToUInt32(e.Node.Nodes.Count)))
            {
                this.addEntryButton.Enabled = true;
                this.cloneEntryButton.Enabled = false;
                this.deleteEntryButton.Enabled = false;
            }
            else if (this.tagToMaxOccurs.ContainsKey(e.Node.Text.Split(new char[] { ':' })[0]) && 
                    (this.tagToMaxOccurs[e.Node.Text.Split(new char[] { ':' })[0]] > Convert.ToUInt32(e.Node.Parent.Nodes.Count)))
            {
                this.addEntryButton.Enabled = false;
                this.cloneEntryButton.Enabled = true;
                this.deleteEntryButton.Enabled = true;
            }
            else
            {
                this.addEntryButton.Enabled = false;
                this.cloneEntryButton.Enabled = false;
                this.deleteEntryButton.Enabled = false;
            }
        }

        private void nodeTreeAddRow(TreeNodeCollection treenodes, DataRow row)
        {
            DataTable table = row.Table;
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                if (!table.Columns[i].Caption.EndsWith("_Id"))
                {
                    treenodes.Add(table.Columns[i].Caption + ": " + row.ItemArray[i].ToString());
                    this.nodeRowLinks.Add(treenodes[treenodes.Count - 1], row);
                    this.nodeTableLinks.Add(treenodes[treenodes.Count - 1], table);
                    this.nodeRowIndex.Add(treenodes[treenodes.Count - 1], i);
                }
            }
        }

        private void nodeUpdate()
        {
            object[] itemArray = this.nodeRowLinks[this.nodeInEdit].ItemArray;
            itemArray[this.nodeRowIndex[this.nodeInEdit]] = this.textEditorBox.Text;
            this.nodeRowLinks[this.nodeInEdit].ItemArray = itemArray;
            this.nodeInEdit.Text = this.nodeTableLinks[this.nodeInEdit].Columns[this.nodeRowIndex[this.nodeInEdit]].Caption + 
                ": " + this.textEditorBox.Text;
            this.saved = false;
            this.saveButton.Enabled = !this.saved;
            this.nodeInEdit = null;
            this.nodeSaved = true;
            this.updateNode.Enabled = !this.nodeSaved;
            this.revertNodeButton.Enabled = !this.nodeSaved;
        }

        private void revertNodeButton_Click(object sender, EventArgs e)
        {
            this.textEditorBox.Text = this.nodeRowLinks[this.nodeInEdit].ItemArray[this.nodeRowIndex[this.nodeInEdit]].ToString();
            this.nodeSaved = true;
            this.updateNode.Enabled = !this.nodeSaved;
            this.revertNodeButton.Enabled = !this.nodeSaved;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            this.Commit();
            this.saved = true;
            this.saveButton.Enabled = !this.saved;
        }

        private void textEditorBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control || e.Shift) && (e.KeyCode == Keys.Return))
            {
                this.nodeUpdate();
                e.SuppressKeyPress = true;
            }
        }

        private void textEditorBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.nodeSaved = false;
            this.updateNode.Enabled = !this.nodeSaved;
            this.revertNodeButton.Enabled = !this.nodeSaved;
        }

        private void updateNode_Click(object sender, EventArgs e)
        {
            this.nodeUpdate();
        }

        private void updateRowIndices()
        {
            foreach (TreeNode node in this.nodeRowIndex.Keys)
            {
                this.nodeRowIndex[node] = this.nodeTableLinks[node].Rows.IndexOf(this.nodeRowLinks[node]);
            }
        }
    }
}

