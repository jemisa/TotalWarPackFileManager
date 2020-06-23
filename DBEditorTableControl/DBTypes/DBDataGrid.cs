using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DBTableControl
{
    public partial class DBDataGrid : DataGrid
    {
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    base.OnExecutedCopy(e);
                    break;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }

        protected override void OnCanExecuteBeginEdit(System.Windows.Input.CanExecuteRoutedEventArgs e)
        {

            bool hasCellValidationError = false;
            bool hasRowValidationError = false;
            BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance;
            //Current cell
            PropertyInfo cellErrorInfo = this.GetType().BaseType.GetProperty("HasCellValidationError", bindingFlags);
            //Grid level
            PropertyInfo rowErrorInfo = this.GetType().BaseType.GetProperty("HasRowValidationError", bindingFlags);

            if (cellErrorInfo != null) hasCellValidationError = (bool)cellErrorInfo.GetValue(this, null);
            if (rowErrorInfo != null) hasRowValidationError = (bool)rowErrorInfo.GetValue(this, null);

            base.OnCanExecuteBeginEdit(e);
            if (!e.CanExecute && !hasCellValidationError && hasRowValidationError)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        #region baseOnCanExecuteBeginEdit
        //protected virtual void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        //{
        //    bool canExecute = !IsReadOnly && (CurrentCellContainer != null) && !IsEditingCurrentCell && !IsCurrentCellReadOnly && !HasCellValidationError;

        //    if (canExecute && HasRowValidationError)
        //    {
        //        DataGridCell cellContainer = GetEventCellOrCurrentCell(e);
        //        if (cellContainer != null)
        //        {
        //            object rowItem = cellContainer.RowDataItem;

        //            // When there is a validation error, only allow editing on that row
        //            canExecute = IsAddingOrEditingRowItem(rowItem);
        //        }
        //        else
        //        {
        //            // Don't allow entering edit mode when there is a pending validation error
        //            canExecute = false;
        //        }
        //    }

        //    e.CanExecute = canExecute;
        //    e.Handled = true;
        //}
        #endregion baseOnCanExecuteBeginEdit
    }
}
