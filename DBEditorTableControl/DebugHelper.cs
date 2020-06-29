using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DBEditorTableControl
{
    class DebugHelper
    {
        public static void PreviewKeyDown(DBTableControl.DBEditorTableControl mainTable, KeyEventArgs e)
        {
#if DEBUG
            // Set Ctrl-B as testing key;
            if (e.Key == Key.B && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                string rowswitherrors = "Rows that have logged errors: \n\n";
                for (int i = 0; i < mainTable.CurrentTable.Rows.Count; i++)
                {
                    if (mainTable.CurrentTable.Rows[i].HasErrors)
                    {
                        rowswitherrors = rowswitherrors + i + "\n";
                    }
                }

                MessageBox.Show(rowswitherrors);
            }
            else if (e.Key == Key.D && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var test = mainTable.EditedFile.Entries[0];
                var type = mainTable.EditedFile.CurrentType;
            }
            else if (e.Key == Key.Z && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                string clipboardtext = ClipboardHelper.GetClipboardText();

                clipboardtext = clipboardtext.Replace("\t", "\\t");
                clipboardtext = clipboardtext.Replace("\r", "\\r");
                clipboardtext = clipboardtext.Replace("\n", "\\n");

                MessageBox.Show(clipboardtext);
            }
            else if (e.Key == Key.X && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                var cell = mainTable.GetCell(0, 0);
                string outputstring = "";

                for (int i = 0; i < mainTable.dbDataGrid.Items.Count; i++)
                {
                    if (!(mainTable.dbDataGrid.Items[i] is DataRowView))
                    {
                        continue;
                    }

                    int datarow = mainTable.CurrentTable.Rows.IndexOf((mainTable.dbDataGrid.Items[i] as DataRowView).Row);
                    outputstring = outputstring + String.Format("Visual row {0}, stored at CurrentTable[{1}].\n\r", i, datarow);
                }

                MessageBox.Show(outputstring);
            }
#endif
        }
    }
}
