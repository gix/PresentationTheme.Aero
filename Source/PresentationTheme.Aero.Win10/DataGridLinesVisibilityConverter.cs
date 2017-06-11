namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    ///   Converts a <see cref="DataGridGridLinesVisibility"/> to a
    ///   <see cref="Visibility"/>.
    /// </summary>
    public sealed class DataGridLinesVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///   Converts a <see cref="DataGridGridLinesVisibility"/> value to a
        ///   <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">
        ///   The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        ///   The <see cref="DataGridGridLinesVisibility"/> required for visibility.
        /// </param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///   <see cref="Visibility.Visible"/> if the <paramref name="value"/>
        ///   matches the required <paramref name="parameter"/>. A value of
        ///   <see cref="DataGridGridLinesVisibility.All"/> counts as horizontal
        ///   or vertical. Otherwise, or if <paramref name="value"/> is
        ///   <see cref="DataGridGridLinesVisibility.None"/>, returns
        ///   <see cref="Visibility.Collapsed"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DataGridGridLinesVisibility) ||
                !(parameter is DataGridGridLinesVisibility) ||
                targetType != typeof(Visibility))
                return DependencyProperty.UnsetValue;

            var gridLinesVisibility = (DataGridGridLinesVisibility)value;

            bool visible = false;
            switch ((DataGridGridLinesVisibility)parameter) {
                case DataGridGridLinesVisibility.Horizontal:
                    visible =
                        gridLinesVisibility == DataGridGridLinesVisibility.Horizontal ||
                        gridLinesVisibility == DataGridGridLinesVisibility.All;
                    break;

                case DataGridGridLinesVisibility.Vertical:
                    visible =
                        gridLinesVisibility == DataGridGridLinesVisibility.Vertical ||
                        gridLinesVisibility == DataGridGridLinesVisibility.All;
                    break;

                case DataGridGridLinesVisibility.All:
                    visible = gridLinesVisibility == DataGridGridLinesVisibility.All;
                    break;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
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
}
