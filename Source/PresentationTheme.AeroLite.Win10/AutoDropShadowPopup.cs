namespace PresentationTheme.AeroLite.Win10
{
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    ///   A <see cref="Popup"/> that automatically adds a
    ///   <see cref="SystemDropShadowChrome"/> around its <see cref="Popup.Child"/>
    ///   element.
    /// </summary>
    public class AutoDropShadowPopup : Popup
    {
        static AutoDropShadowPopup()
        {
            var forType = typeof(AutoDropShadowPopup);
            ChildProperty.OverrideMetadata(
                forType, new FrameworkPropertyMetadata(null, null, CoerceChild));
        }

        #region public Thickness ShadowMargin { get; set; }

        /// <summary>
        ///   Identifies the <see cref="ShadowMargin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShadowMarginProperty =
            DependencyProperty.Register(
                nameof(ShadowMargin),
                typeof(Thickness),
                typeof(AutoDropShadowPopup),
                new PropertyMetadata(new Thickness()));

        /// <summary>
        ///   Gets or sets the <see cref="FrameworkElement.Margin"/> of
        ///   the <see cref="SystemDropShadowChrome"/>.
        /// </summary>
        public Thickness ShadowMargin
        {
            get => (Thickness)GetValue(ShadowMarginProperty);
            set => SetValue(ShadowMarginProperty, value);
        }

        #endregion

        #region public Color ShadowColor { get; set; }

        /// <summary>
        ///   Identifies the <see cref="ShadowColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShadowColorProperty =
            DependencyProperty.Register(
                nameof(ShadowColor),
                typeof(Color),
                typeof(AutoDropShadowPopup),
                new PropertyMetadata(Colors.Transparent));

        /// <summary>
        ///   Gets or sets the <see cref="SystemDropShadowChrome.Color"/> of the
        ///   <see cref="SystemDropShadowChrome"/>.
        /// </summary>
        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        #endregion

        private static object CoerceChild(DependencyObject d, object baseValue)
        {
            var source = (AutoDropShadowPopup)d;
            var shadow = new SystemDropShadowChrome {
                Child = baseValue as UIElement,
                SnapsToDevicePixels = true
            };
            BindingOperations.SetBinding(
                shadow,
                SystemDropShadowChrome.MarginProperty,
                new Binding {
                    Path = new PropertyPath(ShadowMarginProperty),
                    Source = source
                });
            BindingOperations.SetBinding(
                shadow,
                SystemDropShadowChrome.ColorProperty,
                new Binding {
                    Path = new PropertyPath(ShadowColorProperty),
                    Source = source
                });
            return shadow;
        }
    }
}
