using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DBTableControl
{
    /// <summary>
    /// Interaction logic for ApplyExpressionWindow.xaml
    /// </summary>
    public partial class ApplyExpressionWindow : Window
    {
        public string EnteredExpression { get { return textBox1.Text; } }

        public ApplyExpressionWindow()
        {
            InitializeComponent();

            // Force move the initial focus to the next element, i.e. the input textbox.
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            
        }

        private void ApplyExpButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }

    [ValueConversion(typeof(int), typeof(int))]
    public class ExpConverter : MarkupExtension, IValueConverter
    {
        private static ExpConverter _converter = null;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
            {
                _converter = new ExpConverter();
            }
            return _converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double maxwidth = (double)value - 60;
            if (maxwidth >= 0)
            {
                return maxwidth;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double maxwidth = (double)value - 60;
            if (maxwidth >= 0)
            {
                return maxwidth;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
