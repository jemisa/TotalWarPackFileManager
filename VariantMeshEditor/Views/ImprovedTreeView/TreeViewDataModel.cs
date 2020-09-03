using System.Collections.Generic;
using System.ComponentModel;

namespace TreeViewWithCheckBoxes
{
    public class TreeViewDataModel : INotifyPropertyChanged
    {
        #region Data

        bool? _isChecked = false;
        TreeViewDataModel _parent;

        #endregion // Data

        #region CreateFoos

        public static List<TreeViewDataModel> CreateFoos()
        {
            TreeViewDataModel root = new TreeViewDataModel("Weapons")
            {
                IsInitiallySelected = true,
                Children =
                {
                    new TreeViewDataModel("Blades")
                    {
                        Children =
                        {
                            new TreeViewDataModel("Dagger"),
                            new TreeViewDataModel("Machete"),
                            new TreeViewDataModel("Sword"),
                        }
                    },
                    new TreeViewDataModel("Vehicles")
                    {
                        Children =
                        {
                            new TreeViewDataModel("Apache Helicopter"),
                            new TreeViewDataModel("Submarine"),
                            new TreeViewDataModel("Tank"),                            
                        }
                    },
                    new TreeViewDataModel("Guns")
                    {
                        Children =
                        {
                            new TreeViewDataModel("AK 47"),
                            new TreeViewDataModel("Beretta"),
                            new TreeViewDataModel("Uzi"),
                        }
                    },
                }
            };

            root.Initialize();
            return new List<TreeViewDataModel> { root };
        }

        TreeViewDataModel(string name)
        {
            this.Name = name;
            this.Children = new List<TreeViewDataModel>();
        }

        void Initialize()
        {
            foreach (TreeViewDataModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        #endregion // CreateFoos

        #region Properties

        public List<TreeViewDataModel> Children { get; private set; }

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