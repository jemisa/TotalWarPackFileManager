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
        public ObservableCollection<FieldInfoViewModel> TableTypeInformationRows { get; set; } = new ObservableCollection<FieldInfoViewModel>();

        FieldInfoViewModel _selectedTypeInformationRow;
        public FieldInfoViewModel SelectedTypeInformationRow
        {
            get { return _selectedTypeInformationRow; }
            set
            {
                _selectedTypeInformationRow = value;
                _eventHub.TriggerOnSelectedDbSchemaRowChanged(this, _selectedTypeInformationRow);
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

        string _currentTableName = null;
        int _currentVersion = -1;


        EventHub _eventHub;
        public DbTableDefinitionController(EventHub eventHub)
        {
            _eventHub = eventHub;
            _eventHub.OnSetDbSchema +=(sender, newSchema) => { Set(newSchema); };
            _eventHub.OnFileSelected += _eventHub_OnFileSelected;
            _eventHub.OnHeaderVersionChanged += _eventHub_OnHeaderVersionChanged;
            _eventHub.OnNewDbSchemaRowCreated += AppendRowOfTypeEventHandler;


            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionRemovedAllCommand = new RelayCommand(OnDbDefinitionRemovedAll);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
            CreateNewDbDefinitionCommand = new RelayCommand(OnCreateNewDbDefinitionCommand);
            DbDefinitionReloadAllCommand = new RelayCommand(OnDefinitionReloadAll);
            DeselectCommand = new RelayCommand(OnDeselectCommand);
        }

        private void _eventHub_OnHeaderVersionChanged(object sender, int e)
        {
            if (_currentVersion != e)
            {
                _currentVersion = e;
                LoadCurrentTableDefinition();
            }
        }

        private void _eventHub_OnFileSelected(object sender, DataBaseFile e)
        {
            if (e.TableType != _currentTableName)
            {
                _currentTableName = e.TableType;
                LoadCurrentTableDefinition();
            }
        }

        void LoadCurrentTableDefinition()
        {
            if (_currentTableName == null || _currentVersion == -1)
                return;

            var allTableDefinitions = DBTypeMap.Instance.GetVersionedInfos(_currentTableName, _currentVersion);

            var fieldCollection = allTableDefinitions.FirstOrDefault(x => x.Version == _currentVersion);
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
            _eventHub.TriggerOnDbSchemaChanged(this, TableTypeInformationRows.Select(x => x.GetFieldInfo()).ToList());
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
                LoadCurrentTableDefinition();
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

        void AppendRowOfTypeEventHandler(object e, DbTypesEnum type)
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
