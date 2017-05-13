namespace PresentationTheme.Aero
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

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

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
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
