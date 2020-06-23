namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    internal class IntUpDown : NumericUpDown
    {
        public IntUpDown()
        {
            base.DecimalPlaces = 0;
            base.Minimum = -9999999M;
            base.Maximum = 9999999M;
        }

        public int Value
        {
            get
            {
                return decimal.ToInt32(base.Value);
            }
            set
            {
                base.Value = new decimal(value);
            }
        }
    }
}

