namespace PackFileManager
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    public class CustomMessageBox : Form
    {
        private Button btnCancel;
        private Button btnNext;
        private Button btnOverwriteAll;
        private Button btnPrev;
        private Button btnReplace;
        private Button btnSearch;
        public static string buttonId;
        private IContainer components = null;
        private List<bool>[] equalsList;
        public Label lblMessage;
        public static CustomMessageBox newMessageBox;
        public TreeNode nextNode;
        public List<TreeNode> tnList = new List<TreeNode>();
        public int tnSelected;
        private TextBox txtSearch;
        private string[] wordList;

        public CustomMessageBox()
        {
            this.InitializeComponent();
            base.Size = new Size(600, 150);
            base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            base.ShowInTaskbar = false;
            this.MaximumSize = new Size(600, 150);
            base.CenterToScreen();
            this.btnReplace.Location = new Point(100, 0x56);
            this.btnReplace.Size = new Size(0x54, 0x1b);
            this.btnReplace.Text = "Replace";
            this.btnOverwriteAll.Location = new Point(0xe9, 0x56);
            this.btnOverwriteAll.Size = new Size(0x54, 0x1b);
            this.btnOverwriteAll.Text = "Overwrite All";
            this.btnCancel.Location = new Point(0x16e, 0x56);
            this.btnCancel.Size = new Size(0x54, 0x1b);
            this.btnCancel.Text = "Cancel";
            this.lblMessage.Location = new Point(9, 7);
            this.btnSearch.Location = new Point(100, 0x56);
            this.btnSearch.Size = new Size(0x54, 0x1b);
            this.btnSearch.Text = "Search";
            this.btnPrev.Location = new Point(0xe9, 0x56);
            this.btnPrev.Size = new Size(0x54, 0x1b);
            this.btnPrev.Text = "Previous";
            this.btnNext.Location = new Point(0x16e, 0x56);
            this.btnNext.Size = new Size(0x54, 0x1b);
            this.btnNext.Text = "Next";
            this.txtSearch.Location = new Point(100, 7);
            this.txtSearch.Size = new Size(300, 0x1b);
            this.txtSearch.Text = "Separate values with a space";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            buttonId = "5";
            newMessageBox.Dispose();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            for (int i = this.tnSelected + 1; i < this.tnList.Count; i++)
            {
                if (!this.equalsList[i].Contains(false))
                {
                    this.nextNode = this.tnList[i];
                    this.tnSelected = i;
                    i = this.tnList.Count;
                }
            }
            base.Owner.BringToFront();
        }

        private void btnOverwriteAll_Click(object sender, EventArgs e)
        {
            buttonId = "2";
            newMessageBox.Dispose();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            for (int i = this.tnSelected - 1; i > 0; i--)
            {
                if (!this.equalsList[i].Contains(false))
                {
                    this.nextNode = this.tnList[i];
                    this.tnSelected = i;
                    i = 0;
                }
            }
            base.Owner.BringToFront();
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            buttonId = "1";
            newMessageBox.Dispose();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.getText(this.txtSearch.Text);
            this.getEquality();
            try
            {
                for (int i = 0; i < this.tnList.Count; i++)
                {
                    if (!this.equalsList[i].Contains(false))
                    {
                        this.nextNode = this.tnList[i];
                        this.tnSelected = i;
                        i = this.tnList.Count;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            base.Owner.BringToFront();
        }

        private void customMessageBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.tnList.Clear();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void getEquality()
        {
            this.equalsList = new List<bool>[this.tnList.Count];
            for (int i = 0; i < this.tnList.Count; i++)
            {
                this.equalsList[i] = new List<bool>();
                for (int j = 0; j < this.wordList.Length; j++)
                {
                    this.equalsList[i].Add(this.tnList[i].Name.Contains(this.wordList[j]));
                }
            }
        }

        private void getText(string text)
        {
            this.wordList = Regex.Split(text, " ");
        }

        private void InitializeComponent()
        {
            this.btnOverwriteAll = new Button();
            this.lblMessage = new Label();
            this.btnCancel = new Button();
            this.btnReplace = new Button();
            this.btnSearch = new Button();
            this.btnNext = new Button();
            this.btnPrev = new Button();
            this.txtSearch = new TextBox();
            base.SuspendLayout();
            this.btnOverwriteAll.Location = new Point(-1, 0x2a);
            this.btnOverwriteAll.Name = "btnOverwriteAll";
            this.btnOverwriteAll.Size = new Size(0x4b, 0x17);
            this.btnOverwriteAll.TabIndex = 2;
            this.btnOverwriteAll.Text = "button2";
            this.btnOverwriteAll.UseVisualStyleBackColor = true;
            this.btnOverwriteAll.Click += new EventHandler(this.btnOverwriteAll_Click);
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new Point(80, 4);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new Size(0x23, 13);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "label1";
            this.btnCancel.Location = new Point(-1, -1);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(0x4b, 0x17);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "button2";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            this.btnReplace.Location = new Point(-1, 20);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new Size(0x4b, 0x17);
            this.btnReplace.TabIndex = 1;
            this.btnReplace.Text = "button1";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new EventHandler(this.btnReplace_Click);
            this.btnSearch.Location = new Point(-1, 0x48);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new Size(0x4b, 0x17);
            this.btnSearch.TabIndex = 5;
            this.btnSearch.Text = "button1";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
            this.btnNext.Location = new Point(-1, 0x5d);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new Size(0x4b, 0x17);
            this.btnNext.TabIndex = 7;
            this.btnNext.Text = "button1";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new EventHandler(this.btnNext_Click);
            this.btnPrev.Location = new Point(-1, 0x72);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new Size(0x4b, 0x17);
            this.btnPrev.TabIndex = 6;
            this.btnPrev.Text = "button1";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new EventHandler(this.btnPrev_Click);
            this.txtSearch.Location = new Point(-1, 0x88);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new Size(100, 20);
            this.txtSearch.TabIndex = 4;
            this.txtSearch.Click += new EventHandler(this.txtSearch_Click);
            base.AcceptButton = this.btnSearch;
//            base.AutoScaleDimensions = new SizeF(6f, 13f);
//            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x170, 0x9c);
            base.Controls.Add(this.txtSearch);
            base.Controls.Add(this.btnPrev);
            base.Controls.Add(this.btnNext);
            base.Controls.Add(this.btnSearch);
            base.Controls.Add(this.btnOverwriteAll);
            base.Controls.Add(this.lblMessage);
            base.Controls.Add(this.btnCancel);
            base.Controls.Add(this.btnReplace);
            base.Name = "customMessageBox";
            this.Text = "customMessageBox";
            base.FormClosing += new FormClosingEventHandler(this.customMessageBox_FormClosing);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public static string ShowBox(string txtMessage, string txtTitle)
        {
            newMessageBox = new CustomMessageBox();
            newMessageBox.btnNext.Visible = false;
            newMessageBox.btnPrev.Visible = false;
            newMessageBox.btnSearch.Visible = false;
            newMessageBox.txtSearch.Visible = false;
            newMessageBox.lblMessage.Visible = true;
            newMessageBox.AcceptButton = newMessageBox.btnReplace;
            newMessageBox.lblMessage.Text = txtMessage;
            newMessageBox.Text = txtTitle;
            newMessageBox.ShowDialog();
            return buttonId;
        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            if (this.txtSearch.Text == "Separate values with a space")
            {
                this.txtSearch.Text = "";
            }
        }
    }
}

