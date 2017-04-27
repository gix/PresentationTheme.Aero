namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    public class TickBarEx : TickBar
    {
        #region public Brush SelectionTickBrush { get; set; }

        /// <summary>
        ///   Identifies the <see cref="SelectionTickBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionTickBrushProperty =
            DependencyProperty.Register(
                nameof(SelectionTickBrush),
                typeof(Brush),
                typeof(TickBarEx),
                new FrameworkPropertyMetadata(
                    Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        ///   Gets or sets the selection tick brush.
        /// </summary>
        public Brush SelectionTickBrush
        {
            get => (Brush)GetValue(SelectionTickBrushProperty);
            set => SetValue(SelectionTickBrushProperty, value);
        }

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            // Do not call base.OnRender, we replace it completely.

            Size size = new Size(ActualWidth, ActualHeight);
            double range = Maximum - Minimum;
            double tickLen = 0.0d;  // Height for Primary Tick (for Mininum and Maximum value)
            double tickLen2;        // Height for Secondary Tick
            double logicalToPhysical = 1.0;
            double progression = 1.0d;
            Point startPoint = new Point(0d, 0d);
            Point endPoint = new Point(0d, 0d);

            // Take Thumb size in to account
            double halfReservedSpace = ReservedSpace * 0.5;

            switch (Placement) {
                case TickBarPlacement.Top:
                    if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Width))
                        return;
                    size.Width -= ReservedSpace;
                    tickLen = -size.Height;
                    startPoint = new Point(halfReservedSpace, size.Height);
                    endPoint = new Point(halfReservedSpace + size.Width, size.Height);
                    logicalToPhysical = size.Width / range;
                    progression = 1;
                    break;

                case TickBarPlacement.Bottom:
                    if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Width))
                        return;
                    size.Width -= ReservedSpace;
                    tickLen = size.Height;
                    startPoint = new Point(halfReservedSpace, 0d);
                    endPoint = new Point(halfReservedSpace + size.Width, 0d);
                    logicalToPhysical = size.Width / range;
                    progression = 1;
                    break;

                case TickBarPlacement.Left:
                    if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Height))
                        return;
                    size.Height -= ReservedSpace;
                    tickLen = -size.Width;
                    startPoint = new Point(size.Width, size.Height + halfReservedSpace);
                    endPoint = new Point(size.Width, halfReservedSpace);
                    logicalToPhysical = size.Height / range * -1;
                    progression = -1;
                    break;

                case TickBarPlacement.Right:
                    if (DoubleUtil.GreaterThanOrClose(ReservedSpace, size.Height))
                        return;
                    size.Height -= ReservedSpace;
                    tickLen = size.Width;
                    startPoint = new Point(0d, size.Height + halfReservedSpace);
                    endPoint = new Point(0d, halfReservedSpace);
                    logicalToPhysical = size.Height / range * -1;
                    progression = -1;
                    break;
            };

            tickLen2 = tickLen * 0.75;

            // Invert direciton of the ticks
            if (IsDirectionReversed) {
                progression = -progression;
                logicalToPhysical *= -1;

                // swap startPoint & endPoint
                Point pt = startPoint;
                startPoint = endPoint;
                endPoint = pt;
            }

            Pen pen = new Pen(Fill, 1.0d);

            bool snapsToDevicePixels = SnapsToDevicePixels;
            DoubleCollection xLines = snapsToDevicePixels ? new DoubleCollection() : null;
            DoubleCollection yLines = snapsToDevicePixels ? new DoubleCollection() : null;

            // Is it Vertical?
            if (Placement == TickBarPlacement.Left || Placement == TickBarPlacement.Right) {
                // Reduce tick interval if it is more than would be visible on the screen
                double interval = TickFrequency;
                if (interval > 0.0) {
                    double minInterval = (Maximum - Minimum) / size.Height;
                    if (interval < minInterval)
                        interval = minInterval;
                }

                // Draw Min & Max tick
                dc.DrawLine(pen, startPoint, new Point(startPoint.X + tickLen, startPoint.Y));
                dc.DrawLine(pen, new Point(startPoint.X, endPoint.Y),
                                 new Point(startPoint.X + tickLen, endPoint.Y));

                if (snapsToDevicePixels) {
                    xLines.Add(startPoint.X);
                    yLines.Add(startPoint.Y - 0.5);
                    xLines.Add(startPoint.X + tickLen);
                    yLines.Add(endPoint.Y - 0.5);
                    xLines.Add(startPoint.X + tickLen2);
                }

                // This property is rarely set so let's try to avoid the GetValue
                // caching of the mutable default value
                DoubleCollection ticks = null;

                if (DependencyPropertyHelper.GetValueSource(this, TicksProperty).BaseValueSource
                    != BaseValueSource.Default) {
                    ticks = Ticks;
                }

                // Draw ticks using specified Ticks collection
                if (ticks != null && ticks.Count > 0) {
                    foreach (double tick in ticks) {
                        if (DoubleUtil.LessThanOrClose(tick, Minimum) ||
                            DoubleUtil.GreaterThanOrClose(tick, Maximum))
                            continue;

                        double adjustedTick = tick - Minimum;

                        double y = adjustedTick * logicalToPhysical + startPoint.Y;
                        dc.DrawLine(pen,
                            new Point(startPoint.X, y),
                            new Point(startPoint.X + tickLen2, y));

                        if (snapsToDevicePixels)
                            yLines.Add(y - 0.5);
                    }
                }
                // Draw ticks using specified TickFrequency
                else if (interval > 0.0) {
                    for (double i = interval; i < range; i += interval) {
                        double y = i * logicalToPhysical + startPoint.Y;

                        dc.DrawLine(pen,
                            new Point(startPoint.X, y),
                            new Point(startPoint.X + tickLen2, y));

                        if (snapsToDevicePixels) {
                            yLines.Add(y - 0.5);
                        }
                    }
                }

                // Draw Selection Ticks
                if (IsSelectionRangeEnabled) {
                    double y0 = (SelectionStart - Minimum) * logicalToPhysical + startPoint.Y;
                    Point pt0 = new Point(startPoint.X, y0);
                    Point pt1 = new Point(startPoint.X + tickLen2, y0);
                    Point pt2 = new Point(startPoint.X + tickLen2, y0 - Math.Abs(tickLen2) * progression);

                    PathSegment[] segments = {
                        new LineSegment(pt2, true),
                        new LineSegment(pt0, true),
                    };
                    PathGeometry geo = new PathGeometry(new[] { new PathFigure(pt1, segments, true) });

                    dc.DrawGeometry(Fill, pen, geo);

                    y0 = (SelectionEnd - Minimum) * logicalToPhysical + startPoint.Y;
                    pt0 = new Point(startPoint.X, y0);
                    pt1 = new Point(startPoint.X + tickLen2, y0);
                    pt2 = new Point(startPoint.X + tickLen2, y0 + Math.Abs(tickLen2) * progression);

                    segments = new PathSegment[] {
                        new LineSegment(pt2, true),
                        new LineSegment(pt0, true),
                    };
                    geo = new PathGeometry(new[] { new PathFigure(pt1, segments, true) });
                    dc.DrawGeometry(Fill, pen, geo);
                }
            } else {
                // Placement == Top || Placement == Bottom

                // Reduce tick interval if it is more than would be visible on the screen
                double interval = TickFrequency;
                if (interval > 0.0) {
                    double minInterval = (Maximum - Minimum) / size.Width;
                    if (interval < minInterval)
                        interval = minInterval;
                }

                // Draw Min & Max tick
                dc.DrawLine(pen, startPoint, new Point(startPoint.X, startPoint.Y + tickLen));
                dc.DrawLine(pen, new Point(endPoint.X, startPoint.Y),
                                 new Point(endPoint.X, startPoint.Y + tickLen));

                if (snapsToDevicePixels) {
                    xLines.Add(startPoint.X - 0.5);
                    yLines.Add(startPoint.Y);
                    xLines.Add(startPoint.X - 0.5);
                    yLines.Add(endPoint.Y + tickLen);
                    yLines.Add(endPoint.Y + tickLen2);
                }

                // This property is rarely set so let's try to avoid the GetValue
                // caching of the mutable default value
                DoubleCollection ticks = null;
                if (DependencyPropertyHelper.GetValueSource(this, TicksProperty).BaseValueSource
                    != BaseValueSource.Default) {
                    ticks = Ticks;
                }

                // Draw ticks using specified Ticks collection
                if (ticks != null && ticks.Count > 0) {
                    foreach (double tick in ticks) {
                        if (DoubleUtil.LessThanOrClose(tick, Minimum) ||
                            DoubleUtil.GreaterThanOrClose(tick, Maximum))
                            continue;
                        double adjustedTick = tick - Minimum;

                        double x = adjustedTick * logicalToPhysical + startPoint.X;
                        dc.DrawLine(pen,
                            new Point(x, startPoint.Y),
                            new Point(x, startPoint.Y + tickLen2));

                        if (snapsToDevicePixels)
                            xLines.Add(x - 0.5);
                    }
                }
                // Draw ticks using specified TickFrequency
                else if (interval > 0.0) {
                    for (double i = interval; i < range; i += interval) {
                        double x = i * logicalToPhysical + startPoint.X;
                        dc.DrawLine(pen,
                            new Point(x, startPoint.Y),
                            new Point(x, startPoint.Y + tickLen2));

                        if (snapsToDevicePixels)
                            xLines.Add(x - 0.5);
                    }
                }

                // Draw Selection Ticks
                if (IsSelectionRangeEnabled) {
                    var tickFill = Brushes.Black;

                    double x0 = (SelectionStart - Minimum) * logicalToPhysical + startPoint.X;
                    Point pt0 = new Point(x0, startPoint.Y);
                    Point pt1 = new Point(x0, startPoint.Y + tickLen2);
                    Point pt2 = new Point(x0 - Math.Abs(tickLen2) * progression, startPoint.Y + tickLen2);

                    PathSegment[] segments = {
                        new LineSegment(pt2, true),
                        new LineSegment(pt0, true),
                    };
                    var geo = new PathGeometry(new[] { new PathFigure(pt1, segments, true) });

                    dc.DrawGeometry(tickFill, null, geo);

                    x0 = (SelectionEnd - Minimum) * logicalToPhysical + startPoint.X;
                    pt0 = new Point(x0, startPoint.Y);
                    pt1 = new Point(x0, startPoint.Y + tickLen2);
                    pt2 = new Point(x0 + Math.Abs(tickLen2) * progression, startPoint.Y + tickLen2);

                    segments = new PathSegment[] {
                        new LineSegment(pt2, true),
                        new LineSegment(pt0, true),
                    };
                    geo = new PathGeometry(new[] { new PathFigure(pt1, segments, true) });
                    dc.DrawGeometry(tickFill, null, geo);
                }
            }

            if (snapsToDevicePixels) {
                xLines.Add(ActualWidth);
                yLines.Add(ActualHeight);
                VisualXSnappingGuidelines = xLines;
                VisualYSnappingGuidelines = yLines;
            }
        }
    }
}
