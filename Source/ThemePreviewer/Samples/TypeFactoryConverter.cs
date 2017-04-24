namespace ThemePreviewer.Samples
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class TypeFactoryConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value as Type;
            if (type == null)
                return Binding.DoNothing;

            return Activator.CreateInstance(type);
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;
            return value.GetType();
        }
    }
}
