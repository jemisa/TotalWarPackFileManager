using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Filetypes;
using Filetypes.Codecs;

namespace PackFileManager {
    //public partial class DBFileEditorTree : UserControl {
    public partial class DBFileEditorTree : PackedFileEditor<DBFile> {

        public DBFileEditorTree() : base(new DelegatingDbFileCodec()) {
            InitializeComponent();
   
            // fill data grid view and enable/disable menu items
            treeView.AfterSelect += new TreeViewEventHandler(NodeSelected);
            // allow copy/paste by keyboard
            treeView.KeyDown += new KeyEventHandler(HandleTreeKeys);
            // upon tree view label edit, set the corresponding item's field
            treeView.AfterLabelEdit += new NodeLabelEditEventHandler(LabelEdited);
   
            // set DataChanged on field value edit
            dataGridView.CellEndEdit += new DataGridViewCellEventHandler(CellEdited);
            // allow copy/paste by keyboard
            dataGridView.KeyUp += new KeyEventHandler(HandleDataGridKeys);
            // don't allow editing the field name column
            dataGridView.CellBeginEdit += new DataGridViewCellCancelEventHandler(PreventNameEditing);
        }
  
        /*
         * Set the name field of the edited label's FieldInstance.
         */
        void LabelEdited(object o, NodeLabelEditEventArgs args) {
            FieldInstanceNode instanceNode = args.Node as FieldInstanceNode;
            if (instanceNode != null) {
                instanceNode.Fields[instanceNode.NameIndex].Value = args.Label;
                dataGridView.Refresh();
                DataChanged = true;
            }
        }

        /*
         * Upon tree node selection, fill the grid view with fields of selected element.
         * Also, enable edit menu items depending on selection.
         */
        void NodeSelected(object o, TreeViewEventArgs args) {
            dataGridView.CancelEdit();
            FieldInstanceNode node = treeView.SelectedNode as FieldInstanceNode;
            if (node != null) {
                List<FieldInstance> dataFields = node.SimpleFields;
                dataGridView.DataSource = new BindingList<FieldInstance>(dataFields);
            } else {
                dataGridView.DataSource = new List<FieldInstance>();
            }

            // enable menu items
            cloneToolStripMenuItem.Enabled = CanEditElements;
            copyToolStripMenuItem.Enabled = CanEditElements;
            deleteToolStripMenuItem.Enabled = CanEditElements;
            pasteToolStripMenuItem.Enabled = CopiedNode != null && CanEditElements;
            treeView.LabelEdit = args.Node is FieldInstanceNode;
        }

        #region PackedFileEditor Overrides
        public override bool CanEdit(Common.PackedFile file) {
            bool result = false;
            try {
                //(codec as DelegatingDbFileCodec).Codec = PackedFileDbCodec.GetCodec(file);
                // result = DBFile.typename(file.FullPath).Equals("models_building_tables");
                //result = codec != null;
            } catch { }
            return result;
        }

        public override DBFile EditedFile {
            get {
                return base.EditedFile;
            }
            set {
                base.EditedFile = value;
                if (value != null) {
                    dataGridView.DataSource = new List<FieldInstance>();
                    treeView.Nodes.Clear();
                    foreach (List<FieldInstance> fields in EditedFile.Entries) {
                        if (fields.Count > 0) {
                            treeView.Nodes.Add(new FieldInstanceNode(fields, -1));
                        } else {
                            treeView.Nodes.Add(new TreeNode("This entry is empty."));
                        }
                    }
                }
            }
        }

        protected override void SetData() {
            if (DataChanged) {
                EditedFile.Entries.Clear();
                Fields.ForEach(f => EditedFile.Entries.Add(f));
            }
            base.SetData();
        }
        #endregion

        /*
         * Retrieve all data from the tags in the tree view (will include added items).
         */
        List<DBRow> Fields {
            get {
                List<DBRow> fields = new List<DBRow>();
                foreach (TreeNode node in treeView.Nodes) {
                    FieldInstanceNode fieldNode = node as FieldInstanceNode;
                    fields.Add(fieldNode.Fields);
                }
                return fields;
            }
        }

        #region Tree List Entry Editing
        /* Query if the currently selected tree node can be copied or deleted. */
        bool CanEditElements {
            get {
                if (treeView.SelectedNode == null) {
                    return false;
                }
                bool result;
                if (treeView.SelectedNode.Parent != null) {
                    result = treeView.SelectedNode.Parent.Tag is ListField;
                } else {
                    result = true;
                }
                return result;
            }
        }
  
        /*
         * The nodes to add new items to. May be null.
         */
        TreeNodeCollection EditedNodes {
            get {
                TreeNodeCollection edited = null;
                if (treeView.SelectedNode != null) {
                    if (treeView.SelectedNode.Parent == null) {
                        // no parent: add as root item
                        edited = treeView.Nodes;
                    } else if (EditedListNode != null) {
                        // add to list node if available
                        edited = EditedListNode.Nodes;
                    }
                }
                return edited;
            }
        }
        /*
         * The list to add a new row to. May be null.
         */
        List<List<FieldInstance>> EditedFieldList {
            get {
                List<List<FieldInstance>> parentList = null;
                ListTypeNode edited = EditedListNode;
                if (edited != null) {
                    parentList = edited.Field.Contained;
                }
                return parentList;
            }
        }
        /*
         * Retrieves the currently selected node if it is a list node;
         * otherwise its parent, if that is a list node.
         */
        ListTypeNode EditedListNode {
            get {
                ListTypeNode result = null;
                if (treeView.SelectedNode != null) {
                    result = treeView.SelectedNode as ListTypeNode;
                    result = result ?? treeView.SelectedNode.Parent as ListTypeNode;
                }
                return result;
            }
        }

        /*
         * Key Shortcut handler for copy/paste and delete.
         */
        void HandleTreeKeys(object o, KeyEventArgs args) {
            switch(args.KeyCode) {
            case Keys.C:
                if (args.Control) { CopyTreeItem(); }
                break;
            case Keys.V:
                if (args.Control) { PasteTreeItem(); }
                break;
            case Keys.Delete:
                DeleteEntry();
                break;
            }
        }
        #endregion

        #region Tree Entry Copy/Paste
        private FieldInstanceNode copy;
        FieldInstanceNode CopiedNode {
            get {
                if (copy == null) {
                    return null;
                }
                // we need to create yet another copy or we'll have several nodes
                // referencing the same objects
                List<FieldInstance> fields = new List<FieldInstance>();
                copy.Fields.ForEach(f => {
                    fields.Add(f.CreateCopy());
                });
                return new FieldInstanceNode(fields, copy.NameIndex);
            }
            set {
                if (value != null) {
                    // copy the data so we won't paste edited data along later
                    copy = CreateCopyNode();
                } else {
                    copy = null;
                }
                pasteToolStripMenuItem.Enabled = copy != null;
            }
        }

        /*
         * Copy/paste in one step. Will not set the copied element for later paste.
         */
        private void CloneItem(object sender, EventArgs e) {
            FieldInstanceNode newNode = CreateCopyNode();
            if (newNode != null) {
                InsertTreeItem(newNode);
            }
        }

        /*
         * Create a copy of the currently selected node.
         * Will copy the values from the contained field instances so we don't
         * get into referencing problems.
         */
        private FieldInstanceNode CreateCopyNode() {
            FieldInstanceNode newNode = null;
            List<FieldInstance> toClone = treeView.SelectedNode.Tag as List<FieldInstance>;
            if (toClone != null) {
                ListTypeNode listNode = treeView.SelectedNode.Parent as ListTypeNode;
                int nameIndex = listNode != null
                    ? (listNode.Field.Info as ListType).NameAt
                    : -1;
                List<FieldInstance> fields = new List<FieldInstance>();
                toClone.ForEach(f => {
                    fields.Add(f.CreateCopy());
                });
                newNode = new FieldInstanceNode(fields, nameIndex);
            }
            return newNode;
        }

        /*
         * Insert the given item into the currently selected node.
         * This can only be done for list nodes (new entry in the list)
         * or root nodes (new entry in the model).
         * TODO: check the inserted types when inserting into a list.
         */
        private void InsertTreeItem(FieldInstanceNode newNode) {
            TreeNodeCollection addTo = EditedNodes;
            if (newNode != null && addTo != null) {
                List<List<FieldInstance>> parentList = EditedFieldList;
                addTo.Add(newNode);
                if (parentList != null) {
                    parentList.Add(newNode.Fields);
                }
                newNode.EnsureVisible();
                treeView.SelectedNode = newNode;
                DataChanged = true;
            }
        }

        /*
         * Copy the currently selected tree item (for later paste).
         */
        private void CopyTreeItem(object unused1 = null, EventArgs unused2 = null) {
            CopiedNode = treeView.SelectedNode as FieldInstanceNode;
        }

        /*
         * Paste currently copied tree item.
         */
        private void PasteTreeItem(object sender = null, EventArgs e = null) {
            InsertTreeItem(CopiedNode);
        }

        #endregion

        /*
         * Remove the currently selected item from the tree.
         */
        void DeleteEntry(object unused1 = null, EventArgs unused2 = null) {
            TreeNodeCollection removeFrom = EditedNodes;
            FieldInstanceNode toRemove = treeView.SelectedNode as FieldInstanceNode;
            if (removeFrom != null && toRemove != null) {
                if (EditedFieldList != null) {
                    EditedFieldList.Remove(toRemove.Fields);
                }
                removeFrom.Remove(toRemove);
                DataChanged = true;
            }
        }

        #region Field Grid Data editing
        /*
         * Apply value edited in data grid view to appropriate FieldInstance.
         */
        void CellEdited(object o, DataGridViewCellEventArgs args) {
            FieldInstanceNode fieldNode = treeView.SelectedNode as FieldInstanceNode;
            if (fieldNode != null && args.ColumnIndex == 1) {
                DataChanged = true;
                
                // update node label
                if (args.RowIndex == fieldNode.NameIndex) {
                    fieldNode.Text = fieldNode.Fields[args.RowIndex].Value;
                }
            }
        }

        /*
         * Prevent the user from editing the field name description.
         */
        void PreventNameEditing(object o, DataGridViewCellCancelEventArgs args) {
            if (args.ColumnIndex == 0) {
                args.Cancel = true;
            }
        }
        #endregion
        
        #region Copy/Paste Grid View Data
        // copied items stored here.
        List<string> copiedDataValues = new List<string>();
        /*
         * Handle copy/paste keys.
         */
        void HandleDataGridKeys(object o = null, KeyEventArgs args = null) {
            switch(args.KeyCode) {
            case Keys.C:
                if (args.Control) { CopyDataValues(); }
                args.Handled = true;
                break;
            case Keys.V:
                if (args.Control) { PasteDataValues(); }
                args.Handled = true;
                break;
            }
        }

        /*
         * Retrieve the value cells (column 1) that are currently selected.
         */
        List<DataGridViewCell> SelectedValueCells {
            get {
                List<DataGridViewCell> cells = new List<DataGridViewCell>();
                foreach(DataGridViewCell cell in dataGridView.SelectedCells) {
                    if (cell.ColumnIndex == 1) {
                        cells.Add(cell);
                    }
                }
                return cells;
            }
        }

        /*
         * Copy the values of the currently selected data cells.
         */
        void CopyDataValues() {
            copiedDataValues.Clear();
            SelectedValueCells.ForEach(cell => copiedDataValues.Add(dataGridView[cell.ColumnIndex, cell.RowIndex].Value.ToString()));
#if DEBUG
            Console.WriteLine("{0} values copied: {1}", copiedDataValues.Count, string.Join(",", copiedDataValues));
#endif
        }

        /*
         * Paste previously copied values into currently selected cells.
         */
        void PasteDataValues() {
#if DEBUG
            Console.WriteLine("pasting {0} values into {1} cells", copiedDataValues.Count, SelectedValueCells.Count);
#endif
            int count = Math.Min(copiedDataValues.Count, SelectedValueCells.Count);
            List<FieldInstance> setTo = (treeView.SelectedNode as FieldInstanceNode).SimpleFields;
            string pasted = "";
            try {
                for (int i = 0; i < count; i++) {
                    int setIndex = SelectedValueCells[i].RowIndex;
                    pasted = copiedDataValues[i];
                    setTo[setIndex].Value = pasted;
                }
            } catch (Exception e) {
                MessageBox.Show(string.Format("Failed to paste '{0}': {1}", pasted, e.Message));
            }
            dataGridView.Refresh();
        }
        #endregion
    }

 
    /*
     * Node representing a field containing field lists.
     */
    class ListTypeNode : TreeNode {
        public ListTypeNode(ListField list) : 
        base(string.Format("{0} ({1} entries)", list.Info.Name, list.Contained.Count)) {
            Tag = list;
            foreach (List<FieldInstance> infos in list.Contained) {
                Nodes.Add(new FieldInstanceNode(infos, (list.Info as ListType).NameAt));
            }
        }
        public ListField Field {
            get {
                return Tag as ListField;
            }
        }
    }
 
    /*
     * Class representing a list of fields.
     */
    class FieldInstanceNode : TreeNode {
        int nameIndex = -1;
        /*
         * The index into the field list to display as this node's text.
         */
        public int NameIndex {
            get {
                int result = nameIndex;
                if (result == -1) {
                    // use first string field; if we don't find any, just use the first field
                    result = 0;
                    for (int i = 0; i < SimpleFields.Count; i++) {
                        if (SimpleFields[i].Info.TypeCode == TypeCode.String) {
                            result = i;
                            break;
                        }
                    }
                }
                return result;
            }
            private set {
                nameIndex = value;
                Text = Fields[NameIndex].Value;
            }
        }

        public FieldInstanceNode(List<FieldInstance> fields, int nameIndex = -1) {
            Tag = fields;
            ListFields.ForEach(f => Nodes.Add (new ListTypeNode(f)));
            NameIndex = nameIndex;
        }

        #region Contained Field list access
        public DBRow Fields {
            get { return Tag as DBRow; }
        }

        public List<ListField> ListFields {
            get {
                List<ListField> result = new List<ListField>();
                Fields.ForEach(f => { if (f is ListField) { result.Add(f as ListField); } });
                return result;
            }
        }

        public List<FieldInstance> SimpleFields {
            get {
                List<FieldInstance> result = new List<FieldInstance>();
                Fields.ForEach(f => { if (!(f is ListField)) { result.Add(f); } });
                return result;
            }
        }
        #endregion
    }
}
