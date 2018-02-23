namespace ThemeBrowser.Extensions
{
    using System.Windows;
    using System.Windows.Media;

    public static class FrameworkExtensions
    {
        public static T FindVisualParent<T>(this Visual element) where T : Visual
        {
            for (Visual it = element; it != null; it = VisualTreeHelper.GetParent(it) as Visual) {
                if (it is T result)
                    return result;
            }

            return default;
        }

        public static T EnsureFrozen<T>(this T freezable) where T : Freezable
        {
            if (freezable.CanFreeze)
                freezable.Freeze();
            return freezable;
        }
    }
}
