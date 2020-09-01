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
using VariantMeshEditor.Views.Animation;
using VariantMeshEditor.Views.VariantMesh;

namespace VariantMeshEditor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();

            /*var b = new Button();
            b.Content = "Code button";
            DockPanel.SetDock(b, Dock.Top);
           // b.dock
            DockPanel.Children.Add(b);*/

            var path = @"C:\Users\ole_k\Desktop\ModelDecoding\brt_paladin\";
            var variantMeshDefinition = VariantMeshDefinition.Create(path + "VariantMeshDef.txt");



            var animationControl = new AnimationControl();
            animationControl.Initialize();
            this.ContentControl.Children.Add(animationControl);


            var VariantMeshContainer = new VariantMeshControl();
            VariantMeshContainer.Initialize(variantMeshDefinition);
            this.ContentControl.Children.Add(VariantMeshContainer);
          

         


            
        }
    }
}
