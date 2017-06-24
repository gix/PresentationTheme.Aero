namespace ThemePreviewer.Samples
{
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;

    public partial class CombinedSampleWpf
    {
        public CombinedSampleWpf()
        {
            InitializeComponent();

            menuPopupContainer.Child = RemovePopupChild(fileMenuItem);
            fileMenuItem.ForceIsMouseOver = true;
            newMenuItem.IsHighlighted = true;
        }

        private UIElement RemovePopupChild(MenuItem item)
        {
            item.ApplyTemplate();

            var popup = item.Template?.FindName("PART_Popup", item) as Popup;
            if (popup?.Child == null)
                return null;

            var shadowChrome = popup.Child as Decorator;

            UIElement child;
            if (shadowChrome != null && shadowChrome.GetType().Name.Contains("SystemDropShadowChrome")) {
                child = shadowChrome.Child;
                shadowChrome.Child = null;
            } else {
                child = popup.Child;
                popup.Child = null;
            }

            return child;
        }
    }

    public class MenuItemEx : MenuItem
    {
        private static readonly DependencyPropertyKey IsMouseOverPropertyKey;

        static MenuItemEx()
        {
            IsMouseOverPropertyKey = (DependencyPropertyKey)typeof(UIElement).GetField(
                "IsMouseOverPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null);
        }

        public static readonly DependencyProperty ForceIsMouseOverProperty =
            DependencyProperty.Register(
                nameof(ForceIsMouseOver),
                typeof(bool?),
                typeof(MenuItemEx),
                new PropertyMetadata(null, OnForceIsMouseOverChanged));

        private static void OnForceIsMouseOverChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var source = (MenuItemEx)d;
            var newValue = (bool?)args.NewValue;
            if (newValue != null)
                source.SetValue(IsMouseOverPropertyKey, newValue.Value);
        }

        public bool ForceIsMouseOver
        {
            get => (bool)GetValue(ForceIsMouseOverProperty);
            set => SetValue(ForceIsMouseOverProperty, value);
        }

        public new bool IsHighlighted
        {
            get => base.IsHighlighted;
            set => base.IsHighlighted = value;
        }
    }
}
