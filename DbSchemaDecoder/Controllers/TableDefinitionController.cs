using DbSchemaDecoder.Models;
using DbSchemaDecoder.Util;
using Filetypes;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DbSchemaDecoder.Controllers
{
    class DbTableDefinitionController : NotifyPropertyChangedImpl
    {
        public event EventHandler<List<FieldInfo>> OnDefinitionChangedEvent;
        public event EventHandler<FieldInfoViewModel> OnSelectedRowChangedEvent;


        public List<FieldInfo> TableTypeInformationTypes { get { return TableTypeInformationRows.Select(x => x.GetFieldInfo()).ToList(); } }
        public ObservableCollection<FieldInfoViewModel> TableTypeInformationRows { get; set; } = new ObservableCollection<FieldInfoViewModel>();

        FieldInfoViewModel _selectedTypeInformationRow;
        public FieldInfoViewModel SelectedTypeInformationRow
        {
            get { return _selectedTypeInformationRow; }
            set
            {
                _selectedTypeInformationRow = value;
                OnSelectedRowChangedEvent?.Invoke(this, _selectedTypeInformationRow);
                NotifyPropertyChanged();
            }
        }

        public ICommand DbDefinitionRemovedCommand { get; private set; }
        public ICommand DbDefinitionRemovedAllCommand { get; private set; }
        public ICommand DbDefinitionMovedUpCommand { get; private set; }
        public ICommand DbDefinitionMovedDownCommand { get; private set; }
        public ICommand CreateNewDbDefinitionCommand { get; private set; }
        public ICommand DeselectCommand { get; private set; }
        public ICommand DbDefinitionReloadAllCommand { get; private set; }

        string _currentTableName;
        int _currentVersion;

        public DbTableDefinitionController()
        {
            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionRemovedAllCommand = new RelayCommand(OnDbDefinitionRemovedAll);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
            CreateNewDbDefinitionCommand = new RelayCommand(OnCreateNewDbDefinitionCommand);
            DbDefinitionReloadAllCommand = new RelayCommand(OnDefinitionReloadAll);
            DeselectCommand = new RelayCommand(OnDeselectCommand);
        }

        public void LoadCurrentTableDefinition(string tableName, int currentVersion)
        {
            _currentTableName = tableName;
            _currentVersion = currentVersion;

            var allTableDefinitions = DBTypeMap.Instance.GetVersionedInfos(tableName, currentVersion);

            var fieldCollection = allTableDefinitions.FirstOrDefault(x => x.Version == currentVersion);
            if (fieldCollection == null)
            {
                Set(new List<FieldInfo>());
                return;
            }

            Set(fieldCollection.Fields);
        }

        public void Set(List<FieldInfo> fields)
        {
            TableTypeInformationRows.Clear();
            for (int i = 0; i < fields.Count(); i++)
            {
                var newFieldInfoViewModel = new FieldInfoViewModel(fields[i], i + 1);
                newFieldInfoViewModel.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
                TableTypeInformationRows.Add(newFieldInfoViewModel);
            }
            OnDefinitionChanged();
        }

        private void NewFieldInfoViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDefinitionChanged();
        }

        private void OnDefinitionChanged()
        {
            OnDefinitionChangedEvent?.Invoke(this, TableTypeInformationRows.Select(x=>x.GetFieldInfo()).ToList());
        }

        private void OnCreateNewDbDefinitionCommand()
        {
            var type = Types.StringType();
            type.Name = "Column_" + TableTypeInformationRows.Count() + 1;
            var item = new FieldInfoViewModel(type, TableTypeInformationRows.Count() + 1);
            item.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
            TableTypeInformationRows.Add(item);
            OnDefinitionChanged();
        }



        private void OnDbDefinitionRemoved()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            TableTypeInformationRows.RemoveAt(index);
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionRemovedAll()
        {
            TableTypeInformationRows.Clear();
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionMovedUp()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            if (index == 0)
                return;
            var selectedRef = SelectedTypeInformationRow;
            TableTypeInformationRows.RemoveAt(index);
            TableTypeInformationRows.Insert(index - 1, selectedRef);
            SelectedTypeInformationRow = TableTypeInformationRows[index - 1];
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        private void OnDbDefinitionMovedDown()
        {
            if (SelectedTypeInformationRow == null)
                return;
            var index = TableTypeInformationRows.IndexOf(SelectedTypeInformationRow);
            if (index == TableTypeInformationRows.Count - 1)
                return;
            var selectedRef = SelectedTypeInformationRow;
            TableTypeInformationRows.RemoveAt(index);
            TableTypeInformationRows.Insert(index + 1, selectedRef);
            SelectedTypeInformationRow = TableTypeInformationRows[index + 1];
            RecomputeIndexes();
            OnDefinitionChanged();
        }

        void OnDeselectCommand()
        {
            SelectedTypeInformationRow = null;
        }
        void OnDefinitionReloadAll()
        {
            if (!string.IsNullOrWhiteSpace(_currentTableName))
                LoadCurrentTableDefinition(_currentTableName, _currentVersion);
        }

        void RecomputeIndexes()
        {
            for (int i = 0; i < TableTypeInformationRows.Count; i++)
            {
                TableTypeInformationRows[i].SetIndex(i + 1);
                if (TableTypeInformationRows[i].Name == "unknown")
                    TableTypeInformationRows[i].Name = "unknown_" + i;
            }
        }

        public void AppendRowOfTypeEventHandler(object e, DbTypesEnum type)
        {
            var newType = Types.FromEnum(type);
            var newFieldInfoViewModel = new FieldInfoViewModel(newType, 99);
            newFieldInfoViewModel.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
            TableTypeInformationRows.Add(newFieldInfoViewModel);

            
            RecomputeIndexes();

            OnDefinitionChanged();
        }
    }
}
