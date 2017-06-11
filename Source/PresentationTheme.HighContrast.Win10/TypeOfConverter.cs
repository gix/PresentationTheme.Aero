namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>Converts a value to its <see cref="Type"/>.</summary>
    public sealed class TypeOfConverter : IValueConverter
    {
        /// <summary>Converts a value to its <see cref="Type"/>.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">
        ///   The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        ///   The converter parameter to use. <b>Not used.</b>
        /// </param>
        /// <param name="culture">
        ///   The culture to use in the converter. <b>Not used.</b>
        /// </param>
        /// <returns>
        ///   The <see cref="Type"/> of <paramref name="value"/> or
        ///   <see langword="null"/> if <paramref name="value"/> is
        ///   <see langword="null"/>.
        /// </returns>
        public object Convert(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType();
        }

        object IValueConverter.ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
