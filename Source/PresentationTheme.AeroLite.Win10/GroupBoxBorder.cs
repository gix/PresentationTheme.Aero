namespace PresentationTheme.AeroLite.Win10
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    ///   Creates a theme-specific look for <see cref="GroupBox"/> elements.
    /// </summary>
    /// <remarks>
    ///   The actual appearance of a <see cref="GroupBox"/> is dependent on which
    ///   theme is active on the user's system. The properties of this class
    ///   allow WPF to set the appearance based on the current theme.
    /// </remarks>
    public class GroupBoxBorder : Border
    {
        private Pen darkPenCache;
        private Pen lightPenCache;
        private Pen cornerPenCache;

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext dc)
        {
            if (BorderThickness != new Thickness(1, 1, 1, 1)) {
                base.OnRender(dc);
                return;
            }

            if (darkPenCache == null) {
                darkPenCache = new Pen(BorderBrush, 1);
                if (BorderBrush.IsFrozen)
                    darkPenCache.Freeze();
            }

            if (lightPenCache == null) {
                lightPenCache = new Pen(Brushes.White, 1);
                lightPenCache.Freeze();
            }

            if (cornerPenCache == null) {
                cornerPenCache = new Pen(new SolidColorBrush(Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFC)), 1);
                cornerPenCache.Freeze();
            }

            var border = BorderThickness;
            var renderSize = RenderSize;
            const double halfThickness = 0.5;

            // Left
            dc.DrawLine(
                darkPenCache,
                new Point(halfThickness, 2),
                new Point(halfThickness, renderSize.Height - 2));
            dc.DrawLine(
                lightPenCache,
                new Point(halfThickness + 1, 2),
                new Point(halfThickness + 1, renderSize.Height - 2));

            // Right
            dc.DrawLine(
                darkPenCache,
                new Point(renderSize.Width - halfThickness, 2),
                new Point(renderSize.Width - halfThickness, renderSize.Height - 3));
            dc.DrawLine(
                lightPenCache,
                new Point(renderSize.Width - halfThickness - 1, 2),
                new Point(renderSize.Width - halfThickness - 1, renderSize.Height - 3));

            // Top
            dc.DrawLine(
                darkPenCache,
                new Point(2, halfThickness),
                new Point(renderSize.Width - 2, halfThickness));
            dc.DrawLine(
                lightPenCache,
                new Point(2, halfThickness + 1),
                new Point(renderSize.Width - 2, halfThickness + 1));

            // Bottom
            dc.DrawLine(
                lightPenCache,
                new Point(2, renderSize.Height - halfThickness),
                new Point(renderSize.Width - 2, renderSize.Height - halfThickness));
            dc.DrawLine(
                darkPenCache,
                new Point(2, renderSize.Height - halfThickness - 1),
                new Point(renderSize.Width - 2, renderSize.Height - halfThickness - 1));

            // Top-left
            dc.DrawLine(
                darkPenCache,
                new Point(1, halfThickness + 1),
                new Point(2, halfThickness + 1));

            // Top-right
            dc.DrawLine(
                darkPenCache,
                new Point(renderSize.Width - 2, halfThickness + 1),
                new Point(renderSize.Width - 1, halfThickness + 1));

            // Bottom-left
            dc.DrawLine(
                darkPenCache,
                new Point(1, renderSize.Height - halfThickness - 2),
                new Point(2, renderSize.Height - halfThickness - 2));
            dc.DrawLine(
                cornerPenCache,
                new Point(1, renderSize.Height - halfThickness - 1),
                new Point(2, renderSize.Height - halfThickness - 1));
            dc.DrawLine(
                cornerPenCache,
                new Point(0, renderSize.Height - halfThickness - 2),
                new Point(1, renderSize.Height - halfThickness - 2));

            // Bottom-right
            dc.DrawLine(
                darkPenCache,
                new Point(renderSize.Width - 2, renderSize.Height - halfThickness - 2),
                new Point(renderSize.Width - 1, renderSize.Height - halfThickness - 2));
            dc.DrawLine(
                cornerPenCache,
                new Point(renderSize.Width - 2, renderSize.Height - halfThickness - 1),
                new Point(renderSize.Width - 1, renderSize.Height - halfThickness - 1));
            dc.DrawLine(
                cornerPenCache,
                new Point(renderSize.Width - 1, renderSize.Height - halfThickness - 2),
                new Point(renderSize.Width, renderSize.Height - halfThickness - 2));

            // Draw background in rectangle inside border.
            Brush background = Background;
            if (background != null) {
                Point ptTL = new Point(border.Left, border.Top);
                Point ptBR = new Point(renderSize.Width - border.Right, renderSize.Height - border.Bottom);

                // Do not draw background if the borders are so large that they overlap.
                if (ptBR.X > ptTL.X && ptBR.Y > ptTL.Y)
                    dc.DrawRectangle(background, null, new Rect(ptTL, ptBR));
            }
        }
    }
}
