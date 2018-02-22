namespace PresentationTheme.Aero.Win8
{
    using System;

    /// <summary>Windows 8 Aero Theme</summary>
    public static class AeroWin8Theme
    {
        /// <summary>
        ///   Gets the Pack <see cref="Uri"/> for the theme resources.
        /// </summary>
        public static Uri ResourceUri =>
            PackUriUtils.MakeContentPackUri(
                typeof(AeroWin8Theme).Assembly.GetName(),
                "Themes/Aero.Win8.NormalColor.xaml");
    }
}
