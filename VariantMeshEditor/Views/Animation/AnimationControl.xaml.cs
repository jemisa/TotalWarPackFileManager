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

namespace VariantMeshEditor.Views.Animation
{
    /// <summary>
    /// Interaction logic for AnimationControl.xaml
    /// </summary>
    public partial class AnimationControl : UserControl
    {
        public AnimationControl()
        {
            InitializeComponent();
        }


        public void Initialize()
        { 
        
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
