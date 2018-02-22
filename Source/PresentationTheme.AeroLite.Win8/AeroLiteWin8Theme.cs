namespace PresentationTheme.AeroLite.Win8
{
    using System;
    using Aero;

    /// <summary>Windows 8 AeroLite Theme</summary>
    public static class AeroLiteWin8Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroLiteWin8Theme).Assembly.GetName(),
                "Themes/AeroLite.Win8.NormalColor.xaml");
    }
}
