namespace PresentationTheme.HighContrast.Win8
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    ///   Creates a <see cref="Binding"/> that evaluates whether a specific property
    ///   is less than or equal to a specified <see cref="Threshold"/>.
    /// </summary>
    [ValueConversion(typeof(double), typeof(bool))]
    public class LessThanOrEqualBinding : MarkupExtension, IValueConverter
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="LessThanOrEqualBinding"/>
        ///   class.
        /// </summary>
        /// <param name="path">The <see cref="Path"/> value.</param>
        public LessThanOrEqualBinding(string path)
        {
            Path = path;
        }

        /// <summary>Gets or sets the threshold.</summary>
        public double Threshold { get; set; }

        /// <summary>
        ///   Gets or sets the <see cref="Binding.Path"/> of the
        ///   <see cref="Binding"/> returned by <see cref="ProvideValue"/>.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///   Gets or sets the <see cref="Binding.ElementName"/> of the
        ///   <see cref="Binding"/> returned by <see cref="ProvideValue"/>.
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        ///   Returns a <see cref="Binding"/> that evaluates whether a specific
        ///   property is less than or equal to a specified <see cref="Threshold"/>.
        /// </summary>
        /// <param name="serviceProvider">
        ///   A service provider helper that can provide services for the markup
        ///   extension. <b>Not used.</b>
        /// </param>
        /// <returns>
        ///   The object value to set on the property where the extension is
        ///   applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new Binding(Path) {
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
