namespace PresentationTheme.AeroLite.Win8
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

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            RenderThemeExtras(dc);
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
                    if (ascending)
                        arrowGeometry = Geometry.Parse("M 2.5,4 6.5,0 10.5,4 z");
                    else
                        arrowGeometry = Geometry.Parse("M 2.5,0 6.5,4 10.5,0 z");

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
                // AeroLite has a TMT_MINSIZE specified for headers which shifts
                // the sort arrow a few pixels down.
                var offsetY = 3.0;
                var translation = new TranslateTransform(Math.Round(offsetX), offsetY);
                translation.Freeze();

                dc.PushTransform(translation);
                dc.DrawGeometry(arrowFill, null, arrowGeometry);
                dc.Pop();
            }
        }

        private enum PartState
        {
            ArrowFill,
            ArrowUpGeometry,
            ArrowDownGeometry,
            NumFreezables
        }
    }
}
