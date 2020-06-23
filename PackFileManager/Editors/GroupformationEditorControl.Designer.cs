namespace PackFileManager {
    partial class GroupformationEditorControl {
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.infoGroup = new System.Windows.Forms.GroupBox();
            this.purposeComboBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.priorityInput = new System.Windows.Forms.TextBox();
            this.nameInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.factionList = new System.Windows.Forms.ListBox();
            this.lineGroup = new System.Windows.Forms.GroupBox();
            this.addSpanButton = new System.Windows.Forms.Button();
            this.deleteLineButton = new System.Windows.Forms.Button();
            this.typeGroup = new System.Windows.Forms.GroupBox();
            this.spanningRadioButton = new System.Windows.Forms.RadioButton();
            this.relativeRadioButton = new System.Windows.Forms.RadioButton();
            this.absoluteButton = new System.Windows.Forms.RadioButton();
            this.addLineButton = new System.Windows.Forms.Button();
            this.shapeGroup = new System.Windows.Forms.GroupBox();
            this.crescFrontRadioButton = new System.Windows.Forms.RadioButton();
            this.lineRadioButton = new System.Windows.Forms.RadioButton();
            this.columnRadioButton = new System.Windows.Forms.RadioButton();
            this.crescBackRadioButton = new System.Windows.Forms.RadioButton();
            this.relativeToInput = new System.Windows.Forms.TextBox();
            this.relativeToLabel = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.previewButton = new System.Windows.Forms.Button();
            this.editUnitPriorityButton = new System.Windows.Forms.Button();
            this.deleteUnitPriorityButton = new System.Windows.Forms.Button();
            this.addUnitPriorityButton = new System.Windows.Forms.Button();
            this.unitPriorityList = new System.Windows.Forms.ListBox();
            this.maxThresholdInput = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.minThresholdInput = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.yInput = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.xInput = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.crescOffsetInput = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.spacingInput = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.linePriorityInput = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.linesList = new System.Windows.Forms.ListBox();
            this.formationPreview = new PackFileManager.FormationPreview();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.infoGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.lineGroup.SuspendLayout();
            this.typeGroup.SuspendLayout();
            this.shapeGroup.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.lineGroup);
            this.splitContainer1.Panel1MinSize = 500;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.formationPreview);
            this.splitContainer1.Size = new System.Drawing.Size(961, 596);
            this.splitContainer1.SplitterDistance = 624;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 25;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.infoGroup);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(624, 157);
            this.panel1.TabIndex = 32;
            // 
            // infoGroup
            // 
            this.infoGroup.AutoSize = true;
            this.infoGroup.Controls.Add(this.purposeComboBox);
            this.infoGroup.Controls.Add(this.label3);
            this.infoGroup.Controls.Add(this.priorityInput);
            this.infoGroup.Controls.Add(this.nameInput);
            this.infoGroup.Controls.Add(this.label2);
            this.infoGroup.Controls.Add(this.label1);
            this.infoGroup.Dock = System.Windows.Forms.DockStyle.Left;
            this.infoGroup.Location = new System.Drawing.Point(0, 0);
            this.infoGroup.MinimumSize = new System.Drawing.Size(250, 0);
            this.infoGroup.Name = "infoGroup";
            this.infoGroup.Size = new System.Drawing.Size(250, 157);
            this.infoGroup.TabIndex = 34;
            this.infoGroup.TabStop = false;
            this.infoGroup.Text = "General";
            // 
            // purposeComboBox
            // 
            this.purposeComboBox.Location = new System.Drawing.Point(67, 90);
            this.purposeComboBox.Name = "purposeComboBox";
            this.purposeComboBox.Size = new System.Drawing.Size(177, 20);
            this.purposeComboBox.TabIndex = 35;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 34;
            this.label3.Text = "Purpose:";
            // 
            // priorityInput
            // 
            this.priorityInput.Location = new System.Drawing.Point(67, 52);
            this.priorityInput.Name = "priorityInput";
            this.priorityInput.Size = new System.Drawing.Size(177, 20);
            this.priorityInput.TabIndex = 33;
            // 
            // nameInput
            // 
            this.nameInput.Location = new System.Drawing.Point(67, 17);
            this.nameInput.Name = "nameInput";
            this.nameInput.Size = new System.Drawing.Size(177, 20);
            this.nameInput.TabIndex = 31;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 32;
            this.label2.Text = "Priority:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Name:";
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.factionList);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Location = new System.Drawing.Point(374, 0);
            this.groupBox1.MinimumSize = new System.Drawing.Size(250, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(250, 157);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Factions";
            // 
            // factionList
            // 
            this.factionList.Dock = System.Windows.Forms.DockStyle.Left;
            this.factionList.FormattingEnabled = true;
            this.factionList.Location = new System.Drawing.Point(3, 16);
            this.factionList.Name = "factionList";
            this.factionList.Size = new System.Drawing.Size(168, 138);
            this.factionList.TabIndex = 0;
            // 
            // lineGroup
            // 
            this.lineGroup.Controls.Add(this.addSpanButton);
            this.lineGroup.Controls.Add(this.deleteLineButton);
            this.lineGroup.Controls.Add(this.typeGroup);
            this.lineGroup.Controls.Add(this.addLineButton);
            this.lineGroup.Controls.Add(this.shapeGroup);
            this.lineGroup.Controls.Add(this.relativeToInput);
            this.lineGroup.Controls.Add(this.relativeToLabel);
            this.lineGroup.Controls.Add(this.groupBox2);
            this.lineGroup.Controls.Add(this.maxThresholdInput);
            this.lineGroup.Controls.Add(this.label11);
            this.lineGroup.Controls.Add(this.minThresholdInput);
            this.lineGroup.Controls.Add(this.label12);
            this.lineGroup.Controls.Add(this.yInput);
            this.lineGroup.Controls.Add(this.label9);
            this.lineGroup.Controls.Add(this.xInput);
            this.lineGroup.Controls.Add(this.label10);
            this.lineGroup.Controls.Add(this.crescOffsetInput);
            this.lineGroup.Controls.Add(this.label8);
            this.lineGroup.Controls.Add(this.spacingInput);
            this.lineGroup.Controls.Add(this.label7);
            this.lineGroup.Controls.Add(this.linePriorityInput);
            this.lineGroup.Controls.Add(this.label5);
            this.lineGroup.Controls.Add(this.linesList);
            this.lineGroup.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lineGroup.Location = new System.Drawing.Point(0, 176);
            this.lineGroup.MinimumSize = new System.Drawing.Size(500, 420);
            this.lineGroup.Name = "lineGroup";
            this.lineGroup.Size = new System.Drawing.Size(624, 420);
            this.lineGroup.TabIndex = 31;
            this.lineGroup.TabStop = false;
            this.lineGroup.Text = "Lines";
            // 
            // addSpanButton
            // 
            this.addSpanButton.Location = new System.Drawing.Point(84, 199);
            this.addSpanButton.MinimumSize = new System.Drawing.Size(50, 0);
            this.addSpanButton.Name = "addSpanButton";
            this.addSpanButton.Size = new System.Drawing.Size(68, 23);
            this.addSpanButton.TabIndex = 29;
            this.addSpanButton.Text = "Add Span";
            this.addSpanButton.UseVisualStyleBackColor = true;
            this.addSpanButton.Click += new System.EventHandler(this.addSpanButton_Click);
            // 
            // deleteLineButton
            // 
            this.deleteLineButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deleteLineButton.Enabled = false;
            this.deleteLineButton.Location = new System.Drawing.Point(158, 199);
            this.deleteLineButton.MinimumSize = new System.Drawing.Size(50, 0);
            this.deleteLineButton.Name = "deleteLineButton";
            this.deleteLineButton.Size = new System.Drawing.Size(66, 23);
            this.deleteLineButton.TabIndex = 2;
            this.deleteLineButton.Text = "Delete";
            this.deleteLineButton.UseVisualStyleBackColor = true;
            this.deleteLineButton.Click += new System.EventHandler(this.deleteLineButton_Click);
            // 
            // typeGroup
            // 
            this.typeGroup.Controls.Add(this.spanningRadioButton);
            this.typeGroup.Controls.Add(this.relativeRadioButton);
            this.typeGroup.Controls.Add(this.absoluteButton);
            this.typeGroup.Location = new System.Drawing.Point(233, 20);
            this.typeGroup.Name = "typeGroup";
            this.typeGroup.Size = new System.Drawing.Size(384, 55);
            this.typeGroup.TabIndex = 28;
            this.typeGroup.TabStop = false;
            this.typeGroup.Text = "Type";
            // 
            // spanningRadioButton
            // 
            this.spanningRadioButton.AutoSize = true;
            this.spanningRadioButton.Enabled = false;
            this.spanningRadioButton.Location = new System.Drawing.Point(284, 19);
            this.spanningRadioButton.Name = "spanningRadioButton";
            this.spanningRadioButton.Size = new System.Drawing.Size(68, 17);
            this.spanningRadioButton.TabIndex = 4;
            this.spanningRadioButton.TabStop = true;
            this.spanningRadioButton.Text = "spanning";
            this.spanningRadioButton.UseVisualStyleBackColor = true;
            // 
            // relativeRadioButton
            // 
            this.relativeRadioButton.AutoSize = true;
            this.relativeRadioButton.Enabled = false;
            this.relativeRadioButton.Location = new System.Drawing.Point(144, 19);
            this.relativeRadioButton.Name = "relativeRadioButton";
            this.relativeRadioButton.Size = new System.Drawing.Size(59, 17);
            this.relativeRadioButton.TabIndex = 3;
            this.relativeRadioButton.TabStop = true;
            this.relativeRadioButton.Text = "relative";
            this.relativeRadioButton.UseVisualStyleBackColor = true;
            // 
            // absoluteButton
            // 
            this.absoluteButton.AutoSize = true;
            this.absoluteButton.Enabled = false;
            this.absoluteButton.Location = new System.Drawing.Point(6, 19);
            this.absoluteButton.Name = "absoluteButton";
            this.absoluteButton.Size = new System.Drawing.Size(65, 17);
            this.absoluteButton.TabIndex = 2;
            this.absoluteButton.TabStop = true;
            this.absoluteButton.Text = "absolute";
            this.absoluteButton.UseVisualStyleBackColor = true;
            // 
            // addLineButton
            // 
            this.addLineButton.Location = new System.Drawing.Point(3, 199);
            this.addLineButton.MinimumSize = new System.Drawing.Size(50, 0);
            this.addLineButton.Name = "addLineButton";
            this.addLineButton.Size = new System.Drawing.Size(75, 23);
            this.addLineButton.TabIndex = 1;
            this.addLineButton.Text = "Add Line";
            this.addLineButton.UseVisualStyleBackColor = true;
            this.addLineButton.Click += new System.EventHandler(this.addLineButton_Click);
            // 
            // shapeGroup
            // 
            this.shapeGroup.Controls.Add(this.crescFrontRadioButton);
            this.shapeGroup.Controls.Add(this.lineRadioButton);
            this.shapeGroup.Controls.Add(this.columnRadioButton);
            this.shapeGroup.Controls.Add(this.crescBackRadioButton);
            this.shapeGroup.Location = new System.Drawing.Point(230, 107);
            this.shapeGroup.Name = "shapeGroup";
            this.shapeGroup.Size = new System.Drawing.Size(382, 45);
            this.shapeGroup.TabIndex = 27;
            this.shapeGroup.TabStop = false;
            this.shapeGroup.Text = "Shape";
            // 
            // crescFrontRadioButton
            // 
            this.crescFrontRadioButton.AutoSize = true;
            this.crescFrontRadioButton.Enabled = false;
            this.crescFrontRadioButton.Location = new System.Drawing.Point(195, 19);
            this.crescFrontRadioButton.Name = "crescFrontRadioButton";
            this.crescFrontRadioButton.Size = new System.Drawing.Size(75, 17);
            this.crescFrontRadioButton.TabIndex = 10;
            this.crescFrontRadioButton.TabStop = true;
            this.crescFrontRadioButton.Text = "cresc front";
            this.crescFrontRadioButton.UseVisualStyleBackColor = true;
            // 
            // lineRadioButton
            // 
            this.lineRadioButton.AutoSize = true;
            this.lineRadioButton.Enabled = false;
            this.lineRadioButton.Location = new System.Drawing.Point(6, 19);
            this.lineRadioButton.Name = "lineRadioButton";
            this.lineRadioButton.Size = new System.Drawing.Size(41, 17);
            this.lineRadioButton.TabIndex = 8;
            this.lineRadioButton.TabStop = true;
            this.lineRadioButton.Text = "line";
            this.lineRadioButton.UseVisualStyleBackColor = true;
            // 
            // columnRadioButton
            // 
            this.columnRadioButton.AutoSize = true;
            this.columnRadioButton.Enabled = false;
            this.columnRadioButton.Location = new System.Drawing.Point(81, 19);
            this.columnRadioButton.Name = "columnRadioButton";
            this.columnRadioButton.Size = new System.Drawing.Size(59, 17);
            this.columnRadioButton.TabIndex = 9;
            this.columnRadioButton.TabStop = true;
            this.columnRadioButton.Text = "column";
            this.columnRadioButton.UseVisualStyleBackColor = true;
            // 
            // crescBackRadioButton
            // 
            this.crescBackRadioButton.AutoSize = true;
            this.crescBackRadioButton.Enabled = false;
            this.crescBackRadioButton.Location = new System.Drawing.Point(287, 19);
            this.crescBackRadioButton.Name = "crescBackRadioButton";
            this.crescBackRadioButton.Size = new System.Drawing.Size(78, 17);
            this.crescBackRadioButton.TabIndex = 11;
            this.crescBackRadioButton.TabStop = true;
            this.crescBackRadioButton.Text = "cresc back";
            this.crescBackRadioButton.UseVisualStyleBackColor = true;
            // 
            // relativeToInput
            // 
            this.relativeToInput.Location = new System.Drawing.Point(517, 81);
            this.relativeToInput.Name = "relativeToInput";
            this.relativeToInput.Size = new System.Drawing.Size(100, 20);
            this.relativeToInput.TabIndex = 26;
            // 
            // relativeToLabel
            // 
            this.relativeToLabel.AutoSize = true;
            this.relativeToLabel.Location = new System.Drawing.Point(422, 84);
            this.relativeToLabel.Name = "relativeToLabel";
            this.relativeToLabel.Size = new System.Drawing.Size(57, 13);
            this.relativeToLabel.TabIndex = 25;
            this.relativeToLabel.Text = "relative To";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.previewButton);
            this.groupBox2.Controls.Add(this.editUnitPriorityButton);
            this.groupBox2.Controls.Add(this.deleteUnitPriorityButton);
            this.groupBox2.Controls.Add(this.addUnitPriorityButton);
            this.groupBox2.Controls.Add(this.unitPriorityList);
            this.groupBox2.Location = new System.Drawing.Point(0, 254);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(618, 160);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Unit Priorities";
            // 
            // previewButton
            // 
            this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.previewButton.AutoSize = true;
            this.previewButton.Location = new System.Drawing.Point(361, 125);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(115, 23);
            this.previewButton.TabIndex = 4;
            this.previewButton.Text = "Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            // 
            // editUnitPriorityButton
            // 
            this.editUnitPriorityButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.editUnitPriorityButton.AutoSize = true;
            this.editUnitPriorityButton.Enabled = false;
            this.editUnitPriorityButton.Location = new System.Drawing.Point(360, 76);
            this.editUnitPriorityButton.Name = "editUnitPriorityButton";
            this.editUnitPriorityButton.Size = new System.Drawing.Size(116, 23);
            this.editUnitPriorityButton.TabIndex = 3;
            this.editUnitPriorityButton.Text = "Edit";
            this.editUnitPriorityButton.UseVisualStyleBackColor = true;
            this.editUnitPriorityButton.Click += new System.EventHandler(this.editUnitPriorityButton_Click);
            // 
            // deleteUnitPriorityButton
            // 
            this.deleteUnitPriorityButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deleteUnitPriorityButton.AutoSize = true;
            this.deleteUnitPriorityButton.Enabled = false;
            this.deleteUnitPriorityButton.Location = new System.Drawing.Point(360, 46);
            this.deleteUnitPriorityButton.Name = "deleteUnitPriorityButton";
            this.deleteUnitPriorityButton.Size = new System.Drawing.Size(116, 23);
            this.deleteUnitPriorityButton.TabIndex = 2;
            this.deleteUnitPriorityButton.Text = "Delete";
            this.deleteUnitPriorityButton.UseVisualStyleBackColor = true;
            this.deleteUnitPriorityButton.Click += new System.EventHandler(this.deleteUnitPriorityButton_Click);
            // 
            // addUnitPriorityButton
            // 
            this.addUnitPriorityButton.AutoSize = true;
            this.addUnitPriorityButton.Enabled = false;
            this.addUnitPriorityButton.Location = new System.Drawing.Point(360, 16);
            this.addUnitPriorityButton.Name = "addUnitPriorityButton";
            this.addUnitPriorityButton.Size = new System.Drawing.Size(116, 23);
            this.addUnitPriorityButton.TabIndex = 1;
            this.addUnitPriorityButton.Text = "Add";
            this.addUnitPriorityButton.UseVisualStyleBackColor = true;
            this.addUnitPriorityButton.Click += new System.EventHandler(this.addUnitPriorityButton_Click);
            // 
            // unitPriorityList
            // 
            this.unitPriorityList.Dock = System.Windows.Forms.DockStyle.Left;
            this.unitPriorityList.FormattingEnabled = true;
            this.unitPriorityList.Location = new System.Drawing.Point(3, 16);
            this.unitPriorityList.Name = "unitPriorityList";
            this.unitPriorityList.Size = new System.Drawing.Size(351, 141);
            this.unitPriorityList.TabIndex = 0;
            // 
            // maxThresholdInput
            // 
            this.maxThresholdInput.Location = new System.Drawing.Point(517, 228);
            this.maxThresholdInput.Name = "maxThresholdInput";
            this.maxThresholdInput.Size = new System.Drawing.Size(100, 20);
            this.maxThresholdInput.TabIndex = 23;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(422, 231);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "Max Threshold";
            // 
            // minThresholdInput
            // 
            this.minThresholdInput.Location = new System.Drawing.Point(311, 228);
            this.minThresholdInput.Name = "minThresholdInput";
            this.minThresholdInput.Size = new System.Drawing.Size(100, 20);
            this.minThresholdInput.TabIndex = 21;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(230, 231);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(74, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "Min Threshold";
            // 
            // yInput
            // 
            this.yInput.Location = new System.Drawing.Point(517, 193);
            this.yInput.Name = "yInput";
            this.yInput.Size = new System.Drawing.Size(100, 20);
            this.yInput.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(422, 196);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(14, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Y";
            // 
            // xInput
            // 
            this.xInput.Location = new System.Drawing.Point(311, 193);
            this.xInput.Name = "xInput";
            this.xInput.Size = new System.Drawing.Size(100, 20);
            this.xInput.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(230, 196);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(14, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "X";
            // 
            // crescOffsetInput
            // 
            this.crescOffsetInput.Location = new System.Drawing.Point(517, 158);
            this.crescOffsetInput.Name = "crescOffsetInput";
            this.crescOffsetInput.Size = new System.Drawing.Size(100, 20);
            this.crescOffsetInput.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(422, 161);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Cresc Offset";
            // 
            // spacingInput
            // 
            this.spacingInput.Location = new System.Drawing.Point(311, 158);
            this.spacingInput.Name = "spacingInput";
            this.spacingInput.Size = new System.Drawing.Size(100, 20);
            this.spacingInput.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(230, 161);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Spacing";
            // 
            // linePriorityInput
            // 
            this.linePriorityInput.Location = new System.Drawing.Point(311, 81);
            this.linePriorityInput.Name = "linePriorityInput";
            this.linePriorityInput.Size = new System.Drawing.Size(100, 20);
            this.linePriorityInput.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(230, 84);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Priority:";
            // 
            // linesList
            // 
            this.linesList.FormattingEnabled = true;
            this.linesList.Location = new System.Drawing.Point(4, 20);
            this.linesList.Name = "linesList";
            this.linesList.Size = new System.Drawing.Size(220, 173);
            this.linesList.TabIndex = 0;
            // 
            // formationPreview
            // 
            this.formationPreview.AutoScroll = true;
            this.formationPreview.AutoScrollMinSize = new System.Drawing.Size(200, 0);
            this.formationPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.formationPreview.Formation = null;
            this.formationPreview.Location = new System.Drawing.Point(0, 0);
            this.formationPreview.MinimumSize = new System.Drawing.Size(300, 0);
            this.formationPreview.Name = "formationPreview";
            this.formationPreview.SelectedLine = null;
            this.formationPreview.Size = new System.Drawing.Size(327, 596);
            this.formationPreview.TabIndex = 3;
            // 
            // GroupformationEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "GroupformationEditorControl";
            this.Size = new System.Drawing.Size(961, 596);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.infoGroup.ResumeLayout(false);
            this.infoGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.lineGroup.ResumeLayout(false);
            this.lineGroup.PerformLayout();
            this.typeGroup.ResumeLayout(false);
            this.typeGroup.PerformLayout();
            this.shapeGroup.ResumeLayout(false);
            this.shapeGroup.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox lineGroup;
        private System.Windows.Forms.TextBox relativeToInput;
        private System.Windows.Forms.Label relativeToLabel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.Button editUnitPriorityButton;
        private System.Windows.Forms.Button deleteUnitPriorityButton;
        private System.Windows.Forms.Button addUnitPriorityButton;
        private System.Windows.Forms.ListBox unitPriorityList;
        private System.Windows.Forms.TextBox maxThresholdInput;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox minThresholdInput;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox yInput;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox xInput;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox crescOffsetInput;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox spacingInput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton crescBackRadioButton;
        private System.Windows.Forms.RadioButton crescFrontRadioButton;
        private System.Windows.Forms.RadioButton columnRadioButton;
        private System.Windows.Forms.RadioButton lineRadioButton;
        private System.Windows.Forms.TextBox linePriorityInput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton spanningRadioButton;
        private System.Windows.Forms.RadioButton relativeRadioButton;
        private System.Windows.Forms.RadioButton absoluteButton;
        private System.Windows.Forms.ListBox linesList;
        private FormationPreview formationPreview;
        private System.Windows.Forms.GroupBox typeGroup;
        private System.Windows.Forms.GroupBox shapeGroup;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox infoGroup;
        private System.Windows.Forms.TextBox purposeComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox priorityInput;
        private System.Windows.Forms.TextBox nameInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button deleteLineButton;
        private System.Windows.Forms.Button addLineButton;
        private System.Windows.Forms.ListBox factionList;
        private System.Windows.Forms.Button addSpanButton;

        //private PackFileManager.FormationPreview formationPreview;
    }
}
