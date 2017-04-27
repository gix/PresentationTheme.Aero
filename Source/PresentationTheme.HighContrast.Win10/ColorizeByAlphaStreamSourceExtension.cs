namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.IO;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    ///   Colorizes the bitmap specified by <see cref="UriSource"/> by combining
    ///   its alpha channel with the specified <see cref="Tint"/> color and returns
    ///   the resulting bitmap as a <see cref="Stream"/>. Any RGB values in the
    ///   original image are ignored.
    /// </summary>
    [MarkupExtensionReturnType(typeof(Stream))]
    public class ColorizeByAlphaStreamSourceExtension : MarkupExtension
    {
        /// <summary>
        ///   Initializes a new instance of the
        ///   <see cref="ColorizeByAlphaStreamSourceExtension"/> class.
        /// </summary>
        public ColorizeByAlphaStreamSourceExtension()
        {
        }

        /// <summary>
        ///   Initializes a new instance of the
        ///   <see cref="ColorizeByAlphaStreamSourceExtension"/> class with the
        ///   specified <see cref="UriSource"/> and <see cref="Tint"/>.
        /// </summary>
        public ColorizeByAlphaStreamSourceExtension(Uri uriSource, Color tint)
        {
            UriSource = uriSource;
            Tint = tint;
        }

        /// <summary>Gets or sets the <see cref="Uri"/> source.</summary>
        public Uri UriSource { get; set; }

        /// <summary>Gets or sets the tint <see cref="Color"/>.</summary>
        public Color Tint { get; set; }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (UriSource == null)
                throw new InvalidOperationException(
                    nameof(UriSource) + " property must be specified on " +
                    nameof(ColorizeByAlphaStreamSourceExtension));

            BitmapSource sourceBitmap = new BitmapImage(UriSource);
            sourceBitmap = ColorizeByAlpha(sourceBitmap, Tint);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(sourceBitmap));
            var stream = new MemoryStream();
            encoder.Save(stream);
            stream.Position = 0;
            return stream;
        }

        private static BitmapSource ColorizeByAlpha(BitmapSource bitmap, Color tint)
        {
            var src = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            int width = src.PixelWidth;
            int height = src.PixelHeight;
            int stride = ((width * src.Format.BitsPerPixel) + 7) / 8;

            var pixels = new byte[height * stride];

            src.CopyPixels(pixels, stride, 0);
            for (int i = 0; i < pixels.Length; i += 4) {
                var srcColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                var dstColor = Colorize(srcColor, tint);

                if (dstColor != srcColor) {
                    pixels[i] = dstColor.B;
                    pixels[i + 1] = dstColor.G;
                    pixels[i + 2] = dstColor.R;
                    pixels[i + 3] = dstColor.A;
                }
            }

            var colorized = BitmapSource.Create(
                width, height, src.DpiX, src.DpiY, src.Format, null, pixels, stride);
            if (colorized.CanFreeze)
                colorized.Freeze();
            return colorized;
        }

        private static Color Colorize(Color source, Color tint)
        {
            byte alpha = source.A;
            if (alpha == 0x00)
                return Colors.Transparent;
            return Color.FromArgb(alpha, tint.R, tint.G, tint.B);
        }
    }
}
