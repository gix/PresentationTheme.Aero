namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///   Data binding converter to handle whether repeat buttons in scrolling
    ///   menus are enabled.
    /// </summary>
    public sealed class MenuScrollingEnabledConverter : IMultiValueConverter
    {
        /// <summary>
        ///   Converts a <see cref="System.Windows.Controls.ScrollViewer"/> state
        ///   to a <see cref="bool"/> value indicating whether a specific scrolled
        ///   percentage is reached.
        /// </summary>
        /// <param name="values">
        ///   <see cref="double"/> values indicating the current state of the
        ///   <see cref="System.Windows.Controls.ScrollViewer"/>.
        ///   <list type="number">
        ///     <item>
        ///       <description>
        ///         <see cref="System.Windows.Controls.ScrollViewer.VerticalOffset"/>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="System.Windows.Controls.ScrollViewer.ExtentHeight"/>
        ///       </description>
        ///     </item>
        ///     <item>
        ///       <description>
        ///         <see cref="System.Windows.Controls.ScrollViewer.ViewportHeight"/>
        ///       </description>
        ///     </item>
        ///   </list>
        /// </param>
        /// <param name="targetType">
        ///   The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        ///   A <see cref="double"/> value indicating the scrolled percentage at
        ///   which the converter returns <see langword="true"/>. Use <c>0.0</c>
        ///   or <c>100.0</c> to target the beginning or end respectively.
        /// </param>
        /// <param name="culture">
        ///   The culture to use in the converter. <b>Not used.</b>
        /// </param>
        /// <returns>
        ///   <see langword="true"/> if the <paramref name="values"/> indicate a
        ///   scrolled percentage that equals the target <paramref name="parameter"/>;
        ///   otherwise <see langword="false"/>.
        /// </returns>
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Type doubleType = typeof(double);
            if (parameter == null ||
                values == null ||
                values.Length != 3 ||
                values[0] == null ||
                values[1] == null ||
                values[2] == null ||
                !doubleType.IsInstanceOfType(values[0]) ||
                !doubleType.IsInstanceOfType(values[1]) ||
                !doubleType.IsInstanceOfType(values[2]) ||
                targetType != typeof(bool))
                return DependencyProperty.UnsetValue;

            Type paramType = parameter.GetType();
            if (!doubleType.IsAssignableFrom(paramType) &&
                !typeof(string).IsAssignableFrom(paramType))
                return DependencyProperty.UnsetValue;

            //
            // Conversion
            //

            double target;
            if (parameter is string str)
                target = double.Parse(str, NumberFormatInfo.InvariantInfo);
            else
                target = (double)parameter;

            double verticalOffset = (double)values[0];
            double extentHeight = (double)values[1];
            double viewportHeight = (double)values[2];

            if (extentHeight != viewportHeight) {
                // Calculate the percent so that we can see if we are near the edge of the range
                double percent = Math.Min(100.0, Math.Max(0.0, (verticalOffset * 100.0 / (extentHeight - viewportHeight))));

                if (DoubleUtil.AreClose(percent, target))
                    return false;
            }

            return true;
        }

        object[] IMultiValueConverter.ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new[] { Binding.DoNothing };
        }
    }
}
