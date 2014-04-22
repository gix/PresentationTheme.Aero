namespace StyleInspector
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class DoubleMultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double product = 1;
            foreach (var value in values) {
                if (value == DependencyProperty.UnsetValue)
                    return DependencyProperty.UnsetValue;
                product *= (double)value;
            }

            return product;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
