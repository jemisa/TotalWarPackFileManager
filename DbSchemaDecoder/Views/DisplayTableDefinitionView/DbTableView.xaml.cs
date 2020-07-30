using DbSchemaDecoder.Controllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

namespace DbSchemaDecoder.DisplayTableDefinitionView
{
    /// <summary>
    /// Interaction logic for DbTableView.xaml
    /// </summary>
    public partial class DbTableView : UserControl
    {
        //public DataMatrix MyDataMatrix { get; set; }

        
        public DbTableView()
        {

         



            InitializeComponent();
            //this.DataContext = this;
            //this.DataContext = this;
            //gridEmployees.DataContext = MyTable.DefaultView;
        }

        private void onKeyup(object sender, KeyEventArgs e)
        {
        
        }
    }

    public class DataMatrix : IEnumerable
    {
        public List<MatrixColumn> Columns { get; set; } = new List<MatrixColumn>();
        public List<object[]> Rows { get; set; } = new List<object[]>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new GenericEnumerator(Rows.ToArray());
        }
    }

    public class MatrixColumn
    {
        public string Name { get; set; }
        public string StringFormat { get; set; }
    }

    public class ListViewExtension
    {
        public static readonly DependencyProperty MatrixSourceProperty =
            DependencyProperty.RegisterAttached("MatrixSource",
            typeof(DataMatrix), typeof(ListViewExtension),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnMatrixSourceChanged)));

        public static DataMatrix GetMatrixSource(DependencyObject d)
        {
            return (DataMatrix)d.GetValue(MatrixSourceProperty);
        }

        public static void SetMatrixSource(DependencyObject d, DataMatrix value)
        {
            d.SetValue(MatrixSourceProperty, value);
        }

        private static void OnMatrixSourceChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ListView listView = d as ListView;
            DataMatrix dataMatrix = e.NewValue as DataMatrix;

            listView.ItemsSource = dataMatrix;
            GridView gridView = listView.View as GridView;
            int count = 0;
            gridView.Columns.Clear();
            foreach (var col in dataMatrix.Columns)
            {
                gridView.Columns.Add(
                    new GridViewColumn
                    {
                        Header = col.Name,
                        DisplayMemberBinding = new Binding(string.Format("[{0}]", count))
                    });
                count++;
            }
        }
    }

    class GenericEnumerator : IEnumerator
    {
        private readonly object[] _list;
        // Enumerators are positioned before the first element
        // until the first MoveNext() call. 

        private int _position = -1;

        public GenericEnumerator(object[] list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            _position++;
            return (_position < _list.Length);
        }

        public void Reset()
        {
            _position = -1;
        }

        public object Current
        {
            get
            {
                try { return _list[_position]; }
                catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }
    }
    
}
