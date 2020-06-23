namespace DecodeTool {
    partial class DecodeTool {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip ();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.definitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.availableDefinitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.moreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.transformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.parseHereToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.parseFromStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.reapplyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.more5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.toggleEncodingStripMenuItem = new System.Windows.Forms.ToolStripMenuItem ();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer ();
            this.hexView = new System.Windows.Forms.RichTextBox ();
            this.navigationPanel = new System.Windows.Forms.Panel ();
            this.goProblem = new System.Windows.Forms.Button ();
            this.goStart = new System.Windows.Forms.Button ();
            this.forward = new System.Windows.Forms.Button ();
            this.back = new System.Windows.Forms.Button ();
            this.listSplitter = new System.Windows.Forms.SplitContainer ();
            this.typeList = new System.Windows.Forms.ListBox ();
            this.valueList = new System.Windows.Forms.ListBox ();
            this.typePreviewPanel = new System.Windows.Forms.Panel ();
            this.optstringLabel = new System.Windows.Forms.Label ();
            this.floatLabel = new System.Windows.Forms.Label ();
            this.boolLabel = new System.Windows.Forms.Label ();
            this.intLabel = new System.Windows.Forms.Label ();
            this.stringLabel = new System.Windows.Forms.Label ();
            this.setButton = new System.Windows.Forms.Button ();
            this.showTypes = new System.Windows.Forms.Button ();
            this.optStringType = new TypeSelection ();
            this.singleType = new TypeSelection ();
            this.boolType = new TypeSelection ();
            this.deleteButton = new System.Windows.Forms.Button ();
            this.nameButton = new System.Windows.Forms.Button ();
            this.intType = new TypeSelection ();
            this.stringType = new TypeSelection ();
            this.typeNameLabel = new System.Windows.Forms.Label ();
            this.headerControlPanel = new System.Windows.Forms.Panel ();
            this.setHeader = new System.Windows.Forms.Button ();
            this.headerLengthField = new System.Windows.Forms.TextBox ();
            this.headerLengthLabel = new System.Windows.Forms.Label ();
            this.repeatInfo = new System.Windows.Forms.Label ();
            this.byteLabel = new System.Windows.Forms.Label ();
            this.byteType = new TypeSelection ();
            this.menuStrip1.SuspendLayout ();
            //((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout ();
            this.splitContainer1.Panel2.SuspendLayout ();
            this.splitContainer1.SuspendLayout ();
            this.navigationPanel.SuspendLayout ();
            //((System.ComponentModel.ISupportInitialize)(this.listSplitter)).BeginInit();
            this.listSplitter.Panel1.SuspendLayout ();
            this.listSplitter.Panel2.SuspendLayout ();
            this.listSplitter.SuspendLayout ();
            this.typePreviewPanel.SuspendLayout ();
            this.headerControlPanel.SuspendLayout ();
            this.SuspendLayout ();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.definitionsToolStripMenuItem,
            this.moreToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point (0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size (634, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size (37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size (103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler (this.OpenEncodedFile);
            // 
            // definitionsToolStripMenuItem
            // 
            this.definitionsToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.availableDefinitionsToolStripMenuItem });
            this.definitionsToolStripMenuItem.Name = "definitionsToolStripMenuItem";
            this.definitionsToolStripMenuItem.Size = new System.Drawing.Size (76, 20);
            this.definitionsToolStripMenuItem.Text = "Definitions";
            // 
            // availableDefinitionsToolStripMenuItem
            // 
            this.availableDefinitionsToolStripMenuItem.Name = "availableDefinitionsToolStripMenuItem";
            this.availableDefinitionsToolStripMenuItem.Size = new System.Drawing.Size (76, 20);
            this.availableDefinitionsToolStripMenuItem.Text = "available";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size (100, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler (this.LoadSchemaFile);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size (100, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler (this.SaveSchemaFile);
            // 
            // moreToolStripMenuItem
            // 
            this.moreToolStripMenuItem.DropDownItems.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.transformToolStripMenuItem, this.parseHereToolStripMenuItem, this.parseFromStartToolStripMenuItem, 
                this.reapplyToolStripMenuItem, this.more5ToolStripMenuItem, toggleEncodingStripMenuItem  });
            this.moreToolStripMenuItem.Name = "moreToolStripMenuItem";
            this.moreToolStripMenuItem.Size = new System.Drawing.Size (47, 20);
            this.moreToolStripMenuItem.Text = "More";
            // 
            // more1ToolStripMenuItem
            // 
            this.transformToolStripMenuItem.Name = "more1ToolStripMenuItem";
            this.transformToolStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.transformToolStripMenuItem.Text = "Transform";
            // this.more1ToolStripMenuItem.Click += new System.EventHandler(this.ToggleBoolOptstring);
            // 
            // parseHereToolStripMenuItem
            // 
            this.parseHereToolStripMenuItem.Name = "parseHereToolStripMenuItem";
            this.parseHereToolStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.parseHereToolStripMenuItem.Text = "Parse from here";
            this.parseHereToolStripMenuItem.Visible = true;
            this.parseHereToolStripMenuItem.Enabled = false;
            this.parseHereToolStripMenuItem.Click += new System.EventHandler(ParseFromHere);
            // 
            // parseFromStartToolStripMenuItem
            // 
            this.parseFromStartToolStripMenuItem.Name = "parseFromStartToolStripMenuItem";
            this.parseFromStartToolStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.parseFromStartToolStripMenuItem.Text = "Parse from Start";
            this.parseFromStartToolStripMenuItem.Visible = true;
            this.parseFromStartToolStripMenuItem.Click += new System.EventHandler(ParseFromStart);
            // 
            // reapplyToolStripMenuItem
            // 
            this.reapplyToolStripMenuItem.Name = "reapplyToolStripMenuItem";
            this.reapplyToolStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.reapplyToolStripMenuItem.Text = "Add all again";
            this.reapplyToolStripMenuItem.Visible = true;
            this.reapplyToolStripMenuItem.Click += new System.EventHandler(this.ReapplyExisting);
            // 
            // more5ToolStripMenuItem
            // 
            this.more5ToolStripMenuItem.Name = "more5ToolStripMenuItem";
            this.more5ToolStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.more5ToolStripMenuItem.Text = "Bool <-> OptString";
            this.more5ToolStripMenuItem.Visible = false;
            //this.more5ToolStripMenuItem.Click += new System.EventHandler(this.ToggleBoolOptstring);
            // 
            this.toggleEncodingStripMenuItem.Name = "toggleEncodingStripMenuItem";
            this.toggleEncodingStripMenuItem.Size = new System.Drawing.Size (176, 22);
            this.toggleEncodingStripMenuItem.Text = "Toggle encoding";
            this.toggleEncodingStripMenuItem.Visible = true;
            this.toggleEncodingStripMenuItem.Click += toggleEncoding;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point (0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.headerControlPanel
            // 
            this.splitContainer1.Panel1.Controls.Add (this.hexView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add (this.navigationPanel);
            this.splitContainer1.Panel2.Controls.Add (this.listSplitter);
            this.splitContainer1.Panel2.Controls.Add (this.typePreviewPanel);
            this.splitContainer1.Panel2.Controls.Add (this.typeNameLabel);
            this.splitContainer1.Panel2.Controls.Add (this.headerControlPanel);
            this.splitContainer1.Panel2.Controls.Add (this.repeatInfo);
            this.splitContainer1.Size = new System.Drawing.Size (634, 627);
            this.splitContainer1.SplitterDistance = 210;
            this.splitContainer1.TabIndex = 1;
            // 
            // hexView
            // 
            this.hexView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView.Font = new System.Drawing.Font ("Courier", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexView.Location = new System.Drawing.Point (0, 0);
            this.hexView.Name = "hexView";
            this.hexView.Size = new System.Drawing.Size (210, 627);
            this.hexView.TabIndex = 0;
            this.hexView.Text = "";
            // 
            // navigationPanel
            // 
            this.navigationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.navigationPanel.Controls.Add (this.goProblem);
            this.navigationPanel.Controls.Add (this.goStart);
            this.navigationPanel.Controls.Add (this.forward);
            this.navigationPanel.Controls.Add (this.back);
            this.navigationPanel.Location = new System.Drawing.Point (7, 256);
            this.navigationPanel.Name = "panel3";
            this.navigationPanel.Size = new System.Drawing.Size (415, 26);
            this.navigationPanel.TabIndex = 16;
            // 
            // goProblem
            // 
            this.goProblem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.goProblem.Location = new System.Drawing.Point (246, 3);
            this.goProblem.Name = "goProblem";
            this.goProblem.Size = new System.Drawing.Size (75, 23);
            this.goProblem.TabIndex = 23;
            this.goProblem.Text = "problem";
            this.goProblem.UseVisualStyleBackColor = true;
            this.goProblem.Click += new System.EventHandler (this.goProblem_Click);
            // 
            // goStart
            // 
            this.goStart.Location = new System.Drawing.Point (3, 3);
            this.goStart.Name = "goStart";
            this.goStart.Size = new System.Drawing.Size (75, 23);
            this.goStart.TabIndex = 22;
            this.goStart.Text = "<<";
            this.goStart.UseVisualStyleBackColor = true;
            this.goStart.Click += new System.EventHandler (this.goStart_Click);
            // 
            // forward
            // 
            this.forward.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.forward.Location = new System.Drawing.Point (165, 3);
            this.forward.Name = "forward";
            this.forward.Size = new System.Drawing.Size (75, 23);
            this.forward.TabIndex = 21;
            this.forward.Text = ">";
            this.forward.UseVisualStyleBackColor = true;
            this.forward.Click += new System.EventHandler (this.forward_Click);
            // 
            // back
            // 
            this.back.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.back.Location = new System.Drawing.Point (84, 3);
            this.back.Name = "back";
            this.back.Size = new System.Drawing.Size (75, 23);
            this.back.TabIndex = 20;
            this.back.Text = "<";
            this.back.UseVisualStyleBackColor = true;
            this.back.Click += new System.EventHandler (this.back_Click);
            // 
            // listSplitter
            // 
            this.listSplitter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listSplitter.Location = new System.Drawing.Point (7, 4);
            this.listSplitter.Name = "listSplitter";
            // 
            // listSplitter.headerControlPanel
            // 
            this.listSplitter.Panel1.Controls.Add (this.typeList);
            // 
            // listSplitter.Panel2
            // 
            this.listSplitter.Panel2.Controls.Add (this.valueList);
            this.listSplitter.Size = new System.Drawing.Size (413, 246);
            this.listSplitter.SplitterDistance = 134;
            this.listSplitter.TabIndex = 15;
            // 
            // typeList
            // 
            this.typeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.typeList.FormattingEnabled = true;
            this.typeList.Location = new System.Drawing.Point (0, 0);
            this.typeList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.typeList.Name = "typeList";
            this.typeList.Size = new System.Drawing.Size (134, 246);
            this.typeList.TabIndex = 14;
            // 
            // valueList
            // 
            this.valueList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.valueList.FormattingEnabled = true;
            this.valueList.Location = new System.Drawing.Point (0, 0);
            this.valueList.Name = "valueList";
            this.valueList.Size = new System.Drawing.Size (275, 246);
            this.valueList.TabIndex = 13;
            this.valueList.SelectedIndexChanged += new System.EventHandler (this.valueList_SelectedIndexChanged);
            // 
            // typePreviewPanel
            // 
            this.typePreviewPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.typePreviewPanel.Controls.Add (this.byteLabel);
            this.typePreviewPanel.Controls.Add (this.byteType);
            this.typePreviewPanel.Controls.Add (this.optstringLabel);
            this.typePreviewPanel.Controls.Add (this.floatLabel);
            this.typePreviewPanel.Controls.Add (this.boolLabel);
            this.typePreviewPanel.Controls.Add (this.intLabel);
            this.typePreviewPanel.Controls.Add (this.stringLabel);
            this.typePreviewPanel.Controls.Add (this.setButton);
            this.typePreviewPanel.Controls.Add (this.showTypes);
            this.typePreviewPanel.Controls.Add (this.optStringType);
            this.typePreviewPanel.Controls.Add (this.singleType);
            this.typePreviewPanel.Controls.Add (this.boolType);
            this.typePreviewPanel.Controls.Add (this.deleteButton);
            this.typePreviewPanel.Controls.Add (this.nameButton);
            this.typePreviewPanel.Controls.Add (this.intType);
            this.typePreviewPanel.Controls.Add (this.stringType);
            this.typePreviewPanel.Location = new System.Drawing.Point (7, 288);
            this.typePreviewPanel.Name = "typePreviewPanel";
            this.typePreviewPanel.Size = new System.Drawing.Size (415, 250);
            this.typePreviewPanel.TabIndex = 14;
            // 
            // stringLabel
            // 
            this.stringLabel.AutoSize = true;
            this.stringLabel.Location = new System.Drawing.Point (2, 16);
            this.stringLabel.Name = "stringLabel";
            this.stringLabel.Size = new System.Drawing.Size (32, 13);
            this.stringLabel.TabIndex = 19;
            this.stringLabel.Text = "string";
            // 
            // setButton
            // 
            this.setButton.Location = new System.Drawing.Point (275, 224);
            this.setButton.Name = "setButton";
            this.setButton.Size = new System.Drawing.Size (75, 23);
            this.setButton.TabIndex = 18;
            this.setButton.Text = "Set";
            this.setButton.UseVisualStyleBackColor = true;
            this.setButton.Click += new System.EventHandler (this.setButton_Click);
            // 
            // showTypes
            // 
            this.showTypes.Location = new System.Drawing.Point (194, 224);
            this.showTypes.Name = "showTypes";
            this.showTypes.Size = new System.Drawing.Size (75, 23);
            this.showTypes.TabIndex = 17;
            this.showTypes.Text = "Show";
            this.showTypes.UseVisualStyleBackColor = true;
            this.showTypes.Click += new System.EventHandler (this.showTypes_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point (3, 224);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size (75, 23);
            this.deleteButton.TabIndex = 13;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler (this.DeleteType);
            // 
            // nameButton
            // 
            this.nameButton.Location = new System.Drawing.Point (90, 224);
            this.nameButton.Name = "nameButton";
            this.nameButton.Size = new System.Drawing.Size (75, 23);
            this.nameButton.TabIndex = 13;
            this.nameButton.Text = "Name";
            this.nameButton.UseVisualStyleBackColor = true;
            this.nameButton.Click += new System.EventHandler (this.NameType);
            // 
            // stringType
            // 
            this.stringType.Location = new System.Drawing.Point (55, 9);
            this.stringType.Name = "stringType";
            this.stringType.Size = new System.Drawing.Size (302, 31);
            this.stringType.TabIndex = 11;
            this.stringType.Type = null;
            // 
            // stringTypeAscii
            // 
//            this.stringType.Location = new System.Drawing.Point (55, 9);
//            this.stringType.Name = "stringTypeAscii";
//            this.stringType.Size = new System.Drawing.Size (302, 31);
//            this.stringType.TabIndex = 11;
//            this.stringType.Type = null;
            // 
            // intLabel
            // 
            this.intLabel.AutoSize = true;
            this.intLabel.Location = new System.Drawing.Point (2, 55);
            this.intLabel.Name = "intLabel";
            this.intLabel.Size = new System.Drawing.Size (18, 13);
            this.intLabel.TabIndex = 20;
            this.intLabel.Text = "int";
            // 
            // intType
            // 
            this.intType.Location = new System.Drawing.Point (55, 46);
            this.intType.Name = "intType";
            this.intType.Size = new System.Drawing.Size (302, 33);
            this.intType.TabIndex = 12;
            this.intType.Type = null;
            // 
            // boolLabel
            // 
            this.boolLabel.AutoSize = true;
            this.boolLabel.Location = new System.Drawing.Point (2, 89);
            this.boolLabel.Name = "boolLabel";
            this.boolLabel.Size = new System.Drawing.Size (27, 13);
            this.boolLabel.TabIndex = 21;
            this.boolLabel.Text = "bool";
            // 
            // boolType
            // 
            this.boolType.Location = new System.Drawing.Point (55, 84);
            this.boolType.Name = "boolType";
            this.boolType.Size = new System.Drawing.Size (302, 28);
            this.boolType.TabIndex = 14;
            this.boolType.Type = null;
            // 
            // floatLabel
            // 
            this.floatLabel.AutoSize = true;
            this.floatLabel.Location = new System.Drawing.Point (2, 127);
            this.floatLabel.Name = "floatLabel";
            this.floatLabel.Size = new System.Drawing.Size (27, 13);
            this.floatLabel.TabIndex = 22;
            this.floatLabel.Text = "float";
            // 
            // singleType
            // 
            this.singleType.Location = new System.Drawing.Point (55, 118);
            this.singleType.Name = "singleType";
            this.singleType.Size = new System.Drawing.Size (302, 31);
            this.singleType.TabIndex = 15;
            this.singleType.Type = null;
            // 
            // optstringLabel
            // 
            this.optstringLabel.AutoSize = true;
            this.optstringLabel.Location = new System.Drawing.Point (2, 159);
            this.optstringLabel.Name = "optstringLabel";
            this.optstringLabel.Size = new System.Drawing.Size (47, 13);
            this.optstringLabel.TabIndex = 23;
            this.optstringLabel.Text = "optstring";
            // 
            // optStringType
            // 
            this.optStringType.Location = new System.Drawing.Point (55, 150);
            this.optStringType.Name = "optStringType";
            this.optStringType.Size = new System.Drawing.Size (302, 31);
            this.optStringType.TabIndex = 16;
            this.optStringType.Type = null;
            // 
            // byteLabel
            // 
            this.byteLabel.AutoSize = true;
            this.byteLabel.Location = new System.Drawing.Point (2, 196);
            this.byteLabel.Name = "byteLabel";
            this.byteLabel.Size = new System.Drawing.Size (27, 13);
            this.byteLabel.TabIndex = 25;
            this.byteLabel.Text = "byte";
            // 
            // byteType
            // 
            this.byteType.Location = new System.Drawing.Point (55, 187);
            this.byteType.Name = "byteType";
            this.byteType.Size = new System.Drawing.Size (302, 31);
            this.byteType.TabIndex = 24;
            this.byteType.Type = null;
            // 
            // typeNameLabel
            // 
            this.typeNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.typeNameLabel.AutoSize = true;
            this.typeNameLabel.Location = new System.Drawing.Point (7, 541);
            this.typeNameLabel.Name = "typeNameLabel";
            this.typeNameLabel.Size = new System.Drawing.Size (60, 13);
            this.typeNameLabel.TabIndex = 13;
            this.typeNameLabel.Text = "Typename:";
            // 
            // headerControlPanel
            // 
            this.headerControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerControlPanel.Controls.Add (this.setHeader);
            this.headerControlPanel.Controls.Add (this.headerLengthField);
            this.headerControlPanel.Controls.Add (this.headerLengthLabel);
            this.headerControlPanel.Location = new System.Drawing.Point (3, 582);
            this.headerControlPanel.Name = "headerControlPanel";
            this.headerControlPanel.Size = new System.Drawing.Size (414, 42);
            this.headerControlPanel.TabIndex = 12;
            // 
            // setHeader
            // 
            this.setHeader.Location = new System.Drawing.Point (191, 10);
            this.setHeader.Name = "setHeader";
            this.setHeader.Size = new System.Drawing.Size (162, 23);
            this.setHeader.TabIndex = 8;
            this.setHeader.Text = "Set";
            this.setHeader.UseVisualStyleBackColor = true;
            this.setHeader.Click += new System.EventHandler (this.setHeaderLength_Click);
            // 
            // headerLengthField
            // 
            this.headerLengthField.Location = new System.Drawing.Point (85, 12);
            this.headerLengthField.Name = "headerLengthField";
            this.headerLengthField.Size = new System.Drawing.Size (100, 20);
            this.headerLengthField.TabIndex = 7;
            // 
            // headerLengthLabel
            // 
            this.headerLengthLabel.AutoSize = true;
            this.headerLengthLabel.Location = new System.Drawing.Point (7, 15);
            this.headerLengthLabel.Name = "headerLengthLabel";
            this.headerLengthLabel.Size = new System.Drawing.Size (72, 13);
            this.headerLengthLabel.TabIndex = 6;
            this.headerLengthLabel.Text = "header length";
            // 
            // repeatInfo
            // 
            this.repeatInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.repeatInfo.AutoSize = true;
            this.repeatInfo.Location = new System.Drawing.Point (4, 566);
            this.repeatInfo.Name = "repeatInfo";
            this.repeatInfo.Size = new System.Drawing.Size (68, 13);
            this.repeatInfo.TabIndex = 9;
            this.repeatInfo.Text = "select data...";
            // 
            // DecodeTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size (634, 651);
            this.Controls.Add (this.splitContainer1);
            this.Controls.Add (this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DecodeTool";
            this.Text = "HexView";
            this.menuStrip1.ResumeLayout (false);
            this.menuStrip1.PerformLayout ();
            this.splitContainer1.Panel1.ResumeLayout (false);
            this.splitContainer1.Panel2.ResumeLayout (false);
            this.splitContainer1.Panel2.PerformLayout ();
            //((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout (false);
            this.navigationPanel.ResumeLayout (false);
            this.listSplitter.Panel1.ResumeLayout (false);
            this.listSplitter.Panel2.ResumeLayout (false);
            //((System.ComponentModel.ISupportInitialize)(this.listSplitter)).EndInit();
            this.listSplitter.ResumeLayout (false);
            this.typePreviewPanel.ResumeLayout (false);
            this.typePreviewPanel.PerformLayout ();
            this.headerControlPanel.ResumeLayout (false);
            this.headerControlPanel.PerformLayout ();
            this.ResumeLayout (false);
            this.PerformLayout ();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox hexView;
        private System.Windows.Forms.Label repeatInfo;
        private System.Windows.Forms.Panel headerControlPanel;
        private System.Windows.Forms.Button setHeader;
        private System.Windows.Forms.TextBox headerLengthField;
        private System.Windows.Forms.Label headerLengthLabel;
        private System.Windows.Forms.ToolStripMenuItem definitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem availableDefinitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.Label typeNameLabel;
        private System.Windows.Forms.Panel typePreviewPanel;
        private TypeSelection optStringType;
        private TypeSelection singleType;
        private TypeSelection boolType;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button nameButton;
        private TypeSelection intType;
        private TypeSelection stringType;
        private System.Windows.Forms.SplitContainer listSplitter;
        private System.Windows.Forms.ListBox typeList;
        private System.Windows.Forms.ListBox valueList;
        private System.Windows.Forms.Button showTypes;
        private System.Windows.Forms.ToolStripMenuItem moreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transformToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parseHereToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parseFromStartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reapplyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem more5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleEncodingStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Button setButton;
        private System.Windows.Forms.Label stringLabel;
        private System.Windows.Forms.Label intLabel;
        private System.Windows.Forms.Label optstringLabel;
        private System.Windows.Forms.Label floatLabel;
        private System.Windows.Forms.Label boolLabel;
        private System.Windows.Forms.Panel navigationPanel;
        private System.Windows.Forms.Button goProblem;
        private System.Windows.Forms.Button goStart;
        private System.Windows.Forms.Button forward;
        private System.Windows.Forms.Button back;
        private System.Windows.Forms.Label byteLabel;
        private TypeSelection byteType;

    }
}

