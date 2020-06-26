using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using Common;
using CommonDialogs;
using Filetypes;

using TableRow = System.Collections.Generic.List<Filetypes.FieldInstance>;
using Filetypes.Codecs;

namespace DecodeTool {
    public partial class DecodeTool : Form {
        private bool unicode;
        public DecodeTool (bool encodeUnicode = false) {
            InitializeComponent ();
            
            unicode = encodeUnicode;
            
            #region Type Selection Listener Initialization
            /* Add to the TypeSelection listeners. */
            if (unicode) {
                stringType.Factory = Types.StringType;
                optStringType.Factory = Types.OptStringType;
            } else {
                stringType.Factory = Types.StringTypeAscii;
                optStringType.Factory = Types.OptStringTypeAscii;
            }
            stringType.Selected += AddType;

            intType.Factory = Types.IntType;
            intType.Selected += AddType;

            boolType.Factory = Types.BoolType;
            boolType.Selected += AddType;

            singleType.Factory = Types.SingleType;
            singleType.Selected += AddType;
   
            optStringType.Selected += AddType;

            byteType.Factory = Types.ByteType;
            byteType.Selected += AddType;
            #endregion
   
            /* Clear selection in list upon right-click. */
            typeList.MouseUp += new MouseEventHandler (ClearSelection);
            valueList.MouseUp += new MouseEventHandler (ClearSelection);
            
            /* Add a menu item for each available type transformation. */
            foreach (ITransform transform in Transformations.TRANSFORMS) {
                ToolStripMenuItem transformItem = new ToolStripMenuItem (transform.Name) {
                    Tag = transform,
                    Enabled = false
                };
                transformItem.Click += new EventHandler (TransformSelection);
                this.transformToolStripMenuItem.DropDownItems.Add (transformItem);
            }
            /* Enable/disable them depending on selection in type list. */
            typeList.SelectedValueChanged += new EventHandler (EnableTransforms);
            valueList.SelectedValueChanged += new EventHandler (MarkSelectedValue);
        }

        /* The data currently parsed by this decode tool. */
        byte[] bytes;
        public byte[] Bytes {
			set {
                bytes = value;
                this.parseHereToolStripMenuItem.Enabled = value != null;
                ParseHeader();
                ParseData();
                
                availableDefinitionsToolStripMenuItem.DropDownItems.Clear();
                foreach(TypeInfo info in DBTypeMap.Instance.GetAllInfos(currentTypeInfo.Name)) {
                    ToolStripMenuItem item = new ToolStripMenuItem(String.Format("Version {0}", info.Version)) {
                        Tag = info,
                        CheckOnClick = true
                    };
                    item.CheckedChanged += availableSelected;
                    availableDefinitionsToolStripMenuItem.DropDownItems.Add(item);
                }
			}
			get {
				return bytes;
			}
		}
        
        void availableSelected(object sender, EventArgs args) {
            TypeInfo info = null;
            foreach(ToolStripMenuItem item in availableDefinitionsToolStripMenuItem.DropDownItems) {
                if (item == sender) {
                    info = item.Tag as TypeInfo;
                } else {
                    item.Checked = false;
                }
            }
            if (info != null) {
                CurrentTypeInfo = info;
            }
        }
        
        #region Data Header
        /* The header within the data (excluded from parsing of contained values). */
        int headerLength;
        public int HeaderLength {
            get {
                return headerLength;
            }
            set {
                headerLength = value;
                headerLengthField.Text = headerLength.ToString();
                ParseData();
            }
        }
        /* The number of entries expected to be in the data (determined from header). */
        uint expectedEntries = 0;
        uint ExpectedEntries {
            get {
                return expectedEntries;
            }
            set {
                expectedEntries = value;
            }
        }
        #endregion
        
        #region Type Info
        /* The type info for the current data set. Setting this will reparse data. */
        TypeInfo currentTypeInfo = new TypeInfo {
            Name = ""
        };
        TypeInfo CurrentTypeInfo {
            get {
                return currentTypeInfo;
            }
            set {
                if (value != null) {
                    currentTypeInfo = new TypeInfo(value);
                } else {
                    currentTypeInfo = new TypeInfo { Name = "" };
                }
                TypeName = currentTypeInfo.Name;
#if DEBUG
                Console.WriteLine("Type info now {0}", currentTypeInfo);
#endif
                ParseData();
                FillTypeList();
            }
        }        
        /* The name of the current type (for display mostly). */
        public string TypeName {
            get {
                return CurrentTypeInfo.Name;
            }
            set {
                CurrentTypeInfo.Name = value;
                typeNameLabel.Text = string.Format("Typename: {0} Version: {1} (parsed with {2})", TypeName, version, currentTypeInfo.Version);
            }
        }
        /* The list of type definitions. Setting this will trigger new parsing of the data. */
        List<FieldInfo> FieldTypes {
            get {
                return CurrentTypeInfo.Fields;
            }
            set {
                currentTypeInfo.Fields.Clear();
                List<FieldInfo> cleaned = new List<FieldInfo>();
                for (int i = 0; i < value.Count; i++) {
                    // correct the "unknown" columns
                    Regex unknowns = new Regex("[Uu]nknown([0-9]*)");
                    if (unknowns.IsMatch(value[i].Name)) {
                        value[i].Name = String.Format("unknown{0}", i);
#if DEBUG
                    } else {
#endif
                        Console.WriteLine("Does not match unknown: {0}", value[i].Name);
                    }
                    if (!value[i].TypeName.StartsWith("blob")) {
                        cleaned.Add(value[i]);
                    } else {
                        // compact byte types into multi-byte entry
                        int byteCount = value[i].CreateInstance().Length;
                        int nextIndex = i + 1;
                        while (nextIndex < value.Count && value[nextIndex].TypeName.StartsWith("blob")) {
                            i++;
                            nextIndex = i + 1;
                            byteCount += value[i].CreateInstance().Length;
                        }
                        cleaned.Add(new VarBytesType(byteCount));
                    }
                }
                currentTypeInfo.Fields.AddRange(cleaned);
                ParseData();
                FillTypeList();
            }
        }
        int version = 0;
        #endregion

        #region Navigation Attributes
        /*
         * The index of the parsed data element (nth element within data).
         */
        int currentRowIndex = 0;
        int CurrentRowIndex {
            get {
                return currentRowIndex;
            }
            set {
                currentRowIndex = Math.Min(value, currentValues.Count - 1);
                currentRowIndex = Math.Max(0, currentRowIndex);
                ShowPreview();
                FillValueList();
            }
        }
        /* 
         * The position within the data of the currently selected value in the value list.
         * If no value is selected, returns the offset after the currently shown element.
         */
        long CurrentCursorPosition {
            get {
                long showFrom = HeaderLength;
                if (currentValues.Count > CurrentRowIndex) {
                    showFrom = valueStartPositions[CurrentRowIndex];
                    if (valueList.SelectedIndex != -1) {
                        for (int i = 0; i < valueList.SelectedIndex; i++) {
                            showFrom += currentValues[currentRowIndex][i].ReadLength;
                        }
                    } else {
                        showFrom += CurrentValueLength;
                    }
                }
                return showFrom;
            }
        }
        /*
         * The length of the currently displayed parsed data element.
         */
        int CurrentValueLength {
            get {
                int result = 0;
                if (currentValues.Count > CurrentRowIndex) {
                    TableRow row = currentValues[CurrentRowIndex];
                    foreach (FieldInstance field in row) {
                        result += field.ReadLength;
                    }
                }
                return result;
            }
        }
        #endregion

        List<TableRow> currentValues = new List<TableRow>();
        List<long> valueStartPositions = new List<long>();        
        
        /* The types currently selected in the typeList. */
        List<FieldInfo> SelectedTypes {
            get {
                List<FieldInfo> result = new List<FieldInfo> ();
                foreach (object o in typeList.SelectedItems) {
                    result.Add(o as FieldInfo);
                }
                return result;
            }
        }
  
        #region Parsing
        /* Ignore the header bytes when parsing. */
        public bool IgnoreHeader {
            get; set;
        }
        /* 
         * The offset within the data bytes to start parsing at.
         * The length of the header, if it is not ignored.
         * Can be set differently to start parsing within a file.
         */
        long parserStart = -1;
        public long ParserStart {
            get {
                long result = parserStart;
                if (result == -1) {
                    result = IgnoreHeader ? parserStart : HeaderLength;
                }
                return result;
            }
            set {
                parserStart = value;
                Console.WriteLine("starting parsing from {0}", parserStart);
                ParseData();
            }
        }

        void ParseHeader() {
            // only read header if we have any data at all,
            // and if we're not in list parsing mode
            if (Bytes == null || IgnoreHeader) {
                return;
            }
            using (BinaryReader reader = new BinaryReader(new MemoryStream(Bytes))) {
                DBFileHeader header = PackedFileDbCodec.readHeader(reader);
                version = header.Version;
                if (Bytes != null && header != null) {
                    if (DBTypeMap.Instance.IsSupported(TypeName)) {
                        try {
                            DBFile decoded = new PackedFileDbCodec(TypeName).Decode(Bytes);
                            CurrentTypeInfo = new TypeInfo(decoded.CurrentType);
#if DEBUG
                            Console.WriteLine("Found decoding type version {0}", decoded.CurrentType.Version);
#endif
                        } catch {
                            List<TypeInfo> infos = DBTypeMap.Instance.GetVersionedInfos(TypeName, header.Version);
                            CurrentTypeInfo = infos[infos.Count-1];
                        }
                    }
                }
                HeaderLength = header.Length;
                ExpectedEntries = header.EntryCount;
            }
        }
        
        /*
         * Parse data from after the header until end of stream,
         * or until an exception occurs for one of the fields.
         * Fills the currentValues list with the parsing results.
         */
        void ParseData() {
            currentValues.Clear();
            valueStartPositions.Clear();
            long readUpTo = 0;
            if (Bytes != null && FieldTypes.Count > 0) {
                long currentPosition = 0;
                MemoryStream stream = new MemoryStream(Bytes);
                using (var reader = new BinaryReader(stream)) {
                    reader.BaseStream.Seek(ParserStart, SeekOrigin.Begin);
                    for (uint index = 0; index < ExpectedEntries; index++) {
                        currentPosition = reader.BaseStream.Position;
                        try {
                            // create new row to fill with data
                            TableRow newRow = new TableRow();
                            bool stopReading = false;
                            foreach (FieldInfo info in FieldTypes) {
                                FieldInstance result;
                                try {
                                    if (stopReading) {
                                        result = info.CreateInstance();
                                    } else {
                                        result = info.CreateInstance();
                                        result.Decode(reader);
                                    }
                                } catch (Exception e) {
                                    // show problems until end of this row
                                    result = info.CreateInstance();
                                    result = new FieldWrapper(result);
                                    result.Value = string.Format("{0:x} : {1}", 
                                                                 currentPosition, e.Message);
                                    Console.WriteLine(e);
                                    // finish reading for this row still
                                    stopReading = true;
                                }
                                newRow.Add(result);
                            }
                            // store new values and their start position
                            valueStartPositions.Add(currentPosition);
                            currentValues.Add(newRow);
                            // set when a problem has occurred: stop parsing
                            if (stopReading) {
                                break;
                            }
                        } catch {
                            break;
                        }
                    }
                    readUpTo = reader.BaseStream.Position;
                }
                repeatInfo.Text = string.Format("{0}/{1} entries, {2}/{3} bytes", 
                                                currentValues.Count, ExpectedEntries, 
                                                readUpTo, Bytes.Length);
                if (currentValues.Count == ExpectedEntries && readUpTo == Bytes.Length) {
                    repeatInfo.ForeColor = Color.Green;
                    Console.WriteLine("decoding finished");
                } else {
                    repeatInfo.ForeColor = Color.Black;
                }
            }
            FillValueList();
            ShowPreview();
#if DEBUG
            Console.WriteLine("cursor position now {0}", CurrentCursorPosition);
#endif
        }
        #endregion

        #region Display
        /* Show header bytes in the data preview. */
        bool showHeader = true;
        public bool ShowHeader {
            get { 
                return showHeader;
            }
            set {
                showHeader = value;
                ShowHexPreview();
            }
        }
        /* Fill type list with the current infos. */
        void FillTypeList() {
            typeList.Items.Clear();
            foreach (FieldInfo info in FieldTypes) {
                typeList.Items.Add(info);
            }
        }
        /* Fill value list with parsed data. */
        void FillValueList() {
            valueList.Items.Clear();
            if (currentValues.Count > CurrentRowIndex) {
                TableRow row = currentValues[CurrentRowIndex];
                foreach (FieldInstance instance in row) {
                    valueList.Items.Add(instance.Value);
                }
            }
        }

        /* Amount of bytes to show in preview field. */
        static int showByteCount = 4096;
        /* Show hex view in preview and value fields. */
        void ShowPreview() {
            ShowHexPreview();
            ShowValuePreviews();
        }
        /* Show hex data in preview field. */
        void ShowHexPreview() {
            hexView.Text = "";
            if (Bytes == null) {
                return;
            }
            // show header if applicable
            StringBuilder result =
                new StringBuilder(showHeader
                                  ? Util.FormatHex(Bytes, 0, HeaderLength) + " "
                                  : "");
            
            // show data
            if (valueStartPositions.Count > CurrentRowIndex) {
                result.Append(Util.FormatHex(Bytes, valueStartPositions[CurrentRowIndex], showByteCount));
            } else {
                result.Append(Util.FormatHex(Bytes, HeaderLength, showByteCount));
            }
            hexView.Text = result.ToString();
   
            // color header if applicable
            int selectFromIndex = 0;
            if (ShowHeader) {
                selectFromIndex = Math.Max((HeaderLength * 3) - 1, 0);
                hexView.Select(0, selectFromIndex);
                hexView.SelectionColor = Color.Red;
            }
            // color data
            hexView.Select(selectFromIndex, (CurrentValueLength) * 3);
            hexView.SelectionColor = Color.Blue;
            
            if (valueList.SelectedIndex != -1) {
                selectFromIndex = Math.Max((HeaderLength * 3) - 1, 0);
                if (currentValues.Count > CurrentRowIndex) {
                    TableRow row = currentValues[CurrentRowIndex];
                    int i = 0;
                    for (i = 0; i < valueList.SelectedIndex; i++) {
#if DEBUG
                        Console.WriteLine("selected '{0}', length {1}", row[i].Value, row[i].ReadLength);
#endif
                        selectFromIndex += row[i].ReadLength * 3;
                    }
                    int selectLength = row[valueList.SelectedIndex].ReadLength*3;
#if DEBUG
                    Console.WriteLine("selecting from {0}, length {1}", selectFromIndex, selectLength);
#endif
                    hexView.Select(selectFromIndex, selectLength);
                    hexView.SelectionColor = Color.Green;
                }
            }
        }

        /* Show preview of next or selected value for each type. */
        void ShowValuePreviews() {
            if (Bytes == null) {
                return;
            }
            TypeSelection[] selections = new TypeSelection[] {
                intType, stringType, boolType, singleType, optStringType, byteType
            };
            long showFrom = CurrentCursorPosition;
#if DEBUG
            Console.WriteLine("parser position {0}", showFrom);
#endif
            using (var reader = new BinaryReader(new MemoryStream(Bytes))) {
                foreach (TypeSelection selection in selections) {
                    reader.BaseStream.Position = showFrom;
                    selection.ShowPreview(reader);
                }
            }
        }
        #endregion

        #region Type Management
        /* Add the given type at the currently selected index in the type list,
         * or the end if none is selected.
         */
        private void AddType(FieldInfo type) {
            List<FieldInfo> types = new List<FieldInfo>(FieldTypes);
            if (typeList.SelectedIndex != -1) {
                types.Insert(typeList.SelectedIndex, type);
            } else {
                types.Add(type);
            }
            FieldTypes = types;
        }
        /* Remove selected types, or the last one if none is selected. */
        private void DeleteType(object sender, EventArgs e) {
            if (typeList.Items.Count == 0) {
                return;
            }
            List<FieldInfo> types = new List<FieldInfo> ();
            if (typeList.SelectedIndex != -1) {
                for (int i = 0; i < FieldTypes.Count; i++) {
                    if (!typeList.SelectedIndices.Contains (i)) {
                        types.Add (FieldTypes [i]);
                    }
                }
            } else {
                types.AddRange (FieldTypes);
                types.RemoveAt(types.Count - 1);
            }
            FieldTypes = types;
        }
        
        private void NameType(object sender, EventArgs e) {
            if (typeList.Items.Count == 0) {
                return;
            }
            InputBox box = new InputBox();
            List<FieldInfo> types = new List<FieldInfo> (FieldTypes);
            foreach(int i in typeList.SelectedIndices) {
                box.Input = types[i].Name;
                if (box.ShowDialog() == DialogResult.OK) {
                    types[i].Name = box.Input;
                }
            }
            FieldTypes = types;
        }
        
        /*
         * Menu handler for the transform menu items.
         * Apply the corresponding Transformation on the selected types.
         */
        void TransformSelection(object o, EventArgs args) {
            ITransform transformation = (o as ToolStripMenuItem).Tag as ITransform;
            List<FieldInfo> newTypes = new List<FieldInfo> ();
            List<FieldInfo> transformed = transformation.Transform (SelectedTypes);
            bool added = false;
            for (int i = 0; i < FieldTypes.Count; i++) {
                if (!typeList.SelectedIndices.Contains (i)) {
                    newTypes.Add (FieldTypes [i]);
                } else if (!added) {
                    newTypes.AddRange (transformed);
                    added = true;
                }
            }
            FieldTypes = newTypes;
        }
        #endregion

        #region Menu Handler
        private void OpenEncodedFile(object sender, EventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog ();
			if (dlg.ShowDialog () == DialogResult.OK) {
                // prevent superfluous re-parse
                Bytes = null;
                TypeName = Path.GetFileName(Path.GetDirectoryName (dlg.FileName));
                CurrentTypeInfo.Fields.Clear();
				Bytes = File.ReadAllBytes (dlg.FileName);
			}
		}

        private void LoadSchemaFile(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                DBTypeMap.Instance.initializeFromFile(dlg.FileName);
                ParseHeader();
                ParseData();
                ShowPreview();
            }
        }

        private void SaveSchemaFile(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                using (FileStream stream = File.OpenWrite(dlg.FileName)) {
                    XmlExporter exporter = new XmlExporter(stream);
                    exporter.Export(DBTypeMap.Instance.AllInfos);
                }
            }
        }

        private void toggleEncoding(object sender, EventArgs e) {
            unicode = !unicode;
            if (unicode) {
                stringType.Factory = Types.StringType;
                optStringType.Factory = Types.OptStringType;
            } else {
                stringType.Factory = Types.StringTypeAscii;
                optStringType.Factory = Types.OptStringTypeAscii;
            }
        }
#endregion

        #region Browsing
        private void goStart_Click(object sender, EventArgs e) {
            CurrentRowIndex = 0;
        }

        private void back_Click(object sender, EventArgs e) {
            CurrentRowIndex = CurrentRowIndex - 1;
        }

        private void forward_Click(object sender, EventArgs e) {
            CurrentRowIndex = CurrentRowIndex + 1;
        }

        private void goProblem_Click(object sender, EventArgs e) {
            if (currentValues.Count > 0) {
                CurrentRowIndex = currentValues.Count - 1;
            }
        }
        #endregion

        private void setHeaderLength_Click(object sender, EventArgs e) {
            int newLength;
            if (int.TryParse(headerLengthField.Text, out newLength)) {
                HeaderLength = newLength;
            }
        }

        private void showTypes_Click(object sender, EventArgs e) {
            string text = XmlExporter.TableToString(TypeName, version, FieldTypes);
			TextDisplay d = new TextDisplay (text);
			d.ShowDialog ();
		}

        private void setButton_Click(object sender, EventArgs e) {
            TypeInfo info = new TypeInfo {
                Name = TypeName,
                Version = version
            };
            info.Fields.AddRange(FieldTypes);
            DBTypeMap.Instance.AllInfos.Add(info);
            DBTypeMap.Instance.SaveToFile("schema_current.xml");
            Close();
        }

        #region Extended Type Management
        private void ParseFromHere(object sender, EventArgs e) {
            long dataStart = valueStartPositions[CurrentRowIndex];
            ParserStart = dataStart;
        }
        private void ParseFromStart(object sender, EventArgs e) {
            ParserStart = -1;
        }

        private void ReapplyExisting(object sender, EventArgs e) {
            List<FieldInfo> existing = new List<FieldInfo>(FieldTypes);
            existing.AddRange(FieldTypes);
            FieldTypes = existing;
        }

        private void more5ToolStripMenuItem_Click(object sender, EventArgs e) {

        }
        #endregion

        #region List Selection
        /*
         * Go through each of the available transforms and enable the corresponding
         * menu items, depending on selection in type list.
         */
        void EnableTransforms(object o, EventArgs args) {
            List<FieldInfo> selected = SelectedTypes;
            foreach (ToolStripMenuItem item in this.transformToolStripMenuItem.DropDownItems) {
                ITransform transform = item.Tag as ITransform;
                item.Enabled = transform.CanTransform (selected);
            }
        }
        private void valueList_SelectedIndexChanged(object sender, EventArgs e) {
            // show would-be preview of values at currently selected position
            ShowValuePreviews();
        }
        private void ClearSelection(object sender, MouseEventArgs args) {
            if (args.Button == MouseButtons.Right) {
                ListBox list = sender as ListBox;
                if (list != null) {
                    list.ClearSelected();
                }
            }
        }
        void MarkSelectedValue(object o, EventArgs args) {
            ShowHexPreview();
        }
        #endregion
  
        #region Attic
//        void StartList(object o, EventArgs args) {
//            long currentPosition = CurrentCursorPosition;
//            byte[] listBytes;
//            uint entries;
//            using (var source = new BinaryReader(new MemoryStream(Bytes))) {
//                source.BaseStream.Position = currentPosition;
//                entries = source.ReadUInt32();
//                using (var dest = new MemoryStream()) {
//                    source.BaseStream.CopyTo(dest);
//                    listBytes = dest.ToArray();
//                }
//            }
//            List<FieldInfo> infos = new List<FieldInfo>();
//            if (typeList.SelectedIndex != -1) {
//                ListType list = FieldTypes[typeList.SelectedIndex] as ListType;
//                if (list != null) {
//                    infos.AddRange(list.Infos);
//                }
//            }
//            DecodeTool listTool = new DecodeTool() {
//                ExpectedEntries = entries,
//                TypeName = "",
//                IgnoreHeader = true,
//                FieldTypes = infos,
//                Bytes = listBytes,
//            };
//            listTool.ShowDialog();
//            if (listTool.FieldTypes.Count > 0) {
//                infos = new List<FieldInfo>(FieldTypes);
//                ListType listType = new ListType() {
//                    Infos = listTool.FieldTypes
//                };
//                AddType(listType);
//            }
//            using (var stream = File.Create("schema_bak.xml")) {
//                new XmlExporter(stream).Export();
//            }
//            listTool.Dispose();
//            FillValueList();
//        }
        #endregion
    }
    
    public class FieldWrapper : FieldInstance {
        public FieldInstance BaseInstance {
            get; private set;
        }
        
        public FieldWrapper (FieldInstance baseInstance) : base(baseInstance.Info) {
            BaseInstance = baseInstance;
        }
        
        public bool Invalid {
            get; private set;
        }
        public override string Value {
            get {
                return Invalid ? base.Value : BaseInstance.Value;
            }
            set {
                try {
                    BaseInstance.Value = value;
                    Invalid = false;
                } catch (Exception) {
                    base.Value = value;
                    Invalid = true;
                }
            }
        }
        public override void Encode(BinaryWriter writer) {
            throw new NotImplementedException ();
        }
        public override void Decode(BinaryReader reader) {
            throw new NotImplementedException ();
        }
    }
}
