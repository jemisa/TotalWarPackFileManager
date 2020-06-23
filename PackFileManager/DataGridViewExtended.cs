using System.Text;
using System.Windows.Forms;

namespace PackFileManager
{
    public class DataGridViewExtended : DataGridView
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                var clipBoardString = new StringBuilder();
                for (int i = SelectedCells.Count - 1; i >= 0; i--)
                {
                    clipBoardString.Append(SelectedCells[i].Value);
                    if (i > 0) clipBoardString.Append(", ");
                }
                Clipboard.SetData(DataFormats.UnicodeText, clipBoardString.ToString());
            }

            if (keyData == (Keys.Control | Keys.V) && SelectedCells.Count == 1)
            {
                var data = Clipboard.GetData(DataFormats.UnicodeText);
                if (data.GetType() == SelectedCells[0].ValueType)
                    SelectedCells[0].Value = data;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
