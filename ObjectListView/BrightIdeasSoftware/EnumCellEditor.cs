namespace BrightIdeasSoftware
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    internal class EnumCellEditor : ComboBox
    {
        public EnumCellEditor(System.Type type)
        {
            base.DropDownStyle = ComboBoxStyle.DropDownList;
            base.ValueMember = "Key";
            ArrayList list = new ArrayList();
            foreach (object obj2 in Enum.GetValues(type))
            {
                list.Add(new ComboBoxItem(obj2, Enum.GetName(type, obj2)));
            }
            base.DataSource = list;
        }
    }
}

