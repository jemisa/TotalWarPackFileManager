using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PackFileManager {
    /*
     * Manages copy/paste from and into a DataGridView.
     * Uses the system clipboard to be able to copy from and paste into outside applications.
     */
    class GridViewCopyPaste {
        
        // notifies clients that a copy or paste took place
        public delegate void CopyPasteEvent();
        public event CopyPasteEvent Copied;
        public event CopyPasteEvent Pasted;

        DataGridView dataGridView;
        public GridViewCopyPaste(DataGridView view) {
            dataGridView = view;
            dataGridView.KeyUp += CopyPaste;
        }

        private void CopyPaste(object sender, KeyEventArgs arge) {
            if (arge.Control) {
                if (arge.KeyCode == Keys.C) {
                    CopyEvent();
                } else if (arge.KeyCode == Keys.V) {
                    PasteEvent();
                }
            }
        }
        
        /*
         * Retrieve the cells in the given collection of selected cells as a two-dimensional data structure
         * (rows/columns).
         */
        List<List<DataGridViewCell>> SelectedCells(DataGridViewSelectedCellCollection collection) {
            List<List<DataGridViewCell>> rows = new List<List<DataGridViewCell>>();
            foreach (DataGridViewCell cell in collection) {
                int rowIndex = cell.RowIndex;
                while (rowIndex >= rows.Count) {
                    rows.Add(new List<DataGridViewCell>());
                }
                List<DataGridViewCell> addTo = rows[rowIndex];
                while (cell.ColumnIndex >= addTo.Count) {
                    addTo.Add(null);
                }
                addTo[cell.ColumnIndex] = cell;
            }
            List<List<DataGridViewCell>> result = new List<List<DataGridViewCell>>();
            rows.ForEach(row => {
                if (row.Count > 0) {
                    List<DataGridViewCell> newRow = new List<DataGridViewCell>();
                    row.ForEach(cell => { if (cell != null) newRow.Add(cell); });
                    result.Add(newRow);
                }
            });
            return result;
        }
        /*
         * Event handler for copy.
         */
        public void CopyEvent() {
            if (dataGridView.SelectedCells.Count == 0) {
                return;
            }

            string encoded = "";
            DataGridViewSelectedCellCollection cells = dataGridView.SelectedCells;
            List<List<DataGridViewCell>> selected = SelectedCells(cells);
            for (int rowNum = 0; rowNum < selected.Count; rowNum++) {
                List<DataGridViewCell> row = selected[rowNum];
                string line = "";
                for (int colNum = 0; colNum < row.Count; colNum++) {
                    line += row[colNum].Value + "\t";
                }
                line.Remove(line.LastIndexOf("\t"));
                encoded += line + "\n";
            }

            Clipboard.SetText(encoded);
            if (encoded.Length > 2 && Copied != null) {
                Copied();
            }
        }

        /*
         * Event handler for paste.
         */
        public void PasteEvent() {
            string encoded = Clipboard.GetText();
            string[] lines = encoded.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[][] values = new string[lines.Length][];
            for (int i = 0; i < lines.Length; i++) {
                string[] line = lines[i].Split(new char[] { '\t' });
                values[i] = line;
            }

            DataGridViewSelectedCellCollection cells = dataGridView.SelectedCells;
            List<List<DataGridViewCell>> selected = SelectedCells(cells);
            for (int rowNum = 0; rowNum < Math.Min(values.Length, selected.Count); rowNum++) {
                List<DataGridViewCell> row = selected[rowNum];
                string[] rowValues = values[rowNum];
                for (int col = 0; col < Math.Min(values.Length, row.Count); col++) {
                    try {
                        string setValue = rowValues[col];
                        row[col].Value = setValue;
                    } catch (Exception e) {
                        MessageBox.Show(string.Format("Could not paste {0}/{1}: {2}", rowNum, col, e), "Failed to paste");
                        return;
                    }
                }
            }

            if (Pasted != null) {
                Pasted();
            }
        }
    }
}
