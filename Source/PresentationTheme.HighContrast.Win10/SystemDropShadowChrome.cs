namespace PresentationTheme.HighContrast.Win10
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
        private const double ShadowDepth = 5;

        private static readonly object ResourceAccess = new object();
        private static Brush[] commonBrushes;
        private static CornerRadius commonCornerRadius;

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
            double gradientScale = 1.0 / (cornerRadius + ShadowDepth);
            var stops = new GradientStopCollection {
                new GradientStop(c, (0.5 + cornerRadius) * gradientScale)
            };

            Color color = c;
            color.A = (byte)(0.74336 * c.A);
            stops.Add(new GradientStop(color, (1.5 + cornerRadius) * gradientScale));

            color.A = (byte)(0.38053 * c.A);
            stops.Add(new GradientStop(color, (2.5 + cornerRadius) * gradientScale));

            color.A = (byte)(0.12389 * c.A);
            stops.Add(new GradientStop(color, (3.5 + cornerRadius) * gradientScale));

            color.A = (byte)(0.02654 * c.A);
            stops.Add(new GradientStop(color, (4.5 + cornerRadius) * gradientScale));

            color.A = 0;
            stops.Add(new GradientStop(color, (5.0 + cornerRadius) * gradientScale));

            stops.Freeze();
            return stops;
        }

        private Brush[] GetBrushes(Color c, CornerRadius cornerRadius)
        {
            if (commonBrushes == null) {
                lock (ResourceAccess) {
                    if (commonBrushes == null) {
                        commonBrushes = CreateBrushes(c, cornerRadius);
                        commonCornerRadius = cornerRadius;
                    }
                }
            }

            if (c == ((SolidColorBrush)commonBrushes[Center]).Color &&
                cornerRadius == commonCornerRadius) {
                brushes = null;
                return commonBrushes;
            }

            return brushes ?? (brushes = CreateBrushes(c, cornerRadius));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var shadowBounds = new Rect(
                new Point(ShadowDepth, ShadowDepth),
                new Size(RenderSize.Width, RenderSize.Height));

            Color color = Color;
            if (shadowBounds.Width <= 0 || shadowBounds.Height <= 0 || color.A <= 0)
                return;

            double centerWidth = (shadowBounds.Right - shadowBounds.Left) - 2 * ShadowDepth;
            double centerHeight = (shadowBounds.Bottom - shadowBounds.Top) - 2 * ShadowDepth;
            double maxRadius = Math.Min(centerWidth * 0.5, centerHeight * 0.5);

            CornerRadius cornerRadius = CornerRadius;
            cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
            cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, maxRadius);
            cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);
            cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);

            Brush[] brushes = GetBrushes(color, cornerRadius);
            double centerTop = shadowBounds.Top + ShadowDepth;
            double centerLeft = shadowBounds.Left + ShadowDepth;
            double centerRight = shadowBounds.Right - ShadowDepth;
            double centerBottom = shadowBounds.Bottom - ShadowDepth;

            double[] guidelineSetX = {
                centerLeft,
                centerLeft + cornerRadius.TopLeft,
                centerRight - cornerRadius.TopRight,
                centerLeft + cornerRadius.BottomLeft,
                centerRight - cornerRadius.BottomRight,
                centerRight
            };
            double[] guidelineSetY = {
                centerTop,
                centerTop + cornerRadius.TopLeft,
                centerTop + cornerRadius.TopRight,
                centerBottom - cornerRadius.BottomLeft,
                centerBottom - cornerRadius.BottomRight,
                centerBottom
            };

            drawingContext.PushGuidelineSet(new GuidelineSet(guidelineSetX, guidelineSetY));
            cornerRadius.TopLeft += ShadowDepth;
            cornerRadius.TopRight += ShadowDepth;
            cornerRadius.BottomLeft += ShadowDepth;
            cornerRadius.BottomRight += ShadowDepth;

            var topLeft = new Rect(
                shadowBounds.Left, shadowBounds.Top, cornerRadius.TopLeft, cornerRadius.TopLeft);
            drawingContext.DrawRectangle(brushes[TopLeft], null, topLeft);

            double topWidth = guidelineSetX[TopRight] - guidelineSetX[Top];
            if (topWidth > 0.0) {
                var top = new Rect(guidelineSetX[Top], shadowBounds.Top, topWidth, ShadowDepth);
                drawingContext.DrawRectangle(brushes[Top], null, top);
            }

            var topRight = new Rect(
                guidelineSetX[TopRight], shadowBounds.Top, cornerRadius.TopRight, cornerRadius.TopRight);
            drawingContext.DrawRectangle(brushes[TopRight], null, topRight);

            double leftHeight = guidelineSetY[Left] - guidelineSetY[Top];
            if (leftHeight > 0.0) {
                var leftBounds = new Rect(shadowBounds.Left, guidelineSetY[Top], ShadowDepth, leftHeight);
                drawingContext.DrawRectangle(brushes[Left], null, leftBounds);
            }

            double rightHeight = guidelineSetY[Center] - guidelineSetY[TopRight];
            if (rightHeight > 0.0) {
                var right = new Rect(guidelineSetX[Right], guidelineSetY[TopRight], ShadowDepth, rightHeight);
                drawingContext.DrawRectangle(brushes[Right], null, right);
            }

            var bottomLeft = new Rect(
                shadowBounds.Left, guidelineSetY[Left],
                cornerRadius.BottomLeft, cornerRadius.BottomLeft);
            drawingContext.DrawRectangle(brushes[BottomLeft], null, bottomLeft);

            double bottomWidth = guidelineSetX[Center] - guidelineSetX[Left];
            if (bottomWidth > 0.0) {
                var bottom = new Rect(guidelineSetX[Left], guidelineSetY[Right], bottomWidth, 5.0);
                drawingContext.DrawRectangle(brushes[Bottom], null, bottom);
            }

            var bottomRight = new Rect(
                guidelineSetX[Center],
                guidelineSetY[Center],
                cornerRadius.BottomRight,
                cornerRadius.BottomRight);
            drawingContext.DrawRectangle(brushes[BottomRight], null, bottomRight);

            if (cornerRadius.TopLeft == ShadowDepth &&
                cornerRadius.TopLeft == cornerRadius.TopRight &&
                cornerRadius.TopLeft == cornerRadius.BottomLeft &&
                cornerRadius.TopLeft == cornerRadius.BottomRight) {
                var center = new Rect(guidelineSetX[0], guidelineSetY[TopLeft], centerWidth, centerHeight);
                drawingContext.DrawRectangle(brushes[Center], null, center);
            } else {
                var figure = new PathFigure();
                if (cornerRadius.TopLeft > ShadowDepth) {
                    figure.StartPoint = new Point(guidelineSetX[Top], guidelineSetY[TopLeft]);
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Top], guidelineSetY[Top]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[TopLeft], guidelineSetY[Top]), true));
                } else
                    figure.StartPoint = new Point(guidelineSetX[TopLeft], guidelineSetY[TopLeft]);

                if (cornerRadius.BottomLeft > ShadowDepth) {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[TopLeft], guidelineSetY[Left]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Left], guidelineSetY[Left]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Left], guidelineSetY[Right]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[TopLeft], guidelineSetY[Right]), true));

                if (cornerRadius.BottomRight > ShadowDepth) {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Center], guidelineSetY[Right]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Center], guidelineSetY[Center]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Right], guidelineSetY[Center]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Right], guidelineSetY[Right]), true));

                if (cornerRadius.TopRight > ShadowDepth) {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Right], guidelineSetY[TopRight]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[TopRight], guidelineSetY[TopRight]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[TopRight], guidelineSetY[TopLeft]), true));
                } else
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[Right], guidelineSetY[TopLeft]), true));

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
