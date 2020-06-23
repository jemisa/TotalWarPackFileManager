namespace PackFileManager
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class CaFileEditAdvisory : Form
    {
        private Button button1;
        private Button button2;
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private LinkLabel linkLabel1;

        public CaFileEditAdvisory()
        {
            this.InitializeComponent();
            base.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.No;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.button1 = new Button();
            this.button2 = new Button();
            this.linkLabel1 = new LinkLabel();
            this.label2 = new Label();
            base.SuspendLayout();
            this.label1.Font = new Font("Microsoft Sans Serif", 10f);
            this.label1.ForeColor = Color.Red;
            this.label1.Location = new Point(0x17, 20);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x179, 0x44);
            this.label1.TabIndex = 0;
            this.label1.Text = "Do not edit CA's files unless you are an advanced modder.\r\nIn many cases, edits to the original files will not show\r\nup in-game.  In others, changes will cause your game to crash.";
            this.button1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this.button1.Location = new Point(13, 0x98);
            this.button1.Name = "button1";
            this.button1.Size = new Size(0xad, 0x1d);
            this.button1.TabIndex = 1;
            this.button1.Text = "I'm not yet the man I hope to be.";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new EventHandler(this.button1_Click);
            this.button2.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.button2.Location = new Point(230, 0x98);
            this.button2.Name = "button2";
            this.button2.Size = new Size(0xad, 0x1d);
            this.button2.TabIndex = 2;
            this.button2.Text = "I'm not afraid, coward!";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new EventHandler(this.button2_Click);
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new Point(0x47, 0x7a);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new Size(0x124, 13);
            this.linkLabel1.TabIndex = 3;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "http://www.twcenter.net/forums/showthread.php?t=620537";
            this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            this.label2.ImageAlign = ContentAlignment.MiddleLeft;
            this.label2.Location = new Point(0x19, 0x58);
            this.label2.Name = "label2";
            this.label2.RightToLeft = RightToLeft.No;
            this.label2.Size = new Size(0x177, 0x22);
            this.label2.TabIndex = 4;
            this.label2.Text = "I've stared into the abyss and, alas, the abyss is staring back into me.\nWhere can I find help?";
            this.label2.TextAlign = ContentAlignment.BottomCenter;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x19f, 0xbb);
            base.Controls.Add(this.linkLabel1);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.button2);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.label1);
            base.Name = "caFileEditAdvisory";
            this.Text = "A moment of choice...";
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Help.ShowHelp(this, "http://www.twcenter.net/forums/showthread.php?t=340047");
        }
    }
}

