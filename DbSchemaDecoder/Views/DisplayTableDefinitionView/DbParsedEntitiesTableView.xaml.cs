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
    public partial class DbParsedEntitiesTableView : UserControl
    {
        public DbParsedEntitiesTableView()
        {
            InitializeComponent();
        }

        public DataGrid DbEntriesViewDataGrid
        { 
            get { return gridEmployees; }
        }
    }
}
