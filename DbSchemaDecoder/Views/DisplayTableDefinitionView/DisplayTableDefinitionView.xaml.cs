﻿using System;
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

namespace DbSchemaDecoder.DisplayTableDefinitionView
{
    /// <summary>
    /// Interaction logic for DisplayTableDefinitionView.xaml
    /// </summary>
    public partial class DisplayTableDefinitionView2 : UserControl
    {
        public DataGrid DbEntriesViewDataGrid 
        { 
            get 
            {
                var tableView = (DbTableViewTabItem.Content as DbTableView);
                return tableView.gridEmployees;
            } 
        }

        public DisplayTableDefinitionView2()
        {
            InitializeComponent();
        }
    }
}
