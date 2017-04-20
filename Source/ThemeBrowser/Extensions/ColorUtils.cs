namespace ThemeBrowser.Extensions
{
    using System.Diagnostics;
    using System.Windows.Media;

    public static class ColorUtils
    {
        public static string ToHex(this Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static uint Blend(uint dst, uint src)
        {
            uint db = (dst >>  0) & 0xFF;
            uint dg = (dst >>  8) & 0xFF;
            uint dr = (dst >> 16) & 0xFF;
            uint da = (dst >> 24) & 0xFF;
            Debug.Assert(da == 0xFF);

            uint sb = (src >> 0) & 0xFF;
            uint sg = (src >> 8) & 0xFF;
            uint sr = (src >> 16) & 0xFF;
            uint sa = (src >> 24) & 0xFF;

            double a = sa / 255.0;
            double b = sb + db * (1.0 - a);
            double g = sg + dg * (1.0 - a);
            double r = sr + dr * (1.0 - a);

            return ((uint)b << 0) | ((uint)g << 8) | ((uint)r << 16) | (0xFFu << 24);
        }

        public static uint ToArgb(this Color color)
        {
            return
                (uint)color.A << 24 |
                (uint)color.R << 16 |
                (uint)color.G << 8 |
                (uint)color.B << 0;
        }
    }
}
