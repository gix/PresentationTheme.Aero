namespace PresentationTheme.Aero
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ThemeOptions
    {
        #region public bool UseExplorerStyle { get; set; }

        /// <summary>
        ///   Identifies the UseExplorerStyle attached dependency property.
        /// </summary>
        public static readonly DependencyProperty UseExplorerStyleProperty =
            DependencyProperty.RegisterAttached(
                "UseExplorerStyle",
                typeof(bool),
                typeof(ThemeOptions),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        ///   Gets a flag indicating whether the <see cref="Control"/> uses the
        ///   Explorer style.
        /// </summary>
        public static bool GetUseExplorerStyle(Control d)
        {
            return (bool)d.GetValue(UseExplorerStyleProperty);
        }

        /// <summary>
        ///   Sets a flag indicating whether the <see cref="Control"/> uses the
        ///   Explorer style.
        /// </summary>
        public static void SetUseExplorerStyle(Control d, bool value)
        {
            d.SetValue(UseExplorerStyleProperty, value);
        }

        #endregion
    }
}
