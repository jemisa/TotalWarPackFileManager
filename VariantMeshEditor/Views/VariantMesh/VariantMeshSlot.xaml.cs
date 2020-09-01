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
    /// Interaction logic for VariantMeshSlot.xaml
    /// </summary>
    public partial class VariantMeshSlot : UserControl
    {
        bool IsOpen = true;
        int _originalHeight = 0;


        SLOT _data;


        public VariantMeshSlot()
        {
            InitializeComponent();
            this.Loaded += VariantMeshSlot_Loaded;
        }

        private void VariantMeshSlot_Loaded(object sender, RoutedEventArgs e)
        {
            if (_data.VariantMeshes.Count() == 0)
                Button_Click(null, null);
        }

        public void Add(SLOT slot)
        {
            _data = slot;
            this.PanelButton.Content = slot.Name;

            for (int i = 0; i < slot.VariantMeshes.Count(); i++)
            {
                VariantModelInstance mySlot = new VariantModelInstance();
                mySlot.Initialize(i == 0, slot.VariantMeshes[i]);
                SlotStackPanel.Children.Add(mySlot);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpen && _originalHeight == 0)
                _originalHeight = (int)this.ActualHeight;

            if (IsOpen)
                this.Height = (int)this.PanelButton.ActualHeight;
            else
                this.Height = _originalHeight;
            IsOpen = !IsOpen;
        }

    }
}
