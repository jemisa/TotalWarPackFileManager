using ICSharpCode.AvalonEdit.Highlighting;
using PackFileManager.Editors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PackFileManager.PackFileManagerSettings;

namespace PackFileManager.Dialogs.Settings
{
    public partial class FileExtentionSyntaxMappingForm : Form
    {
        public FileExtentionSyntaxMappingForm(SettingsFormInput formsInput, List<CustomFileExtentionHighlightsMapping> previouslySavedMappings)
        {
            InitializeComponent();
            Create(formsInput, previouslySavedMappings);
        }

        public List<CustomFileExtentionHighlightsMapping> GetMappings()
        {
            var output = new List<CustomFileExtentionHighlightsMapping>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var defaultCheckBoxCell = row.Cells[0] as DataGridViewCheckBoxCell;
                var fileExtentionCell = row.Cells[1] as DataGridViewTextBoxCell;
                var comboBoxCell = row.Cells[2] as DataGridViewComboBoxCell;

                if (defaultCheckBoxCell.Value != null && (bool)defaultCheckBoxCell.Value == false)
                {
                    var fileExtention = fileExtentionCell.Value as string; ;
                    var highlights = comboBoxCell.Value as string;
                    if(string.IsNullOrWhiteSpace(highlights) == false)
                        output.Add(new CustomFileExtentionHighlightsMapping()
                        {
                            HighlightMapping = highlights,
                            Extention = fileExtention
                        } );
                }
            }

            return output;
        }

        void Create(SettingsFormInput formsInput, List<CustomFileExtentionHighlightsMapping> previouslySavedMappings)
        {
            var allHightLights  = HighlightingManager.Instance.HighlightingDefinitions;

            DataTable highligtsTable = new DataTable("HighlightingDefinitionsTable");
            highligtsTable.Columns.Add("column1", typeof(string));
            foreach (var item in allHightLights)
                highligtsTable.Rows.Add(item.ToString());

            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Default";
            dataGridView1.Columns.Add(checkBoxColumn);

            DataGridViewTextBoxColumn fileExtentionNameColumn = new DataGridViewTextBoxColumn();
            fileExtentionNameColumn.HeaderText = "File Extention";
            dataGridView1.Columns.Add(fileExtentionNameColumn);

            DataGridViewComboBoxColumn highlightsComboBoxColumn = new DataGridViewComboBoxColumn();
            highlightsComboBoxColumn.DataSource = new BindingSource(highligtsTable, null);
            highlightsComboBoxColumn.DisplayMember = "column1"; 
            highlightsComboBoxColumn.ValueMember = "column1"; 
            dataGridView1.Columns.Add(highlightsComboBoxColumn);

            /*DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
            btn.HeaderText = "Click Data";
            btn.Text = "Click Here";
            btn.Name = "btn";
            btn.HeaderText = "Delete";
            btn.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(btn);*/

            var privSavedExts = previouslySavedMappings.Select(x => x.Extention);
            var filteredInputExtentions = formsInput.FileExtentions.Where(x => privSavedExts.Contains(x) == false);


            var defaultList = filteredInputExtentions
                .Where(x => HighlightingManager.Instance.GetDefinitionByExtension(x) != null)
                .OrderBy(x => x)
                .ToList();

            var unknownList = filteredInputExtentions
                .Where(x => HighlightingManager.Instance.GetDefinitionByExtension(x) == null)
                .OrderBy(x => x)
                .ToList();

            for (int i = 0; i < previouslySavedMappings.Count; i++)
            {
                CreateRow(false, previouslySavedMappings[i].Extention, previouslySavedMappings[i].HighlightMapping);
            }

            for (int i = 0; i < defaultList.Count; i++)
            {
                CreateRow(true, defaultList[i], HighlightingManager.Instance.GetDefinitionByExtension(defaultList[i]).ToString());
            }

            for (int i = 0; i < unknownList.Count; i++)
            {
                CreateRow(false, unknownList[i]);
            }

            dataGridView1.CellEnter += datagridview_CellEnter;
            dataGridView1.CurrentCellDirtyStateChanged += datagridview_CurrentCellDirtyStateChanged;
        }

        void CreateRow(bool readOnly, string fileExtention, string dropDownValue = null)
        {
            var idx = dataGridView1.Rows.Add();
            var row = dataGridView1.Rows[idx];

            var defaultCheckBoxCell = row.Cells[0] as DataGridViewCheckBoxCell;
            var fileExtentionCell = row.Cells[1] as DataGridViewTextBoxCell;
            var comboBoxCell = row.Cells[2] as DataGridViewComboBoxCell;

            defaultCheckBoxCell.ReadOnly = true;

            if (readOnly)
            {
                row.ReadOnly = readOnly;
                defaultCheckBoxCell.Value = true;
            }
            else
            {
                defaultCheckBoxCell.Value = false;
            }
            fileExtentionCell.Value = fileExtention;
            fileExtentionCell.ReadOnly = true;

            if (dropDownValue != null)
                comboBoxCell.Value = dropDownValue;

        }

        private void datagridview_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            bool validClick = (e.RowIndex != -1 && e.ColumnIndex != -1); //Make sure the clicked row/column is valid.
            var datagridview = sender as DataGridView;

            // Check to make sure the cell clicked is the cell containing the combobox 
            if (datagridview.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn && validClick)
            {
                datagridview.BeginEdit(true);
                ((ComboBox)datagridview.EditingControl).DroppedDown = true;
            }
        }

        private void datagridview_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

    }
}
