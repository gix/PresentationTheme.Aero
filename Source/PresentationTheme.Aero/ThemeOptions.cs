namespace PresentationTheme.Aero
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>Provides options to customize theme styles.</summary>
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
        ///   Gets a value indicating whether a <see cref="Control"/> should use
        ///   the Explorer style if supported.
        /// </summary>
        /// <param name="control">
        ///   The control from which to read the property value.
        /// </param>
        /// <remarks>
        ///   Supported controls are <see cref="ListView"/> and <see cref="TreeView"/>.
        /// </remarks>
        public static bool GetUseExplorerStyle(Control control)
        {
            return (bool)control.GetValue(UseExplorerStyleProperty);
        }

        /// <summary>
        ///   Sets a value indicating whether a <see cref="Control"/> should use
        ///   the Explorer style if supported.
        /// </summary>
        /// <param name="control">
        ///   The control on which to set the attached property.
        /// </param>
        /// <param name="value">The property value to set.</param>
        /// <remarks>
        ///   Supported controls are <see cref="ListView"/> and <see cref="TreeView"/>.
        /// </remarks>
        public static void SetUseExplorerStyle(Control control, bool value)
        {
            control.SetValue(UseExplorerStyleProperty, value);
        }

        #endregion
    }
}
