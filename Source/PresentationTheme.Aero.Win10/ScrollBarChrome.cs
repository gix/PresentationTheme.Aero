namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class ScrollBarChrome : Decorator
    {
        private Pen lightPenCache;
        private Pen darkPenCache;

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(ScrollBarChrome),
                new PropertyMetadata(Orientation.Vertical));

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty LightBorderBrushProperty =
            DependencyProperty.Register(
                nameof(LightBorderBrush),
                typeof(Brush),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnClearPenCache));

        public static readonly DependencyProperty DarkBorderBrushProperty =
            DependencyProperty.Register(
                nameof(DarkBorderBrush),
                typeof(Brush),
                typeof(ScrollBarChrome),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnClearPenCache));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush LightBorderBrush
        {
            get { return (Brush)GetValue(LightBorderBrushProperty); }
            set { SetValue(LightBorderBrushProperty, value); }
        }

        public Brush DarkBorderBrush
        {
            get { return (Brush)GetValue(DarkBorderBrushProperty); }
            set { SetValue(DarkBorderBrushProperty, value); }
        }

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

        private static void OnClearPenCache(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ScrollBarChrome)d;
            source.lightPenCache = null;
            source.darkPenCache = null;
        }

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
    }
}
