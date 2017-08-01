namespace PresentationTheme.AeroLite.Win10
{
    using System;
    using Aero;

    /// <summary>AeroLite Windows 10 Theme</summary>
    public static class AeroLiteWin10Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroLiteWin10Theme).Assembly.GetName(),
                "Themes/AeroLite.Win10.NormalColor.xaml");
    }
}
