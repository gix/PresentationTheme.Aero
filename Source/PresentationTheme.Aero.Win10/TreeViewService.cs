namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

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
        ///   Gets a flag indicating whether the treeview uses full-row select.
        /// </summary>
        public static bool GetFullRowSelect(TreeView d)
        {
            return (bool)d.GetValue(FullRowSelectProperty);
        }

        /// <summary>
        ///   Sets a flag indicating whether the treeview uses full-row select.
        /// </summary>
        public static void SetFullRowSelect(TreeView d, bool value)
        {
            d.SetValue(FullRowSelectProperty, value);
        }

        #endregion
    }

    public class TreeViewItemLevelConverter : DependencyObject, IValueConverter
    {
        private static readonly Lazy<TreeViewItemLevelConverter> LazyInstance =
            new Lazy<TreeViewItemLevelConverter>(() => new TreeViewItemLevelConverter());

        public static TreeViewItemLevelConverter Instance => LazyInstance.Value;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return DependencyProperty.UnsetValue;

            int level = GetItemLevel(item);
            TreeViewService.SetItemLevel(item, level);

            if (parameter is int)
                return level * (int)parameter;

            if (parameter is double)
                return level * (double)parameter;

            return level;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private int GetItemLevel(TreeViewItem item)
        {
            var parent = GetParentItem(item);
            if (parent != null)
                return TreeViewService.GetItemLevel(parent) + 1;
            return 0;
        }

        private static TreeViewItem GetParentItem(TreeViewItem item)
        {
            DependencyObject obj = item;
            for (obj = obj.GetVisualOrLogicalParent(); obj != null;
                 obj = obj.GetVisualOrLogicalParent()) {
                var parentItem = obj as TreeViewItem;
                if (parentItem != null)
                    return parentItem;
                if (obj is TreeView)
                    break;
            }

            return null;
        }
    }
}
