namespace ThemeBrowser
{
    using System.Windows.Media;

    public static class ThemeExtensions
    {
        public static Color ColorFromCOLORREF(int value)
        {
            var b = (byte)((value >> 16) & 0xFF);
            var g = (byte)((value >> 8) & 0xFF);
            var r = (byte)((value >> 0) & 0xFF);
            return Color.FromArgb(0xFF, r, g, b);
        }

        public static Color ColorFromArgb(int value)
        {
            var a = (byte)((value >> 24) & 0xFF);
            var r = (byte)((value >> 16) & 0xFF);
            var g = (byte)((value >> 8) & 0xFF);
            var b = (byte)((value >> 0) & 0xFF);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color ToWpfColor(this ThemeCore.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
