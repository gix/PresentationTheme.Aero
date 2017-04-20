namespace ThemeBrowser.Extensions
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    public static class GeometryUtils
    {
        public static Point Round(this Point point)
        {
            return new Point(Math.Round(point.X), Math.Round(point.Y));
        }

        public static Point Round(this Point point, int digits)
        {
            return new Point(Math.Round(point.X, digits), Math.Round(point.Y, digits));
        }

        public static PathSegment Round(this PathSegment segment, int digits)
        {
            var lineSegment = segment as LineSegment;
            if (lineSegment != null)
                return new LineSegment(lineSegment.Point.Round(digits), lineSegment.IsStroked);

            var polyLineSegment = segment as PolyLineSegment;
            if (polyLineSegment != null)
                return new PolyLineSegment(
                    polyLineSegment.Points.Select(x => Round(x, digits)),
                    polyLineSegment.IsStroked);

            var arcSegment = segment as ArcSegment;
            if (arcSegment != null)
                return new ArcSegment(
                    arcSegment.Point.Round(digits),
                    arcSegment.Size,
                    arcSegment.RotationAngle,
                    arcSegment.IsLargeArc,
                    arcSegment.SweepDirection,
                    arcSegment.IsStroked);

            return segment.Clone();
        }

        public static PathFigure Round(this PathFigure figure, int digits)
        {
            var segments = figure.Segments.Select(x => x.Round(digits));
            return new PathFigure(figure.StartPoint.Round(2), segments, figure.IsClosed) {
                IsFilled = figure.IsFilled,
            };
        }

        public static PathGeometry Round(this PathGeometry geometry, int digits)
        {
            var figures = geometry.Figures.Select(x => x.Round(digits));
            return new PathGeometry(figures, geometry.FillRule, geometry.Transform);
        }
    }
}
