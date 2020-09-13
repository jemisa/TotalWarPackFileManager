using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace VariantMeshEditor.Views.EditorViews
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        public UserControl1()
        {
            InitializeComponent();

            List<Header> families = new List<Header>();

            Header family1 = new Header() { Name = "The Doe's" };
            family1.Members.Add(new FamilyMember2() {
                Name = "John Doe", 
                Age = 42, 
                Members = new ObservableCollection<Header>()
                { 
                    new Header() 
                    { 
                        Name = "Child fam",
                        Members = new ObservableCollection<BaseItem>()
                        { 
                            new FamilyMember()
                        }
                    }
                    
                } });

  

            families.Add(family1);



            Header family2 = new Header() { Name = "The Moe's" };
            family2.Members.Add(new FamilyMember()
            {
                Name = "John Doe",
                Age = 42,
                Members = new ObservableCollection<Header>() { new Header() { Name = "Child fam" } }
            });
            families.Add(family2);

            trvFamilies.ItemsSource = families;

        }
    }
    public class Header
    {
        public Header()
        {
            this.Members = new ObservableCollection<BaseItem>();
        }

        public string Name { get; set; }

        public ObservableCollection<BaseItem> Members { get; set; }
    }

    public class BaseItem
    { }

    public class FamilyMember : BaseItem
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public ObservableCollection<Header> Members { get; set; } = new ObservableCollection<Header>();
    }

    public class FamilyMember2 : BaseItem
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public ObservableCollection<Header> Members { get; set; } = new ObservableCollection<Header>();
    }

}
