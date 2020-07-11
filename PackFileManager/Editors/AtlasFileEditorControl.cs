namespace PackFileManager
{
    using BrightIdeasSoftware;
    using Common;
    using Filetypes;
    using Filetypes.Codecs;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class AtlasFileEditorControl : PackedFileEditor<AtlasFile>
    {
        private ToolStripMenuItem addAtlasEntryToolStripMenuItem;
        private CheckBox toggleUnitsCheckbox;
        private IContainer components;
        private ContextMenuStrip contextMenuStrip1;
        private OLVColumn CSCont1;
        private OLVColumn CSCont2;
        private OLVColumn CX1;
        private OLVColumn CX1P;
        private OLVColumn CX2;
        private OLVColumn CX2P;
        private OLVColumn CX3;
        private OLVColumn CY1;
        private OLVColumn CY1P;
        private OLVColumn CY2;
        private OLVColumn CY2P;
        private OLVColumn CY3;
        private ToolStripMenuItem exportEntriesToTextFileToolStripMenuItem;
        private float imageHeight;
        private Label heightLabel;
        //private ObjectListView olv;
        private ObjectListView olv;
        private OLVColumn olvColumn1;
        private ToolStripMenuItem removeAtlasEntryToolStripMenuItem;
        private TextBox textBox;
        
        public AtlasFileEditorControl() : base (AtlasCodec.Instance)
        {
            this.components = null;
            this.DataChanged = false;
            this.imageHeight = 4096f;
            this.InitializeComponent();
            this.textBox.Text = this.imageHeight.ToString();
            // EditedFile = AtlasCodec.Instance.Decode(packedFile);
            // EditedFile.setPixelUnits(this.imageHeight);
            this.CSCont1.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.CSCont2.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.olv.CellEditFinishing += new CellEditEventHandler(this.olv_CellEditFinishing);
        }

        string[] EXTENSIONS = { ".atlas" };
        public override bool CanEdit(PackedFile file) {
            return PackedFileEditorHelper.HasExtension(file, EXTENSIONS);
        }

        public override AtlasFile EditedFile {
            get { return base.EditedFile; }
            set {
                base.EditedFile = value;
                if (value != null) {
                    value.setPixelUnits(imageHeight);
                    olv.Clear();
                    this.olv.AddObjects(this.EditedFile.Entries);
                    Console.WriteLine("Rows: {0}", this.olv.GetItemCount());
                }
            }
        }

        private void addAtlasEntry_Click(object sender, EventArgs e)
        {
            AtlasObject newEntry = new AtlasObject {
                Container1 = "NEW_ENTRY",
                Container2 = "NEW_ENTRY"
            };
            EditedFile.add(newEntry);
            this.olv.AddObject(newEntry);
            this.DataChanged = true;
            this.olv.EnsureModelVisible(newEntry);
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.toggleUnitsCheckbox.Checked)
            {
                this.CX1.MaximumWidth = 0;
                this.CY1.MaximumWidth = 0;
                this.CX2.MaximumWidth = 0;
                this.CY2.MaximumWidth = 0;
                this.CX1P.MaximumWidth = -1;
                this.CY1P.MaximumWidth = -1;
                this.CX2P.MaximumWidth = -1;
                this.CY2P.MaximumWidth = -1;
                this.CX1.Width = 0;
                this.CY1.Width = 0;
                this.CX2.Width = 0;
                this.CY2.Width = 0;
                this.CX1P.Width = 60;
                this.CY1P.Width = 60;
                this.CX2P.Width = 60;
                this.CY2P.Width = 60;
            }
            else
            {
                this.CY1P.MaximumWidth = 0;
                this.CX2P.MaximumWidth = 0;
                this.CY1P.MaximumWidth = 0;
                this.CX1P.MaximumWidth = 0;
                this.CX1.MaximumWidth = -1;
                this.CY1.MaximumWidth = -1;
                this.CX2.MaximumWidth = -1;
                this.CY2.MaximumWidth = -1;
                this.CX1P.Width = 0;
                this.CY1P.Width = 0;
                this.CX2P.Width = 0;
                this.CY2P.Width = 0;
                this.CX1.Width = 60;
                this.CY1.Width = 60;
                this.CX2.Width = 60;
                this.CY2.Width = 60;
            }
            this.olv.Refresh();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                Utilities.DisposeHandlers(this);
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void exportTSV_Click(object sender, EventArgs e)
        {
            List<string> strings = new List<string>();
            foreach (OLVListItem item in this.olv.Items)
            {
                AtlasObject rowObject = (AtlasObject) item.RowObject;
                strings.Add(rowObject.Container1 + "\t" + 
                            rowObject.Container2 + "\t" + 
                            rowObject.X1.ToString() + "\t" + 
                            rowObject.Y1.ToString() + "\t" + 
                            rowObject.X3.ToString() + "\t" + 
                            rowObject.Y3.ToString());
            }
            PackedFileEditorHelper.WriteToTSVFile(strings);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.olv = new ObjectListView();
            this.olvColumn1 = new OLVColumn();
            this.CSCont1 = new OLVColumn();
            this.CSCont2 = new OLVColumn();
            this.CX1P = new OLVColumn();
            this.CY1P = new OLVColumn();
            this.CX2P = new OLVColumn();
            this.CY2P = new OLVColumn();
            this.CX1 = new OLVColumn();
            this.CY1 = new OLVColumn();
            this.CX2 = new OLVColumn();
            this.CY2 = new OLVColumn();
            this.CX3 = new OLVColumn();
            this.CY3 = new OLVColumn();
            this.contextMenuStrip1 = new ContextMenuStrip(this.components);
            this.addAtlasEntryToolStripMenuItem = new ToolStripMenuItem();
            this.removeAtlasEntryToolStripMenuItem = new ToolStripMenuItem();
            this.exportEntriesToTextFileToolStripMenuItem = new ToolStripMenuItem();
            this.textBox = new TextBox();
            this.toggleUnitsCheckbox = new CheckBox();
            this.heightLabel = new Label();
            ((ISupportInitialize) this.olv).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            base.SuspendLayout();
            this.olv.AllColumns.Add(this.olvColumn1);
            this.olv.AllColumns.Add(this.CSCont1);
            this.olv.AllColumns.Add(this.CSCont2);
            this.olv.AllColumns.Add(this.CX1P);
            this.olv.AllColumns.Add(this.CY1P);
            this.olv.AllColumns.Add(this.CX2P);
            this.olv.AllColumns.Add(this.CY2P);
            this.olv.AllColumns.Add(this.CX1);
            this.olv.AllColumns.Add(this.CY1);
            this.olv.AllColumns.Add(this.CX2);
            this.olv.AllColumns.Add(this.CY2);
            this.olv.AllColumns.Add(this.CX3);
            this.olv.AllColumns.Add(this.CY3);
            this.olv.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.olv.CausesValidation = false;
            this.olv.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
            this.olv.Columns.AddRange(new ColumnHeader[] { 
                this.olvColumn1, this.CSCont1, this.CSCont2, this.CX1P, this.CY1P, this.CX2P, this.CY2P, this.CX1, this.CY1, this.CX2, this.CY2, this.CX3, this.CY3 });
            this.olv.Cursor = Cursors.Default;
            this.olv.HasCollapsibleGroups = false;
            this.olv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.olv.Location = new Point(0, 0x1c);
            this.olv.Name = "olv";
            this.olv.ShowGroups = false;
            this.olv.Size = new Size(0x361, 0x27e);
            this.olv.SortGroupItemsByPrimaryColumn = false;
            this.olv.SpaceBetweenGroups = 10;
            this.olv.TabIndex = 0;
            this.olv.UseCompatibleStateImageBehavior = false;
            this.olv.View = View.Details;
            this.olv.CellRightClick += new EventHandler<CellRightClickEventArgs>(this.olv_RightClick);
            this.olv.CellEditFinishing += new CellEditEventHandler(this.olv_CellEditFinishing);
            this.olvColumn1.HeaderFont = null;
            this.olvColumn1.IsVisible = false;
            this.olvColumn1.MaximumWidth = 0;
            this.olvColumn1.MinimumWidth = 0;
            this.olvColumn1.Width = 0;
            this.CSCont1.AspectName = "Container1";
            this.CSCont1.AutoCompleteEditor = false;
            this.CSCont1.HeaderFont = null;
            this.CSCont1.Text = "String1";
            this.CSCont1.Width = 200;
            this.CSCont2.AspectName = "Container2";
            this.CSCont2.HeaderFont = null;
            this.CSCont2.Text = "String2";
            this.CSCont2.Width = 200;
            this.CX1P.AspectName = "PX1";
            this.CX1P.HeaderFont = null;
            this.CX1P.Text = "X1(PU)";
            this.CX1P.ToolTipText = "Upper Left Coordinate in PU";
            this.CX1P.Width = 0;
            this.CY1P.AspectName = "PY1";
            this.CY1P.HeaderFont = null;
            this.CY1P.Text = "Y1(PU)";
            this.CY1P.ToolTipText = "Upper Left Coordinate in PU";
            this.CY1P.Width = 0;
            this.CX2P.AspectName = "PX2";
            this.CX2P.HeaderFont = null;
            this.CX2P.Text = "X2(PU)";
            this.CX2P.ToolTipText = "Lower Right Coordinate in PU";
            this.CX2P.Width = 0;
            this.CY2P.AspectName = "PY2";
            this.CY2P.HeaderFont = null;
            this.CY2P.Text = "Y2(PU)";
            this.CY2P.ToolTipText = "Lower Right Coordinate in PU";
            this.CY2P.Width = 0;
            this.CX1.AspectName = "X1";
            this.CX1.HeaderFont = null;
            this.CX1.Text = "X1(TU)";
            this.CX1.ToolTipText = "Upper Left Coordinate in TU";
            this.CX1.Width = 50;
            this.CY1.AspectName = "Y1";
            this.CY1.HeaderFont = null;
            this.CY1.Text = "Y1(TU)";
            this.CY1.ToolTipText = "Upper Left Coordinate in TU";
            this.CY1.Width = 50;
            this.CX2.AspectName = "X2";
            this.CX2.HeaderFont = null;
            this.CX2.Text = "X2(TU)";
            this.CX2.ToolTipText = "Lower Right Coordinate in TU";
            this.CX2.Width = 50;
            this.CY2.AspectName = "Y2";
            this.CY2.HeaderFont = null;
            this.CY2.Text = "Y2(TU)";
            this.CY2.ToolTipText = "Lower Right Coordinate in TU";
            this.CY2.Width = 50;
            this.CX3.AspectName = "X3";
            this.CX3.HeaderFont = null;
            this.CX3.Text = "Width(PU)";
            this.CX3.ToolTipText = "Width (X2-X1) in PU";
            this.CX3.Width = 0x3d;
            this.CY3.AspectName = "Y3";
            this.CY3.AutoCompleteEditor = false;
            this.CY3.HeaderFont = null;
            this.CY3.Text = "Height(PU)";
            this.CY3.ToolTipText = "Height (Y2-Y1) in PU";
            this.CY3.Width = 0x41;
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] { 
                this.addAtlasEntryToolStripMenuItem, this.removeAtlasEntryToolStripMenuItem, this.exportEntriesToTextFileToolStripMenuItem });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new Size(0xcc, 70);
            this.addAtlasEntryToolStripMenuItem.Name = "addAtlasEntryToolStripMenuItem";
            this.addAtlasEntryToolStripMenuItem.Size = new Size(0xcb, 0x16);
            this.addAtlasEntryToolStripMenuItem.Text = "Add Atlas Entry";
            this.addAtlasEntryToolStripMenuItem.Click += new EventHandler(this.addAtlasEntry_Click);
            this.removeAtlasEntryToolStripMenuItem.Name = "removeAtlasEntryToolStripMenuItem";
            this.removeAtlasEntryToolStripMenuItem.Size = new Size(0xcb, 0x16);
            this.removeAtlasEntryToolStripMenuItem.Text = "Remove Atlas Entry";
            this.removeAtlasEntryToolStripMenuItem.Click += new EventHandler(this.removeAtlasEntry_Click);
            this.exportEntriesToTextFileToolStripMenuItem.Name = "exportEntriesToTextFileToolStripMenuItem";
            this.exportEntriesToTextFileToolStripMenuItem.Size = new Size(0xcb, 0x16);
            this.exportEntriesToTextFileToolStripMenuItem.Text = "Export Entries to TSV File";
            this.exportEntriesToTextFileToolStripMenuItem.Click += new EventHandler(this.exportTSV_Click);
            this.textBox.AcceptsReturn = true;
            this.textBox.Location = new Point(0x4f, 3);
            this.textBox.Name = "textBox1";
            this.textBox.Size = new Size(0x2e, 20);
            this.textBox.TabIndex = 2;
            this.textBox.KeyDown += new KeyEventHandler(this.textBox1_KeyDown);
            this.textBox.MouseHover += new EventHandler(this.textBox1_MouseHover);
            this.toggleUnitsCheckbox.AutoSize = true;
            this.toggleUnitsCheckbox.Location = new Point(0xa3, 5);
            this.toggleUnitsCheckbox.Name = "checkBox1";
            this.toggleUnitsCheckbox.Size = new Size(300, 0x11);
            this.toggleUnitsCheckbox.TabIndex = 3;
            this.toggleUnitsCheckbox.Text = "Toggle Coordinates as Pixel Units (PU)/Texture Units (TU)";
            this.toggleUnitsCheckbox.UseVisualStyleBackColor = true;
            this.toggleUnitsCheckbox.CheckStateChanged += new EventHandler(this.checkBox1_CheckStateChanged);
            this.heightLabel.AutoSize = true;
            this.heightLabel.Location = new Point(3, 7);
            this.heightLabel.Name = "label1";
            this.heightLabel.Size = new Size(70, 13);
            this.heightLabel.TabIndex = 4;
            this.heightLabel.Text = "Image Height";
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            //base.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            //base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            base.Controls.Add(this.heightLabel);
            base.Controls.Add(this.toggleUnitsCheckbox);
            base.Controls.Add(this.olv);
            base.Controls.Add(this.textBox);
            base.Name = "AtlasFileEditorControl";
            base.Size = new Size(0x364, 0x29d);
            ((ISupportInitialize) this.olv).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void olv_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            AspectPutterDelegate delegate2 = null;
            AspectPutterDelegate delegate3 = null;
            AspectPutterDelegate delegate4 = null;
            AspectPutterDelegate delegate5 = null;
            AspectPutterDelegate delegate6 = null;
            AspectPutterDelegate delegate7 = null;
            AspectPutterDelegate delegate8 = null;
            AspectPutterDelegate delegate9 = null;
            AspectPutterDelegate delegate10 = null;
            AspectPutterDelegate delegate11 = null;
            AspectPutterDelegate delegate12 = null;
            AspectPutterDelegate delegate13 = null;
            switch (e.Column.Index)
            {
                case 0:
                    if (delegate2 == null)
                    {
                        delegate2 = delegate (object x, object newValue) {
                            if (x is AtlasObject)
                            {
                                ((AtlasObject) x).Container1 = (string) newValue;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CSCont1.AspectPutter = delegate2;
                    break;

                case 1:
                    if (delegate3 == null)
                    {
                        delegate3 = delegate (object x, object newValue) {
                            if (x is AtlasObject)
                            {
                                ((AtlasObject) x).Container2 = (string) newValue;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CSCont2.AspectPutter = delegate3;
                    break;

                case 2:
                    if (delegate4 == null)
                    {
                        delegate4 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CX1))
                            {
                                ((AtlasObject) x).X1 = Convert.ToSingle(newValue);
                                EditedFile.setPixelUnits(this.imageHeight);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CX1.AspectPutter = delegate4;
                    break;

                case 3:
                    if (delegate5 == null)
                    {
                        delegate5 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CY1))
                            {
                                ((AtlasObject) x).Y1 = Convert.ToSingle(newValue);
                                EditedFile.setPixelUnits(this.imageHeight);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CY1.AspectPutter = delegate5;
                    break;

                case 4:
                    if (delegate6 == null)
                    {
                        delegate6 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CX2))
                            {
                                ((AtlasObject) x).X2 = Convert.ToSingle(newValue);
                                EditedFile.setPixelUnits(this.imageHeight);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CX2.AspectPutter = delegate6;
                    break;

                case 5:
                    if (delegate7 == null)
                    {
                        delegate7 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CY2))
                            {
                                ((AtlasObject) x).Y2 = Convert.ToSingle(newValue);
                                EditedFile.setPixelUnits(this.imageHeight);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CY2.AspectPutter = delegate7;
                    break;

                case 6:
                    if (delegate8 == null)
                    {
                        delegate8 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CX3))
                            {
                                ((AtlasObject) x).X3 = Convert.ToSingle(newValue);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CX3.AspectPutter = delegate8;
                    break;

                case 7:
                    if (delegate9 == null)
                    {
                        delegate9 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CY3))
                            {
                                ((AtlasObject) x).Y3 = Convert.ToSingle(newValue);
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CY3.AspectPutter = delegate9;
                    break;

                case 8:
                    if (delegate10 == null)
                    {
                        delegate10 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CX1P))
                            {
                                ((AtlasObject) x).PX1 = Convert.ToSingle(newValue);
                                ((AtlasObject) x).X1 = ((AtlasObject) x).PX1 / this.imageHeight;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CX1P.AspectPutter = delegate10;
                    break;

                case 9:
                    if (delegate11 == null)
                    {
                        delegate11 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CY1P))
                            {
                                ((AtlasObject) x).PY1 = Convert.ToSingle(newValue);
                                ((AtlasObject) x).Y1 = ((AtlasObject) x).PY1 / this.imageHeight;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CY1P.AspectPutter = delegate11;
                    break;

                case 10:
                    if (delegate12 == null)
                    {
                        delegate12 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CX2P))
                            {
                                ((AtlasObject) x).PX2 = Convert.ToSingle(newValue);
                                ((AtlasObject) x).X2 = ((AtlasObject) x).PX2 / this.imageHeight;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CX2P.AspectPutter = delegate12;
                    break;

                case 11:
                    if (delegate13 == null)
                    {
                        delegate13 = delegate (object x, object newValue) {
                            if ((x is AtlasObject) && (e.Column == this.CY2P))
                            {
                                ((AtlasObject) x).PY2 = Convert.ToSingle(newValue);
                                ((AtlasObject) x).Y2 = ((AtlasObject) x).PY2 / this.imageHeight;
                                this.DataChanged = true;
                            }
                        };
                    }
                    this.CY2P.AspectPutter = delegate13;
                    break;
            }
            this.Refresh();
        }

        private void olv_RightClick(object sender, CellRightClickEventArgs e)
        {
            if (e.Item != null)
            {
                e.Item.Selected = true;
                if (e.Model is AtlasObject)
                {
                    this.removeAtlasEntryToolStripMenuItem.Enabled = true;
                }
                else
                {
                    this.removeAtlasEntryToolStripMenuItem.Enabled = false;
                }
            }
            this.contextMenuStrip1.Show(this.olv, e.Location.X, e.Location.Y);
        }

        private void removeAtlasEntry_Click(object sender, EventArgs e)
        {
            AtlasObject selectedObject = (AtlasObject) this.olv.SelectedObject;
            this.EditedFile.removeAt(this.olv.IndexOf(this.olv.SelectedObject));
            this.olv.RemoveObject(selectedObject);
            this.olv.RefreshObjects(this.EditedFile.Entries);
            this.DataChanged = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.textBox.Modified && (e.KeyCode == Keys.Return))
            {
                this.imageHeight = Convert.ToSingle(this.textBox.Text);
                this.EditedFile.setPixelUnits(this.imageHeight);
                this.DataChanged = true;
                this.olv.RefreshObjects(this.EditedFile.Entries);
            }
        }

        private void textBox1_MouseHover(object sender, EventArgs e)
        {
            new ToolTip().SetToolTip(this.textBox, "Width of the .dds texture in Pixel Units");
        }
    }
}

