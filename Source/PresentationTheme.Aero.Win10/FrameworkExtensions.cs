namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    internal static class FrameworkExtensions
    {
        public static TAncestor FindAncestor<TAncestor>(this DependencyObject obj)
            where TAncestor : DependencyObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            for (obj = obj.GetVisualOrLogicalParent(); obj != null;
                 obj = obj.GetVisualOrLogicalParent()) {
                var ancestor = obj as TAncestor;
                if (ancestor != null)
                    return ancestor;
            }

            return null;
        }

        public static DependencyObject GetVisualOrLogicalParent(
            this DependencyObject sourceElement)
        {
            if (sourceElement == null)
                return null;
            if (sourceElement is Visual)
                return VisualTreeHelper.GetParent(sourceElement) ??
                       LogicalTreeHelper.GetParent(sourceElement);
            return LogicalTreeHelper.GetParent(sourceElement);
        }
    }
}
