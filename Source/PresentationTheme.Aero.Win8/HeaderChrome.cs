namespace PresentationTheme.Aero.Win8
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>Creates the theme specific-look for headers.</summary>
    public sealed class HeaderChrome : Border
    {
        private static Lazy<Freezable[]> freezableCache = new Lazy<Freezable[]>(CacheFreezables);

        static HeaderChrome()
        {
            // We always set this to true on these borders, so just default it to true here.
            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(HeaderChrome), new FrameworkPropertyMetadata(true));
        }

        /// <summary>
        ///   Identifies the <see cref="IsHovered"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsHoveredProperty =
            DependencyProperty.Register(
                nameof(IsHovered),
                typeof(bool),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the header appears as
        ///   if the mouse pointer is moved over it.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the header appears as if the mouse pointer
        ///   is moved over it; otherwise, <see langword="false"/>. The registered
        ///   default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="IsHoveredProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool IsHovered
        {
            get => (bool)GetValue(IsHoveredProperty);
            set => SetValue(IsHoveredProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="IsPressed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register(
                nameof(IsPressed),
                typeof(bool),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the header appears
        ///   pressed.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the header appears pressed; otherwise,
        ///   <see langword="false"/>. The registered default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="IsPressedProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsArrange"/>,
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            set => SetValue(IsPressedProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="IsClickable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register(
                nameof(IsClickable),
                typeof(bool),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the header is clickable.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the header is clickable; otherwise,
        ///   <see langword="false"/>. The registered default is <see langword="true"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="IsClickableProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsArrange"/>,
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="ClickOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClickOffsetProperty =
            DependencyProperty.Register(
                nameof(ClickOffset),
                typeof(Vector),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    new Vector(1, 1),
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        ///   Gets or sets the distance the <see cref="P:Border.Child"/> element
        ///   is offset when clicked.
        /// </summary>
        /// <value>
        ///   A <see cref="Vector"/> measuring the distance the
        ///   <see cref="P:Border.Child"/> element is offset when the header is
        ///   clicked. Use an empty <see cref="Vector"/> to disable offsetting.
        ///   The registered default is <c>new Vector { X = 1.0, Y = 1.0 }</c>,
        ///   i.e. a 1-pixel offset in the X and Y direction.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="ClickOffsetProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsArrange"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Vector ClickOffset
        {
            get => (Vector)GetValue(ClickOffsetProperty);
            set => SetValue(ClickOffsetProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="SortDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(
                nameof(SortDirection),
                typeof(ListSortDirection?),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets the header sort direction.
        /// </summary>
        /// <value>
        ///   One of the enumeration values that indicates which direction the
        ///   column is sorted. The registered default is <see langword="null"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="SortDirectionProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the header appears
        ///   selected.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the header appears selected; otherwise,
        ///   <see langword="false"/>. The registered default is <see langword="false"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="IsSelectedProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsRender),
                IsValidOrientation);

        /// <summary>
        ///   Gets or sets whether the header renders in the vertical direction,
        ///   as a column header, or horizontal direction, as a row header.
        /// </summary>
        /// <value>
        ///   One of the enumeration values that indicates which direction the
        ///   header renders. The registered default is
        ///   <see cref="System.Windows.Controls.Orientation.Vertical"/>.
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
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
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

        private static bool IsValidOrientation(object value)
        {
            switch ((Orientation)value) {
                case Orientation.Horizontal:
                case Orientation.Vertical:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///   Identifies the <see cref="SeparatorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(
                nameof(SeparatorBrush),
                typeof(Brush),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        /// <summary>
        ///   Gets or sets the brush that draws the separation between headers.
        /// </summary>
        /// <value>
        ///   The brush that draws the separation between headers. The registered
        ///   default is <see langword="null"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="SeparatorBrushProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.SubPropertiesDoNotAffectRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Brush SeparatorBrush
        {
            get => (Brush)GetValue(SeparatorBrushProperty);
            set => SetValue(SeparatorBrushProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                nameof(SeparatorVisibility),
                typeof(Visibility),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    Visibility.Visible,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the separation between
        ///   headers is visible.
        /// </summary>
        /// <value>
        ///   One of the enumeration values that indicates whether the separator
        ///   is visible. The registered default is <see cref="Visibility.Visible"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="SeparatorVisibilityProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Visibility SeparatorVisibility
        {
            get => (Visibility)GetValue(SeparatorVisibilityProperty);
            set => SetValue(SeparatorVisibilityProperty, value);
        }

        /// <summary>
        ///   Identifies the <see cref="BorderVisibilityProperty"/> dependency
        ///   property.
        /// </summary>
        public static readonly DependencyProperty BorderVisibilityProperty =
            DependencyProperty.Register(
                nameof(BorderVisibility),
                typeof(Visibility),
                typeof(HeaderChrome),
                new FrameworkPropertyMetadata(
                    Visibility.Visible,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets a value that indicates whether the header shows a
        ///   border to an adjacent content cell.
        /// </summary>
        /// <value>
        ///   One of the enumeration values that indicates whether the border is
        ///   visible. The registered default is <see cref="Visibility.Visible"/>.
        /// </value>
        /// <remarks>
        ///   <para>
        ///     <b>Dependency Property Information</b>
        ///     <list type="table">
        ///       <item>
        ///         <term>Identifier field</term>
        ///         <description><see cref="BorderVisibilityProperty"/></description>
        ///       </item>
        ///       <item>
        ///         <term>Metadata properties set to <see langword="true"/></term>
        ///         <description>
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>
        ///         </description>
        ///       </item>
        ///     </list>
        ///   </para>
        /// </remarks>
        public Visibility BorderVisibility
        {
            get => (Visibility)GetValue(BorderVisibilityProperty);
            set => SetValue(BorderVisibilityProperty, value);
        }

        private T GetCachedFreezable<T>(PartState index) where T : Freezable
        {
            return (T)freezableCache.Value[(int)index];
        }

        /// <summary>
        ///   When there is a Background or BorderBrush, revert to the Border
        ///   implementation.
        /// </summary>
        private bool UsingBorderImplementation => Background != null || BorderBrush != null;

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size constraint)
        {
            if (UsingBorderImplementation) {
                // Revert to the Border implementation
                return base.MeasureOverride(constraint);
            }

            UIElement child = Child;
            if (child != null) {
                // Use the public Padding property if it's set
                Thickness padding = Padding;
                if (padding == new Thickness())
                    padding = DefaultPadding;

                double childWidth = constraint.Width;
                double childHeight = constraint.Height;

                // If there is an actual constraint, then reserve space for the chrome
                if (!double.IsInfinity(childWidth))
                    childWidth = Math.Max(0, childWidth - padding.Left - padding.Right);

                if (!double.IsInfinity(childHeight))
                    childHeight = Math.Max(0, childHeight - padding.Top - padding.Bottom);

                child.Measure(new Size(childWidth, childHeight));
                Size desiredSize = child.DesiredSize;

                // Add the reserved space for the chrome
                return new Size(
                    desiredSize.Width + padding.Left + padding.Right,
                    desiredSize.Height + padding.Top + padding.Bottom);
            }

            return new Size();
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (UsingBorderImplementation) {
                // Revert to the Border implementation
                return base.ArrangeOverride(arrangeSize);
            }

            UIElement child = Child;
            if (child != null) {
                // Use the public Padding property if it's set
                Thickness padding = Padding;
                if (padding == new Thickness())
                    padding = DefaultPadding;

                // Reserve space for the chrome
                double childWidth = Math.Max(0, arrangeSize.Width - padding.Left - padding.Right);
                double childHeight = Math.Max(0, arrangeSize.Height - padding.Top - padding.Bottom);

                var offset = IsPressed ? ClickOffset : new Vector();
                child.Arrange(new Rect(
                    padding.Left + offset.X, padding.Top + offset.Y, childWidth, childHeight));
            }

            return arrangeSize;
        }

        /// <summary>
        ///   Returns a default padding for the various themes for use by measure
        ///   and arrange.
        /// </summary>
        private Thickness DefaultPadding
        {
            get
            {
                var padding = new Thickness(3); // The default padding
                if (Orientation == Orientation.Vertical)
                    padding = new Thickness(5, 4, 5, 4);

                // When pressed, offset the child
                if (IsClickable && IsPressed) {
                    padding.Left += 1;
                    padding.Top += 1;
                    padding.Right -= 1;
                    padding.Bottom -= 1;
                }

                return padding;
            }
        }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext dc)
        {
            if (UsingBorderImplementation) {
                base.OnRender(dc);
                RenderThemeExtras(dc);
            } else {
                RenderTheme(dc);
            }
        }

        private void RenderTheme(DrawingContext dc)
        {
            var bounds = new Rect(RenderSize);

            if (Orientation == Orientation.Horizontal) {
                dc.PushTransform(GetCachedFreezable<MatrixTransform>(PartState.SwapXAndYTransform));
                bounds = new Rect(bounds.Y, bounds.X, bounds.Height, bounds.Width);
            }

            if (IsPressed) {
                var borderPen = GetCachedFreezable<Pen>(PartState.BorderPressed);
                var background = GetCachedFreezable<Brush>(PartState.BackgroundPressed);

                var rect = new Rect(new Point(1, 0), new Size(bounds.Width - 2, bounds.Height - 1));
                dc.DrawRectangle(background, null, rect);

                rect = new Rect(new Point(1, 1), new Point(bounds.Width - 1, 2));
                dc.DrawRectangle(GetCachedFreezable<Brush>(PartState.HighlightPressedDark), null, rect);
                rect = new Rect(new Point(1, 2), new Point(bounds.Width - 1, 3));
                dc.DrawRectangle(GetCachedFreezable<Brush>(PartState.HighlightPressedLight), null, rect);

                bounds.Inflate(-0.5, -0.5);
                dc.DrawRectangle(null, borderPen, bounds);
            } else if (IsHovered) {
                var borderPen = GetCachedFreezable<Pen>(PartState.BorderHot);
                var background = GetCachedFreezable<Brush>(PartState.BackgroundHot);

                var rect = new Rect(new Point(2, 1), new Size(bounds.Width - 4, bounds.Height - 3));
                dc.DrawRectangle(background, null, rect);

                rect = new Rect(new Point(1, 0), new Size(bounds.Width - 2, bounds.Height - 1));
                dc.DrawRectangle(null, GetCachedFreezable<Pen>(PartState.InnerBorderHot), rect);

                bounds.Inflate(-0.5, -0.5);
                dc.DrawLine(borderPen, bounds.TopLeft, bounds.BottomLeft);
                dc.DrawLine(borderPen, bounds.BottomLeft, bounds.BottomRight);
                dc.DrawLine(borderPen, bounds.TopRight, bounds.BottomRight);
            } else {
                var borderPen = GetCachedFreezable<Pen>(PartState.BorderNormal);
                var background = GetCachedFreezable<Brush>(PartState.BackgroundNormal);

                dc.DrawRectangle(background, null, bounds);

                bounds.Inflate(-0.5, -0.5);
                dc.DrawLine(borderPen, bounds.BottomLeft, bounds.BottomRight);
                dc.DrawLine(borderPen, bounds.TopRight, bounds.BottomRight);
            }

            if (Orientation == Orientation.Horizontal)
                dc.Pop();

            RenderThemeExtras(dc);
        }

        private void RenderThemeExtras(DrawingContext dc)
        {
            if (SortDirection.HasValue) {
                var transform = new TranslateTransform((RenderSize.Width - 8.0) * 0.5, 1.0);
                transform.Freeze();

                dc.PushTransform(transform);

                bool ascending = SortDirection == ListSortDirection.Ascending;
                var geometry = GetCachedFreezable<PathGeometry>(
                    ascending ? PartState.ArrowUpGeometry : PartState.ArrowDownGeometry);

                dc.DrawGeometry(GetCachedFreezable<Brush>(PartState.ArrowBorderBrush), null, geometry);
                dc.PushTransform(GetCachedFreezable<Transform>(PartState.ArrowFillScale));
                dc.DrawGeometry(GetCachedFreezable<Brush>(PartState.ArrowFillBrush), null, geometry);
                dc.Pop();

                dc.Pop();
            }
        }

        private static Freezable[] CacheFreezables()
        {
            var freezables = new Freezable[(int)PartState.NumFreezables];

            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xF1, 0xFB));
                brush.Freeze();
                var pen = new Pen(brush, 1) {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                pen.Freeze();
                freezables[(int)PartState.BorderNormal] = pen;
            }
            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC));
                brush.Freeze();
                freezables[(int)PartState.BackgroundNormal] = brush;
            }

            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xE9, 0xF7), 0),
                        new GradientStop(Color.FromArgb(0x00, 0xDE, 0xE9, 0xF7), 1),
                    }
                };
                brush.Freeze();
                var pen = new Pen(brush, 1) {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                pen.Freeze();
                freezables[(int)PartState.SeparatorPen] = pen;
            }

            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0xE1, 0xEC, 0xFA), 0),
                        new GradientStop(Color.FromArgb(0xFF, 0xE6, 0xEB, 0xF1), 1),
                    }
                };
                brush.Freeze();
                var pen = new Pen(brush, 1) {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                pen.Freeze();
                freezables[(int)PartState.BorderHot] = pen;
            }
            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0xFD, 0xFE, 0xFF), 0),
                        new GradientStop(Color.FromArgb(0xFF, 0xFC, 0xFD, 0xFE), 1),
                    }
                };
                brush.Freeze();
                var pen = new Pen(brush, 1) {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                pen.Freeze();
                freezables[(int)PartState.InnerBorderHot] = pen;
            }
            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0xF5, 0xFA, 0xFF), 0),
                        new GradientStop(Color.FromArgb(0xFF, 0xF1, 0xF5, 0xFB), 1),
                    }
                };
                brush.Freeze();
                freezables[(int)PartState.BackgroundHot] = brush;
            }

            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xC2, 0xCD, 0xDB));
                brush.Freeze();
                var pen = new Pen(brush, 1) {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                pen.Freeze();
                freezables[(int)PartState.BorderPressed] = pen;
            }
            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0xFA, 0xFB));
                brush.Freeze();
                freezables[(int)PartState.BackgroundPressed] = brush;
            }
            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD9, 0xE0, 0xE9));
                brush.Freeze();
                freezables[(int)PartState.HighlightPressedDark] = brush;
            }
            {
                var brush = new SolidColorBrush(Color.FromArgb(0xFF, 0xEE, 0xF1, 0xF5));
                brush.Freeze();
                freezables[(int)PartState.HighlightPressedLight] = brush;
            }

            {
                var segment1 = new LineSegment(new Point(3.5, 0.0), false);
                segment1.Freeze();
                var segment2 = new LineSegment(new Point(7.0, 4.0), false);
                segment2.Freeze();

                var figure = new PathFigure {
                    StartPoint = new Point(0.0, 4.0),
                    IsClosed = true,
                    Segments = { segment1, segment2 }
                };
                figure.Freeze();

                var geometry = new PathGeometry {
                    Figures = { figure }
                };
                geometry.Freeze();

                freezables[(int)PartState.ArrowUpGeometry] = geometry;
            }
            {
                var segment1 = new LineSegment(new Point(7.0, 0.0), false);
                segment1.Freeze();
                var segment2 = new LineSegment(new Point(3.5, 4.0), false);
                segment2.Freeze();

                var figure = new PathFigure {
                    StartPoint = new Point(0.0, 0.0),
                    IsClosed = true,
                    Segments = { segment1, segment2 }
                };
                figure.Freeze();

                var geometry = new PathGeometry {
                    Figures = { figure }
                };
                geometry.Freeze();

                freezables[(int)PartState.ArrowDownGeometry] = geometry;
            }
            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(),
                    EndPoint = new Point(1, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.0),
                        new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.1),
                        new GradientStop(Color.FromArgb(0xFF, 0xC3, 0xE4, 0xF5), 1.0),
                    }
                };
                brush.Freeze();
                freezables[(int)PartState.ArrowBorderBrush] = brush;
            }
            {
                var brush = new LinearGradientBrush {
                    StartPoint = new Point(),
                    EndPoint = new Point(1, 1),
                    GradientStops = {
                        new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.0),
                        new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.1),
                        new GradientStop(Color.FromArgb(0xFF, 0xCA, 0xE6, 0xF5), 1.0),
                    }
                };
                brush.Freeze();
                freezables[(int)PartState.ArrowFillBrush] = brush;
            }
            {
                var transform = new ScaleTransform(0.75, 0.75, 3.5, 4);
                transform.Freeze();
                freezables[(int)PartState.ArrowFillScale] = transform;
            }

            {
                var transform = new MatrixTransform(0, 1, 1, 0, 0, 0);
                transform.Freeze();
                freezables[(int)PartState.SwapXAndYTransform] = transform;
            }

            return freezables;
        }

        /// <summary>Releases all resources in the cache.</summary>
        public static void ReleaseCache()
        {
            Interlocked.Exchange(ref freezableCache, new Lazy<Freezable[]>(CacheFreezables));
        }

        private enum PartState
        {
            BorderNormal,
            BackgroundNormal,
            BorderHot,
            BackgroundHot,
            InnerBorderHot,
            BorderPressed,
            BackgroundPressed,
            HighlightPressedDark,
            HighlightPressedLight,
            ArrowUpGeometry,
            ArrowDownGeometry,
            ArrowFillScale,
            ArrowBorderBrush,
            ArrowFillBrush,
            SeparatorPen,
            SwapXAndYTransform,
            NumFreezables
        }
    }
}
