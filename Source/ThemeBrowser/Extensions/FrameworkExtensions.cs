﻿namespace StyleInspector.Extensions
{
    using System.Windows;
    using System.Windows.Media;

    public static class FrameworkExtensions
    {
        public static T FindVisualParent<T>(this Visual element) where T : Visual
        {
            for (Visual it = element; it != null; it = VisualTreeHelper.GetParent(it) as Visual) {
                var result = it as T;
                if (result != null)
                    return result;
            }

            return default(T);
        }

        public static T EnsureFrozen<T>(this T freezable) where T : Freezable
        {
            if (freezable.CanFreeze)
                freezable.Freeze();
            return freezable;
        }
    }
}