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
using VariantMeshEditor.ViewModels;
using VariantMeshEditor.Views.EditorViews.Util;

namespace VariantMeshEditor.Views.EditorViews
{
    /// <summary>
    /// Interaction logic for RigidModelEditorView.xaml
    /// </summary>
    public partial class RigidModelEditorView : UserControl
    {
        public RigidModelEditorView()
        {
            InitializeComponent();
            /*return;
            {
                var item = new CollapsableButtonControl();
                item.Resources.Add("Header", "My Lod - 1");

                var stackpanel = new StackPanel();
                item.Content = stackpanel;


                stackpanel.Children.Add(CreateMesh("Group 0 - Mesh 0"));
                stackpanel.Children.Add(CreateMesh("Group 0 - Mesh 1"));
                stackpanel.Children.Add(CreateMesh("Group 0 - Mesh 2"));
                stackpanel.Children.Add(CreateMesh("Group 1 - Mesh 0"));

                LodStackPanel.Children.Add(item);
            }
            return;

            {
                var item1 = new CollapsableButtonControl();
                item1.Resources.Add("Header", "My Lod - 2");

                var stackpanel1 = new StackPanel();
                item1.Content = stackpanel1;


                stackpanel1.Children.Add(CreateMesh("Group 0 - Mesh 0"));
                stackpanel1.Children.Add(CreateMesh("Group 0 - Mesh 1"));
                stackpanel1.Children.Add(CreateMesh("Group 0 - Mesh 2"));
                stackpanel1.Children.Add(CreateMesh("Group 1 - Mesh 0"));

                LodStackPanel.Children.Add(item1);
            }*/
        }


        RigidModelMeshEditorView CreateMesh(string name)
        {
            var item = new RigidModelMeshEditorView();
            item.Resources.Add("Header", name);
            return item;
        }
    }
}
