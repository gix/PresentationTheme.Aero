namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using Aero;

    /// <summary>High Contrast Windows 10 Theme</summary>
    public static class HighContrastWin10Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(HighContrastWin10Theme).Assembly.GetName(),
                "Themes/HighContrast.Win10.NormalColor.xaml");
    }
}
