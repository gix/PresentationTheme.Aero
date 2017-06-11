namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    ///   Creates a theme-specific look for <see cref="ScrollBar"/> elements.
    /// </summary>
    /// <remarks>
    ///   The actual appearance of <see cref="ScrollBar"/> elements is dependent
    ///   on which theme is active on the user's system. The properties of this
    ///   class allow WPF to set the appearance based on the current theme.
    /// </remarks>
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Pressed")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Hover")]
    public class ScrollBarChrome : Decorator
    {
        private Pen lightPenCache;
        private Pen darkPenCache;

        /// <summary>
        ///   Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///   Gets or sets the orientation of the scrollbar chrome.
        /// </summary>
        /// <value>
        ///   An <see cref="System.Windows.Controls.Orientation"/> enumeration
        ///   value that defines whether the <see cref="ScrollBarChrome"/> is
        ///   displayed horizontally or vertically. The default is
        ///   <see cref="System.Windows.Controls.Orientation.Horizontal"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="OrientationProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsMeasure"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="Background"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        ///   Gets or sets the background <see cref="Brush"/>.
        /// </summary>
        /// <value>
        ///   The <see cref="Brush"/> that draws the background. This property
        ///   has no default value.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="BackgroundProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.SubPropertiesDoNotAffectRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="LightBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LightBorderBrushProperty =
            DependencyProperty.Register(
                nameof(LightBorderBrush),
                typeof(Brush),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    OnClearPenCache));

        /// <summary>
        ///   Gets or sets the light border <see cref="Brush"/>.
        /// </summary>
        /// <value>
        ///   The <see cref="Brush"/> for the light border. This property has no
        ///   default value.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="LightBorderBrushProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.SubPropertiesDoNotAffectRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Brush LightBorderBrush
        {
            get => (Brush)GetValue(LightBorderBrushProperty);
            set => SetValue(LightBorderBrushProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="DarkBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DarkBorderBrushProperty =
            DependencyProperty.Register(
                nameof(DarkBorderBrush),
                typeof(Brush),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    OnClearPenCache));

        /// <summary>
        ///   Gets or sets the dark border <see cref="Brush"/>.
        /// </summary>
        /// <value>
        ///   The <see cref="Brush"/> for the dark border. This property has no
        ///   default value.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="DarkBorderBrushProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.SubPropertiesDoNotAffectRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Brush DarkBorderBrush
        {
            get => (Brush)GetValue(DarkBorderBrushProperty);
            set => SetValue(DarkBorderBrushProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="RenderHover"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderHoverProperty =
            DependencyProperty.Register(
                nameof(RenderHover),
                typeof(bool),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnUpdateVisualState));

        /// <summary>
        ///   Gets or sets a value indicating whether the scrollbar chrome should
        ///   render a hover state.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the scrollbar chrome appears in a hover
        ///   state; otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="RenderHoverProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool RenderHover
        {
            get => (bool)GetValue(RenderHoverProperty);
            set => SetValue(RenderHoverProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="ParentElement"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ParentElementProperty =
            DependencyProperty.Register(
                nameof(ParentElement),
                typeof(FrameworkElement),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnUpdateVisualState));

        /// <summary>
        ///   Gets or sets the parent <see cref="FrameworkElement"/>.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="ParentElementProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <b>true</b></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public FrameworkElement ParentElement
        {
            get => (FrameworkElement)GetValue(ParentElementProperty);
            set => SetValue(ParentElementProperty, value);
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size constraint)
        {
            var combined = Orientation == Orientation.Vertical
                ? new Size(2, 0) : new Size(0, 2);

            var child = Child;
            if (child == null)
                return combined;

            Size childConstraint = new Size(
                Math.Max(0, constraint.Width - combined.Width),
                Math.Max(0, constraint.Height - combined.Height));
            child.Measure(childConstraint);

            Size childSize = child.DesiredSize;
            return new Size(
                childSize.Width + combined.Width,
                childSize.Height + combined.Height);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement child = Child;
            if (child != null) {
                Rect finalRect;

                if (Orientation == Orientation.Vertical) {
                    finalRect = new Rect(
                        1,
                        0,
                        Math.Max(0, finalSize.Width - 2),
                        Math.Max(0, finalSize.Height));
                } else {
                    finalRect = new Rect(
                        0,
                        1,
                        Math.Max(0, finalSize.Width),
                        Math.Max(0, finalSize.Height - 2));
                }

                child.Arrange(finalRect);
            }

            return finalSize;
        }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext dc)
        {
            var renderSize = RenderSize;
            var actualWidth = renderSize.Width;
            var actualHeight = renderSize.Height;
            var vertical = Orientation == Orientation.Vertical;
            var thickness = 1.0;
            var offset = thickness * 0.5;

            Brush lightBorderBrush = LightBorderBrush;
            if (lightBorderBrush != null) {
                Pen lightPen = lightPenCache;
                if (lightPen == null) {
                    lightPen = new Pen {
                        Brush = lightBorderBrush,
                        Thickness = thickness
                    };
                    if (lightBorderBrush.IsFrozen)
                        lightPen.Freeze();
                    lightPenCache = lightPen;
                }

                if (vertical)
                    dc.DrawLine(
                        lightPen,
                        new Point(offset, 0),
                        new Point(offset, actualHeight));
                else
                    dc.DrawLine(
                        lightPen,
                        new Point(0, offset),
                        new Point(actualWidth, offset));
            }

            Brush darkBorderBrush = DarkBorderBrush;
            if (darkBorderBrush != null) {
                var darkPen = darkPenCache;
                if (darkPen == null) {
                    darkPen = new Pen {
                        Brush = darkBorderBrush,
                        Thickness = thickness
                    };
                    if (darkBorderBrush.IsFrozen)
                        darkPen.Freeze();
                    darkPenCache = darkPen;
                }

                if (vertical)
                    dc.DrawLine(
                        darkPen,
                        new Point(actualWidth - offset, 0),
                        new Point(actualWidth - offset, actualHeight));
                else
                    dc.DrawLine(
                        darkPen,
                        new Point(0, actualHeight - offset),
                        new Point(actualWidth, actualHeight - offset));
            }

            Brush background = Background;
            if (background != null) {
                Point topLeft, bottomRight;
                if (vertical) {
                    topLeft = new Point(thickness, 0);
                    bottomRight = new Point(actualWidth - thickness, actualHeight);
                } else {
                    topLeft = new Point(0, thickness);
                    bottomRight = new Point(actualWidth, actualHeight - thickness);
                }

                if (bottomRight.X > topLeft.X && bottomRight.Y > topLeft.Y)
                    dc.DrawRectangle(background, null, new Rect(topLeft, bottomRight));
            }
        }

        /// <summary>Changes the visual state of the scrollbar chrome.</summary>
        /// <param name="useTransitions">
        ///   <see langword="true"/> to use a <see cref="VisualTransition"/>
        ///   object to transition between states; otherwise <see langword="false"/>.
        /// </param>
        /// <seealso cref="VisualStateManager.GoToState"/>
        protected virtual void ChangeVisualState(bool useTransitions)
        {
            if (ParentElement == null)
                return;

            var thumb = ParentElement as Thumb;
            var button = ParentElement as ButtonBase;
            if (!ParentElement.IsEnabled)
                VisualStateManager.GoToState(ParentElement, "Disabled", useTransitions);
            else if (ParentElement.IsMouseOver)
                VisualStateManager.GoToState(ParentElement, "MouseOver", useTransitions);
            else if (thumb?.IsDragging ?? button?.IsPressed ?? false)
                VisualStateManager.GoToState(ParentElement, "Pressed", useTransitions);
            else if (RenderHover)
                VisualStateManager.GoToState(ParentElement, "Hover", useTransitions);
            else
                VisualStateManager.GoToState(ParentElement, "Normal", useTransitions);
        }

        private static void OnUpdateVisualState(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ScrollBarChrome)d;
            source.UpdateVisualState();
        }

        private void UpdateVisualState(bool useTransitions = true)
        {
            ChangeVisualState(useTransitions);
        }

        private static void OnClearPenCache(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ScrollBarChrome)d;
            source.lightPenCache = null;
            source.darkPenCache = null;
        }
    }
}
