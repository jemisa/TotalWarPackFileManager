using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace DbSchemaDecoder.EditTableDefinitionView
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DbTypeInstanceView : UserControl, INotifyPropertyChanged
    {
        public string Property1
        {
            get { return (string)GetValue(Property1Property); }
            set { SetValue(Property1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Property1.  
        // This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Property1Property
            = DependencyProperty.Register(
                  "Property1",
                  typeof(string),
                  typeof(DbTypeInstanceView),
                  new FrameworkPropertyMetadata("Label2", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
              );
        public DbTypeInstanceView()
        {
            InitializeComponent();
            //this.DataContext = this;
        }

        private void UseButton_Click(object sender, RoutedEventArgs e)
        {
            Property1 = "smack";
            NotifyPropertyChanged("Property1");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
