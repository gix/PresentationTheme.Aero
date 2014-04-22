namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public sealed class TypeOfConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
