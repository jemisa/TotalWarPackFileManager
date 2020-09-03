using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TreeViewWithCheckBoxes
{
    public partial class Window1 : UserControl
    {
        public Window1()
        {
            InitializeComponent();

           // TreeViewDataModel root = this.tree.Items[0] as TreeViewDataModel;

           

            this.tree.Focus();
        }
    }
}