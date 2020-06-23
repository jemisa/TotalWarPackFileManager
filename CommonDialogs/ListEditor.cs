using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CommonDialogs {
    public partial class ListEditor : Form {
        delegate void MoveToList(string item);

        public ListEditor() {
            InitializeComponent();

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        #region List Content Setting and Retrieval
        public List<string> LeftList {
            set {
                ToListBox(leftListBox, value);
            }
            get {
                return FromListBox(leftListBox);
            }
        }
        public List<string> RightList {
            set {
                ToListBox(rightListBox, value);
            }
            get {
                return FromListBox(rightListBox);
            }
        }
        #endregion

        // set to maintain an initial order when items move between lists
        // if not set, moved items will be added to the end of the target list
        public List<string> OriginalOrder { get; set; }

        #region Label setting
        public string LeftLabel {
            set {
                leftListLabel.Text = value;
            }
        }
        public string RightLabel {
            set {
                rightListLabel.Text = value;
            }
        }
        #endregion

        #region Move Selected
        private void toTheRightButton_Click(object sender, EventArgs e) {
            MoveSelected(leftListBox, rightListBox);
        }
        private void toTheLeftButton_Click(object sender, EventArgs e) {
            MoveSelected(rightListBox, leftListBox);
        }
        void MoveSelected(ListBox fromBox, ListBox toBox) {
            List<string> selected = new List<string>(fromBox.SelectedItems.Count);
            // need to copy into new list because removing the items will change SelectedItems
            foreach (object o in fromBox.SelectedItems) {
                selected.Add(o.ToString());
            }
            selected.ForEach(i => MoveBetweenLists(i.ToString(), fromBox, toBox));
        }
        #endregion

        // shouldn't setting AcceptButton close the dialog?
        private void okButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        #region Move All
        private void moveAllToTheRightButton_Click(object sender, EventArgs e) {
            MoveAll(leftListBox, rightListBox);
        }

        private void moveAllToTheLeftButton_Click(object sender, EventArgs e) {
            MoveAll(rightListBox, leftListBox);
        }

        void MoveAll(ListBox fromListBox, ListBox toListBox) {
            List<string> toMove = FromListBox(fromListBox);
            toMove.ForEach(item => MoveBetweenLists(item, fromListBox, toListBox));
        }
        #endregion

        #region Helpers
        // removes given item from fromList and adds it to appropriate position at toList
        // (if OriginalOrder is set)
        void MoveBetweenLists(string item, ListBox fromList, ListBox toList) {
            fromList.Items.Remove(item);
            int insertAt = FindInsertIndex(toList, item);
            toList.Items.Insert(insertAt, item);
        }

        // find index to add the given item to in the given list,
        // depending on OriginalOrder
        private int FindInsertIndex(ListBox listBox, string item) {
            int result = -1;
            if (OriginalOrder != null) {
                List<string> added = new List<string>(OriginalOrder);
                added.RemoveAll(i => (!item.Equals(i) && !listBox.Items.Contains(i)));
                result = added.IndexOf(item);
            }
            return result == -1 ? listBox.Items.Count : result;
        }
        #endregion

        #region Static Helpers
        // retrieve all items in the given box as list of strings
        static List<string> FromListBox(ListBox listBox) {
            List<string> items = new List<string>();
            foreach (object o in listBox.Items) {
                items.Add(o.ToString());
            }
            return items;
        }
        // set given listbox content to given items
        static void ToListBox(ListBox listBox, List<string> items) {
            listBox.Items.Clear();
            items.ForEach(i => listBox.Items.Add(i));
        }
        #endregion
    }
}
