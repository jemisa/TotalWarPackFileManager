using Filetypes.RigidModel;
using System.Windows.Controls;

namespace VariantMeshEditor.Views.VariantMesh
{
    /// <summary>
    /// Interaction logic for Slot.xaml
    /// </summary>
    public partial class VariantModelInstance : UserControl
    {
        public VariantModelInstance()
        {
            InitializeComponent();
        }

        public void Initialize(bool isActive, VariantMeshDefinition.VariantMesh mesh)
        {
            IsActiveCheckBox.IsChecked = isActive;
            FileNameTextBox.Text = mesh.Name;
        }
    }
}
