namespace PresentationTheme.HighContrast.Win8
{
    using System;
    using Aero;

    /// <summary>Windows 8/8.1 High Contrast Theme</summary>
    public static class HighContrastWin8Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(HighContrastWin8Theme).Assembly.GetName(),
                "Themes/HighContrast.Win8.NormalColor.xaml");
    }
}
