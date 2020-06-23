namespace BrightIdeasSoftware
{
    using System;
    using System.Windows.Forms;

    internal class UintUpDown : NumericUpDown
    {
        public UintUpDown()
        {
            base.DecimalPlaces = 0;
            base.Minimum = 0M;
            base.Maximum = 9999999M;
        }

        public uint Value
        {
            get
            {
                return decimal.ToUInt32(base.Value);
            }
            set
            {
                base.Value = new decimal(value);
            }
        }
    }
}

