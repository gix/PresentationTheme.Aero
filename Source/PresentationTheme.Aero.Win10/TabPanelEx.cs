namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    /// <summary>
    ///   A <see cref="TabPanel"/> that sets the <see cref="TabItemKind"/>
    ///   attached dependency property on contained <see cref="TabItem"/>
    ///   elements.
    /// </summary>
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

        /// <summary>
        ///   Identifies the <c>TabItemKind</c> attached dependency property.
        /// </summary>
        public static readonly DependencyProperty TabItemKindProperty =
            DependencyProperty.RegisterAttached(
                "TabItemKind",
                typeof(TabItemKind),
                typeof(TabPanelEx),
                new PropertyMetadata(TabItemKind.Selected));

        /// <summary>
        ///   Gets the <see cref="TabItemKind"/> of the specified <see cref="TabItem"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TabItemKind GetTabItemKind(TabItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            return (TabItemKind)item.GetValue(TabItemKindProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="kind"></param>
        public static void SetTabItemKind(TabItem item, TabItemKind kind)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            item.SetValue(TabItemKindProperty, kind);
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
