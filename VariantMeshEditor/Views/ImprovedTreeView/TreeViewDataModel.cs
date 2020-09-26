using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TreeViewWithCheckBoxes
{
    public abstract class TreeViewDataModel : INotifyPropertyChanged
    {
        bool _isChecked = false;

        public TreeViewDataModel(string name)
        {
            this.DisplayName = name;
        }

        #region Properties

        

        public bool IsInitiallySelected { get; private set; }

        public string DisplayName { get; protected set; }

        #region IsChecked

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value); }
        }

        System.Windows.Visibility _vis = System.Windows.Visibility.Visible;
        public System.Windows.Visibility Vis 
        { 
            get { return _vis; }
            set
            {
                _vis = value;
                OnPropertyChanged("Vis");
            } 
        } 

        void SetIsChecked(bool value)
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
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}