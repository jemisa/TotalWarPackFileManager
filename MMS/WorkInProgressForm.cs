using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MMS {
    public partial class WorkInProgressForm : Form {
        Thread thread = null;
        ThreadStart worker;

        public WorkInProgressForm(ThreadStart method) {
            InitializeComponent();
            worker = method;
        }

        public string Description {
            get {
                return workLabel.Text;
            }
            set {
                workLabel.Text = value;
            }
        }

        public void Start() {
            new Thread(StartThread).Start();
            Show();
        }

        private void StartThread() {
            worker();
            if (thread != null) {
                thread = new Thread(worker);
                thread.Start();
            }
            thread.Join();
            Close();
        }

        protected override CreateParams CreateParams {
            get {
                CreateParams p = base.CreateParams;
                p.ClassStyle = p.ClassStyle | 0x200;
                return p;
            }
        }
    }
}
