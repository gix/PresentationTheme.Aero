namespace ThemePreviewer
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    public static class ForcedStateProperties
    {
        private static readonly DependencyPropertyKey IsMouseOverPropertyKey;
        private static readonly PropertyInfo ButtonIsPressed;

        static ForcedStateProperties()
        {
            var staticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var instanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            IsMouseOverPropertyKey = (DependencyPropertyKey)typeof(UIElement).GetField(
                    "IsMouseOverPropertyKey", staticFlags)?
                .GetValue(null) ?? throw new Exception("UIElement.IsMouseOverPropertyKey not found");

            ButtonIsPressed = typeof(Button).GetProperty(
                nameof(Button.IsPressed), instanceFlags) ??
                throw new InvalidOperationException("Button.IsPressed not found");
        }

        public static readonly DependencyProperty ForceIsMouseOverProperty =
            DependencyProperty.Register(
                "ForceIsMouseOver",
                typeof(bool?),
                typeof(UIElement),
                new PropertyMetadata(null, OnForceIsMouseOverChanged));

        private static void OnForceIsMouseOverChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var source = (UIElement)d;
            var newValue = (bool?)args.NewValue;
            if (newValue != null)
                source.SetValue(IsMouseOverPropertyKey, newValue.Value);
        }

        public static bool GetForceIsMouseOver(this UIElement uie)
        {
            return (bool)uie.GetValue(ForceIsMouseOverProperty);
        }

        public static void SetForceIsMouseOver(this UIElement uie, bool value)
        {
            uie.SetValue(ForceIsMouseOverProperty, value);
        }

        public static bool GetForceIsPressed(this Button button)
        {
            return (bool)ButtonIsPressed.GetValue(button);
        }

        public static void SetForceIsPressed(this Button button, bool value)
        {
            ButtonIsPressed.SetValue(button, value);
        }
    }
}
