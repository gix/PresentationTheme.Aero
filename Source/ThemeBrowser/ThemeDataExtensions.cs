namespace ThemeBrowser
{
    using System;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class ThemeDataExtensions
    {
        public static ImageSource GetThemeBitmapAsImageSource(
            this IThemeData themeData, int partId, int stateId, int propertyId)
        {
            var hbmp = themeData.GetThemeBitmap(partId, stateId, propertyId);
            if (hbmp == null)
                return null;

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                hbmp.Value, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
    }
}
