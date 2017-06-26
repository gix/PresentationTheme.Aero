namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>A converter that inverts a <see cref="Thickness"/> value.</summary>
    [ValueConversion(typeof(Thickness), typeof(Thickness))]
    public sealed class InvertThicknessConverter : IValueConverter
    {
        /// <summary>Inverts a <see cref="Thickness"/> value.</summary>
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
        /// <returns>The inverted <see cref="Thickness"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness)
                return Invert((Thickness)value);
            return value;
        }

        /// <summary>Inverts a <see cref="Thickness"/> value.</summary>
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
        /// <returns>The inverted <see cref="Thickness"/>.</returns>
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
