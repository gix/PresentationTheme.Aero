namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class GridViewColumnHeaderChrome : Decorator
    {
        private static readonly Lazy<Pen> SeparatorNormalPen;
        private static readonly Lazy<Pen> SeparatorHotPen;
        private static readonly Lazy<Pen> SeparatorPressedPen;
        private static readonly Lazy<Brush> BackgroundNormalBrush;
        private static readonly Lazy<Brush> BackgroundPressedBrush;
        private static readonly Lazy<Brush> BackgroundHotBrush;
        private static readonly Lazy<PathGeometry> ArrowUpGeometry;
        private static readonly Lazy<PathGeometry> ArrowDownGeometry;
        private static readonly Lazy<Transform> ArrowFillScale;
        private static readonly Lazy<Brush> ArrowBorderBrush;
        private static readonly Lazy<Brush> ArrowFillBrush;

        /// <summary>
        ///   Identifies the <see cref="Padding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(
                    new Thickness(6, 4, 6, 5),
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        ///   Identifies the <see cref="RenderPressed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderPressedProperty =
            DependencyProperty.Register(
                nameof(RenderPressed),
                typeof(bool),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderHot"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderHotProperty =
            DependencyProperty.Register(
                nameof(RenderHot),
                typeof(bool),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(
                    false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="SortDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(
                nameof(SortDirection),
                typeof(ListSortDirection?),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                nameof(SeparatorVisibility),
                typeof(Visibility),
                typeof(GridViewColumnHeaderChrome),
                new PropertyMetadata(Visibility.Visible));

        static GridViewColumnHeaderChrome()
        {
            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(GridViewColumnHeaderChrome), new FrameworkPropertyMetadata(true));

            SeparatorNormalPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            SeparatorHotPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });
            SeparatorPressedPen = new Lazy<Pen>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });

            BackgroundNormalBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                    brush.Freeze();
                    return brush;
                });
            BackgroundHotBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                    brush.Freeze();
                    return brush;
                });
            BackgroundPressedBrush = new Lazy<Brush>(
                () => {
                    var brush = new SolidColorBrush(
                        Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                    brush.Freeze();
                    return brush;
                });

            ArrowUpGeometry = new Lazy<PathGeometry>(
                () => {
                    var geometry = new PathGeometry();
                    var figure = new PathFigure();
                    figure.StartPoint = new Point(0.0, 4.0);
                    var segment = new LineSegment(new Point(3.5, 0.0), false);
                    segment.Freeze();
                    figure.Segments.Add(segment);
                    segment = new LineSegment(new Point(7.0, 4.0), false);
                    segment.Freeze();
                    figure.Segments.Add(segment);
                    figure.IsClosed = true;
                    figure.Freeze();
                    geometry.Figures.Add(figure);
                    geometry.Freeze();
                    return geometry;
                });
            ArrowDownGeometry = new Lazy<PathGeometry>(
                () => {
                    var geometry = new PathGeometry();
                    var figure = new PathFigure();
                    figure.StartPoint = new Point(0.0, 0.0);
                    var segment = new LineSegment(new Point(7.0, 0.0), false);
                    segment.Freeze();
                    figure.Segments.Add(segment);
                    segment = new LineSegment(new Point(3.5, 4.0), false);
                    segment.Freeze();
                    figure.Segments.Add(segment);
                    figure.IsClosed = true;
                    figure.Freeze();
                    geometry.Figures.Add(figure);
                    geometry.Freeze();
                    return geometry;
                });
            ArrowBorderBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush {
                        StartPoint = new Point(),
                        EndPoint = new Point(1, 1)
                    };
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.0));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.1));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xC3, 0xE4, 0xF5), 1.0));
                    brush.Freeze();
                    return brush;
                });
            ArrowFillBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush {
                        StartPoint = new Point(),
                        EndPoint = new Point(1, 1)
                    };
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.0));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.1));
                    brush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xCA, 0xE6, 0xF5), 1.0));
                    brush.Freeze();
                    return brush;
                });
            ArrowFillScale = new Lazy<Transform>(
                () => {
                    var transform = new ScaleTransform(0.75, 0.75, 3.5, 4);
                    transform.Freeze();
                    return transform;
                });
        }

        /// <summary>
        ///   Gets or sets the padding.
        /// </summary>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the chrome shows a
        ///   hot state.
        /// </summary>
        public bool RenderHot
        {
            get { return (bool)GetValue(RenderHotProperty); }
            set { SetValue(RenderHotProperty, value); }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the chrome shows a
        ///   pressed state.
        /// </summary>
        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the indicated sort direction.
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get { return (ListSortDirection?)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        /// <summary>Gets or sets the separator visibility.</summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        private Brush BackgroundBrush
        {
            get
            {
                if (RenderPressed)
                    return BackgroundPressedBrush.Value;
                if (RenderHot)
                    return BackgroundHotBrush.Value;
                return BackgroundNormalBrush.Value;
            }
        }

        private Pen SeparatorPen
        {
            get
            {
                if (RenderPressed)
                    return SeparatorPressedPen.Value;
                if (RenderHot)
                    return SeparatorHotPen.Value;
                return SeparatorNormalPen.Value;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            if (child == null)
                return new Size(
                    Math.Min(MinWidth, constraint.Width),
                    Math.Min(MinHeight, constraint.Height));

            var totalPadding = new Size(
                Padding.Left + Padding.Right, Padding.Top + Padding.Bottom);

            var childSize = new Size();
            bool widthTooSmall = constraint.Width < totalPadding.Width;
            bool heightTooSmall = constraint.Height < totalPadding.Height;
            if (!widthTooSmall)
                childSize.Width = constraint.Width - totalPadding.Width;
            if (!heightTooSmall)
                childSize.Height = constraint.Height - totalPadding.Height;

            child.Measure(childSize);

            Size desiredSize = child.DesiredSize;
            if (!widthTooSmall)
                desiredSize.Width += totalPadding.Width;
            if (!heightTooSmall)
                desiredSize.Height += totalPadding.Height;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child == null)
                return arrangeSize;

            var padding = Padding;
            var bounds = new Rect(
                padding.Left,
                padding.Top,
                Math.Max(0, arrangeSize.Width - padding.Left - padding.Right),
                Math.Max(0, arrangeSize.Height - padding.Top - padding.Bottom));

            if (RenderPressed)
                bounds.Location += new Vector(1, 1);

            child.Arrange(bounds);

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bounds = new Rect(RenderSize);
            var topRight = bounds.TopRight + new Vector(-0.5, -0.5);
            var bottomRight = bounds.BottomRight + new Vector(-0.5, +0.5);

            dc.DrawRectangle(BackgroundBrush, null, bounds);
            dc.DrawLine(SeparatorPen, topRight, bottomRight);

            if (SortDirection.HasValue) {
                var transform = new TranslateTransform((RenderSize.Width - 8) * 0.5, 1);
                transform.Freeze();

                bool ascending = SortDirection == ListSortDirection.Ascending;
                var geometry = (ascending ? ArrowUpGeometry : ArrowDownGeometry).Value;

                dc.PushTransform(transform);
                dc.DrawGeometry(ArrowBorderBrush.Value, null, geometry);

                dc.PushTransform(ArrowFillScale.Value);
                dc.DrawGeometry(ArrowFillBrush.Value, null, geometry);
                dc.Pop();

                dc.Pop();
            }
        }
    }
}
