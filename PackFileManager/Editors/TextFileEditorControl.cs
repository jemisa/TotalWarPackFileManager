using Common;
using Filetypes;
using Filetypes.Codecs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PackFileManager {
    /*
     * A simple integrated text editor.
     */
    public class TextFileEditorControl : PackedFileEditor<string> {
        
        #region file extensions for text files
        static string[] DEFAULT_EXTENSIONS = { ".txt", ".lua", ".csv", ".fx", ".fx_fragment", 
                ".h", ".battle_script", ".xml", ".tai", ".xml.rigging", ".placement", ".hlsl"
            };
        static readonly string EXTENSION_FILENAME = "text_extensions.txt";
        List<string> textExtensions = new List<string>();
        #endregion

        private IContainer components = null;
        private RichTextBox richTextBox;

        public TextFileEditorControl() : base(TextCodec.Instance) {
            this.InitializeComponent();

            richTextBox.TextChanged += (b, e) => DataChanged = true;
            richTextBox.KeyUp += HandleRichTextBoxKeyUp;
            
            // read text file containing text extensions (one per line)
            // or fill extension list with default
            try {
                string extensionFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), EXTENSION_FILENAME);
                textExtensions.AddRange(File.ReadAllLines(extensionFilePath));
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            if (textExtensions.Count == 0) {
                textExtensions.AddRange(DEFAULT_EXTENSIONS);
            }
        }
  
        /*
         * Since when doesn't the text box have copy/paste on its own?
         */
        void HandleRichTextBoxKeyUp (object sender, KeyEventArgs e) {
            if (e.Control) {
                if (e.KeyCode == Keys.C) {
                    richTextBox.Copy();
                } else if (e.KeyCode == Keys.V) {
                    richTextBox.Paste();
                }
            }
        }

        /*
         * Can edit if given file has one of the configured text file extensions.
         */
        public override bool CanEdit(PackedFile file) {
            return HasExtension(file, DEFAULT_EXTENSIONS);
        }

        public override bool ReadOnly {
            get {
                return base.ReadOnly;
            }
            set {
                base.ReadOnly = value;
                richTextBox.ReadOnly = value;
            }
        }

        public override string EditedFile {
            get {
                return richTextBox.Text;
            }
            set {
                richTextBox.Text = value;
                DataChanged = false;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (this.components != null))
            {
                Utilities.DisposeHandlers(this);
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.richTextBox = new RichTextBox();
            base.SuspendLayout();
            this.richTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.richTextBox.Location = new Point(0, 0);
            this.richTextBox.Name = "richTextBox1";
            this.richTextBox.Size = new Size(0x4a2, 0x28c);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.Controls.Add(this.richTextBox);
            base.Name = "TextFileEditorControl";
            base.Size = new Size(0x4a2, 0x28c);
            base.ResumeLayout(false);
        }
    }
}

