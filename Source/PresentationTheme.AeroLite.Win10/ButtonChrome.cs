namespace PresentationTheme.AeroLite.Win10
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ButtonChrome : ContentControl
    {
        #region public bool RenderEnabled { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderEnabledProperty =
            DependencyProperty.Register(
                nameof(RenderEnabled),
                typeof(bool),
                typeof(ButtonChrome),
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

        #region public bool RenderHot { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderHot"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderHotProperty =
            DependencyProperty.Register(
                nameof(RenderHot),
                typeof(bool),
                typeof(ButtonChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the button chrome shows a hot state.
        /// </summary>
        public bool RenderHot
        {
            get => (bool)GetValue(RenderHotProperty);
            set => SetValue(RenderHotProperty, value);
        }

        #endregion

        #region public bool RenderPressed { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderPressed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderPressedProperty =
            DependencyProperty.Register(
                nameof(RenderPressed),
                typeof(bool),
                typeof(ButtonChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the button chrome shows a pressed state.
        /// </summary>
        public bool RenderPressed
        {
            get => (bool)GetValue(RenderPressedProperty);
            set => SetValue(RenderPressedProperty, value);
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
                typeof(ButtonChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the button chrome shows a focused state.
        /// </summary>
        public bool RenderFocused
        {
            get => (bool)GetValue(RenderFocusedProperty);
            set => SetValue(RenderFocusedProperty, value);
        }

        #endregion

        #region public bool RenderDefaulted { get; set; }

        /// <summary>
        ///   Identifies the <see cref="RenderDefaulted"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderDefaultedProperty =
            DependencyProperty.Register(
                nameof(RenderDefaulted),
                typeof(bool),
                typeof(ButtonChrome),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        /// <summary>
        ///   Gets or sets whether the button chrome shows a defaulted state.
        /// </summary>
        public bool RenderDefaulted
        {
            get => (bool)GetValue(RenderDefaultedProperty);
            set => SetValue(RenderDefaultedProperty, value);
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
            var control = d as ButtonChrome;
            control?.UpdateVisualState();
        }

        public void UpdateVisualState(bool useTransitions = true)
        {
            ChangeVisualState(useTransitions);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (!RenderEnabled)
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            else if (RenderPressed)
                VisualStateManager.GoToState(this, "Pressed", useTransitions);
            else if (RenderHot)
                VisualStateManager.GoToState(this, "Hot", useTransitions);
            else if (RenderDefaulted || RenderFocused)
                VisualStateManager.GoToState(this, "Defaulted", useTransitions);
            else
                VisualStateManager.GoToState(this, "Normal", useTransitions);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }
    }
}
