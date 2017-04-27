namespace PresentationTheme.AeroLite.Win10
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class EqualsConverter : IMultiValueConverter
    {
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return true;

            var obj = values[0];
            for (int i = 1; i < values.Length; ++i) {
                if (!obj.Equals(values[i]))
                    return false;
            }

            return true;
        }

        public object[] ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
