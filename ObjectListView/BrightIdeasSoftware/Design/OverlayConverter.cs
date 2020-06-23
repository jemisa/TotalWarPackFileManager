namespace BrightIdeasSoftware.Design
{
    using BrightIdeasSoftware;
    using System;
    using System.ComponentModel;
    using System.Globalization;

    internal class OverlayConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                ImageOverlay overlay = value as ImageOverlay;
                if (overlay != null)
                {
                    if (overlay.Image == null)
                    {
                        return "(none)";
                    }
                    return "(set)";
                }
                TextOverlay overlay2 = value as TextOverlay;
                if (overlay2 != null)
                {
                    if (string.IsNullOrEmpty(overlay2.Text))
                    {
                        return "(none)";
                    }
                    return "(set)";
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

