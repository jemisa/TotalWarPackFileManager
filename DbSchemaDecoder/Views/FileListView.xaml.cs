﻿using DbSchemaDecoder.Controllers;
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

namespace DbSchemaDecoder
{
    /// <summary>
    /// Interaction logic for FileListView.xaml
    /// </summary>
    public partial class FileListView : UserControl
    {
        //FileListController _controller;

        public FileListView()
        {
           // _controller = new FileListController();

            InitializeComponent();
            //FileListView1.DataContext = _controller;
        }

  
    }
}
