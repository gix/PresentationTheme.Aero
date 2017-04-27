namespace PresentationTheme.HighContrast.Win10
{
    using System.Windows;
    using System.Windows.Controls;

    public class TextBoxChrome : ContentControl
    {
        #region public bool RenderEnabled { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderEnabledProperty =
            DependencyProperty.Register(
                nameof(RenderEnabled),
                typeof(bool),
                typeof(TextBoxChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets the isEnabled.
        /// </summary>
        public bool RenderEnabled
        {
            get => (bool)GetValue(RenderEnabledProperty);
            set => SetValue(RenderEnabledProperty, value);
        }

        #endregion

        #region public bool RenderReadOnly { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderReadOnly"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderReadOnlyProperty =
            DependencyProperty.Register(
                nameof(RenderReadOnly),
                typeof(bool),
                typeof(TextBoxChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the textbox chrome shows a read-only state.
        /// </summary>
        public bool RenderReadOnly
        {
            get => (bool)GetValue(RenderReadOnlyProperty);
            set => SetValue(RenderReadOnlyProperty, value);
        }

        #endregion

        #region public bool RenderHot { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderHot"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderHotProperty =
            DependencyProperty.Register(
                nameof(RenderHot),
                typeof(bool),
                typeof(TextBoxChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the textbox chrome shows a hot state.
        /// </summary>
        public bool RenderHot
        {
            get => (bool)GetValue(RenderHotProperty);
            set => SetValue(RenderHotProperty, value);
        }

        #endregion

        #region public bool RenderFocused { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderFocused"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderFocusedProperty =
            DependencyProperty.Register(
                nameof(RenderFocused),
                typeof(bool),
                typeof(TextBoxChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the textbox chrome shows a focused state.
        /// </summary>
        public bool RenderFocused
        {
            get => (bool)GetValue(RenderFocusedProperty);
            set => SetValue(RenderFocusedProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChangeVisualState(false);
        }

        private static void OnVisualStatePropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TextBoxChrome;
            control?.UpdateVisualState();
        }

        public void UpdateVisualState(bool useTransitions = true)
        {
            ChangeVisualState(useTransitions);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (!RenderEnabled)
                VisualStateManager.GoToState(this, "BackgroundDisabled", useTransitions);
            else if (RenderReadOnly)
                VisualStateManager.GoToState(this, "BackgroundReadOnly", useTransitions);
            else if (RenderFocused)
                VisualStateManager.GoToState(this, "BackgroundFocused", useTransitions);
            else if (RenderHot)
                VisualStateManager.GoToState(this, "BackgroundHot", useTransitions);
            else
                VisualStateManager.GoToState(this, "BackgroundNormal", useTransitions);

            if (!RenderEnabled)
                VisualStateManager.GoToState(this, "BorderDisabled", useTransitions);
            else if (RenderFocused)
                VisualStateManager.GoToState(this, "BorderFocused", useTransitions);
            else if (RenderHot)
                VisualStateManager.GoToState(this, "BorderHot", useTransitions);
            else
                VisualStateManager.GoToState(this, "BorderNormal", useTransitions);
        }
    }
}
