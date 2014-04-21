namespace Windows.Aero2
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    internal sealed class TypeOfConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.GetType() : null;
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
