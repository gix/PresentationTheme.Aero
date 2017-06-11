namespace PresentationTheme.Aero
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///   Provides attached options for the <see cref="TreeView"/> control.
    /// </summary>
    public static class TreeViewService
    {
        #region public int ItemLevel { get; set; }

        /// <summary>
        ///   Identifies the ItemLevel read-only attached dependency property
        ///   key.
        /// </summary>
        private static readonly DependencyPropertyKey ItemLevelPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ItemLevel",
                typeof(int),
                typeof(TreeViewService),
                new PropertyMetadata(0));

        /// <summary>
        ///   Identifies the ItemLevel read-only attached dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemLevelProperty =
            ItemLevelPropertyKey.DependencyProperty;

        /// <summary>Gets the item level.</summary>
        public static int GetItemLevel(DependencyObject d)
        {
            return (int)d.GetValue(ItemLevelProperty);
        }

        /// <summary>Sets the item level.</summary>
        internal static void SetItemLevel(DependencyObject d, int value)
        {
            d.SetValue(ItemLevelPropertyKey, value);
        }

        #endregion

        #region public bool FullRowSelect { get; set; }

        /// <summary>
        ///   Identifies the FullRowSelect attached dependency property.
        /// </summary>
        public static readonly DependencyProperty FullRowSelectProperty =
            DependencyProperty.RegisterAttached(
                "FullRowSelect",
                typeof(bool),
                typeof(TreeViewService),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        ///   Gets a flag indicating whether a <see cref="TreeView"/> style
        ///   should use full-row selection if supported.
        /// </summary>
        public static bool GetFullRowSelect(TreeView d)
        {
            return (bool)d.GetValue(FullRowSelectProperty);
        }

        /// <summary>
        ///   Sets a flag indicating whether a <see cref="TreeView"/> style
        ///   should use full-row selection if supported.
        /// </summary>
        public static void SetFullRowSelect(TreeView d, bool value)
        {
            d.SetValue(FullRowSelectProperty, value);
        }

        #endregion
    }
}
