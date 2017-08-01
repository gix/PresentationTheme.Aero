namespace PresentationTheme.Aero.Win10
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///   Creates a theme-specific look for <see cref="Button"/> elements.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The actual appearance of a <see cref="Button"/> is dependent on which
    ///     theme is active on the user's system. The properties of this class
    ///     allow WPF to set the appearance based on the current theme.
    ///   </para>
    ///   <para>
    ///     <see cref="ButtonChrome"/> does not actually render a button but
    ///     instead provides visual states allowing its <see cref="Control.Template"/>
    ///     to accurately render the appearance of buttons.
    ///   </para>
    /// </remarks>
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Hot")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Defaulted")]
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
        ///   Gets or sets a value indicating whether the button chrome should
        ///   render an enabled state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the button chrome appears enabled; otherwise
        ///   <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderEnabledProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
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
        ///   Gets or sets a value indicating whether the button chrome should
        ///   render a hot state (i.e., as if the mouse is over it).
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the button chrome appears as if the mouse
        ///   is over it; otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderHotProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
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
        ///   Gets or sets a value indicating whether the button chrome should
        ///   render a pressed state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the button chrome appears pressed; otherwise
        ///   <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderPressedProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
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
        ///   Gets or sets a value indicating whether the button chrome shows a
        ///   focused state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the button chrome appears focused; otherwise
        ///   <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderFocusedProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
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
        /// <value>
        ///   <see langword="true"/> if the button chrome appears defauled; otherwise
        ///   <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderDefaultedProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool RenderDefaulted
        {
            get => (bool)GetValue(RenderDefaultedProperty);
            set => SetValue(RenderDefaultedProperty, value);
        }

        #endregion

        /// <inheritdoc/>
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

        private void UpdateVisualState(bool useTransitions = true)
        {
            ChangeVisualState(useTransitions);
        }

        /// <summary>Changes the visual state of the button chrome.</summary>
        /// <param name="useTransitions">
        ///   <see langword="true"/> to use a <see cref="VisualTransition"/>
        ///   object to transition between states; otherwise <see langword="false"/>.
        /// </param>
        /// <seealso cref="VisualStateManager.GoToState"/>
        protected virtual void ChangeVisualState(bool useTransitions)
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
    }
}
