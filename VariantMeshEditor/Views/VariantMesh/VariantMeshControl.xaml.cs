using Filetypes.RigidModel;
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
using static Filetypes.RigidModel.VariantMeshDefinition;

namespace VariantMeshEditor.Views.VariantMesh
{
    /// <summary>
    /// Interaction logic for VariantMeshControl.xaml
    /// </summary>
    public partial class VariantMeshControl : UserControl
    {


        public VariantMeshControl()
        {
            InitializeComponent();

            this.Loaded += VariantMeshControl_Loaded;
        }

        private void VariantMeshControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Button_Click(null, null);
        }

        public void Initialize(VariantMeshFile file)
        {
            foreach (var item in file.VARIANT_MESH.SLOT)
            {
                var a = new VariantMeshSlot();
                a.Add(item);
                DockPanel.SetDock(a, Dock.Top);
                VariantMeshContainer.Children.Add(a);
            }
        }

        bool IsOpen = true;
        int _originalHeight = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpen && _originalHeight == 0)
                _originalHeight = (int)this.ActualHeight;

            if (IsOpen)
                this.Height = (int)this.ControllerMainButton.ActualHeight;
            else
                this.Height = _originalHeight;
            IsOpen = !IsOpen;
        }
    }
}
