namespace Windows.Aero
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class GridViewColumnHeaderChrome : Decorator
    {
        private static readonly Lazy<Pen> NormalBorderPen;
        private static readonly Lazy<Pen> InnerBorderPen;
        private static readonly Lazy<Pen> SeparatorPen;
        private static readonly Lazy<Pen> PressedBorderPen;
        private static readonly Lazy<Brush> PressedBackgroundBrush;
        private static readonly Lazy<Brush> PressedHighlightBrush;
        private static readonly Lazy<Pen> HoveredBorderPen;
        private static readonly Lazy<Brush> HoveredBackgroundBrush;
        private static readonly Lazy<PathGeometry> ArrowUpGeometry;
        private static readonly Lazy<PathGeometry> ArrowDownGeometry;
        private static readonly Lazy<Transform> ArrowFillScale;
        private static readonly Lazy<Brush> ArrowBorderBrush;
        private static readonly Lazy<Brush> ArrowFillBrush;

        /// <summary>
        ///   Identifies the <see cref="RenderPressed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderPressedProperty =
            DependencyProperty.Register(
                "RenderPressed",
                typeof(bool),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="RenderMouseOver"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderMouseOverProperty =
            DependencyProperty.Register(
                "RenderMouseOver",
                typeof(bool),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="SortDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(
                "SortDirection",
                typeof(ListSortDirection?),
                typeof(GridViewColumnHeaderChrome),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Identifies the <see cref="SeparatorVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register(
                "SeparatorVisibility",
                typeof(Visibility),
                typeof(GridViewColumnHeaderChrome),
                new PropertyMetadata(Visibility.Visible));

        static GridViewColumnHeaderChrome()
        {
            SnapsToDevicePixelsProperty.OverrideMetadata(
                typeof(GridViewColumnHeaderChrome), new FrameworkPropertyMetadata(true));

            NormalBorderPen = new Lazy<Pen>(
                () => {
                    var pen = new Pen { Thickness = 1.0, Brush = Brushes.Transparent };
                    pen.Freeze();
                    return pen;
                });

            InnerBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xFD, 0xFE, 0xFF),
                        Color.FromArgb(0xFF, 0xFC, 0xFD, 0xFE),
                        90);
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });

            SeparatorPen = new Lazy<Pen>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xDE, 0xE9, 0xF7),
                        Color.FromArgb(0x00, 0xDE, 0xE9, 0xF7),
                        90);
                    brush.Freeze();
                    var pen = new Pen(brush, 1);
                    pen.Freeze();
                    return pen;
                });

            PressedBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xC0, 0xCB, 0xD9),
                        Color.FromArgb(0xFF, 0xC0, 0xCB, 0xD9),
                        90);
                    brush.Freeze();
                    var pen = new Pen(brush, 1) {
                        StartLineCap = PenLineCap.Square,
                        EndLineCap = PenLineCap.Square
                    };
                    pen.Freeze();
                    return pen;
                });
            PressedBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xF6, 0xF7, 0xF8),
                        Color.FromArgb(0xFF, 0xF6, 0xF7, 0xF8),
                        90);
                    brush.Freeze();
                    return brush;
                });
            PressedHighlightBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xC1, 0xCC, 0xDA),
                        Color.FromArgb(0xFF, 0xEB, 0xEE, 0xF2),
                        90);
                    brush.Freeze();
                    return brush;
                });

            HoveredBorderPen = new Lazy<Pen>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xDE, 0xE9, 0xF7),
                        Color.FromArgb(0xFF, 0xE3, 0xE8, 0xEE),
                        90);
                    brush.Freeze();
                    var pen = new Pen(brush, 1) {
                        StartLineCap = PenLineCap.Square,
                        EndLineCap = PenLineCap.Square
                    };
                    pen.Freeze();
                    return pen;
                });
            HoveredBackgroundBrush = new Lazy<Brush>(
                () => {
                    var brush = new LinearGradientBrush(
                        Color.FromArgb(0xFF, 0xF3, 0xF8, 0xFD),
                        Color.FromArgb(0xFF, 0xEF, 0xF3, 0xF9),
                        90);
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
        ///   Gets or sets the RenderPressed of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the RenderMouseOver of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the SortDirection of the <see cref="ListViewItemChrome"/>.
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get { return (ListSortDirection?)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the SeparatorVisibility of the <see cref="GridViewColumnHeaderChrome"/>.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        private Pen BorderPen
        {
            get
            {
                if (RenderMouseOver)
                    return HoveredBorderPen.Value;
                if (RenderPressed)
                    return PressedBorderPen.Value;
                return NormalBorderPen.Value;
            }
        }

        private Brush BackgroundBrush
        {
            get
            {
                if (RenderMouseOver)
                    return HoveredBackgroundBrush.Value;
                if (RenderPressed)
                    return PressedBackgroundBrush.Value;
                return Brushes.Transparent;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElement child = Child;
            if (child == null)
                return new Size(Math.Min(4.0, constraint.Width), Math.Min(4.0, constraint.Height));

            var size = new Size();
            bool widthTooSmall = constraint.Width < 4.0;
            bool heightTooSmall = constraint.Height < 4.0;
            if (!widthTooSmall)
                size.Width = constraint.Width - 14;
            if (!heightTooSmall)
                size.Height = constraint.Height - 9;

            child.Measure(size);

            Size desiredSize = child.DesiredSize;
            if (!widthTooSmall)
                desiredSize.Width += 14;
            if (!heightTooSmall)
                desiredSize.Height += 9;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = Child;
            if (child == null)
                return arrangeSize;

            var bounds = new Rect(
                7,
                5,
                Math.Max(0, arrangeSize.Width - 14),
                Math.Max(0, arrangeSize.Height - 9));
            child.Arrange(bounds);

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var bounds = new Rect(RenderSize);

            if (!RenderPressed && !RenderMouseOver) {
                dc.DrawRectangle(Brushes.Transparent, null, bounds);
                if (SeparatorVisibility == Visibility.Visible) {
                    bounds.Inflate(-0.5, -0.5);
                    dc.DrawLine(SeparatorPen.Value, bounds.TopRight, bounds.BottomRight);
                }
            } else if (RenderPressed) {
                var rect = new Rect(new Point(1, 0), new Size(bounds.Width - 2, bounds.Height - 1));
                dc.DrawRectangle(BackgroundBrush, null, rect);

                rect = new Rect(new Point(1, 0), new Point(bounds.Width - 1, 3));
                dc.DrawRectangle(PressedHighlightBrush.Value, null, rect);

                bounds.Inflate(-0.5, -0.5);
                dc.DrawLine(BorderPen, bounds.TopLeft, bounds.BottomLeft);
                dc.DrawLine(BorderPen, bounds.BottomLeft, bounds.BottomRight);
                dc.DrawLine(BorderPen, bounds.TopRight, bounds.BottomRight);
            } else if (RenderMouseOver) {
                var rect = new Rect(new Point(2, 1), new Size(bounds.Width - 4, bounds.Height - 3));
                dc.DrawRectangle(BackgroundBrush, null, rect);

                rect = new Rect(new Point(1, 0), new Size(bounds.Width - 2, bounds.Height - 1));
                dc.DrawRectangle(null, InnerBorderPen.Value, rect);

                bounds.Inflate(-0.5, -0.5);
                dc.DrawLine(BorderPen, bounds.TopLeft, bounds.BottomLeft);
                dc.DrawLine(BorderPen, bounds.BottomLeft, bounds.BottomRight);
                dc.DrawLine(BorderPen, bounds.TopRight, bounds.BottomRight);
            }

            if (SortDirection.HasValue) {
                var transform = new TranslateTransform((RenderSize.Width - 8.0) * 0.5, 1.0);
                transform.Freeze();

                dc.PushTransform(transform);

                bool ascending = SortDirection == ListSortDirection.Ascending;
                var geometry = (@ascending ? ArrowUpGeometry : ArrowDownGeometry).Value;

                dc.DrawGeometry(ArrowBorderBrush.Value, null, geometry);
                dc.PushTransform(ArrowFillScale.Value);
                dc.DrawGeometry(ArrowFillBrush.Value, null, geometry);
                dc.Pop();

                dc.Pop();
            }
        }
    }
}