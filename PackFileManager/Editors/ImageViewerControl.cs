namespace PackFileManager {
    using Common;
    using FreeImageAPI;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;
    using Filetypes;

    public class ImageViewerControl : PackedFileEditor<Bitmap> {
        private AtlasFile atlasFile;
        private Button button1;
        private Button button2;
        private Button button3;
#pragma warning disable 649
        private readonly IContainer components;
#pragma warning restore 649
        private Rectangle[] grid;
        private PictureBox pictureBox1;
        private ToolTipRegion[] toolTipRegions;

        public ImageViewerControl() : base(new BitmapCodec()) {
            InitializeComponent();

            button1.MouseHover += Button1MouseHover;
            button2.MouseHover += Button2MouseHover;
            button3.MouseHover += Button3MouseHover;
        }

        static string[] EXTENSIONS = {
            ".tga",
            ".dds",
            ".png",
            ".jpg",
            ".bmp",
            ".psd"
                                     };
        public override bool CanEdit(PackedFile packedFile) {
            return HasExtension(packedFile, EXTENSIONS);
        }

        public override Bitmap EditedFile {
            get {
                return base.EditedFile;
            }
            set {
                button2.Enabled = false;
                button2.Click -= Button2Click;
                button3.Enabled = false;
                button3.Click -= Button3Click;
                pictureBox1.Enabled = false;

                try {
                    base.EditedFile = value;
                    if (pictureBox1.Image != null) {
                        pictureBox1.Image.Dispose();
                    }

                    pictureBox1.Image = EditedFile;
                    pictureBox1.Enabled = true;
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                } catch (Exception e) {
                    MessageBox.Show(string.Format("Error opening image file. \r\n FreeImage error : {0}", e.Message),
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public override PackedFile CurrentPackedFile {
            get {
                return base.CurrentPackedFile;
            }
            set {
                if (value != null) {
                    string filePath = Path.GetExtension(value.FullPath).Replace(".", "");
                    (codec as BitmapCodec).Format = filePath;
                    button1.Enabled = filePath.EndsWith(".dds");
                }
                base.CurrentPackedFile = value;
            }
        }

        private void Button1Click(object sender, EventArgs e) {
            var dialog = new OpenFileDialog {
                Filter = "Atlas File(*.atlas)|*.atlas"
            };
            if (dialog.ShowDialog() == DialogResult.OK) {
                try {
                    using (Stream stream = new FileStream(dialog.FileName, FileMode.Open)) {
                        atlasFile = AtlasCodec.Instance.Decode(stream);
                        CreateGrid();
                    }
                } catch (IOException) {
                    MessageBox.Show("Error opening *.atlas file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } else {
                dialog.Dispose();
            }
        }

        private void Button1MouseHover(object sender, EventArgs e) {
            new ToolTip().SetToolTip(button1, "Generate a grid overlay from an extracted .atlas file.");
        }

        private void Button2Click(object sender, EventArgs e) {
            // SetImage(file);
        }

        private void Button2MouseHover(object sender, EventArgs e) {
            new ToolTip().SetToolTip(button2, "Remove the grid overlay from a .dds texture.");
        }

        private void Button3Click(object sender, EventArgs e) {
            var dialog = new SaveFileDialog {
                FileName = "AtlasGrid",
                Filter = "PNG File(*.png)|*.png"
            };
            if (dialog.ShowDialog() == DialogResult.OK) {
                DrawBitmap(dialog.FileName);
            }
        }

        private void Button3MouseHover(object sender, EventArgs e) {
            new ToolTip().SetToolTip(button3, "Save the .atlas grid overlay to a PNG file.");
        }

        public void CloseImageViewerControl() {
            pictureBox1.Dispose();
            Dispose();
        }

        public void CreateGrid() {
            atlasFile.setPixelUnits(pictureBox1.Image.Height);
            grid = new Rectangle[atlasFile.numEntries];
            toolTipRegions = new ToolTipRegion[atlasFile.numEntries];
            for (int i = 0; i < grid.Length; i++) {
                AtlasObject aO = atlasFile.Entries[i];
                toolTipRegions[i] = new ToolTipRegion(aO);
                grid[i] = new Rectangle((int)aO.PX1, (int)aO.PY1, (int)aO.X3, (int)aO.Y3);
            }
            using (Graphics graphics = Graphics.FromImage(pictureBox1.Image)) {
                var pen = new Pen(Color.Red, 4f);
                graphics.DrawRectangles(pen, grid);
            }
            pictureBox1.Refresh();
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            button3.Click += Button3Click;
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                Utilities.DisposeHandlers(this);
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DrawBitmap(string filePath) {
            using (var bitmap = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height, PixelFormat.Format32bppArgb)) {
                using (Graphics graphics = Graphics.FromImage(bitmap)) {
                    graphics.DrawRectangles(Pens.Red, grid);
                }
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate)) {
                    using (var stream2 = new MemoryStream()) {
                        bitmap.Save(stream2, ImageFormat.Png);
                        stream2.WriteTo(stream);
                    }
                }
            }
        }

        public void DrawImage() {
            var location = pictureBox1.Location;
            var point2 = new Point {
                X = 0x200,
                Y = 0x200
            };
            using (Graphics graphics = Graphics.FromImage(pictureBox1.Image)) {
                var pen = new Pen(Color.Red, 3f);
                graphics.DrawRectangle(pen, location.X, location.Y, point2.X, point2.Y);
            }
        }

        private void InitializeComponent() {
            pictureBox1 = new PictureBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            pictureBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            pictureBox1.Location = new Point(0, 0x20);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(0x390, 0x27b);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            button1.Location = new Point(14, 3);
            button1.Name = "button1";
            button1.Size = new Size(0x53, 0x17);
            button1.TabIndex = 1;
            button1.Text = "Load .atlas";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Button1Click;
            button2.Location = new Point(0x67, 3);
            button2.Name = "button2";
            button2.Size = new Size(0x53, 0x17);
            button2.TabIndex = 2;
            button2.Text = "Unload .atlas";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Button2Click;
            button3.Location = new Point(0xc0, 3);
            button3.Name = "button3";
            button3.Size = new Size(0x53, 0x17);
            button3.TabIndex = 3;
            button3.Text = "Export Grid";
            button3.UseVisualStyleBackColor = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(pictureBox1);
            Name = "ImageViewerControl";
            Size = new Size(0x393, 0x29b);
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }
        
        public void SetImage(byte[] data, string filePath) {
            button3.Enabled = false;
            button3.MouseHover += Button3MouseHover;
            button2.Enabled = false;
            button2.MouseHover += Button2MouseHover;
            button1.MouseHover += Button1MouseHover;
            button1.Enabled = filePath.EndsWith(".dds");

            try {
                var ending = Path.GetExtension(filePath);
                FREE_IMAGE_FORMAT format;
                switch (ending) {
                    case ".tga":
                        format = FREE_IMAGE_FORMAT.FIF_TARGA;
                        break;
                    case ".dds":
                        format = FREE_IMAGE_FORMAT.FIF_DDS;
                        break;
                    case ".png":
                        format = FREE_IMAGE_FORMAT.FIF_PNG;
                        break;
                    case ".jpg":
                        format = FREE_IMAGE_FORMAT.FIF_JPEG;
                        break;
                    case ".bmp":
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                    case ".psd":
                        format = FREE_IMAGE_FORMAT.FIF_PSD;
                        break;
                    default:
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                }

                FreeImageBitmap bitmap;
                using (MemoryStream stream = new MemoryStream(data)) {
                    bitmap = new FreeImageBitmap(stream, format);
                }
                bitmap.ConvertType(FREE_IMAGE_TYPE.FIT_BITMAP, true);
                if (pictureBox1.Image != null) {
                    pictureBox1.Image.Dispose();
                }
                pictureBox1.Image = (Bitmap)bitmap;
            } catch (Exception e) {
                MessageBox.Show(string.Format("Error opening image file. \r\n FreeImage error : {0}", e.Message), 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class BitmapCodec : Codec<Bitmap> {
        public static readonly BitmapCodec Instance = new BitmapCodec();
        
        FREE_IMAGE_FORMAT format = FREE_IMAGE_FORMAT.FIF_BMP;
        public string Format {
            set {
                switch (value) {
                    case "tga":
                        format = FREE_IMAGE_FORMAT.FIF_TARGA;
                        break;
                    case "dds":
                        format = FREE_IMAGE_FORMAT.FIF_DDS;
                        break;
                    case "png":
                        format = FREE_IMAGE_FORMAT.FIF_PNG;
                        break;
                    case "jpg":
                        format = FREE_IMAGE_FORMAT.FIF_JPEG;
                        break;
                    case "bmp":
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                    case "psd":
                        format = FREE_IMAGE_FORMAT.FIF_PSD;
                        break;
                    default:
                        format = FREE_IMAGE_FORMAT.FIF_BMP;
                        break;
                }
            }
        }
        public Bitmap Decode(Stream stream) {
            try {
                FreeImageBitmap bitmap = new FreeImageBitmap(stream, format);
                bitmap.ConvertType(FREE_IMAGE_TYPE.FIT_BITMAP, true);
                return (Bitmap)bitmap;
            } catch (Exception e) {
                Console.WriteLine(e);
            }
            return null;
        }
        public void Encode(Stream stream, Bitmap toEncode) {
            throw new NotImplementedException ();
        }
    }
}
