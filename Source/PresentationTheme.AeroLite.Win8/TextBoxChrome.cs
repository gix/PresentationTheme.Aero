namespace PresentationTheme.AeroLite.Win8
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///   Creates a theme-specific look for <see cref="TextBox"/> elements.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The actual appearance of a <see cref="TextBox"/> is dependent on
    ///     which theme is active on the user's system. The properties of this
    ///     class allow WPF to set the appearance based on the current theme.
    ///   </para>
    ///   <para>
    ///     <see cref="TextBoxChrome"/> does not actually render a button but
    ///     instead provides visual states allowing its <see cref="Control.Template"/>
    ///     to accurately render the appearance of buttons.
    ///   </para>
    /// </remarks>
    [TemplateVisualState(GroupName = "BackgroundStates", Name = "BackgroundDisabled")]
    [TemplateVisualState(GroupName = "BackgroundStates", Name = "BackgroundReadOnly")]
    [TemplateVisualState(GroupName = "BackgroundStates", Name = "BackgroundFocused")]
    [TemplateVisualState(GroupName = "BackgroundStates", Name = "BackgroundHot")]
    [TemplateVisualState(GroupName = "BackgroundStates", Name = "BackgroundNormal")]
    [TemplateVisualState(GroupName = "BorderCommonStates", Name = "BorderDisabled")]
    [TemplateVisualState(GroupName = "BorderCommonStates", Name = "BorderFocused")]
    [TemplateVisualState(GroupName = "BorderCommonStates", Name = "BorderHot")]
    [TemplateVisualState(GroupName = "BorderCommonStates", Name = "BorderNormal")]
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
        ///   Gets or sets a value indicating whether the textbox chrome should
        ///   render an enabled state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the textbox chrome appears enabled; otherwise
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
        ///         <term>Metadata properties set to <see langword="true"/></term>
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
        ///   Gets or sets a value indicating whether the textbox chrome should
        ///   render a read-only state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the textbox chrome appears read-only;
        ///   otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderReadOnlyProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>None</description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
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
        ///   Gets or sets a value indicating whether the textbox chrome should
        ///   render a hot state (i.e., as if the mouse is over it).
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the textbox chrome appears as if the mouse
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
        ///         <term>Metadata properties set to <see langword="true"/></term>
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
        /// <value>
        ///   <see langword="true"/> if the textbox chrome appears focused; otherwise
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
        ///         <term>Metadata properties set to <see langword="true"/></term>
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

        /// <inheritdoc/>
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

        private void UpdateVisualState(bool useTransitions = true)
        {
            ChangeVisualState(useTransitions);
        }

        /// <summary>Changes the visual state of the textbox chrome.</summary>
        /// <param name="useTransitions">
        ///   <see langword="true"/> to use a <see cref="VisualTransition"/>
        ///   object to transition between states; otherwise <see langword="false"/>.
        /// </param>
        /// <seealso cref="VisualStateManager.GoToState"/>
        protected virtual void ChangeVisualState(bool useTransitions)
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
