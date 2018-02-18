namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>Creates the theme specific-look for headers.</summary>
    public sealed class HeaderChrome : Border
    {
        private static List<Freezable> freezableCache;
        private static readonly object cacheMutex = new object();

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
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

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
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.AffectsArrange"/>
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
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

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
        ///           <see cref="FrameworkPropertyMetadata.AffectsRender"/>,
        ///           <see cref="FrameworkPropertyMetadata.AffectsArrange"/>
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
                    FrameworkPropertyMetadataOptions.AffectsRender));

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

                child.Arrange(new Rect(padding.Left, padding.Top, childWidth, childHeight));
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
                Thickness? themePadding = ThemeDefaultPadding;
                if (themePadding != null)
                    padding = (Thickness)themePadding;

                // When pressed, offset the child
                if (IsPressed && IsClickable) {
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

        /// <summary>
        ///   Creates a cache of frozen Freezable resources for use across all
        ///   instances of the border.
        /// </summary>
        private static void EnsureCache(int size)
        {
            // Quick check to avoid locking
            if (freezableCache == null) {
                lock (cacheMutex) {
                    // Re-check in case another thread created the cache
                    if (freezableCache == null) {
                        freezableCache = new List<Freezable>(size);
                        for (int i = 0; i < size; ++i)
                            freezableCache.Add(null);
                    }
                }
            }

            Debug.Assert(freezableCache.Count == size, "The cache size does not match the requested amount.");
        }

        /// <summary>Releases all resources in the cache.</summary>
        public static void ReleaseCache()
        {
            // Avoid locking if necessary
            if (freezableCache != null) {
                lock (cacheMutex) {
                    // No need to re-check if non-null since it's OK to set it to null multiple times
                    freezableCache = null;
                }
            }
        }

        /// <summary>Retrieves a cached resource.</summary>
        private static Freezable GetCachedFreezable(int index)
        {
            lock (cacheMutex) {
                Freezable freezable = freezableCache[index];
                Debug.Assert((freezable == null) || freezable.IsFrozen, "Cached Freezables should have been frozen.");
                return freezable;
            }
        }

        /// <summary>Caches a resources.</summary>
        private static void CacheFreezable(Freezable freezable, int index)
        {
            Debug.Assert(freezable.IsFrozen, "Cached Freezables should be frozen.");

            lock (cacheMutex) {
                if (freezableCache[index] != null)
                    freezableCache[index] = freezable;
            }
        }

        private Thickness? ThemeDefaultPadding
        {
            get
            {
                if (Orientation == Orientation.Vertical)
                    return new Thickness(5, 4, 5, 4);
                return null;
            }
        }

        private void RenderTheme(DrawingContext dc)
        {
            Size size = RenderSize;
            bool horizontal = Orientation == Orientation.Horizontal;
            bool isClickable = IsClickable && IsEnabled;
            bool isHovered = isClickable && IsHovered;
            bool isPressed = isClickable && IsPressed;
            bool isSelected = IsSelected;

            EnsureCache((int)PartState.NumFreezables);

            if (horizontal) {
                // When horizontal, rotate the rendering by -90 degrees
                Matrix m1 = new Matrix();
                m1.RotateAt(-90, 0, 0);
                Matrix m2 = new Matrix();
                m2.Translate(0, size.Height);

                var horizontalRotate = new MatrixTransform(m1 * m2);
                horizontalRotate.Freeze();
                dc.PushTransform(horizontalRotate);

                double temp = size.Width;
                size.Width = size.Height;
                size.Height = temp;
            }

            // Fill the background
            PartState backgroundType = PartState.BackgroundNormal;
            if (isPressed)
                backgroundType = PartState.BackgroundPressed;
            else if (isHovered)
                backgroundType = PartState.BackgroundHot;
            else if (isSelected)
                backgroundType = PartState.BackgroundPressed;

            Brush background = (Brush)GetCachedFreezable((int)backgroundType);
            if (background == null) {
                switch (backgroundType) {
                    default:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                        break;

                    case PartState.BackgroundHot:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                        break;

                    case PartState.BackgroundPressed:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                        break;
                }

                background.Freeze();

                CacheFreezable(background, (int)backgroundType);
            }

            dc.DrawRectangle(background, null, new Rect(size));

            if (size.Width >= 2) {
                // Draw the borders on the sides
                PartState separatorType = PartState.SeparatorNormal;
                if (isPressed)
                    separatorType = PartState.SeparatorPressed;
                else if (isHovered)
                    separatorType = PartState.SeparatorHot;

                if (SeparatorVisibility == Visibility.Visible) {
                    Brush sepBrush;
                    if (SeparatorBrush != null) {
                        sepBrush = SeparatorBrush;
                    } else {
                        sepBrush = (Brush)GetCachedFreezable((int)separatorType);
                        if (sepBrush == null) {
                            switch (separatorType) {
                                default:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5));
                                    break;
                                case PartState.SeparatorHot:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                                    break;
                                case PartState.SeparatorPressed:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                                    break;
                            }

                            sepBrush.Freeze();
                            CacheFreezable(sepBrush, (int)separatorType);
                        }
                    }

                    if (Orientation == Orientation.Horizontal)
                        dc.DrawRectangle(sepBrush, null, new Rect(0.0, 0.0, 1.0, Math.Max(0, size.Height - 0.95)));
                    else
                        dc.DrawRectangle(sepBrush, null, new Rect(size.Width - 1, 0, 1, size.Height));
                }
            }

            if (size.Height >= 2 && BorderVisibility == Visibility.Visible) {
                // Separator and Border use the same brush.
                PartState separatorType = PartState.SeparatorNormal;
                if (isPressed)
                    separatorType = PartState.SeparatorPressed;
                else if (isHovered)
                    separatorType = PartState.SeparatorHot;

                Brush sepBrush = (Brush)GetCachedFreezable((int)separatorType);
                if (sepBrush == null) {
                    switch (separatorType) {
                        default:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5));
                            break;
                        case PartState.SeparatorHot:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                            break;
                        case PartState.SeparatorPressed:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                            break;
                    }

                    sepBrush.Freeze();
                    CacheFreezable(sepBrush, (int)separatorType);
                }

                dc.DrawRectangle(sepBrush, null, new Rect(0, size.Height - 1, size.Width, 1));
            }

            RenderSortIndicator(dc);

            if (horizontal)
                dc.Pop(); // Horizontal Rotate
        }

        private void RenderThemeExtras(DrawingContext dc)
        {
            Size size = RenderSize;
            bool horizontal = Orientation == Orientation.Horizontal;

            EnsureCache((int)PartState.NumFreezables);

            if (horizontal) {
                // When horizontal, rotate the rendering by -90 degrees
                var m1 = new Matrix();
                m1.RotateAt(-90, 0, 0);
                var m2 = new Matrix();
                m2.Translate(0, size.Height);

                var horizontalRotate = new MatrixTransform(m1 * m2);
                horizontalRotate.Freeze();
                dc.PushTransform(horizontalRotate);

                double temp = size.Width;
                size.Width = size.Height;
                size.Height = temp;
            }

            RenderSortIndicator(dc);

            if (horizontal)
                dc.Pop(); // Horizontal Rotate
        }

        private void RenderSortIndicator(DrawingContext dc)
        {
            Size size = RenderSize;
            ListSortDirection? sortDirection = SortDirection;
            bool isSorted = sortDirection != null;

            if (Orientation == Orientation.Horizontal) {
                double temp = size.Width;
                size.Width = size.Height;
                size.Height = temp;
            }

            if (isSorted && size.Height > 10) {
                bool ascending = sortDirection == ListSortDirection.Ascending;
                Geometry arrowGeometry = (Geometry)GetCachedFreezable(ascending ? (int)PartState.ArrowUpGeometry : (int)PartState.ArrowDownGeometry);
                if (arrowGeometry == null) {
                    if (ascending) {
                        // M 3,4 6.5,0.5 10,4 z (widen=1)
                        arrowGeometry = Geometry.Parse(
                            "F1M3.35,4.35L2.646,3.646 6.146,0.146 6.5,-0.207 10.354,3.646 9.646,4.354 6.146,0.854 6.5,0.5 6.854,0.854 3.354,4.354z");
                    } else {
                        // M 3,0 6.5,3.5 10,0 z (widen=1)
                        arrowGeometry = Geometry.Parse(
                            "F1M2.65,0.35L3.354,-0.354 6.854,3.146 6.5,3.5 6.146,3.146 9.646,-0.354 10.354,0.354 6.5,4.207 6.146,3.854 2.646,0.354z");
                    }

                    arrowGeometry.Freeze();

                    CacheFreezable(arrowGeometry, ascending ? (int)PartState.ArrowUpGeometry : (int)PartState.ArrowDownGeometry);
                }

                Brush arrowFill = (Brush)GetCachedFreezable((int)PartState.ArrowFill);
                if (arrowFill == null) {
                    arrowFill = new SolidColorBrush(Color.FromArgb(0xDD, 0x33, 0x33, 0x33));
                    arrowFill.Freeze();
                    CacheFreezable(arrowFill, (int)PartState.ArrowFill);
                }

                var offsetX = (size.Width - arrowGeometry.Bounds.Width - 2 * arrowGeometry.Bounds.X) * 0.5;
                var translation = new TranslateTransform(Math.Round(offsetX), 0);
                translation.Freeze();

                dc.PushTransform(translation);
                dc.DrawGeometry(arrowFill, null, arrowGeometry);
                dc.Pop();
            }
        }

        private enum PartState
        {
            BackgroundNormal,
            BackgroundHot,
            BackgroundPressed,
            SeparatorNormal,
            SeparatorHot,
            SeparatorPressed,
            ArrowFill,
            ArrowUpGeometry,
            ArrowDownGeometry,
            NumFreezables
        }
    }
}
