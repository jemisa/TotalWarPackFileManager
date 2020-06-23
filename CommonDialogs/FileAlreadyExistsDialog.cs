namespace CommonDialogs
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    public class FileAlreadyExistsDialog : Form
    {
        private Button cancelButton;
        private Action chosenAction;
        private IContainer components = null;
        private ComboBox defaultActionComboBox;
        private Label defaultActionLabel;
        private TextBox messageTextBox;
        private Button overwriteButton;
        private Button renameExistingButton;
        private Button renameNewButton;
        private Button skipButton;

        public FileAlreadyExistsDialog(string filepath)
        {
            this.InitializeComponent();
            this.CanRename = true;
            this.defaultActionComboBox.SelectedIndex = 0;
            this.messageTextBox.Text = string.Format("The file \"{0}\" already exists.\r\n\r\nDo you want to overwrite the existing file, skip this file, rename the existing file, or rename the new file?", Path.GetFileName(filepath));
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.chosenAction = Action.Cancel;
            base.Close();
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
            this.overwriteButton = new Button();
            this.skipButton = new Button();
            this.messageTextBox = new TextBox();
            this.renameExistingButton = new Button();
            this.renameNewButton = new Button();
            this.defaultActionLabel = new Label();
            this.defaultActionComboBox = new ComboBox();
            this.cancelButton = new Button();
            base.SuspendLayout();
            this.overwriteButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.overwriteButton.Location = new Point(0x131, 12);
            this.overwriteButton.Name = "overwriteButton";
            this.overwriteButton.Size = new Size(0x4b, 0x17);
            this.overwriteButton.TabIndex = 2;
            this.overwriteButton.Text = "Overwrite";
            this.overwriteButton.UseVisualStyleBackColor = true;
            this.overwriteButton.Click += new EventHandler(this.overwriteButton_Click);
            this.skipButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.skipButton.Location = new Point(0x182, 12);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new Size(0x4b, 0x17);
            this.skipButton.TabIndex = 3;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new EventHandler(this.skipButton_Click);
            this.messageTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.messageTextBox.BorderStyle = BorderStyle.None;
            this.messageTextBox.Location = new Point(12, 12);
            this.messageTextBox.Multiline = true;
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.ReadOnly = true;
            this.messageTextBox.Size = new Size(0x11f, 110);
            this.messageTextBox.TabIndex = 5;
            this.messageTextBox.Text = "The file \"foo\" already exists.\r\n\r\nDo you want to overwrite the existing file, skip this file, rename the existing file, or rename the new file?";
            this.renameExistingButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.renameExistingButton.Location = new Point(0x131, 0x29);
            this.renameExistingButton.Name = "renameExistingButton";
            this.renameExistingButton.Size = new Size(0x9c, 0x17);
            this.renameExistingButton.TabIndex = 6;
            this.renameExistingButton.Text = "Rename Existing File";
            this.renameExistingButton.UseVisualStyleBackColor = true;
            this.renameExistingButton.Click += new EventHandler(this.renameExistingButton_Click);
            this.renameNewButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.renameNewButton.Location = new Point(0x131, 70);
            this.renameNewButton.Name = "renameNewButton";
            this.renameNewButton.Size = new Size(0x9c, 0x17);
            this.renameNewButton.TabIndex = 7;
            this.renameNewButton.Text = "Rename New File";
            this.renameNewButton.UseVisualStyleBackColor = true;
            this.renameNewButton.Click += new EventHandler(this.renameNewButton_Click);
            this.defaultActionLabel.AutoSize = true;
            this.defaultActionLabel.Location = new Point(12, 0x90);
            this.defaultActionLabel.Name = "defaultActionLabel";
            this.defaultActionLabel.Size = new Size(0x10f, 13);
            this.defaultActionLabel.TabIndex = 8;
            this.defaultActionLabel.Text = "You can change the default action for all remaining files:";
            this.defaultActionComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.defaultActionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.defaultActionComboBox.FormattingEnabled = true;
            this.defaultActionComboBox.Items.AddRange(new object[] { "Ask for each file", "Overwrite all files", "Skip all files", "Rename existing files", "Rename new files" });
            this.defaultActionComboBox.Location = new Point(0x131, 0x8d);
            this.defaultActionComboBox.Name = "defaultActionComboBox";
            this.defaultActionComboBox.Size = new Size(0x9c, 0x15);
            this.defaultActionComboBox.TabIndex = 9;
            this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this.cancelButton.Location = new Point(0x131, 0x63);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new Size(0x9c, 0x17);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "Cancel Extraction";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
//            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1d9, 0xad);
            base.Controls.Add(this.cancelButton);
            base.Controls.Add(this.defaultActionComboBox);
            base.Controls.Add(this.defaultActionLabel);
            base.Controls.Add(this.renameNewButton);
            base.Controls.Add(this.renameExistingButton);
            base.Controls.Add(this.messageTextBox);
            base.Controls.Add(this.skipButton);
            base.Controls.Add(this.overwriteButton);
//            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "FileAlreadyExistsDialog";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "File Already Exists";
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void overwriteButton_Click(object sender, EventArgs e)
        {
            this.chosenAction = Action.Overwrite;
            base.Close();
        }

        private void renameExistingButton_Click(object sender, EventArgs e)
        {
            this.chosenAction = Action.RenameExisting;
            base.Close();
        }

        private void renameNewButton_Click(object sender, EventArgs e)
        {
            this.chosenAction = Action.RenameNew;
            base.Close();
        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            this.chosenAction = Action.Skip;
            base.Close();
        }

        public bool CanRename
        {
            get
            {
                return this.renameExistingButton.Enabled;
            }
            set
            {
                if (this.renameExistingButton.Enabled != value)
                {
                    if (value)
                    {
                        this.renameExistingButton.Enabled = true;
                        this.renameNewButton.Enabled = true;
                        this.defaultActionComboBox.Items.Add("Rename existing files");
                        this.defaultActionComboBox.Items.Add("Rename new files");
                    }
                    else
                    {
                        this.renameExistingButton.Enabled = false;
                        this.renameNewButton.Enabled = false;
                        this.defaultActionComboBox.Items.Remove("Rename existing files");
                        this.defaultActionComboBox.Items.Remove("Rename new files");
                    }
                }
            }
        }

        public Action ChosenAction
        {
            get
            {
                return this.chosenAction;
            }
        }

        public Action NextAction
        {
            get
            {
                return (Action) this.defaultActionComboBox.SelectedIndex;
            }
        }

        public enum Action
        {
            Ask,
            Overwrite,
            Skip,
            RenameExisting,
            RenameNew,
            Cancel
        }

        //public enum DefaultAction
        //{
        //    Ask,
        //    Overwrite,
        //    Skip,
        //    RenameExisting,
        //    RenameNew
        //}
    }
}

