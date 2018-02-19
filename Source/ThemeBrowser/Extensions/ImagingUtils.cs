namespace ThemeBrowser.Extensions
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class ImagingUtils
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr ptr);

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

            var backgroundIsPremultiplied = background.Format == PixelFormats.Pbgra32;
            var pixels = new uint[width * height];
            background.CopyPixels(sourceRect, pixels, width * 4, 0);

            var overlayIsPremultiplied = overlay.Format == PixelFormats.Pbgra32;
            var overlayPixels = new uint[width * height];
            overlay.CopyPixels(sourceRect, overlayPixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; ++i) {
                var p = pixels[i];
                var o = overlayPixels[i];
                if (backgroundIsPremultiplied)
                    p = Unpremultiply(p);
                if (overlayIsPremultiplied)
                    o = Unpremultiply(o);
                p = ColorUtils.Blend(destColor, p);
                o = ColorUtils.Blend(destColor, o);
                pixels[i] = 0xFF000000 | (uint)Math.Abs(p - o) & 0xFFFFFFU;
            }

            return BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        }

        public static BitmapImage LoadBitmapImageFromStream(Stream source)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new DisposableStreamWrapper(source);
            image.EndInit();
            image.Freeze();
            return image;
        }

        public static BitmapImage LoadPremultipliedBitmapImageFromStream(Stream source)
        {
            return LoadBitmapImageFromStream(source).Unpremultiply();
        }

        public static BitmapImage Unpremultiply(this BitmapSource source)
        {
            var bitmap = new WriteableBitmap(source);
            bitmap.Unpremultiply();
            return AsPngBitmap(bitmap);
        }

        public static BitmapImage AsPngBitmap(BitmapSource source)
        {
            using (var stream = new MemoryStream()) {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);

                stream.Position = 0;
                return LoadBitmapImageFromStream(stream);
            }
        }

        public static void CopyUnpremultiplied(Stream input, Stream output)
        {
            var source = LoadBitmapImageFromStream(input);

            var image = new WriteableBitmap(source);
            image.Unpremultiply();

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(output);
        }

        public static unsafe void Unpremultiply(this WriteableBitmap bitmap)
        {
            bitmap.Lock();
            try {
                var ptr = (byte*)bitmap.BackBuffer.ToPointer();
                var stride = bitmap.BackBufferStride;
                var width = bitmap.PixelWidth;
                var height = bitmap.PixelHeight;

                for (int h = 0; h < height; ++h, ptr += stride) {
                    var pixel = (uint*)ptr;
                    for (int w = 0; w < width; ++w, ++pixel)
                        *pixel = Unpremultiply(*pixel);
                }
            } finally {
                bitmap.Unlock();
            }
        }

        public static Color Premultiply(this Color argb)
        {
            double a = argb.A / 255.0;
            double r = argb.R / 255.0;
            double g = argb.G / 255.0;
            double b = argb.B / 255.0;

            r *= a;
            g *= a;
            b *= a;

            var ba = (byte)Math.Round(a * 255);
            var br = (byte)Math.Round(r * 255);
            var bg = (byte)Math.Round(g * 255);
            var bb = (byte)Math.Round(b * 255);
            return Color.FromArgb(ba, br, bg, bb);
        }

        public static Color Unpremultiply(this Color argb)
        {
            double a = argb.A / 255.0;
            double r = argb.R / 255.0;
            double g = argb.G / 255.0;
            double b = argb.B / 255.0;

            r /= a;
            g /= a;
            b /= a;

            var ba = (byte)Math.Round(a * 255);
            var br = (byte)Math.Round(r * 255);
            var bg = (byte)Math.Round(g * 255);
            var bb = (byte)Math.Round(b * 255);
            return Color.FromArgb(ba, br, bg, bb);
        }

        public static uint Unpremultiply(uint argb)
        {
            double a = ((argb >> 24) & 0xFF) / 255.0;
            double r = ((argb >> 16) & 0xFF) / 255.0;
            double g = ((argb >> 8) & 0xFF) / 255.0;
            double b = ((argb >> 0) & 0xFF) / 255.0;

            r /= a;
            g /= a;
            b /= a;

            var ba = (byte)Math.Round(a * 255);
            var br = (byte)Math.Round(r * 255);
            var bg = (byte)Math.Round(g * 255);
            var bb = (byte)Math.Round(b * 255);
            return bb | (uint)(bg << 8) | (uint)(br << 16) | (uint)(ba << 24);
        }
    }
}
