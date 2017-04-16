namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public sealed class SystemDropShadowChrome : Decorator
    {
        private const int TopLeft = 0;
        private const int Top = 1;
        private const int TopRight = 2;
        private const int Left = 3;
        private const int Center = 4;
        private const int Right = 5;
        private const int BottomLeft = 6;
        private const int Bottom = 7;
        private const int BottomRight = 8;
        private const double ShadowDepth = 5.0;

        private static readonly object ResourceAccess = new object();
        private static Brush[] CommonBrushes;
        private static CornerRadius CommonCornerRadius;
        private Brush[] brushes;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            nameof(Color), typeof(Color), typeof(SystemDropShadowChrome),
            new FrameworkPropertyMetadata(
                Color.FromArgb(0x71, 0, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender,
                ClearBrushes));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius), typeof(CornerRadius), typeof(SystemDropShadowChrome),
                new FrameworkPropertyMetadata(
                    new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    ClearBrushes),
                IsCornerRadiusValid);

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static bool IsCornerRadiusValid(object value)
        {
            var radius = (CornerRadius)value;
            return
                radius.TopLeft >= 0.0 &&
                radius.TopRight >= 0.0 &&
                radius.BottomLeft >= 0.0 &&
                radius.BottomRight >= 0.0 &&
                !double.IsNaN(radius.TopLeft) &&
                !double.IsNaN(radius.TopRight) &&
                !double.IsNaN(radius.BottomLeft) &&
                !double.IsNaN(radius.BottomRight) &&
                !double.IsInfinity(radius.TopLeft) &&
                !double.IsInfinity(radius.TopRight) &&
                !double.IsInfinity(radius.BottomLeft) &&
                !double.IsInfinity(radius.BottomRight);
        }

        private static void ClearBrushes(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SystemDropShadowChrome)d).brushes = null;
        }

        private static Brush[] CreateBrushes(Color c, CornerRadius cornerRadius)
        {
            Brush[] brushArray = new Brush[9];
            brushArray[Center] = new SolidColorBrush(c);
            brushArray[Center].Freeze();

            GradientStopCollection stops = CreateStops(c, 0.0);

            var topBrush = new LinearGradientBrush(
                stops, new Point(0.0, 1.0), new Point(0.0, 0.0));
            topBrush.Freeze();
            brushArray[Top] = topBrush;

            var leftBrush = new LinearGradientBrush(
                stops, new Point(1.0, 0.0), new Point(0.0, 0.0));
            leftBrush.Freeze();
            brushArray[Left] = leftBrush;

            var rightBrush = new LinearGradientBrush(
                stops, new Point(0.0, 0.0), new Point(1.0, 0.0));
            rightBrush.Freeze();
            brushArray[Right] = rightBrush;

            var bottomBrush = new LinearGradientBrush(
                stops, new Point(0.0, 0.0), new Point(0.0, 1.0));
            bottomBrush.Freeze();
            brushArray[Bottom] = bottomBrush;

            GradientStopCollection topLeftStops;
            if (cornerRadius.TopLeft == 0.0)
                topLeftStops = stops;
            else
                topLeftStops = CreateStops(c, cornerRadius.TopLeft);

            var topLeftBrush = new RadialGradientBrush(topLeftStops) {
                RadiusX = 1.0,
                RadiusY = 1.0,
                Center = new Point(1.0, 1.0),
                GradientOrigin = new Point(1.0, 1.0)
            };
            topLeftBrush.Freeze();
            brushArray[TopLeft] = topLeftBrush;

            GradientStopCollection topRightStops;
            if (cornerRadius.TopRight == 0.0)
                topRightStops = stops;
            else if (cornerRadius.TopRight == cornerRadius.TopLeft)
                topRightStops = topLeftStops;
            else
                topRightStops = CreateStops(c, cornerRadius.TopRight);

            var topRightBrush = new RadialGradientBrush(topRightStops) {
                RadiusX = 1.0,
                RadiusY = 1.0,
                Center = new Point(0.0, 1.0),
                GradientOrigin = new Point(0.0, 1.0)
            };
            topRightBrush.Freeze();
            brushArray[TopRight] = topRightBrush;

            GradientStopCollection bottomLeftStops;
            if (cornerRadius.BottomLeft == 0.0)
                bottomLeftStops = stops;
            else if (cornerRadius.BottomLeft == cornerRadius.TopLeft)
                bottomLeftStops = topLeftStops;
            else if (cornerRadius.BottomLeft == cornerRadius.TopRight)
                bottomLeftStops = topRightStops;
            else
                bottomLeftStops = CreateStops(c, cornerRadius.BottomLeft);

            var bottomLeftBrush = new RadialGradientBrush(bottomLeftStops) {
                RadiusX = 1.0,
                RadiusY = 1.0,
                Center = new Point(1.0, 0.0),
                GradientOrigin = new Point(1.0, 0.0)
            };
            bottomLeftBrush.Freeze();
            brushArray[BottomLeft] = bottomLeftBrush;

            GradientStopCollection bottomRightStops;
            if (cornerRadius.BottomRight == 0.0)
                bottomRightStops = stops;
            else if (cornerRadius.BottomRight == cornerRadius.TopLeft)
                bottomRightStops = topLeftStops;
            else if (cornerRadius.BottomRight == cornerRadius.TopRight)
                bottomRightStops = topRightStops;
            else if (cornerRadius.BottomRight == cornerRadius.BottomLeft)
                bottomRightStops = bottomLeftStops;
            else
                bottomRightStops = CreateStops(c, cornerRadius.BottomRight);

            var bottomRightBrush = new RadialGradientBrush(bottomRightStops) {
                RadiusX = 1.0,
                RadiusY = 1.0,
                Center = new Point(0.0, 0.0),
                GradientOrigin = new Point(0.0, 0.0)
            };
            bottomRightBrush.Freeze();
            brushArray[BottomRight] = bottomRightBrush;

            return brushArray;
        }

        private static GradientStopCollection CreateStops(Color c, double cornerRadius)
        {
            double num = 1.0 / (cornerRadius + 5.0);
            var stops = new GradientStopCollection {
                new GradientStop(c, (0.5 + cornerRadius) * num)
            };

            Color color = c;
            color.A = (byte)(0.74336 * c.A);
            stops.Add(new GradientStop(color, (1.5 + cornerRadius) * num));

            color.A = (byte)(0.38053 * c.A);
            stops.Add(new GradientStop(color, (2.5 + cornerRadius) * num));

            color.A = (byte)(0.12389 * c.A);
            stops.Add(new GradientStop(color, (3.5 + cornerRadius) * num));

            color.A = (byte)(0.02654 * c.A);
            stops.Add(new GradientStop(color, (4.5 + cornerRadius) * num));

            color.A = 0;
            stops.Add(new GradientStop(color, (5.0 + cornerRadius) * num));

            stops.Freeze();
            return stops;
        }

        private Brush[] GetBrushes(Color c, CornerRadius cornerRadius)
        {
            if (CommonBrushes == null) {
                lock (ResourceAccess) {
                    if (CommonBrushes == null) {
                        CommonBrushes = CreateBrushes(c, cornerRadius);
                        CommonCornerRadius = cornerRadius;
                    }
                }
            }

            if (c == ((SolidColorBrush)CommonBrushes[Center]).Color &&
                cornerRadius == CommonCornerRadius) {
                brushes = null;
                return CommonBrushes;
            }

            return brushes ?? (brushes = CreateBrushes(c, cornerRadius));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var bounds = new Rect(
                new Point(5.0, 5.0),
                new Size(RenderSize.Width, RenderSize.Height));

            Color c = Color;
            if (bounds.Width <= 0 || bounds.Height <= 0 || c.A <= 0)
                return;

            double width = (bounds.Right - bounds.Left) - 10.0;
            double height = (bounds.Bottom - bounds.Top) - 10.0;
            double minRadius = Math.Min(width * 0.5, height * 0.5);

            CornerRadius cornerRadius = CornerRadius;
            cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, minRadius);
            cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, minRadius);
            cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, minRadius);
            cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, minRadius);

            Brush[] brushes = GetBrushes(c, cornerRadius);
            double top = bounds.Top + 5.0;
            double left = bounds.Left + 5.0;
            double right = bounds.Right - 5.0;
            double bottom = bounds.Bottom - 5.0;

            double[] guidelinesX = {
                left,
                left + cornerRadius.TopLeft,
                right - cornerRadius.TopRight,
                left + cornerRadius.BottomLeft,
                right - cornerRadius.BottomRight,
                right
            };
            double[] guidelinesY = {
                top,
                top + cornerRadius.TopLeft,
                top + cornerRadius.TopRight,
                bottom - cornerRadius.BottomLeft,
                bottom - cornerRadius.BottomRight,
                bottom
            };

            drawingContext.PushGuidelineSet(new GuidelineSet(guidelinesX, guidelinesY));
            cornerRadius.TopLeft += 5.0;
            cornerRadius.TopRight += 5.0;
            cornerRadius.BottomLeft += 5.0;
            cornerRadius.BottomRight += 5.0;

            var topLeft = new Rect(
                bounds.Left, bounds.Top, cornerRadius.TopLeft, cornerRadius.TopLeft);
            drawingContext.DrawRectangle(brushes[TopLeft], null, topLeft);

            double num8 = guidelinesX[TopRight] - guidelinesX[Top];
            if (num8 > 0.0) {
                var topBounds = new Rect(guidelinesX[Top], bounds.Top, num8, 5.0);
                drawingContext.DrawRectangle(brushes[Top], null, topBounds);
            }

            var topRightBounds = new Rect(
                guidelinesX[TopRight], bounds.Top, cornerRadius.TopRight, cornerRadius.TopRight);
            drawingContext.DrawRectangle(brushes[TopRight], null, topRightBounds);

            double num9 = guidelinesY[Left] - guidelinesY[Top];
            if (num9 > 0.0) {
                var leftBounds = new Rect(bounds.Left, guidelinesY[Top], 5.0, num9);
                drawingContext.DrawRectangle(brushes[Left], null, leftBounds);
            }

            double num10 = guidelinesY[Center] - guidelinesY[TopRight];
            if (num10 > 0.0) {
                var rightBounds = new Rect(guidelinesX[Right], guidelinesY[TopRight], 5.0, num10);
                drawingContext.DrawRectangle(brushes[Right], null, rightBounds);
            }

            var bottomLeft = new Rect(
                bounds.Left, guidelinesY[Left],
                cornerRadius.BottomLeft, cornerRadius.BottomLeft);
            drawingContext.DrawRectangle(brushes[BottomLeft], null, bottomLeft);

            double num11 = guidelinesX[Center] - guidelinesX[Left];
            if (num11 > 0.0) {
                var bottomBounds = new Rect(guidelinesX[Left], guidelinesY[Right], num11, 5.0);
                drawingContext.DrawRectangle(brushes[Bottom], null, bottomBounds);
            }

            var bottomRight = new Rect(
                guidelinesX[Center],
                guidelinesY[Center],
                cornerRadius.BottomRight,
                cornerRadius.BottomRight);
            drawingContext.DrawRectangle(brushes[BottomRight], null, bottomRight);

            if (cornerRadius.TopLeft == 5.0 &&
                cornerRadius.TopLeft == cornerRadius.TopRight &&
                cornerRadius.TopLeft == cornerRadius.BottomLeft &&
                cornerRadius.TopLeft == cornerRadius.BottomRight) {
                var center = new Rect(guidelinesX[0], guidelinesY[TopLeft], width, height);
                drawingContext.DrawRectangle(brushes[Center], null, center);
            } else {
                PathFigure figure = new PathFigure();
                if (cornerRadius.TopLeft > 5.0) {
                    figure.StartPoint = new Point(guidelinesX[Top], guidelinesY[TopLeft]);
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Top], guidelinesY[Top]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[TopLeft], guidelinesY[Top]), true));
                } else
                    figure.StartPoint = new Point(guidelinesX[TopLeft], guidelinesY[TopLeft]);

                if (cornerRadius.BottomLeft > 5.0) {
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[TopLeft], guidelinesY[Left]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Left], guidelinesY[Left]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Left], guidelinesY[Right]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[TopLeft], guidelinesY[Right]), true));

                if (cornerRadius.BottomRight > 5.0) {
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Center], guidelinesY[Right]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Center], guidelinesY[Center]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Right], guidelinesY[Center]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Right], guidelinesY[Right]), true));

                if (cornerRadius.TopRight > 5.0) {
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Right], guidelinesY[TopRight]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[TopRight], guidelinesY[TopRight]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[TopRight], guidelinesY[TopLeft]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelinesX[Right], guidelinesY[TopLeft]), true));

                figure.IsClosed = true;
                figure.Freeze();

                var geometry = new PathGeometry {
                    Figures = { figure }
                };
                geometry.Freeze();
                drawingContext.DrawGeometry(brushes[Center], null, geometry);
            }

            drawingContext.Pop();
        }
    }
}
