using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Common;
using Filetypes;

namespace PackFileManager {
    public partial class BuildingModelEditor : UserControl, IPackedFileEditor {
        public BuildingModelEditor() {
            InitializeComponent();

            modelGridView.SelectionChanged += new EventHandler(SetEntrySource);
            entryGridView.SelectionChanged += new EventHandler(SetCoordinates);

            modelGridView.UserAddedRow += new DataGridViewRowEventHandler(AddModelRow);
        }

        public bool CanEdit(PackedFile file) {
            return DBFile.typename(file.FullPath).Equals("models_building_tables");
        }

        private PackedFile packedFile;
        public PackedFile CurrentPackedFile {
            get {
                return packedFile;
            }
            set {
                DBFile dbFile = PackedFileDbCodec.Decode(value);
                EditedFile = new BuildingModelFile(dbFile);
                packedFile = value;
            }
        }

        public void Commit() {
        }

        BuildingModelFile file;
        public BuildingModelFile EditedFile {
            get {
                return file;
            }
            set {
                file = value;
                EntryDataSource = new List<BuildingModel>();
                coordinatesSource.DataSource = new List<Coordinates>();
                modelSource.DataSource = file.Models;
            }
        }

        private void SetEntrySource(object o, EventArgs args) {
            int index = -1;
            if (EditedFile != null) {
                index = SelectedRowIndex(modelGridView);
            }
            if (index != -1) {
                try {
                    entrySource.DataSource = EditedFile.Models[index].Entries;
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            } else {
                EntryDataSource = new List<BuildingModel>();
                coordinatesSource.DataSource = new List<Coordinates>();
            }
        }

        List<BuildingModel> EntryDataSource {
            set {
                entrySource.DataSource = value;
                entryGridView.AllowUserToAddRows = value.Count != 0;
                entrySource.AllowNew = value.Count != 0;
            }
        }

        private void SetCoordinates(object o, EventArgs args) {
            int index = SelectedRowIndex(entryGridView);
            if (index != -1) {
                BuildingModelEntry entry = ((List<BuildingModelEntry>)entrySource.DataSource)[index];
                List<Coordinates> coords = entry.Coordinates;
                coordinatesSource.DataSource = coords;
            } else {
                coordinatesSource.DataSource = new List<Coordinates>();
            }
        }

        private int SelectedRowIndex(DataGridView gridView) {
            int index = -1;
            if (gridView.SelectedRows.Count > 0) {
                index = gridView.SelectedRows[0].Index;
            } else if (gridView.SelectedCells.Count > 0) {
                index = gridView.SelectedCells[0].RowIndex;
            }
            return index;
        }

        void AddModelRow(object o, DataGridViewRowEventArgs args) {
            Console.WriteLine("adding row");
        }
    }
}
