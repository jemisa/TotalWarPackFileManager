namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    internal class BooleanCellEditor : ComboBox
    {
        public BooleanCellEditor()
        {
            base.DropDownStyle = ComboBoxStyle.DropDownList;
            base.ValueMember = "Key";
            ArrayList list = new ArrayList();
            list.Add(new ComboBoxItem(false, "False"));
            list.Add(new ComboBoxItem(true, "True"));
            base.DataSource = list;
        }
    }
}

