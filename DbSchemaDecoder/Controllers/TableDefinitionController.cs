﻿using DbSchemaDecoder.Util;
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


    class FieldInfoViewModel : NotifyPropertyChangedImpl
    {

        public int Index { get { return _index; } }
        bool _use = true;
        int _index;
        public bool Use { get { return _use;  } set { _use = value;  NotifyPropertyChanged(); } }
        public string Name { get { return _fieldInfo.Name; } set { _fieldInfo.Name = value; NotifyPropertyChanged(); } }
        public DbTypesEnum Type 
        { 
            get { return _fieldInfo.TypeEnum; } 
            set
            {
                var newInstance = Types.FromEnum(value);
                newInstance.Name = _fieldInfo.Name;
                newInstance.FieldReference = _fieldInfo.FieldReference;
                newInstance.Optional = _fieldInfo.Optional;
                newInstance.PrimaryKey = _fieldInfo.PrimaryKey;
                _fieldInfo = newInstance;
                NotifyPropertyChanged();
            }
        }

        public bool PrimaryKey { get { return _fieldInfo.PrimaryKey; } set { _fieldInfo.PrimaryKey = value; } }
        public bool Optional { get { return _fieldInfo.Optional; } set { _fieldInfo.Optional = value; } }
        public string ReferencedTable { get { return _fieldInfo.ReferencedTable; } }
        public string ReferencedField { get { return _fieldInfo.ReferencedField; } set { _fieldInfo.ReferencedField = value; } }

        public FieldInfoViewModel(FieldInfo info, int idx)
        {
            _index = idx;
            _fieldInfo = info;
        }

        FieldInfo _fieldInfo;
        public FieldInfo GetFieldInfo() { return _fieldInfo; }
        public void SetIndex(int idx) { _index = idx; }
    } 

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
        public ICommand DbDefinitionMovedUpCommand { get; private set; }
        public ICommand DbDefinitionMovedDownCommand { get; private set; }
        public ICommand CreateNewDbDefinitionCommand { get; private set; }
        public ICommand DeselectCommand { get; private set; }

        public DbTableDefinitionController()
        {
            DbDefinitionRemovedCommand = new RelayCommand(OnDbDefinitionRemoved);
            DbDefinitionMovedUpCommand = new RelayCommand(OnDbDefinitionMovedUp);
            DbDefinitionMovedDownCommand = new RelayCommand(OnDbDefinitionMovedDown);
            CreateNewDbDefinitionCommand = new RelayCommand(OnCreateNewDbDefinitionCommand);
            DeselectCommand = new RelayCommand(OnDeselectCommand);
        }

        public void LoadCurrentTableDefinition(string tableName, int currentVersion)
        {
            var allTableDefinitions = DBTypeMap.Instance.GetVersionedInfos(tableName, currentVersion);
            TableTypeInformationRows.Clear();

            var fieldCollection = allTableDefinitions.FirstOrDefault(x => x.Version == currentVersion);
            if (fieldCollection == null)
                return;

            for (int i = 0; i < fieldCollection.Fields.Count(); i++)
            {
                var newFieldInfoViewModel = new FieldInfoViewModel(fieldCollection.Fields[i], i + 1);
                newFieldInfoViewModel.PropertyChanged += NewFieldInfoViewModel_PropertyChanged;
                TableTypeInformationRows.Add(newFieldInfoViewModel);
            }
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

        void RecomputeIndexes()
        {
            for (int i = 0; i < TableTypeInformationRows.Count; i++)
                TableTypeInformationRows[i].SetIndex(i + 1);
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
