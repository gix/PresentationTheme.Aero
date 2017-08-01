namespace ThemePreviewer.Samples
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using Binding = System.Windows.Data.Binding;

    public class TypeFactoryConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value as Type;
            if (type == null) {
                if (value is Control)
                    return value;
                if (value is System.Windows.Forms.Control)
                    return value;
                return Binding.DoNothing;
            }

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
