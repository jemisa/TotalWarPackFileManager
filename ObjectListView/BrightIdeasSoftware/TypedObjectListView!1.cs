namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public class TypedObjectListView<T> where T: class
    {
        private TypedCheckStateGetterDelegate<T> checkStateGetter;
        private TypedCheckStatePutterDelegate<T> checkStatePutter;
        private ObjectListView olv;

        public TypedObjectListView(ObjectListView olv)
        {
            this.olv = olv;
        }

        public virtual void GenerateAspectGetters()
        {
            for (int i = 0; i < this.ListView.Columns.Count; i++)
            {
                this.GetColumn(i).GenerateAspectGetter();
            }
        }

        public virtual TypedColumn<T> GetColumn(int i)
        {
            return new TypedColumn<T>(this.olv.GetColumn(i));
        }

        public virtual TypedColumn<T> GetColumn(string name)
        {
            return new TypedColumn<T>(this.olv.GetColumn(name));
        }

        public virtual T GetModelObject(int index)
        {
            return (T) this.olv.GetModelObject(index);
        }

        public virtual TypedBooleanCheckStateGetterDelegate<T> BooleanCheckStateGetter
        {
            set
            {
                if (value == null)
                {
                    this.olv.BooleanCheckStateGetter = null;
                }
                else
                {
                    this.olv.BooleanCheckStateGetter = x => value((T) x);
                }
            }
        }

        public virtual TypedBooleanCheckStatePutterDelegate<T> BooleanCheckStatePutter
        {
            set
            {
                if (value == null)
                {
                    this.olv.BooleanCheckStatePutter = null;
                }
                else
                {
                    this.olv.BooleanCheckStatePutter = (x, newValue) => value((T) x, newValue);
                }
            }
        }

        public virtual TypedCellToolTipGetterDelegate<T> CellToolTipGetter
        {
            set
            {
                if (value == null)
                {
                    this.olv.CellToolTipGetter = null;
                }
                else
                {
                    this.olv.CellToolTipGetter = (col, x) => value(col, (T) x);
                }
            }
        }

        public virtual T CheckedObject
        {
            get
            {
                return (T) this.olv.CheckedObject;
            }
        }

        public virtual IList<T> CheckedObjects
        {
            get
            {
                IList checkedObjects = this.olv.CheckedObjects;
                List<T> list2 = new List<T>(checkedObjects.Count);
                foreach (object obj2 in checkedObjects)
                {
                    list2.Add((T) obj2);
                }
                return list2;
            }
            set
            {
                this.olv.CheckedObjects = (IList) value;
            }
        }

        public virtual TypedCheckStateGetterDelegate<T> CheckStateGetter
        {
            get
            {
                return this.checkStateGetter;
            }
            set
            {
                this.checkStateGetter = value;
                if (value == null)
                {
                    this.olv.CheckStateGetter = null;
                }
                else
                {
                    this.olv.CheckStateGetter = x => base.checkStateGetter((T) x);
                }
            }
        }

        public virtual TypedCheckStatePutterDelegate<T> CheckStatePutter
        {
            get
            {
                return this.checkStatePutter;
            }
            set
            {
                this.checkStatePutter = value;
                if (value == null)
                {
                    this.olv.CheckStatePutter = null;
                }
                else
                {
                    this.olv.CheckStatePutter = (x, newValue) => base.checkStatePutter((T) x, newValue);
                }
            }
        }

        public virtual HeaderToolTipGetterDelegate HeaderToolTipGetter
        {
            get
            {
                return this.olv.HeaderToolTipGetter;
            }
            set
            {
                this.olv.HeaderToolTipGetter = value;
            }
        }

        public virtual ObjectListView ListView
        {
            get
            {
                return this.olv;
            }
            set
            {
                this.olv = value;
            }
        }

        public virtual IList<T> Objects
        {
            get
            {
                List<T> list = new List<T>(this.olv.GetItemCount());
                for (int i = 0; i < this.olv.GetItemCount(); i++)
                {
                    list.Add(this.GetModelObject(i));
                }
                return list;
            }
            set
            {
                this.olv.SetObjects(value);
            }
        }

        public virtual T SelectedObject
        {
            get
            {
                return (T) this.olv.GetSelectedObject();
            }
            set
            {
                this.olv.SelectObject(value, true);
            }
        }

        public virtual IList<T> SelectedObjects
        {
            get
            {
                List<T> list = new List<T>(this.olv.SelectedIndices.Count);
                foreach (int num in this.olv.SelectedIndices)
                {
                    list.Add((T) this.olv.GetModelObject(num));
                }
                return list;
            }
            set
            {
                this.olv.SelectObjects((IList) value);
            }
        }

        public delegate bool TypedBooleanCheckStateGetterDelegate(T rowObject);

        public delegate bool TypedBooleanCheckStatePutterDelegate(T rowObject, bool newValue);

        public delegate string TypedCellToolTipGetterDelegate(OLVColumn column, T modelObject);

        public delegate CheckState TypedCheckStateGetterDelegate(T rowObject);

        public delegate CheckState TypedCheckStatePutterDelegate(T rowObject, CheckState newValue);
    }
}

