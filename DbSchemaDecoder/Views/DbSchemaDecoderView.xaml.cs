using DbSchemaDecoder.Controllers;
using DbSchemaDecoder.DisplayTableDefinitionView;
using DbSchemaDecoder.EditTableDefinitionView;
using Filetypes.Codecs;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
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

namespace DbSchemaDecoder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DbSchemaDecoder : UserControl
    {
        PersonsViewModel _temp = new PersonsViewModel();

        DbSchemaDecoderController _mainController;

        FileListController _fileListController;

        public DbSchemaDecoder()
        {
            InitializeComponent();
            Loaded += SettingsControl_Loaded;
        }

        private void SettingsControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This function crashes winforms editing in visual studio, so return early if that is the case
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            _fileListController = new FileListController();
            _mainController = new DbSchemaDecoderController(_fileListController);

            // Hex stuff
            _fileListController.OnFileSelectedEvent += _fileListController_MyEvent;

            FindController<FileListView>().DataContext = _fileListController;
            //FindController<ConfigureTableRowsView>().DataContext = _configureTableRowsController;
            FindController<InformationView>().DataContext = _mainController;
            FindController<ConfigureTableRowsView>().DataContext = _mainController;
            FindController<DisplayTableDefinitionView2>().DataContext = _temp;
            //FindController<DbTableView>().DataContext = _mainController;
        }

     


        //----Hex stuff
        private void _fileListController_MyEvent(object sender, DataBaseFile e)
        {
            HexEdit.Stream = new MemoryStream(e.DbFile.Data);
            // HexEdit.hex
            _temp.Add();
        }
        //---

        T FindController<T>() where T : DependencyObject
        {
            // Helper function to work around the issue that we can not set names in the xaml file becouse of ?

            var allControllers = FindVisualChildren<T>(GetVisualChild(0));
            return allControllers.First();
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void OnShowFileListClick(object sender, RoutedEventArgs e)
        {
            if(FileListColumn.Width.Value == 0)
                FileListColumn.Width = new GridLength(350);
            else
                FileListColumn.Width = new GridLength(0);
        }

        private void OnShowHexClick(object sender, RoutedEventArgs e)
        {
            if (HexViewColumn.Width.Value == 0)
                HexViewColumn.Width = new GridLength(600);
            else
                HexViewColumn.Width = new GridLength(0);
        }
    }

    class ColumnDescriptor
    {
        public string HeaderText { get; set; }
        public string DisplayMember { get; set; }
    }

    class PersonsViewModel
    {
        public PersonsViewModel()
        {
            List<string> headers = new List<string>() { "Id", "Age", "Content" };
            List<string[]> items = new List<string[]>();
            items.Add(new string[] { "0", "25", "My age is 25" });
            items.Add(new string[] { "1", "23", "My age is 23" });
            items.Add(new string[] { "2", "27", "My age is 27" });

            this.Persons = new ObservableCollection<Object>
            {
                new 
                {
                    Name = "Doe",
                    FirstName = "John",
                    DateOfBirth = new DateTime(1981, 9, 12)
                },
                new                 {
                    Name = "Black",
                    FirstName = "Jack",
                    DateOfBirth = new DateTime(1950, 1, 15)
                },
                new 
                {
                    Name = "Smith",
                    FirstName = "Jane",
                    DateOfBirth = new DateTime(1987, 7, 23)
                }
            };

            dynamic data = new ExpandoObject();

            IDictionary<string, object> dictionary = (IDictionary<string, object>)data;
            dictionary.Add("Name", "Bob");
            dictionary.Add("FirstName", "Smith");

            //Persons.Add(data as Object);

            this.Columns = new ObservableCollection<ColumnDescriptor>
            {
                new ColumnDescriptor { HeaderText = "Last name", DisplayMember = "Name" },
                new ColumnDescriptor { HeaderText = "First name", DisplayMember = "FirstName" },
                new ColumnDescriptor { HeaderText = "Date of birth", DisplayMember = "DateOfBirth" }
            };
        }


        public void Add()
        {
            Persons.Add(
                new 
                {
                    Name = "Smith",
                    FirstName = "Jane",
                    DateOfBirth = new DateTime(1987, 7, 23)
                });
        }
        public ObservableCollection<Object> Persons { get; private set; }

        public ObservableCollection<ColumnDescriptor> Columns { get; private set; }

        private ICommand _addColumnCommand;
        public ICommand AddColumnCommand
        {
            get
            {
                if (_addColumnCommand == null)
                {
                    _addColumnCommand = new RelayCommand<string>(
                        s =>
                        {
                            this.Columns.Add(new ColumnDescriptor { HeaderText = s, DisplayMember = s });
                        });
                }
                return _addColumnCommand;
            }
        }

        private ICommand _removeColumnCommand;
        public ICommand RemoveColumnCommand
        {
            get
            {
                if (_removeColumnCommand == null)
                {
                    _removeColumnCommand = new RelayCommand<string>(
                        s =>
                        {
                            this.Columns.Remove(this.Columns.FirstOrDefault(d => d.DisplayMember == s));
                        });
                }
                return _removeColumnCommand;
            }
        }
    }


    


}
