namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public class TabPanelEx : TabPanel
    {
        #region public object SelectedTab { get; set; }

        /// <summary>
        ///   Identifies the <see cref="SelectedTab"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedTabProperty =
            DependencyProperty.Register(
                nameof(SelectedTab),
                typeof(object),
                typeof(TabPanelEx),
                new PropertyMetadata(null, OnSelectedTabChanged));

        /// <summary>
        ///   Gets or sets the selected tab.
        /// </summary>
        public object SelectedTab
        {
            get => (object)GetValue(SelectedTabProperty);
            set => SetValue(SelectedTabProperty, value);
        }

        #endregion

        private static readonly DependencyPropertyKey TabItemKindPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "TabItemKind",
                typeof(TabItemKind),
                typeof(TabPanelEx),
                new PropertyMetadata(TabItemKind.Left));

        /// <summary>
        ///   Identifies the <c>TabItemKind</c> attached dependency property.
        /// </summary>
        public static readonly DependencyProperty TabItemKindProperty =
            TabItemKindPropertyKey.DependencyProperty;

        /// <summary>
        ///   Gets the <see cref="TabItemKind"/> of the specified <see cref="TabItem"/>.
        /// </summary>
        /// <param name="item">The tab item.</param>
        /// <returns>The <see cref="TabItemKind"/>.</returns>
        public static TabItemKind GetTabItemKind(TabItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            return (TabItemKind)item.GetValue(TabItemKindProperty);
        }

        private static void SetTabItemKind(TabItem item, TabItemKind kind)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));
            item.SetValue(TabItemKindPropertyKey, kind);
        }

        private static void OnSelectedTabChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var source = (TabPanelEx)d;

            var kind = TabItemKind.Left;
            foreach (TabItem child in source.Children) {
                if (child.IsSelected) {
                    SetTabItemKind(child, TabItemKind.Selected);
                    kind = TabItemKind.Right;
                } else {
                    SetTabItemKind(child, kind);
                }
            }
        }
    }

    public enum TabItemKind
    {
        None,
        Left,
        Selected,
        Right
    }
}
