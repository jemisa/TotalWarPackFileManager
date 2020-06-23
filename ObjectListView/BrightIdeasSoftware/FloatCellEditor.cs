namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    internal class FloatCellEditor : NumericUpDown
    {
        public FloatCellEditor()
        {
            base.DecimalPlaces = 2;
            base.Minimum = -9999999M;
            base.Maximum = 9999999M;
        }

        public double Value
        {
            get
            {
                return Convert.ToDouble(base.Value);
            }
            set
            {
                base.Value = Convert.ToDecimal(value);
            }
        }
    }
}

