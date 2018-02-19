namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///   Converts a <see cref="Thickness"/> to another <see cref="Thickness"/>
    ///   by only using the values (<see cref="Thickness.Left"/> etc.) of the
    ///   source <see cref="Thickness"/> as indicated by a <see cref="ThicknessMask"/>
    ///   parameter. All other values are set to zero.
    /// </summary>
    [ValueConversion(typeof(Thickness), typeof(Thickness), ParameterType = typeof(ThicknessMask))]
    public sealed class ThicknessMaskConverter : IValueConverter
    {
        /// <summary>
        ///   Converts a <see cref="Thickness"/> value to a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">
        ///   The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        ///   A <see cref="ThicknessMask"/> indicating which values of the source
        ///   value to keep.
        /// </param>
        /// <param name="culture">Not used.</param>
        /// <returns>
        ///   The source <see cref="Thickness"/> with all values not indicating
        ///   by the <see cref="ThicknessMask"/> <paramref name="parameter"/>
        ///   replaced by zero.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Thickness source) ||
                !(parameter is ThicknessMask mask) ||
                targetType != typeof(Thickness))
                return DependencyProperty.UnsetValue;

            return new Thickness(
                (mask & ThicknessMask.Left) != 0 ? source.Left : 0,
                (mask & ThicknessMask.Top) != 0 ? source.Top : 0,
                (mask & ThicknessMask.Right) != 0 ? source.Right : 0,
                (mask & ThicknessMask.Bottom) != 0 ? source.Bottom : 0);
        }

        /// <summary><b>Not supported.</b></summary>
        /// <param name="value">Not used.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Not used.</returns>
        /// <exception cref="NotSupportedException">Always.</exception>
        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    ///   Mask flags used by <see cref="ThicknessMaskConverter"/>.
    /// </summary>
    [Flags]
    public enum ThicknessMask
    {
        /// <summary>Use none of the source <see cref="Thickness"/>.</summary>
        None,

        /// <summary>Use <see cref="Thickness.Left"/>.</summary>
        Left = 1,

        /// <summary>Use <see cref="Thickness.Top"/>.</summary>
        Top = 2,

        /// <summary>Use <see cref="Thickness.Right"/>.</summary>
        Right = 4,

        /// <summary>Use <see cref="Thickness.Bottom"/>.</summary>
        Bottom = 8,

        /// <summary>
        ///   Use all of values of the source <see cref="Thickness"/>.
        /// </summary>
        All = Left | Top | Right | Bottom,

        /// <summary>Use all but <see cref="Thickness.Left"/>.</summary>
        NotLeft = Top | Right | Bottom,

        /// <summary>Use all but <see cref="Thickness.Top"/>.</summary>
        NotTop = Left | Right | Bottom,

        /// <summary>Use all but <see cref="Thickness.Right"/>.</summary>
        NotRight = Left | Top | Bottom,

        /// <summary>Use all but <see cref="Thickness.Bottom"/>.</summary>
        NotBottom = Left | Top | Right,
    }
}
