namespace PresentationTheme.AeroLite.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///   Converts an array of objects to a boolean specifying whether all objects
    ///   are equal.
    /// </summary>
    /// <example>
    ///   The following <see cref="DataTrigger"/> checks whether the Value
    ///   property is equal to the Maximum property.
    ///   <code>
    ///     &lt;theme:EqualsConverter x:Key="EqualsConverter"/&gt;
    ///     &lt;DataTrigger Value="True"&gt;
    ///       &lt;DataTrigger.Binding&gt;
    ///         &lt;MultiBinding Converter="{StaticResource EqualsConverter}"&gt;
    ///           &lt;Binding Path="Value" RelativeSource="{RelativeSource Self}"/&gt;
    ///           &lt;Binding Path="Maximum" RelativeSource="{RelativeSource Self}"/&gt;
    ///         &lt;/MultiBinding&gt;
    ///       &lt;/DataTrigger.Binding&gt;
    ///     &lt;/DataTrigger&gt;
    ///   </code>
    /// </example>
    public class EqualsConverter : IMultiValueConverter
    {
        /// <summary>
        ///   Converts an array of objects to a boolean specifying whether all
        ///   objects are equal.
        /// </summary>
        /// <param name="values">The array of values.</param>
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
        ///   <see langword="true"/> if all objects in <paramref name="values"/>
        ///   are equal; otherwise <see langword="false"/>.
        /// </returns>
        public object Convert(
            object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
                return true;

            var obj = values[0];
            for (int i = 1; i < values.Length; ++i) {
                if (!Equals(obj, values[i]))
                    return false;
            }

            return true;
        }

        object[] IMultiValueConverter.ConvertBack(
            object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
