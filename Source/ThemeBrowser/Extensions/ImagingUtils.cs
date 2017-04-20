namespace ThemeBrowser.Extensions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class ImagingUtils
    {
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr ptr);

        public static BitmapSource ConvertBitmap(System.Drawing.Bitmap source)
        {
            IntPtr hbmp = source.GetHbitmap();
            try {
                return ConvertBitmap(hbmp);
            } finally {
                DeleteObject(hbmp);
            }
        }

        public static BitmapSource ConvertBitmap(IntPtr hbmp)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hbmp, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource TakeSnapshot(System.Windows.Forms.Control control)
        {
            var bounds = control.Bounds;
            var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);

            var screenPoint = control.PointToScreen(new System.Drawing.Point());

            System.Drawing.Graphics.FromImage(bitmap).CopyFromScreen(
                screenPoint,
                new System.Drawing.Point(),
                new System.Drawing.Size(bounds.Width, bounds.Height));

            return ConvertBitmap(bitmap);
        }

        public static BitmapSource RenderSnapshot(System.Windows.Forms.Control control)
        {
            var bounds = control.Bounds;
            var bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
            control.DrawToBitmap(bitmap, bounds);
            return ConvertBitmap(bitmap);
        }

        public static BitmapSource TakeSnapshot(FrameworkElement element)
        {
            var width = (int)element.ActualWidth;
            var height = (int)element.ActualHeight;
            var bitmap = new System.Drawing.Bitmap(width, height);

            var screenPoint = element.PointToScreen(new Point());

            System.Drawing.Graphics.FromImage(bitmap).CopyFromScreen(
                new System.Drawing.Point((int)screenPoint.X, (int)screenPoint.Y),
                new System.Drawing.Point(),
                new System.Drawing.Size(width, height));

            return ConvertBitmap(bitmap);
        }

        public static BitmapSource RenderSnapshot(FrameworkElement element)
        {
            var width = (int)element.ActualWidth;
            var height = (int)element.ActualHeight;
            if (width == 0 || height == 0)
                return null;

            var bitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen()) {
                var brush = new VisualBrush(element) {
                    TileMode = TileMode.None,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
                dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }

            bitmap.Render(visual);

            return bitmap;
        }

        public static BitmapSource Difference(
            BitmapSource background, BitmapSource overlay, uint destColor)
        {
            int width = Math.Min(background.PixelWidth, overlay.PixelWidth);
            int height = Math.Min(background.PixelHeight, overlay.PixelHeight);
            var sourceRect = new Int32Rect(0, 0, width, height);

            var pixels = new uint[width * height];
            background.CopyPixels(sourceRect, pixels, width * 4, 0);

            var overlayPixels = new uint[width * height];
            overlay.CopyPixels(sourceRect, overlayPixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; ++i) {
                var p = ColorUtils.Blend(destColor, pixels[i]);
                var o = ColorUtils.Blend(destColor, overlayPixels[i]);
                pixels[i] = 0xFF000000 | (uint)Math.Abs(p - o) & 0xFFFFFFU;
            }

            return BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        }
    }
}
