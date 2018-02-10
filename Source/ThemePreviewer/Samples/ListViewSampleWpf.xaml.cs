namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Media;
    using PresentationTheme.Aero.Win10;

    public partial class ListViewSampleWpf : IOptionControl
    {
        private readonly OptionList options;

        public ListViewSampleWpf()
        {
            InitializeComponent();

            foreach (var item in ItemGenerator.Generate()) {
                var tuple = Tuple.Create(item[0], item[1], item[2]);
                lv1.Items.Add(tuple);
                lv2.Items.Add(tuple);
            }

            options = new OptionList();
            options.AddOption("Enabled", lv1, l => l.IsEnabled);
            options.AddOption("Enabled", lv2, l => l.IsEnabled);

            options.Add(new GenericBoolOption(
                "GridView", () => lv1.View is GridView,
                v => {
                    lv1.View = v ? gridView1 : null;
                    lv2.View = v ? gridView2 : null;
                }));
        }

        public IReadOnlyList<Option> Options => options;
    }

    public static class FrameworkExtensions
    {
        public static TAncestor FindAncestor<TAncestor>(this DependencyObject obj)
            where TAncestor : DependencyObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            for (obj = obj.GetAnyParent(); obj != null;
                 obj = obj.GetAnyParent()) {
                if (obj is TAncestor ancestor)
                    return ancestor;
            }

            return null;
        }

        private static readonly PropertyInfo InheritanceContextProperty =
            typeof(DependencyObject).GetProperty(
                "InheritanceContext", BindingFlags.NonPublic | BindingFlags.Instance);

        public static DependencyObject GetAnyParent(this DependencyObject sourceElement)
        {
            if (sourceElement == null)
                return null;
            if (sourceElement is Visual) {
                var parent = VisualTreeHelper.GetParent(sourceElement);
                if (parent != null)
                    return parent;
            }

            return LogicalTreeHelper.GetParent(sourceElement) ??
                   InheritanceContextProperty.GetValue(sourceElement, null) as DependencyObject;
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

        public static T FindDescendant<T>(this DependencyObject obj) where T : class
        {
            if (obj == null)
                return default(T);

            for (int i = 0, e = VisualTreeHelper.GetChildrenCount(obj); i < e; ++i) {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child == null)
                    continue;

                var d = (child as T) ?? child.FindDescendant<T>();
                if (d != null)
                    return d;
            }

            return null;
        }
    }
}
