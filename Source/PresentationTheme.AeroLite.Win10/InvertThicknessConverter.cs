namespace PresentationTheme.AeroLite.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public sealed class InvertThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness)
                return Invert((Thickness)value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness)
                return Invert((Thickness)value);
            return value;
        }

        private static Thickness Invert(Thickness value)
        {
            return new Thickness(-value.Left, -value.Top, -value.Right, -value.Bottom);
        }
    }
}
