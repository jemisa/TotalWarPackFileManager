using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TreeViewWithCheckBoxes
{
    public class TreeViewDataModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        TreeViewDataModel _parent;
        public object Tag { get; set; }

        public TreeViewDataModel Parent { get { return _parent; } }

        #endregion // Data



        public TreeViewDataModel(string name)
        {
            this.Name = name;
            this.Children = new ObservableCollection<TreeViewDataModel>();
        }

        public void Initialize()
        {
            foreach (TreeViewDataModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #region Properties

        public ObservableCollection<TreeViewDataModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Name { get; private set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value); }
        }

        public System.Windows.Visibility Vis { get; set; } = System.Windows.Visibility.Visible;

        void SetIsChecked(bool? value)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            this.OnPropertyChanged("IsChecked");
        }


        #endregion // IsChecked

        #endregion // Properties

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}