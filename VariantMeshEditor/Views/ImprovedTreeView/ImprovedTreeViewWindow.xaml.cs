using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TreeViewWithCheckBoxes
{
    public partial class ImprovedTreeViewWindow : UserControl
    {
        public ImprovedTreeViewWindow()
        {
            InitializeComponent();
            this.tree.Focus();
        }
    }
}