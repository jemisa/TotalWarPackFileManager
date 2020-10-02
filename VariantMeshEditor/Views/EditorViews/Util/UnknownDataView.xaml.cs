using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VariantMeshEditor.Views.EditorViews.Util
{
    /// <summary>
    /// Interaction logic for UnknownDataView.xaml
    /// </summary>
    public partial class UnknownDataView : UserControl
    {
        Byte[] _buffer;
        public UnknownDataView()
        {
            InitializeComponent();
            HexCheckBox.Click += (sender, e) => RedrawData();
        }

        void RedrawData()
        {
            TextBox.Document.Blocks.Clear();
            //TextBox.Document.PageWidth = GroupBox.ActualWidth;
            if (HexCheckBox.IsChecked == true)
            {
                string text = BitConverter.ToString(_buffer);
                TextBox.AppendText(text);
            }
            else
            {
                var text = (string.Join("-", _buffer));
                TextBox.AppendText(text);
            }
        }

        public void SetData(string displayName, Byte[] buffer)
        {
            GroupBox.Header = displayName;
            _buffer = buffer;
            RedrawData();
        }
    }
}
