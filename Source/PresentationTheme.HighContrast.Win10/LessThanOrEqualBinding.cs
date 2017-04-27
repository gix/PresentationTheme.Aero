namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class LessThanOrEqualBinding : MarkupExtension, IValueConverter
    {
        private readonly string path;

        public LessThanOrEqualBinding(string path)
        {
            this.path = path;
        }

        public double Threshold { get; set; }
        public string ElementName { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding(path) {
                ElementName = ElementName,
                Converter = this
            };
        }

        object IValueConverter.Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value <= Threshold;
        }

        object IValueConverter.ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
